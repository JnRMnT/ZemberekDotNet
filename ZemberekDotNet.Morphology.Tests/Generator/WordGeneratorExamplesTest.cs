using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Generator;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Morphology.Tests.Generator
{
    [TestClass]
    public class WordGeneratorExamplesTest
    {
        private static TurkishMorphology morphology;
        private static WordGenerator gen;
        private static RootLexicon lexicon;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            morphology = TurkishMorphology.CreateWithDefaults();
            gen = morphology.GetWordGenerator();
            lexicon = morphology.GetLexicon();
        }

        [TestMethod]
        public void NounDativeGeneratesCorrectly()
        {
            DictionaryItem kitap = lexicon.GetMatchingItems("kitap")
                .First(i => i.primaryPos == PrimaryPos.Noun);
            List<WordGenerator.Result> results = gen.Generate(kitap, "Dat");
            CollectionAssert.Contains(results.Select(r => r.surface).ToList(), "kitaba");
        }

        [TestMethod]
        public void NounAccusativeGeneratesCorrectly()
        {
            DictionaryItem kitap = lexicon.GetMatchingItems("kitap")
                .First(i => i.primaryPos == PrimaryPos.Noun);
            List<WordGenerator.Result> results = gen.Generate(kitap, "Acc");
            CollectionAssert.Contains(results.Select(r => r.surface).ToList(), "kitabı");
        }

        [TestMethod]
        public void NounPluralDativeGeneratesCorrectly()
        {
            DictionaryItem kitap = lexicon.GetMatchingItems("kitap")
                .First(i => i.primaryPos == PrimaryPos.Noun);
            List<WordGenerator.Result> results = gen.Generate(kitap, "A3pl", "Dat");
            CollectionAssert.Contains(results.Select(r => r.surface).ToList(), "kitaplara");
        }

        [TestMethod]
        public void NounLocativeGeneratesCorrectly()
        {
            DictionaryItem kitap = lexicon.GetMatchingItems("kitap")
                .First(i => i.primaryPos == PrimaryPos.Noun);
            List<WordGenerator.Result> results = gen.Generate(kitap, "Loc");
            CollectionAssert.Contains(results.Select(r => r.surface).ToList(), "kitapta");
        }

        [TestMethod]
        public void NounAblativeGeneratesCorrectly()
        {
            DictionaryItem kitap = lexicon.GetMatchingItems("kitap")
                .First(i => i.primaryPos == PrimaryPos.Noun);
            List<WordGenerator.Result> results = gen.Generate(kitap, "Abl");
            CollectionAssert.Contains(results.Select(r => r.surface).ToList(), "kitaptan");
        }

        [TestMethod]
        public void VerbPresentProgressiveFirstPersonSingularGeneratesCorrectly()
        {
            DictionaryItem oku = lexicon.GetMatchingItems("okumak")
                .First(i => i.primaryPos == PrimaryPos.Verb);
            List<WordGenerator.Result> results = gen.Generate(oku, "Prog1", "A1sg");
            CollectionAssert.Contains(results.Select(r => r.surface).ToList(), "okuyorum");
        }

        [TestMethod]
        public void VerbPresentProgressiveThirdPersonPluralGeneratesCorrectly()
        {
            DictionaryItem oku = lexicon.GetMatchingItems("okumak")
                .First(i => i.primaryPos == PrimaryPos.Verb);
            List<WordGenerator.Result> results = gen.Generate(oku, "Prog1", "A3pl");
            CollectionAssert.Contains(results.Select(r => r.surface).ToList(), "okuyorlar");
        }

        [TestMethod]
        public void VerbAllSixPersonsYieldDistinctForms()
        {
            DictionaryItem oku = lexicon.GetMatchingItems("okumak")
                .First(i => i.primaryPos == PrimaryPos.Verb);

            string[] personIds = { "A1sg", "A2sg", "A3sg", "A1pl", "A2pl", "A3pl" };
            var surfaces = personIds
                .Select(p => gen.Generate(oku, "Prog1", p))
                .Where(r => r.Count > 0)
                .Select(r => r[0].surface)
                .ToList();

            Assert.AreEqual(6, surfaces.Distinct().Count(),
                "Expected 6 distinct conjugation forms.");
        }
    }
}
