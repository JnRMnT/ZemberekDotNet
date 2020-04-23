using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class FloatValueMapTest
    {
        [TestMethod]
        public void TestValues()
        {
            FloatValueMap<string> set = new FloatValueMap<string>();
            set.Set("a", 7);
            set.Set("b", 2);
            set.Set("c", 3);
            set.Set("d", 4);
            set.Set("d", 5); // overwrite

            Assert.AreEqual(4, set.Size());
            float[] values = set.Values();
            Array.Sort(values);
            Assert.IsTrue(Enumerable.SequenceEqual(new float[] { 2f, 3f, 5f, 7f }, values));
        }
    }
}
