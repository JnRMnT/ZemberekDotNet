using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Normalization.Tests
{
    [TestClass]
    public class SpellCheckerPerformanceTests
    {

        [TestMethod]
        [Ignore("Not a test.")]
        public void CorrectWordFindingTest()
        {
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology);
            TurkishSentenceExtractor extractor = TurkishSentenceExtractor.Default;
            TurkishTokenizer tokenizer = TurkishTokenizer.Default;

            string path = "Resources/spell-checker-test.txt";
            List<string> lines = File.ReadAllLines(path).ToList();
            List<string> sentences = extractor.FromParagraphs(lines);

            Stopwatch sw = Stopwatch.StartNew();

            Histogram<string> incorrectFound = new Histogram<string>();
            Histogram<string> correctFound = new Histogram<string>();

            foreach (string sentence in sentences)
            {
                List<Token> tokens = tokenizer.Tokenize(sentence);
                foreach (Token token in tokens)
                {
                    string text = token.GetText();
                    if (!spellChecker.Check(text))
                    {
                        incorrectFound.Add(text);
                    }
                    else
                    {
                        correctFound.Add(text);
                    }
                }
            }
            Log.Info("Elapsed = {0}", sw.ElapsedMilliseconds);
            Log.Info("Incorrect (total/unique) = {0} / {1}", incorrectFound.TotalCount(),
                incorrectFound.Size());
            Log.Info("Correct (total/unique) = {0} / {1}", correctFound.TotalCount(), correctFound.Size());
            incorrectFound.SaveSortedByCounts("incorrect.txt", " : ");
            correctFound.SaveSortedByCounts("correct.txt", " : ");

            /*
                    Path lmPath = Paths.get(ClassLoader.getSystemResource("lm-bigram.slm").toURI());
                    SmoothLm model = SmoothLm.builder(lmPath.toFile()).build();
            */
        }
    }
}
