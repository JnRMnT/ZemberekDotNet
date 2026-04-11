using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.LangID;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Examples.Pipeline
{
    /// <summary>
    /// Demonstrates a multi-module pipeline that chains three ZemberekDotNet packages:
    ///
    ///   Tokenization  →  Morphology  →  LangID
    ///
    ///   1. Sentence splitter splits a paragraph into sentences
    ///   2. Morphological analysis extracts each word's lemma and POS tag
    ///   3. POS fingerprint groups nouns, verbs and adjectives per sentence
    ///   4. Language guard — skip non-Turkish sentences before morphology
    ///
    /// No Java or JVM required — pure .NET Standard 2.1.
    /// </summary>
    public class PipelineExamples
    {
        public static void Main()
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();

            PosFingerprint(morphology);
            LanguageGuardedPipeline(morphology, lid);

            Console.ReadLine();
        }

        // ------------------------------------------------------------------
        // 1. Split a paragraph into sentences; for each Turkish sentence
        //    extract nouns, verbs and adjectives (lemmatized).
        // ------------------------------------------------------------------
        static void PosFingerprint(TurkishMorphology morphology)
        {
            Console.WriteLine("=== POS fingerprint per sentence ===");

            string paragraph =
                "Güzel bir sonbahar günü, çocuklar parkta oyun oynadı. " +
                "Öğretmenler okula erken geldi ve dersleri hazırladı. " +
                "Kitaplar çantada bekliyor.";

            List<string> sentences = TurkishSentenceExtractor.Default.FromParagraph(paragraph);

            foreach (string sentence in sentences)
            {
                Console.WriteLine($"  Sentence: {sentence}");
                SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate(sentence);

                List<string> nouns = ExtractLemmasByPos(analysis, PrimaryPos.Noun);
                List<string> verbs = ExtractLemmasByPos(analysis, PrimaryPos.Verb);
                List<string> adjs  = ExtractLemmasByPos(analysis, PrimaryPos.Adjective);

                Console.WriteLine($"    Nouns     : {(nouns.Any() ? string.Join(", ", nouns) : "-")}");
                Console.WriteLine($"    Verbs     : {(verbs.Any() ? string.Join(", ", verbs) : "-")}");
                Console.WriteLine($"    Adjectives: {(adjs.Any()  ? string.Join(", ", adjs)  : "-")}");
                Console.WriteLine();
            }
        }

        // ------------------------------------------------------------------
        // 2. Language guard: identify each sentence's language first,
        //    then run morphology only on Turkish sentences.
        // ------------------------------------------------------------------
        static void LanguageGuardedPipeline(TurkishMorphology morphology, LanguageIdentifier lid)
        {
            Console.WriteLine("=== Language-guarded morphology pipeline ===");

            string[] sentences =
            {
                "Türkiye güzel bir ülkedir.",
                "This sentence is written in English.",
                "Öğrenciler sınavlara çalışıyor.",
                "Bonjour tout le monde.",
                "Kış aylarında hava çok soğuk olur.",
            };

            foreach (string sentence in sentences)
            {
                string lang = lid.Identify(sentence);
                if (lang != "tr")
                {
                    Console.WriteLine($"  [{lang}] SKIPPED  : {sentence}");
                    continue;
                }

                SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate(sentence);
                List<string> lemmas = analysis
                    .Where(swa => !swa.GetBestAnalysis().IsUnknown())
                    .Select(swa => swa.GetBestAnalysis().GetLemmas()[0])
                    .ToList();

                Console.WriteLine($"  [tr] Lemmas   : {string.Join(", ", lemmas)}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        static List<string> ExtractLemmasByPos(SentenceAnalysis analysis, PrimaryPos pos)
        {
            return analysis
                .Where(swa =>
                    !swa.GetBestAnalysis().IsUnknown() &&
                    swa.GetBestAnalysis().GetPos() == pos)
                .Select(swa => swa.GetBestAnalysis().GetLemmas()[0])
                .Distinct()
                .ToList();
        }
    }
}
