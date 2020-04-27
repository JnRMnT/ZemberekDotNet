using System.Collections.Generic;
using ZemberekDotNet.Core.Enums;

namespace ZemberekDotNet.Core.Turkish
{
    public class SecondaryPos : IStringEnum
    {
        public static readonly SecondaryPos UnknownSec = new SecondaryPos("Unk");
        public static readonly SecondaryPos DemonstrativePron = new SecondaryPos("Demons");
        public static readonly SecondaryPos Time = new SecondaryPos("Time");
        public static readonly SecondaryPos QuantitivePron = new SecondaryPos("Quant");
        public static readonly SecondaryPos QuestionPron = new SecondaryPos("Ques");
        public static readonly SecondaryPos ProperNoun = new SecondaryPos("Prop");
        public static readonly SecondaryPos PersonalPron = new SecondaryPos("Pers");
        public static readonly SecondaryPos ReflexivePron = new SecondaryPos("Reflex");
        public static readonly SecondaryPos None = new SecondaryPos("None");
        public static readonly SecondaryPos Ordinal = new SecondaryPos("Ord");
        public static readonly SecondaryPos Cardinal = new SecondaryPos("Card");
        public static readonly SecondaryPos Percentage = new SecondaryPos("Percent");
        public static readonly SecondaryPos Ratio = new SecondaryPos("Ratio");
        public static readonly SecondaryPos Range = new SecondaryPos("Range");
        public static readonly SecondaryPos Real = new SecondaryPos("Real");
        public static readonly SecondaryPos Distribution = new SecondaryPos("Dist");
        public static readonly SecondaryPos Clock = new SecondaryPos("Clock");
        public static readonly SecondaryPos Date = new SecondaryPos("Date");
        public static readonly SecondaryPos Email = new SecondaryPos("Email");
        public static readonly SecondaryPos Url = new SecondaryPos("Url");
        public static readonly SecondaryPos Mention = new SecondaryPos("Mention");
        public static readonly SecondaryPos HashTag = new SecondaryPos("HashTag");
        public static readonly SecondaryPos Emoticon = new SecondaryPos("Emoticon");
        public static readonly SecondaryPos RomanNumeral = new SecondaryPos("RomanNumeral");
        public static readonly SecondaryPos RegularAbbreviation = new SecondaryPos("RegAbbrv");
        public static readonly SecondaryPos Abbreviation = new SecondaryPos("Abbrv");

        // Below POS information is for Oflazer compatibility.
        // They indicate that words before Post positive words should end with certain suffixes.
        public static readonly SecondaryPos PCDat = new SecondaryPos("PCDat");
        public static readonly SecondaryPos PCAcc = new SecondaryPos("PCAcc");
        public static readonly SecondaryPos PCIns = new SecondaryPos("PCIns");
        public static readonly SecondaryPos PCNom = new SecondaryPos("PCNom");
        public static readonly SecondaryPos PCGen = new SecondaryPos("PCGen");
        public static readonly SecondaryPos PCAbl = new SecondaryPos("PCAbl");

        private static StringEnumMap<SecondaryPos> shortFormToPosMap = StringEnumMap<SecondaryPos>.Get();
        public string shortForm;

        SecondaryPos(string shortForm)
        {
            this.shortForm = shortForm;
        }

        public static StringEnumMap<SecondaryPos> Converter()
        {
            return shortFormToPosMap;
        }

        public static bool Exists(string stringForm)
        {
            return shortFormToPosMap.EnumExists(stringForm);
        }

        public override string ToString() => shortForm;
        public string GetStringForm()
        {
            return shortForm;
        }

        public static IEnumerable<SecondaryPos> Values
        {
            get
            {
                yield return UnknownSec;
                yield return DemonstrativePron;
                yield return Time;
                yield return QuantitivePron;
                yield return QuestionPron;
                yield return ProperNoun;
                yield return PersonalPron;
                yield return ReflexivePron;
                yield return None;
                yield return Ordinal;
                yield return Cardinal;
                yield return Percentage;
                yield return Ratio;
                yield return Range;
                yield return Real;
                yield return Distribution;
                yield return Clock;
                yield return Date;
                yield return Email;
                yield return Url;
                yield return Mention;
                yield return HashTag;
                yield return Emoticon;
                yield return RomanNumeral;
                yield return RegularAbbreviation;
                yield return Abbreviation;
                yield return PCDat;
                yield return PCAcc;
                yield return PCIns;
                yield return PCNom;
                yield return PCGen;
                yield return PCAbl;
            }
        }
    }
}
