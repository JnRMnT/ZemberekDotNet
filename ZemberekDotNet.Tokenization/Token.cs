using System;

namespace ZemberekDotNet.Tokenization
{
    public class Token
    {
        public readonly String content;
        public readonly String normalized;
        public readonly Type type;
        public readonly int start;
        public readonly int end;

        public Token(String content, String normalized, Type type, int start, int end)
        {
            this.content = content;
            this.normalized = normalized;
            this.type = type;
            this.start = start;
            this.end = end;
        }

        public Token(String content, Type type, int start, int end)
        {
            this.content = content;
            this.normalized = content;
            this.type = type;
            this.start = start;
            this.end = end;
        }

        public Type GetTokenType()
        {
            return type;
        }

        public int GetStart()
        {
            return start;
        }

        public int GetEnd()
        {
            return end;
        }

        internal bool IsNumeral()
        {
            return type == Type.Number ||
                type == Type.RomanNumeral ||
                //        type == Type.CardinalNumber ||
                //        type == Type.RealNumber ||
                //        type == Type.Range ||
                //        type == Type.Ratio ||
                //        type == Type.Distribution ||
                //        type == Type.OrdinalNumber ||
                type == Type.PercentNumeral;
        }

        internal bool IsWhiteSpace()
        {
            return type == Type.SpaceTab ||
                type == Type.NewLine;
        }

        internal bool IsWebRelated()
        {
            return
                type == Type.HashTag ||
                    type == Type.Mention ||
                    type == Type.URL ||
                    type == Type.MetaTag ||
                    type == Type.Email;
        }

        public string GetText()
        {
            return content;
        }

        internal bool IsEmoji()
        {
            return type == Type.Emoji || type == Type.Emoticon;
        }

        internal bool IsUnidentified()
        {
            return type == Type.Unknown || type == Type.UnknownWord;
        }

        internal bool IsWord()
        {
            return type == Type.Word || type == Type.Abbreviation;
        }


        public override string ToString()
        {
            return "[" + content + " " + type + " " + start + "-" + end + "]";
        }

        public enum Type
        {

            // white space
            SpaceTab,
            NewLine,

            // words
            Word,
            WordAlphanumerical,
            WordWithSymbol,
            Abbreviation,
            AbbreviationWithDots,

            Punctuation,

            // numerals. May contain suffixes.
            RomanNumeral,
            Number,
            // TODO: in later versions lexer should handle the types below.
            //Ratio,
            //Range,
            //RealNumber,
            //Distribution,
            //OrdinalNumber,
            //CardinalNumber,
            PercentNumeral,

            // temporal
            Time,
            Date,

            // web related
            URL,
            Email,
            HashTag,
            Mention,
            MetaTag,

            Emoji,
            Emoticon,

            UnknownWord,
            Unknown,
        }
    }
}
