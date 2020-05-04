using System;

namespace ZemberekDotNet.NER
{
    public class NerToken
    {
        internal int index;
        internal string word;
        internal string normalized;
        internal string type;
        internal string tokenId;
        internal NePosition position;

        public NerToken(int index, string word, string normalized, string type, NePosition position)
        {
            this.index = index;
            this.word = word;
            this.normalized = normalized;
            this.type = type;
            this.position = position;
            this.tokenId = GetTokenId();
        }

        public NerToken(int index, string word, string type, NePosition position)
        {
            this.index = index;
            this.word = word;
            this.normalized = word;
            this.type = type;
            this.position = position;
            this.tokenId = GetTokenId();
        }

        public int GetIndex()
        {
            return index;
        }

        public string GetWord()
        {
            return word;
        }

        public string GetNormalized()
        {
            return normalized;
        }

        public string GetTokenType()
        {
            return type;
        }

        public NePosition GetPosition()
        {
            return position;
        }

        public static NerToken FromTypePositionString(int index, string word, string normalized, string id)
        {
            if (id.Equals("O"))
            {
                return new NerToken(index, word, normalized, NerDataSet.OutTokenType, NePosition.OUTSIDE);
            }
            if (!id.Contains("_"))
            {
                throw new InvalidOperationException("Id value should contain _ but : " + id);
            }
            int p = id.IndexOf('_');
            String type = id.Substring(0, p);
            NePosition pos = NePosition.FromString(id.Substring(p + 1));
            return new NerToken(index, word, normalized, type, pos);
        }

        private string GetTokenId()
        {
            if (position == NePosition.OUTSIDE)
            {
                return "O";
            }
            else
            {
                return type + "_" + position.shortForm;
            }
        }

        public override string ToString()
        {
            return "[" +
                +index +
                ", " + word +
                ", " + normalized +
                ", " + type +
                ", " + position +
                ']';
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

            NerToken token = (NerToken)o;

            if (index != token.index)
            {
                return false;
            }
            if (!word.Equals(token.word))
            {
                return false;
            }
            if (!normalized.Equals(token.normalized))
            {
                return false;
            }
            return tokenId.Equals(token.tokenId);
        }

        public override int GetHashCode()
        {
            int result = index;
            result = 31 * result + word.GetHashCode();
            result = 31 * result + normalized.GetHashCode();
            result = 31 * result + tokenId.GetHashCode();
            return result;
        }
    }
}
