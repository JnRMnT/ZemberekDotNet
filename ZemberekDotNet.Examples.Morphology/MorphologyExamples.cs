using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Generator;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Examples.Morphology
{
    /// <summary>
    /// Demonstrates ZemberekDotNet.Morphology:
    ///   1. Single-word analysis
    ///   2. Sentence disambiguation
    ///   3. LINQ-style lemma extraction
    ///
    /// No Java or JVM required — pure .NET Standard 2.1.
    /// </summary>
    public class MorphologyExamples
    {
        public static void Main()
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();

            SingleWordAnalysis(morphology);
            SentenceDisambiguation(morphology);
            LinqLemmaExtraction(morphology);
            WordGeneratorDemo(morphology);

            Console.ReadLine();
        }

        // ------------------------------------------------------------------
        // 1. Analyze a single word — returns all possible parses.
        // ------------------------------------------------------------------
        static void SingleWordAnalysis(TurkishMorphology morphology)
        {
            Console.WriteLine("=== Single-word analysis ===");
            WordAnalysis result = morphology.Analyze("kitaplara");
            foreach (SingleAnalysis analysis in result.GetAnalysisResults())
            {
                Console.WriteLine(analysis.FormatLexical());
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 2. Analyze a full sentence with context-aware disambiguation.
        // ------------------------------------------------------------------
        static void SentenceDisambiguation(TurkishMorphology morphology)
        {
            Console.WriteLine("=== Sentence disambiguation ===");
            string sentence = "Güzel bir gün bugün.";
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate(sentence);

            foreach (SentenceWordAnalysis swa in analysis)
            {
                SingleAnalysis best = swa.GetBestAnalysis();
                string input = swa.GetWordAnalysis().GetInput();
                string lemma = best.IsUnknown() ? "?" : best.GetLemmas()[0];
                Console.WriteLine($"  {input,-12} lemma: {lemma,-10}  parse: {best.FormatLexical()}");
            }
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 3. LINQ-style: collect root lemmas for all known words in a sentence.
        // ------------------------------------------------------------------
        static void LinqLemmaExtraction(TurkishMorphology morphology)
        {
            Console.WriteLine("=== LINQ lemma extraction ===");
            string sentence = "Öğretmenler okula gidiyorlar.";
            List<string> lemmas = morphology
                .AnalyzeAndDisambiguate(sentence)
                .Where(swa => !swa.GetBestAnalysis().IsUnknown())
                .Select(swa => swa.GetBestAnalysis().GetLemmas()[0])
                .ToList();

            Console.WriteLine(string.Join(", ", lemmas));
            Console.WriteLine();
        }

        // ------------------------------------------------------------------
        // 4. Generate all inflected surface forms of a word from morpheme IDs.
        //    Demonstrates Turkish agglutination — one root, dozens of forms.
        // ------------------------------------------------------------------
        static void WordGeneratorDemo(TurkishMorphology morphology)
        {
            WordGenerator gen = morphology.GetWordGenerator();
            RootLexicon lexicon = morphology.GetLexicon();

            Console.WriteLine("=== Word generation: noun cases (kitap) ===");
            DictionaryItem kitap = lexicon.GetMatchingItems("kitap")
                .First(i => i.primaryPos == PrimaryPos.Noun);

            var nounForms = new (string label, string[] morphemes)[]
            {
                ("Accusative  (sg)", new[] { "Acc" }),
                ("Dative      (sg)", new[] { "Dat" }),
                ("Locative    (sg)", new[] { "Loc" }),
                ("Ablative    (sg)", new[] { "Abl" }),
                ("Genitive    (sg)", new[] { "Gen" }),
                ("Plural",           new[] { "A3pl" }),
                ("Plural+Accusative", new[] { "A3pl", "Acc" }),
                ("Plural+Dative",     new[] { "A3pl", "Dat" }),
                ("Plural+Locative",   new[] { "A3pl", "Loc" }),
            };

            foreach (var (label, morphemes) in nounForms)
            {
                List<WordGenerator.Result> results = gen.Generate(kitap, morphemes);
                string surface = results.Count > 0 ? results[0].surface : "(no result)";
                Console.WriteLine($"  {label,-26}: {surface}");
            }
            Console.WriteLine();

            Console.WriteLine("=== Word generation: verb conjugation (okumak, present progressive) ===");
            DictionaryItem oku = lexicon.GetMatchingItems("okumak")
                .First(i => i.primaryPos == PrimaryPos.Verb);

            var verbForms = new (string label, string[] morphemes)[]
            {
                ("1st singular", new[] { "Prog1", "A1sg" }),
                ("2nd singular", new[] { "Prog1", "A2sg" }),
                ("3rd singular", new[] { "Prog1", "A3sg" }),
                ("1st plural",   new[] { "Prog1", "A1pl" }),
                ("2nd plural",   new[] { "Prog1", "A2pl" }),
                ("3rd plural",   new[] { "Prog1", "A3pl" }),
            };

            foreach (var (label, morphemes) in verbForms)
            {
                List<WordGenerator.Result> results = gen.Generate(oku, morphemes);
                string surface = results.Count > 0 ? results[0].surface : "(no result)";
                Console.WriteLine($"  {label,-14}: {surface}");
            }
            Console.WriteLine();
        }
    }
}