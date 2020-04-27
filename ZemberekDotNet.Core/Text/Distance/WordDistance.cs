namespace ZemberekDotNet.Core.Text.Distance
{
    /// <summary>
    /// This class is used for comparing single tokens
    /// </summary>
    public class WordDistance : IStringDistance
    {
        public int SourceSize(TokenSequence sourceSequence)
        {
            return sourceSequence.Size();
        }

        public double Distance(string token1, string token2)
        {
            if (token1.Equals(token2))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
