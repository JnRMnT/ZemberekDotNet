using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// A fixed size bit vector. Size can be maximum 2^31-1
    /// </summary>
    public class FixedBitVector
    {
        private static readonly int[] setMasks = new int[32];
        private static readonly int[] resetMasks = new int[32];
        public readonly int length;
        private int[] words;

        static FixedBitVector()
        {
            for (int i = 0; i < 32; i++)
            {
                setMasks[i] = 1 << i;
                resetMasks[i] = ~setMasks[i];
            }
        }

        public FixedBitVector(int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("Length cannot be negative. But it is:" + length);
            }
            this.length = length;
            int wordCount = (int)((uint)(length + 31) >> 5);
            words = new int[wordCount];
        }

        /// <summary>
        /// Used only for test purposes.
        /// </summary>
        /// <param name="bits">bits bit string. It can contain space characters</param>
        /// <returns>bit vector equivalent.</returns>
        public static FixedBitVector FromBinaryString(String bits)
        {
            bits = Regex.Replace(bits, "\\s+", "");
            FixedBitVector vector = new FixedBitVector(bits.Length);

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == '1')
                {
                    vector.Set(i);
                }
                else
                {
                    vector.Clear(i);
                }
            }
            return vector;
        }

        public bool Get(int n)
        {
            return (words[(uint)n >> 5] & setMasks[n & 31]) != 0;
        }

        public bool SafeGet(int n)
        {
            Check(n);
            return (words[(uint)n >> 5] & setMasks[n & 31]) != 0;
        }

        private void Check(int n)
        {
            if (n < 0 || n >= length)
            {
                throw new ArgumentException("Value must be between 0 and " + length + ". But it is " + n);
            }
        }

        public void Set(int n)
        {
            words[(uint)n >> 5] |= setMasks[n & 31];
        }

        public void SafeSet(int n)
        {
            Check(n);
            words[(uint)n >> 5] |= setMasks[n & 31];
        }

        public void Clear(int n)
        {
            words[(uint)n >> 5] &= resetMasks[n & 31];
        }

        public void SafeClear(int n)
        {
            Check(n);
            words[(uint)n >> 5] &= resetMasks[n & 31];
        }

        public int NumberOfOnes()
        {
            int count = 0;
            foreach (int word in words)
            {
                count += word.BitCount();
            }
            return count;
        }

        public int NumberOfNewOneBitCount(FixedBitVector other)
        {
            int total = 0;
            for (int i = 0; i < this.length; i++)
            {
                if (!this.Get(i) && other.Get(i))
                {
                    total++;
                }
            }
            return total;
        }

        public int DifferentBitCount(FixedBitVector other)
        {
            int total = 0;
            for (int i = 0; i < this.length; i++)
            {
                if (this.Get(i) != other.Get(i))
                {
                    total++;
                }
            }
            return total;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>number of zeroes</returns>
        public int NumberOfZeroes()
        {
            return length - NumberOfOnes();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>an array containing 0 bit indexes.</returns>
        public int[] ZeroIndexes()
        {
            int[] zeroIndexes = new int[NumberOfZeroes()];
            int j = 0;
            for (int i = 0; i < length; i++)
            {
                if (!Get(i))
                {
                    zeroIndexes[j++] = i;
                }
            }
            return zeroIndexes;
        }
    }
}
