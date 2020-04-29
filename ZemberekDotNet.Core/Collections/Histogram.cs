using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using ZemberekDotNet.Core.Native.Collections;
using System.Collections;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// A simple set like data structure for counting unique elements. Not thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Histogram<T> : IEnumerable<T>
    {
        private readonly UIntValueMap<T> map;
        public Histogram(int initialSize)
        {
            map = new UIntValueMap<T>(initialSize);
        }

        public Histogram(Dictionary<T, int> countMap)
        {
            this.map = new UIntValueMap<T>(countMap.Count);
            foreach (T t in countMap.Keys)
            {
                this.map.Put(t, countMap.GetValueOrDefault(t));
            }
        }

        public Histogram()
        {
            map = new UIntValueMap<T>();
        }

        /// <summary>
        /// Loads from file with format: [key][delimiter][count]
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static Histogram<String> LoadFromLines(string[] lines, char delimiter)
        {
            return LoadFromLines(lines, delimiter, true);
        }

        public static Histogram<string> LoadFromLines(
            string[] lines,
            char delimiter,
            bool keyComesFirst)
        {
            Histogram<string> result = new Histogram<string>(lines.Length);
            foreach (string s in lines)
            {
                int index = s.IndexOf(delimiter);
                if (index <= 0)
                {
                    throw new InvalidOperationException("Bad histogram line = " + s);
                }
                string item = keyComesFirst ? s.Substring(0, index) : s.Substring(index + 1);
                string countStr = keyComesFirst ? s.Substring(index + 1) : s.Substring(0, index);
                int count = int.Parse(countStr);
                result.Add(item, count);
            }
            return result;
        }

        /// <summary>
        ///  Loads a String Histogram from a file. Counts are supposedly delimited with `delimiter`
        ///  character. format: [key][delimiter][count]
        /// </summary>
        /// <param name="path">path file path</param>
        /// <param name="delimiter">delimiter delimiter</param>
        /// <returns>a Histogram.</returns>
        public static Histogram<String> LoadFromUtf8File(string path, char delimiter)
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            return LoadFromLines(lines, delimiter);
        }

        public static Histogram<String> LoadFromUtf8File(
            string path,
            char delimiter,
            bool keyComesFirst)
        {
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            return LoadFromLines(lines, delimiter, keyComesFirst);
        }


        public static void SerializeStringHistogram(Histogram<string> h, BinaryWriter dos)
        {
            dos.Write(h.Size());
            foreach (var key in h.map)
            {
                dos.Write(key);
                dos.Write(h.GetCount(key).EnsureEndianness());
            }
        }

        public static Histogram<string> DeserializeStringHistogram(BinaryReader dis)
        {
            int size = dis.ReadInt32().EnsureEndianness();
            if (size < 0)
            {
                throw new InvalidOperationException("Cannot deserialize String histogram. Count value is negative : " + size);
            }
            Histogram<string> result = new Histogram<string>(size);
            for (int i = 0; i < size; i++)
            {
                result.Set(dis.ReadString(), dis.ReadInt32());
            }
            return result;
        }

        public void SaveSortedByCounts(string path, string delimiter)
        {
            try
            {
                using (TextWriter textWriter = File.CreateText(path))
                {
                    List<T> sorted = GetSortedList();
                    foreach (T t in sorted)
                    {
                        textWriter.WriteLine(t + delimiter + GetCount(t));
                    }
                }
            }
            catch { }
        }

        public void SaveSortedByKeys(string path, String delimiter, IComparer<T> comparator)
        {
            try
            {
                using (TextWriter textWriter = File.CreateText(path))
                {
                    List<T> sorted = GetSortedList(comparator);
                    foreach (T t in sorted)
                    {
                        textWriter.WriteLine(t + delimiter + GetCount(t));
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// adds an element. and increments it's count.
        /// </summary>
        /// <param name="t">element to add.</param>
        /// <returns>the count of the added element.</returns>
        public int Add(T t)
        {
            return Add(t, 1);
        }

        /// <summary>
        /// adds an element. and increments it's count.
        /// </summary>
        /// <param name="t">element to add.</param>
        /// <param name="count">the count of the element to add.</param>
        /// <returns>the count of the added element.</returns>
        public int Add(T t, int count)
        {
            return map.IncrementByAmount(t, count);
        }

        /// <summary>
        /// merges another Histogram to this one.
        /// </summary>
        /// <param name="otherSet">another Histogram</param>
        public void Add(Histogram<T> otherSet)
        {
            if (otherSet == null)
            {
                throw new ArgumentNullException("Histogram cannot be null");
            }
            foreach (T t in otherSet)
            {
                Add(t, otherSet.GetCount(t));
            }
        }

        public T Lookup(T item)
        {
            return map.Lookup(item);
        }

        /// <summary>
        ///  adds a collection of elements.
        /// </summary>
        /// <param name="collection">collection a collection of elements.</param>
        public void Add(ICollection<T> collection)
        {

            if (collection == null)
            {
                throw new ArgumentNullException("collection cannot be null");
            }

            foreach (T item in collection)
            {
                Add(item);
            }
        }

        /// <summary>
        /// adds an array of elements.
        /// </summary>
        /// <param name="array">an array of elements to add.</param>
        public void Add(params T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array cannot be null");
            }
            foreach (T t in array)
            {
                Add(t);
            }
        }

        /// <summary>
        ///  returns the total element count of the counting set.
        /// </summary>
        /// <returns>element count.</returns>
        public int Size()
        {
            return map.Size();
        }

        /// <summary>
        /// inserts the element and its value. it overrides the current count
        /// </summary>
        /// <param name="t">element</param>
        /// <param name="c">count value which will override the current count value.</param>
        public void Set(T t, int c)
        {
            map.Put(t, c);
        }

        public int DecrementIfPositive(T t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("Element cannot be null");
            }
            int c = map.Get(t);
            if (c > 0)
            {
                return map.Decrement(t);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// current count of the given element
        /// </summary>
        /// <param name="t">element</param>
        /// <returns>count of the element. if element does not exist, 0</returns>
        public int GetCount(T t)
        {
            int i = map.Get(t);
            return i < 0 ? 0 : i;
        }

        /// <summary>
        /// if element exist.
        /// </summary>
        /// <param name="t">element.</param>
        /// <returns>if element exists.</returns>
        public bool Contains(T t)
        {
            return map.Contains(t);
        }

        /// <summary>
        /// returns the Elements in a list sorted by count, descending..
        /// </summary>
        /// <param name="n"></param>
        /// <returns>Elements in a list sorted by count, descending..</returns>
        public List<T> GetTop(int n)
        {
            if (n > Size())
            {
                n = Size();
            }
            List<IntValueMap<T>.Entry<T>> l = map.GetAsEntryList();
            l.Sort();
            return l.Select(e => e.key).Take(n).ToList();
        }

        /// <summary>
        /// removes the items that has a count smaller than minCount
        /// </summary>
        /// <param name="minCount">minimum count amount to remain in the set.</param>
        /// <returns>reduced set.</returns>
        public int RemoveSmaller(int minCount)
        {
            HashSet<T> toRemove = new HashSet<T>();
            int removeCount = 0;
            foreach (var key in map)
            {
                if (map.Get(key) < minCount)
                {
                    toRemove.Add(key);
                    removeCount++;
                }
            }

            foreach (T item in toRemove)
            {
                map.Remove(item);
            }

            return removeCount;
        }

        /// <summary>
        /// removes the items that has a count larger than minCount
        /// </summary>
        /// <param name="maxCount">maximum count amount to remain in the set.</param>
        /// <returns>reduced set.</returns>
        public int RemoveLarger(int maxCount)
        {
            HashSet<T> toRemove = new HashSet<T>();
            int removeCount = 0;
            foreach (var key in map)
            {
                if (map.Get(key) > maxCount)
                {
                    toRemove.Add(key);
                    removeCount++;
                }
            }

            foreach (T item in toRemove)
            {
                map.Remove(item);
            }

            return removeCount;
        }

        /// <summary>
        /// counts the items those count is smaller than amount
        /// </summary>
        /// <param name="amount">amount to check size</param>
        /// <returns>count.</returns>
        public int SizeSmaller(int amount)
        {
            return map.SizeSmaller(amount);
        }

        /// <summary>
        /// removes an item.
        /// </summary>
        /// <param name="t">item to removed.</param>
        public void Remove(T t)
        {
            map.Remove(t);
        }

        /// <summary>
        /// removes all items.
        /// </summary>
        /// <param name="items">item to removed.</param>
        public void RemoveAll(IEnumerable<T> items)
        {
            foreach (T t in items)
            {
                map.Remove(t);
            }
        }

        /// <summary>
        /// counts the items those count is smaller than amount
        /// </summary>
        /// <param name="amount">amount to check size</param>
        /// <returns>count.</returns>
        public int SizeLarger(int amount)
        {
            return map.SizeLarger(amount);
        }

        /// <summary>
        /// total count of items those value is between "minValue" and "maxValue"
        /// </summary>
        /// <param name="minValue">minValue inclusive</param>
        /// <param name="maxValue">maxValue inclusive</param>
        /// <returns>total count of items those value is between "minValue" and "maxValue"</returns>
        public long TotalCount(int minValue, int maxValue)
        {
            return map.SumOfValues(minValue, maxValue);
        }

        /// <summary>
        /// returns the max count value.
        /// </summary>
        /// <returns>the max value in the set if set is empty, 0 is returned.</returns>
        public int MaxValue()
        {
            return map.MaxValue();
        }


        /// <summary>
        /// returns the min count value.
        /// </summary>
        /// <returns>the min value in the set, if set is empty, Integer.MAX_VALUE is returned.</returns>
        public int MinValue()
        {
            return map.MinValue();
        }

        /// <summary>
        ///  returns the list of elements whose count is equal to "value"
        /// </summary>
        /// <param name="value">the value for the keys</param>
        /// <returns>the list of elements whose count is equal to "value"</returns>
        public List<T> GetItemsWithCount(int value)
        {
            List<T> keys = new List<T>();
            foreach (var key in map)
            {
                if (map.Get(key) == value)
                {
                    keys.Add(key);
                }
            }
            return keys;
        }

        /// <summary>
        /// Returns the list of elements whose count is between "min" and "max" (both inclusive)
        /// </summary>
        /// <param name="min">min</param>
        /// <param name="max">max</param>
        /// <returns>Returns the list of elements whose count is between "min" and "max" (both inclusive)</returns>
        public List<T> GetItemsWithCount(int min, int max)
        {
            List<T> keys = new List<T>();
            foreach (var key in map)
            {
                int value = map.Get(key);
                if (value >= min && value <= max)
                {
                    keys.Add(key);
                }
            }
            return keys;
        }

        /// <summary>
        /// Percentage of total count of [min-max] to total counts.
        /// </summary>
        /// <param name="min">min (inclusive)</param>
        /// <param name="max">max (inclusive)</param>
        /// <returns>count.</returns>
        public double CountPercent(int min, int max)
        {
            return (TotalCount(min, max) * 100d) / TotalCount();
        }

        /// <summary>
        /// returns the Elements in a list sorted by count, descending.
        /// </summary>
        /// <returns>Elements in a list sorted by count, descending.</returns>
        public List<T> GetSortedList()
        {
            List<T> l = new List<T>(map.Size());
            l.AddRange(GetSortedEntryList().Select(entry => entry.key).ToArray());
            return l;
        }

        /// <summary>
        /// Returns keys that both histogram contain.
        /// </summary>
        /// <param name="other">Another Histogram</param>
        /// <returns>A set of keys that both histogram contain.</returns>
        public ISet<T> GetIntersectionOfKeys(Histogram<T> other)
        {
            LinkedHashSet<T> result = new LinkedHashSet<T>();
            Histogram<T> smaller = other.Size() < Size() ? other : this;
            Histogram<T> larger = smaller == this ? other : this;
            foreach (T t in smaller.GetSortedList())
            {
                if (larger.Contains(t))
                {
                    result.Add(t);
                }
            }
            return result;
        }

        /// <summary>
        /// returns the Elements in a list sorted by count, descending.
        /// </summary>
        /// <returns>Elements in a list sorted by count, descending.</returns>
        public List<IntValueMap<T>.Entry<T>> GetSortedEntryList()
        {
            List<IntValueMap<T>.Entry<T>> l = map.GetAsEntryList();
            l.Sort();
            return l;
        }

        /// <summary>
        /// returns the Elements in a list sorted by count, descending.
        /// </summary>
        /// <returns>Elements in a list sorted by count, descending.</returns>
        public List<IntValueMap<T>.Entry<T>> GetEntryList()
        {
            return map.GetAsEntryList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="it"></param>
        /// <returns>total count of the items in the input Iterable.</returns>
        public long TotalCount(IEnumerable<T> it)
        {
            long count = 0;
            foreach (T t in it)
            {
                count += GetCount(t);
            }
            return count;
        }

        /// <summary>
        /// returns the Elements in a list sorted by the given comparator.
        /// </summary>
        /// <param name="comp">a Comarator of T</param>
        /// <returns>Elements in a list sorted by the given comparator.</returns>
        public List<T> GetSortedList(IComparer<T> comp)
        {
            List<T> l = new List<T>(map);
            l.Sort(comp);
            return l;
        }

        /// <summary>
        /// returns elements in a set.
        /// </summary>
        /// <returns>a set containing the elements.</returns>
        public ISet<T> GetKeySet()
        {
            return map.GetKeySet();
        }

        /// <summary>
        /// Sums all item's counts.
        /// </summary>
        /// <returns>sum of all item's count.</returns>
        public long TotalCount()
        {
            return map.SumOfValues();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)map.GetEnumerator();
        }

        /// <summary>
        /// returns an iterator for elements.
        /// </summary>
        /// <returns> an iterator for elements.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return map.GetEnumerator();
        }

        //TODO:Check
        //private class CountComparator: IComparer<Dictionary<T, int>> {
        //    public int compare(Dictionary<T, int> o1, Dictionary<T, int> o2)
        //    {
        //        return (o2.getValue() < o1.getValue()) ? -1 : ((o2.getValue() > o1.getValue()) ? 1 : 0);
        //    }
        //}
    }
}
