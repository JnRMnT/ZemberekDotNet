using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class WordAnalysisSurfaceFormatterTest
    {
        [TestMethod]
        public void FormatNonProperNoun()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .DisableCache()
                .SetLexicon("elma", "kitap", "demek", "evet")
                .Build();

            string[] inputs = {"elmamadaki", "elma", "kitalarımdan", "kitabımızsa", "diyebileceğimiz",
        "dedi", "evet"};

            WordAnalysisSurfaceFormatter formatter = new WordAnalysisSurfaceFormatter();

            foreach (string input in inputs)
            {
                WordAnalysis results = morphology.Analyze(input);
                foreach (SingleAnalysis result in results)
                {
                    Assert.AreEqual(input, formatter.Format(result, null));
                }
            }
        }

        [TestMethod]
        public void FormatKnownProperNouns()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .DisableCache()
                .SetLexicon("Ankara", "Iphone [Pr:ayfon, A:LocaleEn]", "Google [Pr:gugıl]")
                .Build();

            string[] inputs = { "ankarada", "ıphonumun", "googledan", "Iphone", "Google", "Googlesa" };
            string[] expected = { "Ankara'da", "Iphone'umun", "Google'dan", "Iphone", "Google", "Google'sa" };

            Check(morphology, inputs, expected, "'");
        }

        private void Check(TurkishMorphology morphology, string[] inputs, string[] expected, string apostrophe)
        {
            WordAnalysisSurfaceFormatter formatter = new WordAnalysisSurfaceFormatter();

            int i = 0;
            foreach (string input in inputs)
            {
                WordAnalysis results = morphology.Analyze(input);
                foreach (SingleAnalysis result in results)
                {
                    if (result.GetDictionaryItem().secondaryPos == SecondaryPos.ProperNoun)
                    {
                        string format = formatter.Format(result, apostrophe);
                        Assert.AreEqual(expected[i], format);
                    }
                }
                i++;
            }
        }

        [TestMethod]
        public void FormatKnownProperNounsNoQuote()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .DisableCache()
                .SetLexicon("Blah [A:NoQuote]").Build();

            string[] inputs = { "blaha", "Blahta" };
            string[] expected = { "Blaha", "Blahta" };

            Check(morphology, inputs, expected, null);
        }

        [TestMethod]
        public void FormatVerbs()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .DisableCache()
                .SetLexicon("olmak").Build();

            string[] inputs = { "olarak", "Olarak" };
            string[] expected = { "olarak", "Olarak" };

            Check(morphology, inputs, expected, null);
            // giving apostrophe should not effect the output.
            Check(morphology, inputs, expected, "'");
        }

        [TestMethod]
        public void FormatNumerals()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder().DisableCache().Build();
            string[] inputs = { "1e", "4ten", "123ü", "12,5ten" };
            string[] expected = { "1'e", "4'ten", "123'ü", "12,5ten" };

            WordAnalysisSurfaceFormatter formatter = new WordAnalysisSurfaceFormatter();

            int i = 0;
            foreach (string input in inputs)
            {
                WordAnalysis results = morphology.Analyze(input);
                foreach (SingleAnalysis result in results)
                {
                    if (result.GetDictionaryItem().primaryPos == PrimaryPos.Numeral)
                    {
                        Assert.AreEqual(expected[i], formatter.Format(result, "'"));
                    }
                }
                i++;
            }
        }

        [TestMethod]
        public void FormatToCase()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .DisableCache()
                .SetLexicon("kış", "şiir", "Aydın", "Google [Pr:gugıl]")
                .Build();

            string[] inputs =
                {"aydında", "googledan", "Google", "şiirde", "kışçığa", "kış"};

            string[] expectedDefaultCase =
                {"Aydın'da", "Google'dan", "Google", "şiirde", "kışçığa", "kış"};
            string[] expectedLowerCase =
                {"aydın'da", "google'dan", "google", "şiirde", "kışçığa", "kış"};
            string[] expectedUpperCase =
                {"AYDIN'DA", "GOOGLE'DAN", "GOOGLE", "ŞİİRDE", "KIŞÇIĞA", "KIŞ"};
            string[] expectedCapitalCase =
                {"Aydın'da", "Google'dan", "Google", "Şiirde", "Kışçığa", "Kış"};
            string[] expectedUpperRootLowerEndingCase =
                {"AYDIN'da", "GOOGLE'dan", "GOOGLE", "ŞİİRde", "KIŞçığa", "KIŞ"};

            TestCaseType(morphology, inputs, expectedDefaultCase,
                WordAnalysisSurfaceFormatter.CaseType.DEFAULT_CASE);
            TestCaseType(morphology, inputs, expectedLowerCase,
                WordAnalysisSurfaceFormatter.CaseType.LOWER_CASE);
            TestCaseType(morphology, inputs, expectedUpperCase,
                WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE);
            TestCaseType(morphology, inputs, expectedCapitalCase,
                WordAnalysisSurfaceFormatter.CaseType.TITLE_CASE);
            TestCaseType(morphology, inputs, expectedUpperRootLowerEndingCase,
                WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE_ROOT_LOWER_CASE_ENDING);
        }

        private void TestCaseType(
            TurkishMorphology morphology,
            string[] inputs,
            string[] expected,
            WordAnalysisSurfaceFormatter.CaseType caseType)
        {

            WordAnalysisSurfaceFormatter formatter = new WordAnalysisSurfaceFormatter();

            int i = 0;
            foreach (string input in inputs)
            {
                WordAnalysis results = morphology.Analyze(input);
                foreach (SingleAnalysis result in results)
                {
                    Assert.AreEqual(expected[i], formatter.FormatToCase(result, caseType));
                }
                i++;
            }
        }

        [TestMethod]
        public void GuessCaseTest()
        {
            string[] inputs = {"abc", "Abc", "ABC", "Abc'de", "ABC'DE", "ABC.", "ABC'de", "a", "12", "A",
        "A1"};
            WordAnalysisSurfaceFormatter.CaseType[] expected = {
        WordAnalysisSurfaceFormatter.CaseType.LOWER_CASE,
        WordAnalysisSurfaceFormatter.CaseType.TITLE_CASE,
        WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE,
        WordAnalysisSurfaceFormatter.CaseType.TITLE_CASE,
        WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE,
        WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE,
        WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE_ROOT_LOWER_CASE_ENDING,
        WordAnalysisSurfaceFormatter.CaseType.LOWER_CASE,
        WordAnalysisSurfaceFormatter.CaseType.DEFAULT_CASE,
        WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE,
        WordAnalysisSurfaceFormatter.CaseType.UPPER_CASE,
    };

            WordAnalysisSurfaceFormatter formatter = new WordAnalysisSurfaceFormatter();

            int i = 0;
            foreach (string input in inputs)
            {
                Assert.AreEqual(expected[i], formatter.GuessCase(input));
                i++;
            }
        }
    }
}
