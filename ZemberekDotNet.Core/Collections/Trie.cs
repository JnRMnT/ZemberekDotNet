using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Collections
{
    /// <summary>
    /// A simple compact trie.
    /// </summary>
    public class Trie<T>
    {
        private Node<T> root = new Node<T>();
        private int size = 0;

        public void Add(string s, T item)
        {
            if (item == null)
            {
                throw new NullReferenceException("Input key can not be null");
            }
            char[] chars = s.ToCharArray();
            Node<T> node = root;
            Node<T> previousNode;
            // i holds the char index for input
            int i = 0;
            // fragmentSplitIndex is the index of the last fragment
            int fragmentSplitIndex;
            // While we still have chars left on the input, or no child marked with s[i]
            // is found in sub-nodes
            while (node != null)
            {
                previousNode = node;
                node = node.GetChildNode(chars[i]);
                // Cases:
                // root <- foo ==> root-foo*
                // or
                // root-foo* <- bar ==> root-foo*
                //                         \-bar*
                // or
                // root-foo* <- foobar ==> root-foo*-bar*
                if (node == null)
                {
                    previousNode.AddChild(new Node<T>(item, GetSuffix(chars, i)));
                    size++;
                    return;
                }
                else
                {
                    fragmentSplitIndex = GetSplitPoint(chars, i, node.fragment);
                    i += fragmentSplitIndex;
                    // Case:
                    // root-foobar* <- foo ==> root-foo*-bar*
                    // or
                    // root-fo-obar* <-foo ==> root-fo-o*-bar*
                    //        \-x*                    \-x*
                    // or Homonym:
                    // root-foo* <-- foo ==> root-foo**
                    if (i == chars.Length)
                    {
                        // Homonym
                        if (fragmentSplitIndex == node.fragment.Length)
                        {
                            if (node.AddItem(item))
                            {
                                size++;
                            }
                            break;
                        }
                        Node<T> newNode = new Node<T>(item, node.fragment.CopyOf(fragmentSplitIndex));
                        size++;
                        node.TrimLeft(fragmentSplitIndex);
                        newNode.AddChild(node);
                        previousNode.AddChild(newNode);
                        break;
                    }
                    // Case:
                    // root-foobar* <- foxes ==> root-fo-obar*
                    //                                  \-xes*
                    if (i < chars.Length && fragmentSplitIndex < node.fragment.Length)
                    {
                        Node<T> node1 = new Node<T>();
                        node1.SetFragment(node.fragment.CopyOf(fragmentSplitIndex)); // fo
                        previousNode.AddChild(node1);
                        node.TrimLeft(fragmentSplitIndex); // obar
                        node1.AddChild(node);
                        Node<T> node2 = new Node<T>(item, GetSuffix(chars, i)); //xes
                        size++;
                        node1.AddChild(node2);
                        break;
                    }
                }
            }
        }

        // Remove does not apply compaction, just removes the item from node.
        public void Remove(string s, T item)
        {
            Node<T> node = WalkToNode(s);
            if (node != null && node.HasItem())
            {
                node.items.Remove(item);
                size--;
            }
        }

        public int Size()
        {
            return size;
        }

        public bool ContainsItem(String s, T item)
        {
            Node<T> node = WalkToNode(s);
            return (node != null && node.items.Contains(item));
        }

        public List<T> GetItems(String s)
        {
            Node<T> node = WalkToNode(s);
            return node == null ? new List<T>(0) : new List<T>(node.items);
        }

        public List<T> GetAll()
        {
            List<T> items = new List<T>(size);
            List<Node<T>> toWalk = new List<Node<T>> { root };
            while (toWalk.Count > 0)
            {
                List<Node<T>> n = new List<Node<T>>();
                foreach (Node<T> tNode in toWalk)
                {
                    if (tNode.HasItem())
                    {
                        items.AddRange(tNode.items);
                    }
                    if (tNode.children != null && tNode.children.Size() > 0)
                    {
                        n.AddRange(tNode.children.GetValues());
                    }
                }
                toWalk = n;
            }
            return items;
        }

        public List<T> GetPrefixMatchingItems(String input)
        {
            List<T> items = new List<T>(2);
            Node<T> node = root;
            char[] chars = input.ToCharArray();
            int i = 0;

            while (i < chars.Length)
            {
                node = node.GetChildNode(chars[i]);
                // if there are no child node with input char, break
                if (node == null)
                {
                    break;
                }
                char[] fragment = node.fragment;
                // Compare fragment and input.
                int j;
                for (j = 0; j < fragment.Length && i < chars.Length; j++, i++)
                {
                    if (fragment[j] != chars[i])
                    {
                        goto EndOfMainLoop;
                    }
                }
                if (j == fragment.Length)
                {
                    if (node.HasItem())
                    {
                        items.AddRange(node.items);
                    }
                }
                else
                {
                    // no need to go further
                    break;
                }
            }
        EndOfMainLoop: { }
            return items;
        }

        public override string ToString()
        {
            return root != null ? root.Dump() : "";
        }

        private Node<T> WalkToNode(string input)
        {
            Node<T> node = root;
            int i = 0;
            while (i < input.Length)
            {
                node = node.GetChildNode(input[i]);
                // if there are no child node with input char, break
                if (node == null)
                {
                    break;
                }
                char[] fragment = node.fragment;
                // Compare fragment and input.
                int j;
                //TODO: code below may be simplified
                for (j = 0; j < fragment.Length && i < input.Length; j++, i++)
                {
                    if (fragment[j] != input[i])
                    {
                        break;
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// Finds the last position of common chars for 2 char arrays relative to a given index.
        /// </summary>
        /// <param name="input">input char array to look in the fragment</param>
        /// <param name="start">start index where method starts looking the input in the fragment</param>
        /// <param name="fragment">the char array to look input array.</param>
        /// <returns><pre>
        /// for input: "foo" fragment = "foobar" index = 0, returns 3
        /// for input: "fool" fragment = "foobar" index = 0, returns 3
        /// for input: "fool" fragment = "foobar" index = 1, returns 2
        /// for input: "foo" fragment = "obar" index = 1, returns 2
        /// for input: "xyzfoo" fragment = "foo" index = 3, returns 2
        /// for input: "xyzfoo" fragment = "xyz" index = 3, returns 0
        /// for input: "xyz" fragment = "abc" index = 0, returns 0
        /// </pre></returns>
        private static int GetSplitPoint(char[] input, int start, char[] fragment)
        {
            int fragmentIndex = 0;
            while (start < input.Length && fragmentIndex < fragment.Length
                && input[start++] == fragment[fragmentIndex])
            {
                fragmentIndex++;
            }
            return fragmentIndex;
        }

        private static char[] GetSuffix(char[] arr, int index)
        {
            char[] res = new char[arr.Length - index];
            Array.Copy(arr, index, res, 0, arr.Length - index);
            return res;
        }

        public class Node<X>
        {
            internal char[] fragment;
            internal List<X> items;
            internal IntMap<Node<X>> children;

            public Node()
            {
            }

            public Node(X s, char[] fragment)
            {
                AddItem(s);
                SetFragment(fragment);
            }

            public void TrimLeft(int i)
            {
                SetFragment(GetSuffix(fragment, i));
            }

            public void SetFragment(char[] fragment)
            {
                this.fragment = fragment;
            }

            public bool AddItem(X item)
            {
                if (items == null)
                {
                    items = new List<X>(1);
                }
                if (!items.Contains(item))
                {
                    items.Add(item);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            public void AddChild(Node<X> node)
            {
                if (children == null)
                {
                    children = new IntMap<Node<X>>(4);
                }
                children.Put(node.GetChar(), node);
            }

            public Node<X> GetChildNode(char c)
            {
                if (children == null)
                {
                    return null;
                }
                return children.Get(c);
            }

            public override string ToString()
            {
                StringBuilder s = new StringBuilder(fragment == null ? "#" : new String(fragment));
                if (children != null)
                {
                    s.Append("( ");
                    foreach (Node<X> node in children.GetValues())
                    {
                        if (node != null)
                        {
                            s.Append(node.GetChar()).Append(" ");
                        }
                    }
                    s.Append(")");
                }
                if (items != null)
                {
                    foreach (X item in items)
                    {
                        s.Append(" [").Append(item).Append("]");
                    }
                }
                return s.ToString();
            }

            private char GetChar()
            {
                if (fragment == null)
                {
                    return '#';
                }
                return fragment[0];
            }

            /// <summary>
            /// Returns string representation of node and all child nodes until leafs.
            /// </summary>
            /// <param name="b">string buffer to append.</param>
            /// <param name="level">level of the operation</param>
            private void ToDeepString(StringBuilder b, int level)
            {
                char[] indentChars = new char[level * 2];
                for (int i = 0; i < indentChars.Length; i++)
                {
                    indentChars[i] = ' ';
                }
                b.Append(indentChars).Append(this.ToString());
                b.Append("\n");
                if (children != null)
                {
                    foreach (Node<X> subNode in this.children.GetValues())
                    {
                        if (subNode != null)
                        {
                            subNode.ToDeepString(b, level + 1);
                        }
                    }
                }
            }

            /// <summary>
            /// Returns string representation of Node (and subnodes) for testing.
            /// </summary>
            /// <returns>String representation of trie.</returns>
            public string Dump()
            {
                StringBuilder b = new StringBuilder();
                ToDeepString(b, 0);
                return b.ToString();
            }

            public bool HasItem()
            {
                return (items != null && items.Count > 0);
            }
        }
    }
}
