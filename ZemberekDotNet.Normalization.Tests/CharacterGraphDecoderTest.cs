using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Morphology;

namespace ZemberekDotNet.Normalization.Tests
{
    [TestClass]
    public class CharacterGraphDecoderTest
    {
        [TestMethod]
        public void TranspositionTest()
        {
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
            string vocabulary = "elma";
            spellChecker.AddWord(vocabulary);
            List<String> suggestions = spellChecker.GetSuggestions("emla");
            Assert.AreEqual(1, suggestions.Count);
            Assert.AreEqual("elma", suggestions[0]);
        }

        [TestMethod]
        public void SimpleDecodeTest()
        {
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
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
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
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
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
            spellChecker.AddWords("apple", "apples");
            List<ScoredItem<string>> res = spellChecker.GetSuggestionsWithScores("apple");
            foreach (ScoredItem<string> re in res)
            {
                Console.WriteLine(re.Item);
            }
        }

        [TestMethod]
        public void SortedResultSet()
        {
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
            spellChecker.AddWords("apple", "apples", "app", "foo");
            List<String> res = spellChecker.GetSuggestionsSorted("apple");
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual("apple", res[0]);
            Assert.AreEqual("apples", res[1]);
        }

        [TestMethod]
        public void AsciiTolerantTest()
        {
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
            spellChecker.AddWords("şıra", "sıra", "kömür", "giriş");
            CharacterGraphDecoder.ICharMatcher matcher = CharacterGraphDecoder.DIACRITICS_IGNORING_MATCHER;
            List<ScoredItem<String>> res = spellChecker.GetSuggestionsWithScores("komur", matcher);
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("kömür", res[0].Item);

            res = spellChecker.GetSuggestionsWithScores("sıra", matcher);
            Assert.AreEqual(2, res.Count);
            AssertContainsAll(res, "sıra", "şıra");

            res = spellChecker.GetSuggestionsWithScores("gırıs", matcher);
            Assert.AreEqual(1, res.Count);
            AssertContainsAll(res, "giriş");
        }


        [TestMethod]
        public void MultiWordDecodeTest()
        {

            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
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
        [Ignore("Not a unit test.")]
        public void PerformanceTest()
        {
            string r = "Resources/zemberek-parsed-words-min30.txt";
            string[] words = File.ReadAllLines(r, Encoding.UTF8);
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(1);
            spellChecker.AddWords(words);
            long start = DateTime.Now.Ticks;
            int solutionCount = 0;
            int c = 0;
            foreach (string word in words)
            {
                List<String> result = spellChecker.GetSuggestionsSorted(word);
                solutionCount += result.Count;
                if (c++ > 20000)
                {
                    break;
                }
            }
            Console.WriteLine("Elapsed: " + new TimeSpan(DateTime.Now.Ticks - start).TotalMilliseconds);
            Console.WriteLine("Solution count:" + solutionCount);
        }

        private void AssertContainsAll(FloatValueMap<string> set, params string[] words)
        {
            foreach (string word in words)
            {
                Assert.IsTrue(set.Contains(word));
            }
        }

        private void AssertContainsAll(List<ScoredItem<string>> list, params string[] words)
        {
            ISet<string> set = list.Select(s1 => s1.Item).ToHashSet();
            foreach (string word in words)
            {
                Assert.IsTrue(set.Contains(word));
            }
        }

        private void Check1Distance(CharacterGraphDecoder spellChecker, string expected)
        {
            ISet<string> randomDeleted = RandomDelete(expected, 1);
            foreach (string s in randomDeleted)
            {
                FloatValueMap<string> res = spellChecker.Decode(s);
                Assert.AreEqual(1, res.Size(), s);
                Assert.IsTrue(res.Contains(expected), s);
            }

            ISet<string> randomInserted = RandomInsert(expected, 1);
            foreach (string s in randomInserted)
            {
                FloatValueMap<string> res = spellChecker.Decode(s);
                Assert.AreEqual(1, res.Size(), s);
                Assert.IsTrue(res.Contains(expected), s);
            }

            ISet<string> randomSubstitute = RandomSubstitute(expected, 1);
            foreach (string s in randomSubstitute)
            {
                FloatValueMap<String> res = spellChecker.Decode(s);
                Assert.AreEqual(1, res.Size(), s);
                Assert.IsTrue(res.Contains(expected), s);
            }

            ISet<string> transpositions = Transpositions(expected);
            foreach (string s in transpositions)
            {
                FloatValueMap<String> res = spellChecker.Decode(s);
                Assert.AreEqual(1, res.Size(), s);
                Assert.IsTrue(res.Contains(expected), s);
            }
        }

        [TestMethod]
        public void NearKeyCheck()
        {
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(
                1,
                CharacterGraphDecoder.TurkishQNearKeyMap);
            string vocabulary = "elma";
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

        [TestMethod]
        public void StemEndingTest1()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .SetLexicon("bakmak", "gelmek").Build();
            List<string> endings = new List<string> { "acak", "ecek" };
            StemEndingGraph graph = new StemEndingGraph(morphology, endings);
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(graph.StemGraph);
            List<String> res = spellChecker.GetSuggestions("bakcaak");
            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("bakacak", res[0]);
        }

        [TestMethod]
        public void StemEndingTest2()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .SetLexicon("üzmek", "yüz", "güz").Build();
            List<string> endings = new List<string> { "düm" };
            StemEndingGraph graph = new StemEndingGraph(morphology, endings);
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(graph.StemGraph);
            List<ScoredItem<string>> res = spellChecker.GetSuggestionsWithScores("yüzdüm");
            Assert.AreEqual(3, res.Count);
            AssertContainsAll(res, "yüzdüm", "üzdüm", "güzdüm");
        }

        [TestMethod]
        public void StemEndingTest3()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder().SetLexicon("o", "ol", "ola")
                .Build();
            List<String> endings = new List<string> { "arak", "acak" };
            StemEndingGraph graph = new StemEndingGraph(morphology, endings);
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(graph.StemGraph);
            List<ScoredItem<String>> res = spellChecker.GetSuggestionsWithScores("olarak");
            AssertContainsAll(res, "olarak", "olacak", "olaarak");
        }

        [TestMethod]
        public void StemEndingTest()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .SetLexicon("Türkiye", "Bayram").Build();
            List<string> endings = new List<string> { "ında", "de" };
            StemEndingGraph graph = new StemEndingGraph(morphology, endings);
            CharacterGraphDecoder spellChecker = new CharacterGraphDecoder(graph.StemGraph);
            List<ScoredItem<String>> res = spellChecker.GetSuggestionsWithScores("türkiyede");
            AssertContainsAll(res, "türkiyede");
        }
    }
}
