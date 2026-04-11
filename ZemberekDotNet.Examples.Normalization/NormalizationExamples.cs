using System;
using System.Collections.Generic;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Normalization;

namespace ZemberekDotNet.Examples.Normalization
{
    /// <summary>
    /// Demonstrates ZemberekDotNet.Normalization:
    ///   1. Spell check — decide whether a word is valid Turkish
    ///   2. Spell suggestions — offer ranked corrections for misspelled words
    ///   3. Sentence-level spell check — highlight every misspelled token
    ///
    /// No Java or JVM required — pure .NET Standard 2.1.
    /// A compressed unigram language model is bundled with the package.
    /// </summary>
    public class NormalizationExamples
    {
        public static void Main()
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            TurkishSpellChecker spellChecker = new TurkishSpellChecker(morphology);

            SpellCheckWords(spellChecker);
            SpellSuggestions(spellChecker);
            SentenceSpellCheck(spellChecker);

            Console.ReadLine();
        }

        // ------------------------------------------------------------------
        // 1. Check individual words for correctness.
        // ------------------------------------------------------------------
        static void SpellCheckWords(TurkishSpellChecker spellChecker)
        {
            Console.WriteLine("=== Spell check: word validation ===");

            string[] correct = { "kitap", "evlerden", "gidiyorum", "Ankara'ya", "okullar" };
            string[] misspelled = { "ktap", "evledrn", "gidiyourm", "anakar", "okulalr" };

            Console.WriteLine("  Correct words:");
            foreach (string word in correct)
            {
                Console.WriteLine($"    {word,-18} -> {(spellChecker.Check(word) ? "OK" : "wrong")}");
            }

            Console.WriteLine("  Misspelled words:");
            foreach (string word in misspelled)
            {
                Console.WriteLine($"    {word,-18} -> {(spellChecker.Check(word) ? "OK" : "wrong")}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 2. Ranked suggestions for misspelled words.
        // ------------------------------------------------------------------
        static void SpellSuggestions(TurkishSpellChecker spellChecker)
        {
            Console.WriteLine("=== Spell suggestions ===");

            string[] typos = { "ktap", "evledrn", "gidiyourm" };
            foreach (string typo in typos)
            {
                List<string> suggestions = spellChecker.SuggestForWord(typo);
                string top = suggestions.Count > 0
                    ? string.Join(", ", suggestions.GetRange(0, Math.Min(3, suggestions.Count)))
                    : "(no suggestions)";
                Console.WriteLine($"  \"{typo}\" -> {top}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 3. Scan every word in a sentence, flag misspellings.
        // ------------------------------------------------------------------
        static void SentenceSpellCheck(TurkishSpellChecker spellChecker)
        {
            Console.WriteLine("=== Sentence spell check ===");

            string sentence = "Bugün okula gittim ve çok güzl bir gün geçirdim .";
            List<string> words = TurkishSpellChecker.TokenizeForSpelling(sentence);

            Console.WriteLine($"  Input: \"{sentence}\"");
            Console.Write("  Tokens: ");
            foreach (string word in words)
            {
                bool ok = spellChecker.Check(word);
                Console.Write(ok ? $"{word} " : $"[{word}?] ");
            }
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
