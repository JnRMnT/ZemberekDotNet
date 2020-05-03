using System.Collections.Generic;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// Helper class for calculating morphemic attributes.
    /// </summary>
    public class AttributesHelper
    {
        private static readonly List<PhoneticAttribute> NO_VOWEL_ATTRIBUTES = Arrays
      .AsList(PhoneticAttribute.LastLetterConsonant, PhoneticAttribute.FirstLetterConsonant, PhoneticAttribute.HasNoVowel);

        public static AttributeSet<PhoneticAttribute> GetMorphemicAttributes(char[] seq)
        {
            return GetMorphemicAttributes(seq, AttributeSet<PhoneticAttribute>.EmptySet());
        }

        private static TurkishAlphabet alphabet = TurkishAlphabet.Instance;

        public static AttributeSet<PhoneticAttribute> GetMorphemicAttributes(
            char[] seq,
            AttributeSet<PhoneticAttribute> predecessorAttrs)
        {
            if (seq.Length == 0)
            {
                return predecessorAttrs.Copy();
            }
            AttributeSet<PhoneticAttribute> attrs = new AttributeSet<PhoneticAttribute>();
            TurkicLetter last = null;
            if (alphabet.ContainsVowel(seq))
            {

                last = alphabet.GetLastLetter(seq);
                if (last.IsVowel())
                {
                    attrs.Add(PhoneticAttribute.LastLetterVowel);
                }
                else
                {
                    attrs.Add(PhoneticAttribute.LastLetterConsonant);
                }

                TurkicLetter lastVowel = last.IsVowel() ? last : alphabet.GetLastVowel(seq);

                if (lastVowel.IsFrontal())
                {
                    attrs.Add(PhoneticAttribute.LastVowelFrontal);
                }
                else
                {
                    attrs.Add(PhoneticAttribute.LastVowelBack);
                }
                if (lastVowel.IsRounded())
                {
                    attrs.Add(PhoneticAttribute.LastVowelRounded);
                }
                else
                {
                    attrs.Add(PhoneticAttribute.LastVowelUnrounded);
                }

                if (alphabet.GetFirstLetter(seq).IsVowel())
                {
                    attrs.Add(PhoneticAttribute.FirstLetterVowel);
                }
                else
                {
                    attrs.Add(PhoneticAttribute.FirstLetterConsonant);
                }
            }
            else
            {
                // we transfer vowel attributes from the predecessor attributes.
                attrs.CopyFrom(predecessorAttrs);
                attrs.AddAll(NO_VOWEL_ATTRIBUTES);
                attrs.Remove(PhoneticAttribute.LastLetterVowel);
                attrs.Remove(PhoneticAttribute.ExpectsConsonant);
            }

            last = alphabet.GetLastLetter(seq);

            if (last.IsVoiceless())
            {
                attrs.Add(PhoneticAttribute.LastLetterVoiceless);
                if (last.İsStopConsonant())
                {
                    // kitap
                    attrs.Add(PhoneticAttribute.LastLetterVoicelessStop);
                }
            }
            else
            {
                attrs.Add(PhoneticAttribute.LastLetterVoiced);
            }
            return attrs;
        }

    }
}
