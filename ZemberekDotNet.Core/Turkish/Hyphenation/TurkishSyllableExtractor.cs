using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Core.Turkish.Hyphenation
{
    /// <summary>
    /// 
    /// This syllable service is designed for extracting syllable information from Turkish words.
    /// Algorithm cannot parse words like "tren", "spor", "sfinks", "angstrom", "mavimtrak", "stetoskop"
    /// etc.
    /// 
    /// </summary>
    public class TurkishSyllableExtractor : ISyllableExtractor
    {
        private readonly TurkishAlphabet alphabet = TurkishAlphabet.Instance;

        // STRICT extractor cannot parse words ending two consonants. Such as `kart`, `yoğurt`
        public static TurkishSyllableExtractor Strict = new TurkishSyllableExtractor(true);

        // DEFAULT extractor allows parsing words like "kitapt -> ki-tapt"
        public static TurkishSyllableExtractor Default = new TurkishSyllableExtractor(false);

        public readonly bool strict;

        private static readonly HashSet<string> acceptedSyllablePrefixes;

        static TurkishSyllableExtractor()
        {
            acceptedSyllablePrefixes = new HashSet<string>();
            try
            {
                acceptedSyllablePrefixes.AddRange(TextIO.LoadLines("Resources/Syllable/accepted-syllable-prefixes"));
            }
            catch (IOException)
            {
                Log.Warn("Cannot find accepted syllable prefixes.");
            }
        }

        private TurkishSyllableExtractor(bool strict)
        {
            this.strict = strict;
        }

        public List<string> GetSyllables(string str)
        {
            int[] boundaries = SyllableBoundaries(str);
            List<string> result = new List<string>();
            for (int i = 0; i < boundaries.Length - 1; i++)
            {
                int boundary = boundaries[i];
                result.Add(str.Substring(boundary, boundaries[i + 1]));
            }
            if (boundaries.Length > 0)
            {
                result.Add(str.Substring(boundaries[boundaries.Length - 1]));
            }
            return result;
        }

        public int[] SyllableBoundaries(string str)
        {
            int size = str.Length;
            char[] chr = str.ToCharArray();
            int[] boundaryIndexes = new int[size];
            int lastIndex = size;
            int index = 0;
            while (lastIndex > 0)
            {
                int letterCount = LetterCountForLastSyllable(chr, lastIndex);
                if (letterCount == -1)
                {
                    return new int[0];
                }
                boundaryIndexes[index++] = lastIndex - letterCount;
                lastIndex -= letterCount;
            }
            int[] result = new int[index];
            for (int i = 0; i < index; i++)
            {
                result[i] = boundaryIndexes[index - i - 1];
            }
            return result;
        }


        private bool IsVowel(char c)
        {
            return alphabet.IsVowel(c);
        }

        private int LetterCountForLastSyllable(char[] chr, int endIndex)
        {

            if (endIndex == 0)
            {
                return -1;
            }

            if (IsVowel(chr[endIndex - 1]))
            {
                if (endIndex == 1)
                {
                    return 1;
                }
                if (IsVowel(chr[endIndex - 2]))
                {
                    return 1;
                }
                if (endIndex == 2)
                {
                    return 2;
                }
                if (!IsVowel(chr[endIndex - 3]) && endIndex == 3)
                {
                    return 3;
                }
                return 2;
            }
            else
            {
                if (endIndex == 1)
                {
                    return -1;
                }
                if (IsVowel(chr[endIndex - 2]))
                {
                    if (endIndex == 2 || IsVowel(chr[endIndex - 3]))
                    {
                        return 2;
                    }
                    if (endIndex == 3 || IsVowel(chr[endIndex - 4]))
                    {
                        return 3;
                    }
                    // If the word is 4 letters and rules above passed, we assume this cannot be parsed.
                    // That is why words like tren, strateji, krank, angstrom cannot be parsed.
                    if (endIndex == 4)
                    {
                        return -1;
                    }
                    if (!IsVowel(chr[endIndex - 5]))
                    {
                        return 3;
                    }
                    return 3;
                }
                else
                {
                    if (strict && !IsVowel(chr[endIndex - 2]))
                    {
                        return -1;
                    }
                    if (endIndex == 2 || !IsVowel(chr[endIndex - 3]))
                    {
                        return -1;
                    }
                    if (endIndex > 3 && !IsVowel(chr[endIndex - 4]))
                    {
                        return 4;
                    }
                    return 3;
                }
            }
        }
    }
}
