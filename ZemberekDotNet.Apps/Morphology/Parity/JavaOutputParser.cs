using System;
using System.Collections.Generic;
using System.IO;

namespace ZemberekDotNet.Apps.Morphology.Parity
{
    /// <summary>
    /// Parses the Java analysis TSV file produced either manually or by <see cref="JavaProcessRunner"/>.
    ///
    /// Expected format (UTF-8, one word per line):
    ///   sentence_index &lt;TAB&gt; word &lt;TAB&gt; best_analysis &lt;TAB&gt; analysis_count
    ///
    /// Lines starting with '#' are treated as comments and ignored.
    /// Unknown words should use analysis_count=0 and best_analysis=?.
    ///
    /// Example:
    ///   0   Kitaplara   kitap:Noun+A3pl+Dat  1
    ///   0   gidiyorum   git:Verb+Prog1+A1sg  1
    ///   1   xyz         ?                    0
    /// </summary>
    public static class JavaOutputParser
    {
        /// <summary>
        /// Parses the TSV file and returns a dictionary keyed by sentence index,
        /// mapping to the ordered list of word analyses for that sentence.
        /// </summary>
        public static Dictionary<int, List<JavaWordAnalysis>> Parse(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Java output file not found: {filePath}");

            var result = new Dictionary<int, List<JavaWordAnalysis>>();
            int lineNumber = 0;

            foreach (string rawLine in File.ReadLines(filePath))
            {
                lineNumber++;
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                    continue;

                string[] parts = line.Split('\t');
                if (parts.Length < 4)
                {
                    Console.Error.WriteLine(
                        $"[JavaOutputParser] Line {lineNumber}: expected 4 tab-separated columns, got {parts.Length}. Skipping.");
                    continue;
                }

                if (!int.TryParse(parts[0], out int sentenceIndex))
                {
                    Console.Error.WriteLine(
                        $"[JavaOutputParser] Line {lineNumber}: cannot parse sentence index '{parts[0]}'. Skipping.");
                    continue;
                }

                if (!int.TryParse(parts[3], out int count))
                {
                    Console.Error.WriteLine(
                        $"[JavaOutputParser] Line {lineNumber}: cannot parse analysis count '{parts[3]}'. Skipping.");
                    continue;
                }

                var entry = new JavaWordAnalysis
                {
                    SentenceIndex = sentenceIndex,
                    Word = parts[1].TrimStart('\uFEFF'),
                    BestAnalysis = parts[2],
                    AnalysisCount = count
                };

                if (!result.TryGetValue(sentenceIndex, out List<JavaWordAnalysis> list))
                {
                    list = new List<JavaWordAnalysis>();
                    result[sentenceIndex] = list;
                }
                list.Add(entry);
            }

            return result;
        }
    }

    /// <summary>One word entry from the Java analysis output file.</summary>
    public sealed class JavaWordAnalysis
    {
        public int SentenceIndex { get; set; }
        public string Word { get; set; }
        public string BestAnalysis { get; set; }
        public int AnalysisCount { get; set; }
    }
}
