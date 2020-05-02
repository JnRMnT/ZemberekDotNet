using System;
using System.Collections.Generic;

namespace ZemberekDotNet.NER
{
    /// <summary>
    /// Represents a sequence of named entity tokens.
    /// </summary>
    public class NamedEntity
    {
        internal string type;
        internal List<NerToken> tokens;

        public NamedEntity(string type, List<NerToken> tokens)
        {
            this.type = type;
            this.tokens = tokens;
        }

        public List<string> GetWords()
        {
            List<string> s = new List<string>(tokens.Count);
            foreach (NerToken token in tokens)
            {
                s.Add(token.word);
            }
            return s;
        }

        public List<NerToken> GetTokens()
        {
            return tokens;
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

            NamedEntity that = (NamedEntity)o;

            if (!type.Equals(that.type))
            {
                return false;
            }
            return tokens.Equals(that.tokens);
        }


        public override int GetHashCode()
        {
            int result = type.GetHashCode();
            result = 31 * result + tokens.GetHashCode();
            return result;
        }

        public string Content()
        {
            List<string> content = new List<string>();
            foreach (NerToken token in tokens)
            {
                content.Add(token.word);
            }
            return string.Join(" ", content);
        }


        public override string ToString()
        {
            return "[" + type + " " + Content() + "]";
        }
    }
}
