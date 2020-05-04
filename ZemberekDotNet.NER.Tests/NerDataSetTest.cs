using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ZemberekDotNet.Core.IO;
using static ZemberekDotNet.NER.NerDataSet;

namespace ZemberekDotNet.NER.Tests
{
    [TestClass]
    public class NerDataSetTest
    {
        [TestMethod]
        public void TestOpenNlpStyle()
        {
            string p = TestUtil.TempFileWithData(
        "<Start:ABC> Foo Bar <End> ivir zivir <Start:DEF> haha <End> . ");
            NerDataSet set = NerDataSet.Load(p, AnnotationStyle.OPEN_NLP);
            Console.WriteLine("types= " + set.Types);
            Assert.IsTrue(TestUtil.ContainsAll(set.Types, "ABC", "DEF", "OUT"));
        }

        [TestMethod]
        public void TestBracketStyle()
        {
            string p = TestUtil.TempFileWithData(
            "[ABC Foo Bar] ivir zivir [DEF haha] . ");
            NerDataSet set = NerDataSet.Load(p, AnnotationStyle.BRACKET);
            Console.WriteLine("types= " + set.Types);
            Assert.IsTrue(TestUtil.ContainsAll(set.Types, "ABC", "DEF", "OUT"));
        }
    }
}
