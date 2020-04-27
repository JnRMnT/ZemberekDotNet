using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Math;

namespace ZemberekDotNet.Core.Quantization
{
    /// <summary>
    /// This applies a k-means like algorithm to the data points for quantization.It randomly chooses
    /// cluster centers form actual data.After that it re-assgins the mean value to the center of mass
    /// of the cluster points.If a cluster has no data points, it assigns the mean value as (most
    /// crowded-data-point-mean+mean)/2 it iterates 15 times to find the final mean values and uses them
    /// as quatization points.
    /// </summary>
    public class KMeansQuantizer : IQuantizer
    {
        internal readonly int range;
        internal double[] data;
        internal double[] sorted;
        Dictionary<double, int> lookup = new Dictionary<double, int>();

        private KMeansQuantizer(double[] data, int range, Dictionary<double, int> lookup)
        {
            this.data = data;
            this.range = range;
            this.lookup = lookup;
            this.sorted = (double[])data.Clone();
            Array.Sort(sorted);
        }


        public static KMeansQuantizer GenerateFromRawData(float[] dataToQuantize, int bits)
        {
            return GenerateFromRawData(DoubleArrays.Convert(dataToQuantize), bits);
        }

        /**
         * Creates a KMeansQuantizer with given histogram and bitsize.
         *
         * @param dataToQuantize input dataToQuantize to quantize.
         * @param bits quantization level amount in bits. There will be 2^bits level.
         * @return A Quantizer.
         */
        public static KMeansQuantizer GenerateFromRawData(double[] dataToQuantize, int bits)
        {
            if (bits < 4 || bits > 24)
            {
                throw new ArgumentException(
                    "Bit count cannot be less than 4 or larger than 24" + bits);
            }
            int range = 1 << bits;

            Dictionary<double, int> lookup = new Dictionary<double, int>();

            int dataLength = dataToQuantize.Length;
            if (range >= dataLength)
            {
                double[] means = new double[dataLength];
                int i = 0;
                foreach (double v in dataToQuantize)
                {
                    lookup.TryAdd(v, i);
                    means[i] = v;
                    i++;
                }
                return new KMeansQuantizer(means, dataLength, lookup);
            }
            return KMeans(dataToQuantize, range, 10);
        }

        private static KMeansQuantizer KMeans(double[] data, int clusterCount, int iterationCount)
        {
            double[] means = new double[clusterCount];

            Dictionary<double, int> lookup = new Dictionary<double, int>();

            int[] indexes = new int[data.Length];

            //initialization. means are placed using random data.
            MeanCount[] meanCounts = new MeanCount[clusterCount];
            Random r = new Random();
            int i;
            for (i = 0; i < clusterCount; i++)
            {
                means[i] = data[r.Next(data.Length)];
                meanCounts[i] = new MeanCount(i, 0);
            }

            for (int j = 0; j < iterationCount; j++)
            {
                // cluster points.
                for (i = 0; i < data.Length; i++)
                {
                    int closestMeanIndex = -1;
                    double m = double.PositiveInfinity;
                    for (int k = 0; k < means.Length; k++)
                    {
                        double dif = System.Math.Abs(data[i] - means[k]);
                        if (dif < m)
                        {
                            m = dif;
                            closestMeanIndex = k;
                        }
                    }
                    indexes[i] = closestMeanIndex;
                    meanCounts[closestMeanIndex].count++;
                }

                Array.Sort(meanCounts);
                // update means
                for (int k = 0; k < means.Length; k++)
                {
                    int pointCount = 0;
                    double meanTotal = 0;
                    for (i = 0; i < data.Length; i++)
                    {
                        if (indexes[i] == k)
                        {
                            pointCount++;
                            meanTotal += data[i];
                        }
                    }
                    // if there is no point in one cluster,reassign the mean value of the empty cluster to
                    // (most crowded cluster mean + empty cluseter mean ) /2
                    if (pointCount > 0)
                    {
                        means[k] = meanTotal / pointCount;
                    }
                    else
                    {
                        double m = (means[k] + means[meanCounts[0].index]) / 2;
                        means[k] = m;
                    }
                }
            }
            i = 0;
            // generate lookup for quantization.
            foreach (int index in indexes)
            {
                lookup.TryAdd(data[i++], index);
            }
            return new KMeansQuantizer(means, means.Length, lookup);
        }

        public int GetQuantizationIndex(double value)
        {
            if (!lookup.ContainsKey(value))
            {
                throw new ArgumentException(
                    "cannot quantize value. Value does not exist in quantization lookup:" + value);
            }
            return lookup.GetValueOrDefault(value);
        }

        public double GetQuantizedValue(double value)
        {
            return data[lookup.GetValueOrDefault(value)];
        }

        public DoubleLookup GetDequantizer()
        {
            return new DoubleLookup(data);
        }

        private class MeanCount : IComparable<MeanCount>
        {
            internal int index;
            internal int count;

            internal MeanCount(int index, int count)
            {
                this.index = index;
                this.count = count;
            }

            public int CompareTo(MeanCount o)
            {
                return -count.CompareTo(o.count);
            }
        }
    }
}
