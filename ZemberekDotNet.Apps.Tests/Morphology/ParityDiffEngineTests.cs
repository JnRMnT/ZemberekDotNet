using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Apps.Morphology.Parity;

namespace ZemberekDotNet.Apps.Tests.Morphology
{
    [TestClass]
    public class ParityDiffEngineTests
    {
        private static readonly string FixtureDir =
            Path.Combine("Morphology", "Resources");

        private static readonly string FixtureTsv =
            Path.Combine(FixtureDir, "fixture_parity_java.tsv");

        // ── helpers ─────────────────────────────────────────────────────────

        private static Dictionary<int, List<JavaWordAnalysis>> Java(
            params (int si, string word, string best, int count)[] rows)
        {
            var d = new Dictionary<int, List<JavaWordAnalysis>>();
            foreach (var (si, word, best, count) in rows)
            {
                if (!d.TryGetValue(si, out var list)) d[si] = list = new();
                list.Add(new JavaWordAnalysis
                {
                    SentenceIndex = si, Word = word, BestAnalysis = best, AnalysisCount = count
                });
            }
            return d;
        }

        private static Dictionary<int, List<DotNetWordAnalysis>> DotNet(
            params (int si, string word, string best, int count)[] rows)
        {
            var d = new Dictionary<int, List<DotNetWordAnalysis>>();
            foreach (var (si, word, best, count) in rows)
            {
                if (!d.TryGetValue(si, out var list)) d[si] = list = new();
                list.Add(new DotNetWordAnalysis
                {
                    SentenceIndex = si, Word = word, BestAnalysis = best, AnalysisCount = count
                });
            }
            return d;
        }

        // ── JavaOutputParser ─────────────────────────────────────────────────

        [TestMethod]
        public void JavaOutputParser_ParsesAllRows()
        {
            var parsed = JavaOutputParser.Parse(FixtureTsv);

            // Sentences 0-4 should all be present
            Assert.IsTrue(parsed.ContainsKey(0), "Sentence 0 missing");
            Assert.IsTrue(parsed.ContainsKey(1), "Sentence 1 missing");
            Assert.IsTrue(parsed.ContainsKey(2), "Sentence 2 missing");
            Assert.IsTrue(parsed.ContainsKey(3), "Sentence 3 missing");
            Assert.IsTrue(parsed.ContainsKey(4), "Sentence 4 missing");

            // Sentence 0 should have 2 words
            Assert.AreEqual(2, parsed[0].Count, "Sentence 0 word count");
            Assert.AreEqual("kitap", parsed[0][0].Word);
            Assert.AreEqual("kitap:Noun+A3sg+Nom", parsed[0][0].BestAnalysis);
            Assert.AreEqual(1, parsed[0][0].AnalysisCount);

            Assert.AreEqual("güzel", parsed[0][1].Word);
            Assert.AreEqual(2, parsed[0][1].AnalysisCount);
        }

        [TestMethod]
        public void JavaOutputParser_IgnoresCommentAndBlankLines()
        {
            var parsed = JavaOutputParser.Parse(FixtureTsv);

            // No sentence index that corresponds to a comment line should exist
            Assert.IsFalse(parsed.ContainsKey(-1));
        }

        // ── ParityDiffEngine ─────────────────────────────────────────────────

        [TestMethod]
        public void DiffEngine_FullMatch_ReportsZeroMismatches()
        {
            var sentences = new[] { "kitap güzel" };
            var java = Java((0, "kitap", "kitap:Noun+A3sg+Nom", 1),
                            (0, "güzel", "güzel:Adj", 2));
            var dotNet = DotNet((0, "kitap", "kitap:Noun+A3sg+Nom", 1),
                                (0, "güzel", "güzel:Adj", 2));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.AreEqual(2, report.TotalWords);
            Assert.AreEqual(2, report.MatchingWords);
            Assert.AreEqual(0, report.MismatchingWords);
            Assert.AreEqual(0, report.MismatchCounts.Count);
            Assert.AreEqual(100.0, report.MatchRatePct);
        }

        [TestMethod]
        public void DiffEngine_LexiconGap_JavaUnknown()
        {
            var sentences = new[] { "blogging" };
            var java = Java((0, "blogging", "?", 0));
            var dotNet = DotNet((0, "blogging", "blog:Noun+A3sg+Nom", 1));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.AreEqual(1, report.MismatchingWords);
            Assert.IsTrue(report.MismatchCounts.ContainsKey(MismatchCategory.LexiconGap));
            Assert.AreEqual(1, report.MismatchCounts[MismatchCategory.LexiconGap]);
        }

        [TestMethod]
        public void DiffEngine_LexiconGap_DotNetUnknown()
        {
            var sentences = new[] { "blogging" };
            var java = Java((0, "blogging", "blog:Noun+A3sg+Nom", 1));
            var dotNet = DotNet((0, "blogging", "?", 0));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.AreEqual(1, report.MismatchCounts[MismatchCategory.LexiconGap]);
        }

        [TestMethod]
        public void DiffEngine_BestAnalysisDiff()
        {
            var sentences = new[] { "geldi" };
            var java = Java((0, "geldi", "gel:Verb+Past+A3sg", 1));
            var dotNet = DotNet((0, "geldi", "gel:Verb+NarrPast+A3sg", 1));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.AreEqual(1, report.MismatchCounts[MismatchCategory.BestAnalysisDiff]);
        }

        [TestMethod]
        public void DiffEngine_AnalysisCountDiff_BestMatches()
        {
            var sentences = new[] { "araba" };
            var java = Java((0, "araba", "araba:Noun+A3sg+Nom", 1));
            var dotNet = DotNet((0, "araba", "araba:Noun+A3sg+Nom", 3));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.AreEqual(1, report.MismatchCounts[MismatchCategory.AnalysisCountDiff]);
        }

        [TestMethod]
        public void DiffEngine_BothUnknown()
        {
            var sentences = new[] { "xyzqrst" };
            var java = Java((0, "xyzqrst", "?", 0));
            var dotNet = DotNet((0, "xyzqrst", "?", 0));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            // BothUnknown is still a mismatch category (tracked separately)
            Assert.AreEqual(1, report.MismatchCounts[MismatchCategory.BothUnknown]);
            Assert.AreEqual(0, report.MatchingWords);
        }

        [TestMethod]
        public void DiffEngine_TokenizationDiff()
        {
            // Java sees 3 words, .NET sees 2 words for the same sentence
            var sentences = new[] { "Ankara'ya gitti" };
            var java = Java((0, "Ankara", "Ankara:Noun+Prop+A3sg+Nom", 1),
                            (0, "'ya", "ya:PostP", 1),
                            (0, "gitti", "git:Verb+Past+A3sg", 1));
            var dotNet = DotNet((0, "Ankara'ya", "Ankara:Noun+Prop+A3sg+Dat", 1),
                                (0, "gitti", "git:Verb+Past+A3sg", 1));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.IsTrue(report.MismatchCounts.ContainsKey(MismatchCategory.TokenizationDiff));
            Assert.AreEqual(0, report.MatchingWords);
        }

        [TestMethod]
        public void DiffEngine_EmptySentence_NoWords()
        {
            var sentences = new[] { "" };
            var java = new Dictionary<int, List<JavaWordAnalysis>>();
            var dotNet = new Dictionary<int, List<DotNetWordAnalysis>>();

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.AreEqual(0, report.TotalWords);
            Assert.AreEqual(0, report.MismatchingWords);
        }

        [TestMethod]
        public void DiffEngine_MultiSentence_AggregatesCorrectly()
        {
            var sentences = new[] { "kitap", "blogging", "araba" };

            var java = Java(
                (0, "kitap", "kitap:Noun+A3sg+Nom", 1),   // match
                (1, "blogging", "?", 0),                   // LexiconGap
                (2, "araba", "araba:Noun+A3sg+Nom", 1));   // match

            var dotNet = DotNet(
                (0, "kitap", "kitap:Noun+A3sg+Nom", 1),
                (1, "blogging", "blog:Noun+A3sg+Nom", 1),
                (2, "araba", "araba:Noun+A3sg+Nom", 1));

            ParityReport report = ParityDiffEngine.Diff(sentences, java, dotNet, "test");

            Assert.AreEqual(3, report.TotalWords);
            Assert.AreEqual(2, report.MatchingWords);
            Assert.AreEqual(1, report.MismatchingWords);
            Assert.AreEqual(1, report.MismatchCounts[MismatchCategory.LexiconGap]);
        }

        [TestMethod]
        public void DiffEngine_FixtureFile_ParsesAndDiffsWithoutException()
        {
            // Round-trip: parse the fixture file, build matching .NET side, verify 0 mismatches
            // for the fully matched sentence (index 0).
            var javaAnalyses = JavaOutputParser.Parse(FixtureTsv);

            // Build a synthetic .NET side that exactly matches sentence 0 of the fixture
            var dotNet = DotNet(
                (0, "kitap", "kitap:Noun+A3sg+Nom", 1),
                (0, "güzel", "güzel:Adj", 2));

            var sentences = new[] { "kitap güzel" };
            var javaSubset = new Dictionary<int, List<JavaWordAnalysis>>
            {
                [0] = javaAnalyses[0]
            };

            ParityReport report = ParityDiffEngine.Diff(sentences, javaSubset, dotNet, FixtureTsv);

            Assert.AreEqual(2, report.MatchingWords);
            Assert.AreEqual(0, report.MismatchingWords);
        }
    }
}
