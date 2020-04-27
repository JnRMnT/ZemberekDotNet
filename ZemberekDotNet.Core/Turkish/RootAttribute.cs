using System.Collections.Generic;
using ZemberekDotNet.Core.Enums;

namespace ZemberekDotNet.Core.Turkish
{
    /// <summary>
    ///  These represents attributes of roots.
    /// </summary>
    public class RootAttribute : IStringEnum
    {
        // Generally Present tense (Aorist) suffix has the form [Ir]; such as gel-ir, bul-ur, kapat-ır.
        // But for most verbs with single syllable and compound verbs it forms as [Ar].
        // Such as yap-ar, yet-er, hapsed-er. There are exceptions for this case, such as "var-ır".
        // Below two represents the attributes for clearing the ambiguity. These attributes does not
        // modify the root form.
        public static readonly RootAttribute Aorist_I = new RootAttribute(0, "Aorist_I");
        public static readonly RootAttribute Aorist_A = new RootAttribute(1, "Aorist_A");

        // If a verb ends with a vowel and Progressive suffix [Iyor] appended, last vowel of the root
        // form drops. Such as "ara → ar-ıyor" "ye → y-iyor".
        // This also applies to suffixes, such as Negative "mA" suffix. "yap-ma → yap-m-ıyor".
        // But suffix case is handled during graph generation.
        //
        // This attribute is added automatically.
        // TODO: This may be combined with LastVowelDrop or changed as LastLetterDrop.
        public static readonly RootAttribute ProgressiveVowelDrop = new RootAttribute(2, "ProgressiveVowelDrop");

        // For verbs that ends with a vowel or letter "l" Passive voice suffix fors as [+In] and [+InIl].
        // ara-n, ara-nıl and "sarıl-ın-an".
        // For other verbs [+nIl] is used. Such as ser-il, yap-ıl, otur-ul.
        //
        // This attribute is added automatically.
        // TODO: [+nIl] may be changed to [+Il]
        public static readonly RootAttribute Passive_In = new RootAttribute(3, "Passive_In");

        // For verbs that has more than one syllable and end with a vowel or letters "l" or "r",
        // Causative suffix form as [t]. Such as: ara-t, oku-t, getir-t, doğrul-t, bağır-t
        // Otherwise it forms as [tIr]. Such as: ye-dir, sat-tır, dol-dur, seyret-tir
        //
        // This attribute is added automatically.
        public static readonly RootAttribute Causative_t = new RootAttribute(4, "Causative_t");

        // If last letter of a word or suffix is a stop consonant (tr: süreksiz sert sessiz), and a
        // suffix that starts with a vowel is appended to that word, last letter changes.
        // This is called voicing. Changes are p-b, ç-c, k-ğ, t-d, g-ğ.
        // Such as kitap → kitab-a, pabuç → pabuc-u, cocuk → cocuğ-a, hasat → hasad-ı
        //
        // It also applies to some verbs: et→ed-ecek. But for verb roots, only ‘t’ endings are voiced.
        // And most suffixes: elma-cık→elma-cığ-ı, yap-acak→yap-acağ-ım.
        //
        // When a word ends with ‘nk‘, then ‘k’ changes to ‘g’ instead of ‘ğ’.
        // Such as cenk → ceng-e, çelenk → çeleng-i
        //
        // For some loan words, g-ğ change occurs. psikolog → psikoloğ-a
        //
        // Usually if the word has only one syllable, rule does not apply.
        // Such as turp → turp-u, kat → kat-a, kek → kek-e, küp → küp-üm.
        // But this rule has some exceptions as well: harp → harb-e
        //
        // Some multi syllable words also do not obey this rule.
        // Such as taksirat → taksirat-ı, kapat → kapat-ın
        public static readonly RootAttribute Voicing = new RootAttribute(5, "Voicing");

        // NoVoicing attribute is only used for explicitly marking a word in the dictionary
        // that should not have automatic Voicing attribute. So after a DictionaryItem is created
        // only checking Voicing attribute is enough.
        public static readonly RootAttribute NoVoicing = new RootAttribute(6, "NoVoicing");

        // For some loan words, suffix vowel harmony rules does not apply. This usually happens in some
        // loan words. Such as saat-ler and alkol-ü
        public static readonly RootAttribute InverseHarmony = new RootAttribute(7, "InverseHarmony");

        // When a suffix that starts with a vowel is added to some words, last letter is doubled.
        // Such as hat → hat-tı
        //
        // If last letter is also changed by the appended suffix, transformed letter is repeated.
        // Such as ret → red-di
        public static readonly RootAttribute Doubling = new RootAttribute(8, "Doubling");

        // Last vowel before the last consonant drops in some words when a suffix starting with a vowel
        // is appended.
        // ağız → ağz-a, burun → burn-um, zehir → zehr-e.
        //
        // Some words have this property optionally. Both omuz → omuz-a, omz-a are valid. Sometimes
        // different meaning of the words effect the outcome such as oğul-u and oğl-u. In first case
        // "oğul" means "group of bees", second means "son".
        //
        // Some verbs obeys this rule. kavur → kavr-ul. But it only happens for passive suffix.
        // It does not apply to other suffixes. Such as kavur→kavur-acak, not kavur-kavracak
        //
        // In spoken conversation, some vowels are dropped too but those are grammatically incorrect.
        // Such as içeri → içeri-de (not ‘içerde’), dışarı → dışarı-da (not ‘dışarda’)
        //
        // When a vowel is dropped, the form of the suffix to be appended is determined by the original
        // form of the word, not the form after vowel is dropped.
        // Such as nakit → nakd-e, lütuf → lütf-un.
        //
        // If we were to apply the vowel harmony rule after the vowel is dropped,
        // it would be nakit → nakd-a and lütuf → lütf-ün, which are not correct.
        public static readonly RootAttribute LastVowelDrop = new RootAttribute(9, "LastVowelDrop");

        // This is for marking compound words that ends with third person possesive  suffix P3sg [+sI].
        // Such as aşevi, balkabağı, zeytinyağı.
        //
        // These compound words already contains a suffix so their handling is different than other
        // words. For example some suffixes changes the for of the root.
        // Such as zeytinyağı → zeytinyağ-lar-ı atkuyruğu → atkuyruklu
        public static readonly RootAttribute CompoundP3sg = new RootAttribute(10, "CompoundP3sg");

        // No suffix can be appended to this.
        // TODO: this is not yet used. But some words are marked in dictionary.
        public static readonly RootAttribute NoSuffix = new RootAttribute(11, "NoSuffix");

        // Some Compound words adds `n` instead of `y` when used with some suffixes. Such as `Boğaziçi-ne` not `Boğaziçi-ye`
        // TODO: this is not yet used. But some words are marked in dictionary.
        public static readonly RootAttribute NounConsInsert_n = new RootAttribute(12, "NounConsInsert_n");

        // This attribute is used for formatting a word. If this is used, when a suffix is added to a Proper noun, no single
        // quote is used as a separator. Such as "Türkçenin" not "Türkçe'nin"
        public static readonly RootAttribute NoQuote = new RootAttribute(13, "NoQuote");

        // Some compound nouns cannot be used in root form. For example zeytinyağı -> zeytinyağ. For preventing
        // false positives this attribute is added to the zeytinyağ form of the word. So that representing state cannot
        // be terminal.
        // This is added automatically.
        public static readonly RootAttribute CompoundP3sgRoot = new RootAttribute(14, "CompoundP3sgRoot");

        // This is for marking reflexive verbs. Reflexive suffix [+In] can only be added to some verbs.
        // TODO: This is defined but not used in morphotactics.
        public static readonly RootAttribute Reflexive = new RootAttribute(15, "Reflexive");

        // This is for marking reflexive verbs. Reciprocal suffix [+Iş, +yIş] can only be added to some
        // verbs.
        // TODO: Reciprocal suffix is commented out in morphotactics and reciprocal verbs are added with suffixes.
        // Such as boğuşmak [A:Reciprocal]
        public static readonly RootAttribute Reciprocal = new RootAttribute(16, "Reciprocal");
        // if a verb cannot be reciprocal.
        public static readonly RootAttribute NonReciprocal = new RootAttribute(17, "NonReciprocal");

        // for items that are not in official TDK dictionary
        public static readonly RootAttribute Ext = new RootAttribute(18, "Ext");

        // for items that are added to system during runtime
        public static readonly RootAttribute Runtime = new RootAttribute(19, "Runtime");

        //For dummy items. Those are created when processing compound items.
        public static readonly RootAttribute Dummy = new RootAttribute(20, "Dummy");

        // -------------- Experimental attributes.
        public static readonly RootAttribute ImplicitDative = new RootAttribute(21, "ImplicitDative");

        // It contains plural meaning implicitly so adding an external plural suffix is erroneous.
        // This usually applies to arabic loan words. Such as ulema, hayvanat et.
        public static readonly RootAttribute ImplicitPlural = new RootAttribute(22, "ImplicitPlural");
        public static readonly RootAttribute ImplicitP1sg = new RootAttribute(23, "ImplicitP1sg");
        public static readonly RootAttribute ImplicitP2sg = new RootAttribute(24, "ImplicitP2sg");
        public static readonly RootAttribute FamilyMember = new RootAttribute(25, "FamilyMember"); // annemler etc.
        public static readonly RootAttribute PronunciationGuessed = new RootAttribute(26, "PronunciationGuessed");

        // This means word is only used in informal language.
        // Some applications may want to analyze them with a given informal dictionary.
        // Examples: kanka, beyfendi, mütahit, antreman, bilimum, gaste, aliminyum, tırt, tweet
        public static readonly RootAttribute Informal = new RootAttribute(27, "Informal");

        // This is used for words that requires English rules when applying lowercasing and uppercasing.
        // This way, words like "UNICEF" will be lowercased as "unicef", not "unıcef"
        public static readonly RootAttribute LocaleEn = new RootAttribute(28, "LocaleEn");

        // This is used for temporary DictionaryItems created for words that cannot be analyzed.
        public static readonly RootAttribute Unknown = new RootAttribute(29, "Unknown");

        private static StringEnumMap<RootAttribute> shortFormToPosMap = StringEnumMap<RootAttribute>.Get();
        int index;
        string name;

        RootAttribute(int index, string name)
        {
            this.index = index;
            this.name = name;
        }

        public static StringEnumMap<RootAttribute> Converter()
        {
            return shortFormToPosMap;
        }

        public override string ToString() => name;
        public string GetStringForm()
        {
            return this.name;
        }

        public static IEnumerable<RootAttribute> Values
        {
            get
            {
                yield return Aorist_I;
                yield return Aorist_A;
                yield return ProgressiveVowelDrop;
                yield return Passive_In;
                yield return Causative_t;
                yield return Voicing;
                yield return NoVoicing;
                yield return InverseHarmony;
                yield return Doubling;
                yield return LastVowelDrop;
                yield return CompoundP3sg;
                yield return NoSuffix;
                yield return NounConsInsert_n;
                yield return NoQuote;
                yield return CompoundP3sgRoot;
                yield return Reflexive;
                yield return Reciprocal;
                yield return NonReciprocal;
                yield return Ext;
                yield return Runtime;
                yield return Dummy;
                yield return ImplicitDative;
                yield return ImplicitPlural;
                yield return ImplicitP1sg;
                yield return ImplicitP2sg;
                yield return FamilyMember;
                yield return PronunciationGuessed;
                yield return Informal;
                yield return LocaleEn;
                yield return Unknown;
            }
        }
    }
}
