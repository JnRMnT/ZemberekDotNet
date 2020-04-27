using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Text.Distance
{
    /// <summary>
    ///  calculates distance between two strings.
    /// </summary>
    public interface IStringDistance
    {
        /// <summary>
        /// Distance between two strings.
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="target">target string.</param>
        /// <returns>distance.</returns>
        double Distance(String source, String target);

        /// <summary>
        /// This class is used when ratio of the distance to the input tokenSequence is needed to be
        /// calculated.
        /// </summary>
        /// <param name="sourceSequence">source sequence</param>
        /// <returns>size of the token sequence. This may be different than the actual size, because during
        /// the distance calculation size of the source and target may be changed. such as, if we calculate
        /// "hello there" and "hell there", and we use a letter edit distance, the input size is not the
        /// token size = 2 but the letter count = 11.</returns>
        int SourceSize(TokenSequence sourceSequence);
    }
}
