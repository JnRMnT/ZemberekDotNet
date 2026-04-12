using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Extended;

namespace ZemberekDotNet.Morphology.Tests.Analysis.Extended
{
    [TestClass]
    public class NumeralApostropheTest
    {
        private static TurkishMorphology morphology;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            morphology = TurkishMorphology.CreateWithDefaults();
        }

        [TestMethod]
        public void Analyse_90Dan_ReturnsAblative()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("90'dan");
            Assert.IsTrue(results.Count > 0, "Expected at least one result for '90\\'dan'");
            Assert.AreEqual(TurkishCase.Ablative, results[0].ExtdGetCase());
        }

        [TestMethod]
        public void Analyse_5E_ReturnsDative()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("5'e");
            Assert.IsTrue(results.Count > 0, "Expected at least one result for '5\\'e'");
            Assert.AreEqual(TurkishCase.Dative, results[0].ExtdGetCase());
        }

        [TestMethod]
        public void Analyse_3U_ReturnsAccusative()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("3'ü");
            Assert.IsTrue(results.Count > 0, "Expected at least one result for '3\\'ü'");
            Assert.AreEqual(TurkishCase.Accusative, results[0].ExtdGetCase());
        }

        [TestMethod]
        public void Analyse_100De_ReturnsLocative()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("100'de");
            Assert.IsTrue(results.Count > 0, "Expected at least one result for '100\\'de'");
            Assert.AreEqual(TurkishCase.Locative, results[0].ExtdGetCase());
        }

        [TestMethod]
        public void Analyse_2Nin_ReturnsGenitive()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("2'nin");
            Assert.IsTrue(results.Count > 0, "Expected at least one result for '2\\'nin'");
            Assert.AreEqual(TurkishCase.Genitive, results[0].ExtdGetCase());
        }

        [TestMethod]
        public void NoApostrophe_ReturnsEmpty()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("90dan");
            Assert.AreEqual(0, results.Count, "Expected empty list when no apostrophe is present");
        }

        [TestMethod]
        public void NonNumericStem_ReturnsEmpty()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("abc'de");
            Assert.AreEqual(0, results.Count, "Expected empty list when stem is not all-digit");
        }

        [TestMethod]
        public void NullInput_ReturnsEmpty()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix(null);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void ApostropheAtEnd_ReturnsEmpty()
        {
            List<SingleAnalysis> results = morphology.ExtdAnalyzeNumeralWithSuffix("90'");
            Assert.AreEqual(0, results.Count, "Expected empty list when apostrophe is at the end");
        }
    }
}
