using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.Tokenization.Tests
{
    [TestClass]
    public class TurkishSentenceExtractorTest
    {
        static List<string> GetSentences(string pipeDelimited)
        {
            return pipeDelimited.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).Where(e=>!string.IsNullOrEmpty(e)).ToList();
        }

        [TestMethod]
        public void SingletonAccessShouldNotThrowException()
        {
            TurkishSentenceExtractor.Default.FromParagraph("hello");
        }

        [TestMethod]
        public void ShouldExtractSentences1()
        {
            string test = "Merhaba Dünya.| Nasılsın?";
            List<string> expected = GetSentences(test);

            Assert.IsTrue(Enumerable.SequenceEqual(expected,
            TurkishSentenceExtractor.Default.FromParagraph(test.Replace("|", ""))));
        }

        [TestMethod]
        public void ShouldExtractSingleSentences()
        {
            string test = "Merhaba Dünya.";
            List<string> expected = GetSentences(test);

            Assert.IsTrue(Enumerable.SequenceEqual(expected,
            TurkishSentenceExtractor.Default.FromParagraph(test.Replace("|", ""))));
        }

        [TestMethod]
        public void ShouldExtractSentencesSecondDoesNotEndWithDot()
        {
            string test = "Merhaba Dünya.| Nasılsın";
            List<string> expected = GetSentences(test);

            Assert.IsTrue(Enumerable.SequenceEqual(expected,
            TurkishSentenceExtractor.Default.FromParagraph(test.Replace("|", ""))));
        }

        [TestMethod]
        public void ShouldReturnDotForDot()
        {
            List<string> expected = GetSentences(".");
            Assert.IsTrue(Enumerable.SequenceEqual(expected, TurkishSentenceExtractor.Default.FromParagraph(".")));
        }

        [TestMethod]
        public void ShouldReturn0ForEmpty()
        {
            Assert.AreEqual(0, TurkishSentenceExtractor.Default.FromParagraph("").Count);
        }

        [TestMethod]
        public void ExtractFromDocument()
        {
            Assert.AreEqual("Merhaba!|Bugün 2. köprü Fsm.'de trafik vardı.|değil mi?",
            MarkBoundariesDocument("Merhaba!\n Bugün 2. köprü Fsm.'de trafik vardı.değil mi?\n"));
            Assert.AreEqual("Ali|gel.",
            MarkBoundariesDocument("Ali\n\n\rgel.\n"));
            Assert.AreEqual("Ali gel.|Merhaba|Ne haber?",
            MarkBoundariesDocument("\n\nAli gel. Merhaba\n\rNe haber?"));

        }

        private string MarkBoundariesDocument(string input)
        {
            List<String> list = TurkishSentenceExtractor.Default.FromDocument(input);
            return string.Join("|", list);
        }


        private string MarkBoundariesParagraph(string input)
        {
            List<string> list = TurkishSentenceExtractor.Default.FromParagraph(input);
            return string.Join("|", list);
        }

        private string MarkBoundariesParagraph(TurkishSentenceExtractor extractor, string input)
        {
            List<String> list = extractor.FromParagraph(input);
            return string.Join("|", list);
        }

        [TestMethod]
        public void TestSimpleSentence()
        {
            Assert.AreEqual("Merhaba!|Bugün 2. köprü Fsm.'de trafik vardı.|değil mi?",
            MarkBoundariesParagraph("Merhaba! Bugün 2. köprü Fsm.'de trafik vardı.değil mi?"));
            Assert.AreEqual("Prof. Dr. Veli Zambur %2.5 lik enflasyon oranini begenmemis!",
            MarkBoundariesParagraph("Prof. Dr. Veli Zambur %2.5 lik enflasyon oranini begenmemis!"));
            Assert.AreEqual("Ali gel.",
            MarkBoundariesParagraph("Ali gel."));
            Assert.AreEqual("Ali gel.|Okul acildi!",
            MarkBoundariesParagraph("Ali gel. Okul acildi!"));
            Assert.AreEqual("Ali gel.|Okul acildi!",
            MarkBoundariesParagraph("Ali gel. Okul acildi!"));
            Assert.AreEqual("Ali gel...|Okul acildi.",
            MarkBoundariesParagraph("Ali gel... Okul acildi."));
            Assert.AreEqual("Tam 1.000.000 papeli cebe atmislar...",
            MarkBoundariesParagraph("Tam 1.000.000 papeli cebe atmislar..."));
            Assert.AreEqual("16. yüzyılda?|Dr. Av. Blah'a gitmiş.",
            MarkBoundariesParagraph("16. yüzyılda? Dr. Av. Blah'a gitmiş."));
            Assert.AreEqual("Ali gel.|Okul açıldı...|sınavda 2. oldum.",
            MarkBoundariesParagraph("Ali gel. Okul açıldı... sınavda 2. oldum."));
        }

        [TestMethod]
        public void ShouldReturn0ForEmptyff()
        {
            List<string> sentences = TurkishSentenceExtractor.Default.FromParagraph("");
            Assert.AreEqual(0, sentences.Count);
        }

        [TestMethod]
        public void TestDoubleQuotes()
        {
            TurkishSentenceExtractor e = TurkishSentenceExtractor
            .Builder()
            .DoNotSplitInDoubleQuotes()
            .UseDefaultModel().Build();

            Assert.AreEqual(
            "\"Merhaba! Bugün hava çok güzel. Ne dersin?\" dedi tavşan.|Havucu kemirirken.",
            MarkBoundariesParagraph(
                e,
                "\"Merhaba! Bugün hava çok güzel. Ne dersin?\" dedi tavşan. Havucu kemirirken."));

            Assert.AreEqual(
            "\"Buna hakkı yok!\" diye öfkeyle konuşmaya başladı Baba Kurt.",
            MarkBoundariesParagraph(
                e, "\"Buna hakkı yok!\" diye öfkeyle konuşmaya başladı Baba Kurt."));
        }
    }
}
