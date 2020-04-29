using System;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Hash;
using ZemberekDotNet.Core.Quantization;

namespace ZemberekDotNet.LangID.Model
{
    /// <summary>
    /// A compressed character N-gram model that uses Minimal Perfect Hash functions and Quantization.
    /// System uses around 20 bits per n-gram. 8 Bit quantized probability value, 16 bit fingerprint for
    /// OOV detection and 3,5 bit for hash.
    /// </summary>
    public class CompressedCharNgramModel : BaseCharNgramModel, ICharNgramLanguageModel
    {
        public static readonly int UnkCharPenalty = -10;
        static readonly double BackOff = -2;
        static readonly int FingerPrintMask = (1 << 16) - 1;
        // all arrays below are 1 based
        IMphf[] mphfs;
        ProbData[] gramData;
        DoubleLookup[] lookups;


        private CompressedCharNgramModel(int order, String modelId, IMphf[] mphfs, ProbData[] gramData,
            DoubleLookup[] lookups) : base(modelId, order)
        {
            this.mphfs = mphfs;
            this.gramData = gramData;
            this.lookups = lookups;
        }

        /**
         * Applies compression to a char language model using Minimal Perfect hash functions and
         * quantization
         *
         * @param input raw model file
         * @param output compressed file
         */
        public static void Compress(string input, string output)
        {
            Compress(MapBasedCharNgramLanguageModel.LoadCustom(input), output);
        }

        public static void Compress(MapBasedCharNgramLanguageModel model, string output)

        {
            IMphf[] mphfs = new MultiLevelMphf[model.GetOrder() + 1];
            DoubleLookup[] lookups = new DoubleLookup[model.GetOrder() + 1];

            using (FileStream fileStream = new FileStream(output, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter dos = new BinaryWriter(fileStream))
                {
                    dos.Write(model.GetOrder().EnsureEndianness());
                    dos.WriteUTF(model.GetId());

                    for (int i = 1; i <= model.GetOrder(); i++)
                    {
                        Histogram<double> histogram = new Histogram<double>();
                        histogram.Add(model.gramLogProbs[i].values.Values);
                        double[] lookup = new double[histogram.Size()];
                        int j = 0;
                        foreach (double key in histogram)
                        {
                            lookup[j] = key;
                            j++;
                        }
                        IQuantizer quantizer = BinningQuantizer.LinearBinning(lookup, 8);
                        lookups[i] = quantizer.GetDequantizer();
                        List<string> keys = new List<string>(model.gramLogProbs[i].values.Keys);

                        int[] fingerprints = new int[keys.Count];
                        int[] probabilityIndexes = new int[keys.Count];

                        mphfs[i] = MultiLevelMphf.Generate(new StringListKeyProvider(keys));

                        foreach (string key in keys)
                        {
                            int index = mphfs[i].Get(key);
                            fingerprints[index] = MultiLevelMphf.Hash(key, -1) & FingerPrintMask;
                            probabilityIndexes[index] = quantizer.GetQuantizationIndex(
                                model.gramLogProbs[i].values.GetValueOrDefault(key));
                        }

                        lookups[i].Save(dos);
                        dos.Write(keys.Count.EnsureEndianness());
                        for (int k = 0; k < keys.Count; k++)
                        {
                            dos.Write((short)(fingerprints[k] & 0xffff).EnsureEndianness());
                            dos.Write((byte)(probabilityIndexes[k]).EnsureEndianness());
                        }
                        mphfs[i].Serialize(dos);
                    }
                }
            }
        }

        public static CompressedCharNgramModel Load(Stream inputStream)
        {
            using (BinaryReader dis = new BinaryReader(inputStream))
            {
                int order = dis.ReadInt32().EnsureEndianness();
                string modelId = dis.ReadUTF();
                MultiLevelMphf[] mphfs = new MultiLevelMphf[order + 1];
                ProbData[] probDatas = new ProbData[order + 1];
                DoubleLookup[] lookups = new DoubleLookup[order + 1];
                for (int i = 1; i <= order; i++)
                {
                    lookups[i] = DoubleLookup.GetLookup(dis);
                    probDatas[i] = new ProbData(dis);
                    mphfs[i] = MultiLevelMphf.Deserialize(dis);
                }
                return new CompressedCharNgramModel(order, modelId, mphfs, probDatas, lookups);
            }
        }

        public static CompressedCharNgramModel Load(string file)
        {
            using (FileStream fileStream = File.OpenRead(file))
            {
                return Load(fileStream);
            }
        }

        /**
         * simple stupid back-off probability calculation
         *
         * @param gram gram String
         * @return log Probability
         */
        public double GramProbability(String gram)
        {
            if (gram.Length == 0)
            {
                return UnkCharPenalty;
            }
            if (gram.Length > order)
            {
                throw new ArgumentException("Gram size is larger than order! gramSize="
                    + gram.Length + " but order is:" + order);
            }
            int o = gram.Length;
            int fingerPrint = MultiLevelMphf.Hash(gram, -1);
            int hash = mphfs[o].Get(gram, fingerPrint);
            if ((fingerPrint & FingerPrintMask) == gramData[o].GetFP(hash))
            {
                return lookups[o].Get(gramData[o].GetProbLookupIndex(hash));
            }
            else
            {
                return BackOff + GramProbability(gram.Substring(0, o - 1));
            }
        }


        public int GetOrder()
        {
            return order;
        }

        public string GetId()
        {
            return id;
        }

        private class ProbData
        {
            internal byte[] data;

            internal ProbData(BinaryReader dis)
            {
                int count = dis.ReadInt32().EnsureEndianness();
                data = new byte[count * 3];
                dis.Read(data);
            }

            internal int GetFP(int index)
            {
                return ((data[index * 3] & 0xff) << 8) | (data[index * 3 + 1] & 0xff);
            }

            internal int GetProbLookupIndex(int index)
            {
                return data[index * 3 + 2] & 0xff;
            }
        }

        private class StringListKeyProvider : IIntHashKeyProvider
        {
            List<String> keys;

            public StringListKeyProvider(List<String> keys)
            {
                this.keys = keys;
            }

            public int[] GetKey(int index)
            {
                String key = keys[index];
                int[] arr = new int[key.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = key[i];
                }
                return arr;
            }

            public int KeyAmount()
            {
                return keys.Count;
            }
        }
    }
}
