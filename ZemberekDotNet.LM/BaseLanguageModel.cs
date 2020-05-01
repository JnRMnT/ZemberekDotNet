using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Hash;

namespace ZemberekDotNet.LM
{
    /// <summary>
    /// Base class for language models. Contains common Language model parameters, cache etc.
    /// </summary>
    public abstract class BaseLanguageModel
    {
        protected int order;
        protected LmVocabulary vocabulary;

        protected BaseLanguageModel(int order, LmVocabulary vocabulary)
        {
            this.order = order;
            this.vocabulary = vocabulary;
        }

        protected BaseLanguageModel()
        {
        }

        protected int[] Head(int[] arr)
        {
            if (arr.Length == 1)
            {
                return new int[0];
            }
            int[] head = new int[arr.Length - 1];
            Array.Copy(arr, 0, head, 0, arr.Length - 1);
            return head;
        }

        protected int[] Tail(int[] arr)
        {
            if (arr.Length == 1)
            {
                return new int[0];
            }
            int[] head = new int[arr.Length - 1];
            Array.Copy(arr, 1, head, 0, arr.Length - 1);
            return head;
        }

        public string GetBackoffExpression(params int[] wordIndexes)
        {
            StringBuilder sb = new StringBuilder("BO(");
            for (int j = 0; j < wordIndexes.Length; j++)
            {
                sb.Append(vocabulary.GetWord(wordIndexes[j]));
                if (j < wordIndexes.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        public string GetProbabilityExpression(params int[] wordIndexes)
        {
            int last = wordIndexes[wordIndexes.Length - 1];
            StringBuilder sb = new StringBuilder("p(" + vocabulary.GetWord(last));
            if (wordIndexes.Length > 1)
            {
                sb.Append("|");
            }
            for (int j = 0; j < wordIndexes.Length - 1; j++)
            {
                sb.Append(vocabulary.GetWord(wordIndexes[j]));
                if (j < wordIndexes.Length - 2)
                {
                    sb.Append(",");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// This is a simple cache that may be useful if ngram queries exhibit strong temporal locality.
        /// Cache stores key values so it does not produce false positives by itself.However underlying lm
        /// may do.
        /// </summary>
        public class LookupCache
        {

            public static readonly int DefaultLookupCacheSize = 1 << 17;
            internal readonly float[] probabilities;
            internal readonly int[][] keys;
            internal readonly int modulo;
            INgramLanguageModel model;
            int hit;
            int miss;

            /// <summary>
            /// Generates a cache with 2^14 slots.
            /// </summary>
            /// <param name="model"></param>
            public LookupCache(INgramLanguageModel model) : this(model, DefaultLookupCacheSize)
            {

            }

            /// <summary>
            /// Generates a cache where slotSize is the maximum power of two less than the size.
            /// </summary>
            /// <param name="model"></param>
            /// <param name="size"></param>
            public LookupCache(INgramLanguageModel model, int size)
            {
                this.model = model;
                int k = size < DefaultLookupCacheSize ? 2 : DefaultLookupCacheSize;
                while (k < size)
                {
                    k <<= 1;
                }
                modulo = k - 1;
                probabilities = new float[k];
                keys = new int[k][];
                for(int i = 0; i < keys.Length; i++)
                {
                    keys[i] = new int[model.GetOrder()];
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <returns> probability of the input data. If value is already cached, it returns immediately.
            /// Otherwise it calculates the probability using the model reference inside and returns the
            /// value.Overrides the previous keys and probability value.</returns>
            public float Get(int[] data)
            {
                int fastHash = MultiLevelMphf.Hash(data, -1);
                int slotHash = fastHash & modulo;
                if (Enumerable.SequenceEqual(data, keys[slotHash]))
                {
                    hit++;
                    return probabilities[slotHash];
                }
                else
                {
                    miss++;
                    float probability = data.Length == 3 ?
                        model.GetTriGramProbability(data[0], data[1], data[2], fastHash)
                        : model.GetProbability(data);
                    probabilities[slotHash] = probability;
                    Array.Copy(data, 0, keys[slotHash], 0, data.Length);
                    return probability;
                }
            }

            public int GetHit()
            {
                return hit;
            }

            public int GetMiss()
            {
                return miss;
            }
        }
    }
}
