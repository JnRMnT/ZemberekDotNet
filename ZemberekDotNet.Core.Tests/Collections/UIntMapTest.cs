using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class UIntMapTest
    {
        [TestMethod]
        public void GetTest()
        {
            UIntMap<string> map = new UIntMap<string>(1);
            map.Put(1, "2");
            Assert.AreEqual("2", map.Get(1));
            Assert.IsNull(map.Get(2));
            map.Put(1, "3");
            Assert.AreEqual("3", map.Get(1));

            map = new UIntMap<string>();
            for (int i = 0; i < 100000; i++)
            {
                map.Put(i, Convert.ToString(i + 1));
            }
            for (int i = 0; i < 100000; i++)
            {
                Assert.AreEqual(Convert.ToString(i + 1), map.Get(i));
            }
        }

        [TestMethod]
        public void TestTroubleNumbers()
        {
            int[] troubleNumbers = { 14, 1, 30, 31, 4, 21, 8, 37, 39 };
            UIntMap<string> map = new UIntMap<string>();
            foreach (int number in troubleNumbers)
            {
                map.Put(number, Convert.ToString(number));
            }
            map.Put(15, "15");
            Assert.AreEqual("15", map.Get(15));
        }

        [TestMethod]
        public void StressTest()
        {
            UIntMap<string> map = new UIntMap<string>(1);
            int size = 10000;
            for (int i = 0; i < size; i++)
            {
                map.Put(i, Convert.ToString(i + 1));
            }
            Random rnd = new Random();
            int[] removed = new int[size];
            for (int i = 0; i < 5000; i++)
            {
                int key = rnd.Next(size);
                removed[key] = 1;
                map.Remove(key);
            }

            for (int i = 0; i < size; i++)
            {
                if (removed[i] == 0)
                {
                    Assert.AreEqual(Convert.ToString(i + 1), map.Get(i));
                }
                else
                {
                    Assert.IsFalse(map.ContainsKey(i));
                }
            }

            for (int i = 0; i < 2000; i++)
            {
                int key = rnd.Next(size);
                removed[key] = 0;
                map.Put(key, Convert.ToString(key + 1));
            }

            for (int i = 0; i < size; i++)
            {
                if (removed[i] == 0)
                {
                    Assert.AreEqual(Convert.ToString(i + 1), map.Get(i));
                }
                else
                {
                    Assert.IsFalse(map.ContainsKey(i));
                }
            }
        }

        [TestMethod]
        public void GetValuesTest()
        {
            UIntMap<string> map = new UIntMap<string>();
            int size = 1000;
            List<string> expected = new List<string>();
            for (int i = 0; i < size; i++)
            {
                string value = Convert.ToString(i + 1);
                map.Put(i, value);
                expected.Add(value);
            }
            CollectionAssert.AreEqual(expected, map.GetValuesSortedByKey());
        }

        [TestMethod]
        public void GetValuesTest2()
        {
            UIntMap<string> map = new UIntMap<string>();
            map.Put(1, "a");
            map.Put(5, "b");
            map.Put(12345, "a");
            List<string> values = map.GetValues();
            values.Sort();
            List<string> expected = new List<string> { "a", "a", "b" };
            CollectionAssert.AreEqual(expected, values);
        }

        [TestMethod]
        public void RemoveTest()
        {
            UIntMap<string> map = new UIntMap<string>();
            int count = 10000;
            for (int i = 0; i < count; i++)
            {
                map.Put(i, Convert.ToString(i + 1));
            }
            Assert.AreEqual(count, map.Size());
            int removedCount = 0;
            for (int i = 0; i < count; i += 3)
            {
                map.Remove(i);
                removedCount++;
            }
            Assert.AreEqual(count - removedCount, map.Size());

            for (int i = 0; i < count; i += 3)
            {
                Assert.IsFalse(map.ContainsKey(i));
            }

            for (int i = 0; i < count; i++)
            {
                map.Put(i, Convert.ToString(i + 1));
            }
            Assert.AreEqual(count, map.Size());

            for (int i = 0; i < count; i += 3)
            {
                Assert.IsTrue(map.ContainsKey(i));
            }
        }
    }
}
