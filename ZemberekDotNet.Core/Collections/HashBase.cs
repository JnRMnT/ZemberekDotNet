using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using ZemberekDotNet.Core.Native.Helpers;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// Linear probing Hash base class.
    /// </summary>
    public abstract class HashBase<T> : IEnumerable<T>
    {
        public static readonly int InitialSize = 4;
        // Used for marking slots of deleted keys.
        public static readonly object TombStone = GeneralHelper.CreateTombstone<T>();
        private static readonly double DefaultLoadFactor = 0.55;
        // Key array.
        protected T[] keys;
        public int keyCount;
        public int removeCount;
        // When structure has this amount of keys, it expands the key and count arrays.
        public int threshold;
        // This is the size-1 of the key and value array length. Array length is a value power of two
        protected int modulo;

        public HashBase(int size)
        {
            //Primitive types in c# are not nullable, nullable equivalents should be used to avoid colliding with default values
            GeneralHelper.EnsureNullable<T>();
            if (size < 1)
            {
                throw new ArgumentException("Size must be a positive value. But it is " + size);
            }
            int k = 1;
            while (k < size)
            {
                k <<= 1;
            }
            keys = new T[k];
            threshold = (int)(k * DefaultLoadFactor);
            modulo = k - 1;
        }

        protected HashBase(HashBase<T> other, T[] keys)
        {
            this.keys = keys;
            this.threshold = other.threshold;
            this.modulo = other.modulo;
            this.keyCount = other.keyCount;
            this.removeCount = other.removeCount;
        }

        public bool HasValidKey(int i)
        {
            if (i >= keys.Length || i < 0)
            {
                return false;
            }
            T key = keys[i];
            return key != null && !key.Equals(TombStone);
        }

        public void ExpandCopyParameters(HashBase<T> h)
        {
            Contract.Assert(h.keyCount == keyCount);
            this.keys = h.keys;
            this.modulo = h.modulo;
            this.threshold = h.threshold;
            this.removeCount = 0;
        }

        // TODO: here if key count is less than half of the values array should be shrunk.
        // This may happen after lots of removal operations
        public int NewSize()
        {
            long size = keys.Length * 2L;
            if (size > int.MaxValue)
            {
                throw new InvalidOperationException($"Too many items in collection {this.GetType()}");
            }
            return (int)size;
        }

        protected int Hash(T key)
        {
            int h = (int)(key.GetHashCode() * 0x9E3779B9);
            return (int)(h ^ (h >> 16)) & 0x7fff_ffff;
        }

        /**
         * locate operation does the following: - finds the slot - if there was a deleted key before
         * (key[slot]==TOMB_STONE) and pointer is not set yet (pointer==-1) pointer is set to this slot
         * index and index is incremented. This is necessary for the following problem. Suppose we add key
         * "foo" first then key "bar" with key collision. first one is put to slotindex=1 and the other
         * one is located to slot=2. Then we remove the key "foo". Now if we do not use the TOMB_STONE,
         * and want to access the value of key "bar". We would get "2" because slot will be 1 and key does
         * not exist there. that is why we use a TOMB_STONE object for marking deleted slots. So when
         * getting a value we pass the deleted slots. And when we insert, we use the first deleted slot if
         * any.
         * <pre>
         *    Key Val  Key Val  Key Val
         *     0   0    0   0    0   0
         *     foo 2    foo 2    TOMB_STONE  2
         *     0   0    bar 3    bar 3
         *     0   0    0   0    0   0
         * </pre>
         * - if there was no deleted key in that slot, check the value. if value is null then we can put
         * our key here. However, we cannot return the slot value immediately. if pointer value is set, we
         * use it as the vacant index. we do not use the slot or the pointer value itself. we use negative
         * of it, pointing the key does not exist in this list. Also we return -slot-1 or -pointer-1 to
         * avoid the 0 index problem.
         */
        protected int Locate(T key)
        {
            int slot = Hash(key) & modulo;
            int pointer = -1;
            while (true)
            {
                T t = keys[slot];
                if (t == null)
                {
                    return pointer < 0 ? (-slot - 1) : (-pointer - 1);
                }
                if (Object.ReferenceEquals(t, TombStone))
                {
                    if (pointer < 0)
                    {
                        pointer = slot; // marking the first deleted slot.
                    }
                    slot = (slot + 1) & modulo;
                    continue;
                }
                if (t.Equals(key))
                {
                    return slot;
                }
                slot = (slot + 1) & modulo;
            }
        }

        public bool Contains(T key)
        {
            return Locate(key) >= 0;
        }

        public T Remove(T key)
        {
            int k = Locate(key);
            if (k < 0)
            {
                return default(T);
            }
            T removed = keys[k];
            keys[k] = (T)TombStone; // mark deletion
            keyCount--;
            removeCount++;
            return removed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">key object.</param>
        /// <returns>the original key equal to the key, if exists. null otherwise.</returns>
        public T Lookup(T key)
        {
            if (key == null)
            {
                throw new ArgumentException("Key cannot be null.");
            }
            int k = Locate(key);
            if (k < 0)
            {
                return default(T);
            }
            return keys[k];
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
        /// <returns>amount of the hash slots.</returns>
        int Capacity()
        {
            return keys.Length;
        }

        /// <summary>
        ///  an Enumerator for keys.
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<T> Enumerator()
        {
            return new KeyEnumerator(this);
        }

        /// <summary>
        ///  keys in a list
        /// </summary>
        /// <returns>list of keys</returns>
        public List<T> GetKeyList()
        {
            List<T> res = new List<T>(keyCount);
            foreach (T key in keys)
            {
                if (key != null && !Object.ReferenceEquals(key, TombStone))
                {
                    res.Add(key);
                }
            }
            return res;
        }

        /// <summary>
        /// get keys in a set.
        /// </summary>
        /// <returns>keys</returns>
        public ISet<T> GetKeySet()
        {
            ISet<T> res = new HashSet<T>(keyCount);
            foreach (T key in keys)
            {
                if (key != null && !Object.ReferenceEquals(key, TombStone))
                {
                    res.Add(key);
                }
            }
            return res;
        }

        public override string ToString()
        {
            String keys;
            if (keyCount < 100)
            {
                keys = string.Join(",", this.Enumerator());
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                KeyEnumerator it = (KeyEnumerator)Enumerator();
                int i = 0;
                while (it.HasNext() && i < 100)
                {
                    sb.Append(it.Current.ToString());
                    if (i < 99)
                    {
                        sb.Append(", ");
                    }
                    i++;
                }
                sb.Append("...");
                keys = sb.ToString();
            }
            return "[ Size = " + Size() + " Capacity = " + Capacity() +
                " Remove Count = " + removeCount + " Modulo = " + modulo +
                " Keys = " + keys + " ]";
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return new KeyEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class KeyEnumerator : IEnumerator<T>
        {
            int i;
            int k;
            private HashBase<T> iterator;

            public KeyEnumerator(HashBase<T> iterator)
            {
                this.iterator = iterator;
            }
            public T Current { get; set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool HasNext()
            {
                if (k >= iterator.keyCount)
                {
                    return false;
                }
                while (!iterator.HasValidKey(i) && i < iterator.keys.Length)
                {
                    i++;
                }
                if (i < iterator.keys.Length)
                {
                    Current = iterator.keys[i];
                    i++;
                    k++;
                    return true;
                }
                else return false;
            }

            void IDisposable.Dispose()
            {

            }

            bool IEnumerator.MoveNext()
            {
                return HasNext();
            }

            void IEnumerator.Reset()
            {
                i = 0;
                k = 0;
            }
        }
    }
}