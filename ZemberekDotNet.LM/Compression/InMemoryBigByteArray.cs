using System;
using System.IO;

namespace ZemberekDotNet.LM.Compression
{
    public class InMemoryBigByteArray
    {
        private static readonly int MAX_BUF = 0x3fffffff;
        internal readonly int pageShift;
        internal readonly int indexMask;
        internal int count;
        internal int blockSize;
        internal byte[][] data;

        public InMemoryBigByteArray(string file)
        {
            using (FileStream raf = File.OpenRead(file))
            {
                using (BinaryReader binaryReader = new BinaryReader(raf))
                {
                    count = binaryReader.ReadInt32().EnsureEndianness();
                    blockSize = binaryReader.ReadInt32().EnsureEndianness();
                    int pageLength = GetPowerOf2(MAX_BUF / blockSize, MAX_BUF / blockSize);
                    pageShift = 32 - (pageLength - 1).NumberOfLeadingZeros();
                    indexMask = (1 << pageShift - 1) - 1;
                    long l = 0;
                    int pageCounter = 0;
                    while (l < count * blockSize)
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
                            data[i] = new byte[count * blockSize - total];
                        }
                        binaryReader.Read(data[i]);
                    }
                }
            }
        }
        int GetPowerOf2(int k, int limit)
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

        public int GetCount()
        {
            return count;
        }

        public int GetBlockSize()
        {
            return blockSize;
        }

        public void Get(int index, byte[] buff)
        {
            int pageId = index >> pageShift;
            int pageIndex = (index & indexMask) * blockSize;
            byte[] d = data[pageId];
            Array.Copy(d, pageIndex, buff, 0, blockSize);
        }

        public int GetInt(int index)
        {
            int pageId = index >> pageShift;
            int pageIndex = (index & indexMask) * blockSize;
            byte[] d = data[pageId];
            switch (blockSize)
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

        public float GetFloat(int index)
        {
            int pageId = index >> pageShift;
            int pageIndex = (index & indexMask) * blockSize;
            byte[] d = data[pageId];
            return (
                ((d[pageIndex] & 0xff) << 24) |
                    ((d[pageIndex + 1] & 0xff) << 16) |
                    ((d[pageIndex + 2] & 0xff) << 8) |
                    (d[pageIndex + 3] & 0xff)).ToFloatFromBits();
        }

    }
}
