using System;

namespace ZemberekDotNet.Core.Turkish
{
    /// <summary>
    /// This is a Letter which contains Turkic language specific attributes, such as vowel type,
    /// englishEquivalent characters.
    /// </summary>
    public class TurkicLetter
    {
        public static readonly TurkicLetter Undefined = new TurkicLetter((char)0);
        public readonly char charValue;
        public readonly bool vowel;
        public readonly bool frontal;
        public readonly bool rounded;
        public readonly bool voiceless;
        public readonly bool continuant;

        private TurkicLetter(Builder builder)
        {
            this.charValue = builder._charValue;
            this.vowel = builder._vowel;
            this.frontal = builder._frontalVowel;
            this.rounded = builder._roundedVowel;
            this.voiceless = builder._voiceless;
            this.continuant = builder._continuant;
        }

        public TurkicLetter(
            char charValue,
            bool vowel,
            bool frontal,
            bool rounded,
            bool voiceless,
            bool continuant)
        {
            this.charValue = charValue;
            this.vowel = vowel;
            this.frontal = frontal;
            this.rounded = rounded;
            this.voiceless = voiceless;
            this.continuant = continuant;
        }

        // only used for illegal letter.
        private TurkicLetter(char c)
        {
            this.charValue = c;
            vowel = false;
            frontal = false;
            rounded = false;
            voiceless = false;
            continuant = false;
        }

        public char CharValue()
        {
            return charValue;
        }

        public bool IsVowel()
        {
            return vowel;
        }

        public bool IsConsonant()
        {
            return !vowel;
        }

        public bool IsFrontal()
        {
            return frontal;
        }

        public bool IsRounded()
        {
            return rounded;
        }

        public bool IsVoiceless()
        {
            return voiceless;
        }

        public bool İsStopConsonant()
        {
            return voiceless && !continuant;
        }

        public override string ToString()
        {
            return charValue.ToString();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            TurkicLetter that = (TurkicLetter)obj;

            if (charValue != that.charValue)
            {
                return false;
            }

            return true;
        }

        public TurkicLetter CopyFor(char c)
        {
            return new TurkicLetter(c, vowel, frontal, rounded, voiceless, continuant);
        }

        public override int GetHashCode()
        {
            return (int)charValue;
        }

        public class Builder
        {
            internal char _charValue;
            internal bool _vowel = false;
            internal bool _frontalVowel = false;
            internal bool _roundedVowel = false;
            internal bool _voiceless = false;
            internal bool _continuant = false;

            public Builder(char charValue)
            {
                this._charValue = charValue;
            }

            public Builder Vowel()
            {
                this._vowel = true;
                return this;
            }

            public Builder FrontalVowel()
            {
                this._frontalVowel = true;
                return this;
            }

            public Builder RoundedVowel()
            {
                this._roundedVowel = true;
                return this;
            }

            public Builder Voiceless()
            {
                this._voiceless = true;
                return this;
            }

            public Builder Continuant()
            {
                this._continuant = true;
                return this;
            }

            public TurkicLetter Build()
            {
                if (((_voiceless || _continuant) && _vowel) || (!_vowel && (_frontalVowel
                    || _roundedVowel)))
                {
                    throw new ArgumentException("Letter seems to have both vowel and Consonant attributes");
                }
                return new TurkicLetter(this);
            }
        }
    }
}
