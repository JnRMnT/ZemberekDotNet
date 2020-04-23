using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class LongBitVectorTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitializationNegative()
        {
            new LongBitVector(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InitializationOutOfBOund()
        {
            new LongBitVector(int.MaxValue * 64L + 1L);
        }

        [TestMethod]
        public void GetSetTest()
        {
            long[] setBits = { 1, 2, 3, 7, 9, 21, 35, 56, 63, 64, 88, 99, 101, 500, 700, 999 };
            LongBitVector vector = new LongBitVector(1000);
            foreach (long setBit in setBits)
            {
                vector.Set(setBit);
            }

            for (int i = 0; i < 1000; i++)
            {
                if (Array.BinarySearch(setBits, i) >= 0)
                {
                    Assert.IsTrue(vector.Get(i));
                }
                else
                {
                    Assert.IsFalse(vector.Get(i));
                }
            }
        }

        [TestMethod]
        public void SetTest2()
        {
            String s = "101100100 01110110 11001010 11111001 01001110 10111011 01111010 11010100";
            LongBitVector vector = LongBitVector.FromBinaryString(s);
            s = s.Replace(" ", "");
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '1')
                {
                    Assert.IsTrue(vector.Get(i));
                }
                else
                {
                    Assert.IsFalse(vector.Get(i));
                }
            }
        }

        [TestMethod]
        public void SetGetLastBitIndex()
        {
            String s = "101100100 01110110 11001010 11111001 01001110 10111011 01111010 11010100";
            LongBitVector vector = LongBitVector.FromBinaryString(s);
            s = s.Replace(" ", "");
            Assert.AreEqual(s.Length - 3, vector.GetLastBitIndex(true));
            Assert.AreEqual(s.Length - 1, vector.GetLastBitIndex(false));
            vector = LongBitVector.FromBinaryString("000011");
            Assert.AreEqual(5, vector.GetLastBitIndex(true));
            Assert.AreEqual(3, vector.GetLastBitIndex(false));
            vector = LongBitVector.FromBinaryString("0000");
            Assert.AreEqual(-1, vector.GetLastBitIndex(true));
            Assert.AreEqual(3, vector.GetLastBitIndex(false));
            vector = LongBitVector.FromBinaryString("1111");
            Assert.AreEqual(-1, vector.GetLastBitIndex(false));
            Assert.AreEqual(3, vector.GetLastBitIndex(true));
        }


        [TestMethod]
        public void GetResetTest()
        {
            long[] resetBits = { 1, 2, 3, 7, 9, 21, 35, 56, 63, 64, 88, 99, 101, 500, 700, 999 };
            LongBitVector vector = new LongBitVector(1000);
            vector.Add(1000, true);
            foreach (long resetBit in resetBits)
            {
                vector.Clear(resetBit);
            }

            for (int i = 0; i < 1000; i++)
            {
                if (Array.BinarySearch(resetBits, i) >= 0)
                {
                    Assert.IsFalse(vector.Get(i));
                }
                else
                {
                    Assert.IsTrue(vector.Get(i));
                }
            }
        }

        [TestMethod]
        public void AppendTestBoolean()
        {
            LongBitVector vector = new LongBitVector(0);
            vector.Add(true);
            vector.Add(true);
            vector.Add(false);
            vector.Add(true);
            Assert.AreEqual(4, vector.Size());
            for (int i = 0; i < 1000; i++)
            {
                vector.Add(true);
            }
            Assert.AreEqual(1004, vector.Size());
        }

        [TestMethod]
        public void AppendTestInteger()
        {

            LongBitVector vector = new LongBitVector(0);
            vector.Add(0x0000ffff, 16);
            Assert.AreEqual(16, vector.Size());
            vector.Add(0x0000ffff, 32);
            Assert.AreEqual(48, vector.Size());

            LongBitVector vector2 = new LongBitVector(64);
            vector2.Add(64, false);
            vector2.Add(0xff, 6);
            Assert.AreEqual(70, vector2.Size());
            Assert.IsFalse(vector2.Get(63));
            for (int i = 64; i < 70; i++)
            {
                Assert.IsTrue(vector2.Get(i));
            }
        }

        [TestMethod]
        public void FillTest()
        {
            LongBitVector vector = new LongBitVector(128);
            vector.Add(128, false);
            for (int i = 0; i < 128; i++)
            {
                Assert.IsTrue(!vector.Get(i));
            }
            vector.Fill(true);
            for (int i = 0; i < 128; i++)
            {
                Assert.IsTrue(vector.Get(i));
            }
            vector.Fill(false);
            for (int i = 0; i < 128; i++)
            {
                Assert.IsTrue(!vector.Get(i));
            }

            // check filling with 1 does not effect the overflow smoothnlp.core.bits of the last long.
            vector = new LongBitVector(3);
            vector.Add(3, false);
            vector.Fill(true);
            Assert.AreEqual(vector.GetLongArray()[0], 7);

        }

        [TestMethod]
        [Ignore("Not a test.")]
        public void PerformanceTest()
        {
            int itCount = 5;
            Random rnd = new Random(Convert.ToInt32("0xbeefcafe", 16));
            int size = 20000000;
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
            oneIndexes.CopyOf(k);
            LongBitVector vector = new LongBitVector(size);

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
            }
        }

        [TestMethod]
        public void GetLongPiece()
        {
            LongBitVector vector = new LongBitVector(128);
            vector.Add(128, false);
            vector.Set(new long[] { 0, 2, 4, 7 });
            long result = vector.GetLong(0, 5);
            Assert.AreEqual("10101", Convert.ToString(result, 2));
            vector.Set(new long[] { 62, 64, 65 });
            result = vector.GetLong(62, 5);
            Assert.AreEqual("1101", Convert.ToString(result, 2));
            vector.Fill(true);
            vector.Clear(new long[] { 63, 68 });
            Assert.AreEqual("101111011", Convert.ToString(vector.GetLong(61, 9), 2));
            Assert.AreEqual("1", Convert.ToString((vector.GetLong(61, 1)), 2));
            Assert.AreEqual("0", Convert.ToString((vector.GetLong(63, 1)), 2));
            Assert.AreEqual(Convert.ToString(unchecked((long)0xbfffffffffffffffL), 2), Convert.ToString(vector.GetLong(1, 64), 2));
        }
    }
}
