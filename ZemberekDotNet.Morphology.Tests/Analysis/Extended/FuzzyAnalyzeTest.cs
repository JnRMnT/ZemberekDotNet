using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Extended;

namespace ZemberekDotNet.Morphology.Tests.Analysis.Extended
{
    [TestClass]
    public class FuzzyAnalyzeTest
    {
        private static ExtendedMorphologyContext ctx;
        private static TurkishMorphology morphology;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            morphology = TurkishMorphology.CreateWithDefaults();
            ctx = new ExtendedMorphologyContext(morphology);
        }

        [TestMethod]
        public void Typo_Kitaap_Distance1_FindsKitapAnalyses()
        {
            List<SingleAnalysis> results = ctx.FuzzyAnalyze("kitaap", maxEditDistance: 1);
            Assert.IsTrue(results.Count > 0, "Expected fuzzy results for 'kitaap' at distance 1");

            bool foundKitap = false;
            foreach (SingleAnalysis sa in results)
            {
                if (sa.GetDictionaryItem().lemma == "kitap")
                {
                    foundKitap = true;
                    break;
                }
            }
            Assert.IsTrue(foundKitap,
                "Expected at least one result whose lemma is 'kitap' for input 'kitaap'");
        }

        [TestMethod]
        public void ExactMatch_Distance0_SameAsAnalyze()
        {
            List<SingleAnalysis> exact = morphology.Analyze("araba").GetAnalysisResults();
            List<SingleAnalysis> fuzzy = ctx.FuzzyAnalyze("araba", maxEditDistance: 0);

            // At minimum the same lemmas should appear.
            Assert.IsTrue(fuzzy.Count > 0, "Distance-0 fuzzy should return results for 'araba'");
            foreach (SingleAnalysis e in exact)
            {
                bool found = false;
                foreach (SingleAnalysis f in fuzzy)
                {
                    if (f.GetDictionaryItem().lemma == e.GetDictionaryItem().lemma)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found,
                    $"Lemma '{e.GetDictionaryItem().lemma}' from Analyze is missing in FuzzyAnalyze(distance=0)");
            }
        }

        [TestMethod]
        public void UnknownWord_ReturnsEmpty()
        {
            List<SingleAnalysis> results = ctx.FuzzyAnalyze("xyzqwerty", maxEditDistance: 1);
            Assert.AreEqual(0, results.Count, "Expected empty results for nonsense input far from any lexicon entry");
        }

        [TestMethod]
        public void MaxDistanceTooHigh_ThrowsArgumentOutOfRange()
        {
            try
            {
                ctx.FuzzyAnalyze("kitap", maxEditDistance: 4);
                Assert.Fail("Expected ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException) { /* expected */ }
        }

        [TestMethod]
        public void NegativeDistance_ThrowsArgumentOutOfRange()
        {
            try
            {
                ctx.FuzzyAnalyze("kitap", maxEditDistance: -1);
                Assert.Fail("Expected ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException) { /* expected */ }
        }

        [TestMethod]
        public void Distance3_StillReturnsBoundedResults()
        {
            // Should not throw; just must return a list.
            List<SingleAnalysis> results = ctx.FuzzyAnalyze("ev", maxEditDistance: 3);
            // At minimum "ev" itself should be found.
            Assert.IsTrue(results.Count > 0, "Expected results at distance 3 for 'ev'");
        }

        [TestMethod]
        public void NullConstructor_ThrowsArgumentNull()
        {
            try
            {
                new ExtendedMorphologyContext(null);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException) { /* expected */ }
        }
    }
}
