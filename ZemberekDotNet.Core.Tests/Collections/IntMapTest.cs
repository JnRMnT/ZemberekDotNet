using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Collections;
using SystemMath = System.Math;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class IntMapTest
    {
        [TestMethod]
        public void InitializesCorrectly()
        {
            for (int i = 1; i < 1000; i++)
            {
                IntMap<string> im = new IntMap<string>(i);
                CheckSize(im, 0);
            }
        }

        [TestMethod]
        public void FailsOnInvalidSizes()
        {
            CheckInvalidSize(0);
            CheckInvalidSize(-1);
            CheckInvalidSize(int.MaxValue);
            CheckInvalidSize(int.MinValue);
            CheckInvalidSize((1 << 29) + 1);
        }

        private void CheckInvalidSize(int size)
        {
            try
            {
                IntMap<string> im = new IntMap<string>(size);
                Assert.Fail("Illegal size should have thrown an exception. Size: " + size);
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ExpandsCorrectly()
        {
            for (int i = 1; i < 100; i++)
            {
                IntMap<string> im = new IntMap<string>(i);
                int elements = i * 10;
                for (int j = 0; j < elements; j++)
                {
                    im.Put(j, "" + j);
                }
                for (int j = 0; j < elements; j++)
                {
                    Assert.AreEqual("" + j, im.Get(j));
                }
                CheckSize(im, elements);
            }
        }

        [TestMethod]
        public void PutAddsAndUpdatesElementsCorrectly()
        {
            int span = 100;
            for (int i = 0; i < span; i++)
            {
                IntMap<string> im = new IntMap<string>();
                CheckSpanInsertions(im, -i, i);
            }
            // Overwrite values as well
            IntMap<string> im2 = new IntMap<string>();
            for (int i = 0; i < span; i++)
            {
                CheckSpanInsertions(im2, -i, i);
                CheckSpanInsertions(im2, -i, i);
                CheckSpanInsertions(im2, -i, i);
            }
        }

        [TestMethod]
        public void SurvivesSimpleFuzzing()
        {
            List<int[]> fuzzLists = TestUtils.CreateFuzzingLists();
            foreach (int[] arr in fuzzLists)
            {
                IntMap<string> im = new IntMap<string>();
                foreach (int i1 in arr)
                {
                    im.Put(i1, "" + i1);
                    Assert.AreEqual("" + i1, im.Get(i1));
                }
            }

            IntMap<string> im2 = new IntMap<string>();
            foreach (int[] arr in fuzzLists)
            {
                foreach (int i1 in arr)
                {
                    im2.Put(i1, "" + i1);
                    Assert.AreEqual("" + i1, im2.Get(i1));
                }
            }
        }

        private void CheckSpanInsertions(IntMap<string> im, int start, int end)
        {
            InsertSpan(im, start, end);
            int size = SystemMath.Abs(start) + SystemMath.Abs(end) + 1;
            Assert.AreEqual(size, im.Size());
            CheckSpan(im, start, end);
        }

        private void InsertSpan(IntMap<string> im, int start, int end)
        {
            int spanStart = SystemMath.Min(start, end);
            int spanEnd = SystemMath.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                im.Put(i, "" + i);
            }
        }

        private void CheckSpan(IntMap<string> im, int start, int end)
        {
            int spanStart = SystemMath.Min(start, end);
            int spanEnd = SystemMath.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                Assert.AreEqual("" + i, im.Get(i));
            }
            for (int i = spanStart - 1, idx = 0; idx < 100; i--, idx++)
            {
                if (i == int.MinValue) break;
                Assert.IsNull(im.Get(i));
            }
            for (int i = spanEnd + 1, idx = 0; idx < 100; i++, idx++)
            {
                if (i == int.MinValue) break;
                Assert.IsNull(im.Get(i));
            }
        }

        private void CheckSize(IntMap<string> m, int size)
        {
            Assert.AreEqual(size, m.Size());
            Assert.IsTrue(m.Capacity() > m.Size());
            // Check capacity is a power of 2
            Assert.IsTrue((m.Capacity() & (m.Capacity() - 1)) == 0);
        }
    }
}
