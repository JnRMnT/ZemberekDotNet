using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class SpeedTest
    {
        [TestMethod]
        [Ignore("Speed Test.")]
        public void TestNewsCorpus()
        {
            string p = "Resources/corpora/cnn-turk-10k";
            List<string> sentences = GetSentences(p);
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            Stopwatch sw = Stopwatch.StartNew();

            int tokenCount = 0;
            int noAnalysis = 0;
            int sentenceCount = 0;
            Histogram<string> failedWords = new Histogram<string>(100000);
            foreach (string sentence in sentences)
            {
                List<Token> tokens = TurkishTokenizer.Default.Tokenize(sentence);
                foreach (Token token in tokens)
                {
                    if (token.GetTokenType() == Token.Type.Punctuation)
                    {
                        continue;
                    }
                    tokenCount++;
                    WordAnalysis results = morphology.Analyze(token.GetText());
                    if (!results.IsCorrect())
                    {
                        noAnalysis++;
                        failedWords.Add(token.GetText());
                    }
                }
                sentenceCount++;
                if (sentenceCount % 2000 == 0)
                {
                    Log.Info("{0} tokens analyzed.", tokenCount);
                }
            }
            sw.Stop();
            double seconds = sw.ElapsedMilliseconds / 1000d;
            double speed = tokenCount / seconds;
            double parseRatio = 100 - (noAnalysis * 100d / tokenCount);
            Log.Info("{0}Elapsed = {1:N2} seconds", Environment.NewLine, seconds);
            Log.Info("{0}Token Count (No Punc) = {1} {2}Parse Ratio = {3:N4}{4}Speed = {5:N2} tokens/sec{6}",
               Environment.NewLine, tokenCount, Environment.NewLine, parseRatio, Environment.NewLine, speed, Environment.NewLine);
            Log.Info("Saving Unknown Tokens");
            failedWords.SaveSortedByCounts("unk.freq", " ");
            failedWords.SaveSortedByKeys("unk", " ", Turkish.StringComparatorAsc);
        }


        private static void TestForVisualVm(string p, TurkishMorphology analyzer)
        {
            //Path p = Paths.get("/media/aaa/Data/corpora/me-sentences/www.aljazeera.com.tr/2018-02-22");
            List<String> sentences = GetSentences(p);

            Stopwatch sw = Stopwatch.StartNew();

            int tokenCount = 0;
            int noAnalysis = 0;
            int sentenceCount = 0;
            foreach (string sentence in sentences)
            {
                List<Token> tokens = TurkishTokenizer.Default.Tokenize(sentence);
                foreach (Token token in tokens)
                {
                    tokenCount++;
                    WordAnalysis results = analyzer.Analyze(token.GetText());
                    if (!results.IsCorrect())
                    {
                        noAnalysis++;
                    }
                }
                sentenceCount++;
                if (sentenceCount % 2000 == 0)
                {
                    Log.Info("{0} tokens analyzed.", tokenCount);
                }
            }
            sw.Stop();
            double seconds = sw.ElapsedMilliseconds / 1000d;
            double speed = tokenCount / seconds;
            double parseRatio = 100 - (noAnalysis * 100d / tokenCount);
            Console.WriteLine(analyzer.GetCache());
            Log.Info("{0}Elapsed = {1:F2} seconds", Environment.NewLine, seconds);
            Log.Info("{0}Token Count (No Punc) = {1} {2}Parse Ratio = {3:F4}{4}Speed = {5:F2} tokens/sec{6}",
                tokenCount, Environment.NewLine, parseRatio, Environment.NewLine, speed, Environment.NewLine);
        }

        public static void DummyMain(string[] args)
        {
            string p = "Resources/corpora/cnn-turk-10k";

            TurkishMorphology analyzer = TurkishMorphology.CreateWithDefaults();
            for (int i = 0; i < 10; i++)
            {
                TestForVisualVm(p, analyzer);
                analyzer.InvalidateCache();
                Console.Read();
            }
        }

        private static List<string> GetSentences(string p)
        {
            string[] lines = File.ReadAllLines(p, Encoding.UTF8);
            lines = lines.Select(s =>
            Regex.Replace(
            Regex.Replace(
            Regex.Replace(s, "\\s+|\\u00a0", " "),
                    "[\\u00ad]", ""),
                    "[…]", "...")
            ).ToArray();
            return TurkishSentenceExtractor.Default.FromParagraphs(lines);
        }
    }
}
