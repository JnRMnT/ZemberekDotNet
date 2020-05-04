using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Morphology.Analysis.TR;

namespace ZemberekDotNet.Morphology.Tests.Analysis.TR
{
    [TestClass]
    public class TurkishNumbersTest
    {
        [TestMethod]
        public void CardinalTest()
        {
            Assert.AreEqual("sıfır", TurkishNumbers.ConvertToString(0));
            Assert.AreEqual("bin", TurkishNumbers.ConvertToString(1000));
            Assert.AreEqual("bir", TurkishNumbers.ConvertToString(1));
            Assert.AreEqual("on bir", TurkishNumbers.ConvertToString(11));
            Assert.AreEqual("yüz on bir", TurkishNumbers.ConvertToString(111));
            Assert.AreEqual("yüz on bir bin", TurkishNumbers.ConvertToString(111000));
            Assert.AreEqual(
                "bir milyon iki yüz otuz dört bin beş yüz altmış yedi",
                TurkishNumbers.ConvertToString(1_234_567));
            Assert.AreEqual(
                "bir milyar iki yüz otuz dört milyon beş yüz altmış yedi bin sekiz yüz doksan",
                TurkishNumbers.ConvertToString(1_234_567_890));

        }

        [TestMethod]
        public void CardinalTest2()
        {
            Assert.AreEqual("sıfır", TurkishNumbers.ConvertNumberToString("0"));
            Assert.AreEqual("sıfır sıfır", TurkishNumbers.ConvertNumberToString("00"));
            Assert.AreEqual("sıfır sıfır sıfır", TurkishNumbers.ConvertNumberToString("000"));
            Assert.AreEqual("sıfır sıfır sıfır bir", TurkishNumbers.ConvertNumberToString("0001"));
            Assert.AreEqual("bin", TurkishNumbers.ConvertNumberToString("1000"));
            Assert.AreEqual("bir", TurkishNumbers.ConvertNumberToString("1"));
            Assert.AreEqual("on bir", TurkishNumbers.ConvertNumberToString("11"));
            Assert.AreEqual("yüz on bir", TurkishNumbers.ConvertNumberToString("111"));
            Assert.AreEqual("yüz on bir bin", TurkishNumbers.ConvertNumberToString("111000"));
            Assert.AreEqual("sıfır yüz on bir bin", TurkishNumbers.ConvertNumberToString("0111000"));
            Assert.AreEqual("sıfır sıfır yüz on bir bin",
                TurkishNumbers.ConvertNumberToString("00111000"));
        }

        [TestMethod]
        public void OrdinalTest()
        {
            Assert.AreEqual("sıfırıncı",
                TurkishNumbers.ConvertOrdinalNumberString("0."));
        }

        [TestMethod]
        public void SeparateNumbersTest()
        {
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "H", "12", "A", "5" }
                , TurkishNumbers.SeparateNumbers("H12A5")));
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "F", "16", "'ya" }
                , TurkishNumbers.SeparateNumbers("F16'ya")));
        }

        [TestMethod]
        public void SeparateConnectedNumbersTest()
        {
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "on" }
                , TurkishNumbers.SeperateConnectedNumbers("on")));
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "on", "iki", "bin", "altı", "yüz" }
                , TurkishNumbers.SeperateConnectedNumbers("onikibinaltıyüz")));
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "bir", "iki", "üç" }
                , TurkishNumbers.SeperateConnectedNumbers("birikiüç")));
        }

        [TestMethod]
        public void TestTextToNumber1()
        {
            Assert.AreEqual(11, TurkishNumbers.ConvertToNumber("on bir"));
            Assert.AreEqual(111, TurkishNumbers.ConvertToNumber("yüz on bir"));
            Assert.AreEqual(101, TurkishNumbers.ConvertToNumber("yüz bir"));
            Assert.AreEqual(1000_000, TurkishNumbers.ConvertToNumber("bir milyon"));
            Assert.AreEqual(-1, TurkishNumbers.ConvertToNumber("bir bin"));
        }

        [TestMethod]
        public void RomanNumberTest()
        {
            Assert.AreEqual(-1,
                TurkishNumbers.RomanToDecimal("foo"));
            Assert.AreEqual(-1,
                TurkishNumbers.RomanToDecimal("IIIIIII"));
            Assert.AreEqual(1987,
                TurkishNumbers.RomanToDecimal("MCMLXXXVII"));
        }
    }
}
