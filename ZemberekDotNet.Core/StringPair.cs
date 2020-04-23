using System;

namespace ZemberekDotNet.Core
{
    public class StringPair
    {

        public readonly string First;
        public readonly string Second;

        public StringPair(string first, string second)
        {
            this.First = first;
            this.Second = second;
        }

        public static StringPair fromStringLastDelimiter(String str, char delimiter)
        {
            int index = str.LastIndexOf(delimiter);
            return FromString(str, index);
        }

        public static StringPair FromString(string stringText, char delimiter)
        {
            int index = stringText.IndexOf(delimiter);
            return FromString(stringText, index);
        }

        private static StringPair FromString(string stringText, int delimiterPos)
        {
            if (delimiterPos == -1)
            {
                throw new ArgumentException("Cannot extract two string from : [" + stringText + "]");
            }
            String first = stringText.Substring(0, delimiterPos).Trim();
            String second = stringText.Substring(delimiterPos).Trim();
            return new StringPair(first, second);
        }

        public static StringPair FromString(string text)
        {
            return FromString(text, ' ');
        }
    }
}
