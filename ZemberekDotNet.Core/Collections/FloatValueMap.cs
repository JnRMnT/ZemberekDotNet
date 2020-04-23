using System;
using System.Collections;
using System.Collections.Generic;
using ZemberekDotNet.Core.Native.Helpers;

namespace ZemberekDotNet.Core.Collections
{
    public class FloatValueMap<T> : HashBase<T>, IEnumerable<T>
    {
        // Carries count values.
        private float[] values;

        public FloatValueMap() : this(InitialSize)
        {

        }

        public FloatValueMap(int size) : base(size)
        {
            values = new float[keys.Length];
        }

        private FloatValueMap(FloatValueMap<T> other, T[] keys, float[] values) : base(other, keys)
        {
            this.values = values;
        }

        /// <summary>
        /// Returns the count of the key. If key does not exist, returns 0.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>count of the key</returns>
        public float Get(T key)
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
                if (Object.ReferenceEquals(t,TombStone))
                {
                    slot = (slot + 1) & modulo;
                    continue;
                }
                if (t.Equals(key))
                {
                    return values[slot];
                }
                slot = (slot + 1) & modulo;
            }
        }
        
        /// <summary>
        /// AddOrIncrement the value by "amount". If value does not exist, it a applies Set() operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="amount">amount amount to AddOrIncrement</param>
        /// <returns>incremented value</returns>
        public float IncrementByAmount(T key, float amount)
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
                return values[l];
            }
            else
            {
                values[l] += amount;
                return values[l];
            }
        }

        private void Expand()
        {
            FloatValueMap<T> h = new FloatValueMap<T>(NewSize());
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    h.Set(keys[i], values[i]);
                }
            }
            ExpandCopyParameters(h);
            this.values = h.values;
        }

        /// <summary>
        /// Sets the key with the value. If there is a matching key, it overwrites it (key and the value).
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Set(T key, float value)
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
                keys[loc] = key;
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

        public float[] Values()
        {
            float[] result = new float[Size()];
            int j = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    result[j++] = values[i];
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
                    res.Add(new Entry<T>(keys[i], values[i]));
                }
            }
            return res;
        }

        public FloatValueMap<T> Copy()
        {
            return new FloatValueMap<T>(this, (T[])keys.Clone(), (float[])values.Clone());
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EntryIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EntryIterator(this);
        }

        public class Entry<X> : IComparable<Entry<X>>
        {

            public readonly T key;
            public readonly float value;

            public Entry(T key, float value)
            {
                this.key = key;
                this.value = value;
            }

            public int CompareTo(Entry<X> other)
            {
                return value.CompareTo(other.value);
            }
        }

        private class EntryIterator : IEnumerator<T>
        {

            int i;
            int k;

            public T Current { get; set; }

            private FloatValueMap<T> iterator;

            public EntryIterator(FloatValueMap<T> iterator)
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
                    Entry<T> te = new Entry<T>(iterator.keys[i], iterator.values[i]);
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
