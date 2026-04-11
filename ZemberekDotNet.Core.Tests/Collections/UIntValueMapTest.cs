using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class UIntValueMapTest
    {
        [TestMethod]
        public void StressTest()
        {
            List<string> stringSet = RandomNumberStrings(100_000);
            UIntValueMap<string> a = new UIntValueMap<string>();
            UIntValueMap<string> b = new UIntValueMap<string>();

            for (int i = 0; i < 20; i++)
            {
                foreach (string s in stringSet)
                {
                    char c = s[s.Length - 1];
                    if (a.Contains(s))
                    {
                        a.IncrementByAmount(s, 1);
                        continue;
                    }
                    if (b.Contains(s))
                    {
                        b.IncrementByAmount(s, 1);
                        continue;
                    }
                    if (c % 2 == 0)
                    {
                        a.IncrementByAmount(s, 1);
                    }
                    else
                    {
                        b.IncrementByAmount(s, 1);
                    }
                }
            }

            // a and b cannot have shared keys.
            foreach (string k in a)
            {
                Assert.IsFalse(b.Contains(k));
            }
        }

        private List<string> RandomNumberStrings(int k)
        {
            List<string> intSet = new List<string>();
            Random rnd = new Random(1);
            while (intSet.Count < k)
            {
                int r = rnd.Next(100_000);
                intSet.Add(Convert.ToString(r + 1));
            }
            return intSet;
        }
    }
}
