using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.IO
{
    public class TestUtil
    {
        public static bool ContainsAllKeys<T, X>(Dictionary<T, X> map, params T[] keys)
        {
            foreach (T key in keys)
            {
                if (!map.ContainsKey(key))
                {
                    return false;
                }
            }
            return true;
        }


        public static bool ContainsAllValues<V, X>(Dictionary<X, V> map, params V[] values)
        {
            foreach (V value in values)
            {
                if (!map.ContainsValue(value))
                {
                    return false;
                }
            }
            return true;
        }


        public static bool ContainsAll<V>(ISet<V> set, params V[] values)
        {
            foreach (V value in values)
            {
                if (!set.Contains(value))
                {
                    return false;
                }
            }
            return true;
        }

        public static string TempFileWithData(ICollection<string> collection)
        {
            string temp = Path.Combine(Path.GetTempPath(), "zemberek");
            File.WriteAllLines(temp, collection, Encoding.UTF8);
            return temp;
        }

        public static string TempFileWithData(params string[] lines)
        {
            string temp = Path.Combine(Path.GetTempPath(), "zemberek");
            File.WriteAllLines(temp, lines, Encoding.UTF8);
            return temp;
        }
        public static bool ContainsAll<T>(FloatValueMap<T> set, params T[] items)
        {
            foreach (T item in items)
            {
                if (!set.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool ContainsAll<T>(List<ScoredItem<T>> list, params T[] items)
        {
            T[] set = list.Select(s1 => s1.Item).ToArray();
            foreach (T item in items)
            {
                if (!set.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        public static ISet<string> UniqueStrings(int amount, int stringLength)
        {
            return UniqueStrings(amount, stringLength, -1);
        }

        public static ISet<string> UniqueStrings(int amount, int stringLength, int randomSeed)
        {
            ISet<string> set = new HashSet<string>(amount);

            Random r = randomSeed == -1 ? new Random() : new Random(randomSeed);
            while (set.Count < amount)
            {
                StringBuilder sb = new StringBuilder(stringLength);
                for (int i = 0; i < stringLength; i++)
                {
                    sb.Append((char)(r.Next(32) + 'a'));
                }
                set.Add(sb.ToString());
            }
            return set;
        }
    }
}
