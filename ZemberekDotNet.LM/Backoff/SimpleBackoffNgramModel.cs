using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Math;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Native.Helpers;
using ZemberekDotNet.Core.Native.Text;

namespace ZemberekDotNet.LM.Backoff
{
    /// <summary>
    /// Simple Back-off N-gram model that loads an ARPA file.This class is not designed for compactness
    /// or speed but it can be used for validation and debugging. <p> aaa,cd
    /// </summary>
    public class SimpleBackoffNgramModel : BaseLanguageModel, INgramLanguageModel
    {
        public readonly double unigramWeight;
        private Dictionary<NgramData, NgramProb> probabilities;
        private List<int> counts;

        private SimpleBackoffNgramModel(
            int order,
            LmVocabulary vocabulary,
            Dictionary<NgramData, NgramProb> probabilities,
            List<int> counts,
            double unigramWeight) : base(order, vocabulary)
        {
            this.probabilities = probabilities;
            this.counts = counts;
            this.unigramWeight = unigramWeight;
        }

        /// <summary>
        /// Constructs a back-off language model from an Arpa file. Uni-gram weight applies smoothing to
        /// uni-gram values. A value of 1 means uni-gram probabilities from model directly used.
        /// </summary>
        /// <param name="arpaModel">arpa model file</param>
        /// <param name="unigramWeigth">unigram weight. in the moethod code there is explanation about it.</param>
        /// <returns></returns>
        public static SimpleBackoffNgramModel FromArpa(string arpaModel, float unigramWeigth)

        {
            if (unigramWeigth < 0 || unigramWeigth > 1)
            {
                throw new ArgumentException(
                    "Unigram weight must be between 0 and 1 but it is:" + unigramWeigth);
            }
            ArpaLoader converter = new ArpaLoader(unigramWeigth);
            return FileHelper.ReadAllLines(arpaModel, Encoding.UTF8, converter);
        }

        /// <summary>
        /// Constructs a back-off language model from an Arpa file. No unigram weight is applied.
        /// </summary>
        /// <param name="arpaModel">arpa model file</param>
        /// <returns></returns>
        public static SimpleBackoffNgramModel FromArpa(string arpaModel)
        {
            return FromArpa(arpaModel, 1.0f);
        }

        public float GetUnigramProbability(int id)
        {
            NgramData data = new NgramData(id);
            NgramProb res = probabilities.GetValueOrDefault(data);
            if (res == null)
            {
                throw new ArgumentException(
                    "Word does not exist!" + vocabulary.GetWordsString(id) + " with index:" + id);
            }
            else
            {
                return res.prob;
            }
        }


        public bool NGramExists(params int[] wordIndexes)
        {
            if (wordIndexes.Length < 1 || wordIndexes.Length > order - 1)
            {
                throw new ArgumentException("Amount of tokens must be between 1 and " +
                    order + " But it is " + wordIndexes.Length);
            }
            NgramData data = new NgramData(wordIndexes);
            NgramProb res = probabilities.GetValueOrDefault(data);
            return res != null;
        }

        public double GetLogBase()
        {
            return Math.E;
        }

        private float GetProbabilityValue(params int[] ids)
        {
            NgramData data = new NgramData(ids);
            NgramProb res = probabilities.GetValueOrDefault(data);
            if (res != null)
            {
                return res.prob;
            }
            else
            {
                return LogMath.LogZeroFloat;
            }
        }

        private float getBackoffValue(params int[] ids)
        {
            NgramData data = new NgramData(ids);
            NgramProb res = probabilities.GetValueOrDefault(data);
            if (res != null)
            {
                return res.backoff;
            }
            else
            {
                return 0;
            }
        }

        public IEnumerator<NgramData> GetAllIndexes()
        {
            return probabilities.Keys.GetEnumerator();
        }


        public float GetProbability(params int[] ids)
        {
            foreach (int id in ids)
            {
                if (!vocabulary.Contains(id))
                {
                    throw new ArgumentException(
                        "Unigram does not exist!" + vocabulary.GetWordsString(id) + " with index:" + id);
                }
            }
            float result = 0;
            float probability = GetProbabilityValue(ids);
            if (probability == LogMath.LogZeroFloat)
            { // if probability does not exist.
                if (ids.Length == 1)
                {
                    return LogMath.LogZeroFloat;
                }
                float backoffValue = getBackoffValue(Head(ids));
                result = result + backoffValue + GetProbability(Tail(ids));
            }
            else
            {
                result = probability;
            }
            return result;
        }

        public float GetTriGramProbability(int id0, int id1, int id2)
        {
            return GetProbability(id0, id1, id2);
        }

        public float GetTriGramProbability(int id0, int id1, int id2, int fingerPrint)
        {
            return GetProbability(id0, id1, id2);
        }
        
        public int GetOrder()
        {
            return order;
        }

        public int GetGramCount(int order)
        {
            return counts[order - 1];
        }

        public LmVocabulary GetVocabulary()
        {
            return vocabulary;
        }

        public class NgramData
        {
            int[] indexes;

            internal NgramData(params int[] indexes)
            {
                this.indexes = indexes;
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || !GetType().Equals(o.GetType()))
                {
                    return false;
                }
                NgramData ngramData = (NgramData)o;
                return Enumerable.SequenceEqual(indexes, ngramData.indexes);
            }

            public override int GetHashCode()
            {
                return indexes != null ? indexes.GetHashCode() : 0;
            }

            public override string ToString()
            {
                return Arrays.ToString(indexes);
            }

            public int[] GetIndexes()
            {
                return indexes;
            }
        }

        private class NgramProb
        {
            internal float prob;
            internal float backoff;

            internal NgramProb(float prob, float backoff)
            {
                this.prob = prob;
                this.backoff = backoff;
            }


            public override string ToString()
            {
                return prob + " " + backoff;
            }

        }

        private class ArpaLoader : ILineProcessor<SimpleBackoffNgramModel>
        {
            public static readonly int DEFAULT_UNKNOWN_PROBABILITY = -20;
            private readonly Regex SplitPattern = new Regex("\\s+");
            LmVocabulary.LmVocabularyBuilder vocabularyBuilder = LmVocabulary.Builder();
            LmVocabulary vocabulary;
            Dictionary<NgramData, NgramProb> probabilities = new Dictionary<NgramData, NgramProb>();
            int lineCounter = 0;
            int _n;
            int order;
            float logUniformUnigramProbability;
            float logUnigramWeigth;
            float inverseLogUnigramWeigth;
            float unigramWeight;
            State state = State.Begin;
            List<int> ngramCounts = new List<int>();
            bool started = false;
            long start;

            internal ArpaLoader(float unigramWeight)
            {
                this.unigramWeight = unigramWeight;
                start = DateTime.Now.Ticks;
                logUnigramWeigth = (float)Math.Log(unigramWeight);
                inverseLogUnigramWeigth = (float)Math.Log(1 - unigramWeight);
            }

            public bool ProcessLine(string s)
            {
                string clean = s.Trim();
                switch (state)
                {
                    // read n value and ngram counts.
                    case State.Begin:
                        if (clean.Length == 0)
                        {
                            break;
                        }
                        if (clean.StartsWith("\\data\\"))
                        {
                            started = true;
                        }
                        else if (started && clean.StartsWith("ngram"))
                        {
                            started = true;
                            int count = 0, i = 0;
                            foreach (string str in clean.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()))
                            {
                                if (i++ == 0)
                                {
                                    continue;
                                }
                                count = int.Parse(str);
                            }
                            ngramCounts.Add(count);
                        }
                        else if (started)
                        {
                            order = ngramCounts.Count;
                            Log.Info("order = " + order);
                            state = State.UniGrams;
                            int i = 0;
                            foreach (int count in ngramCounts)
                            {
                                Log.Info((++i) + "gram count:" + count);
                            }
                            Log.Info("Processing unigrams.");
                            logUniformUnigramProbability = (float)-Math
                                .Log(ngramCounts[0]); // log(1/#count) = -log(#count)
                            _n++;
                        }
                        break;

                    // read ngrams. if unigram values, we store the strings and related indexes.
                    case State.UniGrams:
                        if (clean.Length == 0 || clean.StartsWith("\\"))
                        {
                            break;
                        }
                        IEnumerator<string> it = SplitPattern.Split(clean).ToList().GetEnumerator();
                        // parse probabilty
                        it.MoveNext();
                        float logProbability = (float)LogMath.Log10ToLog(double.Parse(it.Current));
                        if (unigramWeight < 1)
                        {
                            // apply uni-gram weight. This applies smoothing to unigrams. As lowering high probabilities and
                            // adding gain to small probabilities.
                            // uw = uni-gram weight  , uniformProb = 1/#unigram
                            // so in linear domain, we apply this to all probability values as: p(w1)*uw + uniformProb * (1-uw) to
                            // maintain the probability total is one while smoothing the values.
                            // this converts to log(p(w1)*uw + uniformProb*(1-uw) ) which is calculated with log probabilities
                            // a = log(p(w1)) + log(uw) and b = -log(#unigram)+log(1-uw) applying logsum(a,b)
                            // approach is taken from Sphinx-4
                            float p1 = logProbability + logUnigramWeigth;
                            float p2 = logUniformUnigramProbability + inverseLogUnigramWeigth;
                            logProbability = LogMath.LogSumFloat.Lookup(p1, p2);
                        }
                        it.MoveNext();
                        string word = it.Current;
                        float logBackoff = 0;
                        if (it.MoveNext())
                        {
                            logBackoff = (float)LogMath.Log10ToLog(double.Parse(it.Current));
                        }
                        int index = vocabularyBuilder.Add(word);
                        probabilities.Add(new NgramData(index), new NgramProb(logProbability, logBackoff));
                        lineCounter++;

                        if (lineCounter == ngramCounts[0])
                        {
                            HandleSpecialToken("<unk>");
                            HandleSpecialToken("</s>");
                            HandleSpecialToken("<s>");
                            vocabulary = vocabularyBuilder.Generate();
                            // update the ngram counts in case a special token is added.
                            ngramCounts[0] = vocabulary.Size();
                            lineCounter = 0;
                            state = State.NGrams;
                            _n++;
                            // if there is only unigrams in the arpa file, exit
                            if (ngramCounts.Count> 1)
                            {
                                Log.Info("Processing 2-grams.");
                            }
                        }
                        break;

                    case State.NGrams:
                        if (clean.Length == 0 || clean.StartsWith("\\"))
                        {
                            break;
                        }
                        IEnumerator<string> it2 = SplitPattern.Split(clean).ToList().GetEnumerator();
                        it2.MoveNext();
                        logProbability = (float)LogMath.Log10ToLog(double.Parse(it2.Current));

                        int[] ids = new int[_n];
                        for (int i = 0; i < _n; i++)
                        {
                            it2.MoveNext();
                            ids[i] = vocabulary.IndexOf(it2.Current);
                        }

                        logBackoff = 0;
                        if (_n < ngramCounts.Count)
                        {
                            if (it2.MoveNext())
                            {
                                logBackoff = (float)LogMath.Log10ToLog(double.Parse(it2.Current));
                            }
                        }

                        probabilities.Add(new NgramData(ids), new NgramProb(logProbability, logBackoff));

                        lineCounter++;
                        if (lineCounter == ngramCounts[_n - 1])
                        {
                            // if there is no more ngrams, exit
                            if (ngramCounts.Count == _n)
                            {
                                return false;
                            }
                            else
                            {
                                lineCounter = 0;
                                _n++;
                                Log.Info("Processing " + _n + "-grams.");
                            }
                        }
                        break;
                }
                return true;
            }

            // adds special token with default probability.
            private void HandleSpecialToken(String word)
            {
                if (vocabularyBuilder.IndexOf(word) == -1)
                {
                    Log.Warn("Special token " + word +
                        " does not exist in model. It is added with default unknown probability: " +
                        DEFAULT_UNKNOWN_PROBABILITY);
                    int index = vocabularyBuilder.Add(word);
                    probabilities.Add(new NgramData(index), new NgramProb(DEFAULT_UNKNOWN_PROBABILITY, 0));
                }
            }

            public SimpleBackoffNgramModel GetResult()
            {
                return new SimpleBackoffNgramModel(order, vocabulary, probabilities, ngramCounts,
                    unigramWeight);
            }

            private enum State
            {
                Begin, UniGrams, NGrams
            }
        }
    }
}
