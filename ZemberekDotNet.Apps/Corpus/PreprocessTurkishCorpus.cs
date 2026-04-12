using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Apps.Corpus
{
    public class PreprocessTurkishCorpus : ConsoleApp<PreprocessTurkishCorpus>
    {
        [Parameter("--input", "-i", Required = Required.Yes,
            Description = "Input corpus file or directory.")]
        string input;

        [Parameter("--output", "-o", Required = Required.Yes,
            Description = "Output file (for file input) or output directory (for directory input).")]
        string output;

        [Parameter("--operation", "-p",
            Description = "Preprocessing operation: TOKENIZED or LEMMA.")]
        string operationArg = "TOKENIZED";

        [Parameter("--lowercase", "-l",
            Description = "If used, lower-cases processed sentences.")]
        bool lowercase = true;

        private TurkishTokenizer tokenizer;
        private TurkishSentenceExtractor sentenceExtractor;
        private TurkishMorphology morphology;
        private Operation operation = Operation.TOKENIZED;

        private static readonly Regex MultipleWhitespace = new Regex("\\s+", RegexOptions.Compiled);

        enum Operation
        {
            TOKENIZED,
            LEMMA
        }

        public override string Description()
        {
            return "Applies Turkish sentence boundary detection and tokenization to a corpus file "
                + "or a directory of corpus files. Lines starting with '<' are ignored. "
                + "It normalizes whitespace and removes soft hyphens. Sentences that contain "
                + "combining diacritic symbols are ignored.";
        }

        public override void Run()
        {
            if (!Enum.TryParse(operationArg, true, out operation))
            {
                throw new ArgumentException("Invalid operation value: " + operationArg + ". Allowed values: TOKENIZED, LEMMA");
            }

            tokenizer = TurkishTokenizer.Default;
            sentenceExtractor = TurkishSentenceExtractor.Default;

            if (operation == Operation.LEMMA)
            {
                morphology = TurkishMorphology.Builder(RootLexicon.GetDefault()).Build();
            }

            if (File.Exists(input))
            {
                ProcessFile(input, output);
                return;
            }

            if (!Directory.Exists(input))
            {
                throw new IOException("Input path does not exist: " + input);
            }

            Directory.CreateDirectory(output);
            foreach (string file in Directory.GetFiles(input, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(input, file);
                string outFile = Path.Combine(output, relative);
                string outDir = Path.GetDirectoryName(outFile);
                if (!string.IsNullOrEmpty(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }
                ProcessFile(file, outFile);
            }
        }

        private void ProcessFile(string inFile, string outFile)
        {
            List<string> processed = new List<string>();
            foreach (string line in File.ReadLines(inFile, Encoding.UTF8))
            {
                if (line.StartsWith("<"))
                {
                    continue;
                }

                string normalized = NormalizeLine(line);
                if (normalized.Length == 0)
                {
                    continue;
                }

                List<string> sentences = sentenceExtractor.FromParagraph(normalized);
                foreach (string sentence in sentences)
                {
                    if (TextUtil.ContainsCombiningDiacritics(sentence))
                    {
                        continue;
                    }

                    string value = operation == Operation.TOKENIZED
                        ? string.Join(" ", tokenizer.TokenizeToStrings(sentence))
                        : LemmatizeSentence(sentence);

                    value = RemoveNonWords(value);
                    if (lowercase)
                    {
                        value = value.ToLower(Turkish.Locale);
                    }

                    value = value.Trim();
                    if (value.Length > 0)
                    {
                        processed.Add(value);
                    }
                }
            }

            File.WriteAllLines(outFile, processed, Encoding.UTF8);
            Console.WriteLine("Processed: " + inFile + " -> " + outFile + " (" + processed.Count + " lines)");
        }

        private string LemmatizeSentence(string sentence)
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
            List<Token> docTokens = tokenizer.Tokenize(sentence);
            List<string> reduced = new List<string>(docTokens.Count);
            foreach (Token token in docTokens)
            {
                Token.Type type = token.GetTokenType();
                if (
                    type == Token.Type.Mention ||
                    type == Token.Type.HashTag ||
                    type == Token.Type.URL ||
                    type == Token.Type.Punctuation ||
                    type == Token.Type.RomanNumeral ||
                    type == Token.Type.Time ||
                    type == Token.Type.UnknownWord ||
                    type == Token.Type.Unknown)
                {
                    continue;
                }
                reduced.Add(token.GetText());
            }
            return string.Join(" ", reduced);
        }

        private static string NormalizeLine(string line)
        {
            string value = TextUtil.NormalizeQuotesHyphens(line);
            value = value.Replace("\u00ad", string.Empty);
            value = MultipleWhitespace.Replace(value, " ");
            return value.Trim();
        }

        public static void Main(string[] args)
        {
            new PreprocessTurkishCorpus().Execute(args);
        }
    }
}