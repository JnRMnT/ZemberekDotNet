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
            BinaryReader dis = GetDataInputStream();
            double[] d = new double[amount];
            for (int i = 0; i < amount; i++)
            {
                d[i] = dis.ReadDouble();
            }
            return d;
        }

        public int[] GetAllInts(int amount)
        {
            BinaryReader dis = GetDataInputStream();
            int[] d = new int[amount];

            for (int i = 0; i < amount; i++)
            {
                d[i] = dis.ReadInt32();
            }
            return d;
        }

        public float[] GetAllFloats(int amount)
        {
            BinaryReader dis = GetDataInputStream();
            float[] d = new float[amount];
            for (int i = 0; i < amount; i++)
            {
                d[i] = dis.ReadSingle();
            }
            return d;
        }
        public float[] GetAllFloatsFromDouble(int amount)
        {
            BinaryReader dis = GetDataInputStream();
            float[] d = new float[amount];
            for (int i = 0; i < amount; i++)
            {
                d[i] = (float)dis.ReadDouble();
            }
            return d;
        }
    }
}
