using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ZemberekDotNet.Core.Hash;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Tests.Hash
{
    [TestClass]
    public class MultiLevelMphfTest
    {
        [TestMethod]
        public void IntKeys()
        {
            int[] limits = { 1, 2, 10, 100, 1000, 50000, 100000 };
            int arraySize = 5;
            foreach (int limit in limits)
            {
                Console.WriteLine("Key amount: " + limit);
                int[][] arr = new int[limit][];
                for (int i = 0; i < limit; i++)
                {
                    arr[i] = new int[arraySize];
                    for (int j = 0; j < arraySize; j++)
                    {
                        arr[i][j] = i;
                    }
                }
                GenerateAndTest(new IntArrayKeyProvider(arr));
            }
        }

        [TestMethod]
        public void StringKeys()
        {
            int[]
            limits = { 1, 2, 10, 100, 1000, 50000, 100000 };
            int strSize = 5;
            foreach (int limit in limits)
            {
                Console.WriteLine("Key amount: " + limit);
                Stopwatch sw = Stopwatch.StartNew();
                StringHashKeyProvider provider = new StringHashKeyProvider(
                    TestUtil.UniqueStrings(limit, strSize));
                Console.WriteLine("Generation:" + sw.ElapsedMilliseconds);
                GenerateAndTest(provider);
            }
        }

        private void GenerateAndTest(IIntHashKeyProvider provider)
        {

            long start = DateTime.Now.Ticks;
            MultiLevelMphf fmph = MultiLevelMphf.Generate(provider);
            Console.WriteLine("Time to generate:" + new TimeSpan (DateTime.Now.Ticks - start).Milliseconds);

            Console.WriteLine("Bits per key:" + fmph.AverageBitsPerKey());
            Console.WriteLine("Hash levels:" + fmph.GetLevelCount());
            start = DateTime.Now.Ticks;

            int keyAmount = provider.KeyAmount();
            int[] values = new int[keyAmount];
            for (int i = 0; i < keyAmount; i++)
            {
                values[i] = fmph.Get(provider.GetKey(i));
            }

            Console.WriteLine("Time to query:" + new TimeSpan(DateTime.Now.Ticks - start).Milliseconds);

            ISet<int> results = new HashSet<int>(keyAmount);
            for (int i = 0; i < keyAmount; i++)
            {
                Assert.IsTrue(results.Add(values[i]), (i + ":" + values[i]));
            }
        }
    }
}