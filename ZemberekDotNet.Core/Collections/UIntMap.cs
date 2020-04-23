using System;
using System.Collections;
using System.Collections.Generic;

namespace ZemberekDotNet.Core.Collections
{
    public class UIntMap<T> : UIntKeyHashBase, IEnumerable<T>
    {
        private T[] values;

        public UIntMap() : this(InitialSize)
        {
        }

        public UIntMap(int size) : base(size)
        {
            values = new T[keys.Length];
        }

        /// <summary>
        /// Returns the value for the key. If key does not exist, returns 0.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>count of the key</returns>
        public T Get(int key)
        {
            if (key < 0)
            {
                throw new ArgumentException($"Key cannot be negative: {key}");
            }
            int slot = Hash(key) & modulo;
            while (true)
            {
                int t = keys[slot];
                if (t == Empty)
                {
                    return default(T);
                }
                if (t == Deleted)
                {
                    slot = (slot + 1) & modulo;
                    continue;
                }
                if (t == key)
                {
                    return values[slot];
                }
                slot = (slot + 1) & modulo;
            }
        }

        public new bool ContainsKey(int key)
        {
            return Locate(key) >= 0;
        }

        private void Expand()
        {
            UIntMap<T> h = new UIntMap<T>(values.Length * 2);
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] >= 0)
                {
                    h.Put(keys[i], values[i]);
                }
            }
            CopyParameters(h);
            this.values = h.values;
        }

        /// <summary>
        /// puts `key` with `value`. if `key` already exists, it overwrites its value with `value`
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Put(int key, T value)
        {
            if (key < 0)
            {
                throw new ArgumentException("Key cannot be negative: " + key);
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

        public List<T> GetValues()
        {
            List<T> result = new List<T>();
            for (int i = 0; i < keys.Length; i++)
            {
                int key = keys[i];
                if (key >= 0)
                {
                    result.Add(values[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// returns the values sorted ascending.
        /// </summary>
        /// <returns></returns>
        public List<T> GetValuesSortedByKey()
        {
            int[] sortedKeys = GetKeyArraySorted();
            List<T> result = new List<T>(sortedKeys.Length);
            foreach (int sortedKey in sortedKeys)
            {
                result.Add(Get(sortedKey));
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ValueIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ValueIterator(this);
        }

        private class ValueIterator : IEnumerator<T>
        {
            private int keyCounter = 0;
            private int counter = 0;
            public T Current { get; set; }

            private UIntMap<T> iterator;

            public ValueIterator(UIntMap<T> iterator)
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
                return HasNext();
            }

            public void Reset()
            {
                keyCounter = 0;
                counter = 0;
                Current = default(T);
            }

            public bool HasNext()
            {
                if (counter == iterator.keyCount)
                {
                    return false;
                }
                while (true)
                {
                    if (iterator.keys[keyCounter] >= 0)
                    {
                        keyCounter++;
                        break;
                    }
                    keyCounter++;
                }
                Current = iterator.values[keyCounter - 1];
                counter++;
                return true;
            }
        }
    }
}
