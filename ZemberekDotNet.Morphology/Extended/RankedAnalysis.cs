using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Extended
{
    /// <summary>
    /// A single morphological analysis paired with a normalised confidence score.
    /// Produced by <see cref="WordAnalysisExtensions.ExtdGetRankedAnalyses"/> and
    /// <see cref="TurkishMorphologyExtensions.ExtdAnalyzeWithRanking"/>.
    /// </summary>
    /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
    public sealed class RankedAnalysis
    {
        /// <summary>The underlying morphological analysis.</summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public SingleAnalysis Analysis { get; }

        /// <summary>
        /// Normalised confidence score in [0, 1].
        /// Scores across all analyses for the same input word are softmax-normalised and sum to 1.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public double Confidence { get; }

        internal RankedAnalysis(SingleAnalysis analysis, double confidence)
        {
            Analysis = analysis;
            Confidence = confidence;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[{Confidence:F4}] {Analysis}";
    }
}
