using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Native.Collections
{
    /// <summary>
    /// Sorted map abstraction
    /// </summary>
    /// <typeparam name="K">Key data type</typeparam>
    /// <typeparam name="V">Value data type</typeparam>
    public interface ISortedMap<K, V> : IMap<K, V> where K : IComparable<K> where V : IComparable<V>
    {

        /// <summary>
        /// Returns the first key in this map
        /// </summary>
        /// <returns>first key</returns>
        K FirstKey();

        /// <summary>
        /// Returns all entries that are strictly less than the specified key
        /// </summary>
        /// <param name="to">key up to which to collect elements</param>
        /// <returns>map containing entries</returns>
        ISortedMap<K, V> HeadMap(K to);

        /// <summary>
        /// Returns the last key in this map
        /// </summary>
        /// <returns>last key</returns>
        K LastKey();

        /// <summary>
        /// Returns all entries from the map ranging from fromKey (inclusive) to toKey (exclusive)
        /// </summary>
        /// <param name="fromKey"></param>
        /// <param name="toKey"></param>
        /// <returns>map containing entries</returns>
        ISortedMap<K, V> SubMap(K fromKey, K toKey);

        /// <summary>
        /// Returns all entries from the map that are greater or equal to the specified key
        /// </summary>
        /// <param name="from">key to start collecting elements from</param>
        /// <returns>map containing elements</returns>
        ISortedMap<K, V> TailMap(K from);
    }
}
