using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
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
            string tmp = Path.Combine(Path.GetTempPath(), "foo.bar");
            File.Create(tmp);
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
        [Ignore("Not working. Work in progress.")]
        public void VocabularyGenerationTest()
        {

            /*
                NormalizationVocabularyGenerator vocabularyGenerator =
                    new NormalizationVocabularyGenerator(getDefaultMorphology());

                Vocabulary v = vocabularyGenerator.collectVocabularyHistogram(corpora, 1);
                Assert.assertEquals(5, v.incorrect.size());
                Assert.assertTrue(v.incorrect.contains("acıba"));
                Assert.assertTrue(v.incorrect.contains("ağşam"));

                NoisyWordsLexiconGenerator lexiconGenerator = new NoisyWordsLexiconGenerator();
                NormalizationVocabulary vocabulary = new NormalizationVocabulary(v, 1, 1);
                Assert.assertTrue(vocabulary.isCorrect("akşam"));
                Assert.assertTrue(vocabulary.isCorrect("kişi"));

                Assert.assertFalse(vocabulary.isCorrect("ağşam"));

                ContextualSimilarityGraph graph = lexiconGenerator.buildGraph(corpora, vocabulary, 1, 1);

                // check if contexts are correct.
                int c1Hash = NoisyWordsLexiconGenerator.hash("bu", "eve");
                IntIntMap m1 = graph.contextHashToWordCounts.get(c1Hash);
                Assert.assertNotNull(m1);
                Assert.assertEquals(2, m1.size());
                int idCorrect = vocabulary.getIndex("akşam");
                int idIncorrect = vocabulary.getIndex("ağşam");
                Assert.assertEquals(2, m1.get(idCorrect));
                Assert.assertEquals(1, m1.get(idIncorrect));

                Path tmp = Files.createTempFile("rnd", "foo");

                graph.serializeForRandomWalk(tmp);

                RandomWalker randomWalker = RandomWalker.fromGraphFile(vocabulary, tmp);
                Assert.assertEquals(
                    randomWalker.contextHashesToWords.size(),
                    graph.contextHashToWordCounts.size());

                Assert.assertEquals(
                    randomWalker.wordsToContextHashes.size(),
                    graph.wordToContexts.size());

                WalkResult result = randomWalker.walk(10, 3, 1);
                Assert.assertTrue(result.allCandidates.get("ağşam").contains("akşam"));
            */

        }
    }
}
