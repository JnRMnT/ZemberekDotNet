using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.Core.Text
{
    /// <summary>
    /// a sequence of tokens. usually used for representing a sentence. this class is immutable
    /// </summary>
    public class TokenSequence
    {
        public static readonly string SENTENCE_START = "<s>";
        public static readonly string SENTENCE_END = "</s>";
        readonly string[] words;

        public TokenSequence(List<string> words)
        {
            if (words == null)
            {
                throw new ArgumentException("cannot create a sequence with a null list.");
            }
            this.words = words.ToArray();
        }

        public TokenSequence(string[] words)
        {
            if (words == null)
            {
                throw new ArgumentException("cannot create a sequence with a null list.");
            }
            this.words = (string[])words.Clone();
        }

        public TokenSequence(string spaceSeparatedWords) : this(new List<string>(Separate(spaceSeparatedWords)))
        {

        }

        public static TokenSequenceBuilder Builder()
        {
            return new TokenSequenceBuilder();
        }

        private static IEnumerable<string> Separate(string spaceSeparatedWords)
        {
            return spaceSeparatedWords.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim());
        }

        public static TokenSequence FromStartEndTaggedSequence(string spaceSeparatedWords)
        {
            IEnumerator<string> it = Separate(spaceSeparatedWords).GetEnumerator();
            List<string> list = new List<string>();
            while (it.MoveNext())
            {
                string s = it.Current;
                if (s.Equals(SENTENCE_START, StringComparison.InvariantCultureIgnoreCase)
                    || s.Equals(SENTENCE_END, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                list.Add(s);
            }
            return new TokenSequence(list);
        }

        public string[] GetTokens()
        {
            return words;
        }

        public string AsString()
        {
            return string.Join(" ", words);
        }

        public int Size()
        {
            return words.Length;
        }

        public string Get(int i)
        {
            return words[i];
        }

        public bool IsEmpty()
        {
            return words.Length == 0;
        }

        public string Last()
        {
            if (IsEmpty())
            {
                return "";
            }
            return words[words.Length - 1];
        }

        public string First()
        {
            if (IsEmpty())
            {
                return "";
            }
            return words[0];
        }

        public override string ToString()
        {
            return AsString();
        }

        public List<string> GetGrams(int gramSize)
        {
            int size = Size();
            if (size < 2)
            {
                return new List<string>();
            }
            if (size < gramSize)
            {
                return new List<string> { AsString() };
            }
            List<string> result = new List<string>(Size());
            for (int i = 0; i < words.Length - gramSize + 1; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < gramSize; j++)
                {
                    sb.Append(words[i + j]);
                    if (j != gramSize - 1)
                    {
                        sb.Append(" ");
                    }
                }
                result.Add(sb.ToString());
            }
            return result;
        }

        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !GetType().Equals(o.GetType()))
            {
                return false;
            }

            TokenSequence sequence = (TokenSequence)o;

            return Enumerable.SequenceEqual(words, sequence.words);

        }

        public override int GetHashCode()
        {
            return words.GetHashCode();
        }

        public class TokenSequenceBuilder
        {
            List<string> tokens = new List<string>();

            public TokenSequenceBuilder Add(string token)
            {
                tokens.Add(token);
                return this;
            }

            public TokenSequenceBuilder Add(params string[] tokenz)
            {
                tokens.AddRange(tokenz);
                return this;
            }

            public TokenSequence Build()
            {
                return new TokenSequence(tokens);
            }
        }
    }
}
