using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    /// <summary>
    /// Represents a state in morphotactics graph.
    /// </summary>
    public class MorphemeState
    {
        public readonly string id;
        public readonly Morpheme morpheme;
        private List<MorphemeTransition> outgoing = new List<MorphemeTransition>(2);
        private List<MorphemeTransition> incoming = new List<MorphemeTransition>(2);
        public readonly bool terminal;
        public readonly bool derivative;
        public readonly bool posRoot;

        MorphemeState(
            string id,
            Morpheme morpheme,
            bool terminal,
            bool derivative,
            bool posRoot)
        {
            this.morpheme = morpheme;
            this.id = id;
            this.terminal = terminal;
            this.derivative = derivative;
            this.posRoot = posRoot;
        }

        public static MorphemeStateBuilder Builder(string id, Morpheme morpheme)
        {
            return new MorphemeStateBuilder(id, morpheme);
        }

        public class MorphemeStateBuilder
        {
            internal readonly string _id;
            internal readonly Morpheme _morpheme;
            private bool _terminal = false;
            private bool _derivative = false;
            private bool _posRoot = false;

            public MorphemeStateBuilder(string _id, Morpheme _morpheme)
            {
                this._id = _id;
                this._morpheme = _morpheme;
            }

            public MorphemeStateBuilder Terminal()
            {
                this._terminal = true;
                return this;
            }

            public MorphemeStateBuilder Derivative()
            {
                this._derivative = true;
                return this;
            }

            public MorphemeStateBuilder PosRoot()
            {
                this._posRoot = true;
                return this;
            }

            public MorphemeState Build()
            {
                return new MorphemeState(_id, _morpheme, _terminal, _derivative, _posRoot);
            }
        }

        public static MorphemeState Terminal(String id, Morpheme morpheme)
        {
            return Builder(id, morpheme).Terminal().Build();
        }


        public static MorphemeState NonTerminal(String id, Morpheme morpheme)
        {
            return Builder(id, morpheme).Build();
        }

        public static MorphemeState TerminalDerivative(String id, Morpheme morpheme)
        {
            return Builder(id, morpheme).Terminal().Derivative().Build();
        }

        public static MorphemeState NonTerminalDerivative(String id, Morpheme morpheme)
        {
            return Builder(id, morpheme).Derivative().Build();
        }

        public MorphemeState AddOutgoing(params MorphemeTransition[] suffixTransitions)
        {
            foreach (MorphemeTransition suffixTransition in suffixTransitions)
            {
                if (outgoing.Contains(suffixTransition))
                {
                    Log.Warn("Outgoing transition {0} already exist in {1}", suffixTransition, this);
                }
                outgoing.Add(suffixTransition);
            }
            return this;
        }

        public MorphemeState AddIncoming(params MorphemeTransition[] suffixTransitions)
        {
            foreach (MorphemeTransition suffixTransition in suffixTransitions)
            {
                if (incoming.Contains(suffixTransition))
                {
                    Log.Warn("Incoming transition {0} already exist in {1}", suffixTransition, this);
                }
                incoming.Add(suffixTransition);
            }
            return this;
        }

        public SuffixTransition.SuffixTransitionBuilder Transition(MorphemeState to)
        {
            return new SuffixTransition.SuffixTransitionBuilder().From(this).To(to);
        }

        public SuffixTransition.SuffixTransitionBuilder Transition(MorphemeState to, String template)
        {
            return new SuffixTransition.SuffixTransitionBuilder().SurfaceTemplate(template).From(this).To(to);
        }

        public MorphemeState Add(MorphemeState to, String template, ICondition condition)
        {
            new SuffixTransition.SuffixTransitionBuilder().SurfaceTemplate(template)
                .SetCondition(condition)
                .From(this).To(to).Build();
            return this;
        }

        public MorphemeState AddEmpty(MorphemeState to, ICondition condition)
        {
            new SuffixTransition.SuffixTransitionBuilder().SetCondition(condition)
                .From(this).To(to).Build();
            return this;
        }

        public MorphemeState Add(MorphemeState to, String template)
        {
            new SuffixTransition.SuffixTransitionBuilder().SurfaceTemplate(template)
                .From(this).To(to).Build();
            return this;
        }

        public MorphemeState AddEmpty(MorphemeState to)
        {
            new SuffixTransition.SuffixTransitionBuilder().From(this).To(to).Build();
            return this;
        }

        public void DumpTransitions()
        {
            foreach (MorphemeTransition transition in outgoing)
            {
                Console.WriteLine(transition.Condition);
            }
        }

        public override string ToString()
        {
            return "[" + id + ":" + morpheme.Id + "]";
        }

        public List<MorphemeTransition> GetOutgoing()
        {
            return outgoing;
        }

        public List<MorphemeTransition> GetIncoming()
        {
            return incoming;
        }

        public void CopyOutgoingTransitionsFrom(MorphemeState state)
        {
            foreach (MorphemeTransition transition in state.outgoing)
            {
                MorphemeTransition copy = transition.GetCopy();
                copy.from = this;
                this.AddOutgoing(transition);
            }
        }

        public void RemoveTransitionsTo(MorphemeState state)
        {
            List<MorphemeTransition> transitions = new List<MorphemeTransition>(2);
            foreach (MorphemeTransition transition in outgoing)
            {
                if (transition.to.Equals(state))
                {
                    transitions.Add(transition);
                }
            }
            outgoing.Remove(transitions);
        }

        public void RemoveTransitionsTo(Morpheme morpheme)
        {
            List<MorphemeTransition> transitions = new List<MorphemeTransition>(2);
            foreach (MorphemeTransition transition in outgoing)
            {
                if (transition.to.morpheme.Equals(morpheme))
                {
                    transitions.Add(transition);
                }
            }
            outgoing.Remove(transitions);
        }

        public void RemoveTransitionsTo(params MorphemeState[] state)
        {
            foreach (MorphemeState morphemeState in state)
            {
                RemoveTransitionsTo(morphemeState);
            }
        }

        public void RemoveTransitionsTo(params Morpheme[] morphemes)
        {
            foreach (Morpheme morpheme in morphemes)
            {
                RemoveTransitionsTo(morpheme);
            }
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !GetType().Equals(o.GetType()))
            {
                return false;
            }
            MorphemeState that = (MorphemeState)o;
            return id.Equals(that.id);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
