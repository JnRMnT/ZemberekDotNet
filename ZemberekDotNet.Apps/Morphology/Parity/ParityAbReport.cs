using System;
using System.Collections.Generic;

namespace ZemberekDotNet.Apps.Morphology.Parity
{
    /// <summary>
    /// Summary report for baseline vs trained ambiguity-resolver parity comparison.
    /// </summary>
    public class ParityAbReport
    {
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("o");
        public string InputFile { get; set; }
        public string JavaOutputFile { get; set; }
        public int TotalSentences { get; set; }
        public int IterationCount { get; set; }
        public int TrainSentences { get; set; }
        public int DevSentences { get; set; }
        public int SkippedSentences { get; set; }
        public ParitySummary Baseline { get; set; }
        public ParitySummary Trained { get; set; }

        public double MatchRateDeltaPct =>
            Math.Round((Trained?.MatchRatePct ?? 0d) - (Baseline?.MatchRatePct ?? 0d), 2);
    }

    public class ParitySummary
    {
        public int Words { get; set; }
        public int Match { get; set; }
        public int Mismatch { get; set; }
        public double MatchRatePct { get; set; }
        public Dictionary<string, int> MismatchCounts { get; set; } = new Dictionary<string, int>();

        public static ParitySummary FromParityReport(ParityReport report)
        {
            return new ParitySummary
            {
                Words = report.TotalWords,
                Match = report.MatchingWords,
                Mismatch = report.MismatchingWords,
                MatchRatePct = report.MatchRatePct,
                MismatchCounts = new Dictionary<string, int>(report.MismatchCounts)
            };
        }
    }
}