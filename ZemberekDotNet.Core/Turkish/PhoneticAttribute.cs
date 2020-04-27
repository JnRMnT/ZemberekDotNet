using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Enums;

namespace ZemberekDotNet.Core.Turkish
{
    public class PhoneticAttribute : IStringEnum
    {
        private readonly string shortForm;
        // Turkish vowels are: [a, e, ı, i, o, ö, u, ü]
        public static readonly PhoneticAttribute LastLetterVowel = new PhoneticAttribute("LLV");

        // Turkish consonants are: [b, c, ç, d, f, g, ğ, h, j, k, l, m, n, p, r, s, ş, t, v, y, z]
        public static readonly PhoneticAttribute LastLetterConsonant = new PhoneticAttribute("LLC");

        // Turkish Frontal vowels are: [e, i, ö, ü]
        public static readonly PhoneticAttribute LastVowelFrontal = new PhoneticAttribute("LVF");

        // Back vowels are: [a, ı, o, u]
        public static readonly PhoneticAttribute LastVowelBack = new PhoneticAttribute("LVB");

        // Rounded vowels are: [o, u, ö, ü]
        public static readonly PhoneticAttribute LastVowelRounded = new PhoneticAttribute("LVR");

        // Unrounded vowels are: [a, e, ı, i]
        public static readonly PhoneticAttribute LastVowelUnrounded = new PhoneticAttribute("LVuR");

        // Turkish voiceless consonants are [ç, f, h, k, p, s, ş, t]
        public static readonly PhoneticAttribute LastLetterVoiceless = new PhoneticAttribute("LLVless");

        // Turkish voiced consonants are [b, c, d, g, ğ, h, j, l, m, n, r, v, y, z]
        public static readonly PhoneticAttribute LastLetterVoiced = new PhoneticAttribute("LLVo");

        // Turkish Voiceless stop consonants are: [ç, k, p, t]. Voiced stop consonants are [b, c, d, g, ğ]
        public static readonly PhoneticAttribute LastLetterVoicelessStop = new PhoneticAttribute("LLVlessStop");

        public static readonly PhoneticAttribute FirstLetterVowel = new PhoneticAttribute("FLV");
        public static readonly PhoneticAttribute FirstLetterConsonant = new PhoneticAttribute("FLC");

        public static readonly PhoneticAttribute HasNoVowel = new PhoneticAttribute("NoVow");

        // ---- experimental -----

        public static readonly PhoneticAttribute ExpectsVowel = new PhoneticAttribute("EV");
        public static readonly PhoneticAttribute ExpectsConsonant = new PhoneticAttribute("EC");
        public static readonly PhoneticAttribute ModifiedPronoun = new PhoneticAttribute("MP"); //ben,sen -> ban, san form.
        public static readonly PhoneticAttribute UnModifiedPronoun = new PhoneticAttribute("UMP"); //ben,sen -> ben, sen form.

        // for verbs that and with a vowel and to connect `iyor` progressive tense suffix.
        public static readonly PhoneticAttribute LastLetterDropped = new PhoneticAttribute("LWD");
        public static readonly PhoneticAttribute CannotTerminate = new PhoneticAttribute("CNT");
        PhoneticAttribute(String shortForm)
        {
            this.shortForm = shortForm;
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

        public static StringEnumMap<PhoneticAttribute> Converter()
        {
            return shortFormToPosMap;
        }

        private readonly static StringEnumMap<PhoneticAttribute> shortFormToPosMap = StringEnumMap<PhoneticAttribute>.Get();
    }
}
