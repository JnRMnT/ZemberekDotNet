using System;
using System.Collections.Generic;

namespace ZemberekDotNet.Apps.Morphology.Parity
{
    /// <summary>
    /// Compares Java Zemberek and ZemberekDotNet morphological analyses word-by-word,
    /// categorises each mismatch, and produces a <see cref="ParityReport"/>.
    /// </summary>
    public static class ParityDiffEngine
    {
        /// <summary>
        /// Performs the diff and returns the populated report.
        /// </summary>
        /// <param name="sentences">Original input sentences (0-indexed).</param>
        /// <param name="javaAnalyses">Output of <see cref="JavaOutputParser.Parse"/>.</param>
        /// <param name="dotNetAnalyses">Output of <see cref="DotNetAnalysisRunner.Analyze"/>.</param>
        /// <param name="inputFile">Original input file path (stored in report metadata).</param>
        public static ParityReport Diff(
            IReadOnlyList<string> sentences,
            Dictionary<int, List<JavaWordAnalysis>> javaAnalyses,
            Dictionary<int, List<DotNetWordAnalysis>> dotNetAnalyses,
            string inputFile)
        {
            var report = new ParityReport
            {
                InputFile = inputFile,
                TotalSentences = sentences.Count
            };

            for (int i = 0; i < sentences.Count; i++)
            {
                javaAnalyses.TryGetValue(i, out List<JavaWordAnalysis> javaWords);
                dotNetAnalyses.TryGetValue(i, out List<DotNetWordAnalysis> dotNetWords);

                javaWords ??= new List<JavaWordAnalysis>();
                dotNetWords ??= new List<DotNetWordAnalysis>();

                var sp = DiffSentence(i, sentences[i], javaWords, dotNetWords, report);
                report.Sentences.Add(sp);
            }

            report.MatchRate.ToString(); // force evaluation (computed property)
            return report;
        }

        private static SentenceParity DiffSentence(
            int index,
            string sentence,
            List<JavaWordAnalysis> javaWords,
            List<DotNetWordAnalysis> dotNetWords,
            ParityReport report)
        {
            var sp = new SentenceParity
            {
                Index = index,
                Sentence = sentence
            };

            // If word counts differ for the same sentence, every word in that sentence
            // is marked TokenizationDiff and we skip per-word comparison.
            if (javaWords.Count != dotNetWords.Count && javaWords.Count > 0 && dotNetWords.Count > 0)
            {
                int wordCount = Math.Max(javaWords.Count, dotNetWords.Count);
                report.TotalWords += wordCount;

                for (int wi = 0; wi < wordCount; wi++)
                {
                    string word = wi < dotNetWords.Count ? dotNetWords[wi].Word
                                : wi < javaWords.Count ? javaWords[wi].Word
                                : "?";

                    string javaB = wi < javaWords.Count ? javaWords[wi].BestAnalysis : "—";
                    string dotNetB = wi < dotNetWords.Count ? dotNetWords[wi].BestAnalysis : "—";

                    var wp = new WordParity
                    {
                        Word = word,
                        Match = false,
                        Category = MismatchCategory.TokenizationDiff,
                        JavaBest = javaB,
                        DotNetBest = dotNetB,
                        JavaCount = wi < javaWords.Count ? javaWords[wi].AnalysisCount : 0,
                        DotNetCount = wi < dotNetWords.Count ? dotNetWords[wi].AnalysisCount : 0
                    };

                    sp.Words.Add(wp);
                    report.MismatchingWords++;
                    IncrementMismatch(report, MismatchCategory.TokenizationDiff);
                }

                sp.FullMatch = false;
                return sp;
            }

            bool sentenceFullMatch = true;
            int pairCount = Math.Max(javaWords.Count, dotNetWords.Count);
            report.TotalWords += pairCount;

            for (int wi = 0; wi < pairCount; wi++)
            {
                JavaWordAnalysis java = wi < javaWords.Count ? javaWords[wi] : null;
                DotNetWordAnalysis dotNet = wi < dotNetWords.Count ? dotNetWords[wi] : null;

                string word = dotNet?.Word ?? java?.Word ?? "?";
                string jBest = java?.BestAnalysis ?? "—";
                string dBest = dotNet?.BestAnalysis ?? "—";
                int jCount = java?.AnalysisCount ?? 0;
                int dCount = dotNet?.AnalysisCount ?? 0;

                string category = Categorize(jBest, dBest, jCount, dCount);
                bool match = category == null;

                var wp = new WordParity
                {
                    Word = word,
                    Match = match,
                    Category = category,
                    JavaBest = jBest,
                    DotNetBest = dBest,
                    JavaCount = jCount,
                    DotNetCount = dCount
                };

                sp.Words.Add(wp);

                if (match)
                {
                    report.MatchingWords++;
                }
                else
                {
                    sentenceFullMatch = false;
                    report.MismatchingWords++;
                    IncrementMismatch(report, category);
                }
            }

            sp.FullMatch = sentenceFullMatch;
            return sp;
        }

        /// <summary>
        /// Returns the mismatch category key, or null if the analyses match.
        /// </summary>
        private static string Categorize(string jBest, string dBest, int jCount, int dCount)
        {
            bool jUnknown = jCount == 0 || jBest == "?";
            bool dUnknown = dCount == 0 || dBest == "?";

            if (jUnknown && dUnknown)
                return MismatchCategory.BothUnknown;

            if (jUnknown != dUnknown)
                return MismatchCategory.LexiconGap;

            // Both have analyses — compare best strings
            bool bestMatch = string.Equals(jBest, dBest, StringComparison.Ordinal);

            if (!bestMatch)
                return MismatchCategory.BestAnalysisDiff;

            // Best matches but candidate count differs
            if (jCount != dCount)
                return MismatchCategory.AnalysisCountDiff;

            return null; // full match
        }

        private static void IncrementMismatch(ParityReport report, string category)
        {
            if (!report.MismatchCounts.TryGetValue(category, out int current))
                current = 0;
            report.MismatchCounts[category] = current + 1;
        }
    }
}
