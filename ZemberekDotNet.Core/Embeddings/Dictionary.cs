using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Core.Embeddings
{
    public class Dictionary
    {
        internal static readonly int TypeWord = 0;
        internal static readonly int TypeLabel = 1;

        private static readonly int MaxVocabSize = 10_000_000;
        private static readonly int MaxLineSize = 1024;
        internal static string EOS = "</s>";
        private static readonly string BOW = "<";
        private static readonly string EOW = ">";
        private static readonly SpaceTabTokenizer tokenizer = new SpaceTabTokenizer();
        private readonly Args args_;
        private int[] word2int_;
        private List<Entry> words_;
        private float[] pdiscard_;
        private int size_;
        private int nwords_;
        private int nlabels_;
        private long ntokens_;
        private int pruneidx_size_ = -1;
        private readonly IntIntMap pruneidx_ = new IntIntMap();

        private Dictionary(Args args)
        {
            args_ = args;
            size_ = 0;
            nwords_ = 0;
            nlabels_ = 0;
            ntokens_ = 0;
            word2int_ = new int[MaxVocabSize];
            words_ = new List<Entry>(100_000);
            Array.Fill(word2int_, -1);
        }

        public static int Hash(string str)
        {
            return Hash(str, 0, str.Length);
        }

        public static int Hash(byte[] bytes)
        {
            uint h = 0x811C_9DC5;
            foreach (byte b in bytes)
            {
                h = h ^ (uint)b;
                h = h * 16777619;
            }
            return (int)(h & 0x7fff_ffff);
        }

        // original fasttext code uses this code:
        // uint32_t h = 2166136261;
        // for (size_t i = 0; i < str.size(); i++) {
        //   h = h ^ uint32_t(int8_t(str[i]));
        //   h = h * 16777619;
        // }
        //
        public static int Hash(String str, int start, int end)
        {
            uint h = 0x811C_9DC5;
            for (int i = start; i < end; i++)
            {
                h = h ^ str[i];
                h = h * 16777619;
            }
            return (int)(h & 0x7fff_ffff);
        }

        public static Dictionary ReadFromFile(string file, Args args)
        {
            Log.Info("Initialize dictionary and histograms.");
            Dictionary dictionary = new Dictionary(args);

            Log.Info("Loading text.");
            BlockTextLoader loader = BlockTextLoader.FromPath(file, 100_000);
            SpaceTabTokenizer tokenizer = new SpaceTabTokenizer();

            int blockCounter = 1;

            foreach (TextChunk lines in loader)
            {
                foreach (String line in lines)
                {
                    List<string> split = tokenizer.SplitToList(line);
                    split.Add(EOS);
                    foreach (String word in split)
                    {
                        if (word.StartsWith("#"))
                        {
                            continue;
                        }
                        dictionary.Add(word);
                    }
                }
                Log.Info("Lines read: %d (thousands) ", blockCounter * 100);
                blockCounter++;
            }
            Log.Info("Word + Label count = %d", dictionary.words_.Count);
            Log.Info("Removing word and labels with small counts. Min word = %d, Min Label = %d",
                args.minCount, args.minCountLabel);
            // now we have the histograms. Remove based on count.
            dictionary.words_.Sort((e1, e2) =>
            {
                if (e1.type != e2.type)
                {
                    return e1.type.CompareTo(e2.type);
                }
                else
                {
                    return e2.count.CompareTo(e1.count);
                }
            });

            //TODO: add threshold method.
            LinkedHashSet<Entry> all = new LinkedHashSet<Entry>(dictionary.words_);
            List<Entry> toRemove = dictionary.words_.Where(s => (s.type == TypeWord && s.count < args.minCount ||
                    s.type == TypeLabel && s.count < args.minCountLabel)).ToList();
            foreach (Entry toBeRemoved in toRemove)
            {
                all.Remove(toBeRemoved);
            }

            dictionary.words_ = new List<Entry>(all);
            dictionary.size_ = 0;
            dictionary.nwords_ = 0;
            dictionary.nlabels_ = 0;
            Array.Fill(dictionary.word2int_, -1);
            foreach (Entry e in dictionary.words_)
            {
                int i = dictionary.Find(e.word);
                dictionary.word2int_[i] = dictionary.size_++;
                if (e.type == TypeWord)
                {
                    dictionary.nwords_++;
                }
                if (e.type == TypeLabel)
                {
                    dictionary.nlabels_++;
                }
            }
            Log.Info("Word count = %d , Label count = %d", dictionary.NWords(), dictionary.NLabels());
            dictionary.InitTableDiscard();
            dictionary.InitNGrams();
            return dictionary;
        }

        public static Dictionary Load(BinaryReader dis, Args args)
        {
            Dictionary dict = new Dictionary(args);
            dict.size_ = dis.ReadInt32().EnsureEndianness();
            dict.nwords_ = dis.ReadInt32().EnsureEndianness();
            dict.nlabels_ = dis.ReadInt32().EnsureEndianness();
            dict.ntokens_ = dis.ReadInt64().EnsureEndianness();
            dict.pruneidx_size_ = dis.ReadInt32().EnsureEndianness();
            for (int i = 0; i < dict.size_; i++)
            {
                Entry e = new Entry();
                e.word = dis.ReadUTF();
                e.count = dis.ReadInt32().EnsureEndianness();
                e.type = dis.ReadInt32().EnsureEndianness();
                dict.words_.Add(e);
            }
            for (int i = 0; i < dict.pruneidx_size_; i++)
            {
                int first = dis.ReadInt32().EnsureEndianness();
                int second = dis.ReadInt32().EnsureEndianness();
                dict.pruneidx_.Put(first, second);
            }
            dict.Init();

            int word2IntSize = (int)System.Math.Ceiling(dict.size_ / 0.7);
            dict.word2int_ = new int[word2IntSize];
            Array.Fill(dict.word2int_, -1);
            for (int i = 0; i < dict.size_; i++)
            {
                dict.word2int_[dict.Find(dict.words_[i].word)] = i;
            }

            return dict;
        }

        public void Init()
        {
            InitTableDiscard();
            InitNGrams();
        }

        /// <summary>
        ///  This looks like a linear probing hash table.
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        private int Find(String w)
        {
            return Find(w, Hash(w));
        }

        private int Find(String w, int h)
        {
            int word2IntSize = word2int_.Length;
            int id = h % word2IntSize;
            while (word2int_[id] != -1 && !words_[word2int_[id]].word.Equals(w))
            {
                id = (id + 1) % word2IntSize;
            }
            return id;
        }

        public void Add(String w)
        {
            AddWithCount(w, 1);
        }

        private void AddWithCount(String w, int count)
        {
            int h = Find(w);
            // if this is an empty slot. add a new entry.
            ntokens_ += count;
            if (word2int_[h] == -1)
            {
                Entry e = new Entry();
                e.word = w;
                e.count = count;
                e.type = GetType(w);
                words_.Add(e);
                word2int_[h] = size_++;
            }
            else
            {
                // or increment the count.
                words_[word2int_[h]].count += count;
            }
        }

        private int GetType(string w)
        {
            return (w.StartsWith(args_.label)) ? TypeLabel : TypeWord;
        }

        public int NWords()
        {
            return nwords_;
        }

        public int NLabels()
        {
            return nlabels_;
        }

        public long NTokens()
        {
            return ntokens_;
        }

        public int[] GetSubWords(int i)
        {
            Contract.Assert(i >= 0);
            Contract.Assert(i < nwords_);
            return words_[i].subwords;
        }

        public List<string> GetLabels()
        {
            List<string> results = new List<string>();
            foreach (Entry entry in words_)
            {
                if (entry.type == TypeLabel)
                {
                    results.Add(entry.word);
                }
            }
            return results;
        }

        public int[] GetSubWords(string word)
        {
            int i = GetId(word);
            if (i >= 0)
            {
                return GetSubWords(i);
            }
            if (!word.Equals(EOS))
            {
                return ComputeSubWords(BOW + word + EOW, i);
            }
            else
            {
                return new int[0];
            }
        }

        private bool Discard(int id, float rand)
        {
            Contract.Assert(id >= 0);
            Contract.Assert(id < nwords_);
            return rand > pdiscard_[id];
        }

        public int GetId(string w, int h)
        {
            int index = Find(w, h);
            return word2int_[index];
        }


        public int GetId(string w)
        {
            int h = Find(w);
            return word2int_[h];
        }

        public int GetType(int id)
        {
            Contract.Assert(id >= 0);
            Contract.Assert(id < size_);
            return words_[id].type;
        }

        public string GetWord(int id)
        {
            Contract.Assert(id >= 0);
            Contract.Assert(id < size_);
            return words_[id].word;
        }

        private int[] ComputeSubWords(string word, int wordId)
        {
            int[] hashes = args_.subWordHashProvider.GetHashes(word, wordId);
            IntVector k = new IntVector();
            foreach (int hash in hashes)
            {
                PushHash(k, hash % args_.bucket);
            }
            return k.CopyOf();
        }

        private void InitNGrams()
        {
            for (int i = 0; i < size_; i++)
            {
                String word = BOW + words_[i].word + EOW;
                // adds the wordId to the n-grams as well.
                if (!words_[i].word.Equals(EOS))
                {
                    words_[i].subwords = ComputeSubWords(word, i);
                }
            }
        }

        private void InitTableDiscard()
        {
            pdiscard_ = new float[size_];
            for (int i = 0; i < size_; i++)
            {
                float f = ((float)words_[i].count) / ntokens_;
                pdiscard_[i] = (float)(System.Math.Sqrt(args_.t / f) + args_.t / f);
            }
        }

        public int[] GetCounts(int entry_type)
        {
            int[] counts = entry_type == TypeWord ? new int[nwords_] : new int[nlabels_];
            int c = 0;
            foreach (Entry entry in words_)
            {
                if (entry.type == entry_type)
                {
                    counts[c] = entry.count;
                    c++;
                }
            }
            return counts;
        }

        //adds word level n-grams hash values to input word index Vector.
        // n=1 means uni-grams, no value is added.
        public void AddWordNgramHashes(IntVector line, int n)
        {
            if (n == 1)
            {
                return;
            }
            int line_size = line.Size();
            for (int i = 0; i < line_size; i++)
            {
                long h = line.Get(i);
                for (int j = i + 1; j < line_size && j < i + n; j++)
                {
                    h = h * 116049371 + line.Get(j);
                    PushHash(line, (int)(h % args_.bucket));
                }
            }
        }

        public void AddWordNgramHashes(IntVector line, IntVector hashes, int n)
        {
            for (int i = 0; i < hashes.Size(); i++)
            {
                long h = hashes.Get(i);
                for (int j = i + 1; j < hashes.Size() && j < i + n; j++)
                {
                    h = h * 116049371 + hashes.Get(j);
                    PushHash(line, (int)(h % args_.bucket));
                }
            }
        }

        public void PushHash(IntVector hashes, int id)
        {
            if (pruneidx_size_ == 0 || id < 0)
            {
                return;
            }
            if (pruneidx_size_ > 0)
            {
                if (pruneidx_.ContainsKey(id))
                {
                    id = pruneidx_.Get(id);
                }
                else
                {
                    return;
                }
            }
            hashes.Add(nwords_ + id);
        }


        public int GetLine(
            string line,
            IntVector words,
            Random random)
        {

            int ntokens = 0;
            List<string> tokens = tokenizer.SplitToList(line);

            foreach (String token in tokens)
            {
                if (token.StartsWith("#"))
                {
                    continue;
                }
                int h = Hash(token);
                int wid = GetId(token, h);
                if (wid < 0)
                {
                    continue;
                }
                ntokens++;
                if (GetType(wid) == TypeWord && !Discard(wid, random.NextFloat()))
                {
                    words.Add(wid);
                }
                if (ntokens > MaxLineSize || token.Equals(EOS))
                {
                    break;
                }
            }
            return ntokens;
        }

        public void AddSubwords(IntVector line, string token, int wid)
        {
            if (wid < 0)
            { // out of vocab
                if (!token.Equals(EOS))
                {
                    ComputeSubWords(BOW + token + EOW, wid);
                }
            }
            else
            {
                if (args_.maxn <= 0)
                { // in vocab w/o subwords
                    line.Add(wid);
                }
                else
                { // in vocab w/ subwords
                    int[] ngrams = GetSubWords(wid);
                    line.AddAll(ngrams);
                }
            }
        }

        public int GetLine(
            string line,
            IntVector words,
            IntVector labels)
        {

            IntVector wordHashes = new IntVector();
            int ntokens = 0;
            List<String> tokens = tokenizer.SplitToList(line);

            foreach (String token in tokens)
            {
                if (token.StartsWith("#"))
                {
                    continue;
                }
                int h = Hash(token);
                int wid = GetId(token, h);
                int type = wid < 0 ? GetType(token) : GetType(wid);
                ntokens++;
                if (type == TypeWord)
                {
                    AddSubwords(words, token, wid);
                    wordHashes.Add(h);
                }
                else if (type == TypeLabel)
                {
                    labels.Add(wid - nwords_);
                }
                if (token.Equals(EOS))
                {
                    break;
                }
            }
            AddWordNgramHashes(words, wordHashes, args_.wordNgrams);
            return ntokens;
        }


        public string GetLabel(int lid)
        {
            if (lid < 0 || lid >= nlabels_)
            {
                throw new ArgumentException
                    (String.Format("Label id %d is out of range [0, %d]", lid, nlabels_));
            }
            return words_[lid + nwords_].word;
        }

        public int[] Prune(int[] idx)
        {
            IntVector words = new IntVector();
            IntVector ngrams = new IntVector();
            foreach (int i in idx)
            {
                if (i < nwords_)
                {
                    words.Add(i);
                }
                else
                {
                    ngrams.Add(i);
                }
            }
            words.Sort();
            IntVector newIndexes = new IntVector(words.CopyOf());
            int j;
            if (ngrams.Size() > 0)
            {
                j = 0;
                for (int k = 0; k < ngrams.Size(); k++)
                {
                    int ngram = ngrams.Get(k);
                    pruneidx_.Put(ngram - nwords_, j);
                    j++;
                }
                newIndexes.AddAll(ngrams);
            }
            pruneidx_size_ = pruneidx_.Size();
            Array.Fill(word2int_, -1);

            j = 0;
            for (int i = 0; i < words_.Count; i++)
            {
                if (GetType(i) == TypeLabel || (j < words.Size() && words.Get(j) == i))
                {
                    words_[j] = words_[i];
                    word2int_[Find(words_[j].word)] = j;
                    j++;
                }
            }
            nwords_ = words.Size();
            size_ = nwords_ + nlabels_;
            words_ = new List<Entry>(words_.Take(size_));
            InitNGrams();
            return newIndexes.CopyOf();
        }

        public void Save(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(size_.EnsureEndianness());
            binaryWriter.Write(nwords_.EnsureEndianness());
            binaryWriter.Write(nlabels_.EnsureEndianness());
            binaryWriter.Write(ntokens_.EnsureEndianness());
            binaryWriter.Write(pruneidx_size_.EnsureEndianness());
            for (int i = 0; i < size_; i++)
            {
                Entry e = words_[i];
                binaryWriter.WriteUTF(e.word);
                binaryWriter.Write(e.count.EnsureEndianness());
                binaryWriter.Write(e.type.EnsureEndianness());
            }
            foreach (int key in pruneidx_.GetKeys())
            {
                binaryWriter.Write(key.EnsureEndianness());
                binaryWriter.Write(pruneidx_.Get(key).EnsureEndianness());
            }
        }

        public class Entry
        {
            internal string word;
            internal int count;
            internal int type;
            internal int[] subwords = new int[0];
        }
    }
}
