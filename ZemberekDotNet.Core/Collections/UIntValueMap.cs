using System;
using System.Collections;
using System.Collections.Generic;

namespace ZemberekDotNet.Core.Collections
{
    public class UIntValueMap<T> : HashBase<T>, IEnumerable<T>
    {
        // Carries unsigned integer values.
        private int[] values;
        public UIntValueMap() : this(InitialSize)
        {
        }

        public UIntValueMap(int size) : base(size)
        {
            values = new int[keys.Length];
        }

        /// <summary>
        /// Returns the count of the key. If key does not exist, returns -1.
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
                T t = (T)keys[slot];
                if (t == null)
                {
                    return -1;
                }
                if (Object.ReferenceEquals(t, TombStone))
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

        private void Expand()
        {
            UIntValueMap<T> h = new UIntValueMap<T>(NewSize());
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    h.Put((T)keys[i], values[i]);
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
        public void Put(T key, int value)
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null.");
            }
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be negative : " + value);
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

        /// <summary>
        /// If key does not exist, it adds it with count value 1. Otherwise, it increments the count value
        /// by 1.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>the new value after addOrIncrement</returns>
        public int Increment(T key)
        {
            return IncrementByAmount(key, 1);
        }

        /// <summary>
        /// Adds all keys in Iterable. If key does not exist, it adds it with count value 1. Otherwise, it
        /// increments the count value by 1.
        /// </summary>
        /// <param name="keys">key</param>
        public void IncrementAll(IEnumerable<T> keys)
        {
            foreach (T t in keys)
            {
                IncrementByAmount(t, 1);
            }
        }

        public int Decrement(T key)
        {
            return IncrementByAmount(key, -1);
        }

        /// <summary>
        /// AddOrIncrement the value by "amount". If value does not exist, it a applies set() operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="amount">amount amount to AddOrIncrement</param>
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
                if (values[l] < 0)
                {
                    throw new InvalidOperationException("Value reached to negative.");
                }
                return values[l];
            }
            else
            {
                values[l] += amount;
                if (values[l] < 0)
                {
                    throw new InvalidOperationException("Value reached to negative.");
                }
                return values[l];
            }
        }

        public List<IntValueMap<T>.Entry<T>> GetAsEntryList()
        {
            List<IntValueMap<T>.Entry<T>> res = new List<IntValueMap<T>.Entry<T>>(keyCount);
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    res.Add(new IntValueMap<T>.Entry<T>((T)keys[i], values[i]));
                }
            }
            return res;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EntryIterator(this);
        }

        /// <summary>
        /// counts the items those values are smaller than amount
        /// </summary>
        /// <param name="amount">amount amount to check size</param>
        /// <returns>count.</returns>
        public int SizeLarger(int amount)
        {
            int count = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i) && values[i] > amount)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// counts the items those values are smaller than amount
        /// </summary>
        /// <param name="amount">amount amount to check size</param>
        /// <returns>count.</returns>
        public int SizeSmaller(int amount)
        {
            int count = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i) && values[i] < amount)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// returns the max value.
        /// </summary>
        /// <returns>the max value in the map. If map is empty, returns 0.</returns>
        public int MaxValue()
        {
            int max = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i) && values[i] > max)
                {
                    max = values[i];
                }
            }
            return max;
        }

        /// <summary>
        /// returns the min value.
        /// </summary>
        /// <returns>the min value in the map. If map is empty, returns 0.</returns>
        public int MinValue()
        {
            int min = int.MaxValue;
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i) && values[i] < min)
                {
                    min = values[i];
                }
            }
            return min;
        }

        public long SumOfValues(int minValue, int maxValue)
        {
            long sum = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                int value = values[i];
                if (HasValidKey(i) && value >= minValue && value <= maxValue)
                {
                    sum += value;
                }
            }
            return sum;
        }

        public int[] ValueArray()
        {
            int[] result = new int[Size()];
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

        public long SumOfValues()
        {
            long sum = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (HasValidKey(i))
                {
                    sum += values[i];
                }
            }
            return sum;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (IEnumerator<T>)new EntryIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EntryIterator(this);
        }

        public class Entry<X> : IComparable<IntValueMap<X>.Entry<X>>
        {

            public readonly X key;
            public readonly int count;

            public Entry(X key, int count)
            {
                this.key = key;
                this.count = count;
            }

            public int CompareTo(Collections.IntValueMap<X>.Entry<X> other)
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
            private readonly UIntValueMap<T> iterator;
            public T Current { get; set; }
            object IEnumerator.Current => Current;

            public EntryIterator(UIntValueMap<T> iterator)
            {
                this.iterator = iterator;
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

                    IntValueMap<T>.Entry<T> te = new IntValueMap<T>.Entry<T>((T)iterator.keys[i], iterator.values[i]);
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
