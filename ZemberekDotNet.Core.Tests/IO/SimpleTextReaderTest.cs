using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Tests.IO
{
    [TestClass]
    public class SimpleTextReaderTest
    {
        private static readonly string utf8TrWithBOM = "Resources/IO/turkish_utf8_with_BOM.txt";
        private static readonly string multiLineTextFile = "Resources/IO/multi_line_text_file.txt";

        [TestMethod]
        public void TestUtf8()
        {
            string content = new SimpleTextReader(utf8TrWithBOM, "utf-8").AsString();
            Assert.AreEqual(content, "\u015fey");
        }

        [TestMethod]
        public void MultilineTest()
        {
            List<string> list = new SimpleTextReader(multiLineTextFile).AsStringList();
            Assert.AreEqual(list.Count, 17);
            Assert.AreEqual(list[1], "uno");
            //test trim
            Assert.AreEqual(list[2], "  dos");
        }

        [TestMethod]
        public void MultiLineConstraintTest()
        {
            List<string> list = new SimpleTextReader.Builder(multiLineTextFile)
                .AllowMatchingRegexp("^[^#]")
                .IgnoreWhiteSpaceLines()
                .Trim()
                .Build()
                .AsStringList();
            Assert.AreEqual(list.Count, 12);
            Assert.AreEqual(list[0], "uno");
            Assert.AreEqual(list[1], "dos");
        }

        public void TemplateTest()
        {
            SimpleTextReader.Template template = new SimpleTextReader.Template()
                .AllowMatchingRegexp("^[^#]")
                .IgnoreWhiteSpaceLines()
                .Trim();
            string[] files = Directory.GetFiles("blah");
            foreach (string file in files)
            {
                SimpleTextReader sr = template.GenerateReader(file);
                //....
            }
        }

        [TestMethod]
        public void AsStringTest()
        {
            string a = new SimpleTextReader(multiLineTextFile).AsString();
            Console.Write(a);
        }

        [TestMethod]
        public void IterableTest()
        {
            int i = 0;
            foreach (string s in new SimpleTextReader(multiLineTextFile).GetIterableReader())
            {
                if (i == 1)
                {
                    Assert.AreEqual(s.Trim(), "uno");
                }
                if (i == 2)
                {
                    Assert.AreEqual(s.Trim(), "dos");
                }
                if (i == 3)
                {
                    Assert.AreEqual(s.Trim(), "tres");
                }
                i++;
            }
            Assert.AreEqual(i, 17);
        }

        [TestMethod]
        public void LineIteratorTest2()
        {
            LineIterator li = new SimpleTextReader(multiLineTextFile).GetLineIterator();
            while (li.HasNext())
            {
                li.MoveNext();
                Console.WriteLine(li.Current.ToUpper());
            }
        }

        [TestMethod]
        public void LineIteratorWithConstraint()
        {
            LineIterator li = new SimpleTextReader
                .Builder(multiLineTextFile)
                .IgnoreWhiteSpaceLines()
                .Trim()
                .Build().GetLineIterator();

            int i = 0;
            while (li.HasNext())
            {
                li.MoveNext();
                string s = li.Current;
                if (i == 0)
                {
                    Assert.AreEqual(s, "uno");
                }
                if (i == 1)
                {
                    Assert.AreEqual(s, "dos");
                }
                i++;
            }
        }
    }
}
