using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Examples.Tokenization
{
    /// <summary>
    /// Demonstrates ZemberekDotNet.Tokenization:
    ///   1. Sentence boundary detection from a paragraph
    ///   2. Token-level analysis of a sentence (word, punctuation, number, etc.)
    ///   3. Processing a multi-sentence document
    ///
    /// No Java or JVM required — pure .NET Standard 2.1.
    /// </summary>
    public class TokenizationExamples
    {
        public static void Main()
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            SentenceSplitting();
            TokenAnalysis();
            DocumentProcessing();

            Console.ReadLine();
        }

        // ------------------------------------------------------------------
        // 1. Split a paragraph into individual sentences.
        // ------------------------------------------------------------------
        static void SentenceSplitting()
        {
            Console.WriteLine("=== Sentence splitting ===");
            string paragraph = "Merhaba dünya. Bugün iyi bir gün. Değil mi?";
            List<string> sentences = TurkishSentenceExtractor.Default.FromParagraph(paragraph);
            for (int i = 0; i < sentences.Count; i++)
            {
                Console.WriteLine($"  [{i + 1}] {sentences[i]}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 2. Tokenize with type information (Word, Punctuation, Number, …).
        // ------------------------------------------------------------------
        static void TokenAnalysis()
        {
            Console.WriteLine("=== Token analysis ===");
            string sentence = "Ankara'ya 3 saatte gidilir.";
            List<Token> tokens = TurkishTokenizer.Default.Tokenize(sentence);
            foreach (Token token in tokens)
            {
                Console.WriteLine($"  {token.GetText(),-20} type: {token.GetTokenType()}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 3. Process a full document: split lines → sentences → tokens.
        // ------------------------------------------------------------------
        static void DocumentProcessing()
        {
            Console.WriteLine("=== Document processing ===");
            string document = "Türkiye güzel bir ülkedir.\nİstanbul en kalabalık şehirdir. Nüfusu 15 milyonu geçmiştir.";

            List<string> allSentences = TurkishSentenceExtractor.Default.FromDocument(document);
            Console.WriteLine($"  {allSentences.Count} sentences found.");

            int totalTokens = allSentences
                .Sum(s => TurkishTokenizer.Default.TokenizeToStrings(s).Count);
            Console.WriteLine($"  {totalTokens} tokens total.");
            Console.WriteLine();
        }
    }
}
