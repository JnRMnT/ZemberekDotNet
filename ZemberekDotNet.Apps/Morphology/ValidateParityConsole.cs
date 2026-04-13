using Commander.NET.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using ZemberekDotNet.Apps.Morphology.Parity;
using ZemberekDotNet.Morphology;

namespace ZemberekDotNet.Apps.Morphology
{
    /// <summary>
    /// CLI command that compares ZemberekDotNet morphological analysis against Java Zemberek
    /// on the same input corpus and produces a structured diff report.
    ///
    /// Usage (pre-generated Java output):
    ///   ValidateParityConsole --input sentences.txt --java-output java_analysis.tsv --output report.json
    ///
    /// Usage (auto Java invocation):
    ///   ValidateParityConsole --input sentences.txt --java-jar zemberek-full.jar --output report.json
    ///
    /// Java output TSV format (UTF-8, one word per line):
    ///   sentence_index &lt;TAB&gt; word &lt;TAB&gt; best_analysis &lt;TAB&gt; analysis_count
    ///   0   Kitaplara   kitap:Noun+A3pl+Dat   1
    ///   0   gidiyorum   git:Verb+Prog1+A1sg   1
    ///   1   xyz         ?                     0
    ///
    /// To produce the Java output file with the default Zemberek-NLP JAR:
    ///   java -jar zemberek-full.jar MorphAnalyze --input sentences.txt --output java_out.tsv
    /// (Adjust command name for your JAR version; use --java-args to customise.)
    /// </summary>
    public class ValidateParityConsole : ConsoleApp<ValidateParityConsole>
    {
        [Parameter("--input", "-i",
            Description = "Path to input text file with one sentence per line. Required.")]
        string inputPath;

        [Parameter("--java-output", "-j", Required = Required.No,
            Description = "Pre-generated Java Zemberek analysis TSV file. See class header for format.")]
        string javaOutputPath;

        [Parameter("--java-jar", Required = Required.No,
            Description = "Path to the Zemberek fat JAR. When provided Java is invoked automatically ('java' must be in PATH).")]
        string javaJarPath;

        [Parameter("--java-args",
            Description = "Custom Java invocation args template. Use {input} and {output} as placeholders. " +
                          "Default: 'zemberek.apps.ApplicationRunner MorphAnalyze --input {input} --output {output}'")]
        string javaArgs = "";

        [Parameter("--output", "-o", Required = Required.No,
            Description = "Output JSON report file path. If omitted the summary is printed to stdout only.")]
        string outputPath;

        [Parameter("--max", "-n",
            Description = "Maximum number of sentences to process. Default: 1000.")]
        int maxSentences = 1000;

        [Parameter("--no-detail",
            Description = "Suppress per-sentence word detail from the JSON report (smaller file).")]
        bool noDetail;

        public override string Description() =>
            "Compares ZemberekDotNet morphological analysis against Java Zemberek 0.17.x on the same corpus. " +
            "Produces a JSON diff report categorising mismatches as: " +
            "TokenizationDiff, LexiconGap, BestAnalysisDiff, AnalysisCountDiff, BothUnknown. " +
            "Requires --input and either --java-output (pre-generated) or --java-jar (auto-run).";

        public override void Run()
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.Error.WriteLine("Error: --input is required.");
                return;
            }

            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: input file not found: {inputPath}");
                return;
            }

            if (string.IsNullOrWhiteSpace(javaOutputPath) && string.IsNullOrWhiteSpace(javaJarPath))
            {
                Console.Error.WriteLine("Error: either --java-output or --java-jar must be provided.");
                return;
            }

            ResourceBootstrap.EnsureGlobalResourcesRoot();

            // Load sentences
            string[] allLines = File.ReadAllLines(inputPath)
                .Select(s => s.TrimStart('\uFEFF'))
                .ToArray();
            string[] sentences = allLines.Length > maxSentences
                ? allLines[..maxSentences]
                : allLines;

            Console.WriteLine($"Processing {sentences.Length} sentence(s) from: {inputPath}");

            // Resolve Java output path
            string javaFile = javaOutputPath;
            bool tempJavaFile = false;

            if (string.IsNullOrWhiteSpace(javaFile))
            {
                javaFile = Path.Combine(Path.GetTempPath(),
                    $"zemberek_parity_java_{Guid.NewGuid():N}.tsv");
                tempJavaFile = true;
                Console.WriteLine($"Running Java analysis (output: {javaFile})...");
                JavaProcessRunner.Run(javaJarPath, javaArgs, inputPath, javaFile);
            }

            try
            {
                // Parse Java output
                Console.WriteLine("Parsing Java output...");
                var javaAnalyses = JavaOutputParser.Parse(javaFile);

                // Run .NET analysis
                Console.WriteLine("Loading TurkishMorphology (this may take a few seconds)...");
                TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
                Console.WriteLine("Running .NET analysis...");
                var dotNetAnalyses = DotNetAnalysisRunner.Analyze(morphology, sentences);

                // Diff
                Console.WriteLine("Computing diff...");
                ParityReport report = ParityDiffEngine.Diff(sentences, javaAnalyses, dotNetAnalyses, inputPath);

                // Strip per-sentence detail if requested
                if (noDetail)
                    report.Sentences.Clear();

                // Emit JSON report
                if (!string.IsNullOrWhiteSpace(outputPath))
                {
                    string json = JsonSerializer.Serialize(report,
                        new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(outputPath, json, System.Text.Encoding.UTF8);
                    Console.WriteLine($"Report written to: {outputPath}");
                }

                PrintSummary(report);
            }
            finally
            {
                if (tempJavaFile && File.Exists(javaFile))
                    TryDelete(javaFile);
            }
        }

        private static void PrintSummary(ParityReport report)
        {
            Console.WriteLine();
            Console.WriteLine("=== Parity Report ===");
            Console.WriteLine($"  Generated  : {report.GeneratedAt}");
            Console.WriteLine($"  Input      : {report.InputFile}");
            Console.WriteLine($"  Sentences  : {report.TotalSentences}");
            Console.WriteLine($"  Words      : {report.TotalWords}");
            Console.WriteLine($"  Match      : {report.MatchingWords} ({report.MatchRatePct:F1}%)");
            Console.WriteLine($"  Mismatch   : {report.MismatchingWords}");
            if (report.BothUnknownWords > 0)
                Console.WriteLine($"  BothUnknown: {report.BothUnknownWords} (counted as match)");

            if (report.MismatchCounts.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("  Mismatch categories:");
                foreach (var kv in report.MismatchCounts)
                    Console.WriteLine($"    {kv.Key,-25} {kv.Value,6}");
            }
        }

        private static void TryDelete(string path)
        {
            try { File.Delete(path); }
            catch { /* best-effort */ }
        }

        public static void Main(string[] args) => new ValidateParityConsole().Execute(args);
    }
}
