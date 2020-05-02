using System;

namespace ZemberekDotNet.Tokenization
{
    /// <summary>
    /// Represents a segment from a sequence of data.Span is determined with start and end index values.
    /// Although there is no strict interpretation of index values, Usually start is considered
    /// inclusive, end is considered exclusive index values.
    /// </summary>
    public class Span
    {
        public readonly int start;
        public readonly int end;

        /// <summary>
        /// Start and end index values. They cannot be negative. And end must be equal or larger than the
        /// start value.
        /// </summary>
        /// <param name="start">start index</param>
        /// <param name="end">end index</param>
        public Span(int start, int end)
        {
            if (start < 0 || end < 0)
            {
                throw new ArgumentException("Span start and end values cannot be negative. " +
                    "But start = " + start + " end = " + end);
            }
            if (end < start)
            {
                throw new ArgumentException("Span end value cannot be smaller than start value. " +
                    "But start = " + start + " end = " + end);
            }
            this.start = start;
            this.end = end;
        }

        public int Length()
        {
            return end - start;
        }

        public int MiddleValue()
        {
            return end + (end - start) / 2;
        }

        /// <summary>
        /// Generates another Span with values offset+start and offset+end
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Span Copy(int offset)
        {
            return new Span(offset + start, offset + end);
        }

        /// <summary>
        /// Returns a substring from the input value represented with this span. For example, for input
        /// "abcdefg" span(begin=1 , end=3) represents "bc"
        /// </summary>
        /// <param name="input">a String</param>
        /// <returns>substring.</returns>
        public string GetSubstring(string input)
        {
            return input.Substring(start, end - start);
        }

        public bool InSpan(int i)
        {
            return i >= start && i < end;
        }
    }
}
