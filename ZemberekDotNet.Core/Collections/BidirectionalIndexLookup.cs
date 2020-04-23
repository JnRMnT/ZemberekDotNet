using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZemberekDotNet.Core.Collections
{
    public class BidirectionalIndexLookup<T>
    {
        UIntValueMap<T> indexLookup;
        UIntMap<T> keyLookup;

        public BidirectionalIndexLookup(UIntValueMap<T> indexLookup, UIntMap<T> keyLookup)
        {
            this.indexLookup = indexLookup;
            this.keyLookup = keyLookup;
        }

        public static BidirectionalIndexLookup<string> fromTextFileWithIndex(string path, char delimiter)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException($"File {path} does not exist.");
            }
            IEnumerable<string> lines = File.ReadLines(path);
            UIntValueMap<string> indexLookup = new UIntValueMap<string>(lines.Count());
            UIntMap<string> wordLookup = new UIntMap<string>(lines.Count());
            foreach (String line in lines)
            {
                StringPair pair = StringPair.FromString(line, delimiter);
                String word = pair.First;
                int index = int.Parse(pair.Second);
                if (indexLookup.Contains(word))
                {
                    throw new ArgumentException($"Duplicated word in line : [" + line + "]");
                }
                if (wordLookup.ContainsKey(index))
                {
                    throw new ArgumentException("Duplicated index in line : [" + line + "]");
                }
                if (index < 0)
                {
                    throw new ArgumentException("Index Value cannot be negative : [" + line + "]");
                }
                indexLookup.Put(word, index);
                wordLookup.Put(index, word);
            }
            return new BidirectionalIndexLookup<string>(indexLookup, wordLookup);
        }

        public int GetIndex(T key)
        {
            return indexLookup.Get(key);
        }

        public T GetKey(int index)
        {
            return keyLookup.Get(index);
        }

        public bool ContainsKey(T key)
        {
            return Enumerable.Contains(indexLookup, key);
        }

        public IEnumerable<T> Keys()
        {
            return indexLookup.GetKeyList();
        }

        public int Size()
        {
            return indexLookup.Size();
        }
    }
}