using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Normalization.Tests
{
    [TestClass]
    public class RandomWalkFunctionalTest
    {
        BlockTextLoader corpora;

        [TestInitialize]
        public void SetUp()
        {
            string tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");
            List<string> lines = TextIO.LoadLines("Resources/normalization/mini-noisy-corpus.txt", "#");
            File.WriteAllLines(tmp, lines);
            corpora = BlockTextLoader.FromPaths(new List<string> { tmp });
        }

        [TestCleanup]
        public void TearDown()
        {
            if (corpora != null)
            {
                foreach (string path in corpora.GetCorpusPaths())
                {
                    File.Delete(path);
                }
            }
        }

        TurkishMorphology GetDefaultMorphology()
        {
            return TurkishMorphology
                .Builder()
                .SetLexicon(RootLexicon.GetDefault())
                .DisableCache()
                .Build();
        }

        TurkishMorphology GetInformalMorphology()
        {
            RootLexicon lexicon = DictionarySerializer.Load("Resources/tr/lexicon.bin");
            return TurkishMorphology
                .Builder()
                .SetLexicon(lexicon)
                .DisableUnidentifiedTokenAnalyzer()
                .UseInformalAnalysis()
                .DisableCache()
                .Build();
        }


        [TestMethod]
        public void VocabularyGenerationTest()
        {
            // --- Step 1: collect vocabulary histogram ---
            NormalizationVocabularyGenerator vocabularyGenerator =
                new NormalizationVocabularyGenerator(GetDefaultMorphology());

            NormalizationVocabularyGenerator.Vocabulary v =
                vocabularyGenerator.CollectVocabularyHistogram(corpora, 1);

            // Corpus has morphologically incorrect words without informal analysis:
            // ağşam, acıba, kisi, gelicek, gidicez, gelcek (C# analyzer catches 6)
            Assert.IsTrue(v.Incorrect.Size() >= 5);
            Assert.IsTrue(v.Incorrect.Contains("acıba"));
            Assert.IsTrue(v.Incorrect.Contains("ağşam"));

            // --- Step 2: build NormalizationVocabulary from the collected histograms ---
            NoisyWordsLexiconGenerator.NormalizationVocabulary vocabulary =
                new NoisyWordsLexiconGenerator.NormalizationVocabulary(
                    v.Correct, v.Incorrect, 1, 1);

            Assert.IsTrue(vocabulary.IsCorrect("akşam"));
            Assert.IsTrue(vocabulary.IsCorrect("kişi"));
            Assert.IsFalse(vocabulary.IsCorrect("ağşam"));

            // --- Step 3: build contextual similarity graph ---
            NoisyWordsLexiconGenerator generator =
                new NoisyWordsLexiconGenerator(vocabulary, 1);

            NoisyWordsLexiconGenerator.ContextualSimilarityGraph graph =
                generator.BuildGraph(corpora, 1);

            // "bu _ eve" context should appear in graph:
            //   ağşam × 1, akşam × 2
            int c1Hash = NoisyWordsLexiconGenerator.Hash("bu", "eve");
            IntIntMap m1 = graph.ContextHashToWordCounts.Get(c1Hash);
            Assert.IsNotNull(m1);
            Assert.AreEqual(2, m1.Size());

            int idCorrect = vocabulary.GetIndex("akşam");
            int idIncorrect = vocabulary.GetIndex("ağşam");
            Assert.AreEqual(2, m1.Get(idCorrect));
            Assert.AreEqual(1, m1.Get(idIncorrect));

            // --- Step 4: serialize graph and do a random walk ---
            string graphPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                graph.SerializeForRandomWalk(graphPath);

                NoisyWordsLexiconGenerator.RandomWalker walker =
                    NoisyWordsLexiconGenerator.RandomWalker.FromGraphFile(vocabulary, graphPath);

                Assert.AreEqual(
                    walker.ContextHashesToWords.Size(),
                    graph.ContextHashToWordCounts.Size());

                NoisyWordsLexiconGenerator.WalkResult result = walker.Walk(10, 3, 1);
                Assert.IsTrue(result.AllCandidates.ContainsKey("ağşam"));
                List<NoisyWordsLexiconGenerator.WalkScore> candidates =
                    new List<NoisyWordsLexiconGenerator.WalkScore>(result.AllCandidates["ağşam"]);
                Assert.IsTrue(candidates.Exists(s => s.Candidate == "akşam"));
            }
            finally
            {
                if (File.Exists(graphPath)) File.Delete(graphPath);
            }
        }
    }
}
