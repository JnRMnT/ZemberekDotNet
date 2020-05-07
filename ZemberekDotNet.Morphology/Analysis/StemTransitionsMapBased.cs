using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    public class StemTransitionsMapBased : StemTransitionsBase, IStemTransitions
    {
        private MultiMap<string, StemTransition> multiStems = new MultiMap<string, StemTransition>(1000);
        private MultiMap<string, string> asciiKeys = null;

        // contains a map that holds dictionary items that has
        // multiple or different than item.root stem surface forms.
        private MultiMap<DictionaryItem, StemTransition> differentStemItems = new MultiMap<DictionaryItem, StemTransition>(1000);

        private ConcurrentDictionary<string, StemTransition> singleStems = new ConcurrentDictionary<string, StemTransition>();

        private object lockObject = new object();

        public StemTransitionsMapBased(RootLexicon lexicon, TurkishMorphotactics morphotactics)
        {
            this.lexicon = lexicon;
            this.morphotactics = morphotactics;
            foreach (var item in lexicon)
            {
                this.AddDictionaryItem(item);
            }
        }

        //TODO: this is kind of a hack. Because StemTransitions may be shared between
        // analyzer classes, this may be necessary when one of them happens to be ascii tolerant
        // and other is not.
        private void GenerateAsciiTolerantMap()
        {
            lock (lockObject)
            {
                asciiKeys = new MultiMap<string, string>(1000);
                // generate MultiMap for ascii tolerant keys
                foreach (string s in singleStems.Keys)
                {
                    string ascii = TurkishAlphabet.Instance.ToAscii(s);

                    if (TurkishAlphabet.Instance.ContainsAsciiRelated(s))
                    {
                        asciiKeys.Add(ascii, s);
                    }
                }
                foreach (StemTransition st in multiStems.Values)
                {
                    string s = st.surface;
                    string ascii = TurkishAlphabet.Instance.ToAscii(s);
                    if (TurkishAlphabet.Instance.ContainsAsciiRelated(s))
                    {
                        asciiKeys.Add(ascii, s);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<StemTransition> GetTransitions() {
            HashSet<StemTransition> result = new HashSet<StemTransition>(singleStems.Values);
            result.AddRange(multiStems.Values);
            return result;
        }

        public RootLexicon GetLexicon()
        {
            return lexicon;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AddStemTransition(StemTransition stemTransition)
        {
            string surfaceForm = stemTransition.surface;
            if (multiStems.ContainsKey(surfaceForm))
            {
                multiStems.Add(surfaceForm, stemTransition);
            }
            else if (singleStems.ContainsKey(surfaceForm))
            {
                multiStems.Add(surfaceForm, singleStems.GetValueOrDefault(surfaceForm));
                singleStems.TryRemove(surfaceForm, out StemTransition value);
                multiStems.Add(surfaceForm, stemTransition);
            }
            else
            {
                singleStems.TryAdd(surfaceForm, stemTransition);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void RemoveStemNode(StemTransition stemTransition)
        {
            string surfaceForm = stemTransition.surface;
            if (multiStems.ContainsKey(surfaceForm))
            {
                multiStems.Remove(surfaceForm, stemTransition);
            }
            else if (singleStems.ContainsKey(surfaceForm)
              && singleStems.GetValueOrDefault(surfaceForm).item.Equals(stemTransition.item))
            {
                singleStems.TryRemove(surfaceForm, out StemTransition value);
            }
            if (!differentStemItems.ContainsEntry(stemTransition.item, stemTransition))
            {
                differentStemItems.Remove(stemTransition.item, stemTransition);
            }
        }

        private LinkedHashSet<StemTransition> GetTransitionsAsciiTolerant(string stem)
        {
            lock (lockObject)
            {
                // add actual
                LinkedHashSet<StemTransition> result = new LinkedHashSet<StemTransition>();
                if (singleStems.ContainsKey(stem))
                {
                    result.Add(singleStems.GetValueOrDefault(stem));
                }
                else if (multiStems.ContainsKey(stem))
                {
                    result.AddRange(multiStems[stem]);
                }
                List<string> asciiStems = asciiKeys[TurkishAlphabet.Instance.ToAscii(stem)];
                foreach (string st in asciiStems)
                {
                    if (singleStems.ContainsKey(st))
                    {
                        result.Add(singleStems.GetValueOrDefault(st));
                    }
                    else if (multiStems.ContainsKey(st))
                    {
                        result.AddRange(multiStems[st]);
                    }
                }
                return result;
            }
        }

        private List<StemTransition> GetTransitions(string stem)
        {
            lock (lockObject)
            {
                if (singleStems.ContainsKey(stem))
                {
                    return new List<StemTransition> { singleStems.GetValueOrDefault(stem) };
                }
                else if (multiStems.ContainsKey(stem))
                {
                    return new List<StemTransition>(multiStems[stem]);
                }
                else
                {
                    return new List<StemTransition>();
                }
            }
        }

        public List<StemTransition> GetPrefixMatches(string input, bool asciiTolerant)
        {
            if (asciiKeys == null && asciiTolerant)
            {
                GenerateAsciiTolerantMap();
            }
            lock (lockObject)
            {
                List<StemTransition> matches = new List<StemTransition>(3);
                for (int i = 1; i <= input.Length; i++)
                {
                    String stem = input.Substring(0, i);
                    if (asciiTolerant)
                    {
                        matches.AddRange(GetTransitionsAsciiTolerant(stem));
                    }
                    else
                    {
                        matches.AddRange(GetTransitions(stem));
                    }
                }
                return matches;
            }
        }

        public List<StemTransition> GetTransitions(DictionaryItem item)
        {
            lock (lockObject)
            {
                if (differentStemItems.ContainsKey(item))
                {
                    return differentStemItems[item];
                }
                else
                {

                    List<StemTransition> transitions = GetTransitions(item.root);
                    return transitions.Where(s => s.item.Equals(item)).ToList();
                }
            }
        }

        public void AddDictionaryItem(DictionaryItem item)
        {
            lock (lockObject)
            {
                try
                {
                    List<StemTransition> transitions = Generate(item);
                    foreach (StemTransition transition in transitions)
                    {
                        AddStemTransition(transition);
                    }
                    if (transitions.Count > 1 || (transitions.Count == 1 && !item.root
                        .Equals(transitions[0].surface)))
                    {
                        differentStemItems.Add(item, transitions);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("Cannot generate stem transition for {0} with reason {1}", item, e.Message);
                }
            }
        }

        public void RemoveDictionaryItem(DictionaryItem item)
        {
            lock (lockObject)
            {
                try
                {
                    List<StemTransition> transitions = Generate(item);
                    foreach (StemTransition transition in transitions)
                    {
                        RemoveStemNode(transition);
                    }
                    if (differentStemItems.ContainsKey(item))
                    {
                        differentStemItems.RemoveAll(item);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("Cannot remove {0} ", item, e.Message);
                }
            }
        }
    }
}
