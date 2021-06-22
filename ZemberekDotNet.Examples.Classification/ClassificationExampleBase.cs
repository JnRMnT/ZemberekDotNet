using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Normalization;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Examples.Classification
{
    public class ClassificationExampleBase
    {
        protected static TurkishMorphology morphology;
        protected static TurkishSentenceNormalizer normalizer;

        protected void GenerateSetWithLemmas(List<string> lines, string lemmasPath)
        {
            List<string> lemmas = lines.Select(e => ReplaceWordsWithLemma(e))
                .Select(e => RemoveNonWords(e))
               .Select(e => ReplaceWordsWithLemma(e))
               .Select(e => RemoveNonWords(e))
               .Select(s => s.ToLower(Turkish.Locale)).ToList();
            File.WriteAllLines(lemmasPath, lemmas);
        }

        protected void GenerateSetWithSplit(List<string> lines, string splitPath)
        {
            List<string> lemmas = lines.Select(e => SplitWords(e))
                .Select(e => RemoveNonWords(e))
                .Select(s => s.ToLower(Turkish.Locale)).ToList();
            File.WriteAllLines(splitPath, lemmas);
        }

        protected void GenerateSetTokenized(List<string> lines, string tokenizedPath)
        {
            List<string> tokenized = lines.Select(s => string.Join(" ", TurkishTokenizer.Default.TokenizeToStrings(s)))
                .Select(e => RemoveNonWords(e))
                .Select(s => s.ToLower(Turkish.Locale)).ToList();
            File.WriteAllLines(tokenizedPath, tokenized);
        }

        protected string SplitWords(string sentence)
        {

            List<string> tokens = sentence.Split(" ").ToList();
            // assume first is label. Remove label from sentence for morphological analysis.
            string label = tokens[0];
            tokens = tokens.Skip(1).Take(tokens.Count() - 1).ToList();
            sentence = string.Join(" ", tokens);

            if (sentence.Length == 0)
            {
                return sentence;
            }
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate(sentence);
            List<string> res = new List<string>();
            // add label first.
            res.Add(label);
            foreach (SentenceWordAnalysis e in analysis)
            {
                SingleAnalysis best = e.GetBestAnalysis();
                string input = e.GetWordAnalysis().GetInput();
                if (best.IsUnknown())
                {
                    res.Add(input);
                    continue;
                }
                List<string> lemmas = best.GetLemmas();
                string l = lemmas[0];
                if (l.Length < input.Length)
                {
                    res.Add(l);
                    string substring = input.Substring(l.Length);
                    res.Add("_" + substring);
                }
                else
                {
                    res.Add(l);
                }
            }
            return string.Join(" ", res);
        }

        string ProcessEnding(string input)
        {
            return input.Replace("[ae]", "A").
                Replace("[ıiuü]", "I")
                .Replace("[kğ]", "K")
                .Replace("[cç]", "C")
                .Replace("[dt]", "D");
        }

        protected string ReplaceWordsWithLemma(string sentence)
        {

            List<string> tokens = sentence.Split(" ").ToList();
            // assume first is label. Remove label from sentence for morphological analysis.
            string label = tokens[0];
            tokens = tokens.Skip(1).Take(tokens.Count - 1).ToList();
            sentence = string.Join(" ", tokens);

            if (sentence.Length == 0)
            {
                return sentence;
            }
            SentenceAnalysis analysis = morphology.AnalyzeAndDisambiguate(sentence);
            List<string> res = new List<string>();
            // add label first.
            res.Add(label);
            foreach (SentenceWordAnalysis e in analysis)
            {
                SingleAnalysis best = e.GetBestAnalysis();
                if (best.IsUnknown())
                {
                    res.Add(e.GetWordAnalysis().GetInput());
                    continue;
                }
                List<string> lemmas = best.GetLemmas();
                res.Add(lemmas[0]);
            }
            return string.Join(" ", res);
        }

        protected string RemoveNonWords(string sentence)
        {
            List<Token> docTokens = TurkishTokenizer.Default.Tokenize(sentence);
            List<string> reduced = new List<string>(docTokens.Count);
            foreach (Token token in docTokens)
            {
                string text = token.GetText();

                // skip label and ending words.
                if (text.StartsWith("_") || text.Contains("__"))
                {
                    reduced.Add(text);
                    continue;
                }

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
                reduced.Add(text);
            }
            return string.Join(" ", reduced);
        }
    }
}
