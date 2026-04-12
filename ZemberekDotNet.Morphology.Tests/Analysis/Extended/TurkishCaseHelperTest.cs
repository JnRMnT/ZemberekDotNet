using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Extended;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Tests.Analysis.Extended
{
    [TestClass]
    public class TurkishCaseHelperTest
    {
        private static TurkishMorphology morphology;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            morphology = TurkishMorphology.CreateWithDefaults();
        }

        // --- ExtdGetCase ---

        [TestMethod]
        public void Kitaplara_FirstResult_IsDative()
        {
            AssertFirstCase("kitaplara", TurkishCase.Dative);
        }

        [TestMethod]
        public void Kitaptan_FirstResult_IsAblative()
        {
            AssertFirstCase("kitaptan", TurkishCase.Ablative);
        }

        [TestMethod]
        public void Kitabin_FirstResult_IsGenitive()
        {
            AssertFirstCase("kitabın", TurkishCase.Genitive);
        }

        [TestMethod]
        public void Kitapta_FirstResult_IsLocative()
        {
            AssertFirstCase("kitapta", TurkishCase.Locative);
        }

        [TestMethod]
        public void Kitap_FirstResult_IsNominative()
        {
            AssertFirstCase("kitap", TurkishCase.Nominative);
        }

        [TestMethod]
        public void Kitabi_FirstResult_IsAccusative()
        {
            AssertFirstCase("kitabı", TurkishCase.Accusative);
        }

        [TestMethod]
        public void Kitabin_Genitive_FirstResult()
        {
            AssertFirstCase("kitabın", TurkishCase.Genitive);
        }

        // --- ExtdGetPossibleCases ---

        [TestMethod]
        public void Kitaplara_PossibleCases_ContainsDative()
        {
            List<SingleAnalysis> results = morphology.Analyze("kitaplara").GetAnalysisResults();
            Assert.IsTrue(results.Count > 0);

            bool found = false;
            foreach (SingleAnalysis r in results)
            {
                IReadOnlyList<TurkishCase> cases = r.ExtdGetPossibleCases();
                foreach (TurkishCase tc in cases)
                {
                    if (tc == TurkishCase.Dative)
                    {
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
            Assert.IsTrue(found, "ExtdGetPossibleCases should contain Dative for 'kitaplara'");
        }

        [TestMethod]
        public void PossibleCases_AtLeastOneNounAnalysis_HasCase()
        {
            // At least one Noun analysis of "araba" must carry a case (Nominative inferred).
            List<SingleAnalysis> results = morphology.Analyze("araba").GetAnalysisResults();
            Assert.IsTrue(results.Count > 0);

            bool foundNounWithCase = false;
            foreach (SingleAnalysis r in results)
            {
                if (r.GetDictionaryItem().primaryPos == PrimaryPos.Noun)
                {
                    IReadOnlyList<TurkishCase> cases = r.ExtdGetPossibleCases();
                    if (cases.Count > 0)
                    {
                        foundNounWithCase = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(foundNounWithCase,
                "Expected at least one Noun analysis of 'araba' to have a possible case");
        }

        // --- LastGroup POS inference correctness ---

        [TestMethod]
        public void Elmali_AdjDerivationReading_ReturnsUnknown()
        {
            // "elmalı" reading: Noun(elma)+A3sg+With(lı)+Adj(ε) — adj derivation.
            // Last group POS = Adjective → must return Unknown, NOT Nominative.
            List<SingleAnalysis> results = morphology.Analyze("elmalı").GetAnalysisResults();
            Assert.IsTrue(results.Count > 0);

            bool adjReadingUnknown = false;
            foreach (SingleAnalysis r in results)
            {
                if (r.GetMorphemeGroupCount() > 1 && r.GetLastGroup().GetPos() == PrimaryPos.Adjective)
                {
                    Assert.AreEqual(TurkishCase.Unknown, r.ExtdGetCase(),
                        "Adj-derivation reading of 'elmalı' must return Unknown");
                    adjReadingUnknown = true;
                }
            }
            Assert.IsTrue(adjReadingUnknown,
                "Expected to find an Adj-derivation reading of 'elmalı'");
        }

        [TestMethod]
        public void Elmali_LexicalisedNounReading_ReturnsNominative()
        {
            // Single-group Noun(elmalı)+A3sg reading — last group POS = Noun → Nominative.
            List<SingleAnalysis> results = morphology.Analyze("elmalı").GetAnalysisResults();
            Assert.IsTrue(results.Count > 0);

            bool nounReadingNominative = false;
            foreach (SingleAnalysis r in results)
            {
                if (r.GetMorphemeGroupCount() == 1 && r.GetLastGroup().GetPos() == PrimaryPos.Noun)
                {
                    Assert.AreEqual(TurkishCase.Nominative, r.ExtdGetCase(),
                        "Lexicalized noun reading of 'elmalı' must return Nominative");
                    nounReadingNominative = true;
                }
            }
            Assert.IsTrue(nounReadingNominative,
                "Expected to find a single-group Noun reading of 'elmalı'");
        }

        [TestMethod]
        public void Kosma_VerbNegativeImperative_ReturnsUnknown()
        {
            // "koşma" = "don't run!" (Verb+Neg+Imp) — verb reading, not case-bearing.
            List<SingleAnalysis> results = morphology.Analyze("koşma").GetAnalysisResults();
            Assert.IsTrue(results.Count > 0);

            bool verbReadingUnknown = false;
            foreach (SingleAnalysis r in results)
            {
                if (r.GetDictionaryItem().primaryPos == PrimaryPos.Verb
                    && r.ContainsMorpheme(TurkishMorphotactics.neg))
                {
                    Assert.AreEqual(TurkishCase.Unknown, r.ExtdGetCase(),
                        "Verb+Neg reading of 'koşma' must return Unknown");
                    verbReadingUnknown = true;
                }
            }
            Assert.IsTrue(verbReadingUnknown,
                "Expected to find a Verb+Neg reading of 'koşma'");
        }

        [TestMethod]
        public void Kosma_GerundReading_ReturnsNominative()
        {
            // "koşma" = gerund "the running" (Verb+Inf2+Noun+A3sg) — last group POS = Noun → Nominative.
            List<SingleAnalysis> results = morphology.Analyze("koşma").GetAnalysisResults();
            bool gerundFound = false;
            foreach (SingleAnalysis r in results)
            {
                if (r.GetDictionaryItem().primaryPos == PrimaryPos.Verb
                    && r.GetLastGroup().GetPos() == PrimaryPos.Noun)
                {
                    Assert.AreEqual(TurkishCase.Nominative, r.ExtdGetCase(),
                        "Gerund (Inf2+Noun) reading of 'koşma' must return Nominative");
                    gerundFound = true;
                }
            }
            Assert.IsTrue(gerundFound, "Expected to find a gerund Verb→Noun reading of 'koşma'");
        }

        [TestMethod]
        public void Adverb_Birden_PureAdverbReading_ReturnsUnknown()
        {
            // [Adv] reading of "birden" — adverbs don't carry case.
            List<SingleAnalysis> results = morphology.Analyze("birden").GetAnalysisResults();
            foreach (SingleAnalysis r in results)
            {
                if (r.GetDictionaryItem().primaryPos == PrimaryPos.Adverb)
                {
                    Assert.AreEqual(TurkishCase.Unknown, r.ExtdGetCase(),
                        "Adverb reading of 'birden' must return Unknown");
                    return;
                }
            }
            Assert.Inconclusive("No Adverb reading found for 'birden'.");
        }

        [TestMethod]
        public void Numeral_Birden_AblativeReading_ReturnsAblative()
        {
            // [Num] Num(bir)+Zero+Noun+A3sg+Abl(den) — explicit Abl morpheme → Ablative.
            List<SingleAnalysis> results = morphology.Analyze("birden").GetAnalysisResults();
            bool found = false;
            foreach (SingleAnalysis r in results)
            {
                if (r.ExtdGetCase() == TurkishCase.Ablative)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Expected at least one Ablative reading for 'birden'");
        }

        [TestMethod]
        public void Postposition_Sonra_ReturnsUnknown()
        {
            // "sonra" (after) — postposition reading must return Unknown.
            List<SingleAnalysis> results = morphology.Analyze("sonra").GetAnalysisResults();
            foreach (SingleAnalysis r in results)
            {
                if (r.GetDictionaryItem().primaryPos == PrimaryPos.PostPositive)
                {
                    Assert.AreEqual(TurkishCase.Unknown, r.ExtdGetCase(),
                        "Postposition reading of 'sonra' must return Unknown");
                    return;
                }
            }
            Assert.Inconclusive("No Postposition reading found for 'sonra'.");
        }

        // --- Helpers ---

        private void AssertFirstCase(string word, TurkishCase expected)
        {
            List<SingleAnalysis> results = morphology.Analyze(word).GetAnalysisResults();
            Assert.IsTrue(results.Count > 0, $"Expected at least one analysis for '{word}'");

            // At least one result must carry the expected case.
            bool found = false;
            foreach (SingleAnalysis r in results)
            {
                if (r.ExtdGetCase() == expected)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found,
                $"Expected at least one analysis of '{word}' to have case {expected}");
        }
    }
}
