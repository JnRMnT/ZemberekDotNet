using System;
using System.Collections.Generic;
using ZemberekDotNet.LangID;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Examples.LangID
{
    /// <summary>
    /// Demonstrates ZemberekDotNet.LangID:
    ///   1. Basic language identification using all built-in models
    ///   2. Turkish-focused group model (tr_group) for faster TR/EN/other discrimination
    ///   3. Confidence scores for multi-language text
    ///   4. ContainsLanguage — detect whether a mixed-language text contains a specific language
    ///
    /// No Java or JVM required — pure .NET Standard 2.1.
    /// Models are embedded resources; no external files needed.
    /// </summary>
    public class LangIDExamples
    {
        public static void Main()
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            BasicIdentification();
            TurkishGroupModel();
            ConfidenceScores();
            ContainsLanguage();
            MixedLanguageParagraphScanner();

            Console.ReadLine();
        }

        // ------------------------------------------------------------------
        // 1. Identify the language of several texts using all built-in models.
        // ------------------------------------------------------------------
        static void BasicIdentification()
        {
            Console.WriteLine("=== Basic language identification ===");
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();

            string[] samples =
            {
                "merhaba dünya ve tüm gezegenler",
                "hello world and all the planets",
                "Hola mundo y todos los planetas",
                "Bonjour tout le monde et toutes les planètes",
                "Salam dünya və bütün planetlərin"
            };

            foreach (string text in samples)
            {
                string lang = lid.Identify(text);
                Console.WriteLine($"  [{lang}]  {text}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 2. Use the Turkish-focused model group for lower latency when you
        //    only care about distinguishing Turkish from a handful of languages.
        // ------------------------------------------------------------------
        static void TurkishGroupModel()
        {
            Console.WriteLine("=== Turkish group model (tr_group) ===");
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModelGroup("tr_group");

            Console.WriteLine($"  Supported languages: {string.Join(", ", lid.GetLanguages())}");
            Console.WriteLine($"  Turkish text : {lid.Identify("Güzel bir gün bugün.")}");
            Console.WriteLine($"  Spanish text : {lid.Identify("Hola mundo")} (→ 'unk' = not in group)");
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 3. Retrieve per-language confidence scores.
        // ------------------------------------------------------------------
        static void ConfidenceScores()
        {
            Console.WriteLine("=== Confidence scores ===");
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
            string text = "Türkiye güzel bir ülkedir.";

            List<LanguageIdentifier.IdResult> scores = lid.GetScores(text, maxSampleCount: -1);
            Console.WriteLine($"  Input: \"{text}\"");
            int top = Math.Min(5, scores.Count);
            for (int i = 0; i < top; i++)
            {
                Console.WriteLine($"  [{i + 1}] {scores[i].id,-4}  score: {scores[i].score:F4}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 4. Check whether a mixed-language passage contains a specific language.
        // ------------------------------------------------------------------
        static void ContainsLanguage()
        {
            Console.WriteLine("=== ContainsLanguage (mixed text) ===");
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();
            string mixed = "merhaba dünya ve tüm gezegenler Hola mundo y todos los planetas";

            Console.WriteLine($"  Contains TR: {lid.ContainsLanguage(mixed, "tr", 20)}");  // True
            Console.WriteLine($"  Contains ES: {lid.ContainsLanguage(mixed, "es", 20)}");  // True
            Console.WriteLine($"  Contains AR: {lid.ContainsLanguage(mixed, "ar", 20)}");  // False
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 5. Scan each sentence of a paragraph and annotate with its language.
        //    Real-world use case: language tagging of multilingual social media.
        // ------------------------------------------------------------------
        static void MixedLanguageParagraphScanner()
        {
            Console.WriteLine("=== Mixed-language paragraph scanner ===");
            LanguageIdentifier lid = LanguageIdentifier.FromInternalModels();

            string paragraph =
                "Türkiye harika bir ülkedir. " +
                "Turkey is a beautiful country. " +
                "Türk mutfağı dünyaca ünlüdür. " +
                "Istanbul is one of the largest cities in the world. " +
                "Bu şehirde milyonlarca insan yaşıyor.";

            List<string> sentences = TurkishSentenceExtractor.Default.FromParagraph(paragraph);
            foreach (string sentence in sentences)
            {
                string lang = lid.Identify(sentence);
                Console.WriteLine($"  [{lang}] {sentence}");
            }
            Console.WriteLine();
        }
    }
}
