using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Tests.IO
{
    [TestClass]
    public class SimpleTextWriterTest
    {
        private string tmpDir;
        private string tmpFile;

        [TestInitialize]
        public void TestInitializer()
        {
            tmpDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tmpDir);
            tmpFile = Path.Combine(tmpDir, "jcaki.txt");
        }

        [TestCleanup]
        public void TestCleaner()
        {
            File.Delete(tmpFile);
            Directory.Delete(tmpDir, true);
        }

        [TestMethod]
        public void WriteStringTest()
        {
            new SimpleTextWriter(tmpFile).Write("Hello World!");
            Assert.AreEqual(new SimpleTextReader(tmpFile).AsString(), "Hello World!");
            new SimpleTextWriter(tmpFile).Write(null);
            Assert.AreEqual(new SimpleTextReader(tmpFile).AsString(), "");
            new SimpleTextWriter(tmpFile).Write("");
            Assert.AreEqual(new SimpleTextReader(tmpFile).AsString(), "");
        }

        [TestMethod]
        [Ignore]
        public void WriteStringKeepOpenTest()
        {
            //TODO:Check
            SimpleTextWriter sfw = new SimpleTextWriter
                .Builder(tmpFile)
                .KeepOpen()
                .Build();

            sfw.Write("Hello");
            sfw.Write("Merhaba");
            sfw.Write("");
            sfw.Write(null);

            Assert.AreEqual("HelloMerhaba", new SimpleTextReader(sfw.GetFileStream()).AsString());
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void KeepOpenExcepionTest()
        {
            SimpleTextWriter sfw = new SimpleTextWriter
                .Builder(tmpFile)
                .Build();
            sfw.Write("Hello");
            sfw.Write("Now it will throw an exception..");
        }

        [TestMethod]
        public void WriteMultiLineStringTest()
        {
            List<String> strs = new List<String> { "Merhaba", "Dunya", "" };
            new SimpleTextWriter(tmpFile).WriteLines(strs);
            List<String> read = new SimpleTextReader(tmpFile).AsStringList();
            for (int i = 0; i < read.Count; i++)
            {
                Assert.AreEqual(read[i], strs[i]);
            }
        }
    }
}
