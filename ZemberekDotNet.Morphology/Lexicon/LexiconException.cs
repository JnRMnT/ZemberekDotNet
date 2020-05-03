using System;

namespace ZemberekDotNet.Morphology.Lexicon
{
    public class LexiconException : ApplicationException
    {
        public LexiconException(string message) : base(message)
        {
        }

        public LexiconException(string message, Exception cause) : base(message, cause)
        {

        }
    }
}
