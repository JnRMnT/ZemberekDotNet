using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Core.Tests.Text
{
    [TestClass]
    public class BlockTextLoaderTest
    {
        [TestMethod]
        public void LoadTest1()
        {
            List<string> lines = new List<string>();
            int i;
            for (i = 0; i < 10000; i++)
            {
                lines.Add(i.ToString());
            }
            string path = TestUtil.TempFileWithData(lines);

            BlockTextLoader loader = BlockTextLoader.FromPath(path, 1000);
            i = 0;
            List<string> read = new List<string>();
            foreach (TextChunk block in loader)
            {
                i++;
                read.AddRange(block.GetData());
            }

            Assert.AreEqual(i, 10);
            Assert.IsTrue(Enumerable.SequenceEqual(lines, read));

            loader = BlockTextLoader.FromPath(path, 1001);

            i = 0;
            read = new List<string>();
            foreach (TextChunk block in loader)
            {
                i++;
                read.AddRange(block.GetData());
            }

            Assert.AreEqual(i, 10);
            Assert.IsTrue(Enumerable.SequenceEqual(lines, read));

            loader = BlockTextLoader.FromPath(path, 100000);

            i = 0;
            read = new List<string>();
            foreach (TextChunk block in loader)
            {
                i++;
                read.AddRange(block.GetData());
            }

            Assert.AreEqual(i, 1);
            Assert.IsTrue(Enumerable.SequenceEqual(lines, read));

            File.Delete(path);
        }
    }
}
