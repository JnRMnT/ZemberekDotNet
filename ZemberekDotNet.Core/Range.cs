using System;
using System.Diagnostics.Contracts;

namespace ZemberekDotNet.Core
{
    public class Range : IComparable<Range>
    {
        public readonly int From;
        public readonly int To;

        public Range(int from, int to)
        {
            Contract.Requires(from < to, "Range start cannot be larger than end. But start=" + from + "end=" + to);
            this.To = to;
            this.From = from;
        }

        public int Mid()
        {
            return From + (To - From) / 2;
        }

        public int Length()
        {
            return To - From;
        }

        public Range Copy(int offset)
        {
            return new Range(offset + From, offset + To);
        }

        public override string ToString()
        {
            return From + "-" + To;
        }

        public int CompareTo(Range other)
        {
            return Length().CompareTo(other.Length());
        }
    }
}
