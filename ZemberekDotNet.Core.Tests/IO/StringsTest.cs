using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.IO;

/// <summary>
/// some parts are copied from commons-lang
/// </summary>
namespace ZemberekDotNet.Core.Tests.IO
{
    [TestClass]
    public class StringsTest
    {
        //    ~~~~~~~~~~~ Strings.IsNullOrEmpty ~~~~~~~~~~~~~~
        [TestMethod]
        public void IsEmptyTest()
        {
            Assert.IsTrue(Strings.IsNullOrEmpty(null));
            Assert.IsTrue(Strings.IsNullOrEmpty(""));
            Assert.IsFalse(Strings.IsNullOrEmpty("\n"));
            Assert.IsFalse(Strings.IsNullOrEmpty("\t"));
            Assert.IsFalse(Strings.IsNullOrEmpty(" "));
            Assert.IsFalse(Strings.IsNullOrEmpty("a"));
            Assert.IsFalse(Strings.IsNullOrEmpty("as"));
        }

        //    ~~~~~~~~~~~ Strings.HasText ~~~~~~~~~~~~~~
        [TestMethod]
        public void HasTextTest()
        {
            Assert.IsFalse(Strings.HasText(null));
            Assert.IsTrue(Strings.HasText("a"));
            Assert.IsTrue(Strings.HasText("abc"));
            Assert.IsFalse(Strings.HasText(""));
            Assert.IsFalse(Strings.HasText(null));
            Assert.IsFalse(Strings.HasText(" "));
            Assert.IsFalse(Strings.HasText("\t"));
            Assert.IsFalse(Strings.HasText("\n"));
            Assert.IsFalse(Strings.HasText(" \t"));
        }

        [TestMethod]
        public void TestIfAllHasText()
        {
            Assert.IsTrue(Strings.AllHasText("fg", "a", "hyh"));
            Assert.IsFalse(Strings.AllHasText("fg", null, "hyh"));
            Assert.IsFalse(Strings.AllHasText("fg", " ", "hyh"));
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIfAllHasTextExceptionIAE()
        {
            Strings.AllHasText();
        }

        [TestMethod]
        public void TestAllEmpty()
        {
            Assert.IsTrue(Strings.AllNullOrEmpty("", "", null));
            Assert.IsFalse(Strings.AllNullOrEmpty("", null, "hyh"));
            Assert.IsFalse(Strings.AllNullOrEmpty(" ", "", ""));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAllEmptyExceptionIAE()
        {
            Strings.AllNullOrEmpty();
        }

        //    ~~~~~~~~~~~ Strings.LeftTrim ~~~~~~~~~~~~~~

        [TestMethod]
        public void LeftTrimTest()
        {
            Assert.IsNull(Strings.LeftTrim(null));
            Assert.AreEqual(Strings.LeftTrim(""), "");
            Assert.AreEqual(Strings.LeftTrim(" \t "), "");
            Assert.AreEqual(Strings.LeftTrim(" 123"), "123");
            Assert.AreEqual(Strings.LeftTrim("\t123"), "123");
            Assert.AreEqual(Strings.LeftTrim("\n123"), "123");
            Assert.AreEqual(Strings.LeftTrim("123"), "123");
            Assert.AreEqual(Strings.LeftTrim(" \n  123"), "123");
            Assert.AreEqual(Strings.LeftTrim("123 "), "123 ");
            Assert.AreEqual(Strings.LeftTrim(" 3 123 "), "3 123 ");
        }

        //    ~~~~~~~~~~~ Strings.RightTrim ~~~~~~~~~~~~~~

        [TestMethod]
        public void RightTrimTest()
        {
            Assert.IsNull(Strings.RightTrim(null));
            Assert.AreEqual(Strings.RightTrim(""), "");
            Assert.AreEqual(Strings.RightTrim(" \t"), "");
            Assert.AreEqual(Strings.RightTrim("aaa "), "aaa");
            Assert.AreEqual(Strings.RightTrim("aaa  \t "), "aaa");
            Assert.AreEqual(Strings.RightTrim("aaa\n "), "aaa");
            Assert.AreEqual(Strings.RightTrim("aaa"), "aaa");
            Assert.AreEqual(Strings.RightTrim(" 123 "), " 123");
            Assert.AreEqual(Strings.RightTrim(" 3 123 \t"), " 3 123");
        }

        //    ~~~~~~~~~~~ Strings.Repeat ~~~~~~~~~~~~~~

        [TestMethod]
        public void RepeatTest()
        {
            Assert.AreEqual(Strings.Repeat('c', -1), "");
            Assert.AreEqual(Strings.Repeat('c', 3), "ccc");
            Assert.AreEqual(Strings.Repeat('c', 1), "c");
            Assert.AreEqual(Strings.Repeat('c', 0), "");

            Assert.IsNull(Strings.Repeat(null, 1));
            Assert.AreEqual(Strings.Repeat("ab", -1), "");
            Assert.AreEqual(Strings.Repeat("ab", 3), "ababab");
            Assert.AreEqual(Strings.Repeat("ab", 1), "ab");
            Assert.AreEqual(Strings.Repeat("ab", 0), "");
        }

        //    ~~~~~~~~~~~ Strings.Reverse ~~~~~~~~~~~~~~

        [TestMethod]
        public void ReverseTest()
        {
            Assert.IsNull(Strings.Reverse(null), null);
            Assert.AreEqual(Strings.Reverse(""), "");
            Assert.AreEqual(Strings.Reverse("a"), "a");
            Assert.AreEqual(Strings.Reverse("ab"), "ba");
            Assert.AreEqual(Strings.Reverse("ab cd "), " dc ba");
        }

        //    ~~~~~~~~~~~ Strings.InsertFromLeft ~~~~~~~~~~~~~~

        [TestMethod]
        public void InsertFromLeftTest()
        {
            string s = "0123456789";
            Assert.AreEqual(Strings.InsertFromLeft(s, 0, "-"), "0123456789");
            Assert.AreEqual(Strings.InsertFromLeft(s, 1, "-"), "0-1-2-3-4-5-6-7-8-9");
            Assert.AreEqual(Strings.InsertFromLeft("ahmet", 1, " "), "a h m e t");
            Assert.AreEqual(Strings.InsertFromLeft(s, 2, "-"), "01-23-45-67-89");
            Assert.AreEqual(Strings.InsertFromLeft(s, 3, "-"), "012-345-678-9");
            Assert.AreEqual(Strings.InsertFromLeft(s, 5, "-"), "01234-56789");
            Assert.AreEqual(Strings.InsertFromLeft(s, 6, "-"), "012345-6789");
            Assert.AreEqual(Strings.InsertFromLeft(s, 9, "-"), "012345678-9");
            Assert.AreEqual(Strings.InsertFromLeft(s, 10, "-"), "0123456789");
            Assert.AreEqual(Strings.InsertFromLeft(s, 12, "-"), "0123456789");
            Assert.AreEqual(Strings.InsertFromLeft(s, 2, "--"), "01--23--45--67--89");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertFromLeftExceptionTest2()
        {
            Strings.InsertFromLeft("123", -1, "-");
        }

        //    ~~~~~~~~~~~ Strings.InsertFromRight ~~~~~~~~~~~~~~

        [TestMethod]
        public void InsertFromRightTest()
        {
            string s = "0123456789";
            Assert.AreEqual(Strings.InsertFromRight(s, 0, "-"), "0123456789");
            Assert.AreEqual(Strings.InsertFromRight(s, 1, "-"), "0-1-2-3-4-5-6-7-8-9");
            Assert.AreEqual(Strings.InsertFromRight(s, 2, "-"), "01-23-45-67-89");
            Assert.AreEqual(Strings.InsertFromRight(s, 3, "-"), "0-123-456-789");
            Assert.AreEqual(Strings.InsertFromRight(s, 5, "-"), "01234-56789");
            Assert.AreEqual(Strings.InsertFromRight(s, 6, "-"), "0123-456789");
            Assert.AreEqual(Strings.InsertFromRight(s, 9, "-"), "0-123456789");
            Assert.AreEqual(Strings.InsertFromRight(s, 10, "-"), "0123456789");
            Assert.AreEqual(Strings.InsertFromRight(s, 12, "-"), "0123456789");
            Assert.AreEqual(Strings.InsertFromRight(s, 2, "--"), "01--23--45--67--89");
            Assert.AreEqual(Strings.InsertFromRight(s, 3, "--"), "0--123--456--789");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InsertFromRightExceptionTest2()
        {
            Strings.InsertFromRight("123", -1, "-");
        }

        // ------------ Tests below is taken from commons logging ---------------------------

        [TestMethod]
        public void TestRightPad_StringInt()
        {
            Assert.AreEqual(null, Strings.RightPad(null, 5));
            Assert.AreEqual("     ", Strings.RightPad("", 5));
            Assert.AreEqual("abc  ", Strings.RightPad("abc", 5));
            Assert.AreEqual("abc", Strings.RightPad("abc", 2));
            Assert.AreEqual("abc", Strings.RightPad("abc", -1));
        }

        [TestMethod]
        public void TestRightPad_StringIntChar()
        {
            Assert.AreEqual(null, Strings.RightPad(null, 5, ' '));
            Assert.AreEqual("     ", Strings.RightPad("", 5, ' '));
            Assert.AreEqual("abc  ", Strings.RightPad("abc", 5, ' '));
            Assert.AreEqual("abc", Strings.RightPad("abc", 2, ' '));
            Assert.AreEqual("abc", Strings.RightPad("abc", -1, ' '));
            Assert.AreEqual("abcxx", Strings.RightPad("abc", 5, 'x'));
            String str = Strings.RightPad("aaa", 10000, 'a');  // bigger than pad length
            Assert.AreEqual(10000, str.Length);
        }

        [TestMethod]
        public void TestRightPad_StringIntString()
        {
            Assert.AreEqual(null, Strings.RightPad(null, 5, "-+"));
            Assert.AreEqual("     ", Strings.RightPad("", 5, " "));
            Assert.AreEqual(null, Strings.RightPad(null, 8, null));
            Assert.AreEqual("abc-+-+", Strings.RightPad("abc", 7, "-+"));
            Assert.AreEqual("abc-+~", Strings.RightPad("abc", 6, "-+~"));
            Assert.AreEqual("abc-+", Strings.RightPad("abc", 5, "-+~"));
            Assert.AreEqual("abc", Strings.RightPad("abc", 2, " "));
            Assert.AreEqual("abc", Strings.RightPad("abc", -1, " "));
            Assert.AreEqual("abc  ", Strings.RightPad("abc", 5, null));
            Assert.AreEqual("abc  ", Strings.RightPad("abc", 5, ""));
        }

        [TestMethod]
        public void TestLeftPad_StringInt()
        {
            Assert.AreEqual(null, Strings.LeftPad(null, 5));
            Assert.AreEqual("     ", Strings.LeftPad("", 5));
            Assert.AreEqual("  abc", Strings.LeftPad("abc", 5));
            Assert.AreEqual("abc", Strings.LeftPad("abc", 2));
        }

        [TestMethod]
        public void TestLeftPad_StringIntChar()
        {
            Assert.AreEqual(null, Strings.LeftPad(null, 5, ' '));
            Assert.AreEqual("     ", Strings.LeftPad("", 5, ' '));
            Assert.AreEqual("  abc", Strings.LeftPad("abc", 5, ' '));
            Assert.AreEqual("xxabc", Strings.LeftPad("abc", 5, 'x'));
            Assert.AreEqual("\uffff\uffffabc", Strings.LeftPad("abc", 5, '\uffff'));
            Assert.AreEqual("abc", Strings.LeftPad("abc", 2, ' '));
            String str = Strings.LeftPad("aaa", 10000, 'a');  // bigger than pad length
            Assert.AreEqual(10000, str.Length);
        }

        [TestMethod]
        public void TestLeftPad_StringIntString()
        {
            Assert.AreEqual(null, Strings.LeftPad(null, 5, "-+"));
            Assert.AreEqual(null, Strings.LeftPad(null, 5, null));
            Assert.AreEqual("     ", Strings.LeftPad("", 5, " "));
            Assert.AreEqual("-+-+abc", Strings.LeftPad("abc", 7, "-+"));
            Assert.AreEqual("-+~abc", Strings.LeftPad("abc", 6, "-+~"));
            Assert.AreEqual("-+abc", Strings.LeftPad("abc", 5, "-+~"));
            Assert.AreEqual("abc", Strings.LeftPad("abc", 2, " "));
            Assert.AreEqual("abc", Strings.LeftPad("abc", -1, " "));
            Assert.AreEqual("  abc", Strings.LeftPad("abc", 5, null));
            Assert.AreEqual("  abc", Strings.LeftPad("abc", 5, ""));
        }

        [TestMethod]
        public void TestWhiteSpacesToSingleSpace()
        {
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace(null), null);
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace(""), "");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace("asd"), "asd");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace("a  a"), "a a");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace(" "), " ");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace("\t"), " ");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace("\n"), " ");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace("\t \n"), " ");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace("  \t  \n\r \f"), " ");
            Assert.AreEqual(Strings.WhiteSpacesToSingleSpace("  a\t a\r\fa"), " a a a");
        }

        [TestMethod]
        public void TestEliminateWhiteSpaces()
        {
            Assert.AreEqual(Strings.EliminateWhiteSpaces(null), null);
            Assert.AreEqual(Strings.EliminateWhiteSpaces(""), "");
            Assert.AreEqual(Strings.EliminateWhiteSpaces("asd"), "asd");
            Assert.AreEqual(Strings.EliminateWhiteSpaces("a "), "a");
            Assert.AreEqual(Strings.EliminateWhiteSpaces("a  a "), "aa");
            Assert.AreEqual(Strings.EliminateWhiteSpaces("a \t a \t\r\f"), "aa");
        }

        [TestMethod]
        public void TestSubstringAfterFirst()
        {
            Assert.AreEqual(Strings.SubstringAfterFirst("hello", "el"), "lo");
            Assert.AreEqual(Strings.SubstringAfterFirst("hellohello", "el"), "lohello");
            Assert.AreEqual(Strings.SubstringAfterFirst("hello", "hello"), "");
            Assert.AreEqual(Strings.SubstringAfterFirst("hello", ""), "hello");
            Assert.AreEqual(Strings.SubstringAfterFirst("hello", null), "hello");
            Assert.AreEqual(Strings.SubstringAfterFirst("", "el"), "");
            Assert.AreEqual(Strings.SubstringAfterFirst(null, "el"), null);
        }

        [TestMethod]
        public void TestSubstringAfterLast()
        {
            Assert.AreEqual(Strings.substringAfterLast("hello\\world", "\\"), "world");
            Assert.AreEqual(Strings.substringAfterLast("hello", "el"), "lo");
            Assert.AreEqual(Strings.substringAfterLast("hellohello", "el"), "lo");
            Assert.AreEqual(Strings.substringAfterLast("hello", "hello"), "");
            Assert.AreEqual(Strings.substringAfterLast("hello", ""), "hello");
            Assert.AreEqual(Strings.substringAfterLast("hello", null), "hello");
            Assert.AreEqual(Strings.substringAfterLast("", "el"), "");
            Assert.AreEqual(Strings.substringAfterLast(null, "el"), null);
        }

        [TestMethod]
        public void TestSubstringUntilFirst()
        {
            Assert.AreEqual(Strings.SubstringUntilFirst("hello", "el"), "h");
            Assert.AreEqual(Strings.SubstringUntilFirst("hellohello", "el"), "h");
            Assert.AreEqual(Strings.SubstringUntilFirst("hello", "hello"), "");
            Assert.AreEqual(Strings.SubstringUntilFirst("hello", ""), "hello");
            Assert.AreEqual(Strings.SubstringUntilFirst("hello", null), "hello");
            Assert.AreEqual(Strings.SubstringUntilFirst("", "el"), "");
            Assert.AreEqual(Strings.SubstringUntilFirst(null, "el"), null);
        }

        [TestMethod]
        public void TestSubstringUntilLast()
        {
            Assert.AreEqual(Strings.SubstringUntilLast("hello", "el"), "h");
            Assert.AreEqual(Strings.SubstringUntilLast("hellohello", "el"), "helloh");
            Assert.AreEqual(Strings.SubstringUntilLast("hello", "hello"), "");
            Assert.AreEqual(Strings.SubstringUntilLast("hello", ""), "hello");
            Assert.AreEqual(Strings.SubstringUntilLast("hello", null), "hello");
            Assert.AreEqual(Strings.SubstringUntilLast("", "el"), "");
            Assert.AreEqual(Strings.SubstringUntilLast(null, "el"), null);
        }

        [TestMethod]
        public void TestGrams()
        {
            Assert.IsTrue(Enumerable.SequenceEqual(Strings.SeparateGrams("hello", 1), new String[] { "h", "e", "l", "l", "o" }));
            Assert.IsTrue(Enumerable.SequenceEqual(Strings.SeparateGrams("hello", 2), new String[] { "he", "el", "ll", "lo" }));
            Assert.IsTrue(Enumerable.SequenceEqual(Strings.SeparateGrams("hello", 3), new String[] { "hel", "ell", "llo" }));
            Assert.IsTrue(Enumerable.SequenceEqual(Strings.SeparateGrams("hello", 4), new String[] { "hell", "ello" }));
            Assert.IsTrue(Enumerable.SequenceEqual(Strings.SeparateGrams("hello", 5), new String[] { "hello" }));
            Assert.IsTrue(Enumerable.SequenceEqual(Strings.SeparateGrams("hello", 6), Strings.EmptyStringArray));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Gram0sizeExceptionTest()
        {
            Strings.SeparateGrams("123", 0);
        }
    }
}
