using System.Collections.Generic;

namespace ZemberekDotNet.Core.Turkish.Hyphenation
{
    public interface ISyllableExtractor
    {
        /// <summary>
        /// returns a list of Strings representing syllables for a given input.
        /// </summary>
        /// <param name="input">input word.</param>
        /// <returns>list of syllables. if word cannot be parsed, an empty list is returned.</returns>
        List<string> GetSyllables(string input);
    }
}
