using System.Collections.Generic;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Extended
{
    /// <summary>
    /// Extension methods that add type-safe Turkish case extraction to <see cref="SingleAnalysis"/>.
    /// </summary>
    /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
    public static class SingleAnalysisExtensions
    {
        // Maps morpheme IDs to TurkishCase values (explicit case morphemes).
        private static readonly Dictionary<string, TurkishCase> ExplicitCaseMap =
            new Dictionary<string, TurkishCase>
            {
                { "Nom", TurkishCase.Nominative },
                { "Dat", TurkishCase.Dative },
                { "Acc", TurkishCase.Accusative },
                { "Abl", TurkishCase.Ablative },
                { "Loc", TurkishCase.Locative },
                { "Ins", TurkishCase.Instrumental },
                { "Gen", TurkishCase.Genitive },
                { "Equ", TurkishCase.Equative },
            };

            // Word classes that carry open-class case inflection and can be nominative
            // via a zero suffix. Adjectives, Verbs, Adverbs, Postpositions are excluded.
            // Correctness invariant: we check the LAST morpheme group's POS, not any
            // POS morpheme anywhere in the chain. This prevents adjective-derivation
            // analyses (e.g. elma+With→Adj) from incorrectly returning Nominative —
            // the last group of that chain carries PrimaryPos.Adjective, not Noun.
            private static bool IsNominativeCapable(PrimaryPos pos)
                => pos == PrimaryPos.Noun
                || pos == PrimaryPos.Numeral
                || pos == PrimaryPos.Pronoun;

        /// <summary>
        /// Returns the primary (first) <see cref="TurkishCase"/> present in the morpheme chain,
        /// or <see cref="TurkishCase.Unknown"/> if no case morpheme is found.
        /// </summary>
        /// <remarks>
        /// ZemberekDotNet addition — no equivalent in Java Zemberek.
        /// <para>
        /// Replaces fragile string-matching patterns such as
        /// <c>analysis.ContainsMorpheme(TurkishMorphotactics.abl)</c> in downstream tools.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// using ZemberekDotNet.Morphology.Extended;
        ///
        /// var results = morphology.ExtdAnalyzeNumeralWithSuffix("90'dan");
        /// TurkishCase c = results[0].ExtdGetCase(); // TurkishCase.Ablative
        /// </code>
        /// </example>
        public static TurkishCase ExtdGetCase(this SingleAnalysis analysis)
        {
            foreach (SingleAnalysis.MorphemeData md in analysis.GetMorphemeDataList())
            {
                if (ExplicitCaseMap.TryGetValue(md.morpheme.Id, out TurkishCase tc))
                {
                    return tc;
                }
            }
                // No explicit case morpheme found. Nominative is the unmarked (zero-suffix)
                // case in Turkish. Infer it only when the LAST group's word class can bear
                // case inflection — this correctly rejects adjective-derivation analyses.
                return IsNominativeCapable(analysis.GetLastGroup().GetPos())
                    ? TurkishCase.Nominative
                    : TurkishCase.Unknown;
        }

        /// <summary>
        /// Returns all <see cref="TurkishCase"/> values present in the morpheme chain,
        /// in chain order. Most analyses contain exactly one case morpheme.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public static IReadOnlyList<TurkishCase> ExtdGetPossibleCases(this SingleAnalysis analysis)
        {
            List<TurkishCase> result = new List<TurkishCase>();
            foreach (SingleAnalysis.MorphemeData md in analysis.GetMorphemeDataList())
            {
                if (ExplicitCaseMap.TryGetValue(md.morpheme.Id, out TurkishCase tc))
                {
                    result.Add(tc);
                }
            }
                // Same last-group invariant as ExtdGetCase.
                if (result.Count == 0 && IsNominativeCapable(analysis.GetLastGroup().GetPos()))
            {
                result.Add(TurkishCase.Nominative);
            }
            return result;
        }
    }
}
