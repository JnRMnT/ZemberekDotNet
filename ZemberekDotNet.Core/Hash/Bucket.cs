using System;

namespace ZemberekDotNet.Core.Hash
{
    /// <summary>
    /// A bucket that holds keys. It contains a small array for keys.
    /// </summary>
    public class Bucket : IComparable<Bucket>
    {
        internal static readonly int[] Empty = new int[0];
        public readonly int id;
        public int[] keyIndexes = Empty;

        public Bucket(int id)
        {
            this.id = id;
        }

        public void Add(int i)
        {
            keyIndexes = keyIndexes.CopyOf(keyIndexes.Length + 1);
            keyIndexes[keyIndexes.Length - 1] = i;
        }

        public int CompareTo(Bucket o)
        {
            return o.keyIndexes.Length.CompareTo(keyIndexes.Length);
        }
    }
}
