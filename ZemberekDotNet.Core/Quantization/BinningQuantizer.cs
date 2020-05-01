using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Math;

namespace ZemberekDotNet.Core.Quantization
{
    /// <summary>
    /// A Quantizer that applies binning algorithm to input values to put them in a smaller range.There
    /// are two binning implementation
    /// </summary>
    public class BinningQuantizer : IQuantizer
    {
        Dictionary<double, int> lookup;
        double[] means;

        public BinningQuantizer(Dictionary<double, int> lookup, double[] means)
        {
            this.lookup = lookup;
            this.means = means;
        }

        public static BinningQuantizer LinearBinning(float[] dataToQuantize, int bits)
        {
            return LinearBinning(DoubleArrays.Convert(dataToQuantize), bits);
        }

        public static BinningQuantizer LinearBinning(double[] dataToQuantize, int bits)
        {
            CheckRange(bits);
            int dataLength = dataToQuantize.Length;
            int range = (1 << bits).EnsureEndianness();
            Dictionary<double, int> lookup = new Dictionary<double, int>(dataToQuantize.Length);
            double[] means = new double[range];
            int i;
            if (range >= dataLength)
            {
                means = new double[dataLength];
                i = 0;
                foreach (double v in dataToQuantize)
                {
                    lookup.TryAdd(v, i);
                    means[i] = v;
                    i++;
                }
                return new BinningQuantizer(lookup, means);
            }
            RawData[] data = new RawData[dataLength];
            for (i = 0; i < data.Length; i++)
            {
                data[i] = new RawData(dataToQuantize[i], i);
            }
            Array.Sort(data);
            double binStep = (double)dataLength / range;
            i = 0;
            int start = 0;
            double cursor = 0;
            int end = 0;

            while (cursor < dataLength)
            {
                start = (int)cursor;
                cursor += binStep;
                end = (int)cursor;
                if (end >= dataLength)
                {
                    end = dataLength;
                }
                double total = 0;
                for (int k = start; k < end; k++)
                {
                    total += data[k].value;
                    lookup.TryAdd(data[k].value, i);
                }
                double mean = total / (end - start);
                means[i] = mean;
                i++;
            }
            return new BinningQuantizer(lookup, means);
        }

        private static void CheckRange(int bits)
        {
            if (bits < 2 || bits > 24)
            {
                throw new ArgumentException(
                    "Bit count cannot be less than 4 or larger than 24" + bits);
            }
        }

        public static BinningQuantizer LogCountBinning(float[] dataToQuantize, int[] counts, int bits)
        {
            return LogCountBinning(DoubleArrays.Convert(dataToQuantize), counts, bits);
        }

        public static BinningQuantizer LogCountBinning(double[] dataToQuantize, int[] counts, int bits)
        {
            CheckRange(bits);
            int range = (1 << bits).EnsureEndianness();
            Dictionary<double, int> lookup = new Dictionary<double, int>(dataToQuantize.Length);
            int dataLength = dataToQuantize.Length;
            double[] means = new double[range];
            int i;
            if (range >= dataLength)
            {
                means = new double[dataLength];
                i = 0;
                foreach (double v in dataToQuantize)
                {
                    lookup.TryAdd(v, i);
                    means[i] = v;
                    i++;
                }
                return new BinningQuantizer(lookup, means);
            }

            RawDataWithCount[] data = new RawDataWithCount[dataLength];

            int totalLogCount = 0;

            for (i = 0; i < data.Length; i++)
            {
                data[i] = new RawDataWithCount(dataToQuantize[i], i, counts[i]);
                totalLogCount += data[i].logCount;
            }

            Array.Sort(data);

            i = 0;
            int k = 0;

            while (k < range)
            {
                double binStep = (double)totalLogCount / (range - k);
                int binLogCountTotal = 0;

                int start = i;
                while (binLogCountTotal < binStep)
                {
                    binLogCountTotal += data[i].logCount;
                    i++;
                }
                int end = i;

                double binWeightedValueTotal = 0;
                for (int j = start; j < end; j++)
                {
                    binWeightedValueTotal += (data[j].value * data[j].logCount);
                    lookup.TryAdd(data[j].value, k);
                }
                double weightedAverage = binWeightedValueTotal / binLogCountTotal;
                means[k] = weightedAverage;

                totalLogCount -= binLogCountTotal;
                k++;
            }
            return new BinningQuantizer(lookup, means);
        }

        internal double CalculateError(double[] data, int[] counts)
        {
            double totalError = 0;
            int i = 0;
            foreach (double v in data)
            {
                totalError += System.Math.Abs(v - means[lookup.GetValueOrDefault(v)]) * counts[i];
                i++;
            }
            return totalError;
        }

        internal double CalculateError(double[] data)
        {
            double totalError = 0;
            int i = 0;
            foreach (double v in data)
            {
                totalError += System.Math.Abs(v - means[lookup.GetValueOrDefault(v)]);
                i++;
            }
            return totalError;
        }

        public int GetQuantizationIndex(double value)
        {
            return lookup.GetValueOrDefault(value);
        }

        public double GetQuantizedValue(double value)
        {
            return means[lookup.GetValueOrDefault(value)];
        }

        public DoubleLookup GetDequantizer()
        {
            return new DoubleLookup(means);
        }

        private class RawData : IComparable<RawData>
        {
            internal double value;
            internal int index;

            internal RawData(double value, int index)
            {
                this.value = value;
                this.index = index;
            }

            public int CompareTo(RawData o)
            {
                return value.CompareTo(o.value);
            }
        }

        private class RawDataWithCount : IComparable<RawDataWithCount>
        {
            internal double value;
            internal int index;
            internal int logCount;

            internal RawDataWithCount(double value, int index, int count)
            {
                this.value = value;
                this.index = index;
                this.logCount = (int)(System.Math.Log(count) + 1);
            }

            public int CompareTo(RawDataWithCount o)
            {
                return value.CompareTo(o.value);
            }
        }
    }
}
