using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ZemberekDotNet.LangID.Tests
{
    [TestClass]
    public class LanguageIndentifierTest
    {
        [TestMethod]
        public void AllModelTest()
        {
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
            Assert.AreEqual("tr", lid.Identify("merhaba dünya ve tüm gezegenler"));
            Assert.AreEqual("en", lid.Identify("hello world and all the planets what is this?"));
            Assert.AreEqual("es", lid.Identify("Hola mundo y todos los planetas"));
            Assert.AreEqual("fr", lid.Identify("Bonjour tout le monde et toutes les planètes"));
            Assert.AreEqual("az", lid.Identify("Salam dünya və bütün planetlərin bu həqiqətən pis olur"));
        }

        [TestMethod]
        public void AllModelSamplingTest()
        {
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
            Assert.AreEqual("tr", lid.Identify("merhaba dünya ve tüm gezegenler", 20));
            Assert.AreEqual("en", lid.Identify("hello world and all the planets what is this?", 20));
            Assert.AreEqual("es", lid.Identify("Hola mundo y todos los planetas", 20));
            Assert.AreEqual("fr", lid.Identify("Bonjour tout le monde et toutes les planètes", 20));
            Assert.AreEqual("az", lid.Identify("Salam dünya və bütün planetlərin bu həqiqətən pis olur", 20));
        }

        [TestMethod]
        public void AllModelFastSamplingTest()
        {
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
            Assert.AreEqual("tr", lid.IdentifyFast("merhaba dünya ve tüm gezegenler", 20));
            Assert.AreEqual("en", lid.IdentifyFast("hello world and all the planets what is this?", 20));
            Assert.AreEqual("es", lid.IdentifyFast("Hola mundo y todos los planetas", 20));
            Assert.AreEqual("fr", lid.IdentifyFast("Bonjour tout le monde et toutes les planètes", 20));
            Assert.AreEqual("az", lid.IdentifyFast("Salam dünya və bütün planetlərin bu həqiqətən pis olur", 20));
        }

        [TestMethod]
        public void ModelGroupTest()
        {
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModelGroup("tr_group");
            Assert.AreEqual("tr", lid.Identify("merhaba dünya ve tüm gezegenler"));
            Assert.AreEqual("unk", lid.Identify("Hola mundo y todos los planetas"));
        }

        [TestMethod]
        public void TestContainsLanguage()
        {
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
            String tr_es = "merhaba dünya ve tüm gezegenler Hola mundo y todos los planetas";
            Assert.IsTrue(lid.ContainsLanguage(tr_es, "tr", 20));
            Assert.IsTrue(lid.ContainsLanguage(tr_es, "es", 20));

            Assert.IsFalse(lid.ContainsLanguage(tr_es, "ar", 20));

            String es_en = "Hola mundo y todos los planetas " +
            "The state is that great fiction by which everyone tries to live at the expense of everyone else";
            Assert.IsTrue(lid.ContainsLanguage(es_en, "es", 20));
            Assert.IsTrue(lid.ContainsLanguage(es_en, "en", 20));
            Assert.IsFalse(lid.ContainsLanguage(es_en, "tr", 20));
            Assert.IsFalse(lid.ContainsLanguage(es_en, "ar", 20));
        }

        [TestMethod]
        public void GetLanguagesTest()
        {
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModelGroup("tr_group");
            Assert.IsTrue(lid.GetLanguages().Contains("tr"));
            Assert.IsTrue(lid.GetLanguages().Contains("en"));
            Assert.IsFalse(lid.GetLanguages().Contains("unk"));
            Assert.IsFalse(lid.GetLanguages().Contains("ar"));
        }
    }
}
