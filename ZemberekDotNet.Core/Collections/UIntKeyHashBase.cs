using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// Base class for various specialized hash table like data structures that uses unsigned integer keys.
    /// </summary>
    public abstract class UIntKeyHashBase
    {
        protected static readonly int InitialSize = 4;
        public static readonly int Empty = -1;
        public static readonly int Deleted = -2;
        private static readonly double LoadFactor = 0.55;
        // Array length is a value power of two, so we can use x & modulo instead of
        // x % size to calculate the slot
        protected int modulo;
        protected int[] keys;

        protected int keyCount;
        protected int removeCount;

        // When structure has this amount of keys, it expands the key and count arrays.
        protected int threshold;

        public UIntKeyHashBase(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException($"Size must be a positive value. But it is {size}");
            }
            int k = 1;
            while (k < size)
            {
                k <<= 1;
            }
            keys = new int[k];
            Array.Fill<int>(keys, Empty);
            threshold = (int)(k * LoadFactor);
            modulo = k - 1;
        }

        protected int Hash(int key)
        {
            int h = (int)(key * 0x9E3779B9);
            return (int)(h ^ ((uint)h >> 16)) & 0x7fff_ffff;
        }

        protected int Locate(int key)
        {

            int slot = Hash(key) & modulo;
            int pointer = -1;
            while (true)
            {
                int k = keys[slot];
                if (k == Empty)
                {
                    return pointer < 0 ? (-slot - 1) : (-pointer - 1);
                }
                if (k == Deleted)
                {
                    if (pointer < 0)
                    {
                        pointer = slot;
                    }
                    slot = (slot + 1) & modulo;
                    continue;
                }
                if (k == key)
                {
                    return slot;
                }
                slot = (slot + 1) & modulo;
            }
        }

        public bool ContainsKey(int key)
        {
            return Locate(key) >= 0;
        }

        /// <summary>
        /// Removes the key.
        /// </summary>
        /// <param name="key">Key to remove</param>
        public void Remove(int key)
        {
            int k = Locate(key);
            if (k < 0)
            {
                return;
            }
            keys[k] = Deleted;
            keyCount--;
            removeCount++;
        }

        public void CopyParameters(UIntKeyHashBase h)
        {
            Contract.Assert(h.keyCount == keyCount);
            this.keys = h.keys;
            this.modulo = h.modulo;
            this.threshold = h.threshold;
            this.removeCount = 0;
        }

        public int NewSize()
        {
            // we do not directly expand by [key capacity * 2] because there may be many removed keys.
            // For such cases, actually array should be shrunk.
            long t = keyCount * 2;
            if (t == 0)
            {
                t = 1;
            }
            if (t > threshold)
            {
                t = threshold;
            }

            long size = 1;
            while (size <= t)
            {
                size = size * 2;
            }
            size = size * 2;

            if (size > int.MaxValue)
            {
                throw new InvalidOperationException("Too many items in collection " + this.GetType());
            }
            return (int)size;
        }

        public int Size()
        {
            return keyCount;
        }

        /// <summary>
        /// Returns the keys sorted ascending.
        /// </summary>
        /// <returns></returns>
        public List<int> GetKeysSorted()
        {
            List<int> keyList = new List<int>();
            foreach (int key in keys)
            {
                if (key >= 0)
                {
                    keyList.Add(key);
                }
            }
            keyList.Sort();
            return keyList;
        }

        /// <summary>
        /// Returns the keys sorted ascending.
        /// </summary>
        /// <returns></returns>
        public int[] GetKeyArraySorted()
        {
            int[] sorted = GetKeys();
            Array.Sort(sorted);
            return sorted;
        }

        /// <summary>
        /// Returns the keys in an array.
        /// </summary>
        /// <returns></returns>
        public int[] GetKeys()
        {
            int[] keyArray = new int[keyCount];
            int c = 0;
            foreach (int key in keys)
            {
                if (key >= 0)
                {
                    keyArray[c++] = key;
                }
            }
            Contract.Assert(c == keyArray.Length);
            return keyArray;
        }
    }
}
