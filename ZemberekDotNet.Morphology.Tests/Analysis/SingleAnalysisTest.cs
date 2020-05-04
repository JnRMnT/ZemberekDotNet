using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using static ZemberekDotNet.Morphology.Analysis.SingleAnalysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class SingleAnalysisTest : AnalyzerTestBase
    {
        [TestMethod]
        public void StemEndingTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            List<SingleAnalysis> analyses = analyzer.Analyze("kitaplarda");
            Assert.AreEqual(1, analyses.Count);
            SingleAnalysis analysis = analyses[0];

            Assert.AreEqual(analysis.GetDictionaryItem(),
                analyzer.GetLexicon().GetItemById("kitap_Noun"));
            Assert.AreEqual("larda", analysis.GetEnding());
            Assert.AreEqual("kitap", analysis.GetStem());
        }

        [TestMethod]
        public void GetPosTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("görmek");
            List<SingleAnalysis> analyses = analyzer.Analyze("görmek");
            Assert.AreEqual(1, analyses.Count);
            SingleAnalysis analysis = analyses[0];

            Assert.AreEqual(analysis.GetDictionaryItem(),
                analyzer.GetLexicon().GetItemById("görmek_Verb"));
            Assert.AreEqual(PrimaryPos.Noun, analysis.GetPos());
        }

        [TestMethod]
        public void MorphemeGroupTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");

            SingleAnalysis analysis = analyzer.Analyze("kitaplarda")[0];

            MorphemeGroup group = analysis.GetGroup(0);
            Assert.AreEqual("kitaplarda", group.SurfaceForm());

            analysis = analyzer.Analyze("kitaplı")[0];
            group = analysis.GetGroup(0);
            Assert.AreEqual("kitap", group.SurfaceForm());
            group = analysis.GetGroup(1);
            Assert.AreEqual("lı", group.SurfaceForm());

            analyzer = GetAnalyzer("okumak");
            analysis = analyzer.Analyze("okutmuyor")[0];

            Assert.AreEqual(2, analysis.GetMorphemeGroupCount());
            MorphemeGroup group0 = analysis.GetGroup(0);
            Assert.AreEqual("oku", group0.SurfaceForm());
            MorphemeGroup group1 = analysis.GetGroup(1);
            Assert.AreEqual("tmuyor", group1.SurfaceForm());
        }

        static List<string> ToList(params string[] input)
        {
            return input.ToList();
        }

        [TestMethod]
        public void GetStemsTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            SingleAnalysis analysis = analyzer.Analyze("kitap")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap"), analysis.GetStems()));

            analysis = analyzer.Analyze("kitaplı")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap", "kitaplı"), analysis.GetStems()));

            analysis = analyzer.Analyze("kitaplarda")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap"), analysis.GetStems()));

            analysis = analyzer.Analyze("kitabımmış")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitab", "kitabım"), analysis.GetStems()));

            analysis = analyzer.Analyze("kitapçığa")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap", "kitapçığ"), analysis.GetStems()));

            analyzer = GetAnalyzer("okumak");
            analysis = analyzer.Analyze("okut")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut"), analysis.GetStems()));
            analysis = analyzer.Analyze("okuttur")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur"), analysis.GetStems()));
            analysis = analyzer.Analyze("okutturuluyor")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur", "okutturul"), analysis.GetStems()));
            analysis = analyzer.Analyze("okutturamıyor")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur"), analysis.GetStems()));
            analysis = analyzer.Analyze("okutturabiliyor")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur", "okutturabil"), analysis.GetStems()));
        }

        [TestMethod]
        public void GetLemmasTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            SingleAnalysis analysis = analyzer.Analyze("kitap")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap"), analysis.GetLemmas()));

            analysis = analyzer.Analyze("kitaplı")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap", "kitaplı"), analysis.GetLemmas()));

            analysis = analyzer.Analyze("kitaplarda")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap"), analysis.GetLemmas()));

            analysis = analyzer.Analyze("kitabımmış")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap", "kitabım"), analysis.GetLemmas()));

            analysis = analyzer.Analyze("kitapçığa")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("kitap", "kitapçık"), analysis.GetLemmas()));

            analyzer = GetAnalyzer("okumak");
            analysis = analyzer.Analyze("okut")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut"), analysis.GetLemmas()));
            analysis = analyzer.Analyze("okuttur")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur"), analysis.GetLemmas()));
            analysis = analyzer.Analyze("okutturuluyor")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur", "okutturul"), analysis.GetLemmas()));
            analysis = analyzer.Analyze("okutturamıyor")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur"), analysis.GetLemmas()));
            analysis = analyzer.Analyze("okutturabiliyor")[0];
            Assert.IsTrue(Enumerable.SequenceEqual(ToList("oku", "okut", "okuttur", "okutturabil"), analysis.GetLemmas()));

        }

        [TestMethod]
        public void GetLemmasAfterZeroMorphemeTest_Issue_175()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("gün");
            List<SingleAnalysis> analyses = analyzer.Analyze("günlüğüm");
            bool found = false;
            foreach (SingleAnalysis analysis in analyses)
            {
                if (analysis.FormatLong().Contains("Ness→Noun+A3sg|Zero→Verb"))
                {
                    found = true;
                    Assert.IsTrue(Enumerable.SequenceEqual(ToList("gün", "günlük"), analysis.GetLemmas()));
                }
            }
            if (!found)
            {
                Assert.Fail("Counld not found an analysis with `Ness→Noun+A3sg|Zero→Verb` in it");
            }
        }
    }
}
