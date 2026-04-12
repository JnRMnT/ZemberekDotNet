using System.Collections.Generic;
using ZemberekDotNet.Core.Text.Distance;

namespace ZemberekDotNet.Morphology.Extended
{
    /// <summary>
    /// BK-tree (Burkhard-Keller tree) over strings using Levenshtein edit distance.
    /// Build cost is O(n log n); lookup cost is O(log n) on average.
    /// Used internally by <see cref="ExtendedMorphologyContext.FuzzyAnalyze"/>.
    /// </summary>
    /// <remarks>ZemberekDotNet addition — no equivalent in Java Zemberek.</remarks>
    internal sealed class BkTree
    {
        private readonly Node root;
        private static readonly CharDistance Distance = new CharDistance();

        internal BkTree(IEnumerable<string> items)
        {
            Node rootNode = null;
            foreach (string item in items)
            {
                if (rootNode == null)
                {
                    rootNode = new Node(item);
                }
                else
                {
                    Insert(rootNode, item);
                }
            }
            root = rootNode;
        }

        private static void Insert(Node current, string word)
        {
            int d = (int)Distance.Distance(current.Word, word);
            if (d == 0)
            {
                return; // duplicate — skip
            }
            if (current.Children.TryGetValue(d, out Node child))
            {
                Insert(child, word);
            }
            else
            {
                current.Children[d] = new Node(word);
            }
        }

        /// <summary>
        /// Returns all words in the tree within the given edit distance of <paramref name="query"/>.
        /// </summary>
        internal IReadOnlyList<string> Search(string query, int maxDistance)
        {
            if (root == null)
            {
                return new List<string>();
            }

            List<string> results = new List<string>();
            Stack<Node> stack = new Stack<Node>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                Node current = stack.Pop();
                int d = (int)Distance.Distance(current.Word, query);
                if (d <= maxDistance)
                {
                    results.Add(current.Word);
                }
                int low = d - maxDistance;
                int high = d + maxDistance;
                foreach (KeyValuePair<int, Node> kv in current.Children)
                {
                    if (kv.Key >= low && kv.Key <= high)
                    {
                        stack.Push(kv.Value);
                    }
                }
            }

            return results;
        }

        private sealed class Node
        {
            internal readonly string Word;
            internal readonly Dictionary<int, Node> Children = new Dictionary<int, Node>();

            internal Node(string word)
            {
                Word = word;
            }
        }
    }
}
