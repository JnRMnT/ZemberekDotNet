using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Enums;

namespace ZemberekDotNet.Core.Turkish
{
    public class PrimaryPos : IStringEnum
    {
        public static readonly PrimaryPos Noun = new PrimaryPos("Noun");
        public static readonly PrimaryPos Adjective = new PrimaryPos("Adj");
        public static readonly PrimaryPos Adverb = new PrimaryPos("Adv");
        public static readonly PrimaryPos Conjunction = new PrimaryPos("Conj");
        public static readonly PrimaryPos Interjection = new PrimaryPos("Interj");
        public static readonly PrimaryPos Verb = new PrimaryPos("Verb");
        public static readonly PrimaryPos Pronoun = new PrimaryPos("Pron");
        public static readonly PrimaryPos Numeral = new PrimaryPos("Num");
        public static readonly PrimaryPos Determiner = new PrimaryPos("Det");
        public static readonly PrimaryPos PostPositive = new PrimaryPos("Postp");
        public static readonly PrimaryPos Question = new PrimaryPos("Ques");
        public static readonly PrimaryPos Duplicator = new PrimaryPos("Dup");
        public static readonly PrimaryPos Punctuation = new PrimaryPos("Punc");
        public static readonly PrimaryPos Unknown = new PrimaryPos("Unk");

        private readonly static StringEnumMap<PrimaryPos> shortFormToPosMap = StringEnumMap<PrimaryPos>.Get();
        public string shortForm;

        PrimaryPos(string shortForm)
        {
            this.shortForm = shortForm;
        }

        public static StringEnumMap<PrimaryPos> Converter()
        {
            return shortFormToPosMap;
        }

        public static bool Exists(String stringForm)
        {
            return shortFormToPosMap.EnumExists(stringForm);
        }

        public override string ToString() => shortForm;
        public string GetStringForm()
        {
            return shortForm;
        }
        public static IEnumerable<PrimaryPos> Values
        {
            get
            {
                yield return Noun;
                yield return Adjective;
                yield return Adverb;
                yield return Conjunction;
                yield return Interjection;
                yield return Verb;
                yield return Pronoun;
                yield return Numeral;
                yield return Determiner;
                yield return PostPositive;
                yield return Question;
                yield return Duplicator;
                yield return Punctuation;
                yield return Unknown;
            }
        }
    }
}
