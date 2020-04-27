using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.Core.Turkish;

namespace ZemberekDotNet.Core.Tests.Turkish
{
    [TestClass]
    public class TurkishAlphabetTest
    {
        [TestMethod]
        public void IsVowelTest()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string vowels = "aeiuüıoöâîû";
            foreach (char c in vowels.ToCharArray())
            {
                Assert.IsTrue(alphabet.IsVowel(c));
            }
            string nonvowels = "bcçdfgğjklmnprştvxwzq.";
            foreach (char c in nonvowels.ToCharArray())
            {
                Assert.IsFalse(alphabet.IsVowel(c));
            }
        }

        [TestMethod]
        public void VowelCountTest()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string[] entries = { "a", "aa", "", "bb", "bebaba" };
            int[] expCounts = { 1, 2, 0, 0, 3 };
            int i = 0;
            foreach (string entry in entries)
            {
                Assert.AreEqual(expCounts[i++], alphabet.VowelCount(entry));
            }
        }

        [TestMethod]
        public void VoiceTest()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string iStr = "çÇgGkKpPtTaAbB";
            string oStr = "cCğĞğĞbBdDaAbB";
            for (int i = 0; i < iStr.Length; i++)
            {
                char charInIndex = iStr[i];
                char outExpected = oStr[i];
                Assert.AreEqual(outExpected.ToString(),
                    alphabet.Voice(charInIndex).ToString(),
                    "");
            }
        }

        [TestMethod]
        public void DevoiceTest()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string iStr = "bBcCdDgGğĞaAkK";
            string oStr = "pPçÇtTkKkKaAkK";
            for (int i = 0; i < iStr.Length; i++)
            {
                char charInIndex = iStr[i];
                char outExpected = oStr[i];
                Assert.AreEqual(outExpected.ToString(),
                    alphabet.devoice(charInIndex).ToString(),"");
            }
        }

        [TestMethod]
        public void CircumflexTest()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string iStr = "abcâîûÂÎÛ fg12";
            string oStr = "abcaiuAİU fg12";
            Assert.AreEqual(oStr, alphabet.NormalizeCircumflex(iStr));
        }

        [TestMethod]
        public void ToAsciiTest()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string iStr = "abcçğıiİIoöüşâîûÂÎÛz";
            string oStr = "abccgiiIIoousaiuAIUz";
            Assert.AreEqual(oStr, alphabet.ToAscii(iStr));
        }

        [TestMethod]
        public void EqualsIgnoreDiacritics()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string[] a = { "siraci", "ağac", "ağaç" };
            string[] b = { "şıracı", "ağaç", "agac" };
            for (int i = 0; i < a.Length; i++)
            {
                Assert.IsTrue(alphabet.EqualsIgnoreDiacritics(a[i], b[i]));
            }
        }

        [TestMethod]
        public void VowelHarmonyA()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string[] a = { "elma", "kedi", "turp" };
            string[] b = { "lar", "cik", "un" };
            for (int i = 0; i < a.Length; i++)
            {
                Assert.IsTrue(alphabet.CheckVowelHarmonyA(a[i], b[i]));
            }
        }

        [TestMethod]
        public void VowelHarmonyA2()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string[] a = { "elma", "kedi", "turp" };
            string[] b = { "ler", "cık", "in" };
            for (int i = 0; i < a.Length; i++)
            {
                Assert.IsFalse(alphabet.CheckVowelHarmonyA(a[i], b[i]));
            }
        }

        [TestMethod]
        public void VowelHarmonyI1()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string[] a = { "elma", "kedi", "turp" };
            string[] b = { "yı", "yi", "u" };
            for (int i = 0; i < a.Length; i++)
            {
                Assert.IsTrue(alphabet.CheckVowelHarmonyI(a[i], b[i]));
            }
        }

        [TestMethod]
        public void VowelHarmonyI2()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string[] a = { "elma", "kedi", "turp" };
            string[] b = { "yu", "yü", "ı" };
            for (int i = 0; i < a.Length; i++)
            {
                Assert.IsFalse(alphabet.CheckVowelHarmonyI(a[i], b[i]));
            }
        }

        [TestMethod]
        public void StartsWithDiacriticsIgnoredTest()
        {
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            string[] a = { "siraci", "çağlayan" };
            string[] b = { "şıracı", "cag" };
            for (int i = 0; i < a.Length; i++)
            {
                Assert.IsTrue(alphabet.StartsWithIgnoreDiacritics(a[i], b[i]));
            }
        }
    }
}
