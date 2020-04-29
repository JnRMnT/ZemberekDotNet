using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace ZemberekDotNet.LangID.Model
{
    [Serializable]
    public class MapBasedCharNgramLanguageModel : BaseCharNgramModel, ICharNgramLanguageModel
    {
        private static readonly long serialVersionUID = 0xDEADBEEFCAFEL;
        public static double BackOff = -2;
        internal GramLogProbData[] gramLogProbs;
        [IgnoreDataMember]
        private CharNgramCountModel gramCounts;

        public MapBasedCharNgramLanguageModel(int order, String modelId, GramLogProbData[] gramLogProbs) : base(modelId, order)
        {
            this.gramLogProbs = gramLogProbs;
        }

        public MapBasedCharNgramLanguageModel(CharNgramCountModel gramCounts) : base(gramCounts.id, gramCounts.order)
        {
            this.gramCounts = gramCounts;
            gramLogProbs = new GramLogProbData[order + 1];
            for (int i = 1; i < order + 1; i++)
            {
                gramLogProbs[i] = new GramLogProbData();
            }
        }

        public MapBasedCharNgramLanguageModel(string modelId, int order) : base(modelId, order)
        {
            gramCounts = new CharNgramCountModel(modelId, order);
            gramLogProbs = new GramLogProbData[order + 1];
            for (int i = 1; i < order + 1; i++)
            {
                gramLogProbs[i] = new GramLogProbData();
            }
        }

        public static MapBasedCharNgramLanguageModel Train(CharNgramCountModel countModel)
        {
            MapBasedCharNgramLanguageModel m = new MapBasedCharNgramLanguageModel(countModel);
            for (int i = 1; i <= m.order; ++i)
            {
                m.CalculateProbabilities(i);
            }
            return m;
        }

        public static MapBasedCharNgramLanguageModel LoadCustom(Stream f)
        {
            using (BinaryReader dis = new BinaryReader(f))
            {
                int order = dis.ReadInt32().EnsureEndianness();
                String modelId = dis.ReadUTF();
                GramLogProbData[] logProbs = new GramLogProbData[order + 1];
                for (int j = 1; j <= order; j++)
                {
                    int size = dis.ReadInt32().EnsureEndianness();
                    Dictionary<string, double> probs = new Dictionary<string, double>();
                    for (int i = 0; i < size; i++)
                    {
                        String key = dis.ReadUTF();
                        probs.Add(key, (double)dis.ReadSingle().EnsureEndianness());
                    }
                    logProbs[j] = new GramLogProbData(probs);

                }
                return new MapBasedCharNgramLanguageModel(order, modelId, logProbs);
            }
        }

        public static MapBasedCharNgramLanguageModel LoadCustom(string f)
        {
            using (FileStream fileStream = File.OpenRead(f))
            {
                return LoadCustom(fileStream);
            }
        }

        public void CalculateProbabilities(int o)
        {
            Dictionary<string, double> frequencyMap = gramLogProbs[o].values;
            if (o == 1)
            {
                int total = gramCounts.TotalCount(1);
                foreach (string s in gramCounts.GetKeyIterator(o))
                {
                    double prob = Math.Log((double)gramCounts.GetCount(o, s) / (double)total);
                    frequencyMap.Add(s, prob);
                }
            }
            else
            {
                foreach (string s in gramCounts.GetKeyIterator(o))
                {
                    string parentGram = s.Substring(0, o - 1);
                    if (!gramCounts.ContainsKey(o - 1, parentGram))
                    {
                        continue;
                    }
                    int cnt = gramCounts.GetCount(o - 1, parentGram);
                    double prob = Math.Log((double)gramCounts.GetCount(o, s) / (double)cnt);
                    frequencyMap.Add(s, prob);
                }
            }
        }

        public double GramProbability(string gram)
        {
            if (gram.Length == 0)
            {
                return -10;
            }
            if (gram.Length > order)
            {
                throw new ArgumentException(
                    "Gram size is larger than order! gramSize=" + gram.Length + " but order is:" + order);
            }
            int o = gram.Length;
            if (gramLogProbs[o].values.ContainsKey(gram))
            {
                return gramLogProbs[o].values.GetValueOrDefault(gram);
            }
            return BackOff + GramProbability(gram.Substring(0, o - 1));
        }

        public int GetOrder()
        {
            return order;
        }

        public string GetId()
        {
            return id;
        }

        public void SaveCustom(string f)
        {
            using (FileStream fileStream = new FileStream(f, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter dos = new BinaryWriter(fileStream))
                {
                    dos.Write(order.EnsureEndianness());
                    dos.WriteUTF(id);
                    for (int i = 1; i < gramLogProbs.Length; i++)
                    {
                        Dictionary<string, double> probs = gramLogProbs[i].values;
                        dos.Write(probs.Count);
                        foreach (string key in probs.Keys)
                        {
                            dos.WriteUTF(key);
                            dos.Write((float)probs.GetValueOrDefault(key).EnsureEndianness());
                        }
                    }
                }
            }
        }

        public void Dump()
        {
            Console.WriteLine("Model ID=" + id + " Order=" + order);
            for (int i = 1; i < gramLogProbs.Length; i++)
            {
                Console.WriteLine(gramLogProbs[i].values.Count);
            }
        }

        [Serializable]
        public class GramLogProbData
        {
            internal Dictionary<string, double> values = new Dictionary<string, double>();
            internal GramLogProbData()
            {
            }

            internal GramLogProbData(Dictionary<string, double> values)
            {
                this.values = values;
            }
        }
    }
}
