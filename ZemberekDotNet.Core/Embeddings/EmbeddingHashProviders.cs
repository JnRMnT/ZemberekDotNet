using System;

namespace ZemberekDotNet.Core.Embeddings
{
    /// <summary>
    /// this algorithm is also slightly different than the original. This basically computes the
    /// character ngrams from a word But it does not use the ngram, instead it calculates a hash of
    /// it.
    /// 
    /// minn defines the minimum n-gram length, maxn defines the maximum ngram length. For example, For
    /// word 'zemberek' minn = 3 and maxn = 6 these ngrams are calculated:
    /// <pre>_ze, _zem, _zemb, _zembe
    /// zem, zemb, zembe, zember emb, embe, ember, embere mbe, mber, mbere, mberek ber, bere, berek,
    /// berek_ ere, erek, erek_ rek, rek_, ek_
    /// </pre>
    /// <p>
    /// If wordId is not -1, wordId value is added to result[0]
    /// </summary>
    public class EmbeddingHashProviders
    {
        public class CharacterNgramHashProvider : ISubWordHashProvider
        {
            int minN;
            int maxN;

            public CharacterNgramHashProvider(int minn, int maxn)
            {
                this.minN = minn;
                this.maxN = maxn;
            }

            public int[] GetHashes(String word, int wordId)
            {

                int endGram = maxN < word.Length ? maxN : word.Length;
                int size = 0;
                for (int i = minN; i <= endGram; i++)
                {
                    size += (word.Length - i + 1);
                }

                int[] result;
                int counter;
                if (wordId == -1)
                {
                    result = new int[size];
                    counter = 0;
                }
                else
                {
                    result = new int[size + 1];
                    result[0] = wordId;
                    counter = 1;
                }

                if (word.Length < minN)
                {
                    return result;
                }

                for (int i = 0; i <= word.Length - minN; i++)
                {
                    int n = minN;
                    while (i + n <= word.Length && n <= endGram)
                    {
                        result[counter] = Dictionary.Hash(word, i, i + n);
                        n++;
                        counter++;
                    }
                }
                return result;
            }

            public int GetMinN()
            {
                return minN;
            }

            public int GetMaxN()
            {
                return maxN;
            }
        }

        public class EmptySubwordHashProvider : ISubWordHashProvider
        {
            public int[] GetHashes(String word, int wordId)
            {
                if (wordId == -1)
                {
                    return new int[0];
                }
                else
                {
                    int[] result = new int[1];
                    result[0] = wordId;
                    return result;
                }
            }

            public int GetMinN()
            {
                return 0;
            }

            public int GetMaxN()
            {
                return 0;
            }
        }

        public class SuffixPrefixHashProvider : ISubWordHashProvider
        {
            int minN;
            int maxN;

            public SuffixPrefixHashProvider(int minn, int maxn)
            {
                this.minN = minn;
                this.maxN = maxn;
            }

            public int[] GetHashes(String word, int wordId)
            {
                int endGram = maxN < word.Length ? maxN : word.Length;
                int size = (endGram - minN) * 2;

                int[] result;
                int counter;
                if (wordId == -1)
                {
                    result = new int[size];
                    counter = 0;
                }
                else
                {
                    result = new int[size + 1];
                    result[0] = wordId;
                    counter = 1;
                }

                if (word.Length < minN)
                {
                    return result;
                }

                // prefixes
                for (int i = minN; i < endGram; i++)
                {
                    result[counter] = Dictionary.Hash(word, 0, i);
                    counter++;
                }
                // suffixes
                for (int i = word.Length - endGram + 1; i <= word.Length - minN; i++)
                {
                    result[counter] = Dictionary.Hash(word, i, word.Length);
                    counter++;
                }

                return result;
            }

            public int GetMinN()
            {
                return minN;
            }

            public int GetMaxN()
            {
                return maxN;
            }
        }
    }
}