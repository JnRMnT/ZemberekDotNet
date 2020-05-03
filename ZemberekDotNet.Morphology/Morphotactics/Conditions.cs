using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public class Conditions
    {
        public static readonly ICondition HAS_TAIL = new HasTail();
        public static readonly ICondition HAS_NO_TAIL = new HasNoTail();
        internal static readonly ICondition HAS_SURFACE = new HasAnySuffixSurface();
        internal static readonly ICondition HAS_NO_SURFACE = new HasAnySuffixSurface().Not();
        internal static readonly ICondition CURRENT_GROUP_EMPTY = new NoSurfaceAfterDerivation();
        internal static readonly ICondition CURRENT_GROUP_NOT_EMPTY = new NoSurfaceAfterDerivation().Not();
        internal static readonly ICondition HAS_DERIVATION = new HasDerivation();
        internal static readonly ICondition HAS_NO_DERIVATION = Not(new HasDerivation());


        internal static ICondition Has(RootAttribute attribute)
        {
            return new HasRootAttribute(attribute);
        }

        internal static ICondition Has(PhoneticAttribute attribute)
        {
            return new HasPhoneticAttribute(attribute);
        }

        internal static ICondition RootIs(DictionaryItem item)
        {
            return new DictionaryItemIs(item);
        }

        internal static ICondition RootPrimaryPos(PrimaryPos pos)
        {
            return new RootPrimaryPosIs(pos);
        }

        internal static ICondition RootIsAny(params DictionaryItem[] items)
        {
            return new DictionaryItemIsAny(items);
        }

        internal static ICondition RootIsNone(params DictionaryItem[] items)
        {
            return new DictionaryItemIsNone(items);
        }

        internal static ICondition NotHave(RootAttribute attribute)
        {
            return new HasRootAttribute(attribute).Not();
        }

        public static ICondition NotHaveAny(params RootAttribute[] attributes)
        {
            return new HasAnyRootAttribute(attributes).Not();
        }

        internal static ICondition NotHave(PhoneticAttribute attribute)
        {
            return new HasPhoneticAttribute(attribute).Not();
        }

        internal static ICondition RootIsNot(DictionaryItem item)
        {
            return new DictionaryItemIs(item).Not();
        }

        public static ICondition CurrentMorphemeIsCondition(Morpheme morpheme)
        {
            return new CurrentMorphemeIs(morpheme);
        }

        public static ICondition CurrentMorphemeIsAnyCondition(params Morpheme[] morphemes)
        {
            return new CurrentMorphemeIsAny(morphemes);
        }

        public static ICondition LastMorphemeIsNot(Morpheme morpheme)
        {
            return new CurrentMorphemeIs(morpheme).Not();
        }

        internal static ICondition CurrentStateIsCondition(MorphemeState state)
        {
            return new CurrentStateIs(state);
        }

        internal static ICondition CurrentStateIsNotCondition(MorphemeState state)
        {
            return new CurrentStateIsNot(state);
        }

        internal static ICondition PreviousStateIsCondition(MorphemeState state)
        {
            return new PreviousStateIs(state);
        }

        internal static ICondition PreviousStateIsNotCondition(MorphemeState state)
        {
            return new PreviousStateIsNot(state);
        }

        internal static ICondition PreviousMorphemeIsCondition(Morpheme morpheme)
        {
            return new PreviousMorphemeIs(morpheme);
        }

        internal static ICondition PreviousMorphemeIsNot(Morpheme morpheme)
        {
            return new PreviousMorphemeIs(morpheme).Not();
        }

        public static ICondition And(ICondition left, ICondition right)
        {
            return Condition(Operator.AND, left, right);
        }

        public static ICondition And<T>(ICollection<T> conditions) where T : ICondition
        {
            return Condition(Operator.AND, conditions);
        }

        public static ICondition And(params ICondition[] conditions)
        {
            return Condition(Operator.AND, conditions);
        }

        public static ICondition Condition(Operator op, ICondition left, ICondition right)
        {
            return CombinedCondition.Of(op, left, right);
        }

        public static ICondition Condition(Operator op, params ICondition[] conditions)
        {
            return Condition(op, conditions.ToList());
        }

        public static ICondition Condition<T>(Operator op, ICollection<T> conditions) where T : ICondition
        {
            return CombinedCondition.Of(op, conditions);
        }

        public static ICondition Or(ICondition left, ICondition right)
        {
            return Condition(Operator.OR, left, right);
        }

        public static ICondition Or(params ICondition[] conditions)
        {
            return Condition(Operator.OR, conditions);
        }

        public static ICondition Or<T>(ICollection<T> conditions) where T : ICondition
        {
            return Condition(Operator.OR, conditions);
        }

        public static ICondition Not(ICondition condition)
        {
            return condition.Not();
        }

        private class HasRootAttribute : AbstractCondition
        {
            RootAttribute attribute;

            internal HasRootAttribute(RootAttribute attribute)
            {
                this.attribute = attribute;
            }

            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetDictionaryItem().HasAttribute(attribute);
            }

            public override string ToString()
            {
                return "HasRootAttribute{" + attribute + '}';
            }
        }

        private class HasAnyRootAttribute : AbstractCondition
        {
            RootAttribute[]
            attributes;

            internal HasAnyRootAttribute(params RootAttribute[]
            attributes)
            {
                this.attributes = (RootAttribute[])attributes.Clone();
            }

            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetDictionaryItem().HasAnyAttribute(attributes);
            }

            public override string ToString()
            {
                return "HasAnyRootAttribute{" + Arrays.ToString(attributes) + '}';
            }
        }

        private class HasPhoneticAttribute : AbstractCondition
        {
            PhoneticAttribute attribute;

            internal HasPhoneticAttribute(PhoneticAttribute attribute)
            {
                this.attribute = attribute;
            }

            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetPhoneticAttributes().Contains(attribute);
            }

            public override string ToString()
            {
                return "HasPhoneticAttribute{" + attribute + '}';
            }
        }

        private class DictionaryItemIs : AbstractCondition
        {
            DictionaryItem item;

            internal DictionaryItemIs(DictionaryItem item)
            {
                this.item = item;
            }

            public override bool Accept(SearchPath visitor)
            {
                return item != null && visitor.HasDictionaryItem(item);
            }

            public override string ToString()
            {
                return "DictionaryItemIs{" + item + '}';
            }
        }

        private class RootPrimaryPosIs : AbstractCondition
        {
            internal PrimaryPos pos;

            internal RootPrimaryPosIs(PrimaryPos pos)
            {
                this.pos = pos;
            }

            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetDictionaryItem().primaryPos == pos;
            }

            public override string ToString()
            {
                return "RootPrimaryPosIs{" + pos + '}';
            }
        }

        public class SecondaryPosIs : AbstractCondition
        {
            SecondaryPos pos;

            internal SecondaryPosIs(SecondaryPos pos)
            {
                this.pos = pos;
            }

            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetDictionaryItem().secondaryPos == pos;
            }

            public override string ToString()
            {
                return "SecondaryPosIs{" + pos + '}';
            }
        }

        private class DictionaryItemIsAny : AbstractCondition
        {
            HashSet<DictionaryItem> items;

            internal DictionaryItemIsAny(params DictionaryItem[]
            items)
            {
                this.items = new HashSet<DictionaryItem>(new List<DictionaryItem>(items));
            }

            public override bool Accept(SearchPath visitor)
            {
                return items.Contains(visitor.GetDictionaryItem());
            }

            public override string ToString()
            {
                return "DictionaryItemIsAny{" + items + '}';
            }
        }

        private class DictionaryItemIsNone : AbstractCondition
        {
            HashSet<DictionaryItem> items;

            internal DictionaryItemIsNone(params DictionaryItem[]
            items)
            {
                this.items = new HashSet<DictionaryItem>(new List<DictionaryItem>(items));
            }

            public override bool Accept(SearchPath visitor)
            {
                return !items.Contains(visitor.GetDictionaryItem());
            }

            public override string ToString()
            {
                return "DictionaryItemIsNone{" + items + '}';
            }
        }

        public class HasAnySuffixSurface : AbstractCondition
        {
            public override bool Accept(SearchPath visitor)
            {
                return visitor.ContainsSuffixWithSurface();
            }

            public override string ToString()
            {
                return "HasAnySuffixSurface{}";
            }
        }

        // accepts if visitor has letters to consume.
        public class HasTail : AbstractCondition
        {
            public override bool Accept(SearchPath visitor)
            {
                return !visitor.GetTail().IsEmpty();
            }
            public override string ToString()
            {
                return "HasTail{}";
            }
        }

        public class HasNoTail : AbstractCondition
        {
            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetTail().IsEmpty();
            }

            public override string ToString()
            {
                return "HasNoTail{}";
            }
        }

        public class HasTailSequence : AbstractCondition
        {
            Morpheme[]
            morphemes;

            public HasTailSequence(params Morpheme[]
            morphemes)
            {
                this.morphemes = morphemes;
            }

            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> forms = visitor.GetTransitions();
                if (forms.Count < morphemes.Length)
                {
                    return false;
                }
                int i = 0;
                int j = forms.Count - morphemes.Length;
                while (i < morphemes.Length)
                {
                    if (morphemes[i++] != forms[j++].GetMorpheme())
                    {
                        return false;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                return "HasTailSequence{" + Arrays.ToString(morphemes) + "}";
            }
        }

        public class ContainsMorphemeSequence : AbstractCondition
        {
            Morpheme[]
            morphemes;

            public ContainsMorphemeSequence(params Morpheme[]
            morphemes)
            {
                this.morphemes = morphemes;
            }

            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> forms = visitor.GetTransitions();
                if (forms.Count < morphemes.Length)
                {
                    return false;
                }
                int m = 0;
                foreach (SurfaceTransition form in forms)
                {
                    if (form.GetMorpheme().Equals(morphemes[m]))
                    {
                        m++;
                        if (m == morphemes.Length)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        m = 0;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return "ContainsMorphemeSequence{" + Arrays.ToString(morphemes) + "}";
            }
        }


        public class CurrentMorphemeIs : AbstractCondition
        {
            Morpheme morpheme;

            public CurrentMorphemeIs(Morpheme morpheme)
            {
                this.morpheme = morpheme;
            }

            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetCurrentState().morpheme.Equals(morpheme);
            }

            public override string ToString()
            {
                return "CurrentMorphemeIs{ " + morpheme + " }";
            }
        }

        public class PreviousMorphemeIs : AbstractCondition
        {

            Morpheme morpheme;

            internal PreviousMorphemeIs(Morpheme morpheme)
            {
                this.morpheme = morpheme;
            }

            public override bool Accept(SearchPath visitor)
            {
                MorphemeState previousState = visitor.GetPreviousState();
                return previousState != null && previousState.morpheme.Equals(this.morpheme);
            }

            public override string ToString()
            {
                return "PreviousMorphemeIs{ " + morpheme + " }";
            }
        }


        public class PreviousStateIs : AbstractCondition
        {
            MorphemeState state;

            internal PreviousStateIs(MorphemeState state)
            {
                this.state = state;
            }

            public override bool Accept(SearchPath visitor)
            {
                MorphemeState previousState = visitor.GetPreviousState();
                return previousState != null && previousState.Equals(this.state);
            }

            public override string ToString()
            {
                return "PreviousStateIs{ " + state + " }";
            }
        }

        public class PreviousStateIsNot : AbstractCondition
        {

            MorphemeState state;

            internal PreviousStateIsNot(MorphemeState state)
            {
                this.state = state;
            }

            public override bool Accept(SearchPath visitor)
            {
                MorphemeState previousState = visitor.GetPreviousState();
                return previousState == null || !previousState.Equals(this.state);
            }

            public override string ToString()
            {
                return "PreviousStateIsNot{ " + state + " }";
            }
        }

        public class RootSurfaceIs : AbstractCondition
        {
            string surface;
            internal RootSurfaceIs(string surface)
            {
                this.surface = surface;
            }


            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetStemTransition().surface.Equals(this.surface);
            }

            public override string ToString()
            {
                return "RootSurfaceIs{ " + surface + " }";
            }
        }

        public class RootSurfaceIsAny : AbstractCondition
        {
            string[]
            surfaces;

            internal RootSurfaceIsAny(params string[] surfaces)
            {
                this.surfaces = surfaces;
            }

            public override bool Accept(SearchPath visitor)
            {
                foreach (string s in surfaces)
                {
                    if (visitor.GetStemTransition().surface.Equals(s))
                    {
                        return true;
                    }
                }
                return false;
            }
            public override string ToString()
            {
                return "RootSurfaceIsAny{ " + Arrays.ToString(surfaces) + " }";
            }
        }

        public class CurrentStateIs : AbstractCondition
        {
            MorphemeState state;

            internal CurrentStateIs(MorphemeState state)
            {
                this.state = state;
            }

            public override bool Accept(SearchPath visitor)
            {
                return visitor.GetCurrentState().Equals(state);
            }

            public override string ToString()
            {
                return "CurrentStateIs{ " + state + " }";
            }
        }

        public class CurrentStateIsNot : AbstractCondition
        {
            MorphemeState state;

            internal CurrentStateIsNot(MorphemeState state)
            {
                this.state = state;
            }
            public override bool Accept(SearchPath visitor)
            {
                return !visitor.GetCurrentState().Equals(state);
            }

            public override string ToString()
            {
                return "CurrentStateIsNot{ " + state + " }";
            }
        }

        public class NotCondition : AbstractCondition
        {
            ICondition condition;

            internal NotCondition(ICondition condition)
            {
                this.condition = condition;
            }

            public override bool Accept(SearchPath visitor)
            {
                return !condition.Accept(visitor);
            }


            public override string ToString()
            {
                return "Not(" + condition + ")";
            }
        }

        internal static ICondition LastDerivationIsCondition(MorphemeState state)
        {
            return new LastDerivationIs(state);
        }

        public class LastDerivationIs : AbstractCondition
        {
            MorphemeState state;

            internal LastDerivationIs(MorphemeState state)
            {
                this.state = state;
            }


            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();
                for (int i = suffixes.Count - 1; i > 0; i--)
                {
                    SurfaceTransition sf = suffixes[i];
                    if (sf.GetState().derivative)
                    {
                        return sf.GetState() == state;
                    }
                }
                return false;
            }


            public override string ToString()
            {
                return "LastDerivationIs{" + state + '}';
            }
        }

        public class HasDerivation : AbstractCondition
        {

            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();
                foreach (SurfaceTransition suffix in suffixes)
                {
                    if (suffix.GetState().derivative)
                    {
                        return true;
                    }
                }
                return false;
            }


            public override string ToString()
            {
                return "HasDerivation";
            }
        }

        public class LastDerivationIsAny : AbstractCondition
        {
            HashSet<MorphemeState> states;

            internal LastDerivationIsAny(params MorphemeState[] states)
            {
                this.states = new HashSet<MorphemeState>(states.Length);
                this.states.AddRange(Arrays.AsList(states));
            }

            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();
                for (int i = suffixes.Count - 1; i > 0; i--)
                {
                    SurfaceTransition sf = suffixes[i];
                    if (sf.GetState().derivative)
                    {
                        return states.Contains(sf.GetState());
                    }
                }
                return false;
            }


            public override string ToString()
            {
                return "LastDerivationIsAny{" + states + '}';
            }
        }


        // Checks if any of the "MorphemeState" in "states" exist in current Inflectional Group.
        // If previous group starts after a derivation, derivation MorphemeState is also checked.
        public class CurrentGroupContainsAny : AbstractCondition
        {

            HashSet<MorphemeState> states;

            internal CurrentGroupContainsAny(params MorphemeState[] states)
            {
                this.states = new HashSet<MorphemeState>(states.Length);
                this.states.AddRange(states);
            }


            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();
                for (int i = suffixes.Count - 1; i > 0; i--)
                {
                    SurfaceTransition sf = suffixes[i];
                    if (states.Contains(sf.GetState()))
                    {
                        return true;
                    }
                    if (sf.GetState().derivative)
                    {
                        return false;
                    }
                }
                return false;
            }


            public override string ToString()
            {
                return "CurrentGroupContainsAny{" + states + '}';
            }
        }

        // Checks if any of the "MorphemeState" in "states" exist in previous Inflectional Group.
        // If previous group starts after a derivation, derivation MorphemeState is also checked.
        // TODO: this may have a bug. Add test
        public class PreviousGroupContains : AbstractCondition
        {
            HashSet<MorphemeState> states;

            internal PreviousGroupContains(params MorphemeState[] states)
            {
                this.states = new HashSet<MorphemeState>(states.Length);
                this.states.AddRange(states);
            }


            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();

                int lastIndex = suffixes.Count - 1;
                SurfaceTransition sf = suffixes[lastIndex];
                // go back until a transition that is connected to a derivative morpheme.
                while (!sf.GetState().derivative)
                {
                    if (lastIndex == 0)
                    { // there is no previous group. return early.
                        return false;
                    }
                    lastIndex--;
                    sf = suffixes[lastIndex];
                }

                for (int i = lastIndex - 1; i > 0; i--)
                {
                    sf = suffixes[i];
                    if (states.Contains(sf.GetState()))
                    {
                        return true;
                    }
                    if (sf.GetState().derivative)
                    { //could not found the morpheme in this group.
                        return false;
                    }
                }
                return false;
            }


            public override string ToString()
            {
                return "PreviousGroupContains{" + states + '}';
            }
        }

        // Checks if any of the "Morpheme" in "morphemes" exist in previous Inflectional Group.
        // If previous group starts after a derivation, derivation Morpheme is also checked.
        public class PreviousGroupContainsMorpheme : AbstractCondition
        {
            HashSet<Morpheme> morphemes;

            internal PreviousGroupContainsMorpheme(params Morpheme[]
            morphemes)
            {
                this.morphemes = new HashSet<Morpheme>(morphemes.Length);
                this.morphemes.AddRange(morphemes);
            }


            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();

                int lastIndex = suffixes.Count - 1;
                SurfaceTransition sf = suffixes[lastIndex];
                // go back until a transition that is connected to a derivative morpheme.
                while (!sf.GetState().derivative)
                {
                    if (lastIndex == 0)
                    { // there is no previous group. return early.
                        return false;
                    }
                    lastIndex--;
                    sf = suffixes[lastIndex];
                }

                for (int i = lastIndex - 1; i > 0; i--)
                {
                    sf = suffixes[i];
                    if (morphemes.Contains(sf.GetState().morpheme))
                    {
                        return true;
                    }
                    if (sf.GetState().derivative)
                    { //could not found the morpheme in this group.
                        return false;
                    }
                }
                return false;
            }


            public override string ToString()
            {
                return "PreviousGroupContainsMorpheme{" + morphemes + '}';
            }
        }

        // No letters are consumed after derivation occurred.This does not include the transition
        // that caused derivation.
        public class NoSurfaceAfterDerivation : AbstractCondition
        {
            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();
                for (int i = suffixes.Count - 1; i > 0; i--)
                {
                    SurfaceTransition sf = suffixes[i];
                    if (sf.GetState().derivative)
                    {
                        return true;
                    }
                    if (!sf.surface.IsEmpty())
                    {
                        return false;
                    }
                }
                return true;
            }


            public override string ToString()
            {
                return "NoSurfaceAfterDerivation{}";
            }
        }

        public class ContainsMorpheme : AbstractCondition
        {

            HashSet<Morpheme> morphemes;

            internal ContainsMorpheme(params Morpheme[]
            morphemes)
            {
                this.morphemes = new HashSet<Morpheme>(morphemes.Length);
                this.morphemes.AddRange(morphemes);
            }


            public override bool Accept(SearchPath visitor)
            {
                List<SurfaceTransition> suffixes = visitor.GetTransitions();
                foreach (SurfaceTransition suffix in suffixes)
                {
                    if (morphemes.Contains(suffix.GetState().morpheme))
                    {
                        return true;
                    }
                }
                return false;
            }


            public override string ToString()
            {
                return "ContainsMorpheme{" + morphemes + '}';
            }
        }

        public class PreviousMorphemeIsAny : AbstractCondition
        {

            HashSet<Morpheme> morphemes;

            internal PreviousMorphemeIsAny(params Morpheme[]
            morphemes)
            {
                this.morphemes = new HashSet<Morpheme>(morphemes.Length);
                this.morphemes.AddRange(morphemes);
            }

            public override bool Accept(SearchPath path)
            {
                MorphemeState previousState = path.GetPreviousState();
                return previousState != null && morphemes.Contains(previousState.morpheme);
            }


            public override string ToString()
            {
                return "PreviousMorphemeIsAny{" + morphemes + '}';
            }

        }

        public class CurrentMorphemeIsAny : AbstractCondition
        {
            HashSet<Morpheme> morphemes;

            internal CurrentMorphemeIsAny(params Morpheme[]
            morphemes)
            {
                this.morphemes = new HashSet<Morpheme>(morphemes.Length);
                this.morphemes.AddRange(morphemes);
            }


            public override bool Accept(SearchPath path)
            {
                MorphemeState previousState = path.GetCurrentState();
                return previousState != null && morphemes.Contains(previousState.morpheme);
            }


            public override string ToString()
            {
                return "CurrentMorphemeIsAny{" + morphemes + '}';
            }

        }

        public class PreviousStateIsAny : AbstractCondition
        {
            HashSet<MorphemeState> states;

            internal PreviousStateIsAny(params MorphemeState[] states)
            {
                this.states = new HashSet<MorphemeState>(states.Length);
                this.states.AddRange(states);
            }


            public override bool Accept(SearchPath path)
            {
                MorphemeState previousState = path.GetPreviousState();
                return previousState != null && states.Contains(previousState);
            }
        }
    }
}
