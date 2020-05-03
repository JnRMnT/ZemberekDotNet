using System.Collections.Generic;
using ZemberekDotNet.Core.Enums;
using ZemberekDotNet.Core.Native;

namespace ZemberekDotNet.Core.Turkish
{
    public class SecondaryPos : IStringEnum, IClassEnum
    {
        public struct Constants
        {
            public const string UnknownSec = "UnknownSec";
            public const string DemonstrativePron = "DemonstrativePron";
            public const string Time = "Time";
            public const string QuantitivePron = "QuantitivePron";
            public const string QuestionPron = "QuestionPron";
            public const string ProperNoun = "ProperNoun";
            public const string PersonalPron = "PersonalPron";
            public const string ReflexivePron = "ReflexivePron";
            public const string None = "None";
            public const string Ordinal = "Ordinal";
            public const string Cardinal = "Cardinal";
            public const string Percentage = "Percentage";
            public const string Ratio = "Ratio";
            public const string Range = "Range";
            public const string Real = "Real";
            public const string Distribution = "Distribution";
            public const string Clock = "Clock";
            public const string Date = "Date";
            public const string Email = "Email";
            public const string Url = "Url";
            public const string Mention = "Mention";
            public const string HashTag = "HashTag";
            public const string Emoticon = "Emoticon";
            public const string RomanNumeral = "RomanNumeral";
            public const string RegularAbbreviation = "RegularAbbreviation";
            public const string Abbreviation = "Abbreviation";
            public const string PCDat = "PCDat";
            public const string PCAcc = "PCAcc";
            public const string PCIns = "PCIns";
            public const string PCNom = "PCNom";
            public const string PCGen = "PCGen";
            public const string PCAbl = "PCAbl";
        }
        public static readonly SecondaryPos UnknownSec = new SecondaryPos(0, Constants.UnknownSec, "Unk");
        public static readonly SecondaryPos DemonstrativePron = new SecondaryPos(1, Constants.DemonstrativePron,"Demons");
        public static readonly SecondaryPos Time = new SecondaryPos(2,  Constants.Time, "Time");
        public static readonly SecondaryPos QuantitivePron = new SecondaryPos(3, Constants.QuantitivePron, "Quant");
        public static readonly SecondaryPos QuestionPron = new SecondaryPos(4, Constants.QuestionPron, "Ques");
        public static readonly SecondaryPos ProperNoun = new SecondaryPos(5, Constants.ProperNoun, "Prop");
        public static readonly SecondaryPos PersonalPron = new SecondaryPos(6, Constants.PersonalPron, "Pers");
        public static readonly SecondaryPos ReflexivePron = new SecondaryPos(7, Constants.ReflexivePron, "Reflex");
        public static readonly SecondaryPos None = new SecondaryPos(8, Constants.None, "None");
        public static readonly SecondaryPos Ordinal = new SecondaryPos(9, Constants.Ordinal, "Ord");
        public static readonly SecondaryPos Cardinal = new SecondaryPos(10, Constants.Cardinal, "Card");
        public static readonly SecondaryPos Percentage = new SecondaryPos(11, Constants.Percentage, "Percent");
        public static readonly SecondaryPos Ratio = new SecondaryPos(13, Constants.Ratio, "Ratio");
        public static readonly SecondaryPos Range = new SecondaryPos(14, Constants.Range, "Range");
        public static readonly SecondaryPos Real = new SecondaryPos(15, Constants.Real, "Real");
        public static readonly SecondaryPos Distribution = new SecondaryPos(16, Constants.Distribution, "Dist");
        public static readonly SecondaryPos Clock = new SecondaryPos(17, Constants.Clock, "Clock");
        public static readonly SecondaryPos Date = new SecondaryPos(18, Constants.Date, "Date");
        public static readonly SecondaryPos Email = new SecondaryPos(19, Constants.Email, "Email");
        public static readonly SecondaryPos Url = new SecondaryPos(20, Constants.Url, "Url");
        public static readonly SecondaryPos Mention = new SecondaryPos(21, Constants.Mention, "Mention");
        public static readonly SecondaryPos HashTag = new SecondaryPos(22, Constants.HashTag, "HashTag");
        public static readonly SecondaryPos Emoticon = new SecondaryPos(23, Constants.Emoticon, "Emoticon");
        public static readonly SecondaryPos RomanNumeral = new SecondaryPos(24, Constants.RomanNumeral, "RomanNumeral");
        public static readonly SecondaryPos RegularAbbreviation = new SecondaryPos(25, Constants.RegularAbbreviation, "RegAbbrv");
        public static readonly SecondaryPos Abbreviation = new SecondaryPos(26, Constants.Abbreviation, "Abbrv");

        // Below POS information is for Oflazer compatibility.
        // They indicate that words before Post positive words should end with certain suffixes.
        public static readonly SecondaryPos PCDat = new SecondaryPos(27, Constants.PCDat, "PCDat");
        public static readonly SecondaryPos PCAcc = new SecondaryPos(28, Constants.PCAcc, "PCAcc");
        public static readonly SecondaryPos PCIns = new SecondaryPos(29, Constants.PCIns, "PCIns");
        public static readonly SecondaryPos PCNom = new SecondaryPos(30, Constants.PCNom, "PCNom");
        public static readonly SecondaryPos PCGen = new SecondaryPos(31, Constants.PCGen, "PCGen");
        public static readonly SecondaryPos PCAbl = new SecondaryPos(32, Constants.PCAbl, "PCAbl");

        private static StringEnumMap<SecondaryPos> shortFormToPosMap = StringEnumMap<SecondaryPos>.Get();
        public string shortForm;
        public string LongForm { get; set; }
        private int index;
        SecondaryPos(int index, string longForm, string shortForm)
        {
            this.shortForm = shortForm;
            this.LongForm = longForm;
            this.index = index;
        }
        public override int GetHashCode()
        {
            return index;
        }
        public int GetIndex()
        {
            return index;
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
