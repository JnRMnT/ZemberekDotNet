using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ZemberekDotNet.Morphology.Extended
{
    /// <summary>
    /// Loads a Turkish unigram word-frequency model from the bundled <c>tr_50k.txt</c> corpus
    /// (OPUS/hermitdave, MIT licence) or from a caller-supplied file / stream.
    /// </summary>
    /// <remarks>
    /// ZemberekDotNet addition — no equivalent in Java Zemberek.
    /// <para>
    /// The bundled corpus is embedded as an assembly resource and does not require any external file.
    /// Pass a custom <see cref="WordFrequencyModel"/> to
    /// <see cref="WordAnalysisExtensions.ExtdGetRankedAnalyses"/> to override it.
    /// </para>
    /// </remarks>
    public sealed class WordFrequencyModel
    {
        private const string EmbeddedResourceName =
            "ZemberekDotNet.Morphology.Extended.Resources.tr_50k.txt";

        private readonly Dictionary<string, double> normalizedScores;

        private WordFrequencyModel(Dictionary<string, double> normalizedScores)
        {
            this.normalizedScores = normalizedScores;
        }

        /// <summary>
        /// Creates a model from the bundled <c>tr_50k.txt</c> embedded resource.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public static WordFrequencyModel FromEmbeddedResource()
        {
            Assembly assembly = typeof(WordFrequencyModel).Assembly;
            using Stream stream = assembly.GetManifestResourceStream(EmbeddedResourceName)
                ?? throw new InvalidOperationException(
                    $"Embedded resource '{EmbeddedResourceName}' not found in assembly " +
                    $"'{assembly.FullName}'. Ensure the file is marked as EmbeddedResource in the .csproj.");
            return FromStream(stream);
        }

        /// <summary>
        /// Creates a model from a plain-text file with lines in the format <c>word frequency</c>.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public static WordFrequencyModel FromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path must not be null or empty.", nameof(path));
            }
            using FileStream fs = File.OpenRead(path);
            return FromStream(fs);
        }

        /// <summary>
        /// Creates a model from a stream delivering lines in the format <c>word frequency</c>.
        /// </summary>
        /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
        public static WordFrequencyModel FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Dictionary<string, long> rawFreqs = new Dictionary<string, long>(60000);
            long maxFreq = 1;

            using StreamReader reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                int space = line.IndexOf(' ');
                if (space <= 0 || space == line.Length - 1)
                {
                    continue;
                }
                string word = line.Substring(0, space);
                if (long.TryParse(line.Substring(space + 1), out long freq) && freq > 0)
                {
                    rawFreqs[word] = freq;
                    if (freq > maxFreq)
                    {
                        maxFreq = freq;
                    }
                }
            }

            double logMax = Math.Log(maxFreq + 1.0);
            Dictionary<string, double> normalized = new Dictionary<string, double>(rawFreqs.Count);
            foreach (KeyValuePair<string, long> kv in rawFreqs)
            {
                normalized[kv.Key] = Math.Log(kv.Value + 1.0) / logMax;
            }

            return new WordFrequencyModel(normalized);
        }

        /// <summary>
        /// Returns a normalised score in [0, 1] for <paramref name="word"/>.
        /// Unknown words return 0.
        /// </summary>
        internal double GetNormalizedScore(string word)
        {
            if (word == null)
            {
                return 0.0;
            }
            return normalizedScores.TryGetValue(word, out double score) ? score : 0.0;
        }
    }
}
