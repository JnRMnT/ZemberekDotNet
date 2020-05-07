using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Native;

namespace ZemberekDotNet.Normalization
{
    public class Node
    {
        public static readonly int TypeEmpty = 0;
        public static readonly int TypeWord = 1;
        public static readonly int TypeEnding = 2;
        public static readonly int TypeGraphRoot = 3;
        internal char chr;
        internal string word;
        private int index;
        private UIntMap<Node> nodes = new UIntMap<Node>(2);
        private Node[] epsilonNodes = null;
        private int type;

        internal Node(int index, char chr, int type)
        {
            this.index = index;
            this.chr = chr;
            this.type = type;
        }

        internal Node(int index, char chr, int type, string word)
        {
            this.index = index;
            this.chr = chr;
            this.type = type;
            this.word = word;
        }

        public int GetType()
        {
            return type;
        }

        internal IEnumerable<Node> GetImmediateChildNodeIterable()
        {
            return nodes;
        }

        internal List<Node> GetImmediateChildNodes()
        {
            return nodes.GetValues();
        }

        public List<Node> GetAllChildNodes()
        {
            List<Node> nodeList = nodes.GetValues();
            if (epsilonNodes == null)
            {
                return nodeList;
            }
            foreach (Node emptyNode in epsilonNodes)
            {
                foreach (Node n in emptyNode.nodes)
                {
                    nodeList.Add(n);
                }
            }
            return nodeList;
        }

        public bool HasEpsilonConnection()
        {
            return epsilonNodes != null;
        }

        public bool HasImmediateChild(char c)
        {
            return nodes.ContainsKey(c);
        }

        public bool HasChild(char c)
        {
            if (HasImmediateChild(c))
            {
                return true;
            }
            if (epsilonNodes == null)
            {
                return false;
            }
            foreach (Node node in epsilonNodes)
            {
                if (node.HasImmediateChild(c))
                {
                    return true;
                }
            }
            return false;
        }

        internal Node GetImmediateChild(char c)
        {
            return nodes.Get(c);
        }

        private void AddIfChildExists(char c, List<Node> nodeList)
        {
            Node child = this.nodes.Get(c);
            if (child != null)
            {
                nodeList.Add(child);
            }
        }

        internal List<Node> GetChildList(char[] charArray)
        {
            List<Node> children = new List<Node>(charArray.Length + 1);
            foreach (char c in charArray)
            {
                AddIfChildExists(c, children);
                if (epsilonNodes != null)
                {
                    foreach (Node emptyNode in epsilonNodes)
                    {
                        emptyNode.AddIfChildExists(c, children);
                    }
                }
            }
            return children;
        }

        internal List<Node> GetChildList(char c)
        {
            List<Node> children = new List<Node>(2);
            AddIfChildExists(c, children);
            if (epsilonNodes != null)
            {
                foreach (Node emptyNode in epsilonNodes)
                {
                    emptyNode.AddIfChildExists(c, children);
                }
            }
            return children;
        }

        internal bool ConnectEpsilon(Node node)
        {
            if (epsilonNodes == null)
            {
                epsilonNodes = new Node[1];
                epsilonNodes[0] = node;
            }
            else
            {
                foreach (Node n in epsilonNodes)
                {
                    if (n.Equals(node))
                    {
                        return false;
                    }
                }
                epsilonNodes = epsilonNodes.CopyOf(epsilonNodes.Length + 1);
                epsilonNodes[epsilonNodes.Length - 1] = node;
            }
            return true;
        }

        public Node[] GetEpsilonNodes()
        {
            return epsilonNodes;
        }

        internal Node AddChild(int index, char c, int type)
        {
            Node node = nodes.Get(c);
            if (node == null)
            {
                node = new Node(index, c, type);
                nodes.Put(c, node);
            }
            return node;
        }

        internal Node AddChild(int index, char c, int type, String word)
        {
            Node node = nodes.Get(c);
            if (node == null)
            {
                node = new Node(index, c, type, word);
                nodes.Put(c, node);
            }
            else
            {
                node.word = word;
                node.type = type;
            }
            return node;
        }

        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !GetType().Equals(o.GetType()))
            {
                return false;
            }
            Node node = (Node)o;
            return index == node.index;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[" + chr);
            char[] characters = new char[nodes.Size()];
            int[] keys = nodes.GetKeys();
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = (char)keys[i];
            }
            Array.Sort(characters);
            if (nodes.Size() > 0)
            {
                sb.Append(" children=").Append(Arrays.ToString(characters));
            }
            if (word != null)
            {
                sb.Append(" word=").Append(word);
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
