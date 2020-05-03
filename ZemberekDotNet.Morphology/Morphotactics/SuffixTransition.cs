using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using static ZemberekDotNet.Morphology.Analysis.SurfaceTransition;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public class SuffixTransition : MorphemeTransition
    {
        // this string represents the possible surface forms for this transition.
        private readonly string surfaceTemplate;

        private List<SuffixTemplateToken> tokenList;

        private AttributeToSurfaceCache surfaceCache;

        public void AddToSurfaceCache(
            AttributeSet<PhoneticAttribute> attributes, String value)
        {
            surfaceCache.AddSurface(attributes.GetBits(), value);
        }

        public String GetFromSurfaceCache(AttributeSet<PhoneticAttribute> attributes)
        {
            return surfaceCache.GetSurface(attributes.GetBits());
        }

        private SuffixTransition(SuffixTransitionBuilder builder)
        {
            Contract.Requires(builder.from != null);
            Contract.Requires(builder.to != null);
            this.from = builder.from;
            this.to = builder.to;
            this.surfaceTemplate = builder.surfaceTemplate == null ? "" : builder.surfaceTemplate;
            this.Condition = builder.condition;
            ConditionsFromTemplate(this.surfaceTemplate);
            this.tokenList = new List<SuffixTemplateToken>();
            SuffixTemplateTokenizer suffixTemplateTokenizer = new SuffixTemplateTokenizer(this.surfaceTemplate);
            bool hasNext = suffixTemplateTokenizer.MoveNext();
            while(hasNext)
            {
                SuffixTemplateToken token = suffixTemplateTokenizer.Current;
                this.tokenList.Add(token);
                hasNext = suffixTemplateTokenizer.MoveNext();
            }
            this.ConditionCount = CountConditions();
            this.surfaceCache = new AttributeToSurfaceCache();
        }

        private int CountConditions()
        {
            if (Condition == null)
            {
                return 0;
            }
            if (Condition is CombinedCondition) {
                return ((CombinedCondition)Condition).Count();
            } else
            {
                return 1;
            }
        }

        private SuffixTransition(String surfaceTemplate)
        {
            this.surfaceTemplate = surfaceTemplate;
        }

        public override MorphemeTransition GetCopy()
        {
            SuffixTransition st = new SuffixTransition(surfaceTemplate);
            st.from = from;
            st.to = to;
            st.Condition = Condition;
            st.tokenList = new List<SuffixTemplateToken>(tokenList);
            st.surfaceCache = this.surfaceCache;
            return st;
        }

        public bool CanPass(SearchPath path)
        {
            return Condition == null || Condition.Accept(path);
        }

        private void Connect()
        {
            from.AddOutgoing(this);
            to.AddIncoming(this);
        }

        // adds vowel-consonant expectation related conditions automatically.
        // TODO: consider moving this to morphotactics somehow.
        private void ConditionsFromTemplate(string template)
        {
            if (template == null || template.Length == 0)
            {
                return;
            }
            string lower = template.ToLower(Turkish.Locale);
            ICondition c = null;
            bool firstCharVowel = TurkishAlphabet.Instance.IsVowel(lower[0]);
            if (lower.StartsWith(">") || !firstCharVowel)
            {
                c = Conditions.NotHave(PhoneticAttribute.ExpectsVowel);
            }
            if ((lower.StartsWith("+") && TurkishAlphabet.Instance.IsVowel(lower[2]))
                || firstCharVowel)
            {
                c = Conditions.NotHave(PhoneticAttribute.ExpectsConsonant);
            }
            if (c != null)
            {
                if (Condition == null)
                {
                    Condition = c;
                }
                else
                {
                    Condition = c.And(Condition);
                }
            }
        }

        public SuffixTransitionBuilder Builder()
        {
            return new SuffixTransitionBuilder();
        }

        public override string ToString()
        {
            return "[" + from.id + "→" + to.id +
                (surfaceTemplate.IsEmpty() ? "" : (":" + surfaceTemplate))
                + "]";
        }

        public class SuffixTransitionBuilder
        {
            internal MorphemeState from;
            internal MorphemeState to;
            internal string surfaceTemplate;
            internal ICondition condition;

            public SuffixTransitionBuilder From(MorphemeState from)
            {
                CheckIfDefined(this.from, "from");
                this.from = from;
                return this;
            }

            private void CheckIfDefined(object o, string name)
            {
                Contract.Requires(
                    o == null,
                    string.Format("[{0} = {1}] is already defined.", name, o));
            }

            public SuffixTransitionBuilder To(MorphemeState to)
            {
                CheckIfDefined(this.to, "to");
                this.to = to;
                return this;
            }

            public SuffixTransitionBuilder SetCondition(ICondition _condition)
            {
                if (condition != null)
                {
                    Log.Warn("Condition was already set.");
                }
                this.condition = _condition;
                return this;
            }

            public SuffixTransitionBuilder Empty()
            {
                return SurfaceTemplate("");
            }

            public SuffixTransitionBuilder SurfaceTemplate(string template)
            {
                CheckIfDefined(this.surfaceTemplate, "surfaceTemplate");
                this.surfaceTemplate = template;
                return this;
            }

            // generates a transition and connects it.
            public SuffixTransition Build()
            {
                SuffixTransition transition = new SuffixTransition(this);
                transition.Connect();
                return transition;
            }

            // generates a transition and connects it.
            public MorphemeState Add()
            {
                SuffixTransition transition = new SuffixTransition(this);
                transition.Connect();
                return transition.from;
            }
        }

        public List<SuffixTemplateToken> GetTokenList()
        {
            return tokenList;
        }

        public bool HasSurfaceForm()
        {
            return tokenList.Count > 0;
        }

        public SuffixTemplateToken GetLastTemplateToken()
        {
            if (tokenList.Count == 0)
            {
                return null;
            }
            else
            {
                return tokenList[tokenList.Count - 1];
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
            SuffixTransition that = (SuffixTransition)o;
            string thisCondition = Condition == null ? "" : Condition.ToString();
            string otherCondition = that.Condition == null ? "" : that.Condition.ToString();
            return Objects.Equals(surfaceTemplate, that.surfaceTemplate)
                && Objects.Equals(from, that.from)
                && Objects.Equals(to, that.to)
                && Objects.Equals(thisCondition, otherCondition);
        }

        public override int GetHashCode()
        {
            String thisCondition = Condition == null ? "" : Condition.ToString();
            return Objects.HashCode(surfaceTemplate, from, to, thisCondition);
        }
    }
}
