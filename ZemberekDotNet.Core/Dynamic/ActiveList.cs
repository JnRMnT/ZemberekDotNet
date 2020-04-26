using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Dynamic
{
    public class ActiveList<T> : IEnumerable<T> where T : IScorable
    {
        private static float DefaultLoadFactor = 0.55f;
        private static int DefaultInitialCapacity = 8;

        private T[] items;

        private int modulo;
        private int size;
        private int expandLimit;

        public ActiveList() : this(DefaultInitialCapacity)
        {
        }

        public ActiveList(int size)
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
            items = new IScorable[k] as T[];
            expandLimit = (int)(k * DefaultLoadFactor);
            modulo = k - 1;
        }

        private int Rehash(int key)
        {
            int h = (int)(key * 0x9E3779B9);
            return (h ^ (h >> 16)) & 0x7fff_ffff;
        }

        /**
         * Finds either an empty slot location in Hypotheses array or the location of an equivalent item.
         * If an empty slot is found, it returns -(slot index)-1, if an equivalent item is found, returns
         * equal item's slot index.
         */
        private int Locate(T t)
        {
            int slot = Rehash(t.GetHashCode()) & modulo;
            while (true)
            {
                IScorable h = items[slot];
                if (h == null)
                {
                    return (-slot - 1);
                }
                if (h.Equals(t))
                {
                    return slot;
                }
                slot = (slot + 1) & modulo;
            }
        }

        /**
         * Adds a new scorable to the list.
         **/
        public void Add(T t)
        {

            int slot = Locate(t);

            // if not exist, add.
            if (slot < 0)
            {
                slot = -slot - 1;
                items[slot] = t;
                size++;
            }
            else
            {
                // If exist, check score and if score is better, replace it.
                if (items[slot].GetScore() < t.GetScore())
                {
                    items[slot] = t;
                }
            }
            if (size == expandLimit)
            {
                Expand();
            }
        }

        private void Expand()
        {
            ActiveList<T> expandedList = new ActiveList<T>(items.Length * 2);
            // put items to new list.
            for (int i = 0; i < items.Length; i++)
            {
                T t = items[i];
                if (t == null)
                {
                    continue;
                }
                int slot = Rehash(t.GetHashCode()) & modulo;
                while (true)
                {
                    T h = expandedList.items[slot];
                    if (h == null)
                    {
                        expandedList.items[slot] = t;
                        break;
                    }
                    slot = (slot + 1) & modulo;
                }
            }
            this.modulo = expandedList.modulo;
            this.expandLimit = expandedList.expandLimit;
            this.items = expandedList.items;
        }

        public T GetBest()
        {
            T best = default(T);
            foreach (T t in items)
            {
                if (t == null)
                {
                    continue;
                }
                if (best == null || t.GetScore() > best.GetScore())
                {
                    best = t;
                }
            }
            return best;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new TIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class TIterator : IEnumerator<T>
        {

            int pointer = 0;
            int count = 0;
            ActiveList<T> owner;

            public TIterator(ActiveList<T> owner)
            {
                this.owner = owner;
            }

            public T Current { get; set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                return true;
            }

            public void Reset()
            {
                pointer = 0;
                count = 0;
                Current = default(T);
            }

            public bool HasNext()
            {
                if (count == owner.size)
                {
                    return false;
                }

                while (owner.items[pointer] == null)
                {
                    pointer++;
                }
                Current = owner.items[pointer];
                count++;
                pointer++;
                return true;
            }
        }
    }
}

