using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    public class StemTransitionsTrieBased : StemTransitionsBase, IStemTransitions
    {
        private Trie<StemTransition> stemTransitionTrie = new Trie<StemTransition>();

        // contains a map that holds dictionary items that has multiple or
        // different than item.root stem surface forms.
        private MultiMap<DictionaryItem, StemTransition> differentStemItems = new MultiMap<DictionaryItem, StemTransition>(1000);

        private object lockObject = new object();

        public StemTransitionsTrieBased(RootLexicon lexicon, TurkishMorphotactics morphotactics)
        {
            this.lexicon = lexicon;
            this.morphotactics = morphotactics;
            foreach (var dictionaryItem in lexicon)
            {
                AddTransitions(dictionaryItem);
            }
        }

        public IEnumerable<StemTransition> GetTransitions()
        {
            lock (lockObject)
            {
                return stemTransitionTrie.GetAll();
            }
        }

        public RootLexicon GetLexicon()
        {
            return lexicon;
        }

        public List<StemTransition> GetPrefixMatches(string stem, bool asciiTolerant)
        {
            lock (lockObject)
            {
                return stemTransitionTrie.GetPrefixMatchingItems(stem);
            }
        }

        public List<StemTransition> GetTransitions(DictionaryItem item)
        {
            if (differentStemItems.ContainsKey(item))
            {
                return differentStemItems[item];
            }
            else
            {
                List<StemTransition> transitions = stemTransitionTrie.GetItems(item.root);
                return transitions.Where(s => s.item.Equals(item)).ToList();
            }
        }

        public void AddDictionaryItem(DictionaryItem item)
        {
            lock (lockObject)
            {
                try
                {
                    AddTransitions(item);
                }
                catch (Exception e)
                {
                    Log.Warn("Cannot generate stem transition for {0} with reason {1}", item, e.Message);
                }
            }
        }

        private void AddTransitions(DictionaryItem item)
        {
            lock (lockObject)
            {
                List<StemTransition> transitions = Generate(item);
                foreach (StemTransition transition in transitions)
                {
                    stemTransitionTrie.Add(transition.surface, transition);
                }
                if (transitions.Count > 1 || (transitions.Count == 1 && !item.root
                    .Equals(transitions[0].surface)))
                {
                    differentStemItems.Add(item, transitions);
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
                        stemTransitionTrie.Remove(transition.surface, transition);
                    }
                    if (differentStemItems.ContainsKey(item))
                    {
                        differentStemItems.Remove(item);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("Cannot remove {0} ", e.Message);
                }
            }
        }
    }
}
