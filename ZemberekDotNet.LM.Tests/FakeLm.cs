using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.LM.Tests
{
    public class FakeLm
    {
        public readonly int order;
        public string[] unigrams;
        public LmVocabulary vocabulary;
        int unigramLength = 32 * 32 * 32 * 32;

        public FakeLm(int order)
        {
            this.order = order;
            string alphabet = "abcçdefgğhıijklmnoöpqrsştuüvwxyz";
            unigrams = new string[unigramLength];
            for (int i = 0; i < unigrams.Length; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(alphabet[((i >> 15) & 0x1f)]);
                sb.Append(alphabet[((i >> 10) & 0x1f)]);
                sb.Append(alphabet[((i >> 5) & 0x1f)]);
                sb.Append(alphabet[(i & 0x1f)]);
                unigrams[i] = sb.ToString();
            }
            vocabulary = new LmVocabulary(unigrams);
        }

        public FakeGram[] GetNgramProbs(int o)
        {
            FakeGram[] probs = new FakeGram[unigramLength * o];
            int pp = 0;
            for (int i = 0; i < unigramLength; i++)
            {
                int[][] matrix = new int[o][];
                for (int j = 0; j < o; j++)
                {
                    matrix[j] = new int[o];
                }
                int kk = i % unigramLength;
                for (int j = 0; j < matrix.Length; j++)
                {
                    for (int k = 0; k < matrix.Length; k++)
                    {
                        if (k == 0)
                        {
                            matrix[j][k] = i;
                        }
                        else
                        {
                            matrix[j][k] = kk % unigramLength;
                        }
                        kk++;
                    }
                }
                for (int t = 0; t < o; t++)
                {
                    string[] blah = new string[o];
                    int tt = 0;
                    foreach (int val in matrix[t])
                    {
                        blah[tt++] = unigrams[val];
                    }

                    double p = (i % 1000) + (o * 1000) + 1;
                    if (o < order)
                    {
                        probs[pp] = new FakeGram(matrix[t], blah, p / 10000, -p / 10000);
                    }
                    else
                    {
                        probs[pp] = new FakeGram(matrix[t], blah, p / 10000, 0);
                    }
                    pp++;
                }

            }
            return probs;
        }

        public void Validate(FakeGram[] grams)
        {
            ISet<string> set = new HashSet<string>(grams.Length);
            foreach (FakeGram gram in grams)
            {
                string s = string.Join(" ", gram.vals);
                if (set.Contains(s))
                {
                    throw new InvalidOperationException("Duplicated item:" + s);
                }
                set.Add(s);
            }
        }

        public void GenerateArpa(string fileName)
        {
            Console.WriteLine("unigrams = " + unigrams.Length);
            SimpleTextWriter sw = SimpleTextWriter.KeepOpenUTF8Writer(fileName);
            /*
            \data\
            ngram 1= 4
            ngram 2= 3
            ngram 3= 2
            */
            sw.WriteLine("\\data\\");
            for (int o = 1; o <= order; o++)
            {
                sw.WriteLine("ngram " + o + "=" + o * unigramLength);
            }
            for (int o = 1; o <= order; o++)
            {
                FakeGram[] probs = GetNgramProbs(o);
                Console.WriteLine("Validating..");
                Validate(probs);
                Console.WriteLine("Writing " + o + " grams.");
                sw.WriteLine();
                sw.WriteLine("\\" + o + "-grams:\n");
                foreach (FakeGram prob in probs)
                {
                    if (o < order)
                    {
                        sw.WriteLine(string
                            .Format("{0:F4} {1} {2:F4}", prob.prob, string.Join(" ", prob.vals), prob.backoff));
                    }
                    else
                    {
                        sw.WriteLine(string.Format("{0:F4} {1}", prob.prob, string.Join(" ", prob.vals)));
                    }
                }
            }
            sw.WriteLine();
            sw.WriteLine("\\end\\");
        }

        public class FakeGram
        {

            public int[] indexes;
            public string[] vals;
            public double prob;
            public double backoff;

            public FakeGram(int[] indexes, String[] vals, double prob, double backoff)
            {
                this.indexes = indexes;
                this.vals = vals;
                this.prob = prob;
                this.backoff = backoff;
            }
        }
    }
}
