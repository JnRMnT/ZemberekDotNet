using System;
using System.Collections.Generic;
using System.Linq;

namespace ZemberekDotNet.Core.Native.Collections
{
    public class MultiMap<X, V>
    {
        Dictionary<X, List<V>> _dictionary;

        public MultiMap()
        {
            _dictionary = new Dictionary<X, List<V>>();
        }

        public MultiMap(int initialSize)
        {
            _dictionary = new Dictionary<X, List<V>>(initialSize);
        }

        public void Add(X key, V value)
        {
            // Add a key.
            List<V> list;
            if (this._dictionary.TryGetValue(key, out list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<V>();
                list.Add(value);
                this._dictionary[key] = list;
            }
        }

        public void Add(MultiMap<X,V> multiMap)
        {
            foreach(X key in multiMap.Keys)
            {
                Add(key, multiMap[key]);
            }
        }

        public void Add(X key, IEnumerable<V> values)
        {
            // Add a key.
            List<V> list;
            if (this._dictionary.TryGetValue(key, out list))
            {
                list.AddRange(values);
            }
            else
            {
                list = new List<V>(values);
                this._dictionary[key] = list;
            }
        }

        public void Remove(X key, V value)
        {
            // Add a key.
            List<V> list;
            if (this._dictionary.TryGetValue(key, out list))
            {
                list.Remove(value);
                this._dictionary[key] = list;
            }
        }

        public IEnumerable<X> Keys
        {
            get
            {
                // Get all keys.
                return this._dictionary.Keys;
            }
        }
        public IEnumerable<V> Values
        {
            get
            {
                // Get all keys.
                return this._dictionary.Values.SelectMany(x => x).ToArray();
            }
        }

        public List<V> this[X key]
        {
            get
            {
                // Get list at a key.
                List<V> list;
                if (!this._dictionary.TryGetValue(key, out list))
                {
                    list = new List<V>();
                    this._dictionary[key] = list;
                }
                return list;
            }
        }

        public bool ContainsKey(X key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsEntry(X key, V value)
        {
            List<V> list;
            if (this._dictionary.TryGetValue(key, out list))
            {
                return list.Contains(value);
            }
            else
            {
                return false;
            }
        }

        public void RemoveAll(X key)
        {
            _dictionary.Remove(key);
        }
        public void Remove(V value)
        {
            foreach (var key in Keys)
            {
                while (_dictionary[key].Contains(value))
                {
                    _dictionary[key].Remove(value);
                }
            }
        }
    }
}
