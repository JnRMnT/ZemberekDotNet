using System.Collections.Generic;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Ambiguity
{
    public interface IAmbiguityResolver
    {
        SentenceAnalysis Disambiguate(string sentence, List<WordAnalysis> allAnalyses);
    }
}
