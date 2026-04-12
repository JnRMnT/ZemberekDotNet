using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Generator;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Extended
{
    /// <summary>
    /// Extension methods that add new analysis and synthesis capabilities to
    /// <see cref="TurkishMorphology"/>.
    /// </summary>
    /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
    public static class TurkishMorphologyExtensions
    {
        // --- Feature 1: Numeric Suffix Analysis ---

        /// <summary>
        /// Analyses a numeric token that carries an apostrophe suffix, e.g. <c>"90'dan"</c>,
        /// and returns the <see cref="PrimaryPos.Numeral"/> analyses for it.
        /// </summary>
        /// <remarks>
        /// ZemberekDotNet addition — no equivalent in Java Zemberek.
        /// <para>
        /// The built-in <c>AnalyzeWordsWithApostrophe</c> strips the apostrophe and then
        /// hard-filters results to <see cref="PrimaryPos.Noun"/>, silently discarding numerals.
        /// This method performs the same strip-and-analyse step but keeps only
        /// <see cref="PrimaryPos.Numeral"/> results, which are produced by the
        /// <c>UnidentifiedTokenAnalyzer</c> pipeline.
        /// </para>
        /// <para>
        /// Returns an empty list (never throws) when:
        /// <list type="bullet">
        ///   <item><description>no apostrophe is present,</description></item>
        ///   <item><description>the apostrophe is at position 0 or the last character,</description></item>
        ///   <item><description>the stem portion (before the apostrophe) contains non-digit characters.</description></item>
        /// </list>
        /// </para>
        /// <para>Performance note: calls <c>TurkishMorphology.Analyze</c> once per invocation.
        /// The result is eligible for the internal <see cref="TurkishMorphology"/> cache.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// using ZemberekDotNet.Morphology.Extended;
        ///
        /// var morphology = TurkishMorphology.CreateWithDefaults();
        /// var results = morphology.ExtdAnalyzeNumeralWithSuffix("90'dan");
        /// // results[0].ExtdGetCase() == TurkishCase.Ablative
        /// </code>
        /// </example>
        public static List<SingleAnalysis> ExtdAnalyzeNumeralWithSuffix(
            this TurkishMorphology morphology, string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return new List<SingleAnalysis>();
            }

            int index = word.IndexOf('\'');
            if (index <= 0 || index == word.Length - 1)
            {
                return new List<SingleAnalysis>();
            }

            // Validate that the stem (before the apostrophe) is all-digit.
            for (int i = 0; i < index; i++)
            {
                if (!char.IsDigit(word[i]))
                {
                    return new List<SingleAnalysis>();
                }
            }

            string withoutQuote = word.Replace("'", "");

            // Full pipeline (incl. UnidentifiedTokenAnalyzer) — handles numeric tokens.
            WordAnalysis result = morphology.Analyze(withoutQuote);
            return result.GetAnalysisResults()
                .Where(a => a.GetDictionaryItem().primaryPos == PrimaryPos.Numeral)
                .ToList();
        }

        // --- Feature 3: Ambiguity Ranking (convenience wrapper) ---

        /// <summary>
        /// Analyses <paramref name="word"/> and returns results ranked by confidence,
        /// descending.  Scores are softmax-normalised and sum to 1.
        /// </summary>
        /// <param name="morphology">The <see cref="TurkishMorphology"/> instance.</param>
        /// <param name="word">The word to analyse.</param>
        /// <param name="model">
        /// Optional <see cref="WordFrequencyModel"/> for corpus-blended scoring.
        /// When <c>null</c>, a heuristic based on morpheme-chain complexity is used.
        /// </param>
        /// <remarks>
        /// ZemberekDotNet addition — no equivalent in Java Zemberek.
        /// <para>
        /// For sentence-level disambiguation, prefer
        /// <c>TurkishMorphology.AnalyzeAndDisambiguate(sentence)</c> followed by
        /// <c>SentenceWordAnalysis.GetBestAnalysis()</c>, which uses a trained perceptron model.
        /// This method is intended for single-word / IDE-loop scenarios where sentence context
        /// is unavailable.
        /// </para>
        /// </remarks>
        public static List<RankedAnalysis> ExtdAnalyzeWithRanking(
            this TurkishMorphology morphology,
            string word,
            WordFrequencyModel model = null)
        {
            return morphology.Analyze(word).ExtdGetRankedAnalyses(model);
        }

        // --- Feature 5: Morphology Synthesis ---

        /// <summary>
        /// Generates inflected surface forms for <paramref name="lemma"/> in the given
        /// <paramref name="targetCase"/>.
        /// </summary>
        /// <param name="morphology">The <see cref="TurkishMorphology"/> instance.</param>
        /// <param name="lemma">The dictionary lemma, e.g. <c>"kitap"</c>.</param>
        /// <param name="targetCase">The desired Turkish grammatical case.</param>
        /// <param name="plural">
        /// When <c>true</c>, the plural (A3pl) agreement morpheme is used; singular (A3sg) otherwise.
        /// </param>
        /// <returns>
        /// A deduplicated list of surface forms produced by <see cref="WordGenerator"/>.
        /// Returns an empty list when the lemma is not found or no forms can be generated.
        /// Never throws for unknown lemmas.
        /// </returns>
        /// <remarks>
        /// ZemberekDotNet addition — no equivalent in Java Zemberek.
        /// <para>
        /// Targets <see cref="PrimaryPos.Noun"/> and <see cref="PrimaryPos.Numeral"/> items only.
        /// Possessive suffixes are not applied; add them as a follow-up morpheme chain if needed.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// using ZemberekDotNet.Morphology.Extended;
        ///
        /// var morphology = TurkishMorphology.CreateWithDefaults();
        /// var forms = morphology.ExtdSynthesize("kitap", TurkishCase.Dative);
        /// // forms contains "kitaba"
        /// var plural = morphology.ExtdSynthesize("kitap", TurkishCase.Nominative, plural: true);
        /// // plural contains "kitaplar"
        /// </code>
        /// </example>
        public static List<string> ExtdSynthesize(
            this TurkishMorphology morphology,
            string lemma,
            TurkishCase targetCase,
            bool plural = false)
        {
            if (string.IsNullOrEmpty(lemma))
            {
                return new List<string>();
            }

            Morpheme caseMorpheme = CaseToMorpheme(targetCase);
            if (caseMorpheme == null)
            {
                return new List<string>();
            }

            Morpheme numberMorpheme = plural ? TurkishMorphotactics.a3pl : TurkishMorphotactics.a3sg;

            List<string> results = new List<string>();
            foreach (var item in morphology.GetLexicon().GetMatchingItems(lemma))
            {
                if (item.primaryPos != PrimaryPos.Noun && item.primaryPos != PrimaryPos.Numeral)
                {
                    continue;
                }

                List<WordGenerator.Result> generated =
                    morphology.GetWordGenerator().Generate(item, numberMorpheme, caseMorpheme);

                foreach (WordGenerator.Result r in generated)
                {
                    if (!string.IsNullOrEmpty(r.surface) && !results.Contains(r.surface))
                    {
                        results.Add(r.surface);
                    }
                }
            }

            return results;
        }

        // --- Helpers ---

        private static Morpheme CaseToMorpheme(TurkishCase tc)
        {
            switch (tc)
            {
                case TurkishCase.Nominative:    return TurkishMorphotactics.nom;
                case TurkishCase.Dative:        return TurkishMorphotactics.dat;
                case TurkishCase.Accusative:    return TurkishMorphotactics.acc;
                case TurkishCase.Ablative:      return TurkishMorphotactics.abl;
                case TurkishCase.Locative:      return TurkishMorphotactics.loc;
                case TurkishCase.Instrumental:  return TurkishMorphotactics.ins;
                case TurkishCase.Genitive:      return TurkishMorphotactics.gen;
                case TurkishCase.Equative:      return TurkishMorphotactics.equ;
                default:                        return null;
            }
        }
    }
}
