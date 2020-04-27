using System.Collections.Generic;
using System.Globalization;

namespace ZemberekDotNet.Core.Turkish
{
    public class Turkish
    {
        public static readonly CultureInfo Locale = CultureInfo.GetCultureInfo("tr");
        public static readonly TurkishAlphabet Alphabet = TurkishAlphabet.Instance;
        public static readonly CompareInfo Collator = Locale.CompareInfo;
        public static readonly IComparer<string> StringComparatorAsc = new TurkishStringComparator();

        public static string Capitalize(string word)
        {
            if (word.Length == 0)
            {
                return word;
            }
            return word.Substring(0, 1).ToUpper(Locale) + word.Substring(1).ToLower(Locale);
        }

        private class TurkishStringComparator : IComparer<string>
        {
            public int Compare(string o1, string o2)
            {
                return Collator.Compare(o1,o2);
            }
        }
    }
}
