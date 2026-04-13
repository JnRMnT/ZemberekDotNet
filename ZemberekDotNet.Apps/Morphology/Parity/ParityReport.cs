using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZemberekDotNet.Apps.Morphology.Parity
{
    /// <summary>Root result of a ZemberekDotNet vs Java Zemberek parity run.</summary>
    public class ParityReport
    {
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public string InputFile { get; set; }
        public int TotalSentences { get; set; }
        public int TotalWords { get; set; }
        public int MatchingWords { get; set; }
        public int MismatchingWords { get; set; }

        [JsonIgnore]
        public double MatchRate => TotalWords == 0 ? 0d : (double)MatchingWords / TotalWords;

        public double MatchRatePct => Math.Round(MatchRate * 100, 2);

        /// <summary>Counts per <see cref="MismatchCategory"/> string key.</summary>
        public Dictionary<string, int> MismatchCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>Words where both Java and .NET returned unknown — counted as matches.</summary>
        public int BothUnknownWords { get; set; }

        /// <summary>Per-sentence detail. May be empty when only a summary is requested.</summary>
        public List<SentenceParity> Sentences { get; set; } = new List<SentenceParity>();
    }

    public class SentenceParity
    {
        public int Index { get; set; }
        public string Sentence { get; set; }
        public bool FullMatch { get; set; }
        public List<WordParity> Words { get; set; } = new List<WordParity>();
    }

    public class WordParity
    {
        public string Word { get; set; }
        public bool Match { get; set; }

        /// <summary>Null when <see cref="Match"/> is true.</summary>
        public string Category { get; set; }

        public string JavaBest { get; set; }
        public string DotNetBest { get; set; }
        public int JavaCount { get; set; }
        public int DotNetCount { get; set; }
    }

    /// <summary>Mismatch category string constants used as dictionary keys in the report.</summary>
    public static class MismatchCategory
    {
        /// <summary>Sentence tokenized into a different number of words on each side.</summary>
        public const string TokenizationDiff = "TokenizationDiff";

        /// <summary>Word is unknown (zero analyses) on one side but recognized on the other.</summary>
        public const string LexiconGap = "LexiconGap";

        /// <summary>The disambiguated best analysis string differs.</summary>
        public const string BestAnalysisDiff = "BestAnalysisDiff";

        /// <summary>Best analysis matches but the total candidate count differs.</summary>
        public const string AnalysisCountDiff = "AnalysisCountDiff";

        /// <summary>Both sides return zero analyses (both unknown) — counted separately.</summary>
        public const string BothUnknown = "BothUnknown";
    }
}
