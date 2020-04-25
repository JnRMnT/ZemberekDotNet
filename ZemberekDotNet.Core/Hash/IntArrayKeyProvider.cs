using System;

namespace ZemberekDotNet.Core.Hash
{
    public class IntArrayKeyProvider : IIntHashKeyProvider, IHashKeyProvider
    {
        readonly int[][] arr;

        public IntArrayKeyProvider(int[][] arr)
        {
            this.arr = arr;
        }

        public int[] GetKey(int index)
        {
            return arr[index];
        }

        public byte[] GetKeyAsBytes(int index)
        {
            int[] k = arr[index];
            byte[] bytes = new byte[k.Length * 4];
            int j = 0;
            foreach (int r in k)
            {
                byte[] bytez = BitConverter.GetBytes(r);
                foreach (byte b in bytez)
                {
                    bytes[j++] = b;
                }
            }
            return bytes;
        }

        public int KeyAmount()
        {
            return arr.Length;
        }
    }
}
