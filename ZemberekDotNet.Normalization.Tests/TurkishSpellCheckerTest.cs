using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.LM;
using ZemberekDotNet.LM.Compression;
using ZemberekDotNet.Morphology;

namespace ZemberekDotNet.Normalization.Tests
{
    [TestClass]
    public class TurkishSpellCheckerTest
    {
        private static Random random = new Random(1);

        private static string ApplyDeformation(string word)
        {
            if (word.Length < 3)
            {
                return word;
            }
            int deformation = random.Next(4);
            StringBuilder sb = new StringBuilder(word);
            switch (deformation)
            {
                case 0: // substitution
                    int start = random.Next(sb.Length);
                    sb[start] = 'x';
                    return sb.ToString();
                case 1: // insertion
                    sb.Insert(random.Next(sb.Length + 1), "x");
                    return sb.ToString();
                case 2: // deletion
                    sb.Remove(random.Next(sb.Length), 1);
                    return sb.ToString();
                case 3: // transposition
                    int i = random.Next(sb.Length - 2);
                    char tmp = sb[i];
                    sb[i] = sb[i + 1];
                    sb[i + 1] = tmp;
                    return sb.ToString();
            }
            return word;
        }

        [TestMethod]
        public void CheckProperNounsTest()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
        .DisableCache()
        .SetLexicon("Ankara", "Iphone [Pr:ayfon]", "Google [Pr:gugıl]")
        .Build();
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology);

            string[] correct = {"Ankara", "ANKARA", "Ankara'da", "ANKARA'DA", "ANKARA'da",
        "Iphone'umun", "Google'dan", "Iphone", "Google", "Google'sa"};

            foreach (string input in correct)
            {
                Assert.IsTrue(spellChecker.Check(input), "Fail at " + input);
            }

            string[] fail = { "Ankara'", "ankara", "AnKARA", "Ankarada", "ankara'DA", "-Ankara" };
            foreach (string input in fail)
            {
                Assert.IsFalse(spellChecker.Check(input), "Fail at " + input);
            }
        }

        //TODO: check for ordinals.
        [TestMethod]
        public void FormatNumbersTest()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
                .DisableCache()
                .SetLexicon("bir [P:Num]", "dört [P:Num;A:Voicing]", "üç [P:Num]", "beş [P:Num]")
                .Build();

            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology);

            String[] inputs = {
        "1'e", "4'ten", "123'ü", "12,5'ten",
        "1'E", "4'TEN", "123'Ü", "12,5'TEN",
        "%1", "%1'i", "%1,3'ü",
    };

            foreach (string input in inputs)
            {
                Assert.IsTrue(spellChecker.Check(input), "Fail at " + input);
            }
        }

        [TestMethod]
        [Ignore("Slow. Uses actual data.")]
        public void suggestWordPerformanceStemEnding()
        {
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology);
            INgramLanguageModel lm = GetLm("lm-unigram.slm");
            Run(spellChecker, lm);
        }

        [TestMethod]
        [Ignore("Slow. Uses actual data.")]
        public void SuggestWord1()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder()
            .SetLexicon("Türkiye", "Bayram").Build();
            List<string> endings = new List<string> { "ında", "de" };
            StemEndingGraph graph = new StemEndingGraph(morphology, endings);
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology, graph.StemGraph);
            INgramLanguageModel lm = GetLm("lm-unigram.slm");
            Check(spellChecker, lm, "Türkiye'de", "Türkiye'de");
            // TODO: "Bayramı'nda" fails.
        }

        [TestMethod]
        public void SuggestVerb1()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder().SetLexicon("okumak").Build();

            List<String> endings = new List<string> { "dum" };
            StemEndingGraph graph = new StemEndingGraph(morphology, endings);
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology, graph.StemGraph);

            List<String> res = spellChecker.SuggestForWord("okudm");
            Assert.IsTrue(res.Contains("okudum"));
        }


        [TestMethod]
        public void CheckVerb1()
        {
            TurkishMorphology morphology = TurkishMorphology.Builder().SetLexicon("okumak").Build();

            List<String> endings = new List<string> { "dum" };
            StemEndingGraph graph = new StemEndingGraph(morphology, endings);
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology, graph.StemGraph);

            Assert.IsTrue(spellChecker.Check("okudum"));
        }

        private void Check(TurkishSpellChecker spellChecker, INgramLanguageModel lm, string input,
            string expected)
        {
            List<string> res = spellChecker.SuggestForWord(input, lm);
            Assert.IsTrue(res.Contains(expected));
        }

        [TestMethod]
        [Ignore("Slow. Uses actual data.")]
        public void SuggestWordPerformanceWord()
        {
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            CharacterGraph graph = new CharacterGraph();
            string r = "../data/zemberek-oflazer/oflazer-zemberek-parsed.txt";
            List<string> words = File.ReadAllLines(r, Encoding.UTF8).ToList().GetRange(0, 1000_000);
            Log.Info("Total word count = {0}", words.Count);
            words.ForEach(s => graph.AddWord(s, Node.TypeWord));
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology, graph);
            INgramLanguageModel lm = GetLm("lm-unigram.slm");
            Run(spellChecker, lm);
        }

        private void Run(TurkishSpellChecker spellChecker, INgramLanguageModel lm)
        {
            Log.Info("Node count = {0}", spellChecker.Decoder.GetGraph().GetAllNodes().Count);
            Log.Info("Node count with single connection= {0}",
                spellChecker.Decoder.GetGraph().GetAllNodes(a => a.GetAllChildNodes().Count == 1).Count);

            List<string> words = TextIO.LoadLines("Resources/10000_frequent_turkish_word");
            int c = 0;
            Stopwatch sw = Stopwatch.StartNew();
            foreach (string word in words)
            {
                List<string> suggestions = spellChecker.SuggestForWord(word, lm);
                c += suggestions.Count;
            }
            Log.Info("Elapsed = {0} count = {1} ", sw.ElapsedMilliseconds, c);
        }

        private INgramLanguageModel GetLm(string resource)
        {
            return SmoothLm.Builder(resource).Build();
        }

        [TestMethod]
        [Ignore("Slow. Uses actual data.")]
        public void RunSentence()
        {
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology);
            INgramLanguageModel lm = GetLm("lm-bigram.slm");
            List<String> sentences = TextIO.LoadLines("Resources/spell-checker-test-small.txt");
            using (StreamWriter pw = new StreamWriter("bigram-test-result.txt"))
            {
                foreach (string sentence in sentences)
                {
                    pw.WriteLine(sentence);
                    List<string> input = TurkishSpellChecker.TokenizeForSpelling(sentence);
                    for (int i = 0; i < input.Count; i++)
                    {
                        string left = i == 0 ? null : input[i - 1];
                        string right = i == input.Count - 1 ? null : input[i + 1];
                        string word = input[i];
                        string deformed = ApplyDeformation(word);
                        List<string> res = spellChecker.SuggestForWord(deformed, left, right, lm);
                        pw.WriteLine(
                            string.Format("{0} {1}[{2}] {3} -> {4}", left, deformed, word, right, res.ToString()));
                    }
                    pw.WriteLine();
                }
            }
        }
    }
}
