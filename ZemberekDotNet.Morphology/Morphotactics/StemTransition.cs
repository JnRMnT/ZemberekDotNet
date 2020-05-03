using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public class StemTransition : MorphemeTransition
    {
        public readonly string surface;
        public readonly DictionaryItem item;
        AttributeSet<PhoneticAttribute> phoneticAttributes;

        int cachedHash;

        public StemTransition(
            string surface,
            DictionaryItem item,
            AttributeSet<PhoneticAttribute> phoneticAttributes,
            MorphemeState toState)
        {
            this.surface = surface;
            this.item = item;
            this.phoneticAttributes = phoneticAttributes;
            this.to = toState;
            this.cachedHash = GetHashCode();
        }

        public override MorphemeTransition GetCopy()
        {
            StemTransition t = new StemTransition(surface, item, phoneticAttributes, to);
            t.from = from;
            return t;
        }

        public AttributeSet<PhoneticAttribute> GetPhoneticAttributes()
        {
            return phoneticAttributes;
        }

        public string DebugForm()
        {
            return "[(Dict:" + item.ToString() + "):" + surface +
                " → " + to.ToString() + "]";
        }

        public override string ToString()
        {
            return DebugForm();
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
            StemTransition that = (StemTransition)o;
            return Objects.Equals(surface, that.surface) &&
                Objects.Equals(item, that.item) &&
                Objects.Equals(phoneticAttributes, that.phoneticAttributes);
        }

        public override int GetHashCode()
        {
            if (cachedHash != 0)
            {
                return cachedHash;
            }
            return Objects.HashCode(surface, item, phoneticAttributes);
        }
    }
}
