using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.NER;

namespace ZemberekDotNet.NER.Tests
{
    [TestClass]
    public class PerceptronNerTest
    {
        [TestMethod]
        public void NerTokenCreatesCorrectly()
        {
            NerToken token = new NerToken(0, "Ankara", "LOCATION", NePosition.UNIT);
            Assert.AreEqual(0, token.GetIndex());
            Assert.AreEqual("Ankara", token.GetWord());
            Assert.AreEqual("LOCATION", token.GetTokenType());
            Assert.AreEqual(NePosition.UNIT, token.GetPosition());
        }

        [TestMethod]
        public void NerTokenWithNormalizedCreatesCorrectly()
        {
            NerToken token = new NerToken(1, "Ankara'ya", "ankara", "LOCATION", NePosition.UNIT);
            Assert.AreEqual("Ankara'ya", token.GetWord());
            Assert.AreEqual("ankara", token.GetNormalized());
        }

        [TestMethod]
        public void NerTokenOutsideHasOutsidePosition()
        {
            NerToken token = new NerToken(0, "bir", "OUT", NePosition.OUTSIDE);
            Assert.AreEqual(NePosition.OUTSIDE, token.GetPosition());
        }

        [TestMethod]
        public void NamedEntityReturnsWords()
        {
            List<NerToken> tokens = new List<NerToken>
            {
                new NerToken(0, "Ali", "PERSON", NePosition.BEGIN),
                new NerToken(1, "Veli", "PERSON", NePosition.LAST),
            };
            NamedEntity entity = new NamedEntity("PERSON", tokens);
            List<string> words = entity.GetWords();
            Assert.AreEqual(2, words.Count);
            Assert.AreEqual("Ali", words[0]);
            Assert.AreEqual("Veli", words[1]);
        }

        [TestMethod]
        public void NamedEntityContentJoinsWithSpace()
        {
            List<NerToken> tokens = new List<NerToken>
            {
                new NerToken(0, "Ali", "PERSON", NePosition.BEGIN),
                new NerToken(1, "Veli", "PERSON", NePosition.LAST),
            };
            NamedEntity entity = new NamedEntity("PERSON", tokens);
            Assert.AreEqual("Ali Veli", entity.Content());
        }

        [TestMethod]
        public void NerSentenceGetNamedEntitiesExcludesOutside()
        {
            List<NerToken> tokens = new List<NerToken>
            {
                new NerToken(0, "Ankara", "LOCATION", NePosition.UNIT),
                new NerToken(1, "güzel", "OUT", NePosition.OUTSIDE),
            };
            NerSentence sentence = new NerSentence("Ankara güzel", tokens);
            List<NamedEntity> entities = sentence.GetNamedEntities();
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("LOCATION", entities[0].GetTokens()[0].GetTokenType());
        }

        [TestMethod]
        public void FindNamedEntitiesInSentenceRequiresModel()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(tempFile, new[]
                {
                    "<START:LOCATION> Ankara <END> güzel bir şehir .",
                    "<START:PERSON> Ali <END> gitti .",
                    "<START:LOCATION> İstanbul <END> büyük .",
                    "<START:PERSON> Mehmet <END> çalışır .",
                    "<START:LOCATION> Ankara <END> bir şehir .",
                    "<START:PERSON> Ali <END> çalışır .",
                });

                NerDataSet trainingSet = NerDataSet.Load(tempFile, NerDataSet.AnnotationStyle.OPEN_NLP);
                TurkishMorphology morphology = TurkishMorphology.Builder()
                    .SetLexicon("Ankara", "İstanbul", "güzel", "şehir", "büyük")
                    .Build();
                PerceptronNerTrainer trainer = new PerceptronNerTrainer(morphology);
                PerceptronNer ner = trainer.Train(trainingSet, trainingSet, 2, 0.5f);

                NerSentence result = ner.FindNamedEntities("Ankara güzel");
                Assert.IsNotNull(result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void LoadModelFromDirectoryRequiresFiles()
        {
            string tempFile = Path.GetTempFileName();
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Directory.CreateDirectory(tempDir);
                File.WriteAllLines(tempFile, new[]
                {
                    "<START:LOCATION> Ankara <END> güzel .",
                    "<START:PERSON> Ali <END> gitti .",
                    "<START:LOCATION> İstanbul <END> büyük .",
                    "<START:PERSON> Mehmet <END> geldi .",
                });

                NerDataSet trainingSet = NerDataSet.Load(tempFile, NerDataSet.AnnotationStyle.OPEN_NLP);
                TurkishMorphology morphology = TurkishMorphology.Builder()
                    .SetLexicon("Ankara", "İstanbul", "güzel", "büyük")
                    .Build();
                PerceptronNerTrainer trainer = new PerceptronNerTrainer(morphology);
                PerceptronNer trained = trainer.Train(trainingSet, trainingSet, 2, 0.5f);

                trained.SaveModelAsText(tempDir);
                PerceptronNer loaded = PerceptronNer.LoadModel(tempDir, morphology);
                Assert.IsNotNull(loaded);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
