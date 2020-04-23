using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class FixedBitVectorTest
    {
        [TestMethod]
        public void SetGetLastBitIndex()
        {
            string s = "101100100 01110110 11001010 11110000 11010100";
            int len = s.Replace(" ", "").Length;
            FixedBitVector vector = FixedBitVector.FromBinaryString(s);

            Assert.AreEqual(len, vector.length);
            Assert.IsTrue(vector.Get(0));
            Assert.IsTrue(vector.Get(2));
            Assert.IsTrue(vector.Get(3));
            Assert.IsTrue(vector.Get(len - 3));
            Assert.IsTrue(vector.Get(len - 5));

            Assert.IsFalse(vector.Get(1));
            Assert.IsFalse(vector.Get(4));
            Assert.IsFalse(vector.Get(5));
            Assert.IsFalse(vector.Get(len - 1));
            Assert.IsFalse(vector.Get(len - 2));

            Assert.AreEqual(21, vector.NumberOfOnes());
            Assert.AreEqual(len - 21, vector.NumberOfZeroes());
        }

        [TestMethod]
        public void getSetClear()
        {
            for (int j = 1; j < 10_000_000; j = j * 2)
            {
                FixedBitVector vector = new FixedBitVector(j);
                for (int i = 0; i < vector.length; i++)
                {
                    Assert.IsFalse(vector.Get(i));
                }
                for (int i = 0; i < vector.length; i++)
                {
                    vector.Set(i);
                }
                for (int i = 0; i < vector.length; i++)
                {
                    Assert.IsTrue(vector.Get(i));
                }
                for (int i = 0; i < vector.length; i++)
                {
                    vector.Clear(i);
                }
                for (int i = 0; i < vector.length; i++)
                {
                    Assert.IsFalse(vector.Get(i));
                }
            }
        }

        [TestMethod]
        public void SafeGetSetClear()
        {
            for (int j = 1; j < 10_000_000; j = j * 2)
            {
                FixedBitVector vector = new FixedBitVector(j);
                for (int i = 0; i < vector.length; i++)
                {
                    Assert.IsFalse(vector.SafeGet(i));
                }
                for (int i = 0; i < vector.length; i++)
                {
                    vector.SafeSet(i);
                }
                for (int i = 0; i < vector.length; i++)
                {
                    Assert.IsTrue(vector.SafeGet(i));
                }
                for (int i = 0; i < vector.length; i++)
                {
                    vector.SafeClear(i);
                }
                for (int i = 0; i < vector.length; i++)
                {
                    Assert.IsFalse(vector.SafeGet(i));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SafeGet()
        {
            FixedBitVector vector = new FixedBitVector(10);
            vector.SafeGet(10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SafeSet()
        {
            FixedBitVector vector = new FixedBitVector(10);
            vector.SafeSet(10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SafeClear()
        {
            FixedBitVector vector = new FixedBitVector(10);
            vector.SafeClear(10);
        }

        [TestMethod]
        [Ignore("Not a test.")]
        public void PerformanceTest()
        {
            int itCount = 5;
            Random rnd = new Random(Convert.ToInt32("0xbeefcafe", 16));
            int size = 20_000_000;
            int[] oneIndexes = new int[size];
            int k = 0;
            for (int i = 0; i < oneIndexes.Length; i++)
            {
                if (rnd.NextDouble() > 0.33)
                {
                    oneIndexes[k] = i;
                    k++;
                }
            }
            FixedBitVector vector = new FixedBitVector(size);
            int[] copiedArray = null;
            Array.Copy(oneIndexes, copiedArray, k);
            for (int i = 0; i < itCount; i++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                foreach (int oneIndex in oneIndexes)
                {
                    vector.Set(oneIndex);
                }
                foreach (int oneIndex in oneIndexes)
                {
                    vector.Clear(oneIndex);
                }
                Console.WriteLine(sw.ElapsedMilliseconds);
                sw.Stop();
            }
        }
    }
}
