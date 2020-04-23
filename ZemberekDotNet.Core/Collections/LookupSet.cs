using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// A Set-like data structure that allows looking up if an equivalent instance exists in it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LookupSet<T> : HashBase<T>, IEnumerable<T>
    {
        public LookupSet() : this(InitialSize)
        {

        }

        public LookupSet(int size) : base(size)
        {

        }

        private void Expand()
        {
            LookupSet<T> h = new LookupSet<T>(NewSize());
            foreach (T key in keys)
            {
                if (key != null && !Object.ReferenceEquals(key, TombStone))
                {
                    h.Set(key);
                }
            }
            ExpandCopyParameters(h);
        }

        /// <summary>
        /// Adds this key to Set. If there is an equivalent exist, it overrides it. if there was an
        /// equivalent key, it returns it. Otherwise returns null.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public T Set(T key)
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
                T old = keys[loc];
                keys[loc] = key;
                return old;
            }
            else
            {
                loc = -loc - 1;
                keys[loc] = key;
                keyCount++;
                return default(T);
            }
        }

        public void AddAll(params T[] t)
        {
            foreach (T t1 in t)
            {
                Add(t1);
            }
        }

        public void AddAll(IEnumerable<T> it)
        {
            foreach (T t1 in it)
            {
                Add(t1);
            }
        }

        /// <summary>
        /// If there is an equivalent object, it does nothing and returns false. Otherwise it adds the item
        /// to set and returns true.
        /// </summary>
        /// <param name="key">input</param>
        /// <returns></returns>
        public bool Add(T key)
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

        /// <summary>
        /// If there is an equivalent object, returns it. Otherwise adds it and returns the input.
        /// </summary>
        /// <param name="key">input.</param>
        /// <returns></returns>
        public T GetOrAdd(T key)
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
                return keys[loc];
            }
            else
            {
                loc = -loc - 1;
                keys[loc] = key;
                keyCount++;
                return key;
            }
        }
    }
}