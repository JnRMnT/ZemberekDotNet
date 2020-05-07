using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core
{
    /// <summary>
    /// Represents an object attached with a float score.
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    public class ScoredItem<T> : IComparable<double>
    {
        public static readonly Comparison<ScoredItem<T>> StringCompDescending = (a, b) => { return b.Score.CompareTo(a.Score); };
        public static readonly Comparison<ScoredItem<T>> StringCompAscending = (a, b) => { return a.Score.CompareTo(b.Score); };

        public readonly T Item;
        public readonly float Score;

        public ScoredItem(T item, float score)
        {
            this.Item = item;
            this.Score = score;
        }

        public override string ToString()
        {
            return ToString(6);
        }

        public string ToString(int fractionDigits)
        {
            return Item.ToString() + " : " + Score.ToString(Score.ToString("F" + fractionDigits));
        }

        public int CompareTo(double objectToCompare)
        {
            return objectToCompare.CompareTo((double)Score);
        }
    }
}
