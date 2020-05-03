using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// A simple hashmap with integer keys and T values. implements open address linear probing
    /// algorithm.
    /// <p> Constraints:
    /// <pre>
    /// - Supports int key values in range (int.MinValue..int.MaxValue];
    /// - Does not implement Map interface
    /// - Size can be max 1 << 29
    /// - Does not support remove.
    /// - Does not implement Iterable.
    /// - Class is not thread safe.
    /// </pre>
    /// If created as an externally managed IntMap, it does not expand automatically when capacity
    /// threshold is reached, and must be expanded by callers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntMap<T> : IEnumerable<T>
    {
        private static readonly int DefaultInitialCapacity = 4;

        /// <summary>
        /// Capacity of the map is expanded when size reaches to capacity * LoadFactor. This value is
        /// selected to fit max 5 elements to 8 and 10 elements to a 16 sized map.
        /// </summary>
        private static readonly float LoadFactor = 0.55f;

        private static readonly int MaxCapacity = 1 << 29;

        // Special value to mark empty cells.
        private static readonly int Empty = int.MinValue;

        // Backing arrays for keys and value references.
        private int[] keys;
        private T[] values;

        // Number of keys in the map = size of the map.
        private int keyCount;

        // When size reaches a threshold, backing arrays are expanded.
        private int threshold;

        /// <summary>
        ///  Map capacity is always a power of 2. With this property, integer modulo operation (key %
        ///  capacity) can be replaced with (key & (capacity - 1)) We keep (capacity - 1) value in this
        ///  variable.
        /// </summary>
        private int modulo;

        // If map is externally managed.
        private bool managed;

        public IntMap() : this(DefaultInitialCapacity, false)
        {

        }

        public IntMap(int capacity) : this(capacity, false)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity">initial internal array size. It must be a positive number. If value is not a
        /// power of two, size will be the nearest larger power of two.</param>
        /// <param name="managed"></param>
        public IntMap(int capacity, bool managed)
        {
            capacity = AdjustInitialCapacity(capacity);
            keys = new int[capacity];
            values = new T[keys.Length];
            Array.Fill(keys, Empty);
            modulo = keys.Length - 1;
            threshold = (int)(capacity * LoadFactor);
            this.managed = managed;
        }

        public static IntMap<T> CreateManaged()
        {
            return new IntMap<T>(DefaultInitialCapacity, true);
        }

        private int AdjustInitialCapacity(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentException("Capacity must > 0: " + capacity);
            }
            long k = 1;
            while (k < capacity)
            {
                k <<= 1;
            }
            if (k > MaxCapacity)
            {
                throw new ArgumentException("Map too large: " + capacity);
            }
            return (int)k;
        }

        public int Capacity()
        {
            return keys.Length;
        }

        public int Size()
        {
            return keyCount;
        }

        private void CheckKey(int key)
        {
            if (key == Empty)
            {
                throw new ArgumentException("Illegal key: " + key);
            }
        }

        public bool Put(int key, T value)
        {
            CheckKey(key);
            if (keyCount == threshold)
            {
                if (managed)
                {
                    return false;
                }
                ExpandInternal();
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
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The value that is mapped to given key or null if key does not exist</returns>
        public T Get(int key)
        {
            CheckKey(key);
            int slot = Rehash(key) & modulo;
            // Test the lucky first shot.
            if (key == keys[slot])
            {
                return values[slot];
            }
            // Continue linear probing otherwise
            while (true)
            {
                slot = (slot + 1) & modulo;
                int t = keys[slot];
                if (t == key)
                {
                    return values[slot];
                }
                if (t == Empty)
                {
                    return default(T);
                }
            }
        }

        public bool ContainsKey(int key)
        {
            return Locate(key) >= 0;
        }

        /// <summary>
        /// Returns the array of keys in the map.
        /// </summary>
        /// <returns></returns>
        public int[] GetKeys()
        {
            int[] keyArray = new int[keyCount];
            int c = 0;
            foreach (int key in keys)
            {
                if (key != Empty)
                {
                    keyArray[c++] = key;
                }
            }
            return keyArray;
        }

        /// <summary>
        ///  Returns the array of keys in the map.
        /// </summary>
        /// <returns></returns>
        public List<T> GetValues()
        {
            List<T> result = new List<T>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] >= 0)
                {
                    result.Add(values[i]);
                }
            }
            return result;
        }

        private int Rehash(int hash)
        {
            // 0x9E3779B9 is int phi, it has some nice distributing characteristics.
            int h = (int)(hash * 0x9E3779B9);
            return h ^ (int)((uint)h >> 16);
        }

        private int Locate(int key)
        {
            int slot = Rehash(key) & modulo;
            while (true)
            {
                int k = keys[slot];
                // If slot is empty, return its location
                if (k == Empty)
                {
                    return -slot - 1;
                }
                if (k == key)
                {
                    return slot;
                }
                slot = (slot + 1) & modulo;
            }
        }

        private int NewCapacity()
        {
            long size = (long)(keys.Length * 2);
            if (keys.Length > MaxCapacity)
            {
                throw new SystemException("Map is too large.");
            }
            return (int)size;
        }

        /// <summary>
        /// Expands backing arrays by doubling their capacity.
        /// </summary>
        private void ExpandInternal()
        {
            IntMap<T> expanded = ExpandAndCopy();
            this.keys = expanded.keys;
            this.values = expanded.values;
            this.threshold = expanded.threshold;
            this.modulo = expanded.modulo;
        }

        public IntMap<T> Expand()
        {
            if (!managed)
            {
                throw new InvalidOperationException("Only externally managed maps can be extended by callers.");
            }
            return ExpandAndCopy();
        }

        private IntMap<T> ExpandAndCopy()
        {
            int capacity = NewCapacity();
            IntMap<T> newMap = new IntMap<T>(capacity, managed);
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] != Empty)
                {
                    newMap.Put(keys[i], values[i]);
                }
            }
            return newMap;
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

            private IntMap<T> iterator;

            public ValueIterator(IntMap<T> iterator)
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
                    if (iterator.keys[keyCounter] != Empty)
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
