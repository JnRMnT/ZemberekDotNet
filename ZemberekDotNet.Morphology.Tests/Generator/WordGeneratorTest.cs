using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZemberekDotNet.Morphology.Generator;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;
using ZemberekDotNet.Morphology.Tests.Analysis;
using static ZemberekDotNet.Morphology.Generator.WordGenerator;

namespace ZemberekDotNet.Morphology.Tests.Generator
{
    [TestClass]
    public class WordGeneratorTest : AnalyzerTestBase
    {
        [TestMethod]
        public void TestGeneration1()
        {
            WordGenerator wordGenerator = new WordGenerator(GetMorphotactics("elma"));
            List<string> morphemes = new List<string> { "A3pl", "P1pl" };
            List<Result> results = wordGenerator.Generate(
                "elma",
                morphemes
            );
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("elmalarımız", results[0].surface);
        }

        [TestMethod]
        public void TestGeneration2()
        {
            WordGenerator wordGenerator = new WordGenerator(GetMorphotactics("elma"));
            List<string> morphemes = new List<string> { "Noun", "A3pl", "P1pl" };
            List<Result> results = wordGenerator.Generate(
                "elma",
                morphemes
            );
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("elmalarımız", results[0].surface);
        }

        [TestMethod]
        public void TestGeneration3()
        {
            WordGenerator wordGenerator = new WordGenerator(GetMorphotactics("elma"));
            List<string> morphemes = new List<string> { "Noun", "With" };
            List<Result> results = wordGenerator.Generate(
                "elma",
                morphemes
            );
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("elmalı", results[0].surface);
        }

        [TestMethod]
        public void TestGeneration4()
        {
            TurkishMorphotactics mo = GetMorphotactics("elma");
            WordGenerator wordGenerator = new WordGenerator(mo);
            List<string> morphemes = new List<string> { "Noun", "A3pl", "P1pl" };
            List<Result> results = wordGenerator.Generate(
                mo.GetRootLexicon().GetItemById("elma_Noun"),
                TurkishMorphotactics.GetMorphemes(morphemes)
            );
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("elmalarımız", results[0].surface);
        }

        [TestMethod]
        public void TestGeneration5()
        {
            TurkishMorphotactics mo = GetMorphotactics("yapmak");
            WordGenerator wordGenerator = new WordGenerator(mo);
            List<string> morphemes = new List<string> { "Verb", "Opt", "A1pl" };
            DictionaryItem item = mo.GetRootLexicon().GetItemById("yapmak_Verb");
            List<Result> results = wordGenerator.Generate(
                item,
                TurkishMorphotactics.GetMorphemes(morphemes)
            );
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("yapalım", results[0].surface);
        }
    }
}