using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Tokenization.Tests
{
    [TestClass]
    public class SpanTest
    {
        [TestMethod]
        public void ShouldNotThrowException()
        {
            try
            {
                new Span(0, 0);
                new Span(1, 1);
                new Span(1, 5);
                new Span(int.MaxValue, int.MaxValue);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadInitialization1()
        {
            new Span(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadInitialization2()
        {
            new Span(0, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadInitialization3()
        {
            new Span(1, 0);
        }

        [TestMethod]
        public void SubstringTest()
        {
            Assert.AreEqual("", new Span(0, 0).GetSubstring("hello"));
            Assert.AreEqual("h", new Span(0, 1).GetSubstring("hello"));
            Assert.AreEqual("ello", new Span(1, 5).GetSubstring("hello"));
        }

        [TestMethod]
        public void LengthTest()
        {
            Assert.AreEqual(0, new Span(0, 0).Length());
            Assert.AreEqual(1, new Span(0, 1).Length());
            Assert.AreEqual(4, new Span(1, 5).Length());
        }
    }
}
