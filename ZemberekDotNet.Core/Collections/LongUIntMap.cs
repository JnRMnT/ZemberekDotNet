using System;
using System.Diagnostics.Contracts;

namespace ZemberekDotNet.Core.Collections
{
    public class LongUIntMap
    {
        public static readonly int EmptyValue = -1;
        public static readonly int DeletedValue = -2;
        static readonly int InitialSize = 8;
        static readonly double DefaultLoadFactor = 0.55;
        // Key array.
        long[] keys;
        // Carries unsigned int values.
        int[] values;
        int keyCount;
        int removeCount;
        // When structure has this amount of keys, it expands the key and count arrays.
        int threshold = (int)(InitialSize * DefaultLoadFactor);
        // This is the size-1 of the key and value array length. Array length is a value power of two
        private int modulo = InitialSize - 1;

        public LongUIntMap(): this(InitialSize)
        {

        }

        public LongUIntMap(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("Size must be a positive value. But it is " + size);
            }
            int k = 1;
            while (k < size)
            {
                k <<= 1;
            }
            keys = new long[k];
            values = new int[k];
            Array.Fill(values, -1);
            threshold = (int)(k * DefaultLoadFactor);
            modulo = k - 1;
        }

        private int Hash(long key)
        {
            return key.GetHashCode();
        }


        private int Locate(long key)
        {
            int slot = Hash(key) & modulo;
            int pointer = -1;
            while (true)
            {
                int t = values[slot];
                if (t == EmptyValue)
                {
                    return pointer < 0 ? (-slot - 1) : (-pointer - 1);
                }
                if (t == DeletedValue)
                {
                    if (pointer < 0)
                    {
                        pointer = slot;
                    }
                    slot = (slot + 1) & modulo;
                    continue;
                }
                if (key == keys[slot])
                {
                    return slot;
                }
                slot = (slot + 1) & modulo;
            }
        }

        /**
         * If key does not exist, it adds it with count value 1. Otherwise, it increments the count value
         * by 1.
         *
         * @param key key
         * @return the new count value after addOrIncrement
         */
        public int Increment(long key)
        {
            return IncrementByAmount(key, 1);
        }

        /**
         * Returns the count of the key. If key does not exist, returns 0.
         *
         * @param key key
         * @return count of the key
         */
        public int Get(long key)
        {
            int slot = Hash(key) & modulo;

            while (true)
            {
                int t = values[slot];
                if (t == EmptyValue)
                {
                    return -1;
                }
                if (t == DeletedValue)
                {
                    slot = (slot + 1) & modulo;
                    continue;
                }
                if (keys[slot] == key)
                {
                    return values[slot];
                }
                slot = (slot + 1) & modulo;
            }
        }

        public int Decrement(long key)
        {
            return IncrementByAmount(key, -1);
        }

        public bool ContainsKey(long key)
        {
            return Locate(key) >= 0;
        }

        /**
         * addOrIncrement the value by "amount". If value does not exist, it a applies set() operation.
         *
         * @param key key
         * @param amount amount to addOrIncrement
         * @return incremented value
         */
        public int IncrementByAmount(long key, int amount)
        {
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
                if (values[l] < 0)
                {
                    throw new InvalidOperationException(
                        "Negative Value calculated after incrementing with " + amount);
                }
                return values[l];
            }
        }

        public void Remove(long key)
        {
            int k = Locate(key);
            if (k < 0)
            {
                return;
            }
            values[k] = DeletedValue; // mark deletion
            keyCount--;
            removeCount++;
        }

        private void Expand()
        {
            LongUIntMap h = new LongUIntMap(values.Length * 2);
            for (int i = 0; i < keys.Length; i++)
            {
                if (values[i] != EmptyValue && values[i] != DeletedValue)
                {
                    h.Put(keys[i], values[i]);
                }
            }
            Contract.Requires(h.keyCount == keyCount);
            this.values = h.values;
            this.keys = h.keys;
            this.keyCount = h.keyCount;
            this.modulo = h.modulo;
            this.threshold = h.threshold;
            this.removeCount = 0;
        }

        public void Put(long key, int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Cannot put negative value = " + value);
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
        /// <returns>amount of keys</returns>
        public int Size()
        {
            return keyCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>a clone of value array.</returns>
        public int[] CopyOfValues()
        {
            return (int[])values.Clone();
        }

        public long[] KeyArray()
        {
            long[] keys = new long[Size()];
            int j = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (values[i] != EmptyValue || values[i] != DeletedValue)
                {
                    keys[j++] = keys[i];
                }
            }
            return keys;
        }
    }
}
