using System;
using System.Collections;
using System.Collections.Generic;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    ///  A memory efficient and fast set like data structure with Integer values. Values can be between
    ///  int.MinValue and int.MaxValue.Methods do not check for overflow or underflow.Class is
    ///  not thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntValueMap<T> : HashBase<T>, IEnumerable<T>
    {
        // Carries count values.
        private int?[] values;

        public IntValueMap() : this(InitialSize)
        {

        }

        public IntValueMap(int size) : base(size)
        {
            values = new int?[keys.Length];
        }

        /// <summary>
        /// If key does not exist, it adds it with count value 1. Otherwise, it increments the count value
        /// by 1.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>the new value after AddOrIncrement</returns>
        public int AddOrIncrement(T key)
        {
            return IncrementByAmount(key, 1);
        }

        /// <summary>
        /// Adds all keys in Iterable. If key does not exist, it adds it with count value 1. Otherwise, it
        /// increments the count value by 1.
        /// </summary>
        /// <param name="key">key</param>
        public void AddOrIncrementAll(IEnumerable<T> keys)
        {
            foreach (T t in keys)
            {
                IncrementByAmount(t, 1);
            }
        }

        /// <summary>
        /// Returns the count of the key. If key does not exist, returns 0.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>count of the key</returns>
        public int Get(T key)
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null.");
            }
            int slot = Hash(key) & modulo;
            while (true)
            {
                T t = keys[slot];
                if (t == null)
                {
                    return 0;
                }
                if (Object.ReferenceEquals(t, TombStone))
                {
                    slot = (slot + 1) & modulo;
                    continue;
                }
                if (t.Equals(key))
                {
                    return (int)values[slot];
                }
                slot = (slot + 1) & modulo;
            }
        }

        /// <summary>
        /// Decrements the objects count.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value after decrement.</returns>
        public int Decrement(T key)
        {
            return IncrementByAmount(key, -1);
        }

        /// <summary>
        /// addOrIncrement the value by "amount". If value does not exist, it a applies set() operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="amount">amount amount to addOrIncrement</param>
        /// <returns>incremented value</returns>
        public int IncrementByAmount(T key, int amount)
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null.");
            }
            if (keyCount + removeCount == threshold)
            {
                Expand();
            }
            int l = Locate(key);
            if (l < 0)
            {
                l = -l - 1;
                values[l] = amount;
                keys[l] = key;
                keyCount++;
                return (int)values[l];
            }
            else
            {
                values[l] += amount;
                return (int)values[l];
            }
        }

        private void Expand()
        {
            IntValueMap<T> h = new IntValueMap<T>(NewSize());
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    h.Put(keys[i], (int)values[i]);
                }
            }
            ExpandCopyParameters(h);
            this.values = h.values;
        }

        public void Put(T key, int value)
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null.");
            }
            if (keyCount + removeCount == threshold)
            {
                Expand();
            }
            int loc = Locate(key);
            if (loc >= 0)
            {
                values[loc] = value;
            }
            else
            {
                loc = -loc - 1;
                keys[loc] = key;
                values[loc] = value;
                keyCount++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> a clone of value array.</returns>
        public int[] CopyOfValues()
        {
            int[] result = new int[keyCount];
            int k = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    result[k] = (int)values[i];
                    k++;
                }
            }
            return result;
        }

        public List<Entry<T>> GetAsEntryList()
        {
            List<Entry<T>> res = new List<Entry<T>>(keyCount);
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    res.Add(new Entry<T>(keys[i], (int)values[i]));
                }
            }
            return res;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EntryIterator(this) as IEnumerator<T>;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EntryIterator(this);
        }

        public class Entry<X> : IComparable<Entry<X>>
        {

            public readonly X key;
            public readonly int count;

            public Entry(X key, int count)
            {
                this.key = key;
                this.count = count;
            }

            public int CompareTo(Entry<X> other)
            {
                return other.count.CompareTo(count);
            }

            public override string ToString()
            {
                return key.ToString() + ":" + count;
            }
        }

        private class EntryIterator : IEnumerator<T>
        {

            int i;
            int k;
            public T Current { get; set; }

            private IntValueMap<T> iterator;

            public EntryIterator(IntValueMap<T> iterator)
            {
                this.iterator = iterator;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }


            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (HasNext())
                {
                    while (!iterator.HasValidKey(i))
                    {
                        i++;
                    }
                    Entry<T> te = new Entry<T>(iterator.keys[i], (int)iterator.values[i]);
                    i++;
                    k++;
                    Current = te.key;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                i = 0;
                k = 0;
                Current = default(T);
            }

            public bool HasNext()
            {
                return k < iterator.keyCount;
            }
        }
    }
}