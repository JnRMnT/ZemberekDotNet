using System;

namespace ZemberekDotNet.LM.Compression
{
    public class DataInfo
    {
        public static readonly double LOG_2 = Math.Log(2);
        // total amount ot finger print bits. fingerprint data starts from index 0.
        public readonly int fpBits;
        // This represents the last byte right shift count to retrieve fp data from byte array.
        // For example if fpBits is 13 int (int representation: MSB|---abcdefghijklmn|LSB )
        // Then  it will be put to the byte array as : B0=|abcdefgh| B1=|ijklmn---| So the fpLastByteRightShiftCount = 3
        public readonly int fpLastByteRightShiftCount;
        //The index of last byte that contains fingerprint data.
        public readonly int fpEndByte;
        public readonly int probBits;
        //The index of first byte that contains probability data.
        public readonly int probStartByte;

        // This represents the amount of left shift required for getting the probability data from the last byte.
        // public readonly int probLastByteRightShiftCount;
        // this defines amount of higher bits to truncate from the first byte of probability.
        // Suppose first byte of the probability data is : |---abcde| then mask needs to truncate most significant 3 bits.
        public readonly int probFirstByteMask;
        //The index of last byte that contains probability data.
        public readonly int probEndByte;
        public readonly int backoffBits;
        public readonly int byteCount;

        public DataInfo(int minfingerPrintBits, int probBits, int backoffBits)
        {
            this.probBits = probBits;
            this.backoffBits = backoffBits;

            int total = minfingerPrintBits + probBits + backoffBits;
            if (total % 8 != 0)
            {
                total = ((total / 8) + 1) * 8;
                minfingerPrintBits = total - (probBits + backoffBits);
            }
            this.fpBits = minfingerPrintBits;
            byteCount = (fpBits + probBits + backoffBits) / 8;
            this.fpLastByteRightShiftCount = fpBits % 8;

            this.fpEndByte = fpBits / 8;

            if (fpLastByteRightShiftCount != 0)
            {
                probStartByte = fpEndByte;
                probFirstByteMask = 1 << (7 - fpLastByteRightShiftCount);
            }
            else
            {
                probStartByte = fpEndByte + 1;
                probFirstByteMask = 0xff;
            }

            int probLastByteEndBitIndex = (fpBits + probBits) % 8;

            probEndByte = (fpBits + probBits) / 8;
        }

        public static DataInfo FromCounts(int minfingerPrintBits, int probCount, int backoffCount)
        {
            int probBits = probCount == 0 ? 0 : MinBitCount(probCount);
            int backoffBits = backoffCount == 0 ? 0 : MinBitCount(backoffCount);
            return new DataInfo(minfingerPrintBits, probBits, backoffBits);
        }

        public static DataInfo FromCountsAndExpectedBits(
            int minfingerPrintBits,
            int probCount,
            int probBitsDesired,
            int backoffCount,
            int backoffBitsDesired)
        {
            DataInfo initial = FromCounts(minfingerPrintBits, probCount, backoffCount);
            int bb = backoffBitsDesired;
            if (initial.backoffBits < bb)
            {
                bb = initial.backoffBits;
            }
            int pb = probBitsDesired;
            if (initial.probBits < pb)
            {
                pb = initial.probBits;
            }
            return new DataInfo(minfingerPrintBits, pb, bb);
        }

        /// <summary>
        ///  Calculates 2 base logarithm
        /// </summary>
        /// <param name="input">value to calculate log</param>
        /// <returns>2 base logarithm of the input</returns>
        public static double Log2(double input)
        {
            return Math.Log(input) / LOG_2;
        }

        public static bool PowerOfTwo(int k)
        {
            if (k < 0)
            {
                throw new ArgumentException("Cannot calculate negative numbers:" + k);
            }
            return (k & (k - 1)) == 0;
        }

        public static int MinBitCount(int a)
        {
            int probBits = (int)Log2(a);
            if (!PowerOfTwo(a))
            {
                probBits++;
            }
            return probBits;
        }

        public byte[] Encode(int fp, int probIndex, int backoffIndex)
        {
            long k = backoffIndex;
            k = k << probBits;
            k |= probIndex;
            k = k << fpBits;
            k |= fp;
            byte[] result = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                result[i] = (byte)(k & 0xff);
                k = k >> 8;
            }
            return result;
        }

        public byte[] Encode2(int fp, int prob, int backoff)
        {
            byte[] arr = new byte[byteCount];
            return arr;
        }

        public override string ToString()
        {
            return "DataInfo{" +
                "fpBits=" + fpBits +
                ", probBits=" + probBits +
                ", backoffBits=" + backoffBits +
                ", byteCount=" + byteCount +
                '}';
        }
    }
}
