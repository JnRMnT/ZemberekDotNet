using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Native.Helpers;
using ZemberekDotNet.Core.Native.Text;
using ZemberekDotNet.Core.Quantization;

namespace ZemberekDotNet.LM.Compression
{
    /// <summary>
    /// This is a multiple file representation of an uncompressed backoff language model.Files are:
    /// <p/>info<p/> int32 order<p/> int32 1 gram count <p/>int32 2 gram count <p/>... <p/>[n].gram
    /// <p/>int32 order <p/>int32 count <p/>int32... id[0, .., n] <p> <p/>[n].prob<p/> int32 count
    /// <p/>float32 prob <p/>.... <p> <p/>[n].backoff<p/> int32 count<p/> float32 prob<p/>.... <p>
    /// <p/>vocab<p/> int32 count<p/> UTF-8... word0...n
    /// </summary>
    public class MultiFileUncompressedLm
    {
        public static string InfoFileName = "info";
        public static string GramIdsFileSuffix = ".gram";
        public static string ProbFileSuffix = ".prob";
        public static string BackoffFileSuffix = ".backoff";
        public static string VocabFileName = "vocab";

        internal int[] counts;
        internal int order;
        public string dir;

        public MultiFileUncompressedLm(string dir)
        {
            this.dir = dir;
            using (FileStream fileStream = File.OpenRead(GetFile(InfoFileName)))
            {
                using (BinaryReader dis = new BinaryReader(fileStream))
                {
                    order = dis.ReadInt32().EnsureEndianness();
                    counts = new int[order + 1];
                    for (int i = 0; i < order; i++)
                    {
                        counts[i + 1] = dis.ReadInt32().EnsureEndianness();
                    }
                }
            }
        }

        public static MultiFileUncompressedLm Generate(
            string arpaFile,
            string dir,
            string encoding,
            int fractionDigits)
        {
            if (File.Exists(dir) && !Directory.Exists(dir))
            {
                throw new ArgumentException(dir + " is not a directory!");
            }
            else
            {
                Directory.CreateDirectory(dir);
            }

            long elapsedTime = FileHelper.ReadAllLines(arpaFile, Encoding.GetEncoding(encoding),
                new ArpaToBinaryConverter(dir, fractionDigits));

            Log.Info("Multi file uncompressed binary model is generated in " + (double)elapsedTime / 1000d
                + " seconds");
            if (!File.Exists(Path.Combine(dir, InfoFileName)))
            {
                throw new InvalidOperationException("Arpa file " + arpaFile
                    + " cannot be parsed. Possibly incorrectly formatted or empty file is supplied.");
            }
            return new MultiFileUncompressedLm(dir);
        }

        public string GetLmDir()
        {
            return dir;
        }

        public int GetRankSize(FileStream f)
        {
            BinaryReader dis = new BinaryReader(f);
            int count = dis.ReadInt32().EnsureEndianness();
            dis.Close();
            return count;
        }

        public int GetRankBlockSize(FileStream f)
        {
            BinaryReader dis = new BinaryReader(f);
            dis.ReadInt32().EnsureEndianness();
            int blockSize = dis.ReadInt32().EnsureEndianness();
            dis.Close();
            return blockSize;
        }

        public string GetGramFile(int n)
        {
            return Path.Combine(dir, n + GramIdsFileSuffix);
        }

        public string GetProbFile(int n)
        {
            return Path.Combine(dir, n + ProbFileSuffix);
        }

        public string GetProbRankFile(int n)
        {
            return Path.Combine(dir, n + ProbFileSuffix + ".rank");
        }

        public string GetBackoffRankFile(int n)
        {
            return Path.Combine(dir, n + BackoffFileSuffix + ".rank");
        }

        public string GetBackoffFile(int n)
        {
            return Path.Combine(dir, n + BackoffFileSuffix);
        }

        public string GetProbabilityLookupFile(int n)
        {
            return Path.Combine(dir, Path.GetFileName(GetProbFile(n)) + ".lookup");
        }

        public string GetBackoffLookupFile(int n)
        {
            return Path.Combine(dir, Path.GetFileName(GetBackoffFile(n)) + ".lookup");
        }

        public int GetOrder()
        {
            return order;
        }

        public int GetCount(int n)
        {
            return counts[n];
        }

        private string GetFile(string name)
        {
            return Path.Combine(dir, name);
        }

        public void GenerateRankFiles(int i, int bit, QuantizerType quantizerType)
        {
            if (bit > 24)
            {
                throw new ArgumentException(
                    "Cannot generate rank file larger than 24 bits but it is:" + bit);
            }
            Log.Info("Calculating probabilty rank values for :" + i + " Grams");
            string probFile = GetProbFile(i);
            GenerateRankFile(bit, i, probFile, Path.Combine(dir, i + ProbFileSuffix + ".rank"),
                quantizerType);
            if (i < counts.Length - 1)
            {
                string backoffFile = GetBackoffFile(i);
                Log.Info("Calculating back-off rank values for :" + i + " Grams");
                GenerateRankFile(bit, i, backoffFile, Path.Combine(dir, i + BackoffFileSuffix + ".rank"),
                    quantizerType);
            }
        }

        public void GenerateRankFiles(int bit, QuantizerType quantizerType)
        {
            if (bit > 24)
            {
                throw new ArgumentException(
                    "Cannot generate rank file larger than 24 bits but it is:" + bit);
            }
            for (int i = 1; i < counts.Length; i++)
            {
                Log.Info("Calculating probabilty lookup values for :" + i + " Grams");
                string probFile = GetProbFile(i);
                GenerateRankFile(bit, i, probFile, Path.Combine(dir, i + ProbFileSuffix + ".rank"),
                    quantizerType);
                if (i < counts.Length - 1)
                {
                    string backoffFile = GetBackoffFile(i);
                    Log.Info("Calculating lookup values for " + i + " Grams");
                    GenerateRankFile(bit, i, backoffFile, Path.Combine(dir, i + BackoffFileSuffix + ".rank"),
                        quantizerType);
                }
            }
        }

        private void GenerateRankFile(int bit, int currentOrder, string probFile, string rankFile,
            QuantizerType quantizerType)
        {
            using (FileStream probFileStream = File.OpenRead(probFile))
            {
                using (BinaryReader dis = new BinaryReader(probFileStream))
                {
                    using (FileStream rankFileStream = new FileStream(rankFile, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter dos = new BinaryWriter(rankFileStream))
                        {
                            int count = dis.ReadInt32().EnsureEndianness();
                            IQuantizer quantizer = BinaryFloatFileReader.GetQuantizer(probFile, bit, quantizerType);
                            dos.Write(count.EnsureEndianness());
                            Log.Info("Writing Rank file for " + currentOrder + " grams");
                            int bytecount = (bit % 8 == 0 ? bit / 8 : bit / 8 + 1);
                            if (bytecount == 0)
                            {
                                bytecount = 1;
                            }
                            dos.Write(bytecount.EnsureEndianness());
                            byte[] bytez = new byte[3];
                            for (int j = 0; j < count; j++)
                            {
                                int rank = quantizer.GetQuantizationIndex(dis.ReadSingle().EnsureEndianness());
                                switch (bytecount)
                                {
                                    case 1:
                                        dos.Write((rank & 0xff).EnsureEndianness());
                                        break;
                                    case 2:
                                        dos.Write(((short)(rank & 0xffff)).EnsureEndianness());
                                        break;
                                    case 3:
                                        bytez[0] = (byte)((rank.EnsureEndianness() >> 16) & 0xff);
                                        bytez[1] = (byte)((rank.EnsureEndianness() >> 8) & 0xff);
                                        bytez[2] = (byte)(rank.EnsureEndianness() & 0xff);
                                        dos.Write(bytez);
                                        break;
                                }
                            }

                            DoubleLookup lookup = quantizer.GetDequantizer();
                            Log.Info("Writing lookups for " + currentOrder + " grams. Size= " + lookup.GetRange());
                            lookup.Save(Path.Combine(dir, Path.GetFileName(probFile) + ".lookup"));
                        }
                    }
                }
            }
        }

        public string GetVocabularyFile()
        {
            return Path.Combine(dir, VocabFileName);
        }

        private class ArpaToBinaryConverter : ILineProcessor<long>
        {
            public static readonly int DefaultUnknownProbability = -20;
            int ngramCounter = 0;
            int _n;

            double fractionMultiplier;
            State state = State.Begin;
            List<int> ngramCounts = new List<int>();
            bool started = false;
            string dir;
            BinaryWriter gramOs;
            BinaryWriter probOs;
            BinaryWriter backoOffs;
            int order;
            long start;
            SpaceTabTokenizer tokenizer = new SpaceTabTokenizer();
            LmVocabulary.LmVocabularyBuilder vocabularyBuilder = new LmVocabulary.LmVocabularyBuilder();
            // This will be generated after reading unigrams.
            LmVocabulary lmVocabulary;
            internal ArpaToBinaryConverter(string dir, int fractionDigitCount)

            {
                Log.Info("Generating multi file uncompressed language model from Arpa file in directory: {0}",
                      Path.GetFullPath(dir));
                this.dir = dir;
                if (fractionDigitCount >= 0)
                {
                    fractionMultiplier = Math.Pow(10, fractionDigitCount);
                }
                else
                {
                    fractionMultiplier = 0;
                }
                start = DateTime.Now.Ticks;
            }

            private float ReduceFraction(float input)
            {
                if (fractionMultiplier != 0)
                {
                    return (float)(Math.Round(input * fractionMultiplier) / fractionMultiplier);
                }
                else
                {
                    return input;
                }
            }

            private void NewGramStream(int n)
            {
                if (gramOs != null)
                {
                    gramOs.Close();
                }
                gramOs = GetDos(n + GramIdsFileSuffix);
                gramOs.Write(n.EnsureEndianness());
                gramOs.Write(ngramCounts[n - 1].EnsureEndianness());
            }

            private void NewProbStream(int n)
            {
                if (probOs != null)
                {
                    probOs.Close();
                }
                probOs = GetDos(n + ProbFileSuffix);
                probOs.Write(ngramCounts[n - 1].EnsureEndianness());
            }

            private void NewBackoffStream(int n)
            {
                if (backoOffs != null)
                {
                    backoOffs.Close();
                }
                backoOffs = GetDos(n + BackoffFileSuffix);
                backoOffs.Write(ngramCounts[n - 1].EnsureEndianness());
            }

            public bool ProcessLine(String s)
            {
                string clean = s.Trim();
                switch (state)
                {
                    // read n value and ngram counts.
                    case State.Begin:
                        if (clean.StartsWith("\\data\\"))
                        {
                            started = true;
                        }
                        else if (started && clean.StartsWith("ngram"))
                        {
                            started = true;
                            int count = 0, i = 0;
                            foreach (string str in clean.Split("=").Select(e => e.Trim()))
                            {
                                if (i++ == 0)
                                {
                                    continue;
                                }
                                count = int.Parse(str);
                            }
                            ngramCounts.Add(count);
                        }
                        else if (started)
                        {
                            state = State.Unigrams;
                            NewGramStream(1);
                            NewProbStream(1);
                            NewBackoffStream(1);
                            Log.Info("Gram counts in Arpa file: " + string.Join(" ", ngramCounts));
                            Log.Info("Writing unigrams.");
                            _n++;
                        }
                        break;

                    // read ngrams. if unigram values, we store the strings and related indexes.
                    case State.Unigrams:
                        if (clean.Length == 0 || clean.StartsWith("\\"))
                        {
                            break;
                        }
                        string[] tokens = tokenizer.Split(clean);
                        // parse probabilty
                        float logProbability = float.Parse(tokens[0], CultureInfo.InvariantCulture);

                        String word = tokens[1];
                        float logBackoff = 0;
                        if (tokens.Length == 3)
                        {
                            logBackoff = float.Parse(tokens[_n + 1], CultureInfo.InvariantCulture);
                        }
                        // write unigram id, log-probability and log-backoff value.
                        int wordIndex = vocabularyBuilder.Add(word);
                        gramOs.Write(wordIndex.EnsureEndianness());
                        probOs.Write(ReduceFraction(logProbability).EnsureEndianness());

                        // if there are only ngrams, do not write backoff value.
                        if (ngramCounts.Count > 1)
                        {
                            backoOffs.Write(ReduceFraction(logBackoff).EnsureEndianness());
                        }

                        ngramCounter++;
                        if (ngramCounter == ngramCounts[0])
                        {
                            HandleSpecialToken("<unk>");
                            HandleSpecialToken("</s>");
                            HandleSpecialToken("<s>");
                            lmVocabulary = vocabularyBuilder.Generate();

                            ngramCounts[0] = lmVocabulary.Size();

                            // we write info file after reading unigrams because we may add special tokens to unigrams
                            // so count information may have been changed.
                            order = ngramCounts.Count;
                            using (BinaryWriter infos = GetDos(InfoFileName))
                            {
                                infos.Write(order.EnsureEndianness());
                                foreach (int ngramCount in ngramCounts)
                                {
                                    infos.Write(ngramCount.EnsureEndianness());
                                }
                            }

                            ngramCounter = 0;
                            state = State.NGRams;
                            _n++;
                            // if there is only unigrams in the arpa file, exit
                            if (ngramCounts.Count == 1)
                            {
                                state = State.Vocabulary;
                            }
                            else
                            {
                                NewGramStream(2);
                                NewProbStream(2);
                                if (order > 2)
                                {
                                    NewBackoffStream(2);
                                }
                                Log.Info("Writing 2-grams.");
                            }
                        }
                        break;

                    case State.NGRams:
                        if (clean.Length == 0 || clean.StartsWith("\\"))
                        {
                            break;
                        }
                        tokens = tokenizer.Split(clean);
                        logProbability = float.Parse(tokens[0], CultureInfo.InvariantCulture);

                        for (int i = 0; i < _n; i++)
                        {
                            int id = lmVocabulary.IndexOf(tokens[i + 1]);
                            gramOs.Write(id.EnsureEndianness());
                        }

                        // probabilities
                        probOs.Write(ReduceFraction(logProbability).EnsureEndianness());
                        if (_n < ngramCounts.Count)
                        {
                            logBackoff = 0;
                            if (tokens.Length == _n + 2)
                            {
                                logBackoff = float.Parse(tokens[_n + 1], CultureInfo.InvariantCulture);
                            }
                            backoOffs.Write(ReduceFraction(logBackoff).EnsureEndianness());
                        }

                        if (ngramCounter > 0 && ngramCounter % 1000000 == 0)
                        {
                            Log.Info(ngramCounter + " grams are written so far.");
                        }

                        ngramCounter++;
                        if (ngramCounter == ngramCounts[_n - 1])
                        {
                            ngramCounter = 0;
                            // if there is no more ngrams, exit
                            if (ngramCounts.Count == _n)
                            {
                                state = State.Vocabulary;
                            }
                            else
                            {
                                _n++;
                                NewGramStream(_n);
                                NewProbStream(_n);
                                if (order > _n)
                                {
                                    NewBackoffStream(_n);
                                }
                                Log.Info("Writing " + _n + "-grams.");
                            }
                        }
                        break;

                    case State.Vocabulary:
                        Closeables.Close(gramOs, true);
                        Closeables.Close(probOs, true);
                        Closeables.Close(backoOffs, true);
                        Log.Info("Writing model vocabulary.");
                        lmVocabulary.SaveBinary(Path.Combine(dir, VocabFileName));
                        return false; // we are done.
                }
                return true;
            }

            // adds undefined specials token with default probability.
            private void HandleSpecialToken(String word)
            {
                if (vocabularyBuilder.IndexOf(word) == -1
                    && vocabularyBuilder.IndexOf(word.ToUpperInvariant()) == -1)
                {
                    Log.Warn("Special token " + word +
                        " does not exist in model. It is added with default [unknown word] probability: " +
                        DefaultUnknownProbability);
                    int index = vocabularyBuilder.Add(word);
                    gramOs.Write(index.EnsureEndianness());
                    probOs.Write(DefaultUnknownProbability.EnsureEndianness());
                    if (ngramCounts.Count > 1)
                    {
                        backoOffs.Write(((float)0).EnsureEndianness());
                    }
                }
            }

            private BinaryWriter GetDos(string name)
            {
                return new BinaryWriter(new FileStream(Path.Combine(dir, name), FileMode.Create, FileAccess.Write, FileShare.Write, 100000));
            }

            public long GetResult()
            {
                // just return the time..
                return DateTime.Now.Ticks - start;
            }

            enum State
            {
                Begin, Unigrams, NGRams, Vocabulary
            }
        }
    }
}
