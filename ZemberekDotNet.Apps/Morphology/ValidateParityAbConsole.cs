using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ZemberekDotNet.Apps.Morphology.Parity;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Ambiguity;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Apps.Morphology
{
    /// <summary>
    /// Runs baseline and trained ambiguity-resolver parity comparison against Java TSV output.
    /// </summary>
    public class ValidateParityAbConsole : ConsoleApp<ValidateParityAbConsole>
    {
        [Parameter("--input", "-i", Description = "Path to input text file with one sentence per line.")]
        string inputPath;

        [Parameter("--java-output", "-j", Description = "Pre-generated Java morphology TSV path.")]
        string javaOutputPath = "";

        [Parameter("--java-jar", Description = "Optional Zemberek fat JAR path. If set, Java TSV is generated automatically.")]
        string javaJarPath = "";

        [Parameter("--java-args",
            Description = "Custom Java invocation args template. Use {input} and {output}. " +
                          "Default: 'zemberek.apps.ApplicationRunner MorphAnalyze --input {input} --output {output}'")]
        string javaArgs = "";

        [Parameter("--output", "-o", Description = "Output JSON report path for baseline-vs-trained summary.")]
        string outputPath = "";

        [Parameter("--max", "-n", Description = "Maximum number of sentences to process. Default: 1000.")]
        int maxSentences = 1000;

        [Parameter("--iterations", Description = "Perceptron training iteration count. Default: 2.")]
        int iterationCount = 2;

        [Parameter("--train-ratio", Description = "Train split ratio for generated corpus. Range: (0,1]. Default: 0.8. Use 1.0 to train on all usable sentences (overfit mode).")]
        double trainRatio = 0.8;

        [Parameter("--train-file", Description = "Optional explicit path for generated training file.")]
        string trainFilePath = "";

        [Parameter("--dev-file", Description = "Optional explicit path for generated dev file.")]
        string devFilePath = "";

        [Parameter("--keep-generated", Description = "Keep generated Java/train/dev files even when temp paths are used.")]
        bool keepGenerated;

        public override string Description() =>
            "Runs reproducible parity A/B: baseline default ambiguity model vs resolver trained " +
            "from Java best analyses on the same corpus. Requires --input and either --java-output " +
            "or --java-jar. Produces a compact JSON summary with baseline, trained, and delta metrics.";

        public override void Run()
        {
            if (!ValidateArgs())
            {
                return;
            }

            ResourceBootstrap.EnsureGlobalResourcesRoot();

            string[] allLines = File.ReadAllLines(inputPath)
                .Select(s => s.TrimStart('\uFEFF'))
                .ToArray();
            string[] sentences = allLines.Length > maxSentences ? allLines[..maxSentences] : allLines;

            Console.WriteLine($"Processing {sentences.Length} sentence(s) from: {inputPath}");

            string javaFile = javaOutputPath;
            bool tempJavaFile = false;

            if (string.IsNullOrWhiteSpace(javaFile))
            {
                javaFile = Path.Combine(Path.GetTempPath(), $"zemberek_parity_java_{Guid.NewGuid():N}.tsv");
                tempJavaFile = true;
                Console.WriteLine($"Running Java analysis (output: {javaFile})...");
                JavaProcessRunner.Run(javaJarPath, javaArgs, inputPath, javaFile);
            }

            string trainFile = string.IsNullOrWhiteSpace(trainFilePath)
                ? Path.Combine(Path.GetTempPath(), $"zemberek_parity_train_{Guid.NewGuid():N}.txt")
                : trainFilePath;

            string devFile = string.IsNullOrWhiteSpace(devFilePath)
                ? Path.Combine(Path.GetTempPath(), $"zemberek_parity_dev_{Guid.NewGuid():N}.txt")
                : devFilePath;

            bool tempTrainFile = string.IsNullOrWhiteSpace(trainFilePath);
            bool tempDevFile = string.IsNullOrWhiteSpace(devFilePath);

            try
            {
                Console.WriteLine("Parsing Java output...");
                Dictionary<int, List<JavaWordAnalysis>> javaAnalyses = JavaOutputParser.Parse(javaFile);

                Console.WriteLine("Loading baseline morphology...");
                TurkishMorphology baselineMorphology = TurkishMorphology.CreateWithDefaults();

                Console.WriteLine("Running baseline parity...");
                Dictionary<int, List<DotNetWordAnalysis>> baselineAnalyses =
                    DotNetAnalysisRunner.Analyze(baselineMorphology, sentences);
                ParityReport baselineReport =
                    ParityDiffEngine.Diff(sentences, javaAnalyses, baselineAnalyses, inputPath);
                baselineReport.Sentences.Clear();

                Console.WriteLine("Building training corpus from Java best analyses...");
                CorpusBuildStats corpusStats = BuildTrainingCorpus(
                    sentences,
                    javaAnalyses,
                    baselineMorphology,
                    trainFile,
                    devFile,
                    trainRatio);

                if (corpusStats.TrainSentences == 0 || corpusStats.DevSentences == 0)
                {
                    Console.Error.WriteLine("Error: generated train/dev corpus is empty. Check Java TSV alignment with input.");
                    return;
                }

                Console.WriteLine($"Train sentences: {corpusStats.TrainSentences}, Dev sentences: {corpusStats.DevSentences}");
                Console.WriteLine($"Skipped sentences: {corpusStats.SkippedSentences}");

                Console.WriteLine("Training ambiguity resolver...");
                var trainer = new PerceptronAmbiguityResolverTrainer(baselineMorphology);
                PerceptronAmbiguityResolver trainedResolver =
                    trainer.Train(trainFile, devFile, iterationCount);

                Console.WriteLine("Running trained parity...");
                TurkishMorphology trainedMorphology = TurkishMorphology.Builder(RootLexicon.GetDefault())
                    .SetAmbiguityResolver(trainedResolver)
                    .Build();
                Dictionary<int, List<DotNetWordAnalysis>> trainedAnalyses =
                    DotNetAnalysisRunner.Analyze(trainedMorphology, sentences);
                ParityReport trainedReport =
                    ParityDiffEngine.Diff(sentences, javaAnalyses, trainedAnalyses, inputPath);
                trainedReport.Sentences.Clear();

                ParityAbReport report = new ParityAbReport
                {
                    InputFile = inputPath,
                    JavaOutputFile = javaFile,
                    TotalSentences = sentences.Length,
                    IterationCount = iterationCount,
                    TrainSentences = corpusStats.TrainSentences,
                    DevSentences = corpusStats.DevSentences,
                    SkippedSentences = corpusStats.SkippedSentences,
                    Baseline = ParitySummary.FromParityReport(baselineReport),
                    Trained = ParitySummary.FromParityReport(trainedReport)
                };

                if (!string.IsNullOrWhiteSpace(outputPath))
                {
                    string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(outputPath, json, Encoding.UTF8);
                    Console.WriteLine($"A/B report written to: {outputPath}");
                }

                PrintSummary(report);
            }
            finally
            {
                if (!keepGenerated)
                {
                    if (tempJavaFile)
                    {
                        TryDelete(javaFile);
                    }
                    if (tempTrainFile)
                    {
                        TryDelete(trainFile);
                    }
                    if (tempDevFile)
                    {
                        TryDelete(devFile);
                    }
                }
            }
        }

        private bool ValidateArgs()
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                Console.Error.WriteLine("Error: --input is required.");
                return false;
            }

            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: input file not found: {inputPath}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(javaOutputPath) && string.IsNullOrWhiteSpace(javaJarPath))
            {
                Console.Error.WriteLine("Error: either --java-output or --java-jar must be provided.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(javaOutputPath) && !File.Exists(javaOutputPath))
            {
                Console.Error.WriteLine($"Error: java output file not found: {javaOutputPath}");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(javaJarPath) && !File.Exists(javaJarPath))
            {
                Console.Error.WriteLine($"Error: java jar file not found: {javaJarPath}");
                return false;
            }

            if (maxSentences <= 0)
            {
                Console.Error.WriteLine("Error: --max must be > 0.");
                return false;
            }

            if (iterationCount <= 0)
            {
                Console.Error.WriteLine("Error: --iterations must be > 0.");
                return false;
            }

            if (trainRatio <= 0 || trainRatio > 1.0)
            {
                Console.Error.WriteLine("Error: --train-ratio must be in range (0, 1].");
                return false;
            }

            return true;
        }

        private static CorpusBuildStats BuildTrainingCorpus(
            IReadOnlyList<string> sentences,
            Dictionary<int, List<JavaWordAnalysis>> javaAnalyses,
            TurkishMorphology morphology,
            string trainFile,
            string devFile,
            double trainRatio)
        {
            var usableBlocks = new List<List<string>>();

            int skipped = 0;

            for (int i = 0; i < sentences.Count; i++)
            {
                string sentence = sentences[i];
                if (string.IsNullOrWhiteSpace(sentence))
                {
                    skipped++;
                    continue;
                }

                javaAnalyses.TryGetValue(i, out List<JavaWordAnalysis> javaWords);
                javaWords ??= new List<JavaWordAnalysis>();

                List<WordAnalysis> dotNetWords = morphology.AnalyzeSentence(sentence);

                if (javaWords.Count == 0 || dotNetWords.Count == 0 || javaWords.Count != dotNetWords.Count)
                {
                    skipped++;
                    continue;
                }

                var block = new List<string> { "S:" + sentence };
                bool badSentence = false;

                for (int w = 0; w < dotNetWords.Count; w++)
                {
                    WordAnalysis dotNetWord = dotNetWords[w];
                    JavaWordAnalysis javaWord = javaWords[w];

                    if (!string.Equals(dotNetWord.GetInput(), javaWord.Word, StringComparison.Ordinal))
                    {
                        badSentence = true;
                        break;
                    }

                    List<SingleAnalysis> candidates = dotNetWord.GetAnalysisResults();
                    if (candidates.Count == 0)
                    {
                        badSentence = true;
                        break;
                    }

                    List<SingleAnalysis> uniqueCandidates = candidates
                        .GroupBy(a => a.FormatLong())
                        .Select(g => g.First())
                        .ToList();

                    SingleAnalysis selected = null;

                    if (javaWord.AnalysisCount == 0 || javaWord.BestAnalysis == "?")
                    {
                        selected = uniqueCandidates.FirstOrDefault(a => a.IsUnknown());
                        if (selected == null && uniqueCandidates.Count == 1)
                        {
                            selected = uniqueCandidates[0];
                        }
                    }
                    else
                    {
                        selected = uniqueCandidates.FirstOrDefault(a =>
                            string.Equals(a.FormatLexical(), javaWord.BestAnalysis, StringComparison.Ordinal) ||
                            string.Equals(a.FormatLong(), javaWord.BestAnalysis, StringComparison.Ordinal));
                    }

                    if (selected == null)
                    {
                        badSentence = true;
                        break;
                    }

                    block.Add(dotNetWord.GetInput());
                    for (int c = 0; c < uniqueCandidates.Count; c++)
                    {
                        string analysis = uniqueCandidates[c].FormatLong();
                        if (uniqueCandidates.Count > 1 && ReferenceEquals(uniqueCandidates[c], selected))
                        {
                            analysis += "*";
                        }
                        block.Add(analysis);
                    }
                }

                if (badSentence)
                {
                    skipped++;
                    continue;
                }

                usableBlocks.Add(block);
            }

            List<List<string>> trainBlocks;
            List<List<string>> devBlocks;

            if (trainRatio >= 1.0)
            {
                // Overfit mode for parity debugging: train and evaluate with the same usable set.
                trainBlocks = new List<List<string>>(usableBlocks);
                devBlocks = new List<List<string>>(usableBlocks);
            }
            else
            {
                int trainCount = (int)(usableBlocks.Count * trainRatio);
                if (trainCount <= 0 && usableBlocks.Count > 1)
                {
                    trainCount = 1;
                }

                if (trainCount >= usableBlocks.Count && usableBlocks.Count > 1)
                {
                    trainCount = usableBlocks.Count - 1;
                }

                trainBlocks = usableBlocks.Take(trainCount).ToList();
                devBlocks = usableBlocks.Skip(trainCount).ToList();
            }

            WriteBlocks(trainFile, trainBlocks);
            WriteBlocks(devFile, devBlocks);

            return new CorpusBuildStats
            {
                TrainSentences = trainBlocks.Count,
                DevSentences = devBlocks.Count,
                SkippedSentences = skipped
            };
        }

        private static void WriteBlocks(string path, List<List<string>> blocks)
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            foreach (List<string> block in blocks)
            {
                foreach (string line in block)
                {
                    writer.WriteLine(line);
                }
            }
        }

        private static void PrintSummary(ParityAbReport report)
        {
            Console.WriteLine();
            Console.WriteLine("=== Baseline ===");
            PrintParitySummary(report.Baseline);

            Console.WriteLine();
            Console.WriteLine("=== Trained ===");
            PrintParitySummary(report.Trained);

            Console.WriteLine();
            Console.WriteLine($"MatchRate Delta: {report.MatchRateDeltaPct:+0.00;-0.00;0.00}%");
            Console.WriteLine($"Train/Dev Sentences: {report.TrainSentences}/{report.DevSentences}");
            Console.WriteLine($"Skipped Sentences: {report.SkippedSentences}");
        }

        private static void PrintParitySummary(ParitySummary summary)
        {
            Console.WriteLine($"Words: {summary.Words}");
            Console.WriteLine($"Match: {summary.Match}");
            Console.WriteLine($"Mismatch: {summary.Mismatch}");
            Console.WriteLine($"MatchRatePct: {summary.MatchRatePct:F2}");
            foreach (var kv in summary.MismatchCounts.OrderByDescending(kv => kv.Value))
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // best effort cleanup
            }
        }

        private sealed class CorpusBuildStats
        {
            public int TrainSentences { get; set; }
            public int DevSentences { get; set; }
            public int SkippedSentences { get; set; }
        }
    }
}