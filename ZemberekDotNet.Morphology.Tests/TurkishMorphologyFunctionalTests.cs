using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon.TR;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Core.Turkish;

namespace ZemberekDotNet.Morphology.Tests
{
    [TestClass]
    public class TurkishMorphologyFunctionalTests
    {
        private TurkishMorphology GetEmptyTurkishMorphology()
        {
            return TurkishMorphology
                .Builder()
                .DisableCache()
                .Build();
        }

        private TurkishMorphology GetMorphology(params string[] lines)
        {
            return TurkishMorphology
                .Builder()
                .SetLexicon(lines)
                .DisableCache()
                .Build();
        }

        private TurkishMorphology GetAsciiTolerantMorphology(params string[] lines)
        {
            RootLexicon lexicon = TurkishDictionaryLoader.Load(lines);
            return TurkishMorphology
                .Builder()
                .SetLexicon(lines)
                .SetLexicon(lexicon)
                .DiacriticsInAnalysisIgnored()
                .DisableCache()
                .Build();
        }

        [TestMethod]
        public void TestWordsWithCircumflex()
        {
            TurkishMorphology morphology = GetMorphology("zekâ");
            WordAnalysis result = morphology.Analyze("zekâ");
            Assert.AreEqual(1, result.AnalysisCount());
        }

        [TestMethod]
        public void TestPossibleProper2()
        {
            TurkishMorphology morphology = GetMorphology("Air");
            Assert.AreEqual(0, morphology.Analyze("Air'rrr").AnalysisCount());
            Assert.AreEqual(1, morphology.Analyze("Air").AnalysisCount());
        }

        [TestMethod]
        public void TestWordsWithDot()
        {
            TurkishMorphology morphology = GetMorphology("Dr [P:Abbrv]");
            WordAnalysis result = morphology.Analyze("Dr.");
            Assert.AreEqual(1, result.AnalysisCount());
        }

        [TestMethod]
        public void TestRomanNumeral()
        {
            TurkishMorphology morphology = GetMorphology("dört [P:Num,Card;A:Voicing]");
            WordAnalysis result = morphology.Analyze("IV");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.RomanNumeral,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
        }

        [TestMethod]
        public void TestRomanNumeral2()
        {
            // Instance with no dictionary item.
            TurkishMorphology morphology = GetMorphology("dördüncü [P:Num,Ord]");
            WordAnalysis result = morphology.Analyze("XXIV.");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.RomanNumeral,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
        }

        [TestMethod]
        public void TestRomanNumeral3()
        {
            TurkishMorphology morphology = GetMorphology("dört [P:Num,Card;A:Voicing]");
            WordAnalysis result = morphology.Analyze("XXIV'ten");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.RomanNumeral,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
        }

        [TestMethod]
        public void TestDate()
        {
            TurkishMorphology morphology = GetMorphology("dört [P:Num,Card;A:Voicing]");
            WordAnalysis result = morphology.Analyze("1.1.2014");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.Date,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
        }

        [TestMethod]
        public void TestDate2()
        {
            TurkishMorphology morphology = GetMorphology("dört [P:Num,Card;A:Voicing]");
            WordAnalysis result = morphology.Analyze("1.1.2014'te");
            Assert.AreEqual(1, result.AnalysisCount());
            SingleAnalysis analysis = result.GetAnalysisResults()[0];
            Assert.AreEqual(
                SecondaryPos.Date,
                analysis.GetDictionaryItem().secondaryPos);
            String lexical = analysis.FormatLexical();
            Assert.IsTrue(lexical.EndsWith("A3sg+Loc"));
        }

        [TestMethod]
        public void TestUrl()
        {
            TurkishMorphology morphology = GetMorphology();
            WordAnalysis result = morphology.Analyze("www.foo.com");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.Url,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
            string lexical = result.GetAnalysisResults()[0].FormatLexical();
            Assert.IsTrue(lexical.EndsWith("A3sg"));
        }

        [TestMethod]
        public void TestUrl2()
        {
            TurkishMorphology morphology = GetMorphology();
            WordAnalysis result = morphology.Analyze("www.foo.com'da");
            Assert.AreEqual(1, result.AnalysisCount());
            SingleAnalysis analysis = result.GetAnalysisResults()[0];
            Assert.AreEqual(
                SecondaryPos.Url,
                analysis.GetDictionaryItem().secondaryPos);
            String lexical = analysis.FormatLexical();
            Assert.IsTrue(lexical.EndsWith("A3sg+Loc"));
            Assert.AreEqual("www.foo.com", analysis.GetDictionaryItem().lemma);
        }

        [TestMethod]
        public void TestHashTag()
        {
            TurkishMorphology morphology = GetMorphology();
            WordAnalysis result = morphology.Analyze("#haha_ha'ya");
            Assert.AreEqual(1, result.AnalysisCount());
            SingleAnalysis analysis = result.GetAnalysisResults()[0];
            Assert.AreEqual(
                SecondaryPos.HashTag,
                analysis.GetDictionaryItem().secondaryPos);
            String lexical = analysis.FormatLexical();
            Assert.IsTrue(lexical.EndsWith("A3sg+Dat"));
            Assert.AreEqual("#haha_ha", analysis.GetDictionaryItem().lemma);
        }

        [TestMethod]
        public void TestHashTag2()
        {
            TurkishMorphology morphology = GetMorphology();
            WordAnalysis result = morphology.Analyze("#123'efefe");
            Assert.AreEqual(1, result.AnalysisCount());
            SingleAnalysis analysis = result.GetAnalysisResults()[0];
            Assert.AreEqual(
                SecondaryPos.HashTag,
                analysis.GetDictionaryItem().secondaryPos);
            Assert.AreEqual(
                "#123'efefe",
                analysis.GetDictionaryItem().lemma);
        }

        [TestMethod]
        public void TestMention()
        {
            TurkishMorphology morphology = GetMorphology();
            WordAnalysis result = morphology.Analyze("@haha_ha'ya");
            Assert.AreEqual(1, result.AnalysisCount());
            SingleAnalysis analysis = result.GetAnalysisResults()[0];
            Assert.AreEqual(
                SecondaryPos.Mention,
                analysis.GetDictionaryItem().secondaryPos);
            String lexical = analysis.FormatLexical();
            Assert.IsTrue(lexical.EndsWith("A3sg+Dat"));
            Assert.AreEqual("@haha_ha", analysis.GetDictionaryItem().lemma);
            Assert.IsTrue(lexical.Contains("@haha_ha"));
        }

        [TestMethod]
        public void TestEmail()
        {
            TurkishMorphology morphology = GetMorphology();
            WordAnalysis result = morphology.Analyze("foo@bar.com'a");
            Assert.AreEqual(1, result.AnalysisCount());
            SingleAnalysis analysis = result.GetAnalysisResults()[0];
            Assert.AreEqual(
                SecondaryPos.Email,
                analysis.GetDictionaryItem().secondaryPos);
            String lexical = analysis.FormatLexical();
            Assert.IsTrue(lexical.EndsWith("A3sg+Dat"));
            Assert.AreEqual("foo@bar.com", analysis.GetDictionaryItem().lemma);
        }

        [TestMethod]
        public void TestTime()
        {
            TurkishMorphology morphology = GetMorphology("otuz [P:Num,Card]");
            WordAnalysis result = morphology.Analyze("20:30'da");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.Clock,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
        }


        [TestMethod]
        public void TestTime2()
        {
            TurkishMorphology morphology = GetMorphology("dört [P:Num,Card;A:Voicing]");
            WordAnalysis result = morphology.Analyze("10:24'te");
            Assert.AreEqual(1, result.AnalysisCount());
            SingleAnalysis analysis = result.GetAnalysisResults()[0];
            Assert.AreEqual(
                SecondaryPos.Clock,
                analysis.GetDictionaryItem().secondaryPos);
            String lexical = analysis.FormatLexical();
            Assert.IsTrue(lexical.EndsWith("A3sg+Loc"));
        }

        [TestMethod]
        public void TestRatio()
        {
            TurkishMorphology morphology = GetMorphology("iki [P:Num,Card]");
            WordAnalysis result = morphology.Analyze("1/2");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.Ratio,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
        }

        [TestMethod]
        public void TestPercent()
        {
            TurkishMorphology morphology = GetMorphology("iki [P:Num,Card]");
            string[] correct = { "%2", "%2'si", "%2.2'si", "%2,2'si" };
            foreach (string s in correct)
            {
                WordAnalysis result = morphology.Analyze(s);
                Assert.AreEqual(1, result.AnalysisCount(), "Failed for " + s);
                Assert.AreEqual(SecondaryPos.Percentage,
                    result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos, "Failed for " + s);
            }
        }

        [TestMethod]
        public void TestEmoticon()
        {
            TurkishMorphology morphology = GetEmptyTurkishMorphology();
            WordAnalysis result = morphology.Analyze(":)");
            Assert.AreEqual(1, result.AnalysisCount());
            Assert.AreEqual(
                SecondaryPos.Emoticon,
                result.GetAnalysisResults()[0].GetDictionaryItem().secondaryPos);
        }

        [TestMethod]
        public void TestForeingLocale()
        {
            TurkishMorphology morphology = GetMorphology("UNICEF [A:LocaleEn]");

            WordAnalysis result = morphology.Analyze("Unicefte");
            Assert.AreEqual(1, result.AnalysisCount());

            morphology = GetMorphology("UNICEF");
            result = morphology.Analyze("Unicefte");
            Assert.AreEqual(0, result.AnalysisCount());
        }

        [TestMethod]
        public void TestWordsWithDash()
        {
            // Instance with no dictionary item.
            TurkishMorphology morphology = GetEmptyTurkishMorphology();
            WordAnalysis result = morphology.Analyze("Blah-Foo'ya");
            Assert.AreEqual(1, result.AnalysisCount());
        }

        [TestMethod]
        public void TestUnidentifiedWordNoVowel()
        {
            TurkishMorphology morphology = GetMorphology();
            WordAnalysis result = morphology.Analyze("gnctrkcll");
            Assert.AreEqual(0, result.AnalysisCount());
        }

        [TestMethod]
        public void TestAbbreviationVoicing_Issue_183()
        {
            TurkishMorphology morphology = GetMorphology("Tübitak [P:Abbrv]");
            WordAnalysis result = morphology.Analyze("Tübitak'a");
            Assert.AreEqual(1, result.AnalysisCount());
            result = morphology.Analyze("Tübitaka");
            Assert.AreEqual(1, result.AnalysisCount());
            result = morphology.Analyze("Tübitağa");
            Assert.AreEqual(0, result.AnalysisCount());
        }

        [TestMethod]
        public void TestAbbreviationShouldNotGetBecomeOrAcquire_Issue218()
        {
            TurkishMorphology morphology = GetMorphology("aa [P:Abbrv]");
            WordAnalysis result = morphology.Analyze("aalaş");
            Assert.AreEqual(0, result.AnalysisCount());
            result = morphology.Analyze("aalan");
            Assert.AreEqual(0, result.AnalysisCount());
        }

        [TestMethod]
        public void TestAsciiTolerantMorphology()
        {
            // Instance with no dictionary item.
            TurkishMorphology morphology = GetAsciiTolerantMorphology(
                "sıra", "şıra", "armut", "kazan", "ekonomik [P:Adj]", "insan");
            RuleBasedAnalyzer analyzer = morphology.GetAnalyzer();
            List<SingleAnalysis> result;
            result = analyzer.Analyze("ekonomık");
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "ekonomik"));
            result = analyzer.Analyze("sira");
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "sıra", "şıra"));
            result = analyzer.Analyze("siraci");
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "sıra", "şıra"));
            result = analyzer.Analyze("armutcuga");
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "armut"));
            result = analyzer.Analyze("kazancıga");
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "kazan"));
            result = analyzer.Analyze("kazanciga");
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "kazan"));
            result = analyzer.Analyze("kazançiğimizdan");
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "kazan"));
            result = analyzer.Analyze("ınsanların");
            Assert.IsTrue(ContainsAllDictionaryLemma(result, "insan"));
        }

        private bool ContainsAllDictionaryLemma(List<SingleAnalysis> analyses, params string[] item)
        {
            foreach (string i in item)
            {
                bool fail = true;
                foreach (SingleAnalysis s in analyses)
                {
                    if (s.GetDictionaryItem().lemma.Contains(i))
                    {
                        fail = false;
                        break;
                    }
                }
                if (fail)
                {
                    Log.Info("Failed to find item {0}", i);
                    return false;
                }
            }
            return true;
        }
    }
}
