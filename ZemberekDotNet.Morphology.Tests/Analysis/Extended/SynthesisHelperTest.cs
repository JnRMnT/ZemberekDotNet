using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Extended;

namespace ZemberekDotNet.Morphology.Tests.Analysis.Extended
{
    [TestClass]
    public class SynthesisHelperTest
    {
        private static TurkishMorphology morphology;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            morphology = TurkishMorphology.CreateWithDefaults();
        }

        [TestMethod]
        public void Kitap_Dative_ContainsKitaba()
        {
            List<string> forms = morphology.ExtdSynthesize("kitap", TurkishCase.Dative);
            Assert.IsTrue(forms.Count > 0, "Expected forms for kitap+Dative");
            CollectionAssert.Contains(forms, "kitaba");
        }

        [TestMethod]
        public void Kitap_Ablative_ContainsKitaptan()
        {
            List<string> forms = morphology.ExtdSynthesize("kitap", TurkishCase.Ablative);
            Assert.IsTrue(forms.Count > 0);
            CollectionAssert.Contains(forms, "kitaptan");
        }

        [TestMethod]
        public void Araba_Locative_ContainsArabada()
        {
            List<string> forms = morphology.ExtdSynthesize("araba", TurkishCase.Locative);
            Assert.IsTrue(forms.Count > 0);
            CollectionAssert.Contains(forms, "arabada");
        }

        [TestMethod]
        public void Kitap_NominativePlural_ContainsKitaplar()
        {
            List<string> forms = morphology.ExtdSynthesize("kitap", TurkishCase.Nominative, plural: true);
            Assert.IsTrue(forms.Count > 0);
            CollectionAssert.Contains(forms, "kitaplar");
        }

        [TestMethod]
        public void Araba_Accusative_ContainsArabay()
        {
            List<string> forms = morphology.ExtdSynthesize("araba", TurkishCase.Accusative);
            Assert.IsTrue(forms.Count > 0);
            // "arabayı" is the expected accusative form
            CollectionAssert.Contains(forms, "arabayı");
        }

        [TestMethod]
        public void UnknownLemma_ReturnsEmpty_NoException()
        {
            List<string> forms = morphology.ExtdSynthesize("xyzunknown", TurkishCase.Dative);
            Assert.AreEqual(0, forms.Count, "Expected empty list for unknown lemma — no exception");
        }

        [TestMethod]
        public void NullLemma_ReturnsEmpty_NoException()
        {
            List<string> forms = morphology.ExtdSynthesize(null, TurkishCase.Dative);
            Assert.AreEqual(0, forms.Count);
        }

        [TestMethod]
        public void UnknownCase_ReturnsEmpty_NoException()
        {
            List<string> forms = morphology.ExtdSynthesize("kitap", TurkishCase.Unknown);
            Assert.AreEqual(0, forms.Count, "TurkishCase.Unknown has no mapping and should return empty");
        }

        [TestMethod]
        public void Ev_DativePlural_ContainsEvlere()
        {
            List<string> forms = morphology.ExtdSynthesize("ev", TurkishCase.Dative, plural: true);
            Assert.IsTrue(forms.Count > 0);
            CollectionAssert.Contains(forms, "evlere");
        }
    }
}
