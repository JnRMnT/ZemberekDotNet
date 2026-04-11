using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Morphology.Tests
{
    /// <summary>
    /// Tests the cross-module pipeline logic demonstrated in Examples.Pipeline.
    /// </summary>
    [TestClass]
    public class PipelineExamplesTest
    {
        private static TurkishMorphology morphology;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            morphology = TurkishMorphology.CreateWithDefaults();
        }

        [TestMethod]
        public void SentenceExtractorSplitsIntoExpectedCount()
        {
            string paragraph = "Güzel bir gün bugün. Çocuklar oyun oynadı. Hava serin.";
            List<string> sentences = TurkishSentenceExtractor.Default.FromParagraph(paragraph);
            Assert.AreEqual(3, sentences.Count);
        }

        [TestMethod]
        public void ExtractNounLemmasFromSimpleSentence()
        {
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate("kitap masada.");
            List<string> nouns = analysis
                .Where(swa =>
                    !swa.GetBestAnalysis().IsUnknown() &&
                    swa.GetBestAnalysis().GetPos() == PrimaryPos.Noun)
                .Select(swa => swa.GetBestAnalysis().GetLemmas()[0])
                .ToList();

            CollectionAssert.Contains(nouns, "kitap");
            Assert.IsTrue(
                nouns.Any(n => n.StartsWith("masa")),
                "Expected at least one noun lemma starting with 'masa'.");
        }

        [TestMethod]
        public void ExtractVerbLemmasFromSimpleSentence()
        {
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate("Öğrenciler okula gidiyor.");
            List<string> verbs = analysis
                .Where(swa =>
                    !swa.GetBestAnalysis().IsUnknown() &&
                    swa.GetBestAnalysis().GetPos() == PrimaryPos.Verb)
                .Select(swa => swa.GetBestAnalysis().GetLemmas()[0])
                .ToList();

            CollectionAssert.Contains(verbs, "git");
        }

        [TestMethod]
        public void ExtractAdjectiveLemmaFromSimpleSentence()
        {
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate("Güzel bir gün bugün.");
            List<string> lemmas = analysis
                .Where(swa =>
                    !swa.GetBestAnalysis().IsUnknown())
                .Select(swa => swa.GetBestAnalysis().GetLemmas()[0])
                .ToList();

            CollectionAssert.Contains(lemmas, "güzel");
        }

        [TestMethod]
        public void PipelineProducesLemmasForTurkishParagraph()
        {
            string paragraph = "Öğretmenler okula gitti. Kitaplar hazır.";
            List<string> allLemmas = new List<string>();

            foreach (string sentence in TurkishSentenceExtractor.Default.FromParagraph(paragraph))
            {
                allLemmas.AddRange(
                    morphology.AnalyzeAndDisambiguate(sentence)
                        .Where(swa => !swa.GetBestAnalysis().IsUnknown())
                        .Select(swa => swa.GetBestAnalysis().GetLemmas()[0]));
            }

            CollectionAssert.Contains(allLemmas, "öğretmen");
            CollectionAssert.Contains(allLemmas, "okul");
            CollectionAssert.Contains(allLemmas, "git");
            CollectionAssert.Contains(allLemmas, "kitap");
        }
    }
}
