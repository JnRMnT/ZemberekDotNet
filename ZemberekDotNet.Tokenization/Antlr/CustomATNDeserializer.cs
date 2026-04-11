using Antlr4.Runtime.Atn;

namespace ZemberekDotNet.Tokenization.Antlr
{
    public class CustomATNDeserializer : ATNDeserializer
    {
        public override ATN Deserialize(int[] data)
        {
            return base.Deserialize(data);
        }
    }
}
