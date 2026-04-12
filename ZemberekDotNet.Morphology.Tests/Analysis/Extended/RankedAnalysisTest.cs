using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Extended;

namespace ZemberekDotNet.Morphology.Tests.Analysis.Extended
{
    [TestClass]
    public class RankedAnalysisTest
    {
        private static TurkishMorphology morphology;
        private static WordFrequencyModel model;

        // "yüzde" is famously ambiguous: "face-LOC" (yüz+de) vs percentage adverb.
        private const string AmbiguousWord = "yüzde";

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            morphology = TurkishMorphology.CreateWithDefaults();
            model = WordFrequencyModel.FromEmbeddedResource();
        }

        [TestMethod]
        public void RankedCount_MatchesAnalysisCount()
        {
            WordAnalysis wa = morphology.Analyze(AmbiguousWord);
            List<RankedAnalysis> ranked = wa.ExtdGetRankedAnalyses();

            Assert.AreEqual(wa.AnalysisCount(), ranked.Count,
                "Ranked list must contain exactly as many entries as the underlying analysis.");
        }

        [TestMethod]
        public void AllConfidences_AreInUnitRange()
        {
            List<RankedAnalysis> ranked = morphology.Analyze(AmbiguousWord).ExtdGetRankedAnalyses();
            foreach (RankedAnalysis r in ranked)
            {
                Assert.IsTrue(r.Confidence >= 0.0 && r.Confidence <= 1.0,
                    $"Confidence {r.Confidence} is out of [0, 1]");
            }
        }

        [TestMethod]
        public void List_IsDescendingByConfidence()
        {
            List<RankedAnalysis> ranked = morphology.Analyze(AmbiguousWord).ExtdGetRankedAnalyses();
            for (int i = 1; i < ranked.Count; i++)
            {
                Assert.IsTrue(ranked[i - 1].Confidence >= ranked[i].Confidence,
                    $"Item {i - 1} confidence ({ranked[i - 1].Confidence}) < item {i} ({ranked[i].Confidence})");
            }
        }

        [TestMethod]
        public void SumOfConfidences_ApproximatesOne()
        {
            List<RankedAnalysis> ranked = morphology.Analyze(AmbiguousWord).ExtdGetRankedAnalyses();
            double sum = 0.0;
            foreach (RankedAnalysis r in ranked) sum += r.Confidence;
            Assert.AreEqual(1.0, sum, 0.001, "Softmax scores must sum to 1 (±0.001).");
        }

        [TestMethod]
        public void WithCorpusModel_ConfidencesStillValid()
        {
            List<RankedAnalysis> ranked = morphology.Analyze(AmbiguousWord).ExtdGetRankedAnalyses(model);

            Assert.IsTrue(ranked.Count > 0);
            double sum = 0.0;
            for (int i = 0; i < ranked.Count; i++)
            {
                Assert.IsTrue(ranked[i].Confidence >= 0.0 && ranked[i].Confidence <= 1.0);
                if (i > 0)
                {
                    Assert.IsTrue(ranked[i - 1].Confidence >= ranked[i].Confidence);
                }
                sum += ranked[i].Confidence;
            }
            Assert.AreEqual(1.0, sum, 0.001);
        }

        [TestMethod]
        public void SingleAnalysisWord_ConfidenceIsOne()
        {
            // A word with exactly one analysis must have confidence 1.0.
            WordAnalysis wa = morphology.Analyze("bir");
            // Softmax of a single value: e^x / e^x = 1.0
            List<RankedAnalysis> ranked = wa.ExtdGetRankedAnalyses();
            if (ranked.Count == 1)
            {
                Assert.AreEqual(1.0, ranked[0].Confidence, 0.001);
            }
            // If multiple analyses exist, just verify invariants.
            foreach (RankedAnalysis r in ranked)
            {
                Assert.IsTrue(r.Confidence >= 0.0 && r.Confidence <= 1.0);
            }
        }

        [TestMethod]
        public void ConvenienceMethod_SameResultAsExtension()
        {
            List<RankedAnalysis> viaExtension = morphology.Analyze(AmbiguousWord).ExtdGetRankedAnalyses();
            List<RankedAnalysis> viaMethod = morphology.ExtdAnalyzeWithRanking(AmbiguousWord);

            Assert.AreEqual(viaExtension.Count, viaMethod.Count);
            for (int i = 0; i < viaExtension.Count; i++)
            {
                Assert.AreEqual(viaExtension[i].Confidence, viaMethod[i].Confidence, 0.0001);
            }
        }
    }
}
