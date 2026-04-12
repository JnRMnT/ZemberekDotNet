using System;
using System.Collections.Generic;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Extended
{
    /// <summary>
    /// Extension methods that add ambiguity ranking to <see cref="WordAnalysis"/>.
    /// </summary>
    /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
    public static class WordAnalysisExtensions
    {
        /// <summary>
        /// Returns the analyses for this word ranked by confidence, descending.
        /// Confidence scores are softmax-normalised and sum to 1 (±floating-point rounding).
        /// </summary>
        /// <param name="wordAnalysis">The <see cref="WordAnalysis"/> to rank.</param>
        /// <param name="model">
        /// Optional <see cref="WordFrequencyModel"/> for corpus-blended scoring.
        /// When <c>null</c>, a heuristic based on morpheme-chain complexity is used exclusively.
        /// </param>
        /// <remarks>
        /// ZemberekDotNet addition — no equivalent in Java Zemberek.
        /// <para>
        /// <b>Scoring heuristic</b>: each analysis receives a base score
        /// <c>1 / (1 + morphemeGroupCount + derivationDepth)</c>.
        /// When a <see cref="WordFrequencyModel"/> is supplied the score is blended 50/50 with
        /// the normalised stem frequency: <c>0.5 * heuristic + 0.5 * corpusScore</c>.
        /// All scores are then softmax-normalised so they sum to 1.
        /// </para>
        /// <para>
        /// For sentence-level disambiguation, prefer
        /// <c>TurkishMorphology.AnalyzeAndDisambiguate</c> followed by
        /// <c>SentenceWordAnalysis.GetBestAnalysis()</c>, which uses a trained perceptron model.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// using ZemberekDotNet.Morphology.Extended;
        ///
        /// var morphology = TurkishMorphology.CreateWithDefaults();
        /// var model = WordFrequencyModel.FromEmbeddedResource();
        /// var ranked = morphology.Analyze("yüzde").ExtdGetRankedAnalyses(model);
        /// // ranked[0].Confidence >= ranked[1].Confidence
        /// </code>
        /// </example>
        public static List<RankedAnalysis> ExtdGetRankedAnalyses(
            this WordAnalysis wordAnalysis,
            WordFrequencyModel model = null)
        {
            List<SingleAnalysis> analyses = wordAnalysis.GetAnalysisResults();
            if (analyses.Count == 0)
            {
                return new List<RankedAnalysis>();
            }

            double[] rawScores = new double[analyses.Count];
            for (int i = 0; i < analyses.Count; i++)
            {
                rawScores[i] = ComputeScore(analyses[i], model);
            }

            double[] softmax = Softmax(rawScores);

            // Build result list.
            List<RankedAnalysis> result = new List<RankedAnalysis>(analyses.Count);
            for (int i = 0; i < analyses.Count; i++)
            {
                result.Add(new RankedAnalysis(analyses[i], softmax[i]));
            }

            // Sort descending by confidence.
            result.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
            return result;
        }

        // --- Private helpers ---

        private static double ComputeScore(SingleAnalysis analysis, WordFrequencyModel model)
        {
            int morphemeGroupCount = analysis.GetMorphemeGroupCount();
            int derivationDepth = 0;
            foreach (var m in analysis.GetMorphemes())
            {
                if (m.Derivational1)
                {
                    derivationDepth++;
                }
            }

            double hScore = 1.0 / (1.0 + morphemeGroupCount + derivationDepth);

            if (model != null)
            {
                double corpusScore = model.GetNormalizedScore(analysis.GetStem());
                return 0.5 * hScore + 0.5 * corpusScore;
            }

            return hScore;
        }

        private static double[] Softmax(double[] scores)
        {
            // Find max for numerical stability.
            double max = scores[0];
            for (int i = 1; i < scores.Length; i++)
            {
                if (scores[i] > max)
                {
                    max = scores[i];
                }
            }

            double[] exps = new double[scores.Length];
            double sum = 0.0;
            for (int i = 0; i < scores.Length; i++)
            {
                exps[i] = Math.Exp(scores[i] - max);
                sum += exps[i];
            }

            double[] result = new double[scores.Length];
            for (int i = 0; i < scores.Length; i++)
            {
                result[i] = exps[i] / sum;
            }
            return result;
        }
    }
}
