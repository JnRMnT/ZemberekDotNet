using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Native;

namespace ZemberekDotNet.Normalization
{
    public class SingleWordSpellChecker
    {
        public static readonly Dictionary<char, string> TurkishFQNearKeyMap = new Dictionary<char, string>();
        public static readonly Dictionary<char, string> TurkishQNearKeyMap = new Dictionary<char, string>();
        static readonly float InsertionPenalty = 1;
        static readonly float DeletionPenalty = 1;
        static readonly float SubstitutionPenalty = 1;
        static readonly float NearKeySubstitutionPenalty = 0.5f;
        static readonly float TranspositionPenalty = 1;
        private static int nodeIndexCounter = 0;
        private static readonly CultureInfo tr = CultureInfo.GetCultureInfo("tr");

        static SingleWordSpellChecker()
        {
            TurkishFQNearKeyMap.Add('a', "eüs");
            TurkishFQNearKeyMap.Add('b', "svn");
            TurkishFQNearKeyMap.Add('c', "vçx");
            TurkishFQNearKeyMap.Add('ç', "czö");
            TurkishFQNearKeyMap.Add('d', "orsf");
            TurkishFQNearKeyMap.Add('e', "iawr");
            TurkishFQNearKeyMap.Add('f', "gd");
            TurkishFQNearKeyMap.Add('g', "fğh");
            TurkishFQNearKeyMap.Add('ğ', "gıpü");
            TurkishFQNearKeyMap.Add('h', "npgj");
            TurkishFQNearKeyMap.Add('ı', "ğou");
            TurkishFQNearKeyMap.Add('i', "ueş");
            TurkishFQNearKeyMap.Add('j', "öhk");
            TurkishFQNearKeyMap.Add('k', "tmjl");
            TurkishFQNearKeyMap.Add('l', "mykş");
            TurkishFQNearKeyMap.Add('m', "klnö");
            TurkishFQNearKeyMap.Add('n', "rhbm");
            TurkishFQNearKeyMap.Add('o', "ıdp");
            TurkishFQNearKeyMap.Add('ö', "jvmç");
            TurkishFQNearKeyMap.Add('p', "hqoğ");
            TurkishFQNearKeyMap.Add('r', "dnet");
            TurkishFQNearKeyMap.Add('s', "zbad");
            TurkishFQNearKeyMap.Add('ş', "yli");
            TurkishFQNearKeyMap.Add('t', "ükry");
            TurkishFQNearKeyMap.Add('u', "iyı");
            TurkishFQNearKeyMap.Add('ü', "atğ");
            TurkishFQNearKeyMap.Add('v', "öcb");
            TurkishFQNearKeyMap.Add('y', "lştu");
            TurkishFQNearKeyMap.Add('z', "çsx");
            TurkishFQNearKeyMap.Add('x', "wzc");
            TurkishFQNearKeyMap.Add('q', "pqw");
            TurkishFQNearKeyMap.Add('w', "qxe");



            TurkishQNearKeyMap.Add('a', "s");
            TurkishQNearKeyMap.Add('b', "vn");
            TurkishQNearKeyMap.Add('c', "vx");
            TurkishQNearKeyMap.Add('ç', "ö");
            TurkishQNearKeyMap.Add('d', "sf");
            TurkishQNearKeyMap.Add('e', "wr");
            TurkishQNearKeyMap.Add('f', "gd");
            TurkishQNearKeyMap.Add('g', "fh");
            TurkishQNearKeyMap.Add('ğ', "pü");
            TurkishQNearKeyMap.Add('h', "gj");
            TurkishQNearKeyMap.Add('ı', "ou");
            TurkishQNearKeyMap.Add('i', "ş");
            TurkishQNearKeyMap.Add('j', "hk");
            TurkishQNearKeyMap.Add('k', "jl");
            TurkishQNearKeyMap.Add('l', "kş");
            TurkishQNearKeyMap.Add('m', "nö");
            TurkishQNearKeyMap.Add('n', "bm");
            TurkishQNearKeyMap.Add('o', "ıp");
            TurkishQNearKeyMap.Add('ö', "mç");
            TurkishQNearKeyMap.Add('p', "oğ");
            TurkishQNearKeyMap.Add('r', "et");
            TurkishQNearKeyMap.Add('s', "ad");
            TurkishQNearKeyMap.Add('ş', "li");
            TurkishQNearKeyMap.Add('t', "ry");
            TurkishQNearKeyMap.Add('u', "yı");
            TurkishQNearKeyMap.Add('ü', "ğ");
            TurkishQNearKeyMap.Add('v', "cb");
            TurkishQNearKeyMap.Add('y', "tu");
            TurkishQNearKeyMap.Add('z', "x");
            TurkishQNearKeyMap.Add('x', "zc");
            TurkishQNearKeyMap.Add('q', "w");
            TurkishQNearKeyMap.Add('w', "qe");
        }

        public readonly float maxPenalty;
        public readonly bool checkNearKeySubstitution;
        public ImmutableDictionary<char, string> nearKeyMap = new Dictionary<char, string>().ToImmutableDictionary();
        private Node root = new Node(Interlocked.Increment(ref nodeIndexCounter), (char)0);

        public SingleWordSpellChecker(float maxPenalty)
        {
            this.maxPenalty = maxPenalty;
            this.checkNearKeySubstitution = false;
        }

        public SingleWordSpellChecker()
        {
            this.maxPenalty = 1;
            this.checkNearKeySubstitution = false;
        }

        public SingleWordSpellChecker(float maxPenalty, Dictionary<char, string> nearKeyMap)
        {
            this.maxPenalty = maxPenalty;
            this.nearKeyMap = nearKeyMap.ToImmutableDictionary();
            this.checkNearKeySubstitution = true;
        }

        public string Process(string str)
        {
            return Regex.Replace(str.ToLower(tr), "['.]", "");
        }

        public void AddWord(String word)
        {
            string clean = Process(word);
            AddChar(root, 0, clean, word);
        }

        public void AddWords(params string[] words)
        {
            foreach (string word in words)
            {
                AddWord(word);
            }
        }

        public void BuildDictionary(List<string> vocabulary)
        {
            foreach (string s in vocabulary)
            {
                AddWord(s);
            }
        }

        private Node AddChar(Node currentNode, int index, string word, string actual)
        {
            char c = word[index];
            Node child = currentNode.AddChild(c);
            if (index == word.Length - 1)
            {
                child.Word = actual;
                return child;
            }
            index++;
            return AddChar(child, index, word, actual);
        }

        private ISet<Hypothesis> Expand(Hypothesis hypothesis, string input,
            FloatValueMap<string> finished)
        {

            ISet<Hypothesis> newHypotheses = new HashSet<Hypothesis>();

            int nextIndex = hypothesis.Index + 1;

            // no-error
            if (nextIndex < input.Length)
            {
                if (hypothesis.Node.HasChild(input[nextIndex]))
                {
                    Hypothesis hyp = hypothesis.GetNewMoveForward(
                        hypothesis.Node.GetChild(input[nextIndex]),
                        0,
                        Operation.NE);
                    if (nextIndex >= input.Length - 1)
                    {
                        if (hyp.Node.Word != null)
                        {
                            AddHypothesis(finished, hyp);
                        }
                    } // TODO: below line may produce unnecessary hypotheses.
                    newHypotheses.Add(hyp);
                }
            }
            else if (hypothesis.Node.Word != null)
            {
                AddHypothesis(finished, hypothesis);
            }

            // we don't need to explore further if we reached to max penalty
            if (hypothesis.Penalty >= maxPenalty)
            {
                return newHypotheses;
            }

            // substitution
            if (nextIndex < input.Length)
            {
                foreach (Node childNode in hypothesis.Node.GetChildNodes())
                {

                    float penalty = 0;
                    if (checkNearKeySubstitution)
                    {
                        char nextChar = input[nextIndex];
                        if (childNode.Chr != nextChar)
                        {
                            String nearCharactersString = nearKeyMap.GetValueOrDefault(childNode.Chr);
                            if (nearCharactersString != null && nearCharactersString.IndexOf(nextChar) >= 0)
                            {
                                penalty = NearKeySubstitutionPenalty;
                            }
                            else
                            {
                                penalty = SubstitutionPenalty;
                            }
                        }
                    }
                    else
                    {
                        penalty = SubstitutionPenalty;
                    }

                    if (penalty > 0 && hypothesis.Penalty + penalty <= maxPenalty)
                    {
                        Hypothesis hyp = hypothesis.GetNewMoveForward(
                            childNode,
                            penalty,
                            Operation.SUB);
                        if (nextIndex == input.Length - 1)
                        {
                            if (hyp.Node.Word != null)
                            {
                                AddHypothesis(finished, hyp);
                            }
                        }
                        else
                        {
                            newHypotheses.Add(hyp);
                        }
                    }
                }
            }

            if (hypothesis.Penalty + DeletionPenalty > maxPenalty)
            {
                return newHypotheses;
            }

            // deletion
            newHypotheses
                .Add(hypothesis.GetNewMoveForward(hypothesis.Node, DeletionPenalty, Operation.DEL));

            // insertion
            foreach (Node childNode in hypothesis.Node.GetChildNodes())
            {
                newHypotheses.Add(hypothesis.GetNew(childNode, InsertionPenalty, Operation.INS));
            }

            // transposition
            if (nextIndex < input.Length - 1)
            {
                char transpose = input[nextIndex + 1];
                Node nextNode = hypothesis.Node.GetChild(transpose);
                char nextChar = input[nextIndex];
                if (hypothesis.Node.HasChild(transpose) && nextNode.HasChild(nextChar))
                {
                    Hypothesis hyp = hypothesis.GetNew(
                        nextNode.GetChild(nextChar),
                        TranspositionPenalty,
                        nextIndex + 1,
                        Operation.TR);
                    if (nextIndex == input.Length - 1)
                    {
                        if (hyp.Node.Word != null)
                        {
                            AddHypothesis(finished, hyp);
                        }
                    }
                    else
                    {
                        newHypotheses.Add(hyp);
                    }
                }
            }
            return newHypotheses;
        }

        private void AddHypothesis(FloatValueMap<string> result, Hypothesis hypothesis)
        {
            string hypWord = hypothesis.Node.Word;
            if (hypWord == null)
            {
                return;
            }
            if (!result.Contains(hypWord))
            {
                result.Set(hypWord, hypothesis.Penalty);
            }
            else if (result.Get(hypWord) > hypothesis.Penalty)
            {
                result.Set(hypWord, hypothesis.Penalty);
            }
        }

        public FloatValueMap<string> Decode(string input)
        {
            Hypothesis hyp = new Hypothesis(null, root, 0, Operation.N_A);
            FloatValueMap<string> hypotheses = new FloatValueMap<string>();
            ISet<Hypothesis> next = Expand(hyp, input, hypotheses);
            while (true)
            {
                HashSet<Hypothesis> newHyps = new HashSet<Hypothesis>();
                foreach (Hypothesis hypothesis in next)
                {
                    newHyps.AddRange(Expand(hypothesis, input, hypotheses));
                }
                if (newHyps.Count == 0)
                {
                    break;
                }
                next = newHyps;
            }
            return hypotheses;
        }

        /// <summary>
        /// Returns suggestions sorted by penalty.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<ScoredString> GetSuggestionsWithScores(string input)
        {
            FloatValueMap<string> results = Decode(input);

            List<ScoredString> res = new List<ScoredString>(results.Size());
            foreach (string result in results)
            {
                res.Add(new ScoredString(result, results.Get(result)));
            }
            res.Sort();
            return res;
        }

        public List<string> GetSuggestions(string input)
        {
            return Decode(input).GetKeyList();
        }

        public List<string> GetSuggestionsSorted(string input)
        {
            List<ScoredString> s = GetSuggestionsWithScores(input);
            List<string> result = new List<string>(s.Count);
            result.AddRange(s.Select(s1 => s1.S));
            return result;
        }

        internal enum Operation
        {
            NE, INS, DEL, SUB, TR, N_A
        }

        public class Node
        {
            int index;
            char chr;
            UIntMap<Node> nodes = new UIntMap<Node>(2);
            String word;

            public int Index { get => index; set => index = value; }
            public char Chr { get => chr; set => chr = value; }
            public UIntMap<Node> Nodes { get => nodes; set => nodes = value; }
            public string Word { get => word; set => word = value; }

            public Node(int index, char chr)
            {
                this.Index = index;
                this.Chr = chr;
            }

            public IEnumerable<Node> GetChildNodes()
            {
                return Nodes.GetValues();
            }

            public bool HasChild(char c)
            {
                return Nodes.ContainsKey(c);
            }

            public Node GetChild(char c)
            {
                return Nodes.Get(c);
            }

            public Node AddChild(char c)
            {
                Node node = Nodes.Get(c);
                if (node == null)
                {
                    node = new Node(Interlocked.Increment(ref nodeIndexCounter), c);
                }
                Nodes.Put(c, node);
                return node;
            }

            public void SetWord(String word)
            {
                this.Word = word;
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
                return Index == node.Index;
            }

            public override int GetHashCode()
            {
                return Index;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder("[" + Chr);
                char[] characters = new char[Nodes.Size()];
                int[] keys = Nodes.GetKeys();
                for (int i = 0; i < characters.Length; i++)
                {
                    characters[i] = (char)keys[i];
                }
                Array.Sort(characters);
                if (Nodes.Size() > 0)
                {
                    sb.Append(" children=").Append(Arrays.ToString(characters));
                }
                if (Word != null)
                {
                    sb.Append(" word=").Append(Word);
                }
                sb.Append("]");
                return sb.ToString();
            }
        }

        public class ScoredString : IComparable<ScoredString>
        {
            readonly String s;
            readonly float penalty;

            public string S => s;

            public float Penalty => penalty;

            public ScoredString(String s, float penalty)
            {
                this.s = s;
                this.penalty = penalty;
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

                ScoredString result = (ScoredString)o;

                if (result.Penalty.CompareTo(Penalty) != 0)
                {
                    return false;
                }
                if (!S.Equals(result.S))
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                int result;
                long temp;
                result = S.GetHashCode();
                temp = Penalty.ToIntBits();
                result = 31 * result + (int)(temp ^ (temp >> 32));
                return result;
            }

            public int CompareTo(ScoredString o)
            {
                return Penalty.CompareTo(o.Penalty);
            }
        }

        internal class Hypothesis : IComparable<Hypothesis>
        {
            Operation operation;
            Hypothesis previous;
            Node node;
            float penalty;
            int index;

            private Operation Operation { get => operation; set => operation = value; }
            internal Hypothesis Previous { get => previous; set => previous = value; }
            public Node Node { get => node; set => node = value; }
            public float Penalty { get => penalty; set => penalty = value; }
            public int Index { get => index; set => index = value; }

            internal Hypothesis(Hypothesis previous, Node node, float penalty, Operation operation)
            {
                this.Previous = previous;
                this.Node = node;
                this.Penalty = penalty;
                this.Index = -1;
                this.Operation = operation;
            }

            internal Hypothesis(Hypothesis previous, Node node, float penalty, int index, Operation operation)
            {
                this.Previous = previous;
                this.Node = node;
                this.Penalty = penalty;
                this.Index = index;
                this.Operation = operation;
            }

            internal string BackTrack()
            {
                StringBuilder sb = new StringBuilder();
                Hypothesis p = Previous;
                while (p.Node.Chr != 0)
                {
                    if (p.Node != p.Previous.Node)
                    {
                        sb.Append(p.Node.Chr);
                    }
                    p = p.Previous;
                }
                return sb.ToString().Reverse().ToString();
            }

            internal Hypothesis GetNew(Node node, float penaltyToAdd, Operation operation)
            {
                return new Hypothesis(this, node, this.Penalty + penaltyToAdd, Index, operation);
            }

            internal Hypothesis GetNewMoveForward(Node node, float penaltyToAdd, Operation operation)
            {
                return new Hypothesis(this, node, this.Penalty + penaltyToAdd, Index + 1, operation);
            }

            internal Hypothesis GetNew(Node node, float penaltyToAdd, int index, Operation operation)
            {
                return new Hypothesis(this, node, this.Penalty + penaltyToAdd, index, operation);
            }

            internal Hypothesis GetNew(float penaltyToAdd, Operation operation)
            {
                return new Hypothesis(this, this.Node, this.Penalty + penaltyToAdd, Index, operation);
            }

            public int CompareTo(Hypothesis o)
            {
                return Penalty.CompareTo(o.Penalty);
            }

            public override string ToString()
            {
                return "Hypothesis{" +
                    "previous=" + BackTrack() + " " + Previous.Operation +
                    ", node=" + Node +
                    ", penalty=" + Penalty +
                    ", index=" + Index +
                    ", OP=" + Operation.ToString() +
                    '}';
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

                Hypothesis that = (Hypothesis)o;

                if (Index != that.Index)
                {
                    return false;
                }
                if (that.Penalty.CompareTo(Penalty) != 0)
                {
                    return false;
                }
                if (!Node.Equals(that.Node))
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                int result;
                long temp;
                result = Node.GetHashCode();
                temp = ((double)Penalty).ToLongBits();
                result = 31 * result + (int)(temp ^ (temp >> 32));
                result = 31 * result + Index;
                return result;
            }
        }
    }
}
