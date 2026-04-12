using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Classification;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Apps.FastText
{
    public class ClassificationConsole : ConsoleApp<ClassificationConsole>
    {
        [Parameter("--model", "-m",
            Required = Required.Yes,
            Description = "Model file.")]
        string model;

        [Parameter("--predictionCount", "-k",
            Description = "Amount of top predictions.")]
        int predictionCount = 3;

        [Parameter("--preprocess", "-p",
            Description = "Applies preprocessing to the input. Values: TOKENIZED or LEMMA.")]
        string preprocess = "TOKENIZED";

        TurkishMorphology morphology;

        enum Preprocessor
        {
            TOKENIZED,
            LEMMA
        }

        public override string Description()
        {
            return "Generates a FastTextClassifier from the given model and makes predictions "
                + "for input sentences. By default it applies tokenization and lower-casing. "
                + "If model is generated with lemmatized text use --preprocess LEMMA.";
        }

        public override void Run()
        {
            if (!Enum.TryParse(preprocess, true, out Preprocessor preprocessor))
            {
                throw new ArgumentException("Invalid preprocess value: " + preprocess + ". Allowed values: TOKENIZED, LEMMA");
            }

            Console.WriteLine("Loading classification model...");
            FastTextClassifier classifier = FastTextClassifier.Load(model);

            if (preprocessor == Preprocessor.LEMMA)
            {
                morphology = TurkishMorphology.Builder(RootLexicon.GetDefault()).Build();
            }

            Console.WriteLine("Preprocessing type = " + preprocessor);
            Console.WriteLine("Enter sentence:");

            string input = Console.ReadLine();
            while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Empty line cannot be processed.");
                    input = Console.ReadLine();
                    continue;
                }

                string processed = preprocessor == Preprocessor.TOKENIZED
                    ? string.Join(" ", TurkishTokenizer.Default.TokenizeToStrings(input))
                    : ReplaceWordsWithLemma(input);

                processed = RemoveNonWords(processed).ToLower(Turkish.Locale);
                Console.WriteLine("Processed Input = " + processed);

                if (processed.Trim().Length == 0)
                {
                    Console.WriteLine("Processing result is empty. Enter new sentence.");
                    input = Console.ReadLine();
                    continue;
                }

                List<ScoredItem<string>> res = classifier.Predict(processed, predictionCount);
                List<string> predictedCategories = new List<string>();
                foreach (ScoredItem<string> item in res)
                {
                    predictedCategories.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0} ({1:N6})",
                        item.Item.Replace("__label__", ""),
                        item.Score));
                }
                Console.WriteLine("Predictions   = " + string.Join(", ", predictedCategories));
                Console.WriteLine();

                input = Console.ReadLine();
            }
        }

        private string ReplaceWordsWithLemma(string sentence)
        {
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate(sentence);
            List<string> lemmas = new List<string>();
            foreach (SentenceWordAnalysis item in analysis)
            {
                SingleAnalysis best = item.GetBestAnalysis();
                if (best.IsUnknown())
                {
                    lemmas.Add(item.GetWordAnalysis().GetInput());
                    continue;
                }

                List<string> bestLemmas = best.GetLemmas();
                lemmas.Add(bestLemmas.Count == 0 ? item.GetWordAnalysis().GetInput() : bestLemmas.Last());
            }

            return string.Join(" ", lemmas);
        }

        private string RemoveNonWords(string sentence)
        {
            List<Token> docTokens = TurkishTokenizer.Default.Tokenize(sentence);
            List<string> reduced = new List<string>(docTokens.Count);
            foreach (Token token in docTokens)
            {
                Token.Type type = token.GetTokenType();
                if (
                    type == Token.Type.PercentNumeral ||
                    type == Token.Type.Number ||
                    type == Token.Type.Punctuation ||
                    type == Token.Type.RomanNumeral ||
                    type == Token.Type.Time ||
                    type == Token.Type.UnknownWord ||
                    type == Token.Type.Unknown)
                {
                    if (!token.GetText().Contains("__"))
                    {
                        continue;
                    }
                }
                reduced.Add(token.GetText());
            }

            return string.Join(" ", reduced);
        }

        public static void Main(string[] args)
        {
            new ClassificationConsole().Execute(args);
        }
    }
}