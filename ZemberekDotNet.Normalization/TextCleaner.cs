using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Normalization
{
    public class TextCleaner
    {
        public static List<string> CleanAndExtractSentences(List<string> input)
        {
            List<string> lines = input.Where(s => !s.StartsWith("<")).Select(e => TextUtil.NormalizeSpacesAndSoftHyphens(e)).ToList();

            return TurkishSentenceExtractor.Default.FromParagraphs(lines)
                .Where(s => !TextUtil.ContainsCombiningDiacritics(s))
                .ToList();
        }
    }
}
