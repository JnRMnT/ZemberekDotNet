using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Collections
{
    public abstract class CompactIntMapBase
    {
        public static int NoResult = int.MinValue;
        public static int DefaultInitialCapacity = 4;
        // Special values to mark Empty and Deleted cells.
        public static int Empty = NoResult;
        public static int Deleted = Empty + 1;

        public static int MaxCapacity = 1 << 30;
        // Backing array for keys and values. Each 64 bit slot is used for storing
        // 32 bit key, value pairs.
        protected ulong[] entries;
        // Number of keys in the map = size of the map.
        protected int keyCount;
        // Number of Removed keys.
        protected int removedKeyCount;
        protected int threshold;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity">initial internal array size for capacity amount of key - values. It must be a
        /// positive number. If value is not a power of two, size will be the nearest larger power of two.</param>
        public CompactIntMapBase(int capacity)
        {
            capacity = NearestPowerOf2Capacity(capacity, MaxCapacity);
            entries = new ulong[capacity];
            Array.Fill(entries, (ulong)Empty);
            threshold = (int)(capacity * CalculateLoadFactor(capacity));
        }

        static int NearestPowerOf2Capacity(int capacity, int maxCapacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentException("Capacity must be > 0: " + capacity);
            }
            long k = 1;
            while (k < capacity)
            {
                k <<= 1;
            }
            if (k > maxCapacity)
            {
                throw new ArgumentException("Map too large: " + capacity);
            }
            return (int)k;
        }

        static float CalculateLoadFactor(int capacity)
        {
            // Note: Never return 1.0 as load factor. Backing array should have
            // at least one Empty slot.
            if (capacity <= 4)
            {
                return 0.9f;
            }
            else if (capacity <= 16)
            {
                return 0.75f;
            }
            else if (capacity <= 128)
            {
                return 0.70f;
            }
            else if (capacity <= 512)
            {
                return 0.65f;
            }
            else if (capacity <= 2048)
            {
                return 0.60f;
            }
            else
            {
                return 0.5f;
            }
        }

        public int Capacity()
        {
            return entries.Length;
        }

        public int Size()
        {
            return keyCount;
        }

        /// <summary>
        ///  Map capacity is always a power of 2. With this property, integer modulo operation (key %
        ///  capacity) can be replaced with (key & (capacity - 1)).
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int FirstProbe(int key)
        {
            return Rehash(key) & (entries.Length - 1);
        }

        public int Probe(int slot)
        {
            return (slot + 1) & (entries.Length - 1);
        }

        public int Rehash(int hash)
        {
            // 0x9E3779B9 is int phi, it has some nice distributing characteristics.
            int h = (int)(hash * 0x9E3779B9);
            return (int)(h ^ (h >> 16));
        }

        public void CheckKey(int key)
        {
            if (key <= Deleted)
            {
                throw new ArgumentException("Illegal key: " + key);
            }
        }

        public int GetKey(int i)
        {
            return (int)(entries[i] & 0xFFFF_FFFFL);
        }

        public void SetKey(int i, int key)
        {
            entries[i] = (ulong)(((int)(entries[i] & 0xFFFF_FFFF_0000_0000L)) | key);
        }

        public bool ContainsKey(int key)
        {
            return Locate(key) >= 0;
        }

        public bool HasKey(int i)
        {
            return GetKey(i) > Deleted;
        }

        public void ExpandIfNecessary()
        {
            if (keyCount + removedKeyCount >= threshold)
            {
                Expand();
            }
        }

        // Only marks the slot as Deleted. In get and locate methods, Deleted slots are skipped.
        public void Remove(int key)
        {
            CheckKey(key);
            int loc = Locate(key);
            if (loc >= 0)
            {
                SetKey(loc, Deleted);
                removedKeyCount++;
                keyCount--;
            }
        }

        // This method is only used during expansion of the map. New capacity is calculated as
        // old capacity * 2
        public int NewCapacity()
        {
            int newCapacity = NearestPowerOf2Capacity(Capacity(), MaxCapacity) * 2;
            if (newCapacity > MaxCapacity)
            {
                throw new SystemException("Map size is too large.");
            }
            return newCapacity;
        }

        public int Locate(int key)
        {
            int slot = FirstProbe(key);
            while (true)
            {
                int k = GetKey(slot);
                // If slot is Empty, return its location
                // return -slot -1 to tell that slot is Empty, -1 is for slot = 0.
                if (k == Empty)
                {
                    return -slot - 1;
                }
                if (k == key)
                {
                    return slot;
                }
                // Deleted slots are ignored.
                slot = Probe(slot);
            }
        }

        /// <summary>
        /// The array of keys in the map. Not ordered.
        /// </summary>
        /// <returns></returns>
        public int?[] GetKeys()
        {
            int?[] keyArray = new int?[keyCount];
            int c = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                if (HasKey(i))
                {
                    keyArray[c++] = GetKey(i);
                }
            }
            return keyArray;
        }
        public abstract void Expand();
    }
}
