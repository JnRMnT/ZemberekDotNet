using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Enums;
using ZemberekDotNet.Core.Native;

namespace ZemberekDotNet.Core.Turkish
{
    public class PrimaryPos : IStringEnum, IClassEnum
    {
        public struct Constants
        {
            public const string Noun = "Noun";
            public const string Adjective = "Adjective";
            public const string Adverb = "Adverb";
            public const string Conjunction = "Conjunction";
            public const string Interjection = "Interjection";
            public const string Verb = "Verb";
            public const string Pronoun = "Pronoun";
            public const string Numeral = "Numeral";
            public const string Determiner = "Determiner";
            public const string PostPositive = "PostPositive";
            public const string Question = "Question";
            public const string Duplicator = "Duplicator";
            public const string Punctuation = "Punctuation";
            public const string Unknown = "Unknown";
        }
        public static readonly PrimaryPos Noun = new PrimaryPos(0, Constants.Noun, "Noun");
        public static readonly PrimaryPos Adjective = new PrimaryPos(1, Constants.Adjective, "Adj");
        public static readonly PrimaryPos Adverb = new PrimaryPos(2, Constants.Adverb, "Adv");
        public static readonly PrimaryPos Conjunction = new PrimaryPos(3, Constants.Conjunction, "Conj");
        public static readonly PrimaryPos Interjection = new PrimaryPos(4, Constants.Interjection, "Interj");
        public static readonly PrimaryPos Verb = new PrimaryPos(5, Constants.Verb, "Verb");
        public static readonly PrimaryPos Pronoun = new PrimaryPos(6, Constants.Pronoun, "Pron");
        public static readonly PrimaryPos Numeral = new PrimaryPos(7, Constants.Numeral, "Num");
        public static readonly PrimaryPos Determiner = new PrimaryPos(8, Constants.Determiner, "Det");
        public static readonly PrimaryPos PostPositive = new PrimaryPos(9, Constants.PostPositive, "Postp");
        public static readonly PrimaryPos Question = new PrimaryPos(10, Constants.Question, "Ques");
        public static readonly PrimaryPos Duplicator = new PrimaryPos(11, Constants.Duplicator, "Dup");
        public static readonly PrimaryPos Punctuation = new PrimaryPos(12, Constants.Punctuation, "Punc");
        public static readonly PrimaryPos Unknown = new PrimaryPos(13, Constants.Unknown, "Unk");

        private readonly static StringEnumMap<PrimaryPos> shortFormToPosMap = StringEnumMap<PrimaryPos>.Get();
        public string shortForm;
        public string LongForm { get; set; }
        private int index;

        PrimaryPos(int index, string longForm, string shortForm)
        {
            this.shortForm = shortForm;
            LongForm = longForm;
            this.index = index;
        }

        public static StringEnumMap<PrimaryPos> Converter()
        {
            return shortFormToPosMap;
        }

        public static bool Exists(string stringForm)
        {
            return shortFormToPosMap.EnumExists(stringForm);
        }

        public override int GetHashCode()
        {
            return index;
        }
        public int GetIndex()
        {
            return index;
        }

        public override string ToString() => shortForm;
        public string GetStringForm()
        {
            return shortForm;
        }

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
