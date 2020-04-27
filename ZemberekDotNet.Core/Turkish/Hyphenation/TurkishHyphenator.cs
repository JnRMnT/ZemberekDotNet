using System.Collections.Generic;

namespace ZemberekDotNet.Core.Turkish.Hyphenation
{
    public class TurkishHyphenator : IHyphenator
    {
        readonly ISyllableExtractor syllableExtractor;
        public static readonly TurkishHyphenator Default = new TurkishHyphenator(TurkishSyllableExtractor.Default);
        public static readonly TurkishHyphenator Strict = new TurkishHyphenator(TurkishSyllableExtractor.Strict);

        protected TurkishHyphenator(ISyllableExtractor syllableExtractor)
        {
            this.syllableExtractor = syllableExtractor;
        }

        public int SplitIndex(string input, int spaceAvailable)
        {
            // handle big space amount.
            if (spaceAvailable >= input.Length)
            {
                return input.Length;
            }

            List<string> pieces = syllableExtractor.GetSyllables(input);

            // handle no syllable.
            if (pieces.IsEmpty())
            {
                return -1;
            }

            // find breaking syllable index.
            int remainingSpace = spaceAvailable;
            int index = 0;
            foreach (string piece in pieces)
            {
                if (piece.Length < remainingSpace)
                {
                    remainingSpace -= piece.Length;
                    index++;
                }
                else
                {
                    break;
                }
            }

            // handle first syllable does not fit spaceAvailable.
            if (index == 0)
            {
                return -1;
            }

            // find breaking letter index + 1 .
            int k = 0;
            for (int j = 0; j < index; j++)
            {
                k += pieces[j].Length;
            }
            return k;
        }
    }
}
