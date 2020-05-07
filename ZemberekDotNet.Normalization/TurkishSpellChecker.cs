using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.LM;
using ZemberekDotNet.LM.Compression;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Tokenization;
using static ZemberekDotNet.Normalization.CharacterGraphDecoder;

namespace ZemberekDotNet.Normalization
{
    public class TurkishSpellChecker
    {
        private static readonly INgramLanguageModel DummyLM = new DummyLanguageModel();
        private static readonly TurkishTokenizer tokenizer = TurkishTokenizer.Default;
        TurkishMorphology morphology;
        WordAnalysisSurfaceFormatter formatter = new WordAnalysisSurfaceFormatter();
        CharacterGraphDecoder decoder;
        INgramLanguageModel unigramModel;

        // Null means exact matcher will be used.
        ICharMatcher charMatcher = null;

        // can be used for filtering analysis results.
        Predicate<SingleAnalysis> analysisPredicate;

        public TurkishMorphology Morphology { get => morphology; set => morphology = value; }
        public WordAnalysisSurfaceFormatter Formatter { get => formatter; set => formatter = value; }
        public CharacterGraphDecoder Decoder { get => decoder; set => decoder = value; }
        public INgramLanguageModel UnigramModel { get => unigramModel; set => unigramModel = value; }
        public Predicate<SingleAnalysis> AnalysisPredicate { get => analysisPredicate; set => analysisPredicate = value; }

        public INgramLanguageModel GetUnigramLanguageModel()
        {
            return UnigramModel;
        }

        public TurkishSpellChecker(TurkishMorphology morphology)
        {
            this.Morphology = morphology;
            StemEndingGraph graph = new StemEndingGraph(morphology);
            this.Decoder = new CharacterGraphDecoder(graph.StemGraph);
            using (FileStream fileStream = File.OpenRead("Resources/lm-unigram.slm"))
            {
                UnigramModel = SmoothLm.Builder(fileStream).Build();
            }
        }

        public TurkishSpellChecker(TurkishMorphology morphology, CharacterGraph graph)
        {
            this.Morphology = morphology;
            this.Decoder = new CharacterGraphDecoder(graph);
        }

        public TurkishSpellChecker(
            TurkishMorphology morphology,
            CharacterGraphDecoder decoder,
            ICharMatcher matcher)
        {
            this.Morphology = morphology;
            this.Decoder = decoder;
            this.charMatcher = matcher;
        }

        // TODO: this is a temporary hack.
        public void SetAnalysisPredicate(Predicate<SingleAnalysis> analysisPredicate)
        {
            this.AnalysisPredicate = analysisPredicate;
        }

        //TODO: this does not cover all token types.
        public static List<string> TokenizeForSpelling(string sentence)
        {
            List<Token> tokens = tokenizer.Tokenize(sentence);
            List<string> result = new List<string>(tokens.Count);
            foreach (Token token in tokens)
            {
                if (token.GetTokenType() == Token.Type.Unknown ||
                    token.GetTokenType() == Token.Type.UnknownWord ||
                    token.GetTokenType() == Token.Type.Punctuation)
                {
                    continue;
                }
                string w = token.GetText();
                if (token.GetTokenType() == Token.Type.Word)
                {
                    w = w.ToLower(Turkish.Locale);
                }
                else if (token.GetTokenType() == Token.Type.WordWithSymbol)
                {
                    w = Turkish.Capitalize(w);
                }
                result.Add(w);
            }
            return result;
        }

        public bool Check(string input)
        {
            WordAnalysis analyses = Morphology.Analyze(input);
            WordAnalysisSurfaceFormatter.CaseType caseType = Formatter.GuessCase(input);
            foreach (SingleAnalysis analysis in analyses)
            {
                if (analysis.IsUnknown())
                {
                    continue;
                }
                if (AnalysisPredicate != null && !AnalysisPredicate(analysis))
                {
                    continue;
                }
                string apostrophe = GetApostrophe(input);

                if (Formatter.CanBeFormatted(analysis, caseType))
                {
                    string formatted = Formatter.FormatToCase(analysis, caseType, apostrophe);
                    if (input.Equals(formatted))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetApostrophe(string input)
        {
            if (input.IndexOf('’') > 0)
            {
                return "’";
            }
            else if (input.IndexOf('\'') > 0)
            {
                return "'";
            }
            return null;
        }

        public List<string> SuggestForWord(string word, INgramLanguageModel lm)
        {
            List<string> unRanked = GetUnrankedSuggestions(word);
            return RankWithUnigramProbability(unRanked, lm);
        }

        private List<string> GetUnrankedSuggestions(string word)
        {
            string normalized = TurkishAlphabet.Instance.Normalize(Regex.Replace(word, "['’]", ""));
            List<string> strings = Decoder.GetSuggestions(normalized, charMatcher);

            WordAnalysisSurfaceFormatter.CaseType caseType = Formatter.GuessCase(word);
            if (caseType == WordAnalysisSurfaceFormatter.CaseType.MIXED_CASE ||
                caseType == WordAnalysisSurfaceFormatter.CaseType.LOWER_CASE)
            {
                caseType = WordAnalysisSurfaceFormatter.CaseType.DEFAULT_CASE;
            }
            ISet<string> results = new LinkedHashSet<string>(strings.Count);
            foreach (string text in strings)
            {
                WordAnalysis analyses = Morphology.Analyze(text);
                foreach (SingleAnalysis analysis in analyses)
                {
                    if (analysis.IsUnknown())
                    {
                        continue;
                    }
                    if (AnalysisPredicate != null && !AnalysisPredicate(analysis))
                    {
                        continue;
                    }
                    string formatted = Formatter.FormatToCase(analysis, caseType, GetApostrophe(word));
                    results.Add(formatted);
                }
            }
            return new List<string>(results);
        }

        public List<string> SuggestForWord(
            string word,
            string leftContext,
            string rightContext,
            INgramLanguageModel lm)
        {
            List<string> unRanked = GetUnrankedSuggestions(word);
            if (lm == null)
            {
                Log.Warn("No language model provided. Returning unraked results.");
                return unRanked;
            }
            if (lm.GetOrder() < 2)
            {
                Log.Warn("Language model order is 1. For context ranking it should be at least 2. " +
                    "Unigram ranking will be applied.");
                return SuggestForWord(word, lm);
            }
            LmVocabulary vocabulary = lm.GetVocabulary();
            List<ScoredItem<string>> results = new List<ScoredItem<string>>(unRanked.Count);
            foreach (string str in unRanked)
            {
                if (leftContext == null)
                {
                    leftContext = vocabulary.GetSentenceStart();
                }
                else
                {
                    leftContext = NormalizeForLm(leftContext);
                }
                if (rightContext == null)
                {
                    rightContext = vocabulary.GetSentenceEnd();
                }
                else
                {
                    rightContext = NormalizeForLm(rightContext);
                }
                string w = NormalizeForLm(str);
                int wordIndex = vocabulary.IndexOf(w);
                int leftIndex = vocabulary.IndexOf(leftContext);
                int rightIndex = vocabulary.IndexOf(rightContext);
                float score;
                if (lm.GetOrder() == 2)
                {
                    score = lm.GetProbability(leftIndex, wordIndex) + lm.GetProbability(wordIndex, rightIndex);
                }
                else
                {
                    score = lm.GetProbability(leftIndex, wordIndex, rightIndex);
                }
                results.Add(new ScoredItem<string>(str, score));
            }
            results.Sort(ScoredItem<string>.StringCompDescending);
            return results.Select(s => s.Item).ToList();
        }

        private string NormalizeForLm(string s)
        {
            if (s.IndexOf('\'') > 0)
            {
                return Turkish.Capitalize(s);
            }
            else
            {
                return s.ToLower(Turkish.Locale);
            }
        }

        public List<string> SuggestForWord(string word)
        {
            return SuggestForWord(word, UnigramModel);
        }

        public CharacterGraphDecoder GetDecoder()
        {
            return Decoder;
        }

        public List<string> RankWithUnigramProbability(List<string> strings, INgramLanguageModel lm)
        {
            if (lm == null)
            {
                Log.Warn("No language model provided. Returning unraked results.");
                return strings;
            }
            List<ScoredItem<string>> results = new  List<ScoredItem<string>>(strings.Count);
            foreach (string str in strings)
            {
                string w = NormalizeForLm(str);
                int wordIndex = lm.GetVocabulary().IndexOf(w);
                results.Add(new ScoredItem<string>(str, lm.GetUnigramProbability(wordIndex)));
            }
            results.Sort(ScoredItem<string>.StringCompDescending);
            return results.Select(s => s.Item).ToList();
        }
    }
}
