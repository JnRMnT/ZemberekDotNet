using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZemberekDotNet.Core.Hash
{
    public class ByteGramProvider : IIntHashKeyProvider
    {
        byte[] data;
        int order;
        int ngramCount;

        public ByteGramProvider(FileStream file)
        {
            BinaryReader raf = new BinaryReader(file);
            this.order = raf.ReadInt32().EnsureEndianness();
            this.ngramCount = raf.ReadInt32().EnsureEndianness();
            int byteAmount = order * ngramCount * 4;
            data = new byte[byteAmount];
            int actual = raf.Read(data);
            if (actual != byteAmount)
            {
                throw new InvalidOperationException(
                    "File suppose to have " + byteAmount + " bytes for " + ngramCount + " ngrams");
            }
            raf.Close();
        }

        public ByteGramProvider(byte[] data, int order, int ngramCount)
        {
            this.data = data;
            this.order = order;
            this.ngramCount = ngramCount;
        }

        public int[] GetKey(int index)
        {
            int[] res = new int[order];
            int start = index * order * 4;
            int p = 0;
            for (int i = start; i < start + order * 4; i += 4)
            {
                res[p++] = (data[i] & 0xff) << 24 | (data[i + 1] & 0xff) << 16 | (data[i + 2] & 0xff) << 8 | (
                    data[i + 3] & 0xff);
            }
            return res;
        }

        public void GetKey(int index, int[] b)
        {
            int start = index * order * 4;
            int p = 0;
            for (int i = start; i < start + order * 4; i += 4)
            {
                b[p++] = (data[i] & 0xff) << 24 | (data[i + 1] & 0xff) << 16 | (data[i + 2] & 0xff) << 8 | (
                    data[i + 3] & 0xff);
            }
        }

        public byte[] GetKeyAsBytes(int index)
        {
            int start = index * order * 4;
            byte[] buf = new byte[order * 4];
            Array.Copy(data, start, buf, 0, buf.Length);
            return buf;
        }

        public int KeyAmount()
        {
            return ngramCount;
        }
    }
}
