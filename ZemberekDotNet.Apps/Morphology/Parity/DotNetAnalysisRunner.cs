using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Apps.Morphology.Parity
{
    /// <summary>
    /// Runs <see cref="TurkishMorphology.AnalyzeAndDisambiguate"/> over a list of sentences
    /// and returns per-sentence, per-word analysis results in the same structure used by
    /// <see cref="JavaOutputParser"/>.
    /// </summary>
    public static class DotNetAnalysisRunner
    {
        /// <summary>
        /// Analyzes all sentences and returns a dictionary keyed by sentence index.
        /// Each value is the ordered list of word analyses for that sentence.
        /// </summary>
        public static Dictionary<int, List<DotNetWordAnalysis>> Analyze(
            TurkishMorphology morphology,
            IReadOnlyList<string> sentences)
        {
            var result = new Dictionary<int, List<DotNetWordAnalysis>>(sentences.Count);

            for (int i = 0; i < sentences.Count; i++)
            {
                string sentence = sentences[i];
                if (string.IsNullOrWhiteSpace(sentence))
                {
                    result[i] = new List<DotNetWordAnalysis>();
                    continue;
                }

                try
                {
                    List<WordAnalysis> sentenceWordAnalyses = AnalyzeSentenceForJavaParity(morphology, sentence);

                    SentenceAnalysis sentenceAnalysis = morphology.Disambiguate(sentence, sentenceWordAnalyses);
                    var words = new List<DotNetWordAnalysis>();

                    foreach (SentenceWordAnalysis swa in sentenceAnalysis)
                    {
                        WordAnalysis wa = swa.GetWordAnalysis();
                        SingleAnalysis best = swa.GetBestAnalysis();

                        bool unknown = best.IsUnknown();
                        string bestStr = unknown ? "?" : best.FormatLexical();
                        int count = wa.AnalysisCount();

                        words.Add(new DotNetWordAnalysis
                        {
                            SentenceIndex = i,
                            Word = wa.GetInput(),
                            BestAnalysis = bestStr,
                            AnalysisCount = count
                        });
                    }

                    result[i] = words;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"[DotNetAnalysisRunner] Error analyzing sentence {i}: {ex.Message}");
                    result[i] = new List<DotNetWordAnalysis>();
                }
            }

            return result;
        }

        public static List<WordAnalysis> AnalyzeSentenceForJavaParity(
            TurkishMorphology morphology,
            string sentence)
        {
            return morphology.AnalyzeSentence(sentence)
                .Select(FilterForJavaParity)
                .ToList();
        }

        private static WordAnalysis FilterForJavaParity(WordAnalysis analysis)
        {
            List<SingleAnalysis> analyses = analysis.GetAnalysisResults();
            if (analyses.Count == 0)
            {
                return analysis;
            }

            List<SingleAnalysis> filtered = analyses
                .Where(a => !IsJavaDisabledReciprocal(a))
                .ToList();

            if (filtered.Count == 0 || filtered.Count == analyses.Count)
            {
                return analysis;
            }

            return analysis.CopyFor(filtered);
        }

        private static bool IsJavaDisabledReciprocal(SingleAnalysis analysis)
        {
            return analysis.ContainsMorpheme(TurkishMorphotactics.recip)
                && !analysis.GetDictionaryItem().HasAttribute(RootAttribute.Reciprocal);
        }
    }

    /// <summary>One word entry from the .NET morphology analysis.</summary>
    public sealed class DotNetWordAnalysis
    {
        public int SentenceIndex { get; set; }
        public string Word { get; set; }
        public string BestAnalysis { get; set; }
        public int AnalysisCount { get; set; }
    }
}
