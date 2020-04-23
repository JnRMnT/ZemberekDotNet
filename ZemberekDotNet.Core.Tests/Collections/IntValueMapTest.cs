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
    public class IntValueMapTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            IntValueMap<object> set = new IntValueMap<object>();
            Assert.AreEqual(0, set.Size());
            set = new IntValueMap<object>(1);
            Assert.AreEqual(0, set.Size());
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            IntValueMap<string> set = new IntValueMap<string>(1);
            set.AddOrIncrement("foo");
            Assert.AreEqual(1, set.Size());
            set.Remove("foo");
            Assert.AreEqual(0, set.Size());
            Assert.IsFalse(set.Contains("foo"));
        }

        [TestMethod]
        public void PutTest()
        {
            IntValueMap<string> table = new IntValueMap<string>();
            table.Put("foo", 1);
            Assert.AreEqual(1, table.Size());
            table.Put("foo", 2);
            Assert.AreEqual(1, table.Size());

            table = new IntValueMap<string>();
            for (int i = 0; i < 1000; i++)
            {
                table.Put(i.ToString(), i + 1);
                Assert.AreEqual(i + 1, table.Size());
            }

            table = new IntValueMap<string>();
            for (int i = 0; i < 1000; i++)
            {
                table.Put(i.ToString(), i + 1);
                table.Put(i.ToString(), i + 1);
                Assert.AreEqual(i + 1, table.Size());
            }
        }

        [TestMethod]
        public void ExpandTest()
        {
            IntValueMap<string> table = new IntValueMap<string>();

            // we put 0..9999 keys with 1..10000 values
            for (int i = 0; i < 10000; i++)
            {
                table.Put(i.ToString(), i + 1);
                Assert.AreEqual(i + 1, table.Size());
            }
            // we remove the first half
            for (int i = 0; i < 5000; i++)
            {
                table.Remove(i.ToString());
                Assert.AreEqual(10000 - i - 1, table.Size());
            }
            // now we check if remaining values are intact
            for (int i = 5000; i < 10000; i++)
            {
                Assert.AreEqual(i + 1, table.Get(i.ToString()));
            }
        }

        [TestMethod]
        public void CollisionTest()
        {
            IntValueMap<int?> v = new IntValueMap<int?>(16);
            v.Put(3, 5);
            v.Put(19, 9);
            v.Put(35, 13);
            Assert.AreEqual(3, v.keyCount);

            Assert.AreEqual(5, v.Get(3));
            Assert.AreEqual(9, v.Get(19));
            Assert.AreEqual(13, v.Get(35));

            v.Remove(19);
            Assert.AreEqual(2, v.keyCount);

            Assert.AreEqual(5, v.Get(3));
            Assert.AreEqual(0, v.Get(19));
            Assert.AreEqual(13, v.Get(35));

            v.AddOrIncrement(35);
            Assert.AreEqual(2, v.keyCount);
            Assert.AreEqual(14, v.Get(35));

            v.Remove(35);
            Assert.AreEqual(1, v.keyCount);
            v.Put(19, 5);
            Assert.AreEqual(2, v.keyCount);
            v.AddOrIncrement(35);
            Assert.AreEqual(3, v.keyCount);

            Assert.AreEqual(1, v.Get(35));
            Assert.AreEqual(5, v.Get(19));
        }

        [TestMethod]
        public void RemoveTest()
        {
            IntValueMap<string> table = new IntValueMap<string>();
            table.Put(1.ToString(), 1);
            Assert.AreEqual(1, table.Size());
            table.Remove(1.ToString());
            Assert.AreEqual(0, table.Size());

            table = new IntValueMap<string>();
            for (int i = 0; i < 1000; i++)
            {
                table.Put(i.ToString(), i + 1);
            }
            for (int i = 0; i < 1000; i++)
            {
                table.Remove(i.ToString());
                Assert.AreEqual(0, table.Get(i.ToString()));
                Assert.AreEqual(1000 - i - 1, table.Size());
            }

            table = new IntValueMap<string>(8);
            table.Put(1.ToString(), 1);
            table.Put(9.ToString(), 1);
            Assert.AreEqual(2, table.Size());
            table.Remove(9.ToString());
            Assert.AreEqual(1, table.Size());
            Assert.AreEqual(0, table.Get(9.ToString()));
        }

        [TestMethod]
        public void IncrementTest()
        {
            IntValueMap<int?> table = new IntValueMap<int?>();

            int res = table.AddOrIncrement(1);
            Assert.AreEqual(1, res);
            Assert.AreEqual(1, table.Get(1));

            table.Put(1, 2);
            res = table.AddOrIncrement(1);
            Assert.AreEqual(3, res);
            Assert.AreEqual(3, table.Get(1));

            table = new IntValueMap<int?>();
            for (int i = 0; i < 1000; i++)
            {
                res = table.AddOrIncrement(1);
                Assert.AreEqual(i + 1, res);
                Assert.AreEqual(i + 1, table.Get(1));
                Assert.AreEqual(1, table.Size());
            }
        }

        [TestMethod]
        public void DecrementTest()
        {
            IntValueMap<int?> set = new IntValueMap<int?>();

            int res = set.Decrement(1);
            Assert.AreEqual(-1, res);
            int val = 5;
            set.Put(1, val);
            set.Put(9, val);
            for (int i = 0; i < val; i++)
            {
                res = set.Decrement(1);
                int expected = val - i - 1;
                Assert.AreEqual(expected, res);
                Assert.AreEqual(expected, set.Get(1));
            }
            Assert.AreEqual(2, set.Size());
            res = set.Decrement(1);
            Assert.AreEqual(-1, res);

            set = new IntValueMap<int?>();
            for (int i = 0; i < 1000; i++)
            {
                set.Put(i, 1);
            }

            for (int i = 0; i < 1000; i++)
            {
                res = set.Decrement(i);
                Assert.AreEqual(0, res);
                Assert.AreEqual(0, set.Get(i));
            }

            set = new IntValueMap<int?>(8);
            set.Put(1, 1);
            set.Put(9, 1);
            Assert.AreEqual(2, set.Size());
            set.Decrement(9);
            Assert.AreEqual(0, set.Get(9));
            Assert.AreEqual(2, set.Size());
        }

        [TestMethod]
        public void GetTest()
        {
            IntValueMap<int?> table = new IntValueMap<int?>();
            table.Put(1, 2);
            Assert.AreEqual(2, table.Get(1));
            Assert.AreEqual(0, table.Get(2));
            table.Put(1, 3);
            Assert.AreEqual(3, table.Get(1));

            table = new IntValueMap<int?>();
            for (int i = 0; i < 1000; i++)
            {
                table.Put(i, i + 1);
            }
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(i + 1, table.Get(i));
            }
        }

        [TestMethod]
        public void CopyOfValuesTest()
        {
            IntValueMap<string> set = new IntValueMap<string>();
            for (int i = 0; i < 1000; i++)
            {
                set.Put(i.ToString(), i + 1);
            }
            int[] values = set.CopyOfValues();
            Assert.AreEqual(1000, values.Length);
            Array.Sort(values);
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(i + 1, values[i]);
            }

            set.Remove("768");
            set.Remove("0");
            set.Remove("999");

            values = set.CopyOfValues();
            Assert.AreEqual(997, values.Length);
            Array.Sort(values);

            Assert.IsTrue(Array.BinarySearch(values, 769) < 0);
            Assert.IsTrue(Array.BinarySearch(values, 1) < 0);
            Assert.IsTrue(Array.BinarySearch(values, 1000) < 0);
        }

        [TestMethod]
        [Ignore]
        //TODO:check
        public void KeyIteratorStressTest()
        {
            Random rand = new Random(1);
            for (int i = 0; i < 5; i++)
            {
                IntValueMap<int?> siv = new IntValueMap<int?>();
                ISet<int?> uniqueKeys = new HashSet<int?>();
                for (int k = 0; k < 500_000; k++)
                {
                    uniqueKeys.Add(rand.Next(500_000));
                }
                foreach (int k in uniqueKeys)
                {
                    siv.Put(k, k + 1);
                }

                int itLen = 0;
                foreach (int k in siv)
                {
                    itLen++;
                }
                Assert.AreEqual(itLen, uniqueKeys.Count);

                ISet<int?> readValues = new HashSet<int?>();
                ISet<int?> readKeys = new HashSet<int?>();
                foreach (int key in siv)
                {
                    readValues.Add(siv.Get(key) - 1);
                    readKeys.Add(key);
                }
                Assert.IsTrue(Enumerable.SequenceEqual(uniqueKeys, readValues));
                Assert.IsTrue(Enumerable.SequenceEqual(uniqueKeys, readKeys));

                int j = 0;
                foreach (int ke in uniqueKeys)
                {
                    if (j > 50_000)
                    {
                        break;
                    }
                }

            }
        }

        [TestMethod]
        public void StressTest()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i < 10; i++)
            {
                IntValueMap<int?> siv = new IntValueMap<int?>();
                int kc = 0;
                for (int j = 0; j < 100000; j++)
                {
                    int key = rand.Next(1000);
                    bool exist = siv.Contains(key);
                    int operation = rand.Next(8);
                    switch (operation)
                    {
                        case 0: // insert
                            int value = rand.Next(10) + 1;
                            if (!exist)
                            {
                                siv.Put(key, value);
                                kc++;
                            }
                            break;
                        case 1:
                            if (exist)
                            {
                                siv.Remove(key);
                                kc--;
                            }
                            break;
                        case 2:
                            siv.AddOrIncrement(key);
                            if (!exist)
                            {
                                kc++;
                            }
                            break;
                        case 3:
                            siv.Get(key);
                            break;
                        case 4:
                            if (!exist)
                            {
                                kc++;
                            }
                            siv.Decrement(key);
                            break;
                        case 6:
                            value = rand.Next(10) + 1;
                            siv.IncrementByAmount(key, value);
                            if (!exist)
                            {
                                kc++;
                            }
                            break;
                        case 7:
                            value = rand.Next(10) + 1;
                            siv.IncrementByAmount(key, -value);
                            if (!exist)
                            {
                                kc++;
                            }
                            break;
                    }
                }
                Console.WriteLine(i + " Calculated=" + kc + " Actual=" + siv.keyCount);
            }
        }

        [TestMethod]
        [Ignore("Not a unit test")]
        public void PerformanceAgainstMap()
        {
            Random r = new Random();
            int[][] keyVals = new int[100000][];
            int itCount = 100;
            for (int i = 0; i < keyVals.Length; i++)
            {
                keyVals[i] = new int[2];
                keyVals[i][0] = r.Next(500000);
                keyVals[i][1] = r.Next(5000) + 1;
            }
            Stopwatch sw = Stopwatch.StartNew();
            for (int j = 0; j < itCount; j++)
            {
                Dictionary<int, int> map = new Dictionary<int, int>();

                foreach (int[] keyVal in keyVals)
                {
                    map.TryAdd(keyVal[0], keyVal[1]);
                }

                foreach (int[] keyVal in keyVals)
                {
                    map.GetValueOrDefault(keyVal[0]);
                }

                foreach (int[] keyVal in keyVals)
                {
                    if (map.ContainsKey(keyVal[0]))
                    {
                        map.TryAdd(keyVal[0], map.GetValueOrDefault(keyVal[0]) + 1);
                    }
                }

                foreach (int[] keyVal in keyVals)
                {
                    if (map.ContainsKey(keyVal[0]))
                    {
                        int count = map.GetValueOrDefault(keyVal[0]);
                        if (count == 1)
                        {
                            map.Remove(keyVal[0]);
                        }
                        else
                        {
                            map.TryAdd(keyVal[0], count - 1);
                        }
                    }
                }
            }
            Console.WriteLine("Map Elapsed:" + sw.ElapsedMilliseconds);

            IntValueMap<int?> countTable = new IntValueMap<int?>();
            sw = Stopwatch.StartNew();

            for (int j = 0; j < itCount; j++)
            {

                foreach (int[] keyVal in keyVals)
                {
                    countTable.Put(keyVal[0], keyVal[1]);
                }
                foreach (int[] keyVal in keyVals)
                {
                    countTable.Get(keyVal[0]);
                }

                foreach (int[] keyVal in keyVals)
                {
                    countTable.AddOrIncrement(keyVal[0]);
                }

                foreach (int[] keyVal in keyVals)
                {
                    countTable.Decrement(keyVal[0]);
                }
            }
            Console.WriteLine("Count Elapsed:" + sw.ElapsedMilliseconds);
        }

        [TestMethod]
        [Ignore("Not a unit test")]
        public void PerfStrings()
        {
            for (int i = 0; i < 5; i++)
            {
                ISet<string> strings = UniqueStrings(1000000, 7);
                Stopwatch sw = Stopwatch.StartNew();
                ISet<string> newSet = new HashSet<string>(strings);
                Console.WriteLine("C# Set : " + sw.ElapsedMilliseconds);
                Console.WriteLine("Size  = " + newSet.Count);
                sw.Restart();
                IntValueMap<string> cs = new IntValueMap<string>(strings.Count * 2);
                cs.AddOrIncrementAll(strings);
                Console.WriteLine("Count Add : " + sw.ElapsedMilliseconds);
            }
        }

        private ISet<string> UniqueStrings(int amount, int stringLength)
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
    }
}
