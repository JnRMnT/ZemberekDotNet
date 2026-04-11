using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class UIntSetTest
    {
        [TestMethod]
        public void ContainsSet()
        {
            UIntSet set = new UIntSet();
            for (int i = 0; i < 100000; i++)
            {
                set.Add(i);
            }
            for (int i = 0; i < 200000; i++)
            {
                if (i < 100000)
                {
                    Assert.IsTrue(set.Contains(i));
                }
                else
                {
                    Assert.IsFalse(set.Contains(i));
                }
            }
        }

        [TestMethod]
        public void StressTest()
        {
            UIntSet set = new UIntSet();
            int size = 10000;
            for (int i = 0; i < size; i++)
            {
                set.Add(i);
            }
            Random rnd = new Random();
            int[] removed = new int[size];
            for (int i = 0; i < 5000; i++)
            {
                int key = rnd.Next(size);
                removed[key] = 1;
                set.Remove(key);
            }

            for (int i = 0; i < size; i++)
            {
                if (removed[i] == 0)
                {
                    Assert.IsTrue(set.Contains(i));
                }
                else
                {
                    Assert.IsFalse(set.Contains(i));
                }
            }

            for (int i = 0; i < 2000; i++)
            {
                int key = rnd.Next(size);
                removed[key] = 0;
                set.Add(key);
            }

            for (int i = 0; i < size; i++)
            {
                if (removed[i] == 0)
                {
                    Assert.IsTrue(set.Contains(i));
                }
                else
                {
                    Assert.IsFalse(set.Contains(i));
                }
            }
        }

        [TestMethod]
        public void RemoveTest()
        {
            UIntSet set = new UIntSet();
            int count = 1000;
            for (int i = 0; i < count; i++)
            {
                set.Add(i);
            }
            Assert.AreEqual(count, set.Size());
            int removedCount = 0;
            for (int i = 0; i < count; i += 3)
            {
                set.Remove(i);
                removedCount++;
            }
            Assert.AreEqual(count - removedCount, set.Size());

            for (int i = 0; i < count; i += 3)
            {
                Assert.IsFalse(set.Contains(i));
            }

            for (int i = 0; i < count; i++)
            {
                set.Add(i);
            }
            Assert.AreEqual(count, set.Size());

            for (int i = 0; i < count; i += 3)
            {
                Assert.IsTrue(set.Contains(i));
            }
        }
    }
}
