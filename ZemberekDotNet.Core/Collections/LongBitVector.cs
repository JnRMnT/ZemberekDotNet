using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// A bit vector backed by a long array
    /// </summary>
    public class LongBitVector
    {
        // maximum size for the backing long array.
        public static readonly long MaxArraySize = (int.MaxValue - 1) * 64L;
        // for fast modulo 64 calculation of longs, n mod 64 = n and 0011 1111
        private static readonly long mod64Mask = 0x3FL;
        private static readonly long[] longSetMasks = new long[64];
        private static readonly long[] longResetMasks = new long[64];
        private static readonly int[] intSetMasks = new int[32];
        private static readonly long[] cutMasks = new long[64];

        static LongBitVector()
        {
            for (int i = 0; i < 64; i++)
            {
                longSetMasks[i] = (long)0x1L << i;
                longResetMasks[i] = ~longSetMasks[i];
                if (i < 32)
                {
                    intSetMasks[i] = (int)0x1 << i;
                }
                cutMasks[i] = (long)~(0xfffffffffffffffeL << i);
            }
        }

        private readonly int capacityInterval;
        private long[] words;
        private long size;

        /// <summary>
        /// creates an empty bit vector.
        /// </summary>
        public LongBitVector() : this(128)
        {

        }

        /// <summary>
        /// Creates an empty bit vector with initial bit capacity of initialCapcity.
        /// </summary>
        /// <param name="initialCapacity">Initial Capacity</param>
        public LongBitVector(long initialCapacity) : this(initialCapacity, 7)
        {

        }

        /// <summary>
        /// Creates a bit vector with the bit values from words with size of size.
        /// </summary>
        /// <param name="words">long values carrying bits.</param>
        /// <param name="size">vector size.</param>
        public LongBitVector(long[] words, long size)
        {
            if (size < 0 || size > words.Length * 64)
            {
                throw new ArgumentException("Cannot create vector with size:" + size);
            }
            this.size = size;
            this.words = (long[])words.Clone();
            this.capacityInterval = 7;
        }

        /// <summary>
        /// creates an empty bit vector with determined initial capacity and capacity interval.
        /// </summary>
        /// <param name="initialCapacity">initial bit capacity.</param>
        /// <param name="capacityInterval">amount of long values to add when capacity is not enough.</param>
        public LongBitVector(long initialCapacity, int capacityInterval)
        {
            if (capacityInterval < 0)
            {
                throw new ArgumentException("Cannot create vector with capacityInterval:" + capacityInterval);
            }
            this.capacityInterval = capacityInterval;
            EnsureSize(initialCapacity);
            words = new long[(int)(initialCapacity >> 6) + capacityInterval];
            this.size = 0;
        }

        /// <summary>
        /// Used only for test purposes.
        /// </summary>
        /// <param name="bits">bit string. It can contain space characters</param>
        /// <returns>bit vector equivalent.</returns>
        public static LongBitVector FromBinaryString(String bits)
        {
            bits = bits.Replace(" ", "");
            LongBitVector vector = new LongBitVector(bits.Length);

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == '1')
                {
                    vector.AddFast(true);
                }
                else
                {
                    vector.AddFast(false);
                }
            }
            return vector;
        }

        /// <summary>
        /// Custom deserializer. It generates a LongBitVector from the stream. Method does not closes the
        /// input Stream.
        /// </summary>
        /// <param name="dis">input stream</param>
        /// <returns>a new LongbotVector loaded from the data input stream.</returns>
        public static LongBitVector Deserialize(BinaryReader dis)
        {
            int length = dis.ReadInt32();
            long size = dis.ReadInt64();
            long[]
            words = new long[length];
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = dis.ReadInt64();
            }
            return new LongBitVector(words, size);
        }

        /// <summary>
        /// retrieves the index of the last bit in the vector with the value of bitValue.
        /// </summary>
        /// <param name="bitValue">value of the bit to search.</param>
        /// <returns>the index of the last bit with the specified value. -1 if there is not such bit.</returns>
        public long GetLastBitIndex(bool bitValue)
        {
            for (long i = size - 1; i >= 0; i--)
            {
                if (Get(i) == bitValue)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Fills all bits with the value.
        /// </summary>
        /// <param name="bitValue">bit value to fill.</param>
        public void Fill(bool bitValue)
        {
            if (bitValue)
            {
                Array.Fill(words, -1);
                int last = (int)(size / 64);
                words[last] &= cutMasks[(int)(size & mod64Mask)] >> 1;
            }
            else
            {
                Array.Fill(words, (long)0);
            }
        }


        // TODO: can be optimized for almost filled with 1 vectors.
        /// <summary>
        /// retrieves the long array carrying the bit values.
        /// </summary>
        /// <returns>word array.</returns>
        public long[] GetLongArray()
        {
            return (long[])words.Clone();
        }

        /// <summary>
        /// creates a long array containing 0 bit indexes.
        /// </summary>
        /// <returns>zero bit indexes.</returns>
        public long[] ZeroIndexes()
        {
            long[] zeroIndexes = new long[(int)NumberOfZeros()];

            int j = 0;
            for (long i = 0; i < size; i++)
            {
                if (!Get(i))
                {
                    zeroIndexes[j++] = i;
                }
            }
            return zeroIndexes;
        }

        /// <summary>
        /// creates a long array containing 0 bit indexes.
        /// </summary>
        /// <returns>zero bit indexes.</returns>
        public int[] ZeroIntIndexes()
        {
            int[] zeroIndexes = new int[(int)NumberOfZeros()];

            int j = 0;
            for (int i = 0; i < size; i++)
            {
                if (!Get(i))
                {
                    zeroIndexes[j++] = i;
                }
            }
            return zeroIndexes;
        }

        private void EnsureSize(long _size)
        {
            if (_size < 0 || _size > MaxArraySize)
            {
                throw new ArgumentException("Cannot create vector with size:" + size);
            }
        }

        /// <summary>
        /// vector size.
        /// </summary>
        /// <returns>size</returns>
        public long Size()
        {
            return size;
        }

        /// <summary>
        /// appends a bit to the vector. it expads the vector capacity if there is no space left.
        /// </summary>
        /// <param name="b">bit value</param>
        public void Add(bool b)
        {
            if (words.Length << 6 == size + 1)
            {
                EnsureCapacity(capacityInterval);
            }
            if (b)
            {
                Set(size);
            }
            size++;
        }

        /// <summary>
        /// appends a bit to the vector. it does not expad the vector capacity if there is no space left.
        /// So user must be sure there is space left in the vector before calling this method.
        /// </summary>
        /// <param name="b">bit value.</param>
        public void AddFast(bool b)
        {
            if (b)
            {
                Set(size);
            }
            size++;
        }

        public long GetLong(long start, int bitAmount)
        {
            int startInd = (int)(start >> 6);
            int endInd = (int)((start + bitAmount) >> 6);
            long startMod = start & mod64Mask;
            if (startInd == endInd)
            {
                return (words[startInd] >> (int)startMod) & cutMasks[bitAmount - 1];
            }
            else
            {
                long first = (long)(words[startInd] >> (int)startMod);
                long second = (long)(words[endInd] << (int)(64 - startMod));
                return (long)(first | second) & cutMasks[bitAmount - 1];
            }
        }

        public void Add(int a, int bitLength)
        {
            if (bitLength < 0 || bitLength > 32)
            {
                throw new ArgumentException("Bit length cannot be negative or larger than 32:" + bitLength);
            }
            if (words.Length << 6 < size + 1)
            {
                EnsureCapacity(7);
            }
            for (int i = 0; i < bitLength; i++)
            {
                if ((a & intSetMasks[i]) != 0)
                {
                    Set(size);
                }
                size++;
            }
        }

        public void Add(int amount, bool bit)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Amount cannot be negative:" + amount);
            }
            if (words.Length << 6 < size + 1 + amount >> 6)
            {
                EnsureCapacity(capacityInterval + (int)(amount >> 6));
            }
            for (int i = 0; i < amount; i++)
            {
                if (bit)
                {
                    Set(size);
                }
                size++;
            }
        }

        public void Add(long a, int bitLength)
        {
            if (bitLength < 0 || bitLength > 64)
            {
                throw new ArgumentException("Bit length cannot be negative or lareger than 64:" + bitLength);
            }
            if (words.Length << 6 < size + 1)
            {
                EnsureCapacity(capacityInterval);
            }
            for (int i = 0; i < bitLength; i++)
            {
                if ((a & longSetMasks[i]) != 0)
                {
                    Set(size);
                }
                size++;
            }
        }

        public long NumberOfOnes()
        {
            long count = 0;
            foreach (long word in words)
            {
                count += word.BitCount();
            }
            return count;
        }

        public long NumberOfZeros()
        {
            return size - NumberOfOnes();
        }

        private void EnsureCapacity(int longsToExpand)
        {
            long[] newData = new long[words.Length + longsToExpand];
            Array.Copy(words, 0, newData, 0, words.Length);
            words = newData;
        }

        /// <summary>
        /// checks if there is enough free space for bitAmount of space in the vector. if not, it extends
        /// capacity.
        /// </summary>
        /// <param name="bitAmount">keyAmount of bits to check.</param>
        public void CheckAndEnsureCapacity(int bitAmount)
        {
            if (words.Length << 6 < size + bitAmount + 1)
            {
                EnsureCapacity(capacityInterval);
            }
        }

        /// <summary>
        /// returns the n.th bit value.This is an unsafe method. it does not check argument limits so it
        /// can throw an ArrayIndexOutOfBound exception if n is equal or larger than size value or smaller
        /// than zero.
        /// </summary>
        /// <param name="n">bit index.</param>
        /// <returns>bit value.</returns>
        public bool Get(long n)
        {
            return (words[(int)(n >> 6)] & longSetMasks[(int)(n & mod64Mask)]) != 0L;
        }

        /// <summary>
        /// eliminates the long values that do not carry actual bits.
        /// </summary>
        void Compress()
        {
            long[] newData = new long[(size >> 6) + 1];
            Array.Copy(words, 0, newData, 0, (size >> 6) + 1);
            words = newData;
        }

        /// <summary>
        /// sets the n.th bit. This is an unsafe method. it does not check argument limits so it can throw
        /// an ArrayIndexOutOfBound exception if n is equal or larger than size value or smaller than
        /// zero.
        /// </summary>
        /// <param name="n">bit index</param>
        public void Set(long n)
        {
            words[(int)(n >> 6)] |= longSetMasks[(int)(n & mod64Mask)];
        }

        /// <summary>
        /// sets the bit indexes from the an array .This is an unsafe method. it does not check argument
        /// limits so it can throw an ArrayIndexOutOfBound exception if one of the value is equal or larger
        /// than size value or smaller than zero.
        /// </summary>
        /// <param name="n">bit index array</param>
        public void Set(long[] n)
        {
            foreach (long l in n)
            {
                words[(int)(l >> 6)] |= longSetMasks[(int)(l & mod64Mask)];
            }
        }

        /// <summary>
        /// resets the n.th bit. This is an unsafe method. it does not check argument limits so it can
        /// throw an ArrayIndexOutOfBound exception if n is equal or larger than size value or smaller than
        /// zero.
        /// </summary>
        /// <param name="n">bit index</param>
        public void Clear(long n)
        {
            words[(int)(n >> 6)] &= longResetMasks[(int)(n & mod64Mask)];
        }

        /// <summary>
        /// resets the bit indexes from the an array .This is an unsafe method. it does not check argument
        /// limits so it can throw an ArrayIndexOutOfBound exception if one of the value is equal or larger
        /// than size value or smaller than zero.
        /// </summary>
        /// <param name="n">bit index array</param>
        public void Clear(long[] n)
        {
            foreach (long l in n)
            {
                words[(int)(l >> 6)] &= longResetMasks[(int)(l & mod64Mask)];
            }
        }


        //TODO:Check
        /**
         * Custom serializer. Method does not closes the output stream.
         *
         * @param dos data output stream to write.
         */
        //        public void serialize(DataOutputStream dos) throws IOException
        //        {
        //            dos.writeInt(words.length);
        //    dos.writeLong(size);
        //    for (long word : words) {
        //        dos.writeLong(word);
        //    }
        //}
    }
}
