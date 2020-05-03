using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Data;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Morphology.Analysis;
using static ZemberekDotNet.Morphology.Ambiguity.PerceptronAmbiguityResolver;

namespace ZemberekDotNet.Morphology.Ambiguity
{
    /// <summary>
    /// This is the trainer class for the Turkish morphological ambiguity resolution mechanism. This
    /// class generates the model for actual ambiguity resolution class {@link
    /// PerceptronAmbiguityResolver}
    /// <p>
    /// Trainer uses text files for training. It parses them and then converts them to Morphological
    /// analysis result using the Morphological analyzer.
    /// </summary>
    public class PerceptronAmbiguityResolverTrainer
    {
        private Weights weights = new Weights();
        private Weights averagedWeights = new Weights();
        private IntValueMap<string> counts = new IntValueMap<string>();
        private TurkishMorphology analyzer;

        // during model updates, keys with lower than this value will be removed from the model.
        private double minPruneWeight = 0;

        public PerceptronAmbiguityResolverTrainer(TurkishMorphology analyzer)
        {
            this.analyzer = analyzer;
        }

        public PerceptronAmbiguityResolverTrainer(TurkishMorphology analyzer, double weightThreshold)
        {
            this.analyzer = analyzer;
            this.minPruneWeight = weightThreshold;
        }

        public PerceptronAmbiguityResolver Train(
            DataSet trainingSet,
            DataSet devSet,
            int iterationCount)
        {
            FeatureExtractor extractor = new FeatureExtractor(false);
            PerceptronAmbiguityResolver.Decoder decoder = new PerceptronAmbiguityResolver.Decoder(weights, extractor);

            int numExamples = 0;
            for (int i = 0; i < iterationCount; i++)
            {
                Log.Info("Iteration:" + i);
                trainingSet.Shuffle();
                foreach (SentenceAnalysis sentence in trainingSet.Sentences)
                {
                    if (sentence.Size() == 0)
                    {
                        continue;
                    }
                    numExamples++;
                    DecodeResult result = decoder.BestPath(sentence.AmbiguousAnalysis());
                    if (sentence.BestAnalysis().Equals(result.BestParse))
                    {
                        continue;
                    }
                    if (sentence.BestAnalysis().Count != result.BestParse.Count)
                    {
                        throw new InvalidOperationException(
                            "Best parse result must have same amount of tokens with Correct parse." +
                                " \nCorrect = " + sentence.BestAnalysis() + " \nBest = " + result.BestParse);
                    }

                    IntValueMap<String> correctFeatures =
                        extractor.ExtractFeatureCounts(sentence.BestAnalysis());
                    IntValueMap<String> bestFeatures =
                        extractor.ExtractFeatureCounts(result.BestParse);
                    UpdateModel(correctFeatures, bestFeatures, numExamples);
                }

                foreach (string feat in averagedWeights)
                {
                    UpdateAveragedWeights(feat, numExamples);
                    counts.Put(feat, numExamples);
                }

                Log.Info("Testing development set.");
                PerceptronAmbiguityResolver disambiguator =
                    new PerceptronAmbiguityResolver(averagedWeights, extractor);
                Test(devSet, disambiguator);

            }
            return new PerceptronAmbiguityResolver(averagedWeights, new FeatureExtractor(false));
        }

        public PerceptronAmbiguityResolver Train(string trainFile, string devFile, int iterationCount)
        {
            DataSet trainingSet = DataSet.Load(trainFile, analyzer);
            DataSet devSet = DataSet.Load(devFile, analyzer);
            return Train(trainingSet, devSet, iterationCount);
        }

        private void UpdateModel(
            IntValueMap<string> correctFeatures,
            IntValueMap<string> bestFeatures,
            int numExamples)
        {
            HashSet<string> keySet = new HashSet<string>();
            keySet.AddRange(correctFeatures.GetKeyList());
            keySet.AddRange(bestFeatures.GetKeyList());

            foreach (string feat in keySet)
            {
                UpdateAveragedWeights(feat, numExamples);

                weights.Increment(
                    feat,
                    (correctFeatures.Get(feat) - bestFeatures.Get(feat)));

                counts.Put(feat, numExamples);

                // reduce model by eliminating near zero weights.
                float wa = averagedWeights.Get(feat);
                if (Math.Abs(wa) <= minPruneWeight)
                {
                    averagedWeights.GetData().Remove(feat);
                }
                float w = weights.Get(feat);
                if (Math.Abs(w) <= minPruneWeight)
                {
                    weights.GetData().Remove(feat);
                }
            }
        }

        private void UpdateAveragedWeights(String feat, int numExamples)
        {
            int featureCount = counts.Get(feat);
            float updatedWeight = (averagedWeights.Get(feat) * featureCount
                + (numExamples - featureCount) * weights.Get(feat))
                / numExamples;

            averagedWeights.Put(
                feat,
                updatedWeight);
        }


        public class DataSet
        {
            List<SentenceAnalysis> sentences;
            Random rnd = new Random(0xbeef);

            public List<SentenceAnalysis> Sentences { get => sentences; set => sentences = value; }

            public void Shuffle()
            {
                Sentences.Shuffle(rnd);
            }

            public DataSet()
            {
                Sentences = new List<SentenceAnalysis>();
            }

            internal DataSet(List<SentenceAnalysis> sentences)
            {
                this.Sentences = sentences;
            }

            internal void Add(DataSet other)
            {
                this.Sentences.AddRange(other.Sentences);
            }

            internal static DataSet Load(string path, TurkishMorphology analyzer)
            {
                List<SentenceDataStr> sentencesFromTextFile = DataSet.LoadTrainingDataText(path);
                return new DataSet(DataSet.Convert(sentencesFromTextFile, analyzer));
            }

            static List<SentenceAnalysis> Convert(
                List<SentenceDataStr> set,
                TurkishMorphology analyzer)
            {

                List<SentenceAnalysis> sentences = new List<SentenceAnalysis>();
                // Find actual analysis equivalents.
                foreach (SentenceDataStr sentenceFromTrain in set)
                {
                    string sentence = sentenceFromTrain.Sentence;

                    List<WordAnalysis> sentenceAnalysis = analyzer.AnalyzeSentence(sentence);

                    if (sentenceAnalysis.Count != sentenceFromTrain.WordList.Count)
                    {
                        Log.Warn("Actual analysis token size [{0}] and sentence from file token size [{1}] "
                                + "does not match for sentence [{2}]",
                            sentenceAnalysis.Count, sentenceFromTrain.WordList.Count, sentence);
                    }

                    List<SentenceWordAnalysis> unambigiousAnalyses = new List<SentenceWordAnalysis>();

                    for (int i = 0; i < sentenceAnalysis.Count; i++)
                    {

                        WordAnalysis w = sentenceAnalysis[i];

                        Dictionary<string, SingleAnalysis> analysisMap = new Dictionary<string, SingleAnalysis>();
                        foreach (SingleAnalysis single in w)
                        {
                            analysisMap.Add(single.FormatLong(), single);
                        }

                        WordDataStr s = sentenceFromTrain.WordList[i];

                        if (!w.GetInput().Equals(s.Word))
                        {
                            Log.Warn(
                                "Actual analysis token [{0}] at index [{1}] is different than word from training [{2}] "
                                    + " for sentence [{3}]", w.GetInput(), i, s.Word, sentence);
                        }

                        if (w.AnalysisCount() != s.WordAnalysis.Count)
                        {
                            Log.Warn(
                                "Actual analysis token [{0}] has [{1}] analyses but word from training file has [{2}] "
                                    + " analyses for sentence [{3}]",
                                w.GetInput(), w.AnalysisCount(), s.WordAnalysis.Count, sentence);
                            break;
                        }

                        foreach (string analysis in s.WordAnalysis)
                        {
                            if (!analysisMap.ContainsKey(analysis))
                            {
                                Log.Warn(
                                    "Anaysis [{0}] from training set cannot be generated by Analyzer for sentence "
                                        + " [{1}]. Skipping sentence.", analysis, sentence);
                                goto BAD_SENTENCE;
                            }
                        }

                        if (analysisMap.ContainsKey(s.CorrectAnalysis))
                        {
                            SingleAnalysis correct = analysisMap.GetValueOrDefault(s.CorrectAnalysis);
                            unambigiousAnalyses.Add(new SentenceWordAnalysis(correct, w));
                        }
                        else
                        {
                            break;
                        }
                    }

                BAD_SENTENCE:
                    if (unambigiousAnalyses.Count == sentenceFromTrain.WordList.Count)
                    {
                        sentences.Add(new SentenceAnalysis(sentence, unambigiousAnalyses));
                    }
                }
                return sentences;
            }

            public void Info()
            {
                Log.Info("There are {0} sentences and {1} tokens.",
                    Sentences.Count,
                    Sentences.Sum(e => e.Size()));
            }

            public static List<SentenceDataStr> LoadTrainingDataText(string input)
            {
                List<string> allLines = TextIO.LoadLines(input);

                List<SentenceDataStr> set = new List<SentenceDataStr>();

                TextConsumer tc = new TextConsumer(allLines);
                while (!tc.Finished())
                {
                    List<string> sentenceData = new List<string>();
                    sentenceData.Add(tc.Current());
                    tc.Advance();
                    sentenceData.AddRange(tc.MoveUntil(s => s.StartsWith("S:")));

                    List<WordDataStr> wordDataStrList = new List<WordDataStr>();
                    TextConsumer tw = new TextConsumer(sentenceData);
                    string sentence = tw.GetAndAdvance().Substring(2);

                    bool ignoreSentence = false;
                    while (!tw.Finished())
                    {
                        string word = tw.GetAndAdvance();
                        List<string> analysesFromLines = tw.MoveUntil(s => !s.StartsWith("["));
                        analysesFromLines = analysesFromLines
                            .Select(s => s.EndsWith("-") ? s.Substring(0, s.Length - 1) : s).ToList();

                        string selected = null;
                        if (analysesFromLines.Count == 1)
                        {
                            selected = analysesFromLines[0];
                            analysesFromLines[0] = selected;
                        }
                        else
                        {
                            int i = 0;
                            int index = -1;
                            foreach (string s in analysesFromLines)
                            {
                                if (s.EndsWith("*"))
                                {
                                    selected = s.Substring(0, s.Length - 1);
                                    index = i;
                                    break;
                                }
                                i++;
                            }
                            if (index >= 0)
                            {
                                analysesFromLines[index] = selected;
                            }
                        }

                        WordDataStr w = new WordDataStr(word, selected, analysesFromLines);
                        if (w.CorrectAnalysis == null)
                        {
                            Log.Warn("Sentence [{0}] contains ambiguous analysis for word {1}. It will be ignored.",
                                sentence, word);
                            ignoreSentence = true;
                            break;
                        }
                        else
                        {
                            wordDataStrList.Add(w);
                        }
                    }

                    if (!ignoreSentence)
                    {
                        set.Add(new SentenceDataStr(sentence, wordDataStrList));
                    }
                }
                Log.Info("There are {0} sentences and {1} tokens in {2}.",
                    set.Count,
                    set.Sum(s => s.WordList.Count),
                    input);
                return set;
            }
        }

        public class SentenceDataStr
        {
            string sentence;
            List<WordDataStr> wordList;

            internal SentenceDataStr(string sentence,
                List<WordDataStr> wordList)
            {
                this.Sentence = sentence;
                this.WordList = wordList;
            }

            public string Sentence { get => sentence; set => sentence = value; }
            internal List<WordDataStr> WordList { get => wordList; set => wordList = value; }
        }

        internal class WordDataStr
        {
            string word;
            string correctAnalysis;
            List<string> wordAnalysis;

            internal WordDataStr(string word, string correctAnalysis,
                List<string> wordAnalysis)
            {
                this.Word = word;
                this.CorrectAnalysis = correctAnalysis;
                this.WordAnalysis = wordAnalysis;
            }

            public string Word { get => word; set => word = value; }
            public string CorrectAnalysis { get => correctAnalysis; set => correctAnalysis = value; }
            public List<string> WordAnalysis { get => wordAnalysis; set => wordAnalysis = value; }
        }

        /// <summary>
        /// For evaluating a test file.
        /// </summary>
        /// <param name="testFilePath"></param>
        /// <param name="morphology"></param>
        /// <param name="resolver"></param>
        public static void Test(
            string testFilePath,
            TurkishMorphology morphology,
            PerceptronAmbiguityResolver resolver)
        {
            DataSet testSet = DataSet.Load(testFilePath, morphology);
            Test(testSet, resolver);
        }

        public static void Test(DataSet set, PerceptronAmbiguityResolver resolver)
        {
            int hit = 0, total = 0;
            Stopwatch sw = Stopwatch.StartNew();
            foreach (SentenceAnalysis sentence in set.Sentences)
            {
                DecodeResult result = resolver.GetDecoder().BestPath(sentence.AmbiguousAnalysis());
                int i = 0;
                List<SingleAnalysis> bestExpected = sentence.BestAnalysis();
                foreach (SingleAnalysis bestActual in result.BestParse)
                {
                    if (bestExpected[i].Equals(bestActual))
                    {
                        hit++;
                    }
                    total++;
                    i++;
                }
            }
            Log.Info("Elapsed: " + sw.ElapsedMilliseconds);
            Log.Info(
                "Word count:" + total + " hit=" + hit + string.Format(" Accuracy:{0}", hit * 1.0 / total));
        }
    }
}
