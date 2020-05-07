using System;
using System.Collections.Generic;
using System.Threading;

namespace ZemberekDotNet.Normalization
{
    public class CharacterGraph
    {
        private static int nodeIndexCounter = 0;
        private Node root = new Node(Interlocked.Increment(ref nodeIndexCounter), (char)0, Node.TypeGraphRoot);

        internal bool IsRoot(Node node)
        {
            return node == root;
        }

        public Node GetRoot()
        {
            return root;
        }

        public Node AddWord(string word, int type)
        {
            return Add(root, 0, word, type);
        }

        private Node Add(Node currentNode, int index, string word, int type)
        {
            char c = word[index];
            if (index == word.Length - 1)
            {
                return currentNode.AddChild(Interlocked.Increment(ref nodeIndexCounter), c, type, word);
            }
            Node child = currentNode.AddChild(Interlocked.Increment(ref nodeIndexCounter), c, Node.TypeEmpty);
            index++;
            return Add(child, index, word, type);
        }

        public HashSet<Node> GetAllNodes()
        {
            HashSet<Node> nodes = new HashSet<Node>();
            Walk(root, nodes, node => true);
            return nodes;
        }

        public HashSet<Node> GetAllNodes(Predicate<Node> predicate)
        {
            HashSet<Node> nodes = new HashSet<Node>();
            Walk(root, nodes, predicate);
            return nodes;
        }

        private void Walk(Node current, HashSet<Node> nodes, Predicate<Node> predicate)
        {
            if (nodes.Contains(current))
            {
                return;
            }
            if (predicate(current))
            {
                nodes.Add(current);
            }
            foreach (Node node in current.GetImmediateChildNodes())
            {
                Walk(node, nodes, predicate);
            }
        }

        private void Walk(Node current, HashSet<Node> nodes, Action<Node> consumer)
        {
            if (nodes.Contains(current))
            {
                return;
            }
            consumer(current);
            nodes.Add(current);

            foreach (Node node in current.GetImmediateChildNodes())
            {
                Walk(node, nodes, consumer);
            }
        }


        internal bool WordExists(string w, int type)
        {
            HashSet<Node> nodes = new HashSet<Node>();
            Walk(root, nodes, node => node.word != null && node.word.Equals(w) && node.GetType() == type);
            return nodes.Count > 0;
        }
    }
}
