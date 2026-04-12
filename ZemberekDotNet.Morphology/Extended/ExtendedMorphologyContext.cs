using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Extended
{
    /// <summary>
    /// Stateful context that wraps a <see cref="TurkishMorphology"/> instance and exposes all
    /// ZemberekDotNet Extended API features as instance methods.
    /// </summary>
    /// <remarks>
    /// ZemberekDotNet addition — no equivalent in Java Zemberek.
    /// <para>
    /// Use this class when you need <see cref="FuzzyAnalyze"/>, which internally maintains a
    /// BK-tree (<see cref="BkTree"/>) that is built once on the first call and then cached.
    /// The BK-tree is thread-safe for reads after construction; construction itself is
    /// synchronised via a lock.
    /// </para>
    /// <para>
    /// All other methods are thin wrappers over the corresponding extension methods and can
    /// alternatively be called directly via <c>using ZemberekDotNet.Morphology.Extended;</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var morphology = TurkishMorphology.CreateWithDefaults();
    /// var model      = WordFrequencyModel.FromEmbeddedResource();
    /// var ctx        = new ExtendedMorphologyContext(morphology, model);
    ///
    /// var fuzzy   = ctx.FuzzyAnalyze("kitaap", maxEditDistance: 1);
    /// var ranked  = ctx.AnalyzeWithRanking("yüzde");
    /// var forms   = ctx.Synthesize("kitap", TurkishCase.Dative);
    /// </code>
    /// </example>
    public sealed class ExtendedMorphologyContext
    {
        private readonly TurkishMorphology morphology;
        private readonly WordFrequencyModel model;

        private BkTree bkTree;
        private readonly object bkTreeLock = new object();

        /// <summary>
        /// Initialises a new context for the given <paramref name="morphology"/> instance.
        /// </summary>
        /// <param name="morphology">The <see cref="TurkishMorphology"/> instance to wrap.</param>
        /// <param name="model">
        /// Optional corpus model for confidence blending in <see cref="AnalyzeWithRanking"/>.
        /// Use <see cref="WordFrequencyModel.FromEmbeddedResource"/> to load the bundled corpus.
        /// </param>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public ExtendedMorphologyContext(TurkishMorphology morphology, WordFrequencyModel model = null)
        {
            this.morphology = morphology ?? throw new ArgumentNullException(nameof(morphology));
            this.model = model;
        }

        // --- Feature 4: Fuzzy Analysis ---

        /// <summary>
        /// Returns morphological analyses for all lexicon entries within
        /// <paramref name="maxEditDistance"/> Levenshtein edits of <paramref name="word"/>.
        /// </summary>
        /// <param name="word">The (possibly misspelled) input word.</param>
        /// <param name="maxEditDistance">
        /// Maximum allowed edit distance. Must be in [0, 3].
        /// Use 0 for exact-match only (equivalent to <c>TurkishMorphology.Analyze</c>).
        /// </param>
        /// <returns>
        /// A deduplicated flat list of <see cref="SingleAnalysis"/> results from all matching
        /// lexicon roots. Returns an empty list when no candidates are found.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxEditDistance"/> is less than 0 or greater than 3.
        /// </exception>
        /// <remarks>
        /// ZemberekDotNet addition — no equivalent in Java Zemberek.
        /// <para>
        /// <b>Performance</b>: a BK-tree is built from all lexicon root forms on the first call
        /// (O(n log n), cached) and reused on subsequent calls (O(log n) lookup average).
        /// The BK-tree is built lazily and its construction is thread-safe.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var ctx = new ExtendedMorphologyContext(TurkishMorphology.CreateWithDefaults());
        /// var results = ctx.FuzzyAnalyze("kitaap", maxEditDistance: 1);
        /// // At least one result has GetDictionaryItem().lemma == "kitap"
        /// </code>
        /// </example>
        public List<SingleAnalysis> FuzzyAnalyze(string word, int maxEditDistance = 1)
        {
            if (maxEditDistance < 0 || maxEditDistance > 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxEditDistance),
                    maxEditDistance,
                    "maxEditDistance must be between 0 and 3 (inclusive).");
            }

            BkTree tree = GetOrBuildBkTree();
            IReadOnlyList<string> candidates = tree.Search(word, maxEditDistance);

            // Deduplicate candidates so we don't analyze the same lemma twice (possible when
            // multiple BK-tree paths converge on the same item). Keep ALL analyses per candidate.
            HashSet<string> processedCandidates = new HashSet<string>(StringComparer.Ordinal);
            List<SingleAnalysis> results = new List<SingleAnalysis>();

            foreach (string candidate in candidates)
            {
                if (processedCandidates.Add(candidate))
                {
                    WordAnalysis wa = morphology.Analyze(candidate);
                    results.AddRange(wa.GetAnalysisResults());
                }
            }

            return results;
        }

        // --- Convenience wrappers (no Extd prefix — class is entirely new) ---

        /// <summary>
        /// Analyses a numeric token with an apostrophe suffix. See
        /// <see cref="TurkishMorphologyExtensions.ExtdAnalyzeNumeralWithSuffix"/>.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public List<SingleAnalysis> AnalyzeNumeralWithSuffix(string word)
            => morphology.ExtdAnalyzeNumeralWithSuffix(word);

        /// <summary>
        /// Analyses <paramref name="word"/> and returns results ranked by confidence.
        /// Uses the <see cref="WordFrequencyModel"/> supplied at construction time (if any).
        /// See <see cref="TurkishMorphologyExtensions.ExtdAnalyzeWithRanking"/>.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public List<RankedAnalysis> AnalyzeWithRanking(string word)
            => morphology.ExtdAnalyzeWithRanking(word, model);

        /// <summary>
        /// Generates inflected surface forms for <paramref name="lemma"/> in the given
        /// <paramref name="targetCase"/>. See
        /// <see cref="TurkishMorphologyExtensions.ExtdSynthesize"/>.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public List<string> Synthesize(string lemma, TurkishCase targetCase, bool plural = false)
            => morphology.ExtdSynthesize(lemma, targetCase, plural);

        // --- BK-tree lazy init ---

        private BkTree GetOrBuildBkTree()
        {
            if (bkTree != null)
            {
                return bkTree;
            }

            lock (bkTreeLock)
            {
                if (bkTree == null)
                {
                    IEnumerable<string> roots = morphology.GetLexicon()
                        .GetAllItems()
                        .Select(item => item.lemma)
                        .Where(l => !string.IsNullOrEmpty(l))
                        .Distinct(StringComparer.Ordinal);

                    bkTree = new BkTree(roots);
                }
                return bkTree;
            }
        }
    }
}
