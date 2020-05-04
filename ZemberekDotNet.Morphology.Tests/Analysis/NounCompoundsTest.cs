using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class NounCompoundsTest : AnalyzerTestBase
    {
        [TestMethod]
        public void Incorrect1()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");
            ExpectFail(analyzer, "zeytinyağ", "zeytinyağıya", "zeytinyağılar", "zeytinyağlar"
                , "zeytinyağya", "zeytinyağna", "zeytinyağda", "zeytinyağdan");
        }

        [TestMethod]
        public void Incorrect2()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "bal",
                "kabak",
                "balkabağı [A:CompoundP3sg; Roots:bal-kabak]");
            ExpectFail(analyzer, "balkabak", "balkabağa", "balkabakta", "balkabaktan");
        }

        [TestMethod]
        public void ExpectsResult()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");
            ExpectSuccess(analyzer, "zeytinyağı", "zeytinyağına", "zeytinyağım", "zeytinyağlarıma");
        }

        [TestMethod]
        public void ExpectsResult2()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "bal",
                "kabak",
                "balkabağı [A:CompoundP3sg; Roots:bal-kabak]");
            ExpectSuccess(analyzer, "balkabağı", "balkabakları", "balkabağına");
        }

        [TestMethod]
        public void MustHaveTwoResults()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            ExpectSuccess(analyzer, 2, "zeytinyağı");
        }

        [TestMethod]
        public void ResultDictionaryItemCannotBeDummy()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");
            List<SingleAnalysis> analyses = analyzer.Analyze("zeytinyağlı");
            Assert.AreEqual(1, analyses.Count);
            SingleAnalysis a = analyses[0];
            Assert.IsTrue(!a.IsUnknown());
            Assert.AreEqual("zeytinyağı", a.GetDictionaryItem().lemma);
            Assert.IsFalse(a.GetDictionaryItem().HasAttribute(RootAttribute.Dummy));
        }
    }
}
