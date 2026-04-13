using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ZemberekDotNet.Apps.Morphology.Parity;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Apps.Morphology
{
    /// <summary>
    /// Audits Java vs .NET morphology parity at the candidate level.
    ///
    /// For every word where Java and .NET disagree, this command determines whether:
    ///   (A) CANDIDATE_GAP   — Java's best analysis is not generated at all by .NET morphotactics/lexicon
    ///                         → this is a genuine port incompleteness; no amount of training fixes it
    ///   (B) DISAMBIG_MISS   — .NET generates Java's best analysis as a candidate but the
    ///                         disambiguation model chose a different one
    ///                         → fixable by training the perceptron on better data
    ///
    /// The output is a JSON report plus a console summary grouped by analysis suffix pattern.
    ///
    /// Usage:
    ///   AuditParityConsole --input sentences.txt --java-output java.tsv --output audit.json
    /// </summary>
    public class AuditParityConsole : ConsoleApp<AuditParityConsole>
    {
        [Parameter("--input", "-i", Description = "Input sentences file (one per line).")]
        string inputPath;

        [Parameter("--java-output", "-j", Description = "Pre-generated Java morphology TSV path.")]
        string javaOutputPath = "";

        [Parameter("--java-jar", Description = "Optional Zemberek fat JAR; auto-generates TSV when provided.")]
        string javaJarPath = "";

        [Parameter("--java-args", Description = "Custom Java invocation template. Use {input} and {output} placeholders.")]
        string javaArgs = "";

        [Parameter("--output", "-o", Description = "Output JSON audit report path.")]
        string outputPath = "";

        [Parameter("--max", "-n", Description = "Maximum sentences to process. Default: 1000.")]
        int maxSentences = 1000;

        [Parameter("--top", "-t", Description = "Show top N gap patterns in console summary. Default: 20.")]
        int topPatterns = 20;

        public override string Description() =>
            "Audits Java vs .NET parity at the candidate-list level. " +
            "Classifies each mismatch as CANDIDATE_GAP (port incompleteness — Java best not in .NET candidate list) " +
            "or DISAMBIG_MISS (.NET has the candidate but picked a different best). " +
            "Reports top missing analysis patterns to guide morphotactics/lexicon fixes.";

        public override void Run()
        {
            if (!ValidateArgs()) return;

            ResourceBootstrap.EnsureGlobalResourcesRoot();

            string[] allLines = File.ReadAllLines(inputPath)
                .Select(s => s.TrimStart('\uFEFF'))
                .ToArray();
            string[] sentences = allLines.Length > maxSentences ? allLines[..maxSentences] : allLines;
            Console.WriteLine($"Auditing {sentences.Length} sentence(s) from: {inputPath}");

            string javaFile = javaOutputPath;
            bool tempJava = false;
            if (string.IsNullOrWhiteSpace(javaFile))
            {
                javaFile = Path.Combine(Path.GetTempPath(), $"zemberek_audit_java_{Guid.NewGuid():N}.tsv");
                tempJava = true;
                Console.WriteLine($"Running Java analysis (output: {javaFile})...");
                JavaProcessRunner.Run(javaJarPath, javaArgs, inputPath, javaFile);
            }

            try
            {
                Console.WriteLine("Parsing Java output...");
                var javaAnalyses = JavaOutputParser.Parse(javaFile);

                Console.WriteLine("Loading .NET morphology...");
                TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();

                Console.WriteLine("Auditing candidates...");
                AuditResult result = RunAudit(morphology, sentences, javaAnalyses);

                if (!string.IsNullOrWhiteSpace(outputPath))
                {
                    string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(outputPath, json, Encoding.UTF8);
                    Console.WriteLine($"Audit report written to: {outputPath}");
                }

                PrintSummary(result);
            }
            finally
            {
                if (tempJava && File.Exists(javaFile))
                    TryDelete(javaFile);
            }
        }

        // ─── Core audit logic ─────────────────────────────────────────────────────

        private static AuditResult RunAudit(
            TurkishMorphology morphology,
            string[] sentences,
            Dictionary<int, List<JavaWordAnalysis>> javaAnalyses)
        {
            var result = new AuditResult();

            for (int si = 0; si < sentences.Count(); si++)
            {
                string sentence = sentences[si];
                if (string.IsNullOrWhiteSpace(sentence)) continue;

                javaAnalyses.TryGetValue(si, out var javaWords);
                if (javaWords == null || javaWords.Count == 0) continue;

                    List<WordAnalysis> dotNetWords = DotNetAnalysisRunner.AnalyzeSentenceForJavaParity(morphology, sentence);
                if (dotNetWords.Count != javaWords.Count)
                {
                    result.TokenizationDiffs++;
                    continue;
                }

                for (int wi = 0; wi < dotNetWords.Count; wi++)
                {
                    WordAnalysis wa = dotNetWords[wi];
                    JavaWordAnalysis jw = javaWords[wi];

                    result.TotalWords++;

                    // Skip both-unknown
                    bool javaUnknown = jw.AnalysisCount == 0 || jw.BestAnalysis == "?";
                    bool dotNetUnknown = wa.AnalysisCount() == 0;

                    if (javaUnknown && dotNetUnknown)
                    {
                        result.BothUnknown++;
                        continue;
                    }

                    // Get .NET candidates in FormatLexical form
                    List<SingleAnalysis> candidates = wa.GetAnalysisResults();
                    HashSet<string> candidateLexical = new HashSet<string>(StringComparer.Ordinal);
                    foreach (SingleAnalysis sa in candidates)
                        candidateLexical.Add(sa.FormatLexical());

                    // Check if .NET best matches Java best
                    SentenceAnalysis sentenceAnalysis = morphology.AnalyzeAndDisambiguate(sentence);
                    // Use per-word best from disambiguation
                    // We need per-word best so we rebuild per sentence lazily below
                    // (handled outside — see comment)
                    break; // placeholder — real logic below in RunAuditWord
                }
            }

            // Real implementation: rebuild per-word best via a single sentence pass
            return RunAuditFull(morphology, sentences, javaAnalyses);
        }

        private static AuditResult RunAuditFull(
            TurkishMorphology morphology,
            string[] sentences,
            Dictionary<int, List<JavaWordAnalysis>> javaAnalyses)
        {
            var result = new AuditResult();

            for (int si = 0; si < sentences.Length; si++)
            {
                string sentence = sentences[si];
                if (string.IsNullOrWhiteSpace(sentence)) continue;

                javaAnalyses.TryGetValue(si, out var javaWords);
                if (javaWords == null || javaWords.Count == 0) continue;

                    List<WordAnalysis> wordAnalyses = DotNetAnalysisRunner.AnalyzeSentenceForJavaParity(morphology, sentence);
                if (wordAnalyses.Count != javaWords.Count)
                {
                    result.TokenizationDiffs++;
                    continue;
                }

                // Run disambiguation once for the sentence
                SentenceAnalysis disambiguated;
                try
                {
                    disambiguated = morphology.Disambiguate(sentence, wordAnalyses);
                }
                catch
                {
                    result.TokenizationDiffs++;
                    continue;
                }

                List<SentenceWordAnalysis> swaList = new List<SentenceWordAnalysis>();
                foreach (SentenceWordAnalysis swa in disambiguated)
                    swaList.Add(swa);

                if (swaList.Count != javaWords.Count)
                {
                    result.TokenizationDiffs++;
                    continue;
                }

                for (int wi = 0; wi < javaWords.Count; wi++)
                {
                    result.TotalWords++;
                    JavaWordAnalysis jw = javaWords[wi];
                    WordAnalysis wa = wordAnalyses[wi];
                    SingleAnalysis dotNetBest = swaList[wi].GetBestAnalysis();

                    bool javaUnknown = jw.AnalysisCount == 0 || jw.BestAnalysis == "?";
                    bool dotNetUnknown = dotNetBest.IsUnknown();

                    if (javaUnknown && dotNetUnknown)
                    {
                        result.BothUnknown++;
                        continue;
                    }

                    if (javaUnknown != dotNetUnknown)
                    {
                        result.LexiconGap++;
                        result.AddGap("LexiconGap", jw.Word, jw.BestAnalysis, dotNetBest.FormatLexical(), sentence);
                        continue;
                    }

                    string javaBest = jw.BestAnalysis;
                    string dotNetBestStr = dotNetBest.FormatLexical();

                    if (string.Equals(javaBest, dotNetBestStr, StringComparison.Ordinal))
                    {
                        result.Match++;
                        continue;
                    }

                    // Mismatch — classify by checking if .NET candidates include Java best
                    List<SingleAnalysis> candidates = wa.GetAnalysisResults();
                    bool javaBestInCandidates = candidates.Any(sa =>
                        string.Equals(sa.FormatLexical(), javaBest, StringComparison.Ordinal));

                    if (javaBestInCandidates)
                    {
                        result.DisambigMiss++;
                        result.AddMiss("DisambigMiss", jw.Word, javaBest, dotNetBestStr, sentence);
                    }
                    else
                    {
                        result.CandidateGap++;
                        // Derive a pattern key from the Java analysis suffix for grouping
                        string pattern = DerivePattern(javaBest);
                        result.AddGap(pattern, jw.Word, javaBest, dotNetBestStr, sentence);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts a grouping key like "Noun+A3sg+P3sg+*" from a full lexical analysis string.
        /// Strips the lemma prefix so patterns across different lemmas are grouped together.
        /// </summary>
        private static string DerivePattern(string javaBest)
        {
            if (string.IsNullOrEmpty(javaBest) || javaBest == "?") return "Unknown";
            int colonIdx = javaBest.IndexOf(':');
            return colonIdx >= 0 ? javaBest[(colonIdx + 1)..] : javaBest;
        }

        // ─── Reporting ────────────────────────────────────────────────────────────

        private void PrintSummary(AuditResult r)
        {
            Console.WriteLine();
            Console.WriteLine("=== Parity Audit ===");
            Console.WriteLine($"  TotalWords      : {r.TotalWords}");
            Console.WriteLine($"  Match           : {r.Match} ({r.Match * 100.0 / Math.Max(r.TotalWords, 1):F1}%)");
            Console.WriteLine($"  DisambigMiss    : {r.DisambigMiss}  ← fixable by model retraining");
            Console.WriteLine($"  CandidateGap    : {r.CandidateGap}  ← requires port fix (morphotactics/lexicon)");
            Console.WriteLine($"  BothUnknown     : {r.BothUnknown}");
            Console.WriteLine($"  LexiconGap      : {r.LexiconGap}");
            Console.WriteLine($"  TokenizationDiffs: {r.TokenizationDiffs} sentences skipped");

            if (r.CandidateGap > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"  Top {topPatterns} CANDIDATE_GAP patterns (analysis suffix → count, examples):");
                var gapGroups = r.GapEntries
                    .GroupBy(g => g.Pattern)
                    .OrderByDescending(g => g.Count())
                    .Take(topPatterns);

                foreach (var grp in gapGroups)
                {
                    Console.WriteLine();
                    Console.WriteLine($"    [{grp.Count(),4}] {grp.Key}");
                    foreach (var ex in grp.Take(3))
                        Console.WriteLine($"           word=\"{ex.Word}\"  java=\"{ex.JavaBest}\"  dotnet=\"{ex.DotNetBest}\"");
                }
            }

            if (r.DisambigMiss > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"  Top 10 DISAMBIG_MISS patterns (Java chose vs .NET chose):");
                var missGroups = r.MissEntries
                    .GroupBy(m => $"{DerivePattern(m.JavaBest)} vs {DerivePattern(m.DotNetBest)}")
                    .OrderByDescending(g => g.Count())
                    .Take(10);

                foreach (var grp in missGroups)
                {
                    Console.WriteLine($"    [{grp.Count(),4}] {grp.Key}");
                    foreach (var ex in grp.Take(2))
                        Console.WriteLine($"           word=\"{ex.Word}\"");
                }
            }
        }

        // ─── Validation ───────────────────────────────────────────────────────────

        private bool ValidateArgs()
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            { Console.Error.WriteLine("Error: --input is required."); return false; }
            if (!File.Exists(inputPath))
            { Console.Error.WriteLine($"Error: input not found: {inputPath}"); return false; }
            if (string.IsNullOrWhiteSpace(javaOutputPath) && string.IsNullOrWhiteSpace(javaJarPath))
            { Console.Error.WriteLine("Error: --java-output or --java-jar required."); return false; }
            if (!string.IsNullOrWhiteSpace(javaOutputPath) && !File.Exists(javaOutputPath))
            { Console.Error.WriteLine($"Error: java output not found: {javaOutputPath}"); return false; }
            return true;
        }

        private static void TryDelete(string path)
        {
            try { File.Delete(path); } catch { }
        }
    }

    // ─── Result model ──────────────────────────────────────────────────────────────

    public class AuditResult
    {
        public int TotalWords { get; set; }
        public int Match { get; set; }
        public int DisambigMiss { get; set; }
        public int CandidateGap { get; set; }
        public int BothUnknown { get; set; }
        public int LexiconGap { get; set; }
        public int TokenizationDiffs { get; set; }

        public List<GapEntry> GapEntries { get; set; } = new List<GapEntry>();
        public List<GapEntry> MissEntries { get; set; } = new List<GapEntry>();

        public void AddGap(string pattern, string word, string javaBest, string dotNetBest, string sentence)
        {
            GapEntries.Add(new GapEntry
            {
                Pattern = pattern,
                Word = word,
                JavaBest = javaBest,
                DotNetBest = dotNetBest,
                Sentence = sentence
            });
        }

        public void AddMiss(string pattern, string word, string javaBest, string dotNetBest, string sentence)
        {
            MissEntries.Add(new GapEntry
            {
                Pattern = pattern,
                Word = word,
                JavaBest = javaBest,
                DotNetBest = dotNetBest,
                Sentence = sentence
            });
        }
    }

    public class GapEntry
    {
        public string Pattern { get; set; }
        public string Word { get; set; }
        public string JavaBest { get; set; }
        public string DotNetBest { get; set; }
        public string Sentence { get; set; }
    }
}
