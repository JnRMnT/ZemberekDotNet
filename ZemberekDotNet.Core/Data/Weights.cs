using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Compression;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Core.Data
{
    public class Weights : IWeightLookup, IEnumerable<string>
    {
        private static readonly float epsilon = 0.0001f;

        FloatValueMap<string> data;

        public Weights(FloatValueMap<string> data)
        {
            this.data = data;
        }

        public Weights()
        {
            data = new FloatValueMap<string>(10000);
        }

        public FloatValueMap<string> GetData()
        {
            return data;
        }

        public int Size()
        {
            return data.Size();
        }

        public static Weights loadFromFile(string file)
        {
            List<string> all = TextIO.LoadLines(file);
            return LoadFromLines(all);
        }

        public static Weights LoadFromLines(List<string> lines)
        {
            FloatValueMap<string> data = new FloatValueMap<string>(10000);
            foreach (string s in lines)
            {
                float weight = float.Parse(Strings.SubstringUntilFirst(s, " "));
                string key = Strings.SubstringAfterFirst(s, " ");
                data.Set(key, weight);
            }
            return new Weights(data);
        }

        public void SaveAsText(string file)
        {
            using (StreamWriter streamWriter = new StreamWriter(file, false, Encoding.UTF8))
            {
                foreach (string s in data.GetKeyList())
                {
                    streamWriter.WriteLine(string.Format("{0:0.0000} {1}", data.Get(s), s));
                }
            }
        }

        public Weights Copy()
        {
            return new Weights(data.Copy());
        }

        public void PruneNearZeroWeights()
        {
            FloatValueMap<string> pruned = new FloatValueMap<string>();

            foreach (string key in data)
            {
                float w = data.Get(key);
                if (System.Math.Abs(w) > epsilon)
                {
                    pruned.Set(key, w);
                }
            }
            this.data = pruned;
        }

        public CompressedWeights Compress()
        {
            return new CompressedWeights(LossyIntLookup.Generate(data));
        }

        public float Get(string key)
        {
            return data.Get(key);
        }

        public void Put(string key, float value)
        {
            this.data.Set(key, value);
        }

        public void Increment(string key, float value)
        {
            data.IncrementByAmount(key, value);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }
    }
}
