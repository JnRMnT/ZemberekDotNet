using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Lexicon.TR;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Normalization
{
    public class NormalizationVocabularyGenerator
    {
        private TurkishMorphology morphology;
        private object lockObject = new object();
        bool normalize;

        public NormalizationVocabularyGenerator(TurkishMorphology morphology)
        {
            this.morphology = morphology;
            this.normalize = true;
        }

        public NormalizationVocabularyGenerator(TurkishMorphology morphology, bool normalize)
        {
            this.morphology = morphology;
            this.normalize = normalize;
        }

        public static void DummyMain(String[] args)
        {
            TurkishMorphology morphology = GetTurkishMorphology();

            NormalizationVocabularyGenerator generator = new NormalizationVocabularyGenerator(morphology);

            string corporaRoot = "/home/aaa/data/normalization/corpus";
            string outRoot = "/home/aaa/data/normalization/vocab-clean";
            string rootList = Path.Combine(corporaRoot, "clean-list");

            BlockTextLoader corpusProvider = BlockTextLoader
                .FromDirectoryRoot(corporaRoot, rootList, 30_000);

            Directory.CreateDirectory(outRoot);

            // create vocabularies
            int threadCount = Environment.ProcessorCount / 2;
            if (threadCount > 22)
            {
                threadCount = 22;
            }

            generator.CreateVocabulary(
                corpusProvider,
                threadCount,
                outRoot);
        }

        internal static TurkishMorphology GetTurkishMorphology()
        {
            return GetTurkishMorphology(false);
        }

        internal static TurkishMorphology GetTurkishMorphology(bool asciiTolerant)
        {
            AnalysisCache cache = AnalysisCache
            .Builder()
            .DynamicCacheSize(200_000, 400_000).Build();

            RootLexicon lexicon = TurkishDictionaryLoader.LoadFromResources(
            "Resources/tr/master-dictionary.dict",
            "Resources/tr/non-tdk.dict",
            "Resources/tr/proper.dict",
            "Resources/tr/proper-from-corpus.dict",
            "Resources/tr/abbreviations.dict",
            "Resources/tr/person-names.dict"
        );

            TurkishMorphology.TurkishMorphologyBuilder builder = TurkishMorphology
            .Builder()
            .SetLexicon(lexicon)
            .DisableUnidentifiedTokenAnalyzer()
            .SetCache(cache);
            if (asciiTolerant)
            {
                builder.DiacriticsInAnalysisIgnored();
            }
            return builder.Build();
        }

        internal class Vocabulary
        {
            Histogram<string> correct = new Histogram<string>(100_000);
            Histogram<string> incorrect = new Histogram<string>(100_000);
            Histogram<string> ignored = new Histogram<string>(100_000);

            public Vocabulary()
            {
            }

            public Vocabulary(Histogram<string> correct,
                Histogram<string> incorrect, Histogram<string> ignored)
            {
                this.Correct = correct;
                this.Incorrect = incorrect;
                this.Ignored = ignored;
            }

            public Histogram<string> Correct { get => correct; set => correct = value; }
            public Histogram<string> Incorrect { get => incorrect; set => incorrect = value; }
            public Histogram<string> Ignored { get => ignored; set => ignored = value; }

            public override string ToString()
            {
                return string.Format("Correct ={0} Incorrect={1} Ignored={2}",
                    Correct.Size(),
                    Incorrect.Size(),
                    Ignored.Size());
            }

            internal void CheckConsistency()
            {
                ISet<String> intersectionOfKeys = Incorrect.GetIntersectionOfKeys(Correct);
                int sharedKeyCount = intersectionOfKeys.Count;
                if (sharedKeyCount > 0)
                {
                    Log.Warn("Incorrect and correct sets share {0} keys", sharedKeyCount);
                }
                sharedKeyCount = Incorrect.GetIntersectionOfKeys(Ignored).Count;
                if (sharedKeyCount > 0)
                {
                    Log.Warn("Incorrect and ignored sets share {0} keys", sharedKeyCount);
                }
                sharedKeyCount = Correct.GetIntersectionOfKeys(Ignored).Count;
                if (sharedKeyCount > 0)
                {
                    Log.Warn("Correct and ignored sets share %d keys", sharedKeyCount);
                }
            }

        }

        internal void CreateVocabulary(
             BlockTextLoader corpora,
             int threadCount,
             string outRoot)
        {
            Log.Info("Thread count = {0}", threadCount);
            Vocabulary vocabulary = CollectVocabularyHistogram(corpora, threadCount);

            Log.Info("Checking consistency.");
            vocabulary.CheckConsistency();

            Log.Info("Saving vocabularies.");

            vocabulary.Correct.SaveSortedByCounts(Path.Combine(outRoot, "correct"), " ");
            vocabulary.Correct.SaveSortedByKeys(
            Path.Combine(outRoot, "correct.abc"),
            " ", StringComparer.Create(CultureInfo.InvariantCulture, false));

            vocabulary.Incorrect.SaveSortedByCounts(Path.Combine(outRoot, "incorrect"), " ");
            vocabulary.Incorrect.SaveSortedByKeys(
            Path.Combine(outRoot, "incorrect.abc"),
            " ", StringComparer.Create(CultureInfo.InvariantCulture, false));

            vocabulary.Ignored.SaveSortedByCounts(Path.Combine(outRoot, "ignored"), " ");
            vocabulary.Ignored.SaveSortedByKeys(
            Path.Combine(outRoot, "ignored.abc"),
            " ", StringComparer.Create(CultureInfo.InvariantCulture, false));
        }

        internal Vocabulary CollectVocabularyHistogram(BlockTextLoader corpora, int threadCount)
        {
            Vocabulary result = new Vocabulary();

            Parallel.ForEach(corpora, new ParallelOptions
            {
                MaxDegreeOfParallelism = threadCount
            }, (TextChunk chunk) =>
            {
                Log.Info("Processing {0}", chunk);
                new WordCollectorTask(chunk, result, this).Call();
            });
            return result;
        }

        internal class WordCollectorTask
        {
            TextChunk chunk;
            Vocabulary globalVocabulary;
            NormalizationVocabularyGenerator generator;

            internal WordCollectorTask(TextChunk chunk, Vocabulary globalVocabulary, NormalizationVocabularyGenerator generator)
            {
                this.chunk = chunk;
                this.globalVocabulary = globalVocabulary;
                this.generator = generator;
            }

            public Vocabulary Call()
            {
                Vocabulary local = new Vocabulary();
                List<string> sentences = TextCleaner.CleanAndExtractSentences(chunk.GetData());
                foreach (string sentence in sentences)
                {
                    List<Token> tokens = TurkishTokenizer.Default.Tokenize(sentence);
                    foreach (Token token in tokens)
                    {
                        string s = token.GetText();
                        if (local.Correct.Contains(s) || globalVocabulary.Correct.Contains(s))
                        {
                            local.Correct.Add(s);
                            continue;
                        }
                        if (local.Incorrect.Contains(s) || globalVocabulary.Incorrect.Contains(s))
                        {
                            local.Incorrect.Add(s);
                            continue;
                        }
                        // TODO: fix below.
                        if (token.GetTokenType() == Token.Type.URL ||
                            token.GetTokenType() == Token.Type.Punctuation ||
                            token.GetTokenType() == Token.Type.Email ||
                            token.GetTokenType() == Token.Type.HashTag ||
                            token.GetTokenType() == Token.Type.Mention ||
                            token.GetTokenType() == Token.Type.Emoticon ||
                            token.GetTokenType() == Token.Type.Unknown ||
                            local.Ignored.Contains(s) ||
                            globalVocabulary.Ignored.Contains(s) ||
                            TurkishAlphabet.Instance.ContainsDigit(s) /*||
              TurkishAlphabet.INSTANCE.containsApostrophe(s) ||
              Character.isUpperCase(s.charAt(0))*/)
                        {
                            local.Ignored.Add(s);
                            continue;
                        }
                        if (generator.normalize)
                        {
                            s = s.ToLower(Turkish.Locale);
                            s = s.Replace("'", "");
                        }
                        WordAnalysis results = generator.morphology.Analyze(s);
                        if (results.AnalysisCount() == 0)
                        {
                            local.Incorrect.Add(s);
                        }
                        else
                        {
                            local.Correct.Add(s);
                        }
                    }
                }
                Log.Info("{0} processed. {1}", chunk.ToString(), local.ToString());
                lock (generator.lockObject)
                {
                    globalVocabulary.Correct.Add(local.Correct);
                    globalVocabulary.Incorrect.Add(local.Incorrect);
                    globalVocabulary.Ignored.Add(local.Ignored);
                    Log.Info("Correct = {0}, Incorrect = {1}, Ignored = {2}",
                        globalVocabulary.Correct.Size(),
                        globalVocabulary.Incorrect.Size(),
                        globalVocabulary.Ignored.Size()
                    );
                }
                return local;
            }
        }
    }
}
