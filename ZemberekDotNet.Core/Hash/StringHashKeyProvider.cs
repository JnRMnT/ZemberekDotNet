using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Hash
{
    public class StringHashKeyProvider : IIntHashKeyProvider
    {
        private readonly List<string> strings;

        public StringHashKeyProvider(List<string> strings)
        {
            this.strings = strings;
        }

        public StringHashKeyProvider(IEnumerable<string> iterable)
        {
            this.strings = new List<string>(iterable);
        }


        public int[] GetKey(int index)
        {
            String s = strings[index];
            int[] chars = new int[s.Length];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = s[i];
            }
            return chars;
        }

        public int KeyAmount()
        {
            return strings.Count;
        }
    }
}
