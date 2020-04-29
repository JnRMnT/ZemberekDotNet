using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Native.Collections;

namespace ZemberekDotNet.LangID
{
    public class Language
    {
        public static readonly Language AR = new Language("Arabic", "AR");
        public static readonly Language AZ = new Language("Azeri", "AZ");
        public static readonly Language BA = new Language("Bashkir", "BA");
        public static readonly Language BE = new Language("Belarusian", "BE");
        public static readonly Language BG = new Language("Bulgarian", "BG");
        public static readonly Language BS = new Language("Bosnian", "BS");
        public static readonly Language CA = new Language("Catalan", "CA");
        public static readonly Language CE = new Language("Chechen", "CE");
        public static readonly Language CK = new Language("Kurdish-Sorani", "CK");
        public static readonly Language CS = new Language("Czech", "CS");
        public static readonly Language CV = new Language("Chuvash", "CV");
        public static readonly Language DA = new Language("Danish", "DA");
        public static readonly Language DE = new Language("German", "DE");
        public static readonly Language EL = new Language("Greek", "EL");
        public static readonly Language EN = new Language("English", "EN");
        public static readonly Language EO = new Language("Esperanto", "EO");
        public static readonly Language ES = new Language("Spanish", "ES");
        public static readonly Language ET = new Language("Estonian", "ET");
        public static readonly Language EU = new Language("Basque", "EU");
        public static readonly Language FA = new Language("Persian", "FA");
        public static readonly Language FI = new Language("Finnish", "FI");
        public static readonly Language FR = new Language("French", "FR");
        public static readonly Language HE = new Language("Hebrew", "HE");
        public static readonly Language HI = new Language("Hindi", "HI");
        public static readonly Language HR = new Language("Croatian", "HR");
        public static readonly Language HU = new Language("Hungarian", "HU");
        public static readonly Language HY = new Language("Armenian", "HY");
        public static readonly Language ID = new Language("Indonesian", "ID");
        public static readonly Language IS = new Language("Icelandic", "IS");
        public static readonly Language IT = new Language("Italian", "IT");
        public static readonly Language JA = new Language("Japanese", "JA");
        public static readonly Language JV = new Language("Javanese", "JV");
        public static readonly Language KA = new Language("Georgian", "KA");
        public static readonly Language KK = new Language("Kazakh", "KK");
        public static readonly Language KM = new Language("Khmer", "KM");
        public static readonly Language KO = new Language("Korean", "KO");
        public static readonly Language KU = new Language("Kurdish", "KU");
        public static readonly Language KY = new Language("Krgyz", "KY");
        public static readonly Language LA = new Language("Latin", "LA");
        public static readonly Language LT = new Language("Lithuanian", "LT");
        public static readonly Language LV = new Language("Latvian", "LV");
        public static readonly Language ML = new Language("Malayalam", "ML");
        public static readonly Language MN = new Language("Mongolian", "MN");
        public static readonly Language MS = new Language("Malay", "MS");
        public static readonly Language MY = new Language("Burmese", "MY");
        public static readonly Language NL = new Language("Dutch", "NL");
        public static readonly Language NO = new Language("Norwegian", "NO");
        public static readonly Language PL = new Language("Polish", "PL");
        public static readonly Language PT = new Language("Portuguese", "PT");
        public static readonly Language RO = new Language("Romanian", "RO");
        public static readonly Language RU = new Language("Russian", "RU");
        public static readonly Language SK = new Language("Slovak", "SK");
        public static readonly Language SL = new Language("Slovene", "SL");
        public static readonly Language SR = new Language("Serbian", "SR");
        public static readonly Language SV = new Language("Swedish", "SV");
        public static readonly Language TR = new Language("Turkish", "TR");
        public static readonly Language UK = new Language("Ukranian", "UK");
        public static readonly Language UZ = new Language("Uzbek", "UZ");
        public static readonly Language VI = new Language("Vietnamese", "VI");
        public static readonly Language WAR = new Language("Waray", "WAR");
        public static readonly Language ZH = new Language("Chinese", "ZH");

        public string id;
        public string name;

        Language(string name, string id)
        {
            this.name = name;
            this.id = id.ToLowerInvariant();
        }

        public static Language GetByName(string input)
        {
            foreach (Language language in Language.Values)
            {
                if (language.id.Equals(input, StringComparison.InvariantCultureIgnoreCase))
                {
                    return language;
                }
            }
            throw new ArgumentException("Cannot find language with name:" + input);
        }

        public static string[] AllLanguages()
        {
            string[] ids = new string[Language.Values.Count()];
            int i = 0;
            foreach (Language l in Language.Values)
            {
                ids[i++] = l.id;
            }
            return ids;
        }

        public static ISet<string> LanguageIdSet()
        {
            return new LinkedHashSet<string>(AllLanguages());
        }
        public override string ToString() => name;

        public static IEnumerable<Language> Values
        {
            get
            {
                yield return AR;
                yield return AZ;
                yield return BA;
                yield return BE;
                yield return BG;
                yield return BS;
                yield return CA;
                yield return CE;
                yield return CK;
                yield return CS;
                yield return CV;
                yield return DA;
                yield return DE;
                yield return EL;
                yield return EN;
                yield return EO;
                yield return ES;
                yield return ET;
                yield return EU;
                yield return FA;
                yield return FI;
                yield return FR;
                yield return HE;
                yield return HI;
                yield return HR;
                yield return HU;
                yield return HY;
                yield return ID;
                yield return IS;
                yield return IT;
                yield return JA;
                yield return JV;
                yield return KA;
                yield return KK;
                yield return KM;
                yield return KO;
                yield return KU;
                yield return KY;
                yield return LA;
                yield return LT;
                yield return LV;
                yield return ML;
                yield return MN;
                yield return MS;
                yield return MY;
                yield return NL;
                yield return NO;
                yield return PL;
                yield return PT;
                yield return RO;
                yield return RU;
                yield return SK;
                yield return SL;
                yield return SR;
                yield return SV;
                yield return TR;
                yield return UK;
                yield return UZ;
                yield return VI;
                yield return WAR;
                yield return ZH;
            }
        }
    }
}