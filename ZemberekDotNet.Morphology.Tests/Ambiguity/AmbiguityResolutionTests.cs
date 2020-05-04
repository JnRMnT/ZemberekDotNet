using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Morphology.Tests.Ambiguity
{
    [TestClass]
    public class AmbiguityResolutionTests
    {

        [TestMethod]
        public void Issue157ShouldNotThrowNPE()
        {
            string input = "Yıldız Kızlar Dünya Şampiyonası FIVB'nin düzenlediği ve 18 "
                + "yaşının altındaki voleybolcuların katılabildiği bir şampiyonadır.";
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate(input);
            Assert.AreEqual(TurkishTokenizer.Default.Tokenize(input).Count, analysis.Size());
            foreach (SentenceWordAnalysis sentenceWordAnalysis in analysis)
            {
                string token = sentenceWordAnalysis.GetWordAnalysis().GetInput();
                SingleAnalysis an = sentenceWordAnalysis.GetBestAnalysis();
                Console.WriteLine(token + " = " + an.FormatLong());
            }
        }

        [TestMethod]
        public void ShouldNotThrowException()
        {
            List<String> lines = TextIO.LoadLines("Resources/corpora/cnn-turk-10k");
            lines = lines.GetRange(0, 1000);
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            foreach (string line in lines)
            {
                List<string> sentences = TurkishSentenceExtractor.Default.FromParagraph(line);
                foreach (string sentence in sentences)
                {
                    morphology.AnalyzeAndDisambiguate(sentence);
                }
            }
        }
    }
}
