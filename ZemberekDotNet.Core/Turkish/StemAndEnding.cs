using System;
using System.Diagnostics.Contracts;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Core.Turkish
{
    /// <summary>
    /// Represents a word splitted as stem and ending. if there is only stem, ending is empty string ""
    /// </summary>
    public class StemAndEnding
    {
        public readonly string stem;
        public readonly string ending;

        public StemAndEnding(string stem, string ending)
        {
            if (Strings.HasText(stem))
            {
                Contract.Requires(Strings.HasText(stem), "Stem needs to have text");
            }
            if (!Strings.HasText(ending))
            {
                ending = "";
            }
            this.stem = stem;
            this.ending = ending;
        }

        public static StemAndEnding FromSpaceSepareted(string input)
        {
            string[] splitResult = TextUtil.SpaceSplitter(input);
            if (splitResult.Length == 1)
            {
                return new StemAndEnding(splitResult[0], "");
            }
            else if (splitResult.Length == 2)
            {
                return new StemAndEnding(splitResult[0], splitResult[1]);
            }
            throw new ArgumentException("Input contains more than two words" + input);
        }

        public override string ToString()
        {
            return stem + "-" + ending;
        }

        public string Concat()
        {
            return stem + ending;
        }

        public bool HasEnding()
        {
            return ending.Length > 0;
        }

        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !GetType().Equals(o.GetType()))
            {
                return false;
            }

            StemAndEnding that = (StemAndEnding)o;

            if (!Objects.Equals(ending, that.ending))
            {
                return false;
            }
            if (!Objects.Equals(stem, that.stem))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = stem != null ? stem.GetHashCode() : 0;
            result = 31 * result + (ending != null ? ending.GetHashCode() : 0);
            return result;
        }
    }
}
