using System.Collections.Generic;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    public interface IStemTransitions
    {
        IEnumerable<StemTransition> GetTransitions();

        RootLexicon GetLexicon();

        List<StemTransition> GetPrefixMatches(string stem, bool asciiTolerant);

        List<StemTransition> GetTransitions(DictionaryItem item);

        void AddDictionaryItem(DictionaryItem item);

        void RemoveDictionaryItem(DictionaryItem item);

        List<StemTransition> Generate(DictionaryItem item);
    }
}
