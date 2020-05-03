using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// This class represents a path in morphotactics graph.During analysis many SearchPaths are created
    /// and surviving paths are used for generating analysis results.
    /// </summary>
    public class SearchPath
    {
        // letters to parse.
        internal string tail;

        internal MorphemeState currentState;

        internal List<SurfaceTransition> transitions;

        internal AttributeSet<PhoneticAttribute> phoneticAttributes;

        private bool terminal;
        private bool containsDerivation = false;
        private bool containsSuffixWithSurface = false;

        public static SearchPath InitialPath(StemTransition stemTransition, String tail)
        {
            List<SurfaceTransition> morphemes = new List<SurfaceTransition>(4);
            SurfaceTransition root = new SurfaceTransition(stemTransition.surface, stemTransition);
            morphemes.Add(root);
            return new SearchPath(
                tail,
                stemTransition.to,
                morphemes,
                stemTransition.GetPhoneticAttributes().Copy(),
                stemTransition.to.terminal);
        }

        private SearchPath(
            string tail,
            MorphemeState currentState,
            List<SurfaceTransition> transitions,
            AttributeSet<PhoneticAttribute> phoneticAttributes,
            bool terminal)
        {
            this.tail = tail;
            this.currentState = currentState;
            this.transitions = transitions;
            this.phoneticAttributes = phoneticAttributes;
            this.terminal = terminal;
        }

        public SearchPath GetCopy(
            SurfaceTransition surfaceNode,
            AttributeSet<PhoneticAttribute> phoneticAttributes)
        {

            bool isTerminal = surfaceNode.GetState().terminal;
            List<SurfaceTransition> hist = new List<SurfaceTransition>(transitions);
            hist.Add(surfaceNode);
            String newTail = tail.Substring(surfaceNode.surface.Length);
            SearchPath path = new SearchPath(
                newTail,
                surfaceNode.GetState(),
                hist,
                phoneticAttributes,
                isTerminal);
            path.containsSuffixWithSurface = containsSuffixWithSurface || !surfaceNode.surface.IsEmpty();
            path.containsDerivation = containsDerivation || surfaceNode.GetState().derivative;
            return path;
        }

        public SearchPath GetCopyForGeneration(
            SurfaceTransition surfaceNode,
            AttributeSet<PhoneticAttribute> phoneticAttributes)
        {

            bool isTerminal = surfaceNode.GetState().terminal;
            List<SurfaceTransition> hist = new List<SurfaceTransition>(transitions);
            hist.Add(surfaceNode);
            SearchPath path = new SearchPath(
                tail,
                surfaceNode.GetState(),
                hist,
                phoneticAttributes,
                isTerminal);
            path.containsSuffixWithSurface = containsSuffixWithSurface || !surfaceNode.surface.IsEmpty();
            path.containsDerivation = containsDerivation || surfaceNode.GetState().derivative;
            return path;
        }

        public override string ToString()
        {
            StemTransition st = GetStemTransition();
            String morphemeStr = string.Join(" + ", transitions.Select(e => e.ToString()));
            return "[(" + st.item.id + ")(-" + tail + ") " + morphemeStr + "]";
        }

        public string GetTail()
        {
            return tail;
        }

        public StemTransition GetStemTransition()
        {
            return (StemTransition)transitions[0].lexicalTransition;
        }

        public MorphemeState GetCurrentState()
        {
            return currentState;
        }

        public MorphemeState GetPreviousState()
        {
            if (transitions.Count < 2)
            {
                return null;
            }
            return transitions[transitions.Count - 2].GetState();
        }

        public AttributeSet<PhoneticAttribute> GetPhoneticAttributes()
        {
            return phoneticAttributes;
        }

        public bool ContainsPhoneticAttribute(PhoneticAttribute attribute)
        {
            return phoneticAttributes.Contains(attribute);
        }

        public bool IsTerminal()
        {
            return terminal;
        }

        public List<SurfaceTransition> GetTransitions()
        {
            return transitions;
        }

        public bool ContainsDerivation()
        {
            return containsDerivation;
        }

        public bool ContainsSuffixWithSurface()
        {
            return containsSuffixWithSurface;
        }

        public bool HasDictionaryItem(DictionaryItem item)
        {
            // TODO: for performance, probably it is safe to check references only.
            return item.Equals(GetStemTransition().item);
        }

        public SurfaceTransition GetLastTransition()
        {
            return transitions[transitions.Count - 1];
        }

        public DictionaryItem GetDictionaryItem()
        {
            return GetStemTransition().item;
        }
    }
}
