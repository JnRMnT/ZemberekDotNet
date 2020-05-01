using System;
using System.IO;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Quantization;

namespace ZemberekDotNet.LM.Compression
{
    public class BinaryFloatFileReader
    {
        string file;
        int count;
        FileStream raf;
        BinaryReader binaryReader;

        public BinaryFloatFileReader(string file)
        {
            this.file = file;
            raf = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1000000);
            binaryReader = new BinaryReader(raf);
            count = binaryReader.ReadInt32().EnsureEndianness();
        }

        public static IQuantizer GetQuantizer(string file, int bitCount, QuantizerType quantizerType)
        {
            BinaryFloatFileReader reader = new BinaryFloatFileReader(file);
            using (BinaryReader dis = reader.GetStream())
            {
                dis.BaseStream.Seek(4, SeekOrigin.Begin); // skip the count.
                LookupCalculator lookupCalc = new LookupCalculator(bitCount);
                for (int i = 0; i < reader.count; i++)
                {
                    double d = dis.ReadSingle().EnsureEndianness();
                    lookupCalc.Add(d);
                    if (i % 500000 == 0)
                    {
                        Log.Debug("Values added to value histogram = {0}", i);
                    }
                }
                return lookupCalc.GetQuantizer(quantizerType);
            }
        }

        public BinaryReader GetStream()
        {
            return this.binaryReader;
        }

        public void GetFloat(int index, float[] data)
        {
            binaryReader.BaseStream.Seek(index * 4, SeekOrigin.Begin);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = binaryReader.ReadSingle().EnsureEndianness();
            }
        }

        private class LookupCalculator
        {
            int bitCount;
            int n;
            Histogram<double?> histogram;

            internal LookupCalculator(int bitCount)
            {
                this.bitCount = bitCount;
                this.n = 1 << bitCount;
                histogram = new Histogram<double?>(n / 2);
            }

            internal void Add(double d)
            {
                histogram.Add(d);
            }

            public IQuantizer GetQuantizer(QuantizerType type)
            {
                Log.Info("Unique value count:" + histogram.Size());
                double[] lookup = new double[histogram.Size()];
                int[] counts = new int[histogram.Size()];
                int j = 0;
                foreach (double key in histogram)
                {
                    lookup[j] = key;
                    counts[j] = histogram.GetCount(key);
                    j++;
                }
                Log.Info("Quantizing to " + bitCount + " bits");

                switch (type)
                {
                    case QuantizerType.Binning:
                        return BinningQuantizer.LinearBinning(lookup, bitCount);
                    case QuantizerType.BinningWeighted:
                        return BinningQuantizer.LogCountBinning(lookup, counts, bitCount);
                    case QuantizerType.KMeans:
                        return KMeansQuantizer.GenerateFromRawData(lookup, bitCount);
                    default:
                        throw new NotSupportedException("Linear cannot be used in this operation");
                }
            }

            public int GetSize()
            {
                return histogram.Size();
            }
        }
    }
}
