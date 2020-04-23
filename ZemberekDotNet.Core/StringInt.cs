using System;

namespace ZemberekDotNet.Core
{
    public class StringInt : IComparable<int>
    {
        public readonly string String;
        public readonly int Value;

        public StringInt(String text, int value)
        {
            this.String = text;
            this.Value = value;
        }

        public static StringInt FromString(String input, char delimiter)
        {
            int index = input.IndexOf(delimiter);
            if (index < 0)
            {
                throw new ArgumentException(String.Format($"Cannot parse line {input} with delimiter {delimiter}. There is no delimiter."));
            }
            String first = input.Substring(0, index).Trim();
            String second = input.Substring(index).Trim();
            try
            {
                return new StringInt(first, int.Parse(second));
            }
            catch
            {
                throw new ArgumentException($"Cannot parse line {input} with delimiter {delimiter}. Integer parse error.");
            }
        }

        public override string ToString()
        {
            return String + ":" + Value;
        }

        public int CompareTo(int comparedObject)
        {
            return ((int)comparedObject).CompareTo(Value);
        }
    }
}
