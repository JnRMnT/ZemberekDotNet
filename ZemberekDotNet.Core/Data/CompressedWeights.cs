using System.IO;
using ZemberekDotNet.Core.Compression;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Data
{
    public class CompressedWeights : IWeightLookup
    {
        LossyIntLookup lookup;

        public CompressedWeights(LossyIntLookup lookup)
        {
            this.lookup = lookup;
        }

        public float Get(string key)
        {
            return lookup.GetAsFloat(key);
        }

        public int Size()
        {
            return lookup.Size();
        }

        public void Serialize(string path)
        {
            lookup.Serialize(path);
        }

        public static CompressedWeights Deserialize(string path)
        {
            LossyIntLookup lookup = LossyIntLookup.Deserialize(IOUtil.GetDataInputStream(path));
            return new CompressedWeights(lookup);
        }

        public static bool IsCompressed(BinaryReader dis)
        {
            return LossyIntLookup.CheckStream(dis);
        }

        public static bool IsCompressed(string path)
        {
            BinaryReader dis = IOUtil.GetDataInputStream(path);
            return IsCompressed(dis);
        }
    }
}
