using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.NER
{
    public class NerDataSet
    {
        public static readonly string OutTokenType = "OUT";
        private static readonly Regex enamexNeRegex = new Regex(
            "((<b_enamex TYPE=\")(?<TYPE>.+?)(\">)(?<CONTENT>.+?)(<e_enamex>))",
           RegexOptions.IgnoreCase);
        private static readonly Regex enamexNeSplitRegex = new Regex(
            "((<b_enamex TYPE=\")(?<TYPE>.+?)(\">)(?<CONTENT>.+?)(<e_enamex>))|([^ ]+)",
            RegexOptions.IgnoreCase);
        private static readonly Regex bracketNeRegex = new Regex(
            "((\\[)(?<TYPE>[^ ]+)( )(?<CONTENT>.+?)(]))", RegexOptions.IgnoreCase);
        private static readonly Regex bracketNeSplitRegex = new Regex(
            "((\\[)(?<TYPE>[^ ]+)( )(?<CONTENT>.+?)(]))|([^ ]+)", RegexOptions.IgnoreCase);
        private static readonly Regex openNlpNeRegex = new Regex(
            "((<START:)(?<TYPE>.+?)(>)(?<CONTENT>.+?)(<END>))", RegexOptions.IgnoreCase);
        private static readonly Regex openNlpNeSplitRegex = new Regex(
            "((<START:)(?<TYPE>.+?)(>)(?<CONTENT>.+?)(<END>))|([^ ]+)", RegexOptions.IgnoreCase);


        internal ISet<string> types = new HashSet<string>();
        internal ISet<string> typeIds = new HashSet<string>();
        internal List<NerSentence> sentences;

        public NerDataSet(List<NerSentence> sentences)
        {
            this.sentences = sentences;

            foreach (NerSentence sentence in sentences)
            {
                foreach (NerToken token in sentence.tokens)
                {
                    types.Add(token.type);
                    typeIds.Add(token.tokenId);
                }
            }
        }

        public List<NerSentence> GetSentences()
        {
            return sentences;
        }

        public enum AnnotationStyle
        {
            ENAMEX,
            BRACKET,
            OPEN_NLP
        }

        static readonly Random rnd = new Random(0xcafe);

        public void Shuffle()
        {
            sentences.Shuffle(rnd);
        }

        public static NerDataSet Load(string path, AnnotationStyle style)
        {
            switch (style)
            {
                case AnnotationStyle.BRACKET:
                    return LoadBracketStyle(path);
                case AnnotationStyle.ENAMEX:
                    return LoadEnamexStyle(path);
                case AnnotationStyle.OPEN_NLP:
                    return LoadOpenNlpStyle(path);
            }
            throw new IOException(string.Format("Cannot load data from {0} with style {1}", path, style));
        }

        static NerDataSet LoadBracketStyle(string path)
        {
            return LoadDataSet(path, bracketNeRegex, bracketNeSplitRegex);
        }


        static NerDataSet LoadEnamexStyle(string path)
        {
            return LoadDataSet(path, enamexNeRegex, enamexNeSplitRegex);
        }

        static NerDataSet LoadOpenNlpStyle(string path)
        {
            return LoadDataSet(path, openNlpNeRegex, openNlpNeSplitRegex);
        }

        public static string NormalizeForNer(string input)
        {
            input = input.ToLower(Turkish.Locale);
            List<string> result = new List<string>();
            foreach (Token t in TurkishTokenizer.Default.Tokenize(input))
            {
                string s = t.GetText();
                if (t.GetTokenType() == Token.Type.Date || t.GetTokenType() == Token.Type.Number
                    || t.GetTokenType() == Token.Type.Time)
                {
                    s = "*" + Regex.Replace(s, "[0-9]", "D") + "*";
                }
                result.Add(s);
            }
            return string.Join("", result);
        }

        static NerDataSet LoadDataSet(
            string path,
            Regex neRegex,
            Regex splitRegex)
        {
            Log.Info("Extracting data from {0}", path);

            List<String> lines = TextIO.LoadLines(path);

            List<NerSentence> nerSentences = new List<NerSentence>(lines.Count);
            foreach (string rawLine in lines)
            {
                string line = TextUtil.NormalizeApostrophes(rawLine);
                line = TextUtil.NormalizeQuotesHyphens(line);
                line = TextUtil.NormalizeSpacesAndSoftHyphens(line);

                if (line.Trim().Length < 2)
                {
                    continue;
                }
                List<String> tokens = Regexps.AllMatches(splitRegex, line);
                List<NerToken> nerTokens = new List<NerToken>(tokens.Count);
                int index = 0;
                foreach (string token in tokens)
                {
                    //combine apostrophe suffix to previous word.
                    if (index > 0 && token.StartsWith("'") && !token.EndsWith("'"))
                    {
                        nerTokens[index - 1].word = nerTokens[index - 1].word + token;
                        nerTokens[index - 1].normalized = nerTokens[index - 1].normalized + token;
                        continue;
                    }

                    // not a ner word.
                    if (!neRegex.IsMatch(token))
                    {
                        nerTokens.Add(new NerToken(index, token, NormalizeForNer(token), OutTokenType,
                            NePosition.OUTSIDE));
                        index++;
                        continue;
                    }
                    Match matcher = neRegex.Match(token);
                    if (matcher.Success)
                    {
                        string type = matcher.Groups["TYPE"].Value;
                        string content = matcher.Groups["CONTENT"].Value;
                        List<string> neWords = content.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToList();
                        for (int i = 0; i < neWords.Count; i++)
                        {
                            String s = neWords[i];
                            NePosition position;
                            if (neWords.Count == 1)
                            {
                                position = NePosition.UNIT;
                            }
                            else if (i == 0)
                            {
                                position = NePosition.BEGIN;
                            }
                            else if (i == neWords.Count - 1)
                            {
                                position = NePosition.LAST;
                            }
                            else
                            {
                                position = NePosition.INSIDE;
                            }
                            nerTokens.Add(new NerToken(index, s, NormalizeForNer(s), type, position));
                            index++;
                        }
                    }
                }
                nerSentences.Add(new NerSentence(line, nerTokens));
            }
            return new NerDataSet(nerSentences);
        }

        public void AddSet(NerDataSet set)
        {
            this.sentences.AddRange(set.sentences);
            types.AddRange(set.types);
            typeIds.AddRange(set.typeIds);
        }

        public NerDataSet GetSubSet(int from, int to)
        {
            return new NerDataSet(sentences.GetRange(from, to - from).ToList());
        }

        /// <summary>
        /// prints information about the data set.
        /// </summary>
        /// <returns></returns>
        public string Info()
        {
            return new DataSetInfo(this).Log();
        }

        public class DataSetInfo
        {
            internal int numberOfSentences;
            internal ISet<string> types;
            internal Histogram<string> typeHistogram = new Histogram<string>();
            internal Histogram<string> tokenHistogram = new Histogram<string>();
            internal int numberOfTokens;

            public DataSetInfo(NerDataSet set)
            {
                this.types = set.types;
                this.numberOfSentences = set.sentences.Count;
                foreach (NerSentence sentence in set.sentences)
                {
                    numberOfTokens += sentence.tokens.Count;
                    foreach (NerToken token in sentence.tokens)
                    {
                        tokenHistogram.Add(token.type);
                        if (token.position == NePosition.OUTSIDE ||
                            token.position == NePosition.BEGIN ||
                            token.position == NePosition.UNIT)
                        {
                            typeHistogram.Add(token.type);
                        }
                    }
                }
            }

            public string Log()
            {
                List<String> res = new List<string>();
                res.Add(string.Format("Number of sentences      = {0}", numberOfSentences));
                res.Add(string.Format("Number of tokens         = {0}", numberOfTokens));
                foreach (string type in typeHistogram.GetSortedList())
                {
                    res.Add(string.Format("Type = {0} (Count = {1}, Token Count = {2} Av. Token = {3:F2} )",
                        type,
                        typeHistogram.GetCount(type),
                        tokenHistogram.GetCount(type),
                        tokenHistogram.GetCount(type) * 1f / typeHistogram.GetCount(type)));
                }
                return string.Join("\n", res);
            }

        }
    }
}
