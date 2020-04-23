using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Collections
{
    public class UIntSet : UIntKeyHashBase
    {
        public UIntSet() : this(InitialSize)
        {
        }

        public UIntSet(int size) : base(size)
        {

        }

        public bool Contains(int key)
        {
            return Locate(key) >= 0;
        }

        private void Expand()
        {
            UIntSet h = new UIntSet(NewSize());
            foreach (int key in keys)
            {
                if (key >= 0)
                {
                    h.Add(key);
                }
            }
            CopyParameters(h);
        }

        public static UIntSet Of(params int[] vals)
        {
            UIntSet set = new UIntSet(vals.Length);
            foreach (int val in vals)
            {
                set.Add(val);
            }
            return set;
        }

        /// <summary>
        /// puts `key` with `value`. if `key` already exists, it overwrites its value with `value`
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Add(int key)
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
                return false;
            }
            else
            {
                loc = -loc - 1;
                keys[loc] = key;
                keyCount++;
                return true;
            }
        }

        public void AddAll(params int[] keys)
        {
            foreach (int key in keys)
            {
                Add(key);
            }
        }

        public new int Size()
        {
            return keyCount;
        }
    }
}
