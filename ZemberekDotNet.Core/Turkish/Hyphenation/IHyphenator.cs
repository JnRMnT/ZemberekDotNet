namespace ZemberekDotNet.Core.Turkish.Hyphenation
{
    /// <summary>
    /// Provides syllable related operations.
    /// </summary>
    public interface IHyphenator
    {
        /// <summary>
        /// 
        /// 
        /// Finds the splitting index of a word for a space constraint. if <code>spaceAvailable</code> is
        /// smaller than the length of the string, it will return word's length. if it is not possible to
        /// fit first syllable to the <code>spaceAvailable</code> it will return -1. <p>Example for
        /// Turkish: <p><code>("merhaba", 4) -> 3 ["mer-haba"]</code> <p><code>("merhaba", 6) -> 5
        /// ["merha-ba"]</code> <p><code>("merhaba", 2) -> -1 []</code> <p><code>("dddaddd", 2) -> -1
        /// []</code> <p><code>("merhaba", 8) -> 7 ["merhaba"]</code>
        /// </summary>
        /// <param name="input">input String.</param>
        /// <param name="spaceAvailable">the available space</param>
        /// <returns>an integer.</returns>
        int SplitIndex(string input, int spaceAvailable);
    }
}
