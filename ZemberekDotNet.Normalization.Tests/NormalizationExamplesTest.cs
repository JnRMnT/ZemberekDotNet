using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Normalization;

namespace ZemberekDotNet.Normalization.Tests
{
    [TestClass]
    public class NormalizationExamplesTest
    {
        private static TurkishSpellChecker spellChecker;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            spellChecker = new TurkishSpellChecker(morphology);
        }

        [TestMethod]
        public void CheckReturnsTrueForCorrectWord()
        {
            Assert.IsTrue(spellChecker.Check("kitap"));
        }

        [TestMethod]
        public void CheckReturnsFalseForMisspelledWord()
        {
            Assert.IsFalse(spellChecker.Check("ktap"));
        }

        [TestMethod]
        public void SuggestForWordReturnsAtLeastOneSuggestion()
        {
            List<string> suggestions = spellChecker.SuggestForWord("ktap");
            Assert.IsTrue(suggestions.Count > 0);
        }

        [TestMethod]
        public void SuggestForWordContainsExpectedCandidate()
        {
            List<string> suggestions = spellChecker.SuggestForWord("ktap");
            CollectionAssert.Contains(suggestions, "kitap");
        }

        [TestMethod]
        public void TokenizeForSpellingSkipsPunctuation()
        {
            List<string> tokens = TurkishSpellChecker.TokenizeForSpelling("Merhaba, dünya!");
            Assert.IsFalse(tokens.Contains(","));
            Assert.IsFalse(tokens.Contains("!"));
            CollectionAssert.Contains(tokens, "merhaba");
            CollectionAssert.Contains(tokens, "dünya");
        }

        [TestMethod]
        public void SentenceContainsAtLeastOneMisspelling()
        {
            string sentence = "Bugün okula gittim ve çok güzl bir gün geçirdim .";
            List<string> tokens = TurkishSpellChecker.TokenizeForSpelling(sentence);

            bool hasMisspelling = false;
            foreach (string token in tokens)
            {
                if (!spellChecker.Check(token))
                {
                    hasMisspelling = true;
                    break;
                }
            }

            Assert.IsTrue(hasMisspelling);
        }

        [TestMethod]
        public void SentenceContainsKnownCorrectWords()
        {
            string sentence = "Bugün okula gittim ve çok güzel bir gün geçirdim .";
            List<string> tokens = TurkishSpellChecker.TokenizeForSpelling(sentence);

            Assert.IsTrue(spellChecker.Check(tokens[0])); // bugün
            Assert.IsTrue(spellChecker.Check(tokens[1])); // okula
            Assert.IsTrue(spellChecker.Check(tokens[2])); // gittim
        }
    }
}
