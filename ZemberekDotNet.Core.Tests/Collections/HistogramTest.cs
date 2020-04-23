using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class HistogramTest
    {
        [TestMethod]
        public void TestGenerate()
        {
            Histogram<string> histogram = new Histogram<string>();
            histogram.Add("Apple", "Pear", "Plum", "Apple", "Apple", "Grape", "Pear");
            Assert.AreEqual(3, histogram.GetCount("Apple"));
            Assert.AreEqual(2, histogram.GetCount("Pear"));
            Assert.AreEqual(1, histogram.GetCount("Plum"));
        }

        [TestMethod]
        public void TestRemove()
        {
            Histogram<string> histogram = new Histogram<string>();
            histogram.Add("Apple", "Pear", "Plum", "Apple", "Apple", "Grape", "Pear");
            histogram.Remove("Apple");
            Assert.AreEqual(0, histogram.GetCount("Apple"));
            Assert.AreEqual(2, histogram.GetCount("Pear"));
            Assert.AreEqual(1, histogram.GetCount("Plum"));
            Assert.IsFalse(histogram.Contains("Apple"));
        }

        [TestMethod]
        public void TestSortedByCount()
        {
            Histogram<string> histogram = new Histogram<string>();
            histogram.Add("Apple", "Pear", "Apple", "Apple", "Grape", "Pear");
            List<string> sortedByCount = histogram.GetSortedList();
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "Apple", "Pear", "Grape" }, sortedByCount));

            sortedByCount = histogram.GetTop(1); // top 1
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "Apple" }, sortedByCount));
            sortedByCount = histogram.GetTop(2); // top 2
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "Apple", "Pear" }, sortedByCount));
            sortedByCount = histogram.GetTop(5); // top 5 should return all list
            Assert.IsTrue(Enumerable.SequenceEqual(new List<string> { "Apple", "Pear", "Grape" }, sortedByCount));
        }

        [TestMethod]
        public void TestAddHistogram()
        {
            Histogram<string> histogram = new Histogram<string>();
            histogram.Add("Apple", "Pear", "Apple", "Apple", "Grape", "Pear");
            Histogram<string> histogram2 = new Histogram<string>();
            histogram2.Add("Apple", "Mango", "Apple", "Grape");
            histogram.Add(histogram2);
            Assert.AreEqual(5, histogram.GetCount("Apple"));
            Assert.AreEqual(1, histogram.GetCount("Mango"));
            Assert.AreEqual(2, histogram.GetCount("Grape"));
        }

        [TestMethod]
        [Ignore("Not a test.")]
        public void TestMergePerformance()
        {
            Histogram<string> first = new Histogram<string>();
            Histogram<string> second = new Histogram<string>();
            ISet<string> c1 = UniqueStrings(1000000, 5);
            ISet<string> c2 = UniqueStrings(1000000, 5);
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            first.Add(c1);
            second.Add(c2);
            Console.WriteLine("Elapsed:" + sw.ElapsedMilliseconds);
            sw.Restart();
            first.Add(second);
            Console.WriteLine("Elapsed:" + sw.ElapsedMilliseconds);
        }

        [TestMethod]
        public void StressTestRemoveSmaller()
        {
            Histogram<string> h = new Histogram<string>(100_000);
            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                List<String> strings = RandomStrings(100_000, 4);
                h.Add(strings);
                Console.WriteLine("before " + h.Size());
                h.RemoveSmaller(3);
                Console.WriteLine("after " + h.Size());
                foreach (string s in h)
                {
                    Assert.IsTrue(h.GetCount(s) >= 3);
                }
            }
        }

        public ISet<string> UniqueStrings(int amount, int stringLength)
        {
            ISet<string> set = new HashSet<string>(amount);
            Random r = new Random();
            while (set.Count < amount)
            {
                StringBuilder sb = new StringBuilder(stringLength);
                for (int i = 0; i < stringLength; i++)
                {
                    sb.Append((char)(r.Next(26) + 'a'));
                }
                set.Add(sb.ToString());
            }
            return set;
        }

        public List<String> RandomStrings(int amount, int stringLength)
        {
            List<string> set = new List<string>(amount);
            Random r = new Random();
            while (set.Count < amount)
            {
                StringBuilder sb = new StringBuilder(stringLength);
                for (int i = 0; i < stringLength; i++)
                {
                    sb.Append((char)(r.Next(26) + 'a'));
                }
                set.Add(sb.ToString());
            }
            return set;
        }
    }
}
