using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Ambiguity;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Generator;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Morphology
{
    // TODO: mothods require some re-thinking.
    // analysis method should probably not apply unidentified token analysis.
    // this should be left to the user.
    public class TurkishMorphology
    {
        private RootLexicon lexicon;
        private RuleBasedAnalyzer analyzer;
        private WordGenerator wordGenerator;
        private UnidentifiedTokenAnalyzer unidentifiedTokenAnalyzer;
        private TurkishTokenizer tokenizer;
        private AnalysisCache cache;
        private TurkishMorphotactics morphotactics;
        private IAmbiguityResolver ambiguityResolver;

        private bool useUnidentifiedTokenAnalyzer;
        private bool useCache;

        private TurkishMorphology(TurkishMorphologyBuilder builder)
        {
            this.lexicon = builder.Lexicon;
            if (lexicon.IsEmpty())
            {
                Log.Warn("TurkishMorphology class is being instantiated with empty root lexicon.");
            }

            this.morphotactics = builder.InformalAnalysis ?
                new InformalTurkishMorphotactics(this.lexicon) : new TurkishMorphotactics(this.lexicon);

            this.analyzer = builder.IgnoreDiacriticsInAnalysis ?
                RuleBasedAnalyzer.IgnoreDiacriticsInstance(morphotactics) :
                RuleBasedAnalyzer.Instance(morphotactics);

            this.wordGenerator = new WordGenerator(morphotactics);
            this.unidentifiedTokenAnalyzer = new UnidentifiedTokenAnalyzer(analyzer);
            this.tokenizer = builder.Tokenizer;

            if (builder.UseDynamicCache)
            {
                if (builder.Cache == null)
                {
                    cache = new AnalysisCache.AnalysisCacheBuilder().Build();
                }
                else
                {
                    cache = builder.Cache;
                }
                cache.InitializeStaticCache(this.AnalyzeWithoutCache);
            }
            this.useCache = builder.UseDynamicCache;
            this.useUnidentifiedTokenAnalyzer = builder.UseUnidentifiedTokenAnalyzer;

            if (builder.AmbiguityResolver == null)
            {
                string resourcePath = "Resources/tr/ambiguity/model-compressed";
                try
                {
                    this.ambiguityResolver =
                        PerceptronAmbiguityResolver.FromModelFile(resourcePath);
                }
                catch (IOException e)
                {
                    throw new ApplicationException(
                        "Cannot initialize PerceptronAmbiguityResolver from resource " + resourcePath, e);
                }
            }
            else
            {
                this.ambiguityResolver = builder.AmbiguityResolver;
            }
        }

        public RuleBasedAnalyzer GetAnalyzer()
        {
            return analyzer;
        }

        public UnidentifiedTokenAnalyzer GetUnidentifiedTokenAnalyzer()
        {
            return unidentifiedTokenAnalyzer;
        }

        public static TurkishMorphology CreateWithDefaults()
        {
            Stopwatch sw = Stopwatch.StartNew();
            TurkishMorphology instance = new TurkishMorphologyBuilder().SetLexicon(RootLexicon.GetDefault()).Build();
            Log.Info("Initialized in %d ms.", sw.ElapsedMilliseconds);
            return instance;
        }

        public static TurkishMorphology Create(RootLexicon lexicon)
        {
            return new TurkishMorphologyBuilder().SetLexicon(lexicon).Build();
        }

        public TurkishMorphotactics GetMorphotactics()
        {
            return morphotactics;
        }

        public WordAnalysis Analyze(String word)
        {
            return useCache ? AnalyzeWithCache(word) : AnalyzeWithoutCache(word);
        }

        public WordAnalysis Analyze(Token token)
        {
            return cache.GetAnalysis(token, this.AnalyzeWithoutCache);
        }

        private WordAnalysis AnalyzeWithCache(string word)
        {
            return cache.GetAnalysis(word, this.AnalyzeWithoutCache);
        }

        public void InvalidateCache()
        {
            if (useCache)
            {
                cache.InvalidateDynamicCache();
            }
        }

        public RootLexicon GetLexicon()
        {
            return lexicon;
        }

        /// <summary>
        /// Normalizes the input word and analyses it. If word cannot be parsed following occurs: - if
        /// input is a number, system tries to parse it by creating a number DictionaryEntry. - if input
        /// starts with a capital letter, or contains ['] adds a Dictionary entry as a proper noun. - if
        /// above options does not generate a result, it generates an UNKNOWN dictionary entry and returns
        /// a parse with it.
        /// </summary>
        /// <param name="word">input word.</param>
        /// <returns>WordAnalysis list.</returns>
        private WordAnalysis AnalyzeWithoutCache(string word)
        {
            List<Token> tokens = tokenizer.Tokenize(word);
            if (tokens.Count!= 1)
            {
                return new WordAnalysis(word, word, new List<SingleAnalysis>(0));
            }
            return AnalyzeWithoutCache(tokens[0]);
        }

        public static string NormalizeForAnalysis(string word)
        {
            // TODO: This may cause problems for some foreign words with letter I.
            string s = word.ToLower(Turkish.Locale);
            s = TurkishAlphabet.Instance.NormalizeCircumflex(s);
            string noDot = s.Replace(".", "");
            if (noDot.Length == 0)
            {
                noDot = s;
            }
            return TextUtil.NormalizeApostrophes(noDot);
        }

        private WordAnalysis AnalyzeWithoutCache(Token token)
        {
            string word = token.GetText();
            string s = NormalizeForAnalysis(word);

            if (s.Length == 0)
            {
                return WordAnalysis.EmptyInputResult;
            }

            List<SingleAnalysis> result;

            if (TurkishAlphabet.Instance.ContainsApostrophe(s))
            {
                s = TurkishAlphabet.Instance.NormalizeApostrophe(s);
                result = AnalyzeWordsWithApostrophe(s);
            }
            else
            {
                result = analyzer.Analyze(s);
            }

            if (result.Count == 0 && useUnidentifiedTokenAnalyzer)
            {
                result = unidentifiedTokenAnalyzer.Analyze(token);
            }

            if (result.Count == 1 && result[0].GetDictionaryItem().IsUnknown())
            {
                result = new List<SingleAnalysis>();
            }

            return new WordAnalysis(word, s, result);
        }

        public List<SingleAnalysis> AnalyzeWordsWithApostrophe(string word)
        {
            int index = word.IndexOf('\'');

            if (index <= 0 || index == word.Length - 1)
            {
                return new List<SingleAnalysis>();
            }

            StemAndEnding se = new StemAndEnding(
                word.Substring(0, index),
                word.Substring(index + 1));

            string stem = TurkishAlphabet.Instance.Normalize(se.stem);

            string withoutQuote = word.Replace("'", "");

            List<SingleAnalysis> noQuotesParses = analyzer.Analyze(withoutQuote);
            if (noQuotesParses.Count == 0)
            {
                return new List<SingleAnalysis>();
            }

            // TODO: this is somewhat a hack.Correct here once we decide what to do about
            // words like "Hastanesi'ne". Should we accept Hastanesi or Hastane?
            return noQuotesParses.Where(a => a.GetDictionaryItem().primaryPos == PrimaryPos.Noun &&
                        (a.ContainsMorpheme(TurkishMorphotactics.p3sg) || a.GetStem().Equals(stem))).ToList();
        }

        public List<WordAnalysis> AnalyzeSentence(String sentence)
        {
            string normalized = TextUtil.NormalizeQuotesHyphens(sentence);
            List<WordAnalysis> result = new List<WordAnalysis>();
            foreach (Token token in tokenizer.Tokenize(normalized))
            {
                result.Add(Analyze(token));
            }
            return result;
        }

        public SentenceAnalysis Disambiguate(string sentence, List<WordAnalysis> sentenceAnalysis)
        {
            return ambiguityResolver.Disambiguate(sentence, sentenceAnalysis);
        }

        /// <summary>
        /// Applies morphological analysis and disambiguation to a sentence.
        /// </summary>
        /// <param name="sentence">Sentence.</param>
        /// <returns>SentenceAnalysis instance.</returns>
        public SentenceAnalysis AnalyzeAndDisambiguate(String sentence)
        {
            return Disambiguate(sentence, AnalyzeSentence(sentence));
        }

        public AnalysisCache GetCache()
        {
            return cache;
        }

        public WordGenerator GetWordGenerator()
        {
            return wordGenerator;
        }

        public WordGenerator GetWordGenerator(TurkishMorphotactics morphotactics)
        {
            return new WordGenerator(morphotactics);
        }

        public static TurkishMorphologyBuilder Builder()
        {
            return new TurkishMorphologyBuilder();
        }

        public static TurkishMorphologyBuilder Builder(RootLexicon lexicon)
        {
            return new TurkishMorphologyBuilder().SetLexicon(lexicon);
        }

        public class TurkishMorphologyBuilder
        {
            RootLexicon lexicon = new RootLexicon();
            bool useDynamicCache = true;
            bool useUnidentifiedTokenAnalyzer = true;
            AnalysisCache cache;
            IAmbiguityResolver ambiguityResolver;
            TurkishTokenizer tokenizer = TurkishTokenizer.Default;
            bool informalAnalysis = false;
            bool ignoreDiacriticsInAnalysis = false;

            public RootLexicon Lexicon { get => lexicon; set => lexicon = value; }
            public bool InformalAnalysis { get => informalAnalysis; set => informalAnalysis = value; }
            public bool IgnoreDiacriticsInAnalysis { get => ignoreDiacriticsInAnalysis; set => ignoreDiacriticsInAnalysis = value; }
            public bool UseDynamicCache { get => useDynamicCache; set => useDynamicCache = value; }
            public bool UseUnidentifiedTokenAnalyzer { get => useUnidentifiedTokenAnalyzer; set => useUnidentifiedTokenAnalyzer = value; }
            public TurkishTokenizer Tokenizer { get => tokenizer; set => tokenizer = value; }
            public AnalysisCache Cache { get => cache; set => cache = value; }
            public IAmbiguityResolver AmbiguityResolver { get => ambiguityResolver; set => ambiguityResolver = value; }

            public TurkishMorphologyBuilder SetLexicon(RootLexicon lexicon)
            {
                this.Lexicon = lexicon;
                return this;
            }

            public TurkishMorphologyBuilder SetLexicon(params string[] dictionaryLines)
            {
                this.Lexicon = RootLexicon.FromLines(dictionaryLines);
                return this;
            }

            public TurkishMorphologyBuilder UseInformalAnalysis()
            {
                this.InformalAnalysis = true;
                return this;
            }

            public TurkishMorphologyBuilder DiacriticsInAnalysisIgnored()
            {
                this.IgnoreDiacriticsInAnalysis = true;
                return this;
            }

            public TurkishMorphologyBuilder SetCache(AnalysisCache cache)
            {
                this.Cache = cache;
                return this;
            }

            public TurkishMorphologyBuilder SetAmbiguityResolver(IAmbiguityResolver ambiguityResolver)
            {
                this.AmbiguityResolver = ambiguityResolver;
                return this;
            }

            public TurkishMorphologyBuilder SetTokenizer(TurkishTokenizer tokenizer)
            {
                this.Tokenizer = tokenizer;
                return this;
            }

            public TurkishMorphologyBuilder DisableCache()
            {
                UseDynamicCache = false;
                return this;
            }

            public TurkishMorphologyBuilder DisableUnidentifiedTokenAnalyzer()
            {
                UseUnidentifiedTokenAnalyzer = false;
                return this;
            }

            public TurkishMorphology Build()
            {
                return new TurkishMorphology(this);
            }
        }
    }
}
