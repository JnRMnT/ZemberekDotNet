using System;

namespace ZemberekDotNet.Core
{
    public class StringFloat : IComparable<float>
    {
        public readonly string String;
        public readonly float Value;

        public StringFloat(String text, float value)
        {
            this.String = text;
            this.Value = value;
        }

        public override string ToString()
        {
            return ToString(4);
        }

        public String ToString(int fractionDigits)
        {
            return String + ":" + Value.ToString("F" + fractionDigits);
        }

        public int CompareTo(float objectToCompare)
        {
            return ((float)objectToCompare).CompareTo(Value);
        }
    }
}
