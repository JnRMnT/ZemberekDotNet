using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.LangID;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.LangID.Tests
{
    [TestClass]
    public class MixedLanguageScannerTest
    {
        private static readonly LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();

        [TestMethod]
        public void IdentifiesTurkishSentenceAsTr()
        {
            string sentence = "Türkiye güzel bir ülkedir.";
            Assert.AreEqual("tr", lid.Identify(sentence));
        }

        [TestMethod]
        public void IdentifiesEnglishSentenceAsEn()
        {
            string sentence = "This sentence is written in English.";
            Assert.AreEqual("en", lid.Identify(sentence));
        }

        [TestMethod]
        public void SentenceSplitterSplitsMixedParagraph()
        {
            string paragraph =
                "Türkiye harika bir ülkedir. " +
                "Turkey is a beautiful country. " +
                "Türk mutfağı dünyaca ünlüdür.";

            var sentences = TurkishSentenceExtractor.Default.FromParagraph(paragraph);
            Assert.AreEqual(3, sentences.Count);
        }

        [TestMethod]
        public void ContainsLanguageDetectsTrAndEsInMixedText()
        {
            string mixed = "merhaba dünya ve tüm gezegenler Hola mundo y todos los planetas";

            Assert.IsTrue(lid.ContainsLanguage(mixed, "tr", 20));
            Assert.IsTrue(lid.ContainsLanguage(mixed, "es", 20));
            Assert.IsFalse(lid.ContainsLanguage(mixed, "ar", 20));
        }

        [TestMethod]
        public void TurkishGroupModelReturnsKnownLanguageForTurkishSentence()
        {
            LanguageIdentifier trGroup = LanguageIdentifier.FromInternalModelGroup("tr_group");
            string lang = trGroup.Identify("Bugün hava çok güzel.");
            Assert.AreEqual("tr", lang);
        }
    }
}
