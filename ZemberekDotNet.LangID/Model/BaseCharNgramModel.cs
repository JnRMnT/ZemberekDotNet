using System.Collections.Generic;

namespace ZemberekDotNet.LangID.Model
{
    public abstract class BaseCharNgramModel
    {
        public static readonly string Unknown = "unk";
        public readonly string id;
        public readonly int order;

        protected BaseCharNgramModel(string id, int order)
        {
            this.id = id;
            this.order = order;
        }

        public static List<string> GetGrams(string input, int order)
        {
            List<string> grams = new List<string>(input.Length);
            for (int i = 0; i < order - 1; i++)
            {
                if (i == input.Length)
                {
                    return grams;
                }
                grams.Add(input.Substring(0, i + 1));
            }
            for (int i = 0; i < input.Length - order + 1; i++)
            {
                grams.Add(input.Substring(i, order));
            }
            return grams;
        }

        public List<string> GetGram(string s, int order)
        {
            List<string> grams = new List<string>();
            for (int i = 0; i < (s.Length - order + 1); ++i)
            {
                grams.Add(s.Substring(i, order));
            }
            return grams;
        }
    }
}
