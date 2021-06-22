using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon.TR;

namespace ZemberekDotNet.Morphology.Lexicon
{
    /// <summary>
    /// This is the collection of all Dictionary Items.
    /// </summary>
    public class RootLexicon : IEnumerable<DictionaryItem>
    {
        private static readonly int InitialCapacity = 1000;

        private static readonly RootLexicon defaultLexicon = DefaultBinaryLexicon();

        public static RootLexicon GetDefault()
        {
            return defaultLexicon;
        }

        private MultiMap<string, DictionaryItem> itemMap = new MultiMap<string, DictionaryItem>(InitialCapacity);
        private Dictionary<string, DictionaryItem> idMap = new Dictionary<string, DictionaryItem>(InitialCapacity);
        private ISet<DictionaryItem> itemSet = new LinkedHashSet<DictionaryItem>(InitialCapacity);

        public RootLexicon(List<DictionaryItem> dictionaryItems)
        {
            foreach (DictionaryItem dictionaryItem in dictionaryItems)
            {
                Add(dictionaryItem);
            }
        }

        public RootLexicon()
        {
        }

        public void Add(DictionaryItem item)
        {
            if (itemSet.Contains(item))
            {
                Log.Warn("Duplicated item:" + item);
                return;
            }
            if (idMap.ContainsKey(item.id))
            {
                Log.Warn("Duplicated item id of:" + item + " with " + idMap.GetValueOrDefault(item.id));
                return;
            }
            this.itemSet.Add(item);
            idMap.Add(item.id, item);
            itemMap.Add(item.lemma, item);
        }

        public void AddAll(IEnumerable<DictionaryItem> items)
        {
            foreach (DictionaryItem item in items)
            {
                Add(item);
            }
        }

        public IEnumerable<DictionaryItem> GetAllItems()
        {
            return itemMap.Values;
        }

        public List<DictionaryItem> GetMatchingItems(string lemma)
        {
            ICollection<DictionaryItem> items = itemMap[lemma];
            if (items == null)
            {
                return new List<DictionaryItem>();
            }
            else
            {
                return new List<DictionaryItem>(items);
            }
        }

        public void Remove(DictionaryItem item)
        {
            itemMap[item.lemma].Remove(item);
            idMap.Remove(item.id);
            itemSet.Remove(item);
        }

        public void RemoveAllLemmas(string lemma)
        {
            foreach (DictionaryItem item in GetMatchingItems(lemma))
            {
                Remove(item);
            }
        }

        public void RemoveAllLemmas(IEnumerable<string> lemmas)
        {
            foreach (string lemma in lemmas)
            {
                RemoveAllLemmas(lemma);
            }
        }

        public void RemoveAll(IEnumerable<DictionaryItem> items)
        {
            foreach (DictionaryItem item in items)
            {
                Remove(item);
            }
        }

        public bool ContainsItem(DictionaryItem item)
        {
            return itemSet.Contains(item);
        }

        public DictionaryItem GetItemById(string id)
        {
            return idMap.GetValueOrDefault(id);
        }

        public List<DictionaryItem> GetMatchingItems(string lemma, PrimaryPos pos)
        {
            ICollection<DictionaryItem> items = itemMap[lemma];
            if (items == null)
            {
                return new List<DictionaryItem>();
            }
            List<DictionaryItem> matches = new List<DictionaryItem>(1);
            foreach (DictionaryItem item in items)
            {
                if (item.primaryPos == pos)
                {
                    matches.Add(item);
                }
            }
            return matches;
        }

        public static RootLexicon FromLines(params string[] lines)
        {
            return Builder().AddDictionaryLines(lines).Build();
        }

        public static RootLexicon FromResources(params string[] resources)
        {
            return Builder().AddTextDictionaryResources(resources).Build();
        }

        public static RootLexicon FromResources(ICollection<string> resources)
        {
            return Builder().AddTextDictionaryResources(resources).Build();
        }

        public bool IsEmpty()
        {
            return itemSet.IsEmpty();
        }

        public int Size()
        {
            return itemSet.Count;
        }

        public IEnumerator<DictionaryItem> GetEnumerator()
        {
            return itemSet.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static RootLexiconBuilder Builder()
        {
            return new RootLexiconBuilder();
        }

        public class RootLexiconBuilder
        {
            RootLexicon lexicon = new RootLexicon();
            public RootLexiconBuilder AddBinaryDictionary(string dictionaryPath)
            {
                lexicon.AddAll(DictionarySerializer.Load(dictionaryPath).GetAllItems());
                return this;
            }

            public RootLexiconBuilder AddDefaultLexicon()
            {
                if (lexicon.Size() == 0)
                {
                    lexicon = GetDefault();
                }
                else
                {
                    AddLexicon(GetDefault());
                }
                return this;
            }

            public RootLexiconBuilder SetLexicon(RootLexicon lexicon)
            {
                this.lexicon = lexicon;
                return this;
            }

            public RootLexiconBuilder AddLexicon(RootLexicon lexicon)
            {
                this.lexicon.AddAll(lexicon.GetAllItems());
                return this;
            }

            public RootLexiconBuilder AddTextDictionaries(params string[] dictionaryFiles)
            {
                List<string> lines = new List<string>();
                foreach (string file in dictionaryFiles)
                {
                    lines.AddRange(File.ReadAllLines(file));
                }
                lexicon.AddAll(TurkishDictionaryLoader.Load(lines));
                return this;
            }

            public RootLexiconBuilder AddDictionaryLines(params string[] lines)
            {
                lexicon.AddAll(TurkishDictionaryLoader.Load(lines));
                return this;
            }

            public RootLexiconBuilder RemoveDictionaryFiles(params string[] dictionaryFiles)
            {
                foreach (string file in dictionaryFiles)
                {
                    lexicon.RemoveAll(TurkishDictionaryLoader.Load(file));
                }
                return this;
            }

            public RootLexiconBuilder AddTextDictionaryResources(IEnumerable<string> resources)
            {
                Log.Info("Dictionaries :{0}", string.Join(", ", resources));
                List<string> lines = new List<string>();
                foreach (string resource in resources)
                {
                    lines.AddRange(TextIO.LoadLines(resource));
                }
                lexicon.AddAll(TurkishDictionaryLoader.Load(lines));
                return this;
            }

            public RootLexiconBuilder AddTextDictionaryResources(params string[] resources)
            {
                return AddTextDictionaryResources(new List<string>(resources));
            }

            public RootLexiconBuilder RemoveItems(IEnumerable<string> dictionaryString)
            {
                lexicon.RemoveAll(TurkishDictionaryLoader.Load(dictionaryString));
                return this;
            }

            public RootLexiconBuilder RemoveAllLemmas(IEnumerable<string> lemmas)
            {
                lexicon.RemoveAllLemmas(lemmas);
                return this;
            }

            public RootLexiconBuilder AddDictionaryLines(ICollection<string> lines)
            {
                lexicon.AddAll(TurkishDictionaryLoader.Load(lines));
                return this;
            }

            public RootLexicon Build()
            {
                return lexicon;
            }
        }

        private static RootLexicon DefaultBinaryLexicon()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                RootLexicon lexicon = DictionarySerializer.Load("Resources/tr/lexicon.bin");
                Log.Info("Dictionary generated in {0} ms", stopwatch.ElapsedMilliseconds);
                return lexicon;
            }
            catch (IOException e)
            {
                throw new SystemException(
                    "Cannot load default binary dictionary. Reason:" + e.Message, e);
            }
        }
    }
}
