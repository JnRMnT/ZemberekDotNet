using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class IntFloatMapTest
    {
        private IntFloatMap CreateMap()
        {
            return new IntFloatMap();
        }

        private IntFloatMap CreateMap(int initialSize)
        {
            return new IntFloatMap(initialSize);
        }

        private static void AssertEqualsF(float a, float b)
        {
            Assert.AreEqual(a.ToIntBits(), b.ToIntBits());
        }

        [TestMethod]
        public void InitializesCorrectly()
        {
            // Check first 1K initial sizes.
            for (int i = 1; i < 1000; i++)
            {
                IntFloatMap im = CreateMap(i);
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
                IntFloatMap im = CreateMap();
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
            PutGetCheck(0, 0.0f);
            PutGetCheck(0, -1.0f);
            PutGetCheck(0, 1.0f);
            PutGetCheck(0, float.MaxValue);
            PutGetCheck(0, float.MinValue);
            PutGetCheck(-1, 0.0f);
            PutGetCheck(-1, -1.0f);
            PutGetCheck(-1, 1.0f);
            PutGetCheck(-1, float.MaxValue);
            PutGetCheck(-1, float.MinValue);
            PutGetCheck(int.MaxValue, 0.0f);
            PutGetCheck(int.MaxValue, -1.0f);
            PutGetCheck(int.MaxValue, 1.0f);
            PutGetCheck(int.MaxValue, float.MaxValue);
            PutGetCheck(int.MaxValue, float.MinValue);
            PutGetCheck(int.MinValue + 2, 0f);
            PutGetCheck(int.MinValue + 2, -1f);
            PutGetCheck(int.MinValue + 2, 1f);
            PutGetCheck(int.MinValue + 2, float.MaxValue);
            PutGetCheck(int.MinValue + 2, float.MinValue);
        }

        private void PutGetCheck(int key, float value)
        {
            IntFloatMap im = CreateMap();
            im.Put(key, value);
            AssertEqualsF(value, im.Get(key));
        }

        [TestMethod]
        public void ExpandsCorrectly()
        {
            // Create maps with different sizes and add size * 10 elements to each.
            for (int i = 1; i < 100; i++)
            {
                IntFloatMap im = CreateMap(i);
                // Insert i * 10 elements to each and confirm sizes
                int elements = i * 10;
                for (int j = 0; j < elements; j++)
                {
                    im.Put(j, j + 13);
                }
                for (int j = 0; j < elements; j++)
                {
                    AssertEqualsF(im.Get(j), j + 13);
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
                IntFloatMap innerMap = CreateMap();
                CheckSpanInsertions(innerMap, -i, i);
            }
            // Do the same, this time overwrite values as well
            IntFloatMap im = CreateMap();
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
            IntFloatMap im = CreateMap();
            im.Put(0, 0);
            AssertEqualsF(im.Get(0), 0);
            im.Remove(0);
            AssertEqualsF(im.Get(0), IntIntMap.NoResult);
            AssertEqualsF(im.Size(), 0);
            // remove again works
            im.Remove(0);
            AssertEqualsF(im.Get(0), IntIntMap.NoResult);
            AssertEqualsF(im.Size(), 0);
            im.Put(0, 1);
            AssertEqualsF(im.Size(), 1);
            AssertEqualsF(im.Get(0), 1);
        }

        [TestMethod]
        public void RemoveSpansWorksCorrectly()
        {
            IntFloatMap im = CreateMap();
            InsertSpan(im, 0, 99);
            RemoveSpan(im, 1, 98);
            AssertEqualsF(im.Size(), 2);
            CheckSpanRemoved(im, 1, 98);
            InsertSpan(im, 0, 99);
            AssertEqualsF(im.Size(), 100);
            CheckSpan(im, 0, 99);
        }

        [TestMethod]
        public void RemoveSpansWorksCorrectly2()
        {
            IntFloatMap im = CreateMap();
            int limit = 9999;
            InsertSpan(im, 0, limit);
            int[] r = TestUtils.createRandomUintArray(1000, limit);
            foreach (int i in r)
            {
                im.Remove(i);
            }
            foreach (int i in r)
            {
                AssertEqualsF(im.Get(i), IntIntMap.NoResult);
            }
            InsertSpan(im, 0, limit);
            CheckSpan(im, 0, limit);
            RemoveSpan(im, 0, limit);
            AssertEqualsF(im.Size(), 0);
            InsertSpan(im, -limit, limit);
            CheckSpan(im, -limit, limit);
        }

        [TestMethod]
        public void SurvivesSimpleFuzzing()
        {
            List<int[]> fuzzLists = TestUtils.CreateFuzzingLists();
            foreach (int[] arr in fuzzLists)
            {
                IntFloatMap innerMap = CreateMap();
                foreach (int i in arr)
                {
                    innerMap.Put(i, i + 7);
                    AssertEqualsF(innerMap.Get(i), i + 7);
                }
            }

            IntFloatMap im = CreateMap();
            foreach (int[] arr in fuzzLists)
            {
                foreach (int i1 in arr)
                {
                    im.Put(i1, i1 + 7);
                    AssertEqualsF(im.Get(i1), i1 + 7);
                }
            }
        }

        private void RemoveSpan(IntFloatMap im, int start, int end)
        {
            int spanStart = Math.Min(start, end);
            int spanEnd = Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                im.Remove(i);
            }
        }

        private void CheckSpanRemoved(IntFloatMap im, int start, int end)
        {
            int spanStart = Math.Min(start, end);
            int spanEnd = Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                AssertEqualsF(im.Get(i), IntIntMap.NoResult);
            }
        }

        private void CheckSpanInsertions(IntFloatMap im, int start, int end)
        {
            InsertSpan(im, start, end);
            // Expected size.
            int size = Math.Abs(start) + Math.Abs(end) + 1;
            AssertEqualsF(size, im.Size());
            CheckSpan(im, start, end);
        }

        private void InsertSpan(IntFloatMap im, int start, int end)
        {
            int spanStart = Math.Min(start, end);
            int spanEnd = Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                im.Put(i, i);
            }
        }

        private void CheckSpan(IntFloatMap im, int start, int end)
        {
            int spanStart = Math.Min(start, end);
            int spanEnd = Math.Max(start, end);
            for (int i = spanStart; i <= spanEnd; i++)
            {
                AssertEqualsF(im.Get(i), i);
            }
            // Check outside of span values do not exist in the map
            for (int i = spanStart - 1, idx = 0; idx < 100; i--, idx++)
            {
                AssertEqualsF(IntIntMap.NoResult, im.Get(i));
            }
            for (int i = spanEnd + 1, idx = 0; idx < 100; i++, idx++)
            {
                AssertEqualsF(IntIntMap.NoResult, im.Get(i));
            }
        }

        private void CheckSize(IntFloatMap m, int size)
        {
            AssertEqualsF(size, m.Size());
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
            for (int i = 0; i < iter; i++)
            {
                IntFloatMap innerMap = CreateMap();
                foreach (int i1 in arr)
                {
                    innerMap.Put(i1, i1 + 1);
                }
            }
            long elapsed = DateTime.Now.Ticks - start;
            Console.WriteLine("Creation: " + new TimeSpan(elapsed).TotalMilliseconds);

            IntFloatMap imap = CreateMap();
            foreach (int i1 in arr)
            {
                imap.Put(i1, i1 + 1);
            }
            start = DateTime.Now.Ticks;
            for (int i = 0; i < iter; i++)
            {
                for (int j = arr.Length - 1; j >= 0; j--)
                {
                    sum += (long)imap.Get(arr[j]);
                }
            }
            elapsed = DateTime.Now.Ticks - start;
            Console.WriteLine("Retrieval: " + new TimeSpan(elapsed).TotalMilliseconds);
            Console.WriteLine("Val: " + sum);
        }

        [TestMethod]
        public void GetTest2()
        {
            IntFloatMap map = CreateMap();
            map.Put(1, 2);
            AssertEqualsF(2, map.Get(1));
            AssertEqualsF(IntIntMap.NoResult, map.Get(2));
            map.Put(1, 3);
            AssertEqualsF(3, map.Get(1));

            map = CreateMap();
            for (int i = 0; i < 100000; i++)
            {
                map.Put(i, i + 1);
            }
            for (int i = 0; i < 100000; i++)
            {
                AssertEqualsF(i + 1, map.Get(i));
            }
        }

        [TestMethod]
        public void RemoveTest2()
        {
            IntFloatMap map = CreateMap();
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
            int size = 1_000_000;
            int[] keys = new int[size];
            float[] vals = new float[size];
            int iterCreation = 10;
            int iterRetrieval = 50;
            for (int i = 0; i < keys.Length; i++)
            {
                // We allow some duplications.
                keys[i] = r.Next(size * 5);
                vals[i] = (float)r.NextDouble() * 5000f;
            }
            Stopwatch sw = Stopwatch.StartNew();
            Dictionary<int, float> map = null;
            for (int j = 0; j < iterCreation; j++)
            {
                map = new Dictionary<int, float>();
                for (int i = 0; i < size; i++)
                {
                    map.TryAdd(keys[i], vals[i]);
                }
            }
            Console.WriteLine("Map creation: " + sw.ElapsedMilliseconds);
            map = new Dictionary<int, float>();
            for (int i = 0; i < size; i++)
            {
                map.TryAdd(keys[i], vals[i]);
            }
            double val = 0;
            sw = Stopwatch.StartNew();
            for (int j = 0; j < iterRetrieval; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    val += map.GetValueOrDefault(keys[i]);
                }
            }
            Console.WriteLine("Map retrieval: " + sw.ElapsedMilliseconds);
            Console.WriteLine("Verification sum: " + val);
            IntFloatMap countTable = null;
            sw = Stopwatch.StartNew();
            for (int j = 0; j < iterCreation; j++)
            {
                countTable = CreateMap();
                for (int i = 0; i < size; i++)
                {
                    countTable.Put(keys[i], vals[i]);
                }
            }
            Console.WriteLine("IntIntMap creation: " + sw.ElapsedMilliseconds);

            countTable = CreateMap();
            for (int i = 0; i < size; i++)
            {
                countTable.Put(keys[i], vals[i]);
            }
            val = 0.0d;
            sw = Stopwatch.StartNew();
            for (int j = 0; j < iterRetrieval; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    val += countTable.Get(keys[i]);
                }
            }
            Console.WriteLine("IntIntMap retrieval: " + sw.ElapsedMilliseconds);
            Console.WriteLine("Verification sum: " + val);
        }
    }
}
