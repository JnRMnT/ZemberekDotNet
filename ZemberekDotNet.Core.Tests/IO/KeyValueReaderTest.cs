using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Tests.IO
{
    [TestClass]
    public class KeyValueReaderTest
    {
        [TestMethod]
        public void TestReader()
        {
            Dictionary<string, string> map = new KeyValueReader(":").LoadFromFile("Resources/IO/key-value-colon-separator.txt");
            Assert.AreEqual(map.Count, 4);
            Assert.IsTrue(TestUtil.ContainsAllKeys(map, "1", "2", "3", "4"));
            Assert.IsTrue(TestUtil.ContainsAllValues(map, "bir", "iki", "uc", "dort"));
        }
    }
}
