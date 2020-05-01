using System;
using System.IO;

namespace ZemberekDotNet.LM.Compression
{
    public class GramDataArray
    {
        private static readonly int MaxBuf = 0x3fffffff;
        internal readonly int pageShift; // for getting the page index value this amount of left shift is used. page index value resides on higher bits.
        internal readonly int indexMask; // used for obtaining the actual index of the key block.
        internal readonly int fpSize; // length of fingerprint in bytes
        internal readonly uint fpMask; // to access fingerprint data length in bytes.
        internal readonly int probSize; // size of probability data length in bytes
        internal readonly int backoffSize; // size of backoff length in bytes
        internal int count; // gram count
        internal int blockSize; // defines the size of the key data. Such as if 3 bytes FP, 2 bytes Prob , 2 Bytes Backoff blockSize = 7
        internal byte[][] data; // holds the actual data. [page count][page length * block size ] bytes

        public GramDataArray(BinaryReader dis)
        {
            count = dis.ReadInt32().EnsureEndianness();
            this.fpSize = dis.ReadInt32().EnsureEndianness();
            this.probSize = dis.ReadInt32().EnsureEndianness();
            this.backoffSize = dis.ReadInt32().EnsureEndianness();

            if (fpSize == 4)
            {
                fpMask = 0xffffffff;
            }
            else
            {
                fpMask = (uint)(1 << (fpSize * 8)) - 1;
            }

            blockSize = fpSize + probSize + backoffSize;
            int pageLength = GetPowerOf2(MaxBuf / blockSize, MaxBuf / blockSize);
            pageShift = 32 - (pageLength - 1).NumberOfLeadingZeros();
            indexMask = (1 << pageShift - 1) - 1;
            long l = 0;
            int pageCounter = 0;
            while (l < (long)count * blockSize)
            {
                pageCounter++;
                l += (pageLength * blockSize);
            }
            data = new byte[pageCounter][];
            int total = 0;
            for (int i = 0; i < pageCounter; i++)
            {
                if (i < pageCounter - 1)
                {
                    data[i] = new byte[pageLength * blockSize];
                    total += pageLength * blockSize;
                }
                else
                {
                    data[i] = new byte[(int)((long)count * blockSize - total)];
                }
                dis.Read(data[i]);
            }
        }

        public int GetPowerOf2(int k, int limit)
        {
            if (k <= 2)
            {
                return 1;
            }
            int i = 1;
            while (i < k)
            {
                i *= 2;
            }
            if (i >= limit)
            {
                return i / 2;
            }
            else
            {
                return i;
            }
        }

        public int GetFingerPrint(int index)
        {
            int pageIndex = (index & indexMask) * blockSize;
            byte[] d = data[index >> pageShift];
            switch (fpSize)
            {
                case 1:
                    return d[pageIndex] & 0xff;
                case 2:
                    return ((d[pageIndex] & 0xff) << 8) |
                        (d[pageIndex + 1] & 0xff);
                case 3:
                    return ((d[pageIndex] & 0xff) << 16) |
                        ((d[pageIndex + 1] & 0xff) << 8) |
                        (d[pageIndex + 2] & 0xff);
                case 4:
                    return ((d[pageIndex] & 0xff) << 24) |
                        ((d[pageIndex + 1] & 0xff) << 16) |
                        ((d[pageIndex + 2] & 0xff) << 8) |
                        (d[pageIndex + 3] & 0xff);
            }
            return -1;
        }

        public bool CheckFingerPrint(int fpToCheck_, int globalIndex)
        {
            int fpToCheck = (int)(fpToCheck_ & fpMask);
            int pageIndex = (globalIndex & indexMask) * blockSize;
            byte[] d = data[globalIndex >> pageShift];
            switch (fpSize)
            {
                case 1:
                    return fpToCheck == (d[pageIndex] & 0xff);
                case 2:
                    return (fpToCheck >> 8 == (d[pageIndex] & 0xff)) && ((fpToCheck & 0xff) == (
                        d[pageIndex + 1] & 0xff));
                case 3:
                    return (fpToCheck >> 16 == (d[pageIndex] & 0xff)) &&
                        ((fpToCheck >> 8 & 0xff) == (d[pageIndex + 1] & 0xff)) &&
                        ((fpToCheck & 0xff) == (d[pageIndex + 2] & 0xff));
                case 4:
                    return (fpToCheck >> 24 == (d[pageIndex] & 0xff)) &&
                        ((fpToCheck >> 16 & 0xff) == (d[pageIndex + 1] & 0xff)) &&
                        ((fpToCheck >> 8 & 0xff) == (d[pageIndex + 2] & 0xff)) &&
                        ((fpToCheck & 0xff) == (d[pageIndex + 3] & 0xff));
                default:
                    throw new InvalidOperationException("fpSize must be between 1 and 4");
            }
        }

        public int GetProbabilityRank(int index)
        {
            int pageId = index >> pageShift;
            int pageIndex = (index & indexMask) * blockSize + fpSize;
            byte[] d = data[pageId];
            switch (probSize)
            {
                case 1:
                    return d[pageIndex] & 0xff;
                case 2:
                    return ((d[pageIndex] & 0xff) << 8) |
                        (d[pageIndex + 1] & 0xff);
                case 3:
                    return ((d[pageIndex] & 0xff) << 16) |
                        ((d[pageIndex + 1] & 0xff) << 8) | (d[pageIndex + 2] & 0xff);
            }
            return -1;
        }

        /// <summary>
        /// loads fingerprint, probability and backoff values to a single integer. this is only applicaple
        /// when 16 bit fingerprint and 8 bits quantized prob-backoff values are used.
        /// </summary>
        /// <param name="index">index value</param>
        /// <returns>integer carrying all fingerprint, probability and backoff value. structure is:
        /// [fingerprint|probability rank|backoff rank]</returns>
        public int GetCompact(int index)
        {
            int pageIndex = (index & indexMask) * blockSize;
            byte[] d = data[index >> pageShift];
            return ((d[pageIndex] & 0xff) << 24) |
                ((d[pageIndex + 1] & 0xff) << 16) |
                ((d[pageIndex + 2] & 0xff) << 8) |
                (d[pageIndex + 3] & 0xff);
        }

        public int GetBackoffRank(int index)
        {
            int pageId = index >> pageShift;
            int pageIndex = (index & indexMask) * blockSize + fpSize + probSize;
            byte[] d = data[pageId];
            switch (backoffSize)
            {
                case 1:
                    return d[pageIndex] & 0xff;
                case 2:
                    return ((d[pageIndex] & 0xff) << 8) | (d[pageIndex + 1] & 0xff);
                case 3:
                    return ((d[pageIndex] & 0xff) << 16) | ((d[pageIndex + 1] & 0xff) << 8) | (d[pageIndex + 2]
                        & 0xff);
            }
            return -1;
        }

        public void Load(int index, byte[] buff)
        {
            Array.Copy(data[index >> pageShift], (index & indexMask) * blockSize, buff, 0, blockSize);
        }
    }
}
