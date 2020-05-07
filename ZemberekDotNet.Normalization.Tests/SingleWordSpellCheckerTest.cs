using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Normalization.Tests
{
    [TestClass]
    public class SingleWordSpellCheckerTest
    {
        [TestMethod]
        public void SimpleDecodeTest()
        {
            SingleWordSpellChecker spellChecker = new SingleWordSpellChecker(1);
            string vocabulary = "elma";
            spellChecker.AddWord(vocabulary);
            Assert.IsTrue(spellChecker.Decode(vocabulary).Contains(vocabulary));
            Check1Distance(spellChecker, "elma");

            spellChecker.AddWord("armut");
            spellChecker.AddWord("ayva");
            Check1Distance(spellChecker, "armut");
        }

        [TestMethod]
        public void SimpleDecodeTest2()
        {
            SingleWordSpellChecker spellChecker = new SingleWordSpellChecker(1);
            spellChecker.AddWords("apple", "apples");
            FloatValueMap<string> res = spellChecker.Decode("apple");
            foreach (string re in res)
            {
                Console.WriteLine(re);
            }
        }

        [TestMethod]
        public void SimpleDecodeTest3()
        {
            SingleWordSpellChecker spellChecker = new SingleWordSpellChecker(1);
            spellChecker.AddWords("apple", "apples");
            List<SingleWordSpellChecker.ScoredString> res = spellChecker.GetSuggestionsWithScores("apple");
            foreach (SingleWordSpellChecker.ScoredString re in res)
            {
                Console.WriteLine(re.S);
            }
        }

        [TestMethod]
        public void MultiWordDecodeTest()
        {
            SingleWordSpellChecker spellChecker = new SingleWordSpellChecker(1);
            spellChecker.AddWords("çak", "sak", "saka", "bak", "çaka", "çakal", "sakal");

            FloatValueMap<String> result = spellChecker.Decode("çak");

            Assert.AreEqual(4, result.Size());
            AssertContainsAll(result, "çak", "sak", "bak", "çaka");

            double delta = 0.0001;
            Assert.AreEqual(0, result.Get("çak"), delta);
            Assert.AreEqual(1, result.Get("sak"), delta);
            Assert.AreEqual(1, result.Get("bak"), delta);
            Assert.AreEqual(1, result.Get("çaka"), delta);

            result = spellChecker.Decode("çaka");

            Assert.AreEqual(4, result.Size());
            AssertContainsAll(result, "çaka", "saka", "çakal", "çak");

            Assert.AreEqual(0, result.Get("çaka"), delta);
            Assert.AreEqual(1, result.Get("saka"), delta);
            Assert.AreEqual(1, result.Get("çakal"), delta);
            Assert.AreEqual(1, result.Get("çak"), delta);

        }

        [TestMethod]
        public void PerformanceTest()
        {
            string r = "Resources/10000_frequent_turkish_word";
            string[] words = File.ReadAllLines(r, Encoding.UTF8);
            SingleWordSpellChecker spellChecker = new SingleWordSpellChecker();
            spellChecker.BuildDictionary(words.ToList());
            long start = DateTime.Now.Ticks;
            int solutionCount = 0;
            foreach (string word in words)
            {
                FloatValueMap<string> result = spellChecker.Decode(word);
                solutionCount += result.Size();
            }
            Console.WriteLine("Elapsed: " + new TimeSpan(DateTime.Now.Ticks - start).TotalMilliseconds);
            Console.WriteLine("Solution count:" + solutionCount);
        }

        void AssertContainsAll(FloatValueMap<string> set, params string[] words)
        {
            foreach (String word in words)
            {
                Assert.IsTrue(set.Contains(word));
            }
        }

        private void Check1Distance(SingleWordSpellChecker spellChecker, string expected)
        {
            ISet<string> randomDeleted = RandomDelete(expected, 1);
            foreach (string s in randomDeleted)
            {
                FloatValueMap<String> res = spellChecker.Decode(s);
                Assert.AreEqual(res.Size(), 1, s);
                Assert.IsTrue(res.Contains(expected), s);
            }

            ISet<String> randomInserted = RandomInsert(expected, 1);
            foreach (string s in randomInserted)
            {
                FloatValueMap<string> res = spellChecker.Decode(s);
                Assert.AreEqual(res.Size(), 1, s);
                Assert.IsTrue(res.Contains(expected), s);
            }

            ISet<string> randomSubstitute = RandomSubstitute(expected, 1);
            foreach (string s in randomSubstitute)
            {
                FloatValueMap<string> res = spellChecker.Decode(s);
                Assert.AreEqual(res.Size(), 1, s);
                Assert.IsTrue(res.Contains(expected), s);
            }

            ISet<string> transpositions = Transpositions(expected);
            foreach (string s in transpositions)
            {
                FloatValueMap<string> res = spellChecker.Decode(s);
                Assert.AreEqual(res.Size(), 1, s);
                Assert.IsTrue(res.Contains(expected), s);
            }
        }

        [TestMethod]
        public void NearKeyCheck()
        {
            SingleWordSpellChecker spellChecker = new SingleWordSpellChecker(
                1,
                SingleWordSpellChecker.TurkishQNearKeyMap);
            String vocabulary = "elma";
            spellChecker.AddWord(vocabulary);
            Assert.IsTrue(spellChecker.Decode(vocabulary).Contains(vocabulary));

            // "r" is near key "e" therefore it give a smaller penalty.
            FloatValueMap<String> res1 = spellChecker.Decode("rlma");
            Assert.IsTrue(res1.Contains("elma"));
            FloatValueMap<String> res2 = spellChecker.Decode("ylma");
            Assert.IsTrue(res2.Contains("elma"));
            Assert.IsTrue(res1.Get("elma") < res2.Get("elma"));

        }

        ISet<string> RandomDelete(string input, int d)
        {
            ISet<string> result = new HashSet<string>();
            Random r = new Random(0xbeef);
            for (int i = 0; i < 100; i++)
            {
                StringBuilder sb = new StringBuilder(input);
                for (int j = 0; j < d; j++)
                {
                    sb.Remove(r.Next(sb.Length), 1);
                }
                result.Add(sb.ToString());
            }
            return result;
        }

        ISet<string> RandomInsert(string input, int d)
        {
            ISet<string> result = new HashSet<string>();
            Random r = new Random(0xbeef);
            for (int i = 0; i < 100; i++)
            {
                StringBuilder sb = new StringBuilder(input);
                for (int j = 0; j < d; j++)
                {
                    sb.Insert(r.Next(sb.Length + 1), "x");
                }
                result.Add(sb.ToString());
            }
            return result;
        }

        ISet<string> RandomSubstitute(string input, int d)
        {
            ISet<string> result = new HashSet<string>();
            Random r = new Random(0xbeef);
            for (int i = 0; i < 100; i++)
            {
                StringBuilder sb = new StringBuilder(input);
                for (int j = 0; j < d; j++)
                {
                    int start = r.Next(sb.Length);
                    sb[start] = 'x';
                }
                result.Add(sb.ToString());
            }
            return result;
        }

        ISet<string> Transpositions(string input)
        {
            ISet<string> result = new HashSet<string>();
            for (int i = 0; i < input.Length - 1; i++)
            {
                StringBuilder sb = new StringBuilder(input);
                char tmp = sb[i];
                sb[i] = sb[i + 1];
                sb[i + 1] = tmp;
                result.Add(sb.ToString());
            }
            return result;
        }
    }
}
