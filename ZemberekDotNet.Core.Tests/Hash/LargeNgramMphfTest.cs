using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ZemberekDotNet.Core.Hash;

namespace ZemberekDotNet.Core.Tests.Hash
{
    [TestClass]
    public class LargeNgramMphfTest
    {
        [TestMethod]
        [Ignore("Contains File I/O, test manually.")]
        public void NgramFileMPHFTest()
        {
            int[] gramCounts = { 1, 2, 4, 8, 10, 100, 1000, 10000, 100000, 1000000 };
            foreach (int gramCount in gramCounts)
            {
                Console.WriteLine("Gram Count = " + gramCount);
                int order = 5;
                int[][] arr = new int[gramCount][];
                for (int i = 0; i < gramCount; i++)
                {
                    arr[i] = new int[order];
                    for (int j = 0; j < order; j++)
                    {
                        arr[i][j] = i;
                    }
                }
                string file = GenerateBinaryGramFile(order, gramCount, arr);
                Stopwatch sw = Stopwatch.StartNew();
                LargeNgramMphf mphf = LargeNgramMphf.Generate(file, 20);
                Console.WriteLine("Generation time:" + sw.ElapsedMilliseconds);
                sw.Restart();
                foreach (int[] key in arr)
                {
                    mphf.Get(key);
                }
                Console.WriteLine(sw.ElapsedMilliseconds);

                Console.WriteLine("Verifying Results:");
                ISet<int> results = new HashSet<int>(gramCount);
                foreach (int[] key in arr)
                {
                    int res = mphf.Get(key);
                    Assert.IsTrue(res >= 0 && res < gramCount, "unexpected result:" + res);
                    results.Add(res);
                }
                Assert.AreEqual(results.Count, gramCount);
                Console.WriteLine("------------------------------------------");
            }
        }

        private string GenerateBinaryGramFile(int order, int gramCount, int[][] keys)
        {
            string filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "grams");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1000000);
            Console.WriteLine("writing");
            BinaryWriter dos = new BinaryWriter(file);
            dos.Write(order);
            dos.Write(gramCount);
            for (int j = 0; j < keys.Length; j++)
            {
                for (int i = 0; i < order; i++)
                {
                    dos.Write(j);
                }
            }
            dos.Close();
            file.Close();
            return filePath;
        }
    }
}
