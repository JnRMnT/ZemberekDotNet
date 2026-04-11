using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
        [Ignore("Requires trained NER model and morphology.")]
        public void FindNamedEntitiesInSentenceRequiresModel()
        {
            // This test requires a trained model. Run manually after loading a model.
        }

        [TestMethod]
        [Ignore("Requires trained NER model file on disk.")]
        public void LoadModelFromDirectoryRequiresFiles()
        {
            // string modelRoot = @"path/to/model";
            // var morphology = ZemberekDotNet.Morphology.TurkishMorphology.CreateWithDefaults();
            // PerceptronNer ner = PerceptronNer.LoadModel(modelRoot, morphology);
            // Assert.IsNotNull(ner);
        }
    }
}
