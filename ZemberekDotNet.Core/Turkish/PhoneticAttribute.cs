using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Enums;
using ZemberekDotNet.Core.Native;

namespace ZemberekDotNet.Core.Turkish
{
    public class PhoneticAttribute : IStringEnum, IClassEnum
    {
        private int index;
        private readonly string shortForm;
        // Turkish vowels are: [a, e, ı, i, o, ö, u, ü]
        public static readonly PhoneticAttribute LastLetterVowel = new PhoneticAttribute(0, "LLV");

        // Turkish consonants are: [b, c, ç, d, f, g, ğ, h, j, k, l, m, n, p, r, s, ş, t, v, y, z]
        public static readonly PhoneticAttribute LastLetterConsonant = new PhoneticAttribute(1, "LLC");

        // Turkish Frontal vowels are: [e, i, ö, ü]
        public static readonly PhoneticAttribute LastVowelFrontal = new PhoneticAttribute(3, "LVF");

        // Back vowels are: [a, ı, o, u]
        public static readonly PhoneticAttribute LastVowelBack = new PhoneticAttribute(4, "LVB");

        // Rounded vowels are: [o, u, ö, ü]
        public static readonly PhoneticAttribute LastVowelRounded = new PhoneticAttribute(5, "LVR");

        // Unrounded vowels are: [a, e, ı, i]
        public static readonly PhoneticAttribute LastVowelUnrounded = new PhoneticAttribute(6, "LVuR");

        // Turkish voiceless consonants are [ç, f, h, k, p, s, ş, t]
        public static readonly PhoneticAttribute LastLetterVoiceless = new PhoneticAttribute(7, "LLVless");

        // Turkish voiced consonants are [b, c, d, g, ğ, h, j, l, m, n, r, v, y, z]
        public static readonly PhoneticAttribute LastLetterVoiced = new PhoneticAttribute(8, "LLVo");

        // Turkish Voiceless stop consonants are: [ç, k, p, t]. Voiced stop consonants are [b, c, d, g, ğ]
        public static readonly PhoneticAttribute LastLetterVoicelessStop = new PhoneticAttribute(9, "LLVlessStop");

        public static readonly PhoneticAttribute FirstLetterVowel = new PhoneticAttribute(10, "FLV");
        public static readonly PhoneticAttribute FirstLetterConsonant = new PhoneticAttribute(11, "FLC");

        public static readonly PhoneticAttribute HasNoVowel = new PhoneticAttribute(12, "NoVow");

        // ---- experimental -----

        public static readonly PhoneticAttribute ExpectsVowel = new PhoneticAttribute(13, "EV");
        public static readonly PhoneticAttribute ExpectsConsonant = new PhoneticAttribute(14, "EC");
        public static readonly PhoneticAttribute ModifiedPronoun = new PhoneticAttribute(15, "MP"); //ben,sen -> ban, san form.
        public static readonly PhoneticAttribute UnModifiedPronoun = new PhoneticAttribute(16, "UMP"); //ben,sen -> ben, sen form.

        // for verbs that and with a vowel and to connect `iyor` progressive tense suffix.
        public static readonly PhoneticAttribute LastLetterDropped = new PhoneticAttribute(17, "LWD");
        public static readonly PhoneticAttribute CannotTerminate = new PhoneticAttribute(18, "CNT");
        PhoneticAttribute(int index, string shortForm)
        {
            this.index = index;
            this.shortForm = shortForm;
        }
        public override int GetHashCode()
        {
            return index;
        }
        public int GetIndex()
        {
            return index;
        }

        public string GetStringForm()
        {
            return shortForm;
        }

        public static IEnumerable<PhoneticAttribute> Values
        {
            get
            {
                yield return LastLetterVowel;
                yield return LastLetterConsonant;
                yield return LastVowelFrontal;
                yield return LastVowelBack;
                yield return LastVowelRounded;
                yield return LastVowelUnrounded;
                yield return LastLetterVoiceless;
                yield return LastLetterVoiced;
                yield return LastLetterVoicelessStop;
                yield return FirstLetterVowel;
                yield return FirstLetterConsonant;
                yield return HasNoVowel;
                yield return ExpectsVowel;
                yield return ExpectsConsonant;
                yield return ModifiedPronoun;
                yield return UnModifiedPronoun;
                yield return LastLetterDropped;
                yield return CannotTerminate;
            }
        }
        public override string ToString() => shortForm;

        public override bool Equals(object obj)
        {
            if (obj is IClassEnum)
            {
                return ((IClassEnum)obj).GetIndex() == GetIndex();
            }
            else
            {
                return base.Equals(obj);
            }
        }
        public static StringEnumMap<PhoneticAttribute> Converter()
        {
            return shortFormToPosMap;
        }

        private readonly static StringEnumMap<PhoneticAttribute> shortFormToPosMap = StringEnumMap<PhoneticAttribute>.Get();
    }
}
