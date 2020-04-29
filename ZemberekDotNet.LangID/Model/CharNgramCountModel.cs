using System;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.LangID.Model
{
    /// <summary>
    /// This model holds the counts of char ngrams.
    /// </summary>
    [Serializable]
    public class CharNgramCountModel : BaseCharNgramModel
    {
        private static readonly long serialVersionUID = 0xBEEFCAFEABCDL;
        private Histogram<string>[] gramCounts;

        public CharNgramCountModel(string modelId, int order) : base(modelId, order)
        {
            gramCounts = new Histogram<string>[order + 1];
            for (int i = 0; i < gramCounts.Length; i++)
            {
                gramCounts[i] = new Histogram<string>();
            }
        }

        private CharNgramCountModel(string id, int order, Histogram<string>[] gramCounts) : base(id, order)
        {
            this.gramCounts = gramCounts;
        }

        /// <summary>
        /// Loads data from the custom serialized file and generates a CharNgramCountModel from it.
        /// </summary>
        /// <param name="f">file to load data.</param>
        /// <returns>a CharNgramCountModel generated from file.</returns>
        public static CharNgramCountModel Load(string f)
        {
            using (FileStream fileStream = File.OpenRead(f))
            {
                return Load(fileStream);
            }
        }

        /// <summary>
        /// Loads data from the custom serialized file and generates a CharNgramCountModel from it.
        /// </summary>
        /// <param name="stream">InputStream to load data.</param>
        /// <returns>a CharNgramCountModel generated from file.</returns>
        public static CharNgramCountModel Load(Stream stream)
        {
            using (BinaryReader dis = new BinaryReader(stream))
            {
                int order = dis.ReadInt32().EnsureEndianness();
                string modelId = dis.ReadUTF();
                Histogram<string>[] gramCounts = new Histogram<string>[order + 1];
                for (int j = 1; j <= order; j++)
                {
                    int size = dis.ReadInt32().EnsureEndianness();
                    Histogram<string> countSet = new Histogram<string>(size * 2);
                    for (int i = 0; i < size; i++)
                    {
                        string key = dis.ReadUTF();
                        countSet.Add(key, dis.ReadInt32().EnsureEndianness());
                    }
                    gramCounts[j] = countSet;

                }
                return new CharNgramCountModel(modelId, order, gramCounts);
            }
        }


        /// <summary>
        /// A custom serializer. Big-endian format is like this: int32 order Utf id int32 keyCount utf key
        /// int32 count ... int32 keyCount utf key int32 count ...
        /// </summary>
        /// <param name="f">file to serialize.</param>
        public void Save(string f)
        {
            using (FileStream fileStream = new FileStream(f, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter dos = new BinaryWriter(fileStream))
                {
                    dos.Write(order.EnsureEndianness());
                    dos.WriteUTF(id);
                    for (int i = 1; i < gramCounts.Length; i++)
                    {
                        dos.Write(gramCounts[i].Size().EnsureEndianness());
                        foreach (string key in gramCounts[i])
                        {
                            dos.WriteUTF(key);
                            dos.Write(gramCounts[i].GetCount(key).EnsureEndianness());
                        }
                    }
                }
            }
        }

        public void AddGrams(string seq)
        {
            for (int i = 1; i <= order; ++i)
            {
                List<string> grams = this.GetGram(seq, i);
                foreach (string gram in grams)
                {
                    gramCounts[i].Add(gram);
                }
            }
        }

        public void ApplyCutOffs(int[] cutOffs)
        {
            if (cutOffs == null)
            {
                return;
            }
            if (cutOffs.Length > order)
            {
                throw new ArgumentException(
                    "Cannot apply cutoff values. Cutoff array length " + cutOffs.Length
                        + " is larger than the order of the model " + order);
            }
            if (cutOffs.Length > 0)
            {
                for (int i = 0; i < cutOffs.Length; i++)
                {
                    int o = i + 1;
                    Console.WriteLine(o + " gram Count Before cut off: " + KeyCount(o));
                    RemoveSmaller(o, cutOffs[i] + 1);
                    Console.WriteLine(o + " gram Count After cut off: " + KeyCount(o));
                }
            }
        }

        public void Merge(CharNgramCountModel otherModel)
        {
            if (otherModel.order != order)
            {
                throw new ArgumentException(
                    "Model orders does not match. Order of this model is" + order +
                        " but merged model order is " + otherModel.order);
            }
            for (int i = 1; i < gramCounts.Length; i++)
            {
                gramCounts[i].Add(otherModel.gramCounts[i]);
            }
        }

        public int GetCount(int order, string key)
        {
            return gramCounts[order].GetCount(key);
        }

        public void Add(int order, string key)
        {
            gramCounts[order].Add(key);
        }

        public int KeyCount(int order)
        {
            return gramCounts[order].Size();
        }

        public bool ContainsKey(int order, string key)
        {
            return gramCounts[order].Contains(key);
        }

        public int RemoveSmaller(int order, int size)
        {
            return gramCounts[order].RemoveSmaller(size);
        }

        public int TotalCount(int order)
        {
            return (int)gramCounts[order].TotalCount();
        }

        public IEnumerable<string> GetKeyIterator(int order)
        {
            return gramCounts[order];
        }

        public IEnumerable<string> GetSortedKeyIterator(int order)
        {
            return gramCounts[order].GetSortedList();
        }

        public void DumpGrams(int order)
        {
            foreach (string s in GetSortedKeyIterator(order))
            {
                Console.WriteLine(s + " : " + GetCount(order, s));
            }
        }
    }
}
