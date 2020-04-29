using System.IO;

namespace ZemberekDotNet.Core.Hash
{
    public class ByteArrayLoader
    {
        byte[] data;

        public ByteArrayLoader(byte[] data)
        {
            this.data = data;
        }

        public BinaryReader GetDataInputStream()
        {
            return new BinaryReader(new MemoryStream(data));
        }

        public double[] GetAllDoubles(int amount)
        {
            using (BinaryReader dis = GetDataInputStream())
            {
                double[] d = new double[amount];
                for (int i = 0; i < amount; i++)
                {
                    d[i] = dis.ReadDouble().EnsureEndianness();
                }
                return d;
            }
        }

        public int[] GetAllInts(int amount)
        {
            using (BinaryReader dis = GetDataInputStream())
            {
                int[] d = new int[amount];

                for (int i = 0; i < amount; i++)
                {
                    d[i] = dis.ReadInt32().EnsureEndianness();
                }
                return d;
            }
        }

        public float[] GetAllFloats(int amount)
        {
            using (BinaryReader dis = GetDataInputStream())
            {
                float[] d = new float[amount];
                for (int i = 0; i < amount; i++)
                {
                    d[i] = dis.ReadSingle().EnsureEndianness();
                }
                return d;
            }
        }
        public float[] GetAllFloatsFromDouble(int amount)
        {
            using (BinaryReader dis = GetDataInputStream())
            {
                float[] d = new float[amount];
                for (int i = 0; i < amount; i++)
                {
                    d[i] = (float)dis.ReadDouble().EnsureEndianness();
                }
                return d;
            }
        }
    }
}
