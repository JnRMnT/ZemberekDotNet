using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    /// <summary>
    /// Represents a transition in morphotactics graph.
    /// </summary>
    public abstract class MorphemeTransition
    {
        public MorphemeState from;
        public MorphemeState to;

        // Defines the condition(s) to allow or block a graph visitor (SearchPath).
        // A condition can be a single or a group of objects that has Condition interface.
        // For example, if condition is HasPhoneticAttribute(LastLetterVowel), and SearchPath's last
        // letter is a consonant, it cannot pass this transition.
        private ICondition condition;
        private int conditionCount;

        internal ICondition Condition { get => condition; set => condition = value; }
        internal int ConditionCount { get => conditionCount; set => conditionCount = value; }

        public MorphemeState From()
        {
            return from;
        }

        public MorphemeState To()
        {
            return to;
        }

        public ICondition GetCondition()
        {
            return Condition;
        }

        public int GetConditionCount()
        {
            return ConditionCount;
        }

        public abstract MorphemeTransition GetCopy();
    }
}
