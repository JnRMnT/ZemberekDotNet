using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native.Collections;

namespace ZemberekDotNet.LM
{
    public class LmVocabulary
    {
        public static readonly string DefaultSentenceBeginMarker = "<s>";
        public static readonly string DefaultSentenceEndMarker = "</s>";
        public static readonly string DefaultUnknownWord = "<unk>";
        private string unknownWord;
        private string sentenceStart;
        private string sentenceEnd;
        private ReadOnlyCollection<string> vocabulary;
        private Dictionary<string, int> vocabularyIndexMap = new Dictionary<string, int>();

        private int unknownWordIndex = -1;
        private int sentenceStartIndex = -1;
        private int sentenceEndIndex = -1;

        /// <summary>
        /// Generates a vocabulary with given String array.
        /// </summary>
        /// <param name="vocabulary">word array.</param>
        public LmVocabulary(params string[] vocabulary)
        {
            GenerateMap(new List<string>(vocabulary));
        }

        /// <summary>
        /// Generates a vocabulary with given String List.
        /// </summary>
        /// <param name="vocabulary">word list.</param>
        public LmVocabulary(List<string> vocabulary)
        {
            GenerateMap(vocabulary);
        }

        public static LmVocabularyBuilder Builder()
        {
            return new LmVocabularyBuilder();
        }

        /// <summary>
        /// Generates a vocabulary from a binary RandomAccessFile. first integer read from the file pointer
        /// defines the vocabulary size. Rest is read in UTF. Constructor does not close the
        /// RandomAccessFile
        /// </summary>
        /// <param name="fileStream">binary vocabulary RandomAccessFile</param>
        private LmVocabulary(FileStream fileStream)
        {
            using (BinaryReader raf = new BinaryReader(fileStream))
            {
                int vocabLength = raf.ReadInt32().EnsureEndianness();
                List<string> vocabulary = new List<string>(vocabLength);
                for (int i = 0; i < vocabLength; i++)
                {
                    vocabulary.Add(raf.ReadUTF());
                }
                GenerateMap(vocabulary);
            }
        }

        /// <summary>
        /// Generates a vocabulary from a binary BinaryReader. first integer read from the file pointer
        /// defines the vocabulary size. Rest is read in UTF. Constructor does not close the
        /// BinaryReader
        /// </summary>
        /// <param name="dis">input stream to read the vocabulary data.</param>
        private LmVocabulary(BinaryReader dis)
        {
            LoadVocabulary(dis);
        }

        /// <summary>
        /// Generates a vocabulary from a binary vocabulary File. First integer in the file defines the
        /// vocabulary size.Rest is read in UTF.
        /// </summary>
        /// <param name="binaryVocabularyFile"></param>
        /// <returns></returns>
        public static LmVocabulary LoadFromBinary(FileStream binaryVocabularyFile)
        {
            using (BinaryReader dis = new BinaryReader(binaryVocabularyFile))
            {
                return new LmVocabulary(dis);
            }
        }

        /// <summary>
        /// Generates a vocabulary from a binary vocabulary File. First integer in the file defines the
        /// vocabulary size. Rest is read in UTF.
        /// </summary>
        /// <param name="binaryVocabularyFilePath"></param>
        /// <returns></returns>
        public static LmVocabulary LoadFromBinary(string binaryVocabularyFilePath)
        {
            using (FileStream fileStream = File.OpenRead(binaryVocabularyFilePath))
            {
                return LoadFromBinary(fileStream);
            }
        }

        /// <summary>
        /// Generates a vocabulary from a binary RandomAccessFile. first integer read from the file pointer
        /// defines the vocabulary size. Rest is read in UTF. The RandomAccessFile will not be closed by
        /// this method.
        /// </summary>
        /// <param name="raf">binary vocabulary RandomAccessFile</param>
        /// <returns></returns>
        public static LmVocabulary LoadFromRandomAccessFile(FileStream raf)
        {
            return new LmVocabulary(raf);
        }

        /// <summary>
        /// Generates a vocabulary from a binary BinaryReader. first integer read from the file pointer
        /// defines the vocabulary size. Rest is read in UTF. This method does not close the
        /// BinaryReader
        /// </summary>
        /// <param name="dis">input stream to read the vocabulary data.</param>
        /// <returns></returns>
        public static LmVocabulary LoadFromBinaryReader(BinaryReader dis)
        {
            return new LmVocabulary(dis);
        }

        /// <summary>
        /// Generates a vocabulary from a UTF8-encoded text file.
        /// </summary>
        /// <param name="utfVocabularyFile">input utf8 file to read vocabulary data.</param>
        /// <returns>LMVocabulary instance.</returns>
        public static LmVocabulary LoadFromUtf8File(string utfVocabularyFile)
        {
            return new LmVocabulary(SimpleTextReader.TrimmingUTF8Reader(utfVocabularyFile).AsStringList());
        }

        /// <summary>
        /// Generates a new LmVocabulary instance that contains words that exist in both `v1` and `v2`
        /// There is no guarantee that new vocabulary indexes will match with v1 or v2.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static LmVocabulary Intersect(LmVocabulary v1, LmVocabulary v2)
        {
            HashSet<string> ls = new HashSet<string>(v1.vocabulary);
            List<string> intersection = new List<string>(Math.Min(v1.Size(), v2.Size()));
            intersection.AddRange(new HashSet<string>(v2.vocabulary).Where(e => ls.Contains(e)));
            return new LmVocabulary(intersection);
        }

        private void LoadVocabulary(BinaryReader dis)
        {
            int vocabularyLength = dis.ReadInt32().EnsureEndianness();
            List<string> vocabulary = new List<string>(vocabularyLength);
            for (int i = 0; i < vocabularyLength; i++)
            {
                vocabulary.Add(dis.ReadUTF());
            }
            GenerateMap(vocabulary);
        }

        /// <summary>
        /// Binary serialization of the vocabulary.
        /// </summary>
        /// <param name="file">file to serialize.</param>
        public void SaveBinary(string file)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter dos = new BinaryWriter(fileStream))
                {
                    SaveBinary(dos);
                }
            }
        }

        /// <summary>
        ///  Binary serialization of the vocabulary.
        /// </summary>
        /// <param name="dos">output stream to serialize</param>
        public void SaveBinary(BinaryWriter dos)
        {
            dos.Write(vocabulary.Count.EnsureEndianness());
            foreach (string s in vocabulary)
            {
                dos.WriteUTF(s);
            }
        }

        private void GenerateMap(List<string> inputVocabulary)
        {
            // construct vocabulary index lookup.
            int indexCounter = 0;
            List<string> cleanVocab = new List<string>();
            foreach (string word in inputVocabulary)
            {
                if (vocabularyIndexMap.ContainsKey(word))
                {
                    Log.Warn("Language model vocabulary has duplicate item: " + word);
                    continue;
                }
                if (word.Equals(DefaultUnknownWord, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (unknownWordIndex != -1)
                    {
                        Log.Warn(
                            "Unknown word was already defined as %s but another matching token exist in the input vocabulary: %s",
                            unknownWord, word);
                    }
                    else
                    {
                        unknownWord = word;
                        unknownWordIndex = indexCounter;
                    }
                }
                else if (word.Equals(DefaultSentenceBeginMarker, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (sentenceStartIndex != -1)
                    {
                        Log.Warn(
                            "Sentence start index was already defined as %s but another matching token exist in the input vocabulary: %s",
                            sentenceStart, word);
                    }
                    else
                    {
                        sentenceStart = word;
                        sentenceStartIndex = indexCounter;
                    }
                }
                else if (word.Equals(DefaultSentenceEndMarker, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (sentenceEndIndex != -1)
                    {
                        Log.Warn(
                            "Sentence end index was already defined as %s but another matching token exist in the input vocabulary: %s",
                            sentenceEnd, word);
                    }
                    else
                    {
                        sentenceEnd = word;
                        sentenceEndIndex = indexCounter;
                    }
                }
                vocabularyIndexMap.Add(word, indexCounter);
                cleanVocab.Add(word);
                indexCounter++;
            }
            if (unknownWordIndex == -1)
            {
                unknownWord = DefaultUnknownWord;
                cleanVocab.Add(unknownWord);
                vocabularyIndexMap.Add(unknownWord, indexCounter++);
                Log.Debug("Necessary special token " + unknownWord
                    + " was not found in the vocabulary, it is added explicitly");
            }
            unknownWordIndex = vocabularyIndexMap.GetValueOrDefault(unknownWord);
            if (sentenceStartIndex == -1)
            {
                sentenceStart = DefaultSentenceBeginMarker;
                cleanVocab.Add(sentenceStart);
                vocabularyIndexMap.Add(sentenceStart, indexCounter++);
                Log.Debug("Vocabulary does not contain sentence start token, it is added explicitly.");
            }
            sentenceStartIndex = vocabularyIndexMap.GetValueOrDefault(sentenceStart);
            if (sentenceEndIndex == -1)
            {
                sentenceEnd = DefaultSentenceEndMarker;
                cleanVocab.Add(sentenceEnd);
                vocabularyIndexMap.Add(sentenceEnd, indexCounter);
                Log.Debug("Vocabulary does not contain sentence end token, it is added explicitly.");
            }
            sentenceEndIndex = vocabularyIndexMap.GetValueOrDefault(sentenceEnd);
            vocabulary = cleanVocab.AsReadOnly();
        }

        public int Size()
        {
            // Because we may have duplicate items in word list but not in map, we return the size of the list.
            return vocabulary.Count;
        }

        public bool ContainsUnknown(params int[] gramIds)
        {
            foreach (int gramId in gramIds)
            {
                if (gramId < 0 || gramId >= vocabulary.Count || gramId == unknownWordIndex)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// returns true if any word in vocabulary starts with `_` or `-`
        /// </summary>
        /// <returns></returns>
        public bool ContainsSuffix()
        {
            foreach (string s in vocabulary)
            {
                if (s.StartsWith("_") || s.StartsWith("-"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// returns true if any word in vocabulary ends with `_` or `-`
        /// </summary>
        /// <returns></returns>
        public bool ContainsPrefix()
        {
            foreach (string s in vocabulary)
            {
                if (s.EndsWith("_") || s.EndsWith("-"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">Word for the index. if index is out of bounds, <UNK> is returned with warning.
        /// Note that Vocabulary may contain <UNK> token as well.</param>
        /// <returns></returns>
        public string GetWord(int index)
        {
            if (index < 0 || index >= vocabulary.Count)
            {
                Log.Warn("Out of bounds word index is used:" + index);
                return unknownWord;
            }
            return vocabulary[index];
        }

        public int IndexOf(string word)
        {
            if (vocabularyIndexMap.ContainsKey(word))
            {
                return vocabularyIndexMap.GetValueOrDefault(word);
            }
            else
            {
                return unknownWordIndex;
            }
        }

        public int GetSentenceStartIndex()
        {
            return sentenceStartIndex;
        }

        public int GetSentenceEndIndex()
        {
            return sentenceEndIndex;
        }

        public int GetUnknownWordIndex()
        {
            return unknownWordIndex;
        }

        public string GetUnknownWord()
        {
            return unknownWord;
        }

        public string GetSentenceStart()
        {
            return sentenceStart;
        }

        public string GetSentenceEnd()
        {
            return sentenceEnd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>indexes of words when the words are alphabetically sorted according to the input
        /// locale.</returns>
        public IEnumerable<int> AlphabeticallySortedWordsIds(CultureInfo locale)
        {
            return new TreeMap<string, int>(locale.CompareInfo.GetStringComparer(CompareOptions.None)).Values();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>indexes of words when the words are alphabetically sorted according to the default
        /// locale.</returns>
        public IEnumerable<int> AlphabeticallySortedWordsIds()
        {
            return new TreeMap<string, int>(vocabularyIndexMap).Values();
        }

        public IEnumerable<string> Words()
        {
            return this.vocabulary;
        }

        public IEnumerable<string> WordsSorted()
        {
            List<string> sorted = new List<string>(vocabulary);
            sorted.Sort();
            return sorted;
        }

        public IEnumerable<string> WordsSorted(CultureInfo locale)
        {
            List<string> sorted = new List<string>(vocabulary);
            sorted.Sort(locale.CompareInfo.Compare);
            return sorted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexes">word indexes</param>
        /// <returns>the Word representation of indexes. Such as for 2,3,4 it returns "foo bar zipf" For
        /// unknown indexes it uses <unk>.</returns>
        public string GetWordsString(params int[] indexes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (Contains(index))
                {
                    sb.Append(vocabulary[index]);
                }
                else
                {
                    Log.Warn("Out of bounds word index is used:" + index);
                    sb.Append(unknownWord);
                }
                if (i < indexes.Length - 1)
                {
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexes">index array</param>
        /// <returns>true if all indexes are within the Vocabulary boundaries.</returns>
        public bool ContainsAll(params int[] indexes)
        {
            foreach (int index in indexes)
            {
                if (!Contains(index))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">word index</param>
        /// <returns>true if index is within the Vocabulary boundaries.</returns>
        public bool Contains(int index)
        {
            return index >= 0 && index < vocabulary.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="words">Words</param>
        /// <returns> true if vocabulary contains all words.</returns>
        public bool ContainsAll(params string[] words)
        {
            foreach (string word in words)
            {
                if (!Contains(word))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word">Word</param>
        /// <returns>if vocabulary contains the word. For special tokens, it always return true.</returns>
        public bool Contains(string word)
        {
            return vocabularyIndexMap.ContainsKey(word);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0">W0 Index</param>
        /// <param name="g1">W1 Index</param>
        /// <param name="g2">W2 Index</param>
        /// <returns>The encoded long representation of a trigram. Structure: [1 bit EMPTY][21 bit
        /// W2-IND][21 bit W1-IND][21 bit W0-IND]</returns>
        public long EncodeTrigram(int g0, int g1, int g2)
        {
            long encoded = g2;
            encoded = (encoded << 21) | g1;
            return (encoded << 21) | g0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="triGram">trigram Indexes.</param>
        /// <returns>the encoded long representation of a trigram. Structure: [1 bit EMPTY][21 bit
        /// W2-IND][21 bit W1-IND][21 bit W0-IND]</returns>
        public long EncodeTrigram(params int[] triGram)
        {
            if (triGram.Length > 3)
            {
                throw new ArgumentException(
                    "Cannot generate long from order " + triGram.Length + " grams ");
            }
            long encoded = triGram[2];
            encoded = (encoded << 21) | triGram[1];
            return (encoded << 21) | triGram[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="words">word array</param>
        /// <returns>the vocabulary index array for a word array. if a word is unknown, index of <UNK> is
        /// used is returned as its vocabulary index. This value can be -1.</returns>
        public int[] ToIndexes(params string[] words)
        {
            int[] indexes = new int[words.Length];
            int i = 0;
            foreach (string word in words)
            {
                if (!vocabularyIndexMap.ContainsKey(word))
                {
                    indexes[i] = unknownWordIndex;
                }
                else
                {
                    indexes[i] = vocabularyIndexMap.GetValueOrDefault(word);
                }
                i++;
            }
            return indexes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="history"></param>
        /// <param name="word"></param>
        /// <returns>the vocabulary index array for a word array. if a word is unknown, index of <UNK> is
        /// used is returned as its vocabulary index. This value can be -1.</returns>
        public int[] ToIndexes(String[] history, String word)
        {
            int[] indexes = new int[history.Length + 1];
            for (int j = 0; j <= history.Length; j++)
            {
                String s = j < history.Length ? history[j] : word;
                if (!vocabularyIndexMap.ContainsKey(s))
                {
                    indexes[j] = unknownWordIndex;
                }
                else
                {
                    indexes[j] = vocabularyIndexMap.GetValueOrDefault(s);
                }
            }
            return indexes;
        }

        /// <summary>
        /// word index history and current word.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="word"></param>
        /// <returns>the vocabulary index array for a word array. if a word is unknown, index of <UNK> is
        /// used is returned as its vocabulary index. This value can be -1.</returns>
        public int[] ToIndexes(int[] history, String word)
        {
            int[] indexes = new int[history.Length + 1];
            for (int j = 0; j <= history.Length; j++)
            {
                int index = j < history.Length ? history[j] : IndexOf(word);
                if (!Contains(index))
                {
                    indexes[j] = unknownWordIndex;
                }
                else
                {
                    indexes[j] = index;
                }
            }
            return indexes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexes">word indexes.</param>
        /// <returns> Words representations of the indexes. If an index is out of bounds, <UNK>
        /// representation is used. Note that Vocabulary may contain an <UNK> word in it.</returns>
        public string[] ToWords(params int[] indexes)
        {
            string[] words = new string[indexes.Length];
            int k = 0;
            foreach (int index in indexes)
            {
                if (Contains(index))
                {
                    words[k++] = vocabulary[index];
                }
                else
                {
                    Log.Warn("Out of bounds word index is used:" + index);
                    words[k++] = unknownWord;
                }
            }
            return words;
        }

        /// <summary>
        /// This class acts like a mutable vocabulary. It can be used for dynamically generating an
        /// LmVocabulary object.
        /// </summary>
        public class LmVocabularyBuilder
        {
            private Dictionary<string, int> map = new Dictionary<string, int>();
            private List<string> tokens = new List<string>();

            public int Add(string word)
            {
                int index = IndexOf(word);
                if (index != -1)
                {
                    return index;
                }
                else
                {
                    index = tokens.Count;
                    map.Add(word, index);
                    tokens.Add(word);
                    return index;
                }
            }

            public int Size()
            {
                return tokens.Count;
            }

            public LmVocabularyBuilder AddAll(params string[] words)
            {
                foreach (string word in words)
                {
                    Add(word);
                }
                return this;
            }

            public LmVocabularyBuilder AddAll(IEnumerable<string> words)
            {
                foreach (string word in words)
                {
                    Add(word);
                }
                return this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <returns>index of input. -1 if input does not exist.</returns>
            public int IndexOf(string key)
            {
                return map.GetValueOrDefault(key, -1);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>an unmodifiable copy of the words so far added.</returns>
            public ReadOnlyCollection<string> Words()
            {
                return tokens.AsReadOnly();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="locale"></param>
            /// <returns>indexes of words when the words are alphabetically sorted according to the input
            /// locale.</returns>
            public IEnumerable<int> AlphabeticallySortedWordsIds(CultureInfo locale)
            {
                TreeMap<string, int> treeMap = new TreeMap<string, int>(locale.CompareInfo.GetStringComparer(CompareOptions.None));
                treeMap.PutAll(map);
                return treeMap.Values();
            }

            /// <summary>
            /// indexes of words when the words are alphabetically sorted according to EN locale.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<int> AlphabeticallySortedWordsIds()
            {
                TreeMap<string, int> treeMap = new TreeMap<string, int>(CultureInfo.InvariantCulture.CompareInfo.GetStringComparer(CompareOptions.Ordinal));
                treeMap.PutAll(map);
                return treeMap.Values();
            }

            /// <summary>
            /// Generated unmodifiable LmVocabulary
            /// </summary>
            /// <returns></returns>
            public LmVocabulary Generate()
            {
                return new LmVocabulary(tokens);
            }

        }
    }
}
