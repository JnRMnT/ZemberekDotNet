using System;
using System.IO;
using ZemberekDotNet.Core.Hash;

namespace ZemberekDotNet.Core.Quantization
{
    public class DoubleLookup
    {
        int range;
        private double[] data;

        public DoubleLookup(double[] data)
        {
            this.data = data;
            this.range = data.Length;
        }

        public static DoubleLookup GetLookup(BinaryReader dis)
        {
            int range = dis.ReadInt32().EnsureEndianness();
            byte[] data = new byte[range * 8];
            dis.Read(data);
            return new DoubleLookup(new ByteArrayLoader(data).GetAllDoubles(range));
        }

        public static DoubleLookup GetLookup(string file)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1000000))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    return GetLookup(binaryReader);
                }
            }
        }

        public static void ChangeBase(double[] data, double source, double target)
        {
            double multiplier = System.Math.Log(source) / System.Math.Log(target);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = data[i] * multiplier;
            }
        }

        public int GetRange()
        {
            return range;
        }

        public int GetIndex(double value)
        {
            int index = Array.BinarySearch(data, value);
            if (index < 0)
            {
                throw new ArgumentException("value cannot be found in lookup:" + value);
            }
            else
            {
                return index;
            }
        }

        public int GetClosestIndex(double value)
        {
            int index = Array.BinarySearch(data, value);
            if (index < 0)
            {
                return -index;
            }
            else
            {
                return index;
            }
        }

        public void Save(string file)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write, 1000000))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(range.EnsureEndianness());
                    foreach (double v in data)
                    {
                        binaryWriter.Write(v.EnsureEndianness());
                    }
                }
            }
        }

        public void Save(BinaryWriter dos)
        {
            dos.Write(range.EnsureEndianness());
            foreach (double v in data)
            {
                dos.Write(v.EnsureEndianness());
            }
        }

        public void ChangeBase(double source, double target)
        {
            ChangeBase(data, source, target);
        }

        /**
         * Returns dequantized value of the given integer index.
         *
         * @param n value to deQuantize
         * @return dequanztized value.
         */
        public double Get(int n)
        {
            if (n < 0 || n >= range)
            {
                throw new ArgumentException("Cannot dequantize value. Value is out of range:" + n);
            }
            return data[n];
        }
    }
}
