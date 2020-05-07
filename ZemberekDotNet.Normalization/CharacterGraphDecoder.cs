using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Turkish;

namespace ZemberekDotNet.Normalization
{
    public class CharacterGraphDecoder
    {
        public static readonly Dictionary<char, string> TurkishFQNearKeyMap = new Dictionary<char, string>();
        public static readonly Dictionary<char, string> TurkishQNearKeyMap = new Dictionary<char, string>();
        internal static readonly float InsertionPenalty = 1;
        internal static readonly float DeletionPenalty = 1;
        internal static readonly float SubstitutionPenalty = 1;
        internal static readonly float NearKeySubstitutionPenalty = 0.5f;
        internal static readonly float TranspositionPenalty = 1;
        private static readonly CultureInfo tr = CultureInfo.GetCultureInfo("tr");
        public static readonly DiacriticsIgnoringMatcher DIACRITICS_IGNORING_MATCHER =
      new DiacriticsIgnoringMatcher();

        static CharacterGraphDecoder()
        {
            Dictionary<char, string> map = TurkishFQNearKeyMap;
            map.Add('a', "eüs");
            map.Add('b', "svn");
            map.Add('c', "vçx");
            map.Add('ç', "czö");
            map.Add('d', "orsf");
            map.Add('e', "iawr");
            map.Add('f', "gd");
            map.Add('g', "fğh");
            map.Add('ğ', "gıpü");
            map.Add('h', "npgj");
            map.Add('ı', "ğou");
            map.Add('i', "ueş");
            map.Add('j', "öhk");
            map.Add('k', "tmjl");
            map.Add('l', "mykş");
            map.Add('m', "klnö");
            map.Add('n', "rhbm");
            map.Add('o', "ıdp");
            map.Add('ö', "jvmç");
            map.Add('p', "hqoğ");
            map.Add('r', "dnet");
            map.Add('s', "zbad");
            map.Add('ş', "yli");
            map.Add('t', "ükry");
            map.Add('u', "iyı");
            map.Add('ü', "atğ");
            map.Add('v', "öcb");
            map.Add('y', "lştu");
            map.Add('z', "çsx");
            map.Add('x', "wzc");
            map.Add('q', "pqw");
            map.Add('w', "qxe");

            map = TurkishQNearKeyMap;
            map.Add('a', "s");
            map.Add('b', "vn");
            map.Add('c', "vx");
            map.Add('ç', "ö");
            map.Add('d', "sf");
            map.Add('e', "wr");
            map.Add('f', "gd");
            map.Add('g', "fh");
            map.Add('ğ', "pü");
            map.Add('h', "gj");
            map.Add('ı', "ou");
            map.Add('i', "ş");
            map.Add('j', "hk");
            map.Add('k', "jl");
            map.Add('l', "kş");
            map.Add('m', "nö");
            map.Add('n', "bm");
            map.Add('o', "ıp");
            map.Add('ö', "mç");
            map.Add('p', "oğ");
            map.Add('r', "et");
            map.Add('s', "ad");
            map.Add('ş', "li");
            map.Add('t', "ry");
            map.Add('u', "yı");
            map.Add('ü', "ğ");
            map.Add('v', "cb");
            map.Add('y', "tu");
            map.Add('z', "x");
            map.Add('x', "zc");
            map.Add('q', "w");
            map.Add('w', "qe");
        }

        public readonly float MaxPenalty;
        public readonly bool CheckNearKeySubstitution;
        public Dictionary<char, string> nearKeyMap = new Dictionary<char, string>();
        private CharacterGraph graph = new CharacterGraph();

        public CharacterGraphDecoder(float maxPenalty)
        {
            this.MaxPenalty = maxPenalty;
            this.CheckNearKeySubstitution = false;
        }

        public CharacterGraphDecoder()
        {
            this.MaxPenalty = 1;
            this.CheckNearKeySubstitution = false;
        }

        public CharacterGraphDecoder(CharacterGraph graph)
        {
            this.graph = graph;
            this.MaxPenalty = 1;
            this.CheckNearKeySubstitution = false;
        }

        public CharacterGraphDecoder(float maxPenalty, Dictionary<char, string> nearKeyMap)
        {
            this.MaxPenalty = maxPenalty;
            this.nearKeyMap = nearKeyMap;
            this.CheckNearKeySubstitution = true;
        }

        public CharacterGraph GetGraph()
        {
            return graph;
        }

        private string Process(string str)
        {
            return Regex.Replace(str.ToLower(tr), "['.]", "");
        }

        public void AddWord(string word)
        {
            graph.AddWord(Process(word), Node.TypeWord);
        }

        public void AddWords(params string[] words)
        {
            foreach (string word in words)
            {
                graph.AddWord(Process(word), Node.TypeWord);
            }
        }

        public void AddWords(List<string> vocabulary)
        {
            foreach (string s in vocabulary)
            {
                graph.AddWord(Process(s), Node.TypeWord);
            }
        }

        /// <summary>
        /// Returns suggestions sorted by penalty.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<ScoredItem<string>> GetSuggestionsWithScores(string input)
        {
            Decoder decoder = new Decoder(this);
            return GetMatches(input, decoder);
        }

        private List<ScoredItem<string>> GetMatches(string input, Decoder decoder)
        {
            FloatValueMap<string> results = decoder.Decode(input);

            List<ScoredItem<string>> res = new List<ScoredItem<string>>(results.Size());
            foreach (string result in results)
            {
                res.Add(new ScoredItem<string>(result, results.Get(result)));
            }
            res.Sort((a, b) => a.Score.CompareTo(b.Score));
            return res;
        }

        public List<ScoredItem<string>> GetSuggestionsWithScores(string input, ICharMatcher matcher)
        {
            Decoder decoder = new Decoder(this, matcher);
            return GetMatches(input, decoder);
        }

        public FloatValueMap<string> Decode(string input)
        {
            return new Decoder(this).Decode(input);
        }

        public List<string> GetSuggestions(string input)
        {
            return new Decoder(this).Decode(input).GetKeyList();
        }

        public List<string> GetSuggestions(string input, ICharMatcher matcher)
        {
            return new Decoder(this, matcher).Decode(input).GetKeyList();
        }

        public List<string> GetSuggestionsSorted(string input)
        {
            List<ScoredItem<string>> s = GetSuggestionsWithScores(input);
            List<string> result = new List<string>(s.Count);
            result.AddRange(s.Select(s1 => s1.Item));
            return result;
        }

        internal enum Operation
        {
            NO_ERROR, INSERTION, DELETION, SUBSTITUTION, TRANSPOSITION, N_A
        }

        public interface ICharMatcher
        {
            char[] Matches(char c);
        }

        internal class Hypothesis : IComparable<Hypothesis>
        {
            Operation operation;
            int charIndex;
            Node node;
            float penalty;
            string word;
            string ending;
            Hypothesis previous;

            internal Operation Operation { get => operation; set => operation = value; }
            public int CharIndex { get => charIndex; set => charIndex = value; }
            internal Node Node { get => node; set => node = value; }
            public float Penalty { get => penalty; set => penalty = value; }
            public string Word { get => word; set => word = value; }
            public string Ending { get => ending; set => ending = value; }
            internal Hypothesis Previous { get => previous; set => previous = value; }

            internal Hypothesis(Hypothesis previous, Node node, float penalty, Operation operation, string word,
            string ending)
            {
                this.Previous = previous;
                this.Node = node;
                this.Penalty = penalty;
                this.CharIndex = -1;
                this.Operation = operation;
                this.Word = word;
                this.Ending = ending;
            }

            internal Hypothesis(Hypothesis previous, Node node, float penalty, int charIndex, Operation operation,
            string word, string ending)
            {
                this.Previous = previous;
                this.Node = node;
                this.Penalty = penalty;
                this.CharIndex = charIndex;
                this.Operation = operation;
                this.Word = word;
                this.Ending = ending;
            }

            internal string BackTrack()
            {
                StringBuilder sb = new StringBuilder();
                Hypothesis p = Previous;
                while (p.Node.chr != 0)
                {
                    if (p.Node != p.Previous.Node)
                    {
                        sb.Append(p.Node.chr);
                    }
                    p = p.Previous;
                }
                return sb.ToString().Reverse().ToString();
            }

            internal string GetContent()
            {
                string w = Word == null ? "" : Word;
                string e = Ending == null ? "" : Ending;
                return w + e;
            }

            internal void SetWord(Node node)
            {
                if (node.word == null)
                {
                    return;
                }
                if (node.GetType() == Node.TypeWord)
                {
                    this.Word = node.word;
                }
                else if (node.GetType() == Node.TypeEnding)
                {
                    this.Ending = node.word;
                }
            }

            internal Hypothesis GetNew(Node node, float penaltyToAdd, Operation operation)
            {
                return new Hypothesis(this, node, this.Penalty + penaltyToAdd, CharIndex, operation,
                    this.Word, this.Ending);
            }

            internal Hypothesis GetNewMoveForward(Node node, float penaltyToAdd, Operation operation)
            {
                return new Hypothesis(this, node, this.Penalty + penaltyToAdd, CharIndex + 1, operation,
                    this.Word, this.Ending);
            }

            internal Hypothesis GetNew(Node node, float penaltyToAdd, int index, Operation operation)
            {
                return new Hypothesis(this, node, this.Penalty + penaltyToAdd, index, operation, this.Word,
                    this.Ending);
            }

            internal Hypothesis GetNew(float penaltyToAdd, Operation operation)
            {
                return new Hypothesis(this, this.Node, this.Penalty + penaltyToAdd, CharIndex, operation,
                    this.Word, this.Ending);
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
                    ", index=" + CharIndex +
                    ", OP=" + Operation.GetName(typeof(Operation), Operation) +
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

                if (CharIndex != that.CharIndex)
                {
                    return false;
                }
                // TODO: this should not be here.
                if (that.Penalty.CompareTo(Penalty) != 0)
                {
                    return false;
                }
                if (!Node.Equals(that.Node))
                {
                    return false;
                }
                if (!Objects.Equals(Word, that.Word))
                {
                    return false;
                }
                return Objects.Equals(Ending, that.Ending);
            }

            public override int GetHashCode()
            {
                int result = CharIndex;
                result = 31 * result + Node.GetHashCode();
                // TODO: this should not be here.
                result = 31 * result + (Penalty != +0.0f ? Penalty.ToIntBits() : 0);
                result = 31 * result + (Word != null ? Word.GetHashCode() : 0);
                result = 31 * result + (Ending != null ? Ending.GetHashCode() : 0);
                return result;
            }
        }

        public class DiacriticsIgnoringMatcher : ICharMatcher
        {
            static IntMap<char[]> map = new IntMap<char[]>();

            public DiacriticsIgnoringMatcher()
            {
                string allLetters = TurkishAlphabet.Instance.GetAllLetters() + "+.,'-";

                for (int i = 0; i < allLetters.Length; i++)
                {
                    char[] ca = new char[1];
                    char c = allLetters[i];
                    ca[0] = c;
                    map.Put(c, ca);
                }
                // override some
                map.Put('c', new char[] { 'c', 'ç' });
                map.Put('g', new char[] { 'g', 'ğ' });
                map.Put('ı', new char[] { 'ı', 'i' });
                map.Put('i', new char[] { 'ı', 'i' });
                map.Put('o', new char[] { 'o', 'ö' });
                map.Put('s', new char[] { 's', 'ş' });
                map.Put('u', new char[] { 'u', 'ü' });
                map.Put('a', new char[] { 'a', 'â' });
                map.Put('i', new char[] { 'i', 'î' });
                map.Put('u', new char[] { 'u', 'û' });
                map.Put('C', new char[] { 'C', 'Ç' });
                map.Put('G', new char[] { 'G', 'Ğ' });
                map.Put('I', new char[] { 'I', 'İ' });
                map.Put('İ', new char[] { 'İ', 'I' });
                map.Put('O', new char[] { 'O', 'Ö' });
                map.Put('Ö', new char[] { 'Ö', 'Ş' });
                map.Put('U', new char[] { 'U', 'Ü' });
                map.Put('A', new char[] { 'A', 'Â' });
                map.Put('İ', new char[] { 'İ', 'Î' });
                map.Put('U', new char[] { 'U', 'Û' });
            }

            public char[] Matches(char c)
            {
                char[] res = map.Get(c);
                return res == null ? new char[] { c } : res;
            }
        }

        private class Decoder
        {
            FloatValueMap<string> finished = new FloatValueMap<string>(8);
            ICharMatcher matcher;
            private CharacterGraphDecoder decoder;

            internal Decoder(CharacterGraphDecoder decoder) : this(decoder, null)
            {

            }

            internal Decoder(CharacterGraphDecoder decoder, ICharMatcher matcher)
            {
                this.matcher = matcher;
                this.decoder = decoder;
            }

            internal FloatValueMap<string> Decode(string input)
            {
                Hypothesis hyp = new Hypothesis(null, decoder.graph.GetRoot(), 0, Operation.N_A, null, null);

                ISet<Hypothesis> next = Expand(hyp, input);
                while (true)
                {
                    HashSet<Hypothesis> newHyps = new HashSet<Hypothesis>();
                    foreach (Hypothesis hypothesis in next)
                    {
                        ISet<Hypothesis> expand = Expand(hypothesis, input);
                        newHyps.AddRange(expand);
                    }
                    if (newHyps.Count == 0)
                    {
                        break;
                    }
                    next = newHyps;
                }
                return finished;
            }

            private ISet<Hypothesis> Expand(Hypothesis hypothesis, string input)
            {
                ISet<Hypothesis> newHypotheses = new HashSet<Hypothesis>();

                // get next character for this hypothesis.
                int nextIndex = hypothesis.CharIndex + 1;
                char nextChar = nextIndex < input.Length ? input[nextIndex] : (char)0;

                // no-error. Hypothesis moves forward to the exact matching child nodes.
                if (nextIndex < input.Length)
                {

                    // there can be more than one matching character, depending on the matcher.
                    char[] cc = matcher == null ? null : matcher.Matches(nextChar);
                    // because there can be empty connections,
                    // there can be more than 1 matching child nodes per character.
                    if (hypothesis.Node.HasEpsilonConnection())
                    {
                        List<Node> childList = cc == null ?
                            hypothesis.Node.GetChildList(nextChar) :
                            hypothesis.Node.GetChildList(cc);
                        foreach (Node child in childList)
                        {
                            Hypothesis h = hypothesis.GetNewMoveForward(child, 0, Operation.NO_ERROR);
                            h.SetWord(child);
                            newHypotheses.Add(h);
                            if (nextIndex >= input.Length - 1)
                            {
                                if (h.Node.word != null)
                                {
                                    AddHypothesis(h);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (cc == null)
                        {
                            Node child = hypothesis.Node.GetImmediateChild(nextChar);
                            if (child != null)
                            {
                                Hypothesis h = hypothesis.GetNewMoveForward(child, 0, Operation.NO_ERROR);
                                h.SetWord(child);
                                newHypotheses.Add(h);
                                if (nextIndex >= input.Length - 1)
                                {
                                    if (h.Node.word != null)
                                    {
                                        AddHypothesis(h);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (char c in cc)
                            {
                                Node child = hypothesis.Node.GetImmediateChild(c);
                                if (child == null)
                                {
                                    continue;
                                }
                                Hypothesis h = hypothesis.GetNewMoveForward(child, 0, Operation.NO_ERROR);
                                h.SetWord(child);
                                newHypotheses.Add(h);
                                if (nextIndex >= input.Length - 1)
                                {
                                    if (h.Node.word != null)
                                    {
                                        AddHypothesis(h);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (hypothesis.Node.word != null)
                {
                    AddHypothesis(hypothesis);
                }

                // we don't need to explore further if we reached to max penalty
                if (hypothesis.Penalty >= decoder.MaxPenalty)
                {
                    return newHypotheses;
                }

                // For reducing List creation. IF there is no epsilon connection, retrieve the
                // internal data structure iterator.
                IEnumerable<Node> allChildNodes = hypothesis.Node.HasEpsilonConnection() ?
                    hypothesis.Node.GetAllChildNodes() : hypothesis.Node.GetImmediateChildNodeIterable();

                if (nextIndex < input.Length)
                {
                    // substitution
                    foreach (Node child in allChildNodes)
                    {
                        float penalty = 0;
                        if (decoder.CheckNearKeySubstitution)
                        {
                            if (child.chr != nextChar)
                            {
                                string nearCharactersString = decoder.nearKeyMap.GetValueOrDefault(child.chr);
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

                        if (penalty > 0 && hypothesis.Penalty + penalty <= decoder.MaxPenalty)
                        {
                            Hypothesis h = hypothesis.GetNewMoveForward(
                                child,
                                penalty,
                                Operation.SUBSTITUTION);
                            h.SetWord(child);
                            if (nextIndex == input.Length - 1)
                            {
                                if (h.Node.word != null)
                                {
                                    AddHypothesis(h);
                                }
                            }
                            else
                            {
                                newHypotheses.Add(h);
                            }
                        }
                    }
                }

                if (hypothesis.Penalty + DeletionPenalty > decoder.MaxPenalty)
                {
                    return newHypotheses;
                }

                // deletion
                newHypotheses
                    .Add(hypothesis.GetNewMoveForward(hypothesis.Node, DeletionPenalty, Operation.DELETION));

                // insertion
                foreach (Node child in allChildNodes)
                {
                    Hypothesis h = hypothesis.GetNew(child, InsertionPenalty, Operation.INSERTION);
                    h.SetWord(child);
                    newHypotheses.Add(h);
                }

                // transposition
                // TODO: make length check parametric. Also eliminate gross code duplication
                if (input.Length > 2 && nextIndex < input.Length - 1)
                {
                    char transpose = input[nextIndex + 1];
                    if (matcher != null)
                    {
                        char[] tt = matcher.Matches(transpose);
                        char[] cc = matcher.Matches(nextChar);
                        foreach (char t in tt)
                        {
                            List<Node> nextNodes = hypothesis.Node.GetChildList(t);
                            foreach (Node nextNode in nextNodes)
                            {
                                foreach (char c in cc)
                                {
                                    if (hypothesis.Node.HasChild(t) && nextNode.HasChild(c))
                                    {
                                        foreach (Node n in nextNode.GetChildList(c))
                                        {
                                            Hypothesis h = hypothesis.GetNew(
                                                n,
                                                TranspositionPenalty,
                                                nextIndex + 1,
                                                Operation.TRANSPOSITION);
                                            h.SetWord(n);
                                            if (nextIndex == input.Length - 1)
                                            {
                                                if (h.Node.word != null)
                                                {
                                                    AddHypothesis(h);
                                                }
                                            }
                                            else
                                            {
                                                newHypotheses.Add(h);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        List<Node> nextNodes = hypothesis.Node.GetChildList(transpose);
                        foreach (Node nextNode in nextNodes)
                        {
                            if (hypothesis.Node.HasChild(transpose) && nextNode.HasChild(nextChar))
                            {
                                foreach (Node n in nextNode.GetChildList(nextChar))
                                {
                                    Hypothesis h = hypothesis.GetNew(
                                        n,
                                        TranspositionPenalty,
                                        nextIndex + 1,
                                        Operation.TRANSPOSITION);
                                    h.SetWord(n);
                                    if (nextIndex == input.Length - 1)
                                    {
                                        if (h.Node.word != null)
                                        {
                                            AddHypothesis(h);
                                        }
                                    }
                                    else
                                    {
                                        newHypotheses.Add(h);
                                    }
                                }
                            }
                        }
                    }
                }
                return newHypotheses;
            }

            private void AddHypothesis(Hypothesis hypothesis)
            {
                string hypWord = hypothesis.GetContent();
                if (!finished.Contains(hypWord))
                {
                    finished.Set(hypWord, hypothesis.Penalty);
                }
                else if (finished.Get(hypWord) > hypothesis.Penalty)
                {
                    finished.Set(hypWord, hypothesis.Penalty);
                }
            }
        }
    }
}
