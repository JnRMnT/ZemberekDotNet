using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Native.Collections
{
    /// <summary>
    /// Map abstraction
    /// </summary>
    /// <typeparam name="K">Key data type</typeparam>
    /// <typeparam name="V">Value data type</typeparam>
    public interface IMap<K, V> : IEnumerable<KeyValuePair<K, V>> where K : IComparable<K> where V : IComparable<V>
    {

        /// <summary>
        /// Clears the map
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks if the map contains key
        /// </summary>
        /// <param name="key">key to look for</param>
        /// <returns>true if key exists, false otherwise</returns>
        bool ContainsKey(K key);

        /// <summary>
        /// Checks if the map contains value
        /// </summary>
        /// <param name="value">value to look for</param>
        /// <returns>true if value exists, false otherwise</returns>
        bool ContainsValue(V value);

        /// <summary>
        /// Checks if the map is equal to given object
        /// </summary>
        /// <param name="o">object to compare to</param>
        /// <returns>true if objects are equal</returns>
        bool Equals(object o);

        /// <summary>
        /// Gets the value associated with the given key
        /// </summary>
        /// <param name="key">key to look for</param>
        /// <returns>value associated with the key</returns>
        V Get(K key);

        /// <summary>
        /// Checks if the map is empty or not
        /// </summary>
        /// <returns>true if the map is empty, false otherwise</returns>
        bool IsEmpty();

        /// <summary>
        /// Gets all the keys contained in this map
        /// </summary>
        /// <returns>List of keys</returns>
        ICollection<K> Keys();

        /// <summary>
        /// Inserts (if key doesn't exist) or updates the value with the given key
        /// </summary>
        /// <param name="key">key to insert or update</param>
        /// <param name="value">value to insert</param>
        /// <returns>newly inserted value</returns>
        V Put(K key, V value);

        /// <summary>
        /// Inserts all keys and values from the given map to this map
        /// </summary>
        /// <param name="map">map with values to be inserted</param>
        void PutAll(IMap<K, V> map);

        /// <summary>
        /// Deletes an entry from the map
        /// </summary>
        /// <param name="key">key to delete</param>
        /// <returns>deleted value</returns>
        V Remove(K key);

        /// <summary>
        /// Gets the number of entries in this map
        /// </summary>
        /// <returns>number of entries</returns>
        int Size();

        /// <summary>
        /// Gets all the values contained in this map
        /// </summary>
        /// <returns>List of values</returns>
        ICollection<V> Values();
    }
}
