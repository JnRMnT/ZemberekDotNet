using System.Collections.Generic;

namespace ZemberekDotNet.Core.Native.Collections
{
    public class MultiMap<X,V>
    {
        Dictionary<X, List<V>> _dictionary =
            new Dictionary<X, List<V>>();

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

        public IEnumerable<X> Keys
        {
            get
            {
                // Get all keys.
                return this._dictionary.Keys;
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
    }
}
