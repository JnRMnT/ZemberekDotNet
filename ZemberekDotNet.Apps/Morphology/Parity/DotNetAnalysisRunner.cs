using System;
using System.Collections.Generic;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;

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
                    SentenceAnalysis sentenceAnalysis = morphology.AnalyzeAndDisambiguate(sentence);
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
