using System;
using System.Collections;
using System.Collections.Generic;

namespace ZemberekDotNet.Core.Native.Collections
{

    /// <summary>
    /// TreeMap implementation
    /// </summary>
    /// <typeparam name="K">Key data type</typeparam>
    /// <typeparam name="V">Value data type</typeparam>
    public class TreeMap<K, V> : ISortedMap<K, V> where K : IComparable<K> where V : IComparable<V>
    {

        // number of nodes in this tree
        protected int Count { get; private set; }

        // root node
        protected Node<K, V> Root;

        // comparator used to compare keys
        protected IComparer<K> Comparator;

        /// <summary>
        /// Default class constructor. Automatically creates a comparator to use when comparing keys
        /// </summary>
        public TreeMap()
        {
            Comparator = Comparer<K>.Create((a, b) => a.CompareTo(b));
        }

        /// <summary>
        /// Initializes the TreeMap using the default constructor and includes all elements of the collection
        /// </summary>
        /// <param name="collection"></param>
        public TreeMap(ICollection<KeyValuePair<K, V>> collection) : this()
        {
            PutAll(collection);
        }

        /// <summary>
        /// Class constructor. Sets the comparator to use when comparing keys to the specified one
        /// </summary>
        /// <param name="Comparator">comparator to use</param>
        public TreeMap(IComparer<K> Comparator)
        {
            this.Comparator = Comparator;
        }

        /// <summary>
        /// Clears the map
        /// </summary>
        public void Clear()
        {
            Root = null;
            Count = 0;
        }

        /// <summary>
        /// Checks if the map contains specified key
        /// </summary>
        /// <param name="key">key to look for</param>
        /// <returns>true if key exists, false otherwise</returns>
        public bool ContainsKey(K key)
        {
            if (ReferenceEquals(key, null))
            {
                throw new ArgumentException("Key cannot be null at ContainsKey(K key)");
            }

            Node<K, V> node = Root;

            while (!ReferenceEquals(node, null))
            {
                int compare = Comparator.Compare(key, node.Key);

                if (compare < 0)
                {
                    node = node.Left;
                    continue;
                }
                else if (compare > 0)
                {
                    node = node.Right;
                    continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the map contains specified value
        /// </summary>
        /// <param name="value">value to look for</param>
        /// <returns>true if the value exists, false otherwise</returns>
        public bool ContainsValue(V value)
        {
            if (ReferenceEquals(value, null))
            {
                throw new ArgumentException("Value cannot be null at ContainsValue(V value)");
            }

            return ContainsValueRecursive(Root, value);
        }

        /// <summary>
        /// Helper method for ContainsValue(). Checks if the value exists starting from the specified node
        /// </summary>
        /// <param name="node">node to start looking from</param>
        /// <param name="value">value to look for</param>
        /// <returns>true is the value exists in the sub-tree, false otherwise</returns>
        private bool ContainsValueRecursive(Node<K, V> node, V value)
        {
            if (ReferenceEquals(node, null))
            {
                return false;
            }

            if (node.Value.CompareTo(value) == 0)
            {
                return true;
            }

            bool left = ContainsValueRecursive(node.Left, value);
            bool right = ContainsValueRecursive(node.Right, value);

            return left ? left : right;
        }

        /// <summary>
        /// Gets the first key in this map
        /// </summary>
        /// <returns>first key</returns>
        public K FirstKey()
        {
            Node<K, V> node = Root;

            while (!ReferenceEquals(node, null))
            {
                if (ReferenceEquals(node.Left, null))
                {
                    return node.Key;
                }

                node = node.Left;
            }

            return default(K);
        }

        /// <summary>
        /// Gets the value associated with the specified key
        /// </summary>
        /// <param name="key">key to look for</param>
        /// <returns>value associated with the key, default value of V if not found</returns>
        public V Get(K key)
        {
            if (ReferenceEquals(key, null))
            {
                throw new ArgumentException("Key cannot be null at Get(K key)");
            }

            Node<K, V> node = Root;

            while (!ReferenceEquals(node, null))
            {
                int compare = Comparator.Compare(key, node.Key);

                if (compare < 0)
                {
                    node = node.Left;
                    continue;
                }
                else if (compare > 0)
                {
                    node = node.Right;
                    continue;
                }

                return node.Value;
            }

            return default(V);
        }

        /// <summary>
        /// Gets all the elements that are strictly less than the specified key
        /// </summary>
        /// <param name="to">key up to which elements will be added</param>
        /// <returns>map of elements</returns>
        public ISortedMap<K, V> HeadMap(K to)
        {
            if (ReferenceEquals(to, null))
            {
                throw new ArgumentException("Parameter to cannot be null at HeadMap(K to)");
            }

            TreeMap<K, V> map = new TreeMap<K, V>();

            AddNodesRecursive(to, Root, map, false, true);

            return map;
        }

        /// <summary>
        /// Checks if the map is empty or not
        /// </summary>
        /// <returns>true if the map is empty, false if not</returns>
        public bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        /// Gets all keys stored in this map
        /// </summary>
        /// <returns>collection of keys</returns>
        public ICollection<K> Keys()
        {
            List<K> list = new List<K>();

            AddKeysRecursive(Root, list);

            return list;
        }

        /// <summary>
        /// Helper method for Keys(). Adds all keys to the collection starting from specified node
        /// </summary>
        /// <param name="node">node to start from</param>
        /// <param name="collection">collection to add keys to</param>
        private void AddKeysRecursive(Node<K, V> node, ICollection<K> collection)
        {
            if (ReferenceEquals(node, null))
            {
                return;
            }

            AddKeysRecursive(node.Left, collection);

            collection.Add(node.Key);

            AddKeysRecursive(node.Right, collection);
        }

        /// <summary>
        /// Gets the last key in this map
        /// </summary>
        /// <returns>last key</returns>
        public K LastKey()
        {
            Node<K, V> node = Root;

            while (!ReferenceEquals(node, null))
            {
                if (ReferenceEquals(node.Right, null))
                {
                    return node.Key;
                }

                node = node.Right;
            }

            return default(K);
        }

        /// <summary>
        /// Inserts (if key doesn't exist) or updates the value with the given key
        /// </summary>
        /// <param name="key">key to insert or update</param>
        /// <param name="value">value to insert</param>
        /// <returns>newly inserted value</returns>
        public V Put(K key, V value)
        {
            if (ReferenceEquals(key, null))
            {
                throw new ArgumentException("Key cannot be null at Put(K key, V value)");
            }

            Root = PutRecursive(key, value, Root);

            return value;
        }

        /// <summary>
        /// Helper method for Put(). Recursively adds an element
        /// </summary>
        /// <param name="key">key to put</param>
        /// <param name="value">value to put</param>
        /// <param name="node">node to put to</param>
        /// <returns>node with inserted value</returns>
        private Node<K, V> PutRecursive(K key, V value, Node<K, V> node)
        {
            if (ReferenceEquals(node, null))
            {
                Count++;
                return new Node<K, V>(key, value);
            }

            int comp = Comparator.Compare(key, node.Key);

            if (comp < 0)
            {
                node.Left = PutRecursive(key, value, node.Left);

                if (Height(node.Left) - Height(node.Right) == 2)
                {
                    int comp2 = Comparator.Compare(key, node.Left.Key);
                    node = (comp2 < 0) ? RightRotation(node) : DoubleRightRotation(node);
                }
            }
            else if (comp > 0)
            {
                node.Right = PutRecursive(key, value, node.Right);

                if (Height(node.Right) - Height(node.Left) == 2)
                {
                    int comp2 = Comparator.Compare(node.Right.Key, key);
                    node = (comp2 < 0) ? LeftRotation(node) : DoubleLeftRotation(node);
                }
            }
            else
            {
                node.Value = value;
                return node;
            }

            node.Height = System.Math.Max(Height(node.Left), Height(node.Right)) + 1;

            return node;
        }

        /// <summary>
        /// Puts all elements found in the specified map to this map
        /// </summary>
        /// <param name="map">map with elements to put</param>
        public void PutAll(IMap<K, V> map)
        {
            foreach (KeyValuePair<K, V> x in map)
            {
                Put(x.Key, x.Value);
            }
        }

        /// <summary>
        /// Puts all elements found in the specified map to this map
        /// </summary>
        /// <param name="map">map with elements to put</param>
        public void PutAll(ICollection<KeyValuePair<K, V>> map)
        {
            foreach (KeyValuePair<K, V> x in map)
            {
                Put(x.Key, x.Value);
            }
        }

        /// <summary>
        /// Removes the value associated with the specified key from the map
        /// </summary>
        /// <param name="key">key to remove</param>
        /// <returns>removed value</returns>
        public V Remove(K key)
        {
            if (ReferenceEquals(key, null))
            {
                throw new ArgumentException("Key cannot be null at Remove(K key)");
            }

            Node<K, V> removed = new Node<K, V>();

            Root = RemoveRecursive(key, removed, Root);

            return removed.Value;
        }

        /// <summary>
        /// Helper method for Remove(). Recursively removes an element
        /// </summary>
        /// <param name="key">key to remove</param>
        /// <param name="removed">removed node</param>
        /// <param name="node">node to start from</param>
        /// <returns>modified node</returns>
        private Node<K, V> RemoveRecursive(K key, Node<K, V> removed, Node<K, V> node)
        {
            if (ReferenceEquals(node, null))
            {
                return null;
            }

            int compare = Comparator.Compare(key, node.Key);

            if (compare < 0)
            {
                node.Left = RemoveRecursive(key, removed, node.Left);
            }
            else if (compare > 0)
            {
                node.Right = RemoveRecursive(key, removed, node.Right);
            }
            else if (node.Left != null && node.Right != null)
            {
                Node<K, V> maxNode = GetMax(node.Left);

                removed.Key = node.Key;
                removed.Value = node.Value;

                node.Key = maxNode.Key;
                node.Value = maxNode.Value;
                node.Left = RemoveMax(node.Left);

                Count--;
            }
            else
            {
                node = (node.Left != null) ? node.Left : node.Right;
                Count--;
            }

            // re-balance the tree
            if (ReferenceEquals(node, null))
            {
                return null;
            }

            node.Height = System.Math.Max(Height(node.Left), Height(node.Right)) + 1;

            int balance = Height(node.Left) - Height(node.Right);

            if (balance > 1 && Height(node.Left.Left) - Height(node.Left.Right) >= 0)
            {
                return RightRotation(node);
            }

            if (balance > 1 && Height(node.Left.Left) - Height(node.Left.Right) < 0)
            {
                node.Left = LeftRotation(node.Left);
                return RightRotation(node);
            }

            if (balance < -1 && Height(node.Right.Left) - Height(node.Right.Right) <= 0)
            {
                return LeftRotation(node);
            }

            if (balance < -1 && Height(node.Right.Left) - Height(node.Right.Right) > 0)
            {
                node.Right = RightRotation(node.Right);
                return LeftRotation(node);
            }

            return node;
        }

        /// <summary>
        /// Gets the number of elements in this map
        /// </summary>
        /// <returns>number of elements</returns>
        public int Size()
        {
            return Count;
        }

        /// <summary>
        /// Gets all emenents ranging from fromKey (inclusive) and toKey (exclusive)
        /// </summary>
        /// <param name="fromKey">key to start from</param>
        /// <param name="toKey">key to stop at</param>
        /// <returns>map of elements</returns>
        public ISortedMap<K, V> SubMap(K fromKey, K toKey)
        {
            if (ReferenceEquals(fromKey, null) || ReferenceEquals(toKey, null))
            {
                throw new ArgumentException("No parameter can be null at SubMap(K fromKey, K toKey)");
            }

            TreeMap<K, V> map = new TreeMap<K, V>();

            AddNodesRecursiveSubMap(fromKey, toKey, Root, map);

            return map;
        }

        /// <summary>
        /// Helper method for SubMap(). Recursively adds elements to the specified map
        /// </summary>
        /// <param name="from">starting key</param>
        /// <param name="to">ending key</param>
        /// <param name="node">node to start from</param>
        /// <param name="map">map to add to</param>
        private void AddNodesRecursiveSubMap(K from, K to, Node<K, V> node, ISortedMap<K, V> map)
        {
            if (ReferenceEquals(node, null))
            {
                return;
            }

            AddNodesRecursiveSubMap(from, to, node.Left, map);

            int compare1 = Comparator.Compare(from, node.Key);
            int compare2 = Comparator.Compare(to, node.Key);

            if (compare1 <= 0 && compare2 > 0)
            {
                map.Put(node.Key, node.Value);
            }

            AddNodesRecursiveSubMap(from, to, node.Right, map);
        }

        /// <summary>
        /// Gets all elements that are greater of equal to the specified element
        /// </summary>
        /// <param name="from">key to start from</param>
        /// <returns>map of elements</returns>
        public ISortedMap<K, V> TailMap(K from)
        {
            if (ReferenceEquals(from, null))
            {
                throw new ArgumentException("Parameter from cannot be null at TailMap(K from)");
            }

            TreeMap<K, V> map = new TreeMap<K, V>();

            AddNodesRecursive(from, Root, map, true, false);

            return map;
        }

        /// <summary>
        /// Helper method for TailMap() and HeadMap(). Adds elements to the specified map
        /// </summary>
        /// <param name="key">comparison key</param>
        /// <param name="node">node to start from</param>
        /// <param name="map">map to add to</param>
        /// <param name="inclusive">is the key inclusive</param>
        /// <param name="lesser">should it add lesser elements than the specified key</param>
        private void AddNodesRecursive(K key, Node<K, V> node, ISortedMap<K, V> map, bool inclusive, bool lesser)
        {
            if (ReferenceEquals(node, null))
            {
                return;
            }

            AddNodesRecursive(key, node.Left, map, inclusive, lesser);

            int compare = Comparator.Compare(key, node.Key);

            if (compare < 0 && !lesser)
            {
                map.Put(node.Key, node.Value);
            }
            else if (compare > 0 && lesser)
            {
                map.Put(node.Key, node.Value);
            }
            else if (compare == 0 && inclusive)
            {
                map.Put(node.Key, node.Value);
            }

            AddNodesRecursive(key, node.Right, map, inclusive, lesser);
        }

        /// <summary>
        /// Gets all the values stored in this map
        /// </summary>
        /// <returns>collection of values</returns>
        public ICollection<V> Values()
        {
            List<V> list = new List<V>();

            AddValuesRecursive(Root, list);

            return list;
        }

        /// <summary>
        /// Helper method for Values(). Recursively adds values to a collection starting from the specified node
        /// </summary>
        /// <param name="node">node to start from</param>
        /// <param name="collection">collection to add to</param>
        private void AddValuesRecursive(Node<K, V> node, ICollection<V> collection)
        {
            if (ReferenceEquals(node, null))
            {
                return;
            }

            AddValuesRecursive(node.Left, collection);

            collection.Add(node.Value);

            AddValuesRecursive(node.Right, collection);
        }

        /// <summary>
        /// Removes the greatest element from a sub-tree
        /// </summary>
        /// <param name="node">root node of the sub-tree</param>
        /// <returns>modified node</returns>
        protected Node<K, V> RemoveMax(Node<K, V> node)
        {
            if (ReferenceEquals(node, null))
            {
                return null;
            }
            else if (node.Right != null)
            {
                node.Right = RemoveMax(node.Right);
                return node;
            }
            else
            {
                return node.Left;
            }
        }

        /// <summary>
        /// Gets the greatest element in a sub-tree
        /// </summary>
        /// <param name="node">root node of the sub-tree</param>
        /// <returns>greatest element</returns>
        protected Node<K, V> GetMax(Node<K, V> node)
        {
            Node<K, V> parent = null;

            while (!ReferenceEquals(node, null))
            {
                parent = node;
                node = node.Right;
            }

            return parent;
        }

        // ************* TREE TORATIONS ************* //
        protected Node<K, V> RightRotation(Node<K, V> n2)
        {
            Node<K, V> n1 = n2.Left;

            n2.Left = n1.Right;
            n1.Right = n2;

            n2.Height = System.Math.Max(Height(n2.Left), Height(n2.Right)) + 1;
            n1.Height = System.Math.Max(Height(n1.Left), Height(n2)) + 1;

            return n1;
        }

        protected Node<K, V> LeftRotation(Node<K, V> n1)
        {
            Node<K, V> n2 = n1.Right;

            n1.Right = n2.Left;
            n2.Left = n1;

            n1.Height = System.Math.Max(Height(n1.Left), Height(n1.Right)) + 1;
            n2.Height = System.Math.Max(Height(n2.Right), Height(n1)) + 1;

            return n2;
        }

        protected Node<K, V> DoubleRightRotation(Node<K, V> n3)
        {
            n3.Left = LeftRotation(n3.Left);

            return RightRotation(n3);
        }

        protected Node<K, V> DoubleLeftRotation(Node<K, V> n1)
        {
            n1.Right = RightRotation(n1.Right);

            return LeftRotation(n1);
        }

        protected int Height(Node<K, V> node)
        {
            return ReferenceEquals(node, null) ? -1 : node.Height;
        }
        // ************* END OF TREE TORATIONS ************* //

        // ************ ITERATION LOGIC ************ //
        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            List<KeyValuePair<K, V>> nodes = new List<KeyValuePair<K, V>>();

            AddNodesRecursive(Root, nodes);

            foreach (KeyValuePair<K, V> node in nodes)
            {
                yield return node;
            }
        }

        private void AddNodesRecursive(Node<K, V> node, ICollection<KeyValuePair<K, V>> collection)
        {
            if (ReferenceEquals(node, null))
            {
                return;
            }

            AddNodesRecursive(node.Left, collection);

            collection.Add(new KeyValuePair<K, V>(node.Key, node.Value));

            AddNodesRecursive(node.Right, collection);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // ************ END OF ITERATION LOGIC ************ //

        /// <summary>
        /// Tree node class containing key, value, height of the node, left and right nodes
        /// </summary>
        /// <typeparam name="K">Key data type</typeparam>
        /// <typeparam name="V">Value data type</typeparam>
        protected class Node<K, V>
        {

            public K Key { get; set; }
            public V Value { get; set; }

            public Node<K, V> Left { get; set; }
            public Node<K, V> Right { get; set; }

            public int Height { get; set; }

            public Node() { }

            public Node(K Key, V Value) : this(Key, Value, null, null) { }

            public Node(K Key, V Value, Node<K, V> Left, Node<K, V> Right)
            {
                this.Key = Key;
                this.Value = Value;
                this.Left = Left;
                this.Right = Right;
            }
        }
    }
}