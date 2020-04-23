using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    ///  A simple hashmap with integer keys and float values. Implements open address linear probing
    ///  algorithm. Constraints: <pre>
    ///  - Supports int key values in range (Integer.MIN_VALUE+1..Integer.MAX_VALUE];
    ///  - Does not implement Map interface
    ///  - Capacity can be max 1 << 30
    ///  - Load factor is 0.5.
    ///  - Max size is 2^29 (~537M elements)
    ///  - Does not implement Iterable.
    ///  - Class is not thread safe.
    ///  </pre>
    /// </summary>
    public class IntFloatMap : CompactIntMapBase
    {
        public IntFloatMap() : this(DefaultInitialCapacity)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity">capacity initial internal array size for capacity amount of key - values. It must be a
        /// positive number. If value is not a power of two, size will be the nearest larger power of two.</param>
        public IntFloatMap(int capacity) : base(capacity)
        {

        }

        private void SetValue(int i, float value)
        {
            entries[i] = ((entries[i] & 0x0000_0000_FFFF_FFFFL) | ((((ulong)value.ToIntBits()) << 32)));
        }

        private void SetKeyValue(int i, int key, float value)
        {
            entries[i] = ((ulong)key & 0xFFFF_FFFFL) | (((ulong)value.ToIntBits()) << 32);
        }

        private float GetValue(int i)
        {
            return ((int)(entries[i] >> 32)).ToFloatFromBits();
        }

        public void Put(int key, float value)
        {
            CheckKey(key);
            ExpandIfNecessary();
            int loc = Locate(key);
            if (loc >= 0)
            {
                SetValue(loc, value);
            }
            else
            {
                SetKeyValue(-loc - 1, key, value);
                keyCount++;
            }
        }

        /// <summary>
        /// Used only when expanding.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void PutSafe(int key, float value)
        {
            int loc = FirstProbe(key);
            while (true)
            {
                if (GetKey(loc) == Empty)
                {
                    SetKeyValue(loc, key, value);
                    return;
                }
                loc = Probe(loc);
            }
        }

        public void Increment(int key, float value)
        {
            CheckKey(key);
            ExpandIfNecessary();
            int loc = Locate(key);
            if (loc >= 0)
            {
                SetValue(loc, value + GetValue(loc));
            }
            else
            {
                SetKeyValue(-loc - 1, key, value);
                keyCount++;
            }
        }

        /// <summary>
        /// The value that is mapped to given key or NoResult
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float Get(int key)
        {
            CheckKey(key);
            int slot = FirstProbe(key);
            while (true)
            {
                ulong entry = entries[slot];
                int t = (int)(entry & 0xFFFF_FFFFL);
                if (t == key)
                {
                    return ((int)(entry >> 32)).ToFloatFromBits();
                }
                if (t == Empty)
                {
                    return NoResult;
                }
                slot = Probe(slot);
                // DELETED slots are skipped.
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The array of values in the map. Not ordered.</returns>
        public float[] GetValues()
        {
            float[] valueArray = new float[keyCount];
            for (int i = 0, j = 0; i < entries.Length; i++)
            {
                if (HasKey(i))
                {
                    valueArray[j++] = GetValue(i);
                }
            }
            return valueArray;
        }

        /// <summary>
        /// Resize backing arrays. If there are no removed keys, doubles the capacity.
        /// </summary>
        public override void Expand()
        {
            int capacity = NewCapacity();
            IntFloatMap h = new IntFloatMap(capacity);
            for (int i = 0; i < entries.Length; i++)
            {
                if (HasKey(i))
                {
                    h.PutSafe(GetKey(i), GetValue(i));
                }
            }
            this.entries = h.entries;
            this.removedKeyCount = 0;
            this.threshold = h.threshold;
        }
    }
}
