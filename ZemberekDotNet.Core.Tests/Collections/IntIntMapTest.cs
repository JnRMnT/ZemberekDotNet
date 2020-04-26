using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class IntIntMapTest
    {
        private IntIntMap CreateMap()
        {
            return new IntIntMap();
        }

        private IntIntMap CreateMap(int initialSize)
        {
            return new IntIntMap(initialSize);
        }


        [TestMethod]
        public void InitializesCorrectly()
        {
            // Check first 1K initial sizes.
            for (int i = 1; i < 1000; i++)
            {
                IntIntMap im = CreateMap(i);
                CheckSize(im, 0);
            }
        }

        [TestMethod]
        public void FailsOnInvalidSizes()
        {
            CheckInvalidSize(0);
            CheckInvalidSize(-1);
            CheckInvalidSize(int.MaxValue);
        }

        private void CheckInvalidSize(int size)
        {
            try
            {
                CreateMap(size);
                Assert.Fail("Illegal size should have thrown an exception. Size: " + size);
            }
            catch (SystemException e)
            {
                // Nothing to do
            }
        }

        [TestMethod]
        public void FailsOnInvalidKeys()
        {
            CheckInvalidKeys(int.MinValue);
            CheckInvalidKeys(int.MinValue + 1);
        }

        private void CheckInvalidKeys(int key)
        {
            try
            {
                IntIntMap im = CreateMap();
                im.Put(key, 1);
                Assert.Fail("Illegal key should have thrown an exception. Key: " + key);
            }
            catch (SystemException e)
            {
                // Nothing to do
            }
        }

        [TestMethod]
        public void PutGetWorksCorrectly()
        {
            // Test edge conditions.
            PutGetCheck(0, 0);
            PutGetCheck(0, -1);
            PutGetCheck(0, 1);
            PutGetCheck(0, int.MaxValue);
            PutGetCheck(0, int.MinValue);
            PutGetCheck(-1, 0);
            PutGetCheck(-1, -1);
            PutGetCheck(-1, 1);
            PutGetCheck(-1, int.MaxValue);
            PutGetCheck(-1, int.MinValue);
            PutGetCheck(int.MaxValue, 0);
            PutGetCheck(int.MaxValue, -1);
            PutGetCheck(int.MaxValue, 1);
            PutGetCheck(int.MaxValue, int.MaxValue);
            PutGetCheck(int.MaxValue, int.MinValue);
            PutGetCheck(int.MinValue + 2, 0);
            PutGetCheck(int.MinValue + 2, -1);
            PutGetCheck(int.MinValue + 2, 1);
            PutGetCheck(int.MinValue + 2, int.MaxValue);
            PutGetCheck(int.MinValue + 2, int.MinValue);
        }

        private void PutGetCheck(int key, int value)
        {
            IntIntMap im = CreateMap();
            im.Put(key, value);
            Assert.AreEqual(value, im.Get(key));
        }

        [TestMethod]
        public void HandlesExpansionEdgeCases()
        {
            // If load factor is 1 this means backing array is filled completely
            // and this causes an infinite loop in case a non existing element is
            // searched in the map. This happens because we expect at least an
            // empty slot in the map to decide the element does not exist in the
            // map.
            IntIntMap im = new IntIntMap(2);
            im.Put(1, 3);
            im.Put(2, 5);
            // If backing array has no  empty element this call would cause an
            // infinite loop.
            Assert.AreEqual(im.Get(3), IntIntMap.Empty);

            im = new IntIntMap(4);
            im.Put(1, 3);
            im.Put(2, 5);
            im.Put(3, 5);
            im.Put(4, 5);
            Assert.AreEqual(im.Get(5), IntIntMap.Empty);

            im = new IntIntMap(1);
            im.Put(1, 2);
            Assert.AreEqual(im.Get(3), IntIntMap.Empty);
        }

        [TestMethod]
        public void ExpandsCorrectly()
        {
            // Create maps with different sizes and add size * 10 elements to each.
            for (int i = 1; i < 100; i++)
            {
                IntIntMap im = CreateMap(i);
                // Insert i * 10 elements to each and confirm sizes
                int elements = i * 10;
                for (int j = 0; j < elements; j++)
                {
                    im.Put(j, j + 13);
                }
                for (int j = 0; j < elements; j++)
                {
                    Assert.AreEqual(im.Get(j), j + 13);
                }
                CheckSize(im, elements);
            }
        }

        [TestMethod]
        public void PutAddsAndUpdatesElementsCorrectly()
        {
            int span = 100;
            IntIntMap im = null;
            for (int i = 0; i < span; i++)
            {
                im = CreateMap();
                CheckSpanInsertions(im, -i, i);
            }
            // Do the same, this time overwrite values as well
            im = CreateMap();
            for (int i = 0; i < span; i++)
            {
                CheckSpanInsertions(im, -i, i);
                CheckSpanInsertions(im, -i, i);
                CheckSpanInsertions(im, -i, i);
            }
        }

        [TestMethod]
        public void RemoveRemovesCorrectly()
        {
            IntIntMap im = CreateMap();
            im.Put(0, 0);
            Assert.AreEqual(im.Get(0), 0);
            im.Remove(0);
            Assert.AreEqual(im.Get(0), IntIntMap.NoResult);
            Assert.AreEqual(im.Size(), 0);
            // remove again works
            im.Remove(0);
            Assert.AreEqual(im.Get(0), IntIntMap.NoResult);
            Assert.AreEqual(im.Size(), 0);
            im.Put(0, 1);
            Assert.AreEqual(im.Size(), 1);
            Assert.AreEqual(im.Get(0), 1);
        }

        [TestMethod]
        public void RemoveSpansWorksCorrectly()
        {
            IntIntMap im = CreateMap();
            InsertSpan(im, 0, 99);
            RemoveSpan(im, 1, 98);
            Assert.AreEqual(im.Size(), 2);
            CheckSpanRemoved(im, 1, 98);
            InsertSpan(im, 0, 99);
            Assert.AreEqual(im.Size(), 100);
            CheckSpan(im, 0, 99);
        }

        [TestMethod]
        public void RemoveSpansWorksCorrectly2()
        {
            IntIntMap im = CreateMap();
            int limit = 9999;
            InsertSpan(im, 0, limit);
            int[] r = TestUtils.createRandomUintArray(1000, limit);
            foreach (int i in r)
            {
                im.Remove(i);
            }
            foreach (int i in r)
            {
                Assert.AreEqual(im.Get(i), IntIntMap.NoResult);
            }
            InsertSpan(im, 0, limit);
            CheckSpan(im, 0, limit);
            RemoveSpan(im, 0, limit);
            Assert.AreEqual(im.Size(), 0);
            InsertSpan(im, -limit, limit);
            CheckSpan(im, -limit, limit);
        }

        [TestMethod]
        public void SurvivesSimpleFuzzing()
        {
            IntIntMap im = null;
            List<int[]> fuzzLists = TestUtils.CreateFuzzingLists();
            foreach (int[] arr in fuzzLists)
            {
                im = CreateMap();
                foreach (int i1 in arr)
                {
                    im.Put(i1, i1 + 7);
                    Assert.AreEqual(im.Get(i1), i1 + 7);
                }
            }

            im = CreateMap();
            foreach (int[] arr in fuzzLists)
            {
                foreach (int i1 in arr)
                {
                    im.Put(i1, i1 + 7);
                    Assert.AreEqual(im.Get(i1), i1 + 7);
                }
            }
        }

        private void RemoveSpan(IntIntMap im, int start, int end)
        {
            int spanStart = System.Math.Min(start, end);
            int spanEnd = System.Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                im.Remove(i);
            }
        }

        private void CheckSpanRemoved(IntIntMap im, int start, int end)
        {
            int spanStart = System.Math.Min(start, end);
            int spanEnd = System.Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                Assert.AreEqual(im.Get(i), IntIntMap.NoResult);
            }
        }

        private void CheckSpanInsertions(IntIntMap im, int start, int end)
        {
            InsertSpan(im, start, end);
            // Expected size.
            int size = System.Math.Abs(start) + System.Math.Abs(end) + 1;
            Assert.AreEqual(size, im.Size());
            CheckSpan(im, start, end);
        }

        [TestMethod]
        public void CheckLargeValues()
        {
            IntIntMap map = CreateMap();
            int c = 0;
            for (int i = int.MinValue; i < int.MaxValue - 1000; i += 1000)
            {
                map.Put(c, i);
                c++;
            }
            c = 0;
            for (int i = int.MinValue; i < int.MaxValue - 1000; i += 1000)
            {
                int val = map.Get(c);
                Assert.AreEqual(i, val);
                c++;
            }
            c = 0;
            for (int i = int.MinValue; i < int.MaxValue - 1000; i += 1000)
            {
                map.Increment(c, 1);
                c++;
            }

            c = 0;
            for (int i = int.MinValue; i < int.MaxValue - 1000; i += 1000)
            {
                int val = map.Get(c);
                Assert.AreEqual(i + 1, val);
                c++;
            }
        }

        private void InsertSpan(IntIntMap im, int start, int end)
        {
            int spanStart = System.Math.Min(start, end);
            int spanEnd = System.Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                im.Put(i, i);
            }
        }

        private void CheckSpan(IntIntMap im, int start, int end)
        {
            int spanStart = System.Math.Min(start, end);
            int spanEnd = System.Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                Assert.AreEqual(im.Get(i), i);
            }
            // Check outside of span values do not exist in the map
            for (int i = spanStart - 1, idx = 0; idx < 100; i--, idx++)
            {
                Assert.AreEqual(IntIntMap.NoResult, im.Get(i));
            }
            for (int i = spanEnd + 1, idx = 0; idx < 100; i++, idx++)
            {
                Assert.AreEqual(IntIntMap.NoResult, im.Get(i));
            }
        }

        private void CheckSize(IntIntMap m, int size)
        {
            Assert.AreEqual(size, m.Size());
            Assert.IsTrue(m.Capacity() > m.Size());
            // Check capacity is 2^n
            Assert.IsTrue((m.Capacity() & (m.Capacity() - 1)) == 0);
        }

        [TestMethod]
        [Ignore("Not a unit test")]
        public void TestPerformance()
        {
            int[] arr = TestUtils.createRandomUintArray(1_000_000, 1 << 29);
            long sum = 0;
            int iter = 100;
            long start = DateTime.Now.Ticks;
            IntIntMap imap = null;
            for (int i = 0; i < iter; i++)
            {
                imap = CreateMap();
                foreach (int i1 in arr)
                {
                    imap.Put(i1, i1 + 1);
                }
            }
            long elapsed = DateTime.Now.Ticks - start;
            Console.WriteLine("Creation: " + new TimeSpan(elapsed).TotalMilliseconds);

            imap = CreateMap();
            foreach (int i1 in arr)
            {
                imap.Put(i1, i1 + 1);
            }
            start = DateTime.Now.Ticks;
            for (int i = 0; i < iter; i++)
            {
                for (int j = arr.Length - 1; j >= 0; j--)
                {
                    sum += imap.Get(arr[j]);
                }
            }
            elapsed = DateTime.Now.Ticks - start;
            Console.WriteLine("Retrieval: " + new TimeSpan(elapsed).TotalMilliseconds);
            Console.WriteLine("Val: " + sum);
        }

        [TestMethod]
        public void GetTest2()
        {
            IntIntMap map = CreateMap();
            map.Put(1, 2);
            Assert.AreEqual(2, map.Get(1));
            Assert.AreEqual(IntIntMap.NoResult, map.Get(2));
            map.Put(1, 3);
            Assert.AreEqual(3, map.Get(1));

            map = CreateMap();
            for (int i = 0; i < 100000; i++)
            {
                map.Put(i, i + 1);
            }
            for (int i = 0; i < 100000; i++)
            {
                Assert.AreEqual(i + 1, map.Get(i));
            }
        }

        [TestMethod]
        public void RemoveTest2()
        {
            IntIntMap map = CreateMap();
            for (int i = 0; i < 10000; i++)
            {
                map.Put(i, i + 1);
            }
            for (int i = 0; i < 10000; i += 3)
            {
                map.Remove(i);
            }
            for (int i = 0; i < 10000; i += 3)
            {
                Assert.IsTrue(!map.ContainsKey(i));
            }
            for (int i = 0; i < 10000; i++)
            {
                map.Put(i, i + 1);
            }
            for (int i = 0; i < 10000; i += 3)
            {
                Assert.IsTrue(map.ContainsKey(i));
            }
        }

        [TestMethod]
        [Ignore("Not a unit test")]
        public void SpeedAgainstHashMap()
        {
            Random r = new Random(Convert.ToInt32("0xbeefcafe", 16));
            int[][] keyVals = new int[1_000_000][];
            int iterCreation = 10;
            int iterRetrieval = 50;
            for (int i = 0; i < keyVals.Length; i++)
            {
                // We allow some duplications.
                keyVals[i][0] = r.Next(5_000_000);
                keyVals[i][1] = r.Next(5000) + 1;
            }
            Stopwatch sw = Stopwatch.StartNew();
            Dictionary<int, int> map = null;
            for (int j = 0; j < iterCreation; j++)
            {
                map = new Dictionary<int, int>();
                foreach (int[] keyVal in keyVals)
                {
                    map.Add(keyVal[0], keyVal[1]);
                }
            }
            Console.WriteLine("Map creation: " + sw.ElapsedMilliseconds);
            map = new Dictionary<int, int>();
            foreach (int[] keyVal in keyVals)
            {
                map.Add(keyVal[0], keyVal[1]);
            }
            long val = 0;
            sw = Stopwatch.StartNew();
            for (int j = 0; j < iterRetrieval; j++)
            {
                foreach (int[] keyVal in keyVals)
                {
                    val += map.GetValueOrDefault(keyVal[0]);
                }
            }
            Console.WriteLine("Map retrieval: " + sw.ElapsedMilliseconds);
            Console.WriteLine("Verification sum: " + val);

            sw = Stopwatch.StartNew();
            IntIntMap countTable = null;
            for (int j = 0; j < iterCreation; j++)
            {
                countTable = CreateMap();
                foreach (int[] keyVal in keyVals)
                {
                    countTable.Put(keyVal[0], keyVal[1]);
                }
            }
            Console.WriteLine("IntIntMap creation: " + sw.ElapsedMilliseconds);

            countTable = CreateMap();
            foreach (int[] keyVal in keyVals)
            {
                countTable.Put(keyVal[0], keyVal[1]);
            }
            val = 0;
            sw = Stopwatch.StartNew();
            for (int j = 0; j < iterRetrieval; j++)
            {
                foreach (int[] keyVal in keyVals)
                {
                    val += countTable.Get(keyVal[0]);
                }
            }
            Console.WriteLine("IntIntMap retrieval: " + sw.ElapsedMilliseconds);
            Console.WriteLine("Verification sum: " + val);
        }
    }
}
