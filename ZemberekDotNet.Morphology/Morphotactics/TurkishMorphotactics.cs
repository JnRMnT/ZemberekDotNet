using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using static ZemberekDotNet.Morphology.Morphotactics.Conditions;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public class TurkishMorphotactics
    {
        private static Dictionary<string, Morpheme> morphemeMap = new Dictionary<string, Morpheme>();

        public static readonly Morpheme root = AddMorpheme(Morpheme.Instance("Root", "Root"));
        public static readonly Morpheme noun = AddMorpheme(Morpheme.Instance("Noun", "Noun", PrimaryPos.Noun));
        public static readonly Morpheme adj = AddMorpheme(
            Morpheme.Instance("Adjective", "Adj", PrimaryPos.Adjective));
        public static readonly Morpheme verb = AddMorpheme(Morpheme.Instance("Verb", "Verb", PrimaryPos.Verb));
        public static readonly Morpheme pron = AddMorpheme(
            Morpheme.Instance("Pronoun", "Pron", PrimaryPos.Pronoun));
        public static readonly Morpheme adv = AddMorpheme(Morpheme.Instance("Adverb", "Adv", PrimaryPos.Adverb));
        public static readonly Morpheme conj = AddMorpheme(
            Morpheme.Instance("Conjunction", "Conj", PrimaryPos.Conjunction));
        public static readonly Morpheme punc = AddMorpheme(
            Morpheme.Instance("Punctuation", "Punc", PrimaryPos.Punctuation));
        public static readonly Morpheme ques = AddMorpheme(
            Morpheme.Instance("Question", "Ques", PrimaryPos.Question));
        public static readonly Morpheme postp = AddMorpheme(Morpheme.Instance("PostPositive", "Postp",
            PrimaryPos.PostPositive));
        public static readonly Morpheme det = AddMorpheme(
            Morpheme.Instance("Determiner", "Det", PrimaryPos.Determiner));
        public static readonly Morpheme num = AddMorpheme(
            Morpheme.Instance("Numeral", "Num", PrimaryPos.Numeral));
        public static readonly Morpheme dup = AddMorpheme(
            Morpheme.Instance("Duplicator", "Dup", PrimaryPos.Duplicator));
        public static readonly Morpheme interj = AddMorpheme(Morpheme.Instance("Interjection", "Interj",
            PrimaryPos.Interjection));

        // Number-Person agreement.

        public static readonly Morpheme a1sg = AddMorpheme(Morpheme.Instance("FirstPersonSingular", "A1sg"));
        public static readonly Morpheme a2sg = AddMorpheme(Morpheme.Instance("SecondPersonSingular", "A2sg"));
        public static readonly Morpheme a3sg = AddMorpheme(Morpheme.Instance("ThirdPersonSingular", "A3sg"));
        public static readonly Morpheme a1pl = AddMorpheme(Morpheme.Instance("FirstPersonPlural", "A1pl"));
        public static readonly Morpheme a2pl = AddMorpheme(Morpheme.Instance("SecondPersonPlural", "A2pl"));
        public static readonly Morpheme a3pl = AddMorpheme(Morpheme.Instance("ThirdPersonPlural", "A3pl"));

        // Possessive

        // No possession suffix. This is not a real Morpheme but adds information to analysis. "elma = apple"
        public static readonly Morpheme pnon = AddMorpheme(Morpheme.Instance("NoPosession", "Pnon"));

        // First person singular possession suffix.  "elma-m = my apple"
        public static readonly Morpheme p1sg = AddMorpheme(
            Morpheme.Instance("FirstPersonSingularPossessive", "P1sg"));

        public static readonly Morpheme p2sg = AddMorpheme(
            Morpheme.Instance("SecondPersonSingularPossessive", "P2sg"));

        // Third person singular possession suffix. "elma-sı = his/her apple"
        public static readonly Morpheme p3sg = AddMorpheme(
            Morpheme.Instance("ThirdPersonSingularPossessive", "P3sg"));

        // First person plural possession suffix.
        public static readonly Morpheme p1pl = AddMorpheme(
            Morpheme.Instance("FirstPersonPluralPossessive", "P1pl"));

        public static readonly Morpheme p2pl = AddMorpheme(
            Morpheme.Instance("SecondPersonPluralPossessive", "P2pl"));

        public static readonly Morpheme p3pl = AddMorpheme(
            Morpheme.Instance("ThirdPersonPluralPossessive", "P3pl"));

        // Case suffixes

        // elma
        public static readonly Morpheme nom = AddMorpheme(Morpheme.Instance("Nominal", "Nom"));
        // elmaya
        public static readonly Morpheme dat = AddMorpheme(Morpheme.Instance("Dative", "Dat"));
        // elmayı
        public static readonly Morpheme acc = AddMorpheme(Morpheme.Instance("Accusative", "Acc"));
        // elmadan
        public static readonly Morpheme abl = AddMorpheme(Morpheme.Instance("Ablative", "Abl"));
        // elmada
        public static readonly Morpheme loc = AddMorpheme(Morpheme.Instance("Locative", "Loc"));
        // elmayla
        public static readonly Morpheme ins = AddMorpheme(Morpheme.Instance("Instrumental", "Ins"));
        // elmanın
        public static readonly Morpheme gen = AddMorpheme(Morpheme.Instance("Genitive", "Gen"));
        // elmaca
        public static readonly Morpheme equ = AddMorpheme(Morpheme.Instance("Equ", "Equ"));

        // Derivation suffixes

        // elmacık (Noun)
        public static readonly Morpheme dim = AddMorpheme(Morpheme.Derivational("Diminutive", "Dim"));
        // elmalık (Noun) TODO: Find better name.
        public static readonly Morpheme ness = AddMorpheme(Morpheme.Derivational("Ness", "Ness"));
        // elmalı (Adj)
        public static readonly Morpheme with = AddMorpheme(Morpheme.Derivational("With", "With"));
        // elmasız (Adj)
        public static readonly Morpheme without = AddMorpheme(Morpheme.Derivational("Without", "Without"));
        // elmasal (Adj)
        public static readonly Morpheme related = AddMorpheme(Morpheme.Derivational("Related", "Related"));
        // tahtamsı (Adj)
        public static readonly Morpheme justLike = AddMorpheme(Morpheme.Derivational("JustLike", "JustLike"));
        // tahtadaki (Adj)
        public static readonly Morpheme rel = AddMorpheme(Morpheme.Derivational("Relation", "Rel"));
        // elmacı (Noun)
        public static readonly Morpheme agt = AddMorpheme(Morpheme.Derivational("Agentive", "Agt"));
        // tahtalaş (Verb)
        public static readonly Morpheme become = AddMorpheme(Morpheme.Derivational("Become", "Become"));
        // tahtalan (Verb)
        public static readonly Morpheme acquire = AddMorpheme(Morpheme.Derivational("Acquire", "Acquire"));

        // yeşilce (Adj->Adv)
        public static readonly Morpheme ly = AddMorpheme(Morpheme.Derivational("Ly", "Ly"));
        // oku-t oku-t-tur (Verb)
        public static readonly Morpheme caus = AddMorpheme(Morpheme.Derivational("Causative", "Caus"));
        // konuş-uş (Verb)
        public static readonly Morpheme recip = AddMorpheme(Morpheme.Derivational("Reciprocal", "Recip"));
        // kaşınmak (Verb) For now Reflexive suffixes are only implicit. Meaning that
        // dictionary contains "kaşınmak" with Reflexive attribute.
        public static readonly Morpheme reflex = AddMorpheme(Morpheme.Derivational("Reflexive", "Reflex"));
        // oku-yabil (Verb)
        public static readonly Morpheme able = AddMorpheme(Morpheme.Derivational("Ability", "Able"));
        // oku-n, oku-nul (Verb)
        public static readonly Morpheme pass = AddMorpheme(Morpheme.Derivational("Passive", "Pass"));
        // okumak (Noun)
        public static readonly Morpheme inf1 = AddMorpheme(Morpheme.Derivational("Infinitive1", "Inf1"));
        // okuma (Noun)
        public static readonly Morpheme inf2 = AddMorpheme(Morpheme.Derivational("Infinitive2", "Inf2"));
        // okuyuş (Noun)
        public static readonly Morpheme inf3 = AddMorpheme(Morpheme.Derivational("Infinitive3", "Inf3"));
        // okumaca (Noun)
        public static readonly Morpheme actOf = AddMorpheme(Morpheme.Derivational("ActOf", "ActOf"));
        // okuduğum kitap (Adj, Noun)
        public static readonly Morpheme pastPart = AddMorpheme(
            Morpheme.Derivational("PastParticiple", "PastPart"));
        // okumuşlarımız (Adj, Noun)
        public static readonly Morpheme narrPart = AddMorpheme(
            Morpheme.Derivational("NarrativeParticiple", "NarrPart"));
        // okuyacağım kitap (Adj, Noun)
        public static readonly Morpheme futPart = AddMorpheme(
            Morpheme.Derivational("FutureParticiple", "FutPart"));
        // okuyan (Adj, Noun)
        public static readonly Morpheme presPart = AddMorpheme(
            Morpheme.Derivational("PresentParticiple", "PresPart"));
        // okurluk (Noun)
        public static readonly Morpheme aorPart = AddMorpheme(
            Morpheme.Derivational("AoristParticiple", "AorPart"));
        // okumazlık - okumamazlık (Noun)
        public static readonly Morpheme notState = AddMorpheme(Morpheme.Derivational("NotState", "NotState"));
        // okuyan (Adj, Noun)
        public static readonly Morpheme feelLike = AddMorpheme(Morpheme.Derivational("FeelLike", "FeelLike"));
        // okuyagel (Verb)
        public static readonly Morpheme everSince = AddMorpheme(
            Morpheme.Derivational("EverSince", "EverSince"));
        // okuyadur, okuyagör (Verb)
        public static readonly Morpheme repeat = AddMorpheme(Morpheme.Derivational("Repeat", "Repeat"));
        // okuyayaz (Verb)
        public static readonly Morpheme almost = AddMorpheme(Morpheme.Derivational("Almost", "Almost"));
        // okuyuver (Verb)
        public static readonly Morpheme hastily = AddMorpheme(Morpheme.Derivational("Hastily", "Hastily"));
        // okuyakal (Verb)
        public static readonly Morpheme stay = AddMorpheme(Morpheme.Derivational("Stay", "Stay"));
        // okuyakoy (Verb)
        public static readonly Morpheme start = AddMorpheme(Morpheme.Derivational("Start", "Start"));
        // okurcasına (Adv,Adj)
        public static readonly Morpheme asIf = AddMorpheme(Morpheme.Derivational("AsIf", "AsIf"));
        // okurken (Adv)
        public static readonly Morpheme while_ = AddMorpheme(Morpheme.Derivational("While", "While"));
        // okuyunca (Adv)
        public static readonly Morpheme when = AddMorpheme(Morpheme.Derivational("When", "When"));
        // okuyalı (Adv)
        public static readonly Morpheme sinceDoingSo = AddMorpheme(
            Morpheme.Derivational("SinceDoingSo", "SinceDoingSo"));
        // okudukça (Adv)
        public static readonly Morpheme asLongAs = AddMorpheme(Morpheme.Derivational("AsLongAs", "AsLongAs"));
        // okuyarak (Adv)
        public static readonly Morpheme byDoingSo = AddMorpheme(
            Morpheme.Derivational("ByDoingSo", "ByDoingSo"));
        // okuyasıya (Adv)
        public static readonly Morpheme adamantly = AddMorpheme(
            Morpheme.Derivational("Adamantly", "Adamantly"));
        // okuyup (Adv)
        public static readonly Morpheme afterDoingSo = AddMorpheme(
            Morpheme.Derivational("AfterDoingSo", "AfterDoingSo"));
        // okumadan, okumaksızın (Adv)
        public static readonly Morpheme withoutHavingDoneSo =
            AddMorpheme(Morpheme.Derivational("WithoutHavingDoneSo", "WithoutHavingDoneSo"));
        //okuyamadan (Adv)
        public static readonly Morpheme withoutBeingAbleToHaveDoneSo =
            AddMorpheme(
                Morpheme.Derivational("WithoutBeingAbleToHaveDoneSo", "WithoutBeingAbleToHaveDoneSo"));

        // Zero derivation
        public static readonly Morpheme zero = AddMorpheme(Morpheme.Derivational("Zero", "Zero"));

        // Verb specific
        public static readonly Morpheme cop = AddMorpheme(Morpheme.Instance("Copula", "Cop"));

        // Negative Verb
        public static readonly Morpheme neg = AddMorpheme(Morpheme.Instance("Negative", "Neg"));
        // Unable (Negative - Ability such as "okuyamıyorum - I cannot read, I am unable to read.")
        public static readonly Morpheme unable = AddMorpheme(Morpheme.Instance("Unable", "Unable"));

        // Tense
        public static readonly Morpheme pres = AddMorpheme(Morpheme.Instance("PresentTense", "Pres"));
        public static readonly Morpheme past = AddMorpheme(Morpheme.Instance("PastTense", "Past"));
        public static readonly Morpheme narr = AddMorpheme(Morpheme.Instance("NarrativeTense", "Narr"));
        public static readonly Morpheme cond = AddMorpheme(Morpheme.Instance("Condition", "Cond"));
        // oku-yor
        public static readonly Morpheme prog1 = AddMorpheme(Morpheme.Instance("Progressive1", "Prog1"));
        // oku-makta
        public static readonly Morpheme prog2 = AddMorpheme(Morpheme.Instance("Progressive2", "Prog2"));
        // oku-r
        public static readonly Morpheme aor = AddMorpheme(Morpheme.Instance("Aorist", "Aor"));
        // oku-yacak
        public static readonly Morpheme fut = AddMorpheme(Morpheme.Instance("Future", "Fut"));

        // gel, gel-sin
        public static readonly Morpheme imp = AddMorpheme(Morpheme.Instance("Imparative", "Imp"));
        // oku-ya
        public static readonly Morpheme opt = AddMorpheme(Morpheme.Instance("Optative", "Opt"));
        // oku-sa
        public static readonly Morpheme desr = AddMorpheme(Morpheme.Instance("Desire", "Desr"));
        // oku-malı
        public static readonly Morpheme neces = AddMorpheme(Morpheme.Instance("Necessity", "Neces"));

        //-------------- States ----------------------------
        // _ST = Terminal state _S = Non Terminal State.
        // A terminal state means that a walk in the graph can end there.

        // root of the graph.
        MorphemeState root_S = MorphemeState.NonTerminal("root_S", root);

        MorphemeState puncRoot_ST = MorphemeState.Builder("puncRoot_ST", punc).Terminal().PosRoot().Build();

        //-------------- Noun States ------------------------

        MorphemeState noun_S = MorphemeState.Builder("noun_S", noun).PosRoot().Build();
        MorphemeState nounCompoundRoot_S = MorphemeState.Builder("nounCompoundRoot_S", noun).PosRoot().Build();
        MorphemeState nounSuRoot_S = MorphemeState.Builder("nounSuRoot_S", noun).PosRoot().Build();
        MorphemeState nounInf1Root_S = MorphemeState.Builder("nounInf1Root_S", noun).PosRoot().Build();
        MorphemeState nounActOfRoot_S = MorphemeState.Builder("nounActOfRoot_S", noun).PosRoot().Build();

        // Number-Person agreement

        MorphemeState a3sg_S = MorphemeState.NonTerminal("a3sg_S", a3sg);
        MorphemeState a3sgSu_S = MorphemeState.NonTerminal("a3sgSu_S", a3sg);
        MorphemeState a3sgCompound_S = MorphemeState.NonTerminal("a3sgCompound_S", a3sg);
        MorphemeState a3sgInf1_S = MorphemeState.NonTerminal("a3sgInf1_S", a3sg);
        MorphemeState a3sgActOf_S = MorphemeState.NonTerminal("a3sgActOf_S", a3sg);
        MorphemeState a3pl_S = MorphemeState.NonTerminal("a3pl_S", a3pl);
        MorphemeState a3plActOf_S = MorphemeState.NonTerminal("a3plActOf_S", a3pl);
        MorphemeState a3plCompound_S = MorphemeState.NonTerminal("a3plCompound_S", a3pl);
        MorphemeState a3plCompound2_S = MorphemeState.NonTerminal("a3plCompound2_S", a3pl);

        // Possessive

        MorphemeState pnon_S = MorphemeState.NonTerminal("pnon_S", pnon);
        MorphemeState pnonCompound_S = MorphemeState.NonTerminal("pnonCompound_S", pnon);
        MorphemeState pnonCompound2_S = MorphemeState.NonTerminal("pnonCompound2_S", pnon);
        MorphemeState pnonInf1_S = MorphemeState.NonTerminal("pnonInf1_S", pnon);
        MorphemeState pnonActOf = MorphemeState.NonTerminal("pnonActOf", pnon);
        MorphemeState p1sg_S = MorphemeState.NonTerminal("p1sg_S", p1sg);
        MorphemeState p2sg_S = MorphemeState.NonTerminal("p2sg_S", p2sg);
        MorphemeState p3sg_S = MorphemeState.NonTerminal("p3sg_S", p3sg);
        MorphemeState p1pl_S = MorphemeState.NonTerminal("p1pl_S", p1pl);
        MorphemeState p2pl_S = MorphemeState.NonTerminal("p2pl_S", p2pl);
        MorphemeState p3pl_S = MorphemeState.NonTerminal("p3pl_S", p3pl);

        // Case

        MorphemeState nom_ST = MorphemeState.Terminal("nom_ST", nom);
        MorphemeState nom_S = MorphemeState.NonTerminal("nom_S", nom);

        MorphemeState dat_ST = MorphemeState.Terminal("dat_ST", dat);
        MorphemeState abl_ST = MorphemeState.Terminal("abl_ST", abl);
        MorphemeState loc_ST = MorphemeState.Terminal("loc_ST", loc);
        MorphemeState ins_ST = MorphemeState.Terminal("ins_ST", ins);
        MorphemeState acc_ST = MorphemeState.Terminal("acc_ST", acc);
        MorphemeState gen_ST = MorphemeState.Terminal("gen_ST", gen);
        MorphemeState equ_ST = MorphemeState.Terminal("equ_ST", equ);

        // Derivation

        MorphemeState dim_S = MorphemeState.NonTerminalDerivative("dim_S", dim);
        MorphemeState ness_S = MorphemeState.NonTerminalDerivative("ness_S", ness);
        MorphemeState agt_S = MorphemeState.NonTerminalDerivative("agt_S", agt);
        MorphemeState related_S = MorphemeState.NonTerminalDerivative("related_S", related);
        MorphemeState rel_S = MorphemeState.NonTerminalDerivative("rel_S", rel);
        MorphemeState relToPron_S = MorphemeState.NonTerminalDerivative("relToPron_S", rel);
        MorphemeState with_S = MorphemeState.NonTerminalDerivative("with_S", with);
        MorphemeState without_S = MorphemeState.NonTerminalDerivative("without_S", without);
        MorphemeState justLike_S = MorphemeState.NonTerminalDerivative("justLike_S", justLike);
        MorphemeState nounZeroDeriv_S = MorphemeState.NonTerminalDerivative("nounZeroDeriv_S", zero);
        MorphemeState become_S = MorphemeState.NonTerminalDerivative("become_S", become);
        MorphemeState acquire_S = MorphemeState.NonTerminalDerivative("acquire_S", acquire);

        //-------------- Conditions ------------------------------

        protected RootLexicon lexicon;

        protected IStemTransitions stemTransitions;

        protected TurkishMorphotactics()
        {
        }

        protected static Morpheme AddMorpheme(Morpheme morpheme)
        {
            morphemeMap.Add(morpheme.Id, morpheme);
            return morpheme;
        }

        public IStemTransitions GetStemTransitions()
        {
            return stemTransitions;
        }

        public RootLexicon GetRootLexicon()
        {
            return lexicon;
        }

        public static Morpheme GetMorpheme(string id)
        {
            return morphemeMap.GetValueOrDefault(id);
        }

        public static List<Morpheme> GetMorphemes(List<string> ids)
        {
            return ids.Select(e => GetMorpheme(e)).ToList();
        }

        public static List<Morpheme> GetAllMorphemes()
        {
            List<Morpheme> morphemes = new List<Morpheme>(morphemeMap.Values);
            morphemes.OrderBy(a => a.Id);
            return morphemes;
        }


        public List<Morpheme> GetMorphemes(params string[] ids)
        {
            List<Morpheme> morphemes = new List<Morpheme>(ids.Length);
            foreach (string id in ids)
            {
                if (id.Length == 0)
                {
                    continue;
                }
                morphemes.Add(GetMorpheme(id));
            }
            return morphemes;
        }

        public TurkishMorphotactics(RootLexicon lexicon)
        {
            this.lexicon = lexicon;
            MakeGraph();
            this.stemTransitions = new StemTransitionsMapBased(lexicon, this);
        }

        protected void MakeGraph()
        {
            MapSpecialItemsToRootStates();
            ConnectNounStates();
            ConnectProperNounsAndAbbreviations();
            ConnectAdjectiveStates();
            ConnectNumeralStates();
            ConnectVerbAfterNounAdjStates();
            ConnectPronounStates();
            ConnectVerbAfterPronoun();
            ConnectVerbs();
            ConnectQuestion();
            ConnectAdverbs();
            ConnectLastVowelDropWords();
            ConnectPostpositives();
            ConnectImek();
            HandlePostProcessingConnections();
        }

        /// <summary>
        /// Turkish Nouns always have Noun-Person-Possession-Case morphemes.  Even there are no suffix
        /// characters. elma -> Noun:elma - A3sg:ε - Pnon:ε - Nom:ε (Third person singular, No possession,
        /// Nominal Case)
        /// </summary>
        public void ConnectNounStates()
        {

            // ev-ε-?-?
            noun_S.AddEmpty(a3sg_S, Conditions.NotHave(RootAttribute.ImplicitPlural));

            // ev-ler-?-?.
            noun_S.Add(a3pl_S, "lAr",
                Conditions.NotHave(RootAttribute.ImplicitPlural)
                    .And(Conditions.NotHave(RootAttribute.CompoundP3sg)));

            // Allow only implicit plural `hayvanat`.
            noun_S.AddEmpty(a3pl_S, Conditions.Has(RootAttribute.ImplicitPlural));

            // --- Compound Handling ---------
            // for compound roots like "zeytinyağ-" generate two transitions
            // NounCompound--(ε)--> a3sgCompound --(ε)--> pNonCompound_S --> Nom_S
            nounCompoundRoot_S.AddEmpty(
                a3sgCompound_S,
                Conditions.Has(RootAttribute.CompoundP3sgRoot));

            a3sgCompound_S.AddEmpty(pnonCompound_S);
            a3sgCompound_S.Add(p3pl_S, "lArI");

            // ---- For compund derivations -----------------
            pnonCompound_S.AddEmpty(nom_S);
            nom_S.Add(become_S, "lAş");
            nom_S.Add(acquire_S, "lAn");
            // for "zeytinyağlı"
            nom_S.Add(with_S, "lI", new Conditions.ContainsMorpheme(with, without).Not());
            // for "zeytinyağsız"
            nom_S.Add(without_S, "sIz", new Conditions.ContainsMorpheme(with, without).Not());
            // for "zeytinyağlık"
            ContainsMorpheme containsNess = new Conditions.ContainsMorpheme(ness);
            nom_S.Add(ness_S, "lI~k", Conditions.Not(containsNess));
            nom_S.Add(ness_S, "lI!ğ", Conditions.Not(containsNess));
            // for "zeytinyağcı"
            nom_S.Add(agt_S, ">cI", Conditions.Not(new Conditions.ContainsMorpheme(agt)));
            // for "zeytinyağsı"
            nom_S.Add(justLike_S, "+msI", Conditions.Not(new Conditions.ContainsMorpheme(justLike)));
            // for "zeytinyağcık"
            nom_S.Add(dim_S, ">cI~k",
                Conditions.HAS_NO_SURFACE.AndNot(new Conditions.ContainsMorpheme(dim)));
            nom_S.Add(dim_S, ">cI!ğ",
                Conditions.HAS_NO_SURFACE.AndNot(new Conditions.ContainsMorpheme(dim)));
            // "zeytinyağcağız"
            nom_S.Add(dim_S, "cAğIz", Conditions.HAS_NO_SURFACE);

            // for compound roots like "zeytinyağ-lar-ı" generate two transition
            // NounCompound--(lAr)--> a3plCompound ---> p3sg_S, P1sg etc.
            nounCompoundRoot_S.Add(
                a3plCompound_S,
                "lAr",
                Conditions.Has(RootAttribute.CompoundP3sgRoot));

            // but for pnon connection, we use lArI
            nounCompoundRoot_S.Add(
                a3plCompound2_S,
                "lArI",
                Conditions.Has(RootAttribute.CompoundP3sgRoot));

            a3plCompound_S
                .Add(p3sg_S, "I")
                .Add(p2sg_S, "In")
                .Add(p1sg_S, "Im")
                .Add(p1pl_S, "ImIz")
                .Add(p2pl_S, "InIz")
                .Add(p3pl_S, "I");

            // this path is used for plural analysis (A3pl+Pnon+Nom) of compound words.
            a3plCompound2_S.AddEmpty(pnonCompound2_S);
            pnonCompound2_S.AddEmpty(nom_ST);

            // ------


            // do not allow possessive suffixes for abbreviations or words like "annemler"
            ICondition rootIsAbbrv = new SecondaryPosIs(SecondaryPos.Abbreviation);

            ICondition possessionCond = Conditions.NotHave(RootAttribute.FamilyMember)
                .AndNot(rootIsAbbrv);

            a3sg_S
                .AddEmpty(pnon_S, Conditions.NotHave(RootAttribute.FamilyMember))        // ev
                .Add(p1sg_S, "Im", possessionCond)       // evim
                .Add(p2sg_S, "In", possessionCond
                    .AndNot(new Conditions.PreviousGroupContainsMorpheme(justLike)))  // evin
                .Add(p3sg_S, "+sI", possessionCond)      // evi, odası
                .AddEmpty(p3sg_S,
                    Conditions.Has(RootAttribute.CompoundP3sg))  // "zeytinyağı" has two analyses. Pnon and P3sg.
                .Add(p1pl_S, "ImIz", possessionCond)     // evimiz
                .Add(p2pl_S, "InIz", possessionCond
                    .AndNot(new Conditions.PreviousGroupContainsMorpheme(justLike)))  // eviniz
                .Add(p3pl_S, "lArI", possessionCond);    // evleri

            // ev-ler-ε-?
            a3pl_S.AddEmpty(pnon_S, Conditions.NotHave(RootAttribute.FamilyMember));

            // ev-ler-im-?
            a3pl_S
                .Add(p1sg_S, "Im", possessionCond)
                .Add(p2sg_S, "In", possessionCond)
                .AddEmpty(p1sg_S, Conditions.Has(RootAttribute.ImplicitP1sg)) // for words like "annemler"
                .AddEmpty(p2sg_S, Conditions.Has(RootAttribute.ImplicitP2sg)) // for words like "annenler"
                .Add(p3sg_S, "I", possessionCond)
                .Add(p1pl_S, "ImIz", possessionCond)
                .Add(p2pl_S, "InIz", possessionCond)
                .Add(p3pl_S, "I", possessionCond);

            // --- handle su - akarsu roots. ----
            nounSuRoot_S.AddEmpty(a3sgSu_S);
            nounSuRoot_S.Add(a3pl_S, "lar");
            a3sgSu_S
                .AddEmpty(pnon_S)
                .Add(p1sg_S, "yum")
                .Add(p2sg_S, "yun")
                .Add(p3sg_S, "yu")
                .Add(p1pl_S, "yumuz")
                .Add(p2pl_S, "yunuz")
                .Add(p3pl_S, "lArI");

            // ev-?-ε-ε (ev, evler).
            pnon_S.AddEmpty(nom_ST, Conditions.NotHave(RootAttribute.FamilyMember));

            ICondition equCond =
                Conditions.PreviousMorphemeIsCondition(a3pl).Or(
                    new Conditions.ContainsMorpheme(adj, futPart, presPart, narrPart, pastPart).Not())
                    .Or(new Conditions.ContainsMorphemeSequence(able, verb,
                        pastPart)); // allow `yapabildiğince`

            // Not allow "zetinyağı-ya" etc.
            pnon_S
                .Add(dat_ST, "+yA", Conditions.NotHave(RootAttribute.CompoundP3sg))   // ev-e
                .Add(abl_ST, ">dAn", Conditions.NotHave(RootAttribute.CompoundP3sg))  // ev-den
                .Add(loc_ST, ">dA", Conditions.NotHave(RootAttribute.CompoundP3sg))   // evde
                .Add(acc_ST, "+yI", Conditions.NotHave(RootAttribute.CompoundP3sg))   // evi
                .Add(gen_ST, "+nIn", Conditions.PreviousStateIsNotCondition(a3sgSu_S))         // evin, zeytinyağının
                .Add(gen_ST, "yIn", Conditions.PreviousStateIsCondition(a3sgSu_S))             // suyun
                .Add(equ_ST, ">cA", Conditions.NotHave(RootAttribute.CompoundP3sg).And(equCond))   // evce
                .Add(ins_ST, "+ylA");                                      // evle, zeytinyağıyla

            pnon_S.Add(dat_ST, "+nA", Conditions.Has(RootAttribute.CompoundP3sg))   // zeytinyağı-na
                .Add(abl_ST, "+ndAn", Conditions.Has(RootAttribute.CompoundP3sg))   // zeytinyağı-ndan
                .Add(loc_ST, "+ndA", Conditions.Has(RootAttribute.CompoundP3sg))    // zeytinyağı-nda
                .Add(equ_ST, "+ncA", Conditions.Has(RootAttribute.CompoundP3sg).And(equCond))    // zeytinyağı-nca
                .Add(acc_ST, "+nI", Conditions.Has(RootAttribute.CompoundP3sg));    // zeytinyağı-nı

            // This transition is for words like "içeri" or "dışarı".
            // Those words implicitly contains Dative suffix.
            // But It is also possible to add dative suffix +yA to those words such as "içeri-ye".
            pnon_S.AddEmpty(dat_ST, Conditions.Has(RootAttribute.ImplicitDative));

            p1sg_S
                .AddEmpty(nom_ST)    // evim
                .Add(dat_ST, "A")    // evime
                .Add(loc_ST, "dA")   // evimde
                .Add(abl_ST, "dAn")  // evimden
                .Add(ins_ST, "lA")   // evimle
                .Add(gen_ST, "In")   // evimin
                .Add(equ_ST, "cA", equCond.Or(new Conditions.ContainsMorpheme(pastPart)))   // evimce
                .Add(acc_ST, "I");   // evimi

            p2sg_S
                .AddEmpty(nom_ST)    // evin
                .Add(dat_ST, "A")    // evine
                .Add(loc_ST, "dA")   // evinde
                .Add(abl_ST, "dAn")  // evinden
                .Add(ins_ST, "lA")   // evinle
                .Add(gen_ST, "In")   // evinin
                .Add(equ_ST, "cA", equCond.Or(new Conditions.ContainsMorpheme(pastPart)))   // evince
                .Add(acc_ST, "I");   // evini

            p3sg_S
                .AddEmpty(nom_ST)    // evi
                .Add(dat_ST, "nA")   // evine
                .Add(loc_ST, "ndA")  // evinde
                .Add(abl_ST, "ndAn") // evinden
                .Add(ins_ST, "ylA")  // eviyle
                .Add(gen_ST, "nIn")  // evinin
                .Add(equ_ST, "ncA", equCond.Or(new Conditions.ContainsMorpheme(pastPart)))// evince
                .Add(acc_ST, "nI");  // evini

            p1pl_S
                .AddEmpty(nom_ST)    // evimiz
                .Add(dat_ST, "A")    // evimize
                .Add(loc_ST, "dA")   // evimizde
                .Add(abl_ST, "dAn")  // evimizden
                .Add(ins_ST, "lA")   // evimizden
                .Add(gen_ST, "In")   // evimizin
                .Add(equ_ST, "cA", equCond.Or(new Conditions.ContainsMorpheme(pastPart)))   // evimizce
                .Add(acc_ST, "I");   // evimizi

            p2pl_S
                .AddEmpty(nom_ST)    // eviniz
                .Add(dat_ST, "A")    // evinize
                .Add(loc_ST, "dA")   // evinizde
                .Add(abl_ST, "dAn")  // evinizden
                .Add(ins_ST, "lA")   // evinizle
                .Add(gen_ST, "In")   // evinizin
                .Add(equ_ST, "cA", equCond.Or(new Conditions.ContainsMorpheme(pastPart)))  // evinizce
                .Add(acc_ST, "I");   // evinizi

            p3pl_S
                .AddEmpty(nom_ST)     // evleri
                .Add(dat_ST, "nA")    // evlerine
                .Add(loc_ST, "ndA")   // evlerinde
                .Add(abl_ST, "ndAn")  // evlerinden
                .Add(ins_ST, "ylA")   // evleriyle
                .Add(gen_ST, "nIn")   // evlerinin
                                      // For now we omit equCond check because adj+..+A3pl+..+Equ fails.
                .Add(equ_ST, "+ncA")   // evlerince.
                .Add(acc_ST, "nI");   // evlerini

            // ev-ε-ε-ε-cik (evcik). Disallow this path if visitor contains any non empty surface suffix.
            // There are two almost identical suffix transitions with templates ">cI~k" and ">cI!ğ"
            // This was necessary for some simplification during analysis. This way there will be only one
            // surface form generated for each transition.
            nom_ST.Add(dim_S, ">cI~k", Conditions.HAS_NO_SURFACE.AndNot(rootIsAbbrv));
            nom_ST.Add(dim_S, ">cI!ğ", Conditions.HAS_NO_SURFACE.AndNot(rootIsAbbrv));

            // ev-ε-ε-ε-ceğiz (evceğiz)
            nom_ST.Add(dim_S, "cAğIz", Conditions.HAS_NO_SURFACE.AndNot(rootIsAbbrv));

            // connect dim to the noun root.
            dim_S.AddEmpty(noun_S);

            ICondition emptyAdjNounSeq = new ContainsMorphemeSequence(adj, zero, noun, a3sg, pnon, nom);

            nom_ST.Add(ness_S, "lI~k",
                Conditions.CURRENT_GROUP_EMPTY
                    .AndNot(containsNess)
                    .AndNot(emptyAdjNounSeq)
                    .AndNot(rootIsAbbrv));
            nom_ST.Add(ness_S, "lI!ğ",
                Conditions.CURRENT_GROUP_EMPTY
                    .AndNot(containsNess)
                    .AndNot(emptyAdjNounSeq)
                    .AndNot(rootIsAbbrv));

            // connect `ness` to the noun root.
            ness_S.AddEmpty(noun_S);

            nom_ST.Add(agt_S, ">cI",
                Conditions.CURRENT_GROUP_EMPTY.AndNot(new Conditions.ContainsMorpheme(adj, agt)));

            // connect `ness` to the noun root.
            agt_S.AddEmpty(noun_S);

            // here we do not allow an adjective to pass here.
            // such as, adj->zero->noun->ε-ε-ε->zero->Verb is not acceptable because there is already a
            // adj->zero->Verb path.
            ICondition noun2VerbZeroDerivationCondition = Conditions.HAS_TAIL
                .AndNot(Conditions.CURRENT_GROUP_EMPTY
                    .And(new Conditions.LastDerivationIs(adjZeroDeriv_S)));

            nom_ST.AddEmpty(nounZeroDeriv_S, noun2VerbZeroDerivationCondition);

            // elma-ya-yım elma-ya-ydı
            dat_ST.AddEmpty(nounZeroDeriv_S, noun2VerbZeroDerivationCondition);

            // elma-dan-ım elma-dan-dı
            abl_ST.AddEmpty(nounZeroDeriv_S, noun2VerbZeroDerivationCondition);

            // elma-da-yım elma-da-ydı
            loc_ST.AddEmpty(nounZeroDeriv_S, noun2VerbZeroDerivationCondition);

            // elma-yla-yım elma-yla-ydı
            ins_ST.AddEmpty(nounZeroDeriv_S, noun2VerbZeroDerivationCondition);

            // elma-nın-ım elma-nın-dı
            gen_ST.AddEmpty(nounZeroDeriv_S, noun2VerbZeroDerivationCondition);

            nounZeroDeriv_S.AddEmpty(nVerb_S);

            // meyve-li
            ICondition noSurfaceAfterDerivation = new NoSurfaceAfterDerivation();
            nom_ST.Add(with_S, "lI",
                noSurfaceAfterDerivation
                    .AndNot(new Conditions.ContainsMorpheme(with, without))
                    .AndNot(rootIsAbbrv));

            nom_ST.Add(without_S, "sIz",
                noSurfaceAfterDerivation
                    .AndNot(new Conditions.ContainsMorpheme(with, without, inf1))
                    .AndNot(rootIsAbbrv));

            nom_ST.Add(justLike_S, "+msI",
                noSurfaceAfterDerivation
                    .AndNot(new Conditions.ContainsMorpheme(justLike, futPart, pastPart, presPart, adj))
                    .AndNot(rootIsAbbrv));

            nom_ST.Add(justLike_S, "ImsI",
                Conditions.NotHave(PhoneticAttribute.LastLetterVowel)
                    .And(noSurfaceAfterDerivation)
                    .AndNot(new Conditions.ContainsMorpheme(justLike, futPart, pastPart, presPart, adj))
                    .AndNot(rootIsAbbrv));

            nom_ST.Add(related_S, "sAl",
                noSurfaceAfterDerivation
                    .AndNot(new Conditions.ContainsMorpheme(with, without, related))
                    .AndNot(rootIsAbbrv));

            // connect With to Adjective root.
            with_S.AddEmpty(adjectiveRoot_ST);
            without_S.AddEmpty(adjectiveRoot_ST);
            related_S.AddEmpty(adjectiveRoot_ST);

            justLike_S.AddEmpty(adjectiveRoot_ST);

            // meyve-de-ki
            ICondition notRelRepetition = new HasTailSequence(rel, adj, zero, noun, a3sg, pnon, loc).Not();
            loc_ST.Add(rel_S, "ki", notRelRepetition);
            rel_S.AddEmpty(adjectiveRoot_ST);

            // for covering dünkü, anki, yarınki etc. Unlike Oflazer, We also allow dündeki etc.
            // TODO: Use a more general grouping, not using Secondary Pos
            ICondition time = Conditions.CURRENT_GROUP_EMPTY.And(
                new SecondaryPosIs(SecondaryPos.Time));
            DictionaryItem dun = lexicon.GetItemById("dün_Noun_Time");
            DictionaryItem gun = lexicon.GetItemById("gün_Noun_Time");
            DictionaryItem bugun = lexicon.GetItemById("bugün_Noun_Time");
            DictionaryItem ileri = lexicon.GetItemById("ileri_Noun");
            DictionaryItem geri = lexicon.GetItemById("geri_Noun");
            DictionaryItem ote = lexicon.GetItemById("öte_Noun");
            DictionaryItem beri = lexicon.GetItemById("beri_Noun");

            ICondition time2 = Conditions.RootIsAny(dun, gun, bugun);
            nom_ST.Add(rel_S, "ki", time.AndNot(time2));
            nom_ST.Add(rel_S, "ki", Conditions.RootIsAny(ileri, geri, ote, beri));
            nom_ST.Add(rel_S, "kü", time2.And(time));

            // After Genitive suffix, Rel suffix makes a Pronoun derivation.
            gen_ST.Add(relToPron_S, "ki");
            relToPron_S.AddEmpty(pronAfterRel_S);

            ContainsMorpheme verbDeriv = new Conditions.ContainsMorpheme(inf1, inf2, inf3, pastPart, futPart);

            nom_ST.Add(become_S, "lAş",
                noSurfaceAfterDerivation.AndNot(new Conditions.ContainsMorpheme(adj))
                    .AndNot(verbDeriv)
                    .AndNot(rootIsAbbrv));
            become_S.AddEmpty(verbRoot_S);

            nom_ST.Add(acquire_S, "lAn",
                noSurfaceAfterDerivation.AndNot(new Conditions.ContainsMorpheme(adj))
                    .AndNot(verbDeriv)
                    .AndNot(rootIsAbbrv));

            acquire_S.AddEmpty(verbRoot_S);

            // Inf1 mak makes noun derivation. However, it cannot get any possessive or plural suffix.
            // Also cannot be followed by Dat, Gen, Acc case suffixes.
            // So we create a path only for it.
            nounInf1Root_S.AddEmpty(a3sgInf1_S);
            a3sgInf1_S.AddEmpty(pnonInf1_S);
            pnonInf1_S.AddEmpty(nom_ST);
            pnonInf1_S.Add(abl_ST, "tAn");
            pnonInf1_S.Add(loc_ST, "tA");
            pnonInf1_S.Add(ins_ST, "lA");

            nounActOfRoot_S.AddEmpty(a3sgActOf_S);
            nounActOfRoot_S.Add(a3plActOf_S, "lar");
            a3sgActOf_S.AddEmpty(pnonActOf);
            a3plActOf_S.AddEmpty(pnonActOf);
            pnonActOf.AddEmpty(nom_ST);

        }

        //-------- Morphotactics for modified forms of words like "içeri->içerde"
        public MorphemeState nounLastVowelDropRoot_S =
            MorphemeState.Builder("nounLastVowelDropRoot_S", noun).PosRoot().Build();
        public MorphemeState adjLastVowelDropRoot_S =
            MorphemeState.Builder("adjLastVowelDropRoot_S", adj).PosRoot().Build();
        public MorphemeState postpLastVowelDropRoot_S =
            MorphemeState.Builder("postpLastVowelDropRoot_S", postp).PosRoot().Build();
        MorphemeState a3PlLastVowelDrop_S = MorphemeState.NonTerminal("a3PlLastVowelDrop_S", a3pl);
        MorphemeState a3sgLastVowelDrop_S = MorphemeState.NonTerminal("a3sgLastVowelDrop_S", a3sg);
        MorphemeState pNonLastVowelDrop_S = MorphemeState.NonTerminal("pNonLastVowelDrop_S", pnon);
        MorphemeState zeroLastVowelDrop_S = MorphemeState.NonTerminalDerivative("zeroLastVowelDrop_S", zero);

        private void ConnectLastVowelDropWords()
        {
            nounLastVowelDropRoot_S.AddEmpty(a3sgLastVowelDrop_S);
            nounLastVowelDropRoot_S.Add(a3PlLastVowelDrop_S, "lAr");
            a3sgLastVowelDrop_S.AddEmpty(pNonLastVowelDrop_S);
            a3PlLastVowelDrop_S.AddEmpty(pNonLastVowelDrop_S);
            pNonLastVowelDrop_S.Add(loc_ST, ">dA");
            pNonLastVowelDrop_S.Add(abl_ST, ">dAn");

            adjLastVowelDropRoot_S.AddEmpty(zeroLastVowelDrop_S);
            postpLastVowelDropRoot_S.AddEmpty(zeroLastVowelDrop_S);
            zeroLastVowelDrop_S.AddEmpty(nounLastVowelDropRoot_S);
        }

        MorphemeState nounProper_S = MorphemeState.Builder("nounProper_S", noun).PosRoot().Build();
        MorphemeState nounAbbrv_S = MorphemeState.Builder("nounAbbrv_S", noun).PosRoot().Build();
        // this will be used for proper noun separation.
        MorphemeState puncProperSeparator_S = MorphemeState.NonTerminal("puncProperSeparator_S", punc);

        MorphemeState nounNoSuffix_S = MorphemeState.Builder("nounNoSuffix_S", noun).PosRoot().Build();
        MorphemeState nounA3sgNoSuffix_S = MorphemeState.NonTerminal("nounA3sgNoSuffix_S", a3sg);
        MorphemeState nounPnonNoSuffix_S = MorphemeState.NonTerminal("nounPnonNoSuffix_S", pnon);
        MorphemeState nounNomNoSuffix_ST = MorphemeState.Terminal("nounNomNoSuffix_S", nom);

        private void ConnectProperNounsAndAbbreviations()
        {
            // ---- Proper noun handling -------
            // TODO: consider adding single quote after an overhaul.
            // nounProper_S.Add(puncProperSeparator_S, "'");
            nounProper_S.AddEmpty(a3sg_S);
            nounProper_S.Add(a3pl_S, "lAr");
            puncProperSeparator_S.AddEmpty(a3sg_S);
            puncProperSeparator_S.Add(a3pl_S, "lAr");

            // ---- Abbreviation Handling -------
            // TODO: consider restricting possessive, most derivation and plural suffixes.
            nounAbbrv_S.AddEmpty(a3sg_S);
            nounAbbrv_S.Add(a3pl_S, "lAr");

            //----- This is for catching words that cannot have a suffix.
            nounNoSuffix_S.AddEmpty(nounA3sgNoSuffix_S);
            nounA3sgNoSuffix_S.AddEmpty(nounPnonNoSuffix_S);
            nounPnonNoSuffix_S.AddEmpty(nounNomNoSuffix_ST);
        }

        //-------------- Adjective States ------------------------

        MorphemeState adjectiveRoot_ST = MorphemeState.Builder("adjectiveRoot_ST", adj).Terminal().PosRoot().Build();
        MorphemeState adjAfterVerb_S = MorphemeState.Builder("adjAfterVerb_S", adj).PosRoot().Build();
        MorphemeState adjAfterVerb_ST = MorphemeState.Builder("adjAfterVerb_ST", adj).Terminal().PosRoot().Build();

        MorphemeState adjZeroDeriv_S = MorphemeState.NonTerminalDerivative("adjZeroDeriv_S", zero);

        // After verb->adj derivations Adj can get possesive suffixes.
        // Such as "oku-duğ-um", "okuyacağı"
        MorphemeState aPnon_ST = MorphemeState.Terminal("aPnon_ST", pnon);
        MorphemeState aP1sg_ST = MorphemeState.Terminal("aP1sg_ST", p1sg);
        MorphemeState aP2sg_ST = MorphemeState.Terminal("aP2sg_ST", p2sg);
        MorphemeState aP3sg_ST = MorphemeState.Terminal("aP3sg_ST", p3sg);
        MorphemeState aP1pl_ST = MorphemeState.Terminal("aP3sg_ST", p1pl);
        MorphemeState aP2pl_ST = MorphemeState.Terminal("aP2pl_ST", p2pl);
        MorphemeState aP3pl_ST = MorphemeState.Terminal("aP3pl_ST", p3pl);

        MorphemeState aLy_S = MorphemeState.NonTerminalDerivative("aLy_S", ly);
        MorphemeState aAsIf_S = MorphemeState.NonTerminalDerivative("aAsIf_S", asIf);
        MorphemeState aAgt_S = MorphemeState.NonTerminalDerivative("aAgt_S", agt);

        private void ConnectAdjectiveStates()
        {

            // zero morpheme derivation. Words like "yeşil-i" requires Adj to Noun conversion.
            // Since noun suffixes are not derivational a "Zero" morpheme is used for this.
            // Transition has a HAS_TAIL condition because Adj->Zero->Noun+A3sg+Pnon+Nom) is not allowed.
            adjectiveRoot_ST.AddEmpty(adjZeroDeriv_S, Conditions.HAS_TAIL);

            adjZeroDeriv_S.AddEmpty(noun_S);

            adjZeroDeriv_S.AddEmpty(nVerb_S);

            adjectiveRoot_ST.Add(aLy_S, ">cA");
            aLy_S.AddEmpty(advRoot_ST);

            adjectiveRoot_ST
                .Add(aAsIf_S, ">cA", new Conditions.ContainsMorpheme(asIf, ly, agt, with, justLike).Not());
            aAsIf_S.AddEmpty(adjectiveRoot_ST);

            adjectiveRoot_ST
                .Add(aAgt_S, ">cI", new Conditions.ContainsMorpheme(asIf, ly, agt, with, justLike).Not());
            aAgt_S.AddEmpty(noun_S);

            adjectiveRoot_ST.Add(justLike_S, "+msI",
                new NoSurfaceAfterDerivation()
                    .And(new Conditions.ContainsMorpheme(justLike).Not()));

            adjectiveRoot_ST.Add(justLike_S, "ImsI",
                Conditions.NotHave(PhoneticAttribute.LastLetterVowel)
                    .And(new NoSurfaceAfterDerivation())
                    .And(new Conditions.ContainsMorpheme(justLike).Not()));

            adjectiveRoot_ST.Add(become_S, "lAş", new NoSurfaceAfterDerivation());
            adjectiveRoot_ST.Add(acquire_S, "lAn", new NoSurfaceAfterDerivation());

            ICondition c1 = new Conditions.PreviousMorphemeIsAny(futPart, pastPart);

            adjAfterVerb_S.AddEmpty(aPnon_ST, c1);
            adjAfterVerb_S.Add(aP1sg_ST, "Im", c1);
            adjAfterVerb_S.Add(aP2sg_ST, "In", c1);
            adjAfterVerb_S.Add(aP3sg_ST, "I", c1);
            adjAfterVerb_S.Add(aP1pl_ST, "ImIz", c1);
            adjAfterVerb_S.Add(aP2pl_ST, "InIz", c1);
            adjAfterVerb_S.Add(aP3pl_ST, "lArI", c1);

            adjectiveRoot_ST.Add(ness_S, "lI~k");
            adjectiveRoot_ST.Add(ness_S, "lI!ğ");

            adjAfterVerb_ST.Add(ness_S, "lI~k", new Conditions.PreviousMorphemeIs(aorPart));
            adjAfterVerb_ST.Add(ness_S, "lI!ğ", new Conditions.PreviousMorphemeIs(aorPart));
        }

        //--------------------- Numeral Root --------------------------------------------------
        MorphemeState numeralRoot_ST = MorphemeState.Builder("numeralRoot_ST", num).Terminal().PosRoot().Build();
        MorphemeState numZeroDeriv_S = MorphemeState.NonTerminalDerivative("numZeroDeriv_S", zero);

        private void ConnectNumeralStates()
        {
            numeralRoot_ST.Add(ness_S, "lI~k");
            numeralRoot_ST.Add(ness_S, "lI!ğ");
            numeralRoot_ST.AddEmpty(numZeroDeriv_S, Conditions.HAS_TAIL);
            numZeroDeriv_S.AddEmpty(noun_S);
            numZeroDeriv_S.AddEmpty(nVerb_S);

            numeralRoot_ST.Add(justLike_S, "+msI",
                new NoSurfaceAfterDerivation()
                    .And(new Conditions.ContainsMorpheme(justLike).Not()));

            numeralRoot_ST.Add(justLike_S, "ImsI",
                Conditions.NotHave(PhoneticAttribute.LastLetterVowel)
                    .And(new NoSurfaceAfterDerivation())
                    .And(new Conditions.ContainsMorpheme(justLike).Not()));

        }

        //-------------- Adjective-Noun connected Verb States ------------------------

        MorphemeState nVerb_S = MorphemeState.Builder("nVerb_S", verb).PosRoot().Build();
        MorphemeState nVerbDegil_S = MorphemeState.Builder("nVerbDegil_S", verb).PosRoot().Build();

        MorphemeState nPresent_S = MorphemeState.NonTerminal("nPresent_S", pres);
        MorphemeState nPast_S = MorphemeState.NonTerminal("nPast_S", past);
        MorphemeState nNarr_S = MorphemeState.NonTerminal("nNarr_S", narr);
        MorphemeState nCond_S = MorphemeState.NonTerminal("nCond_S", cond);
        MorphemeState nA1sg_ST = MorphemeState.Terminal("nA1sg_ST", a1sg);
        MorphemeState nA2sg_ST = MorphemeState.Terminal("nA2sg_ST", a2sg);
        MorphemeState nA1pl_ST = MorphemeState.Terminal("nA1pl_ST", a1pl);
        MorphemeState nA2pl_ST = MorphemeState.Terminal("nA2pl_ST", a2pl);
        MorphemeState nA3sg_ST = MorphemeState.Terminal("nA3sg_ST", a3sg);
        MorphemeState nA3sg_S = MorphemeState.NonTerminal("nA3sg_S", a3sg);
        MorphemeState nA3pl_ST = MorphemeState.Terminal("nA3pl_ST", a3pl);

        MorphemeState nCop_ST = MorphemeState.Terminal("nCop_ST", cop);
        MorphemeState nCopBeforeA3pl_S = MorphemeState.NonTerminal("nCopBeforeA3pl_S", cop);

        MorphemeState nNeg_S = MorphemeState.NonTerminal("nNeg_S", neg);

        private void ConnectVerbAfterNounAdjStates()
        {

            //elma-..-ε-yım
            nVerb_S.AddEmpty(nPresent_S);

            // elma-ydı, çorap-tı
            nVerb_S.Add(nPast_S, "+y>dI");
            // elma-ymış
            nVerb_S.Add(nNarr_S, "+ymIş");

            nVerb_S.Add(nCond_S, "+ysA");

            nVerb_S.Add(vWhile_S, "+yken");

            // word "değil" is special. It contains negative suffix implicitly. Also it behaves like
            // noun->Verb Zero morpheme derivation. because it cannot have most Verb suffixes.
            // So we connect it to a separate root state "nVerbDegil" instead of Verb
            DictionaryItem degilRoot = lexicon.GetItemById("değil_Verb");
            nVerbDegil_S.AddEmpty(nNeg_S, RootIs(degilRoot));
            // copy transitions from nVerb_S
            nNeg_S.CopyOutgoingTransitionsFrom(nVerb_S);

            ICondition noFamily = Conditions.NotHave(RootAttribute.FamilyMember);
            // for preventing elmamım, elmamdım
            // pP1sg_S, pDat_ST, pA1sg_S, pA1pl_S, pA3pl_S, pP2sg_S, pP1pl_S, pP3sg_S, pP1sg_S
            // TODO: below causes "beklemedeyiz" to fail.
            ContainsMorpheme verbDeriv = new Conditions.ContainsMorpheme(inf1, inf2, inf3, pastPart, futPart);
            ICondition allowA1sgTrans =
                noFamily
                    .AndNot(new Conditions.ContainsMorphemeSequence(p1sg, nom))
                    .AndNot(verbDeriv);
            ICondition allowA2sgTrans =
                noFamily
                    .AndNot(new Conditions.ContainsMorphemeSequence(p2sg, nom))
                    .AndNot(verbDeriv);
            ICondition allowA3plTrans =
                noFamily
                    .AndNot(new Conditions.PreviousGroupContains(a3pl_S))
                    .AndNot(new Conditions.ContainsMorphemeSequence(p3pl, nom))
                    .AndNot(verbDeriv);
            ICondition allowA2plTrans =
                noFamily
                    .AndNot(new Conditions.ContainsMorphemeSequence(p2pl, nom))
                    .AndNot(verbDeriv);
            ICondition allowA1plTrans =
                noFamily
                    .AndNot(new Conditions.ContainsMorphemeSequence(p1sg, nom))
                    .AndNot(new Conditions.ContainsMorphemeSequence(p1pl, nom))
                    .AndNot(verbDeriv);
            // elma-yım
            nPresent_S.Add(nA1sg_ST, "+yIm", allowA1sgTrans);
            nPresent_S.Add(nA2sg_ST, "sIn", allowA2sgTrans);

            // elma-ε-ε-dır to non terminal A3sg. We do not allow ending with A3sg from empty Present tense.
            nPresent_S.AddEmpty(nA3sg_S);

            // we allow `değil` to end with terminal A3sg from Present tense.
            nPresent_S.AddEmpty(nA3sg_ST, RootIs(degilRoot));

            // elma-lar, elma-da-lar as Verb.
            // TODO: consider disallowing this for "elmalar" case.
            nPresent_S.Add(nA3pl_ST, "lAr",
                Conditions.NotHave(RootAttribute.CompoundP3sg)
                    // do not allow "okumak-lar"
                    .AndNot(new Conditions.PreviousGroupContainsMorpheme(inf1))
                    .And(allowA3plTrans));

            // elma-ydı-m. Do not allow "elmaya-yım" (Oflazer accepts this)
            nPast_S.Add(nA1sg_ST, "m", allowA1sgTrans);
            nNarr_S.Add(nA1sg_ST, "Im", allowA1sgTrans);

            nPast_S.Add(nA2sg_ST, "n", allowA2sgTrans);
            nNarr_S.Add(nA2sg_ST, "sIn", allowA2sgTrans);

            nPast_S.Add(nA1pl_ST, "k", allowA1plTrans);
            nNarr_S.Add(nA1pl_ST, "Iz", allowA1plTrans);
            nPresent_S.Add(nA1pl_ST, "+yIz", allowA1plTrans);

            nPast_S.Add(nA2pl_ST, "InIz", allowA2plTrans);
            nNarr_S.Add(nA2pl_ST, "sInIz", allowA2plTrans);
            nPresent_S.Add(nA2pl_ST, "sInIz", allowA2plTrans);

            // elma-ydı-lar.
            nPast_S.Add(nA3pl_ST, "lAr",
                Conditions.NotHave(RootAttribute.CompoundP3sg)
                    .And(allowA3plTrans));
            // elma-ymış-lar.
            nNarr_S.Add(nA3pl_ST, "lAr",
                Conditions.NotHave(RootAttribute.CompoundP3sg)
                    .And(allowA3plTrans));

            // elma-ydı-ε
            nPast_S.AddEmpty(nA3sg_ST);
            // elma-ymış-ε
            nNarr_S.AddEmpty(nA3sg_ST);

            // narr+cons is allowed but not past+cond
            nNarr_S.Add(nCond_S, "sA");

            nCond_S.Add(nA1sg_ST, "m", allowA1sgTrans);
            nCond_S.Add(nA2sg_ST, "n", allowA2sgTrans);
            nCond_S.Add(nA1pl_ST, "k", allowA1plTrans);
            nCond_S.Add(nA2pl_ST, "nIz", allowA2plTrans);
            nCond_S.AddEmpty(nA3sg_ST);
            nCond_S.Add(nA3pl_ST, "lAr");

            // for not allowing "elma-ydı-m-dır"
            ICondition rejectNoCopula = new CurrentGroupContainsAny(nPast_S, nCond_S, nCopBeforeA3pl_S)
                .Not();

            //elma-yım-dır
            nA1sg_ST.Add(nCop_ST, "dIr", rejectNoCopula);
            // elmasındır
            nA2sg_ST.Add(nCop_ST, "dIr", rejectNoCopula);
            // elmayızdır
            nA1pl_ST.Add(nCop_ST, "dIr", rejectNoCopula);
            // elmasınızdır
            nA2pl_ST.Add(nCop_ST, "dIr", rejectNoCopula);

            nA3sg_S.Add(nCop_ST, ">dIr", rejectNoCopula);

            nA3pl_ST.Add(nCop_ST, "dIr", rejectNoCopula);

            PreviousMorphemeIsAny asIfCond = new PreviousMorphemeIsAny(narr);
            nA3sg_ST.Add(vAsIf_S, ">cAsInA", asIfCond);
            nA1sg_ST.Add(vAsIf_S, ">cAsInA", asIfCond);
            nA2sg_ST.Add(vAsIf_S, ">cAsInA", asIfCond);
            nA1pl_ST.Add(vAsIf_S, ">cAsInA", asIfCond);
            nA2pl_ST.Add(vAsIf_S, ">cAsInA", asIfCond);
            nA3pl_ST.Add(vAsIf_S, ">cAsInA", asIfCond);

            // Copula can come before A3pl.
            nPresent_S.Add(nCopBeforeA3pl_S, ">dIr");
            nCopBeforeA3pl_S.Add(nA3pl_ST, "lAr");

        }

        // ----------- Pronoun states --------------------------

        // Pronouns have states similar with Nouns.
        MorphemeState pronPers_S = MorphemeState.Builder("pronPers_S", pron).PosRoot().Build();

        MorphemeState pronDemons_S = MorphemeState.Builder("pronDemons_S", pron).PosRoot().Build();
        public MorphemeState pronQuant_S = MorphemeState.Builder("pronQuant_S", pron).PosRoot().Build();
        public MorphemeState pronQuantModified_S =
            MorphemeState.Builder("pronQuantModified_S", pron).PosRoot().Build();
        public MorphemeState pronQues_S = MorphemeState.Builder("pronQues_S", pron).PosRoot().Build();
        public MorphemeState pronReflex_S = MorphemeState.Builder("pronReflex_S", pron).PosRoot().Build();

        // used for ben-sen modification
        public MorphemeState pronPers_Mod_S = MorphemeState.Builder("pronPers_Mod_S", pron).PosRoot().Build();
        // A root for noun->Rel->Pron derivation.
        public MorphemeState pronAfterRel_S = MorphemeState.Builder("pronAfterRel_S", pron).PosRoot().Build();

        MorphemeState pA1sg_S = MorphemeState.NonTerminal("pA1sg_S", a1sg);
        MorphemeState pA2sg_S = MorphemeState.NonTerminal("pA2sg_S", a2sg);

        MorphemeState pA1sgMod_S = MorphemeState.NonTerminal("pA1sgMod_S", a1sg); // for modified ben
        MorphemeState pA2sgMod_S = MorphemeState.NonTerminal("pA2sgMod_S", a2sg); // for modified sen

        MorphemeState pA3sg_S = MorphemeState.NonTerminal("pA3sg_S", a3sg);
        MorphemeState pA3sgRel_S = MorphemeState.NonTerminal("pA3sgRel_S", a3sg);
        MorphemeState pA1pl_S = MorphemeState.NonTerminal("pA1pl_S", a1pl);
        MorphemeState pA2pl_S = MorphemeState.NonTerminal("pA2pl_S", a2pl);

        MorphemeState pA3pl_S = MorphemeState.NonTerminal("pA3pl_S", a3pl);
        MorphemeState pA3plRel_S = MorphemeState.NonTerminal("pA3plRel_S", a3pl);

        MorphemeState pQuantA3sg_S = MorphemeState.NonTerminal("pQuantA3sg_S", a3sg);
        MorphemeState pQuantA3pl_S = MorphemeState.NonTerminal("pQuantA3pl_S", a3pl);
        MorphemeState pQuantModA3pl_S = MorphemeState.NonTerminal("pQuantModA3pl_S", a3pl); // for birbirleri etc.
        MorphemeState pQuantA1pl_S = MorphemeState.NonTerminal("pQuantA1pl_S", a1pl);
        MorphemeState pQuantA2pl_S = MorphemeState.NonTerminal("pQuantA2pl_S", a2pl);

        MorphemeState pQuesA3sg_S = MorphemeState.NonTerminal("pQuesA3sg_S", a3sg);
        MorphemeState pQuesA3pl_S = MorphemeState.NonTerminal("pQuesA3pl_S", a3pl);

        MorphemeState pReflexA3sg_S = MorphemeState.NonTerminal("pReflexA3sg_S", a3sg);
        MorphemeState pReflexA3pl_S = MorphemeState.NonTerminal("pReflexA3pl_S", a3pl);
        MorphemeState pReflexA1sg_S = MorphemeState.NonTerminal("pReflexA1sg_S", a1sg);
        MorphemeState pReflexA2sg_S = MorphemeState.NonTerminal("pReflexA2sg_S", a2sg);
        MorphemeState pReflexA1pl_S = MorphemeState.NonTerminal("pReflexA1pl_S", a1pl);
        MorphemeState pReflexA2pl_S = MorphemeState.NonTerminal("pReflexA2pl_S", a2pl);

        // Possessive

        MorphemeState pPnon_S = MorphemeState.NonTerminal("pPnon_S", pnon);
        MorphemeState pPnonRel_S = MorphemeState.NonTerminal("pPnonRel_S", pnon);
        MorphemeState pPnonMod_S = MorphemeState.NonTerminal("pPnonMod_S", pnon); // for modified ben-sen
        MorphemeState pP1sg_S = MorphemeState.NonTerminal("pP1sg_S", p1sg); // kimim
        MorphemeState pP2sg_S = MorphemeState.NonTerminal("pP2sg_S", p2sg);
        MorphemeState pP3sg_S = MorphemeState.NonTerminal("pP3sg_S", p3sg); // for `birisi` etc
        MorphemeState pP1pl_S = MorphemeState.NonTerminal("pP1pl_S", p1pl); // for `birbirimiz` etc
        MorphemeState pP2pl_S = MorphemeState.NonTerminal("pP2pl_S", p2pl); // for `birbiriniz` etc
        MorphemeState pP3pl_S = MorphemeState.NonTerminal("pP3pl_S", p3pl); // for `birileri` etc

        // Case

        MorphemeState pNom_ST = MorphemeState.Terminal("pNom_ST", nom);
        MorphemeState pDat_ST = MorphemeState.Terminal("pDat_ST", dat);
        MorphemeState pAcc_ST = MorphemeState.Terminal("pAcc_ST", acc);
        MorphemeState pAbl_ST = MorphemeState.Terminal("pAbl_ST", abl);
        MorphemeState pLoc_ST = MorphemeState.Terminal("pLoc_ST", loc);
        MorphemeState pGen_ST = MorphemeState.Terminal("pGen_ST", gen);
        MorphemeState pIns_ST = MorphemeState.Terminal("pIns_ST", ins);
        MorphemeState pEqu_ST = MorphemeState.Terminal("pEqu_ST", equ);

        MorphemeState pronZeroDeriv_S = MorphemeState.NonTerminalDerivative("pronZeroDeriv_S", zero);

        private void ConnectPronounStates()
        {

            //----------- Personal Pronouns ----------------------------
            DictionaryItem ben = lexicon.GetItemById("ben_Pron_Pers");
            DictionaryItem sen = lexicon.GetItemById("sen_Pron_Pers");
            DictionaryItem o = lexicon.GetItemById("o_Pron_Pers");
            DictionaryItem biz = lexicon.GetItemById("biz_Pron_Pers");
            DictionaryItem siz = lexicon.GetItemById("siz_Pron_Pers");
            DictionaryItem falan = lexicon.GetItemById("falan_Pron_Pers");
            DictionaryItem falanca = lexicon.GetItemById("falanca_Pron_Pers");

            pronPers_S.AddEmpty(pA1sg_S, RootIs(ben));
            pronPers_S.AddEmpty(pA2sg_S, RootIs(sen));
            pronPers_S.AddEmpty(pA3sg_S, RootIsAny(o, falan, falanca));
            pronPers_S
                .Add(pA3pl_S, "nlAr", RootIs(o)); // Oflazer does not have "onlar" as Pronoun root.
            pronPers_S
                .Add(pA3pl_S, "lAr", RootIsAny(falan, falanca));
            pronPers_S.AddEmpty(pA1pl_S, RootIs(biz));
            pronPers_S.Add(pA1pl_S, "lAr", RootIs(biz));
            pronPers_S.AddEmpty(pA2pl_S, RootIs(siz));
            pronPers_S.Add(pA2pl_S, "lAr", RootIs(siz));

            // --- modified `ben-sen` special state and transitions
            pronPers_Mod_S.AddEmpty(pA1sgMod_S, RootIs(ben));
            pronPers_Mod_S.AddEmpty(pA2sgMod_S, RootIs(sen));
            pA1sgMod_S.AddEmpty(pPnonMod_S);
            pA2sgMod_S.AddEmpty(pPnonMod_S);
            pPnonMod_S.Add(pDat_ST, "A");
            // ----

            // Possesive connecitons are not used.
            pA1sg_S.AddEmpty(pPnon_S);
            pA2sg_S.AddEmpty(pPnon_S);
            pA3sg_S.AddEmpty(pPnon_S);
            pA1pl_S.AddEmpty(pPnon_S);
            pA2pl_S.AddEmpty(pPnon_S);
            pA3pl_S.AddEmpty(pPnon_S);

            //------------ Noun -> Rel -> Pron ---------------------------
            // masanınki
            pronAfterRel_S.AddEmpty(pA3sgRel_S);
            pronAfterRel_S.Add(pA3plRel_S, "lAr");
            pA3sgRel_S.AddEmpty(pPnonRel_S);
            pA3plRel_S.AddEmpty(pPnonRel_S);
            pPnonRel_S.AddEmpty(pNom_ST);
            pPnonRel_S.Add(pDat_ST, "+nA");
            pPnonRel_S.Add(pAcc_ST, "+nI");
            pPnonRel_S.Add(pAbl_ST, "+ndAn");
            pPnonRel_S.Add(pLoc_ST, "+ndA");
            pPnonRel_S.Add(pIns_ST, "+ylA");
            pPnonRel_S.Add(pGen_ST, "+nIn");

            //------------ Demonstrative pronouns. ------------------------

            DictionaryItem bu = lexicon.GetItemById("bu_Pron_Demons");
            DictionaryItem su = lexicon.GetItemById("şu_Pron_Demons");
            DictionaryItem o_demons = lexicon.GetItemById("o_Pron_Demons");

            pronDemons_S.AddEmpty(pA3sg_S);
            pronDemons_S.Add(pA3pl_S, "nlAr");

            //------------ Quantitiva Pronouns ----------------------------

            DictionaryItem birbiri = lexicon.GetItemById("birbiri_Pron_Quant");
            DictionaryItem biri = lexicon.GetItemById("biri_Pron_Quant");
            DictionaryItem bazi = lexicon.GetItemById("bazı_Pron_Quant");
            DictionaryItem bircogu = lexicon.GetItemById("birçoğu_Pron_Quant");
            DictionaryItem birkaci = lexicon.GetItemById("birkaçı_Pron_Quant");
            DictionaryItem beriki = lexicon.GetItemById("beriki_Pron_Quant");
            DictionaryItem cogu = lexicon.GetItemById("çoğu_Pron_Quant");
            DictionaryItem cumlesi = lexicon.GetItemById("cümlesi_Pron_Quant");
            DictionaryItem hep = lexicon.GetItemById("hep_Pron_Quant");
            DictionaryItem herbiri = lexicon.GetItemById("herbiri_Pron_Quant");
            DictionaryItem herkes = lexicon.GetItemById("herkes_Pron_Quant");
            DictionaryItem hicbiri = lexicon.GetItemById("hiçbiri_Pron_Quant");
            DictionaryItem hepsi = lexicon.GetItemById("hepsi_Pron_Quant");
            DictionaryItem kimi = lexicon.GetItemById("kimi_Pron_Quant");
            DictionaryItem kimse = lexicon.GetItemById("kimse_Pron_Quant");
            DictionaryItem oburku = lexicon.GetItemById("öbürkü_Pron_Quant");
            DictionaryItem oburu = lexicon.GetItemById("öbürü_Pron_Quant");
            DictionaryItem tumu = lexicon.GetItemById("tümü_Pron_Quant");
            DictionaryItem topu = lexicon.GetItemById("topu_Pron_Quant");
            DictionaryItem umum = lexicon.GetItemById("umum_Pron_Quant");

            // we have separate A3pl and A3sg states for Quantitive Pronouns.
            // herkes and hep cannot be singular.
            pronQuant_S.AddEmpty(pQuantA3sg_S,
                RootIsNone(herkes, umum, hepsi, cumlesi, hep, tumu, birkaci, topu));

            pronQuant_S.Add(pQuantA3pl_S, "lAr",
                RootIsNone(hep, hepsi, birkaci, umum, cumlesi, cogu, bircogu, herbiri, tumu, hicbiri, topu,
                    oburu));

            // bazılarınız -> A1pl+P1pl
            pronQuant_S.Add(pQuantA1pl_S, "lAr", RootIsAny(bazi));
            pronQuant_S.Add(pQuantA2pl_S, "lAr", RootIsAny(bazi));

            // Herkes is implicitly plural.
            pronQuant_S.AddEmpty(pQuantA3pl_S,
                RootIsAny(herkes, umum, birkaci, hepsi, cumlesi, cogu, bircogu, tumu, topu));

            // connect "kimse" to Noun-A3sg and Noun-A3pl. It behaves like a noun.
            pronQuant_S.AddEmpty(a3sg_S, RootIs(kimse));
            pronQuant_S.Add(a3pl_S, "lAr", RootIsAny(kimse));

            // for `birbiri-miz` `hep-imiz`
            pronQuant_S.AddEmpty(pQuantA1pl_S,
                RootIsAny(biri, bazi, birbiri, birkaci, herbiri, hep, kimi,
                    cogu, bircogu, tumu, topu, hicbiri));

            // for `birbiri-niz` and `hep-iniz`
            pronQuant_S.AddEmpty(pQuantA2pl_S,
                RootIsAny(biri, bazi, birbiri, birkaci, herbiri, hep, kimi, cogu, bircogu, tumu, topu,
                    hicbiri));

            // this is used for birbir-ler-i, çok-lar-ı, birçok-lar-ı separate root and A3pl states are
            // used for this.
            pronQuantModified_S.AddEmpty(pQuantModA3pl_S);
            pQuantModA3pl_S.Add(pP3pl_S, "lArI");

            // both `biri-ne` and `birisi-ne` or `birbirine` and `birbirisine` are accepted.
            pQuantA3sg_S.AddEmpty(pP3sg_S,
                RootIsAny(biri, birbiri, kimi, herbiri, hicbiri, oburu, oburku, beriki)
                    .And(Conditions.NotHave(PhoneticAttribute.ModifiedPronoun)));

            pQuantA3sg_S.Add(pP3sg_S, "sI",
                RootIsAny(biri, bazi, kimi, birbiri, herbiri, hicbiri, oburku)
                    .And(Conditions.NotHave(PhoneticAttribute.ModifiedPronoun)));

            // there is no connection from pQuantA3pl to Pnon for preventing `biriler` (except herkes)
            pQuantA3pl_S.Add(pP3pl_S, "I", RootIsAny(biri, bazi, birbiri, kimi, oburku, beriki));
            pQuantA3pl_S.AddEmpty(pP3pl_S, RootIsAny(hepsi, birkaci, cumlesi, cogu, tumu, topu, bircogu));
            pQuantA3pl_S.AddEmpty(pPnon_S, RootIsAny(herkes, umum, oburku, beriki));

            pQuantA1pl_S.Add(pP1pl_S, "ImIz");
            pQuantA2pl_S.Add(pP2pl_S, "InIz");

            //------------ Question Pronouns ----------------------------
            // `kim` (kim_Pron_Ques), `ne` and `nere`
            DictionaryItem ne = lexicon.GetItemById("ne_Pron_Ques");
            DictionaryItem nere = lexicon.GetItemById("nere_Pron_Ques");
            DictionaryItem kim = lexicon.GetItemById("kim_Pron_Ques");

            pronQues_S.AddEmpty(pQuesA3sg_S);
            pronQues_S.Add(pQuesA3pl_S, "lAr");

            pQuesA3sg_S.AddEmpty(pPnon_S)
                .Add(pP3sg_S, "+sI")
                .Add(pP1sg_S, "Im", RootIsNot(ne))
                .Add(pP1sg_S, "yIm", RootIs(ne))
                .Add(pP2sg_S, "In", RootIsNot(ne))
                .Add(pP2sg_S, "yIn", RootIs(ne))
                .Add(pP1pl_S, "ImIz", RootIsNot(ne))
                .Add(pP1pl_S, "yImIz", RootIs(ne));

            pQuesA3pl_S.AddEmpty(pPnon_S)
                .Add(pP3sg_S, "I")
                .Add(pP1sg_S, "Im")
                .Add(pP1pl_S, "ImIz");

            //------------ Reflexive Pronouns ----------------------------
            // `kendi`
            DictionaryItem kendi = lexicon.GetItemById("kendi_Pron_Reflex");
            pronReflex_S.AddEmpty(pReflexA1sg_S)
                .AddEmpty(pReflexA2sg_S)
                .AddEmpty(pReflexA3sg_S)
                .AddEmpty(pReflexA1pl_S)
                .AddEmpty(pReflexA2pl_S)
                .AddEmpty(pReflexA3pl_S);

            pReflexA1sg_S.Add(pP1sg_S, "Im");
            pReflexA2sg_S.Add(pP2sg_S, "In");
            pReflexA3sg_S.Add(pP3sg_S, "+sI").AddEmpty(pP3sg_S);
            pReflexA1pl_S.Add(pP1pl_S, "ImIz");
            pReflexA2pl_S.Add(pP2pl_S, "InIz");
            pReflexA3pl_S.Add(pP3pl_S, "lArI");

            // ------------------------
            // Case connections for all
            ICondition nGroup = RootIsNone(ne, nere, falan, falanca, hep, herkes);
            ICondition yGroup = RootIsAny(ne, nere, falan, falanca, hep, herkes);

            pPnon_S.AddEmpty(pNom_ST)
                // not allowing `ben-e` and `sen-e`. `ban-a` and `san-a` are using different states
                .Add(pDat_ST, "+nA", RootIsNone(ben, sen, ne, nere, falan, falanca, herkes))
                .Add(pDat_ST, "+yA", yGroup)
                .Add(pAcc_ST, "+nI", nGroup)
                .Add(pAcc_ST, "+yI", yGroup)
                .Add(pLoc_ST, "+ndA", nGroup)
                .Add(pLoc_ST, ">dA", yGroup)
                .Add(pAbl_ST, "+ndAn", nGroup)
                .Add(pAbl_ST, ">dAn", yGroup)
                .Add(pGen_ST, "+nIn", nGroup.And(RootIsNone(biz, ben, sen)))
                .Add(pGen_ST, "im", RootIsAny(ben, biz)) // benim, senin, bizim are genitive.
                .Add(pGen_ST, "in", RootIs(sen))
                .Add(pGen_ST, "+yIn", yGroup.And(RootIsNone(biz)))
                .Add(pEqu_ST, ">cA", yGroup)
                .Add(pEqu_ST, ">cA", nGroup)
                .Add(pIns_ST, "+ylA", yGroup)
                .Add(pIns_ST, "+nlA", nGroup)
                .Add(pIns_ST, "+nInlA", nGroup.And(RootIsAny(bu, su, o, sen)))
                .Add(pIns_ST, "inle", RootIs(siz))
                .Add(pIns_ST, "imle", RootIsAny(biz, ben));

            ICondition conditionpP1sg_S = Conditions.RootIsAny(kim, ben, ne, nere, kendi);

            pP1sg_S
                .AddEmpty(pNom_ST)
                .Add(pDat_ST, "+nA", nGroup)
                .Add(pAcc_ST, "+nI", nGroup)
                .Add(pDat_ST, "+yA", yGroup)
                .Add(pAcc_ST, "+yI", yGroup)
                .Add(pLoc_ST, "+ndA", RootIsAny(kendi))
                .Add(pAbl_ST, "+ndAn", RootIsAny(kendi))
                .Add(pEqu_ST, "+ncA", RootIsAny(kendi))
                .Add(pIns_ST, "+nlA", conditionpP1sg_S)
                .Add(pGen_ST, "+nIn", conditionpP1sg_S);

            ICondition conditionP2sg = Conditions.RootIsAny(kim, sen, ne, nere, kendi);
            pP2sg_S
                .AddEmpty(pNom_ST)
                .Add(pDat_ST, "+nA", nGroup)
                .Add(pAcc_ST, "+nI", nGroup)
                .Add(pDat_ST, "+yA", yGroup)
                .Add(pAcc_ST, "+yI", yGroup)
                .Add(pLoc_ST, "+ndA", RootIsAny(kendi))
                .Add(pAbl_ST, "+ndAn", RootIsAny(kendi))
                .Add(pEqu_ST, "+ncA", RootIsAny(kendi))
                .Add(pIns_ST, "+nlA", conditionP2sg)
                .Add(pGen_ST, "+nIn", conditionP2sg);

            ICondition p3sgCond = Conditions.RootIsAny(
                kendi, kim, ne, nere, o, bazi, biri, birbiri, herbiri, hep, kimi, hicbiri);

            pP3sg_S
                .AddEmpty(pNom_ST)
                .Add(pDat_ST, "+nA", nGroup)
                .Add(pAcc_ST, "+nI", nGroup)
                .Add(pDat_ST, "+yA", yGroup)
                .Add(pAcc_ST, "+yI", yGroup)
                .Add(pLoc_ST, "+ndA", p3sgCond)
                .Add(pAbl_ST, "+ndAn", p3sgCond)
                .Add(pGen_ST, "+nIn", p3sgCond)
                .Add(pEqu_ST, "ncA", p3sgCond)
                .Add(pIns_ST, "+ylA", p3sgCond);

            ICondition hepCnd = Conditions.RootIsAny(
                kendi, kim, ne, nere, biz, siz, biri, birbiri, birkaci, herbiri, hep, kimi, cogu, bircogu,
                tumu, topu, bazi, hicbiri);
            pP1pl_S
                .AddEmpty(pNom_ST)
                .Add(pDat_ST, "+nA", nGroup)
                .Add(pAcc_ST, "+nI", nGroup)
                .Add(pDat_ST, "+yA", yGroup)
                .Add(pAcc_ST, "+yI", yGroup)
                .Add(pLoc_ST, "+ndA", hepCnd)
                .Add(pAbl_ST, "+ndAn", hepCnd)
                .Add(pGen_ST, "+nIn", hepCnd)
                .Add(pEqu_ST, "+ncA", hepCnd)
                .Add(pIns_ST, "+nlA", hepCnd);

            pP2pl_S
                .AddEmpty(pNom_ST)
                .Add(pDat_ST, "+nA", nGroup)
                .Add(pAcc_ST, "+nI", nGroup)
                .Add(pDat_ST, "+yA", yGroup)
                .Add(pAcc_ST, "+yI", yGroup)
                .Add(pLoc_ST, "+ndA", hepCnd)
                .Add(pAbl_ST, "+ndAn", hepCnd)
                .Add(pGen_ST, "+nIn", hepCnd)
                .Add(pEqu_ST, "+ncA", hepCnd)
                .Add(pIns_ST, "+nlA", hepCnd);

            ICondition hepsiCnd = Conditions.RootIsAny(
                kendi, kim, ne, nere, o, bazi, biri, herkes, umum, birkaci, hepsi, cumlesi, cogu,
                bircogu, birbiri, tumu, kimi, topu);

            pP3pl_S
                .AddEmpty(pNom_ST)
                .Add(pDat_ST, "+nA", nGroup)
                .Add(pAcc_ST, "+nI", nGroup)
                .Add(pDat_ST, "+yA", yGroup)
                .Add(pAcc_ST, "+yI", yGroup)
                .Add(pLoc_ST, "+ndA", hepsiCnd)
                .Add(pAbl_ST, "+ndAn", hepsiCnd)
                .Add(pGen_ST, "+nIn", hepsiCnd.Or(Conditions.RootIsAny(sen, siz)))
                .Add(pEqu_ST, "+ncA", hepsiCnd)
                .Add(pIns_ST, "+ylA", hepsiCnd);

            pNom_ST.Add(with_S, "+nlI", Conditions.RootIsAny(bu, su, o_demons, ben, sen, o, biz, siz));
            pNom_ST.Add(with_S, "lI", Conditions.RootIsAny(nere));
            pNom_ST.Add(with_S, "+ylI", Conditions.RootIsAny(ne));
            pNom_ST.Add(without_S, "+nsIz",
                Conditions.RootIsAny(nere, bu, su, o_demons, ben, sen, o, biz, siz));
            pNom_ST.Add(without_S, "+ysIz", Conditions.RootIsAny(ne));
            pGen_ST.Add(rel_S, "ki", Conditions.RootIsAny(nere, bu, su, o_demons, ne, sen, o, biz, siz));

            ICondition notRelRepetition = new HasTailSequence(rel, adj, zero, noun, a3sg, pnon, loc).Not();
            pLoc_ST.Add(rel_S, "ki", notRelRepetition);

            pIns_ST.Add(vWhile_S, "+yken");

            //------------- Derivation connections ---------

            pNom_ST.AddEmpty(pronZeroDeriv_S, Conditions.HAS_TAIL);
            pDat_ST.AddEmpty(pronZeroDeriv_S, Conditions.HAS_TAIL);
            pLoc_ST.AddEmpty(pronZeroDeriv_S, Conditions.HAS_TAIL);
            pAbl_ST.AddEmpty(pronZeroDeriv_S, Conditions.HAS_TAIL);
            pGen_ST.AddEmpty(pronZeroDeriv_S, Conditions.HAS_TAIL);
            pIns_ST.AddEmpty(pronZeroDeriv_S, Conditions.HAS_TAIL);

            pronZeroDeriv_S.AddEmpty(pvVerbRoot_S);
        }

        MorphemeState pvPresent_S = MorphemeState.NonTerminal("pvPresent_S", pres);
        MorphemeState pvPast_S = MorphemeState.NonTerminal("pvPast_S", past);
        MorphemeState pvNarr_S = MorphemeState.NonTerminal("pvNarr_S", narr);
        MorphemeState pvCond_S = MorphemeState.NonTerminal("pvCond_S", cond);
        MorphemeState pvA1sg_ST = MorphemeState.Terminal("pvA1sg_ST", a1sg);
        MorphemeState pvA2sg_ST = MorphemeState.Terminal("pvA2sg_ST", a2sg);
        MorphemeState pvA3sg_ST = MorphemeState.Terminal("pvA3sg_ST", a3sg);
        MorphemeState pvA3sg_S = MorphemeState.NonTerminal("pvA3sg_S", a3sg);
        MorphemeState pvA1pl_ST = MorphemeState.Terminal("pvA1pl_ST", a1pl);
        MorphemeState pvA2pl_ST = MorphemeState.Terminal("pvA2pl_ST", a2pl);
        MorphemeState pvA3pl_ST = MorphemeState.Terminal("pvA3pl_ST", a3pl);

        MorphemeState pvCopBeforeA3pl_S = MorphemeState.NonTerminal("pvCopBeforeA3pl_S", cop);
        MorphemeState pvCop_ST = MorphemeState.Terminal("pvCop_ST", cop);

        MorphemeState pvVerbRoot_S = MorphemeState.Builder("pvVerbRoot_S", verb).PosRoot().Build();

        private void ConnectVerbAfterPronoun()
        {

            pvVerbRoot_S.AddEmpty(pvPresent_S);

            pvVerbRoot_S.Add(vWhile_S, "+yken");

            pvVerbRoot_S.Add(pvPast_S, "+ydI");

            pvVerbRoot_S.Add(pvNarr_S, "+ymIş");

            pvVerbRoot_S.Add(pvCond_S, "+ysA");

            // disallow `benin, bizim with A1sg analysis etc.`
            ICondition allowA1sgTrans = new Conditions.PreviousGroupContains(pA1pl_S, pP1sg_S).Not();
            ICondition allowA1plTrans =
                new Conditions.PreviousGroupContains(pA1sg_S, pA2sg_S, pP1sg_S, pP2sg_S).Not();
            ICondition allowA2sgTrans = new Conditions.PreviousGroupContains(pA2pl_S, pP2sg_S).Not();
            ICondition allowA2plTrans = new Conditions.PreviousGroupContains(pA2sg_S, pP2pl_S).Not();

            pvPresent_S.Add(pvA1sg_ST, "+yIm", allowA1sgTrans);
            pvPresent_S.Add(pvA2sg_ST, "sIn", allowA2sgTrans);
            // We do not allow ending with A3sg from empty Present tense.
            pvPresent_S.AddEmpty(nA3sg_S);
            pvPresent_S.Add(pvA1pl_ST, "+yIz", allowA1plTrans);
            pvPresent_S.Add(pvA2pl_ST, "sInIz");
            pvPresent_S.Add(pvA3pl_ST, "lAr", new Conditions.PreviousGroupContains(pLoc_ST));

            pvPast_S.Add(pvA1sg_ST, "m", allowA1sgTrans);
            pvPast_S.Add(pvA2sg_ST, "n", allowA2sgTrans);
            pvPast_S.Add(pvA1pl_ST, "k", allowA1plTrans);
            pvPast_S.Add(pvA2pl_ST, "InIz");
            pvPast_S.Add(pvA3pl_ST, "lAr");
            pvPast_S.AddEmpty(pvA3sg_ST);

            pvNarr_S.Add(pvA1sg_ST, "Im", allowA1sgTrans);
            pvNarr_S.Add(pvA2sg_ST, "sIn", allowA2sgTrans);
            pvNarr_S.Add(pvA1pl_ST, "Iz", allowA1plTrans);
            pvNarr_S.Add(pvA2pl_ST, "sInIz");
            pvNarr_S.Add(pvA3pl_ST, "lAr");
            pvNarr_S.AddEmpty(pvA3sg_ST);
            // narr+cons is allowed but not past+cond
            pvNarr_S.Add(pvCond_S, "sA");

            pvCond_S.Add(pvA1sg_ST, "m", allowA1sgTrans);
            pvCond_S.Add(pvA2sg_ST, "n", allowA2sgTrans);
            pvCond_S.Add(pvA1pl_ST, "k", allowA1plTrans);
            pvCond_S.Add(pvA2pl_ST, "nIz", allowA2plTrans);
            pvCond_S.AddEmpty(pvA3sg_ST);
            pvCond_S.Add(pvA3pl_ST, "lAr");

            ICondition rejectNoCopula = new CurrentGroupContainsAny(pvPast_S, pvCond_S, pvCopBeforeA3pl_S)
                .Not();

            pvA1sg_ST.Add(pvCop_ST, "dIr", rejectNoCopula);
            pvA2sg_ST.Add(pvCop_ST, "dIr", rejectNoCopula);
            pvA1pl_ST.Add(pvCop_ST, "dIr", rejectNoCopula);
            pvA2pl_ST.Add(pvCop_ST, "dIr", rejectNoCopula);

            pvA3sg_S.Add(pvCop_ST, ">dIr", rejectNoCopula);

            pvA3pl_ST.Add(pvCop_ST, "dIr", rejectNoCopula);

            // Copula can come before A3pl.
            pvPresent_S.Add(pvCopBeforeA3pl_S, ">dIr");
            pvCopBeforeA3pl_S.Add(pvA3pl_ST, "lAr");

        }

        // ------------- Adverbs -----------------

        MorphemeState advRoot_ST = MorphemeState.Builder("advRoot_ST", adv).PosRoot().Terminal().Build();
        MorphemeState advNounRoot_ST = MorphemeState.Builder("advRoot_ST", adv).PosRoot().Terminal().Build();
        MorphemeState advForVerbDeriv_ST =
            MorphemeState.Builder("advForVerbDeriv_ST", adv).PosRoot().Terminal().Build();

        MorphemeState avNounAfterAdvRoot_ST = MorphemeState.Builder("advToNounRoot_ST", noun).PosRoot().Build();
        MorphemeState avA3sg_S = MorphemeState.NonTerminal("avA3sg_S", a3sg);
        MorphemeState avPnon_S = MorphemeState.NonTerminal("avPnon_S", pnon);
        MorphemeState avDat_ST = MorphemeState.Terminal("avDat_ST", dat);

        MorphemeState avZero_S = MorphemeState.NonTerminalDerivative("avZero_S", zero);
        MorphemeState avZeroToVerb_S = MorphemeState.NonTerminalDerivative("avZeroToVerb_S", zero);

        private void ConnectAdverbs()
        {
            advNounRoot_ST.AddEmpty(avZero_S);
            avZero_S.AddEmpty(avNounAfterAdvRoot_ST);
            avNounAfterAdvRoot_ST.AddEmpty(avA3sg_S);
            avA3sg_S.AddEmpty(avPnon_S);
            avPnon_S.Add(avDat_ST, "+yA");

            advForVerbDeriv_ST.AddEmpty(avZeroToVerb_S);
            avZeroToVerb_S.AddEmpty(nVerb_S);
        }

        // ------------- Interjection, Conjunctions, Determiner and Duplicator  -----------------

        MorphemeState conjRoot_ST = MorphemeState.Builder("conjRoot_ST", conj).PosRoot().Terminal().Build();
        MorphemeState interjRoot_ST = MorphemeState.Builder("interjRoot_ST", interj).PosRoot().Terminal().Build();
        MorphemeState detRoot_ST = MorphemeState.Builder("detRoot_ST", det).PosRoot().Terminal().Build();
        MorphemeState dupRoot_ST = MorphemeState.Builder("dupRoot_ST", dup).PosRoot().Terminal().Build();

        // ------------- Post Positive ------------------------------------------------

        MorphemeState postpRoot_ST = MorphemeState.Builder("postpRoot_ST", postp).PosRoot().Terminal().Build();
        MorphemeState postpZero_S = MorphemeState.NonTerminalDerivative("postpZero_S", zero);

        MorphemeState po2nRoot_S = MorphemeState.NonTerminal("po2nRoot_S", noun);

        MorphemeState po2nA3sg_S = MorphemeState.NonTerminal("po2nA3sg_S", a3sg);
        MorphemeState po2nA3pl_S = MorphemeState.NonTerminal("po2nA3pl_S", a3pl);

        MorphemeState po2nP3sg_S = MorphemeState.NonTerminal("po2nP3sg_S", p3sg);
        MorphemeState po2nP1sg_S = MorphemeState.NonTerminal("po2nP1sg_S", p1sg);
        MorphemeState po2nP2sg_S = MorphemeState.NonTerminal("po2nP2sg_S", p2sg);
        MorphemeState po2nP1pl_S = MorphemeState.NonTerminal("po2nP1pl_S", p1pl);
        MorphemeState po2nP2pl_S = MorphemeState.NonTerminal("po2nP2pl_S", p2pl);
        MorphemeState po2nPnon_S = MorphemeState.NonTerminal("po2nPnon_S", pnon);


        MorphemeState po2nNom_ST = MorphemeState.Terminal("po2nNom_ST", nom);
        MorphemeState po2nDat_ST = MorphemeState.Terminal("po2nDat_ST", dat);
        MorphemeState po2nAbl_ST = MorphemeState.Terminal("po2nAbl_ST", abl);
        MorphemeState po2nLoc_ST = MorphemeState.Terminal("po2nLoc_ST", loc);
        MorphemeState po2nIns_ST = MorphemeState.Terminal("po2nIns_ST", ins);
        MorphemeState po2nAcc_ST = MorphemeState.Terminal("po2nAcc_ST", acc);
        MorphemeState po2nGen_ST = MorphemeState.Terminal("po2nGen_ST", gen);
        MorphemeState po2nEqu_ST = MorphemeState.Terminal("po2nEqu_ST", equ);

        private void ConnectPostpositives()
        {

            postpRoot_ST.AddEmpty(postpZero_S);
            postpZero_S.AddEmpty(nVerb_S);

            // gibi is kind of special.
            DictionaryItem gibiGen = lexicon.GetItemById("gibi_Postp_PCGen");
            DictionaryItem gibiNom = lexicon.GetItemById("gibi_Postp_PCNom");
            DictionaryItem sonraAbl = lexicon.GetItemById("sonra_Postp_PCAbl");

            postpZero_S.AddEmpty(po2nRoot_S, RootIsAny(gibiGen, gibiNom, sonraAbl));

            po2nRoot_S.AddEmpty(po2nA3sg_S);
            po2nRoot_S.Add(po2nA3pl_S, "lAr");

            // gibisi, gibim-e, gibi-e, gibi-mize
            po2nA3sg_S.Add(po2nP3sg_S, "+sI");
            po2nA3sg_S.Add(po2nP1sg_S, "m", Conditions.RootIsAny(gibiGen, gibiNom));
            po2nA3sg_S.Add(po2nP2sg_S, "n", Conditions.RootIsAny(gibiGen, gibiNom));
            po2nA3sg_S.Add(po2nP1pl_S, "miz", Conditions.RootIsAny(gibiGen, gibiNom));
            po2nA3sg_S.Add(po2nP2pl_S, "niz", Conditions.RootIsAny(gibiGen, gibiNom));

            // gibileri
            po2nA3pl_S.Add(po2nP3sg_S, "+sI");
            po2nA3pl_S.AddEmpty(po2nPnon_S);

            po2nP3sg_S
                .AddEmpty(po2nNom_ST)
                .Add(po2nDat_ST, "nA")
                .Add(po2nLoc_ST, "ndA")
                .Add(po2nAbl_ST, "ndAn")
                .Add(po2nIns_ST, "ylA")
                .Add(po2nGen_ST, "nIn")
                .Add(po2nAcc_ST, "nI");

            po2nPnon_S
                .AddEmpty(po2nNom_ST)
                .Add(po2nDat_ST, "A")
                .Add(po2nLoc_ST, "dA")
                .Add(po2nAbl_ST, "dAn")
                .Add(po2nIns_ST, "lA")
                .Add(po2nGen_ST, "In")
                .Add(po2nEqu_ST, "cA")
                .Add(po2nAcc_ST, "I");

            po2nP1sg_S.Add(po2nDat_ST, "e");
            po2nP2sg_S.Add(po2nDat_ST, "e");
            po2nP1pl_S.Add(po2nDat_ST, "e");
            po2nP2pl_S.Add(po2nDat_ST, "e");
        }

        // ------------- Verbs -----------------------------------

        public MorphemeState verbRoot_S = MorphemeState.Builder("verbRoot_S", verb).PosRoot().Build();
        public MorphemeState verbLastVowelDropModRoot_S =
            MorphemeState.Builder("verbLastVowelDropModRoot_S", verb).PosRoot().Build();
        public MorphemeState verbLastVowelDropUnmodRoot_S =
            MorphemeState.Builder("verbLastVowelDropUnmodRoot_S", verb).PosRoot().Build();

        public MorphemeState vA1sg_ST = MorphemeState.Terminal("vA1sg_ST", a1sg);
        public MorphemeState vA2sg_ST = MorphemeState.Terminal("vA2sg_ST", a2sg);
        public MorphemeState vA3sg_ST = MorphemeState.Terminal("vA3sg_ST", a3sg);
        public MorphemeState vA1pl_ST = MorphemeState.Terminal("vA1pl_ST", a1pl);
        public MorphemeState vA2pl_ST = MorphemeState.Terminal("vA2pl_ST", a2pl);
        public MorphemeState vA3pl_ST = MorphemeState.Terminal("vA3pl_ST", a3pl);

        public MorphemeState vPast_S = MorphemeState.NonTerminal("vPast_S", past);
        public MorphemeState vNarr_S = MorphemeState.NonTerminal("vNarr_S", narr);
        public MorphemeState vCond_S = MorphemeState.NonTerminal("vCond_S", cond);
        public MorphemeState vCondAfterPerson_ST = MorphemeState.Terminal("vCondAfterPerson_ST", cond);
        public MorphemeState vPastAfterTense_S = MorphemeState.NonTerminal("vPastAfterTense_S", past);
        public MorphemeState vNarrAfterTense_S = MorphemeState.NonTerminal("vNarrAfterTense_S", narr);

        // terminal cases are used if A3pl comes before NarrAfterTense, PastAfterTense or vCond
        public MorphemeState vPastAfterTense_ST = MorphemeState.Terminal("vPastAfterTense_ST", past);
        public MorphemeState vNarrAfterTense_ST = MorphemeState.Terminal("vNarrAfterTense_ST", narr);
        public MorphemeState vCond_ST = MorphemeState.Terminal("vCond_ST", cond);

        public MorphemeState vProgYor_S = MorphemeState.NonTerminal("vProgYor_S", prog1);
        public MorphemeState vProgMakta_S = MorphemeState.NonTerminal("vProgMakta_S", prog2);
        public MorphemeState vFut_S = MorphemeState.NonTerminal("vFut_S", fut);

        public MorphemeState vCop_ST = MorphemeState.Terminal("vCop_ST", cop);
        public MorphemeState vCopBeforeA3pl_S = MorphemeState.NonTerminal("vCopBeforeA3pl_S", cop);

        public MorphemeState vNeg_S = MorphemeState.NonTerminal("vNeg_S", neg);
        public MorphemeState vUnable_S = MorphemeState.NonTerminal("vUnable_S", unable);
        // for negative before progressive-1 "Iyor"
        public MorphemeState vNegProg1_S = MorphemeState.NonTerminal("vNegProg1_S", neg);
        public MorphemeState vUnableProg1_S = MorphemeState.NonTerminal("vUnableProg1_S", unable);


        public MorphemeState vImp_S = MorphemeState.NonTerminal("vImp_S", imp);
        public MorphemeState vImpYemekYi_S = MorphemeState.NonTerminal("vImpYemekYi_S", imp);
        public MorphemeState vImpYemekYe_S = MorphemeState.NonTerminal("vImpYemekYe_S", imp);

        public MorphemeState vCausT_S = MorphemeState.NonTerminalDerivative("vCaus_S", caus);
        public MorphemeState vCausTır_S = MorphemeState.NonTerminalDerivative("vCausTır_S", caus);

        public MorphemeState vRecip_S = MorphemeState.NonTerminalDerivative("vRecip_S", recip);
        public MorphemeState vImplicitRecipRoot_S = MorphemeState.Builder("vImplicitRecipRoot_S", verb).PosRoot().Build();

        public MorphemeState vReflex_S = MorphemeState.NonTerminalDerivative("vReflex_S", reflex);
        public MorphemeState vImplicitReflexRoot_S = MorphemeState.Builder("vImplicitReflexRoot_S", verb).PosRoot().Build();

        // for progressive vowel drop.
        public MorphemeState verbRoot_VowelDrop_S = MorphemeState.Builder("verbRoot_VowelDrop_S", verb).PosRoot().Build();

        public MorphemeState vAor_S = MorphemeState.NonTerminal("vAor_S", aor);
        public MorphemeState vAorNeg_S = MorphemeState.NonTerminal("vAorNeg_S", aor);
        public MorphemeState vAorNegEmpty_S = MorphemeState.NonTerminal("vAorNegEmpty_S", aor);
        public MorphemeState vAorPartNeg_S = MorphemeState.NonTerminalDerivative("vAorPartNeg_S", aorPart);
        public MorphemeState vAorPart_S = MorphemeState.NonTerminalDerivative("vAorPart_S", aorPart);

        public MorphemeState vAble_S = MorphemeState.NonTerminalDerivative("vAble_S", able);
        public MorphemeState vAbleNeg_S = MorphemeState.NonTerminalDerivative("vAbleNeg_S", able);
        public MorphemeState vAbleNegDerivRoot_S = MorphemeState.Builder("vAbleNegDerivRoot_S", verb).PosRoot().Build();

        public MorphemeState vPass_S = MorphemeState.NonTerminalDerivative("vPass_S", pass);

        public MorphemeState vOpt_S = MorphemeState.NonTerminal("vOpt_S", opt);
        public MorphemeState vDesr_S = MorphemeState.NonTerminal("vDesr_S", desr);
        public MorphemeState vNeces_S = MorphemeState.NonTerminal("vNeces_S", neces);

        public MorphemeState vInf1_S = MorphemeState.NonTerminalDerivative("vInf1_S", inf1);
        public MorphemeState vInf2_S = MorphemeState.NonTerminalDerivative("vInf2_S", inf2);
        public MorphemeState vInf3_S = MorphemeState.NonTerminalDerivative("vInf3_S", inf3);

        public MorphemeState vAgt_S = MorphemeState.NonTerminalDerivative("vAgt_S", agt);
        public MorphemeState vActOf_S = MorphemeState.NonTerminalDerivative("vActOf_S", actOf);

        public MorphemeState vPastPart_S = MorphemeState.NonTerminalDerivative("vPastPart_S", pastPart);
        public MorphemeState vFutPart_S = MorphemeState.NonTerminalDerivative("vFutPart_S", futPart);
        public MorphemeState vPresPart_S = MorphemeState.NonTerminalDerivative("vPresPart_S", presPart);
        public MorphemeState vNarrPart_S = MorphemeState.NonTerminalDerivative("vNarrPart_S", narrPart);

        public MorphemeState vFeelLike_S = MorphemeState.NonTerminalDerivative("vFeelLike_S", feelLike);

        public MorphemeState vNotState_S = MorphemeState.NonTerminalDerivative("vNotState_S", notState);

        public MorphemeState vEverSince_S = MorphemeState.NonTerminalDerivative("vEverSince_S", everSince);
        public MorphemeState vRepeat_S = MorphemeState.NonTerminalDerivative("vRepeat_S", repeat);
        public MorphemeState vAlmost_S = MorphemeState.NonTerminalDerivative("vAlmost_S", almost);
        public MorphemeState vHastily_S = MorphemeState.NonTerminalDerivative("vHastily_S", hastily);
        public MorphemeState vStay_S = MorphemeState.NonTerminalDerivative("vStay_S", stay);
        public MorphemeState vStart_S = MorphemeState.NonTerminalDerivative("vStart_S", start);

        public MorphemeState vWhile_S = MorphemeState.NonTerminalDerivative("vWhile_S", while_);
        public MorphemeState vWhen_S = MorphemeState.NonTerminalDerivative("vWhen_S", when);
        public MorphemeState vAsIf_S = MorphemeState.NonTerminalDerivative("vAsIf_S", asIf);
        public MorphemeState vSinceDoingSo_S = MorphemeState.NonTerminalDerivative("vSinceDoingSo_S", sinceDoingSo);
        public MorphemeState vAsLongAs_S = MorphemeState.NonTerminalDerivative("vAsLongAs_S", asLongAs);
        public MorphemeState vByDoingSo_S = MorphemeState.NonTerminalDerivative("vByDoingSo_S", byDoingSo);
        public MorphemeState vAdamantly_S = MorphemeState.NonTerminalDerivative("vAdamantly_S", adamantly);
        public MorphemeState vAfterDoing_S = MorphemeState.NonTerminalDerivative("vAfterDoing_S", afterDoingSo);
        public MorphemeState vWithoutHavingDoneSo_S =
            MorphemeState.NonTerminalDerivative("vWithoutHavingDoneSo_S", withoutHavingDoneSo);
        public MorphemeState vWithoutBeingAbleToHaveDoneSo_S =
            MorphemeState.NonTerminalDerivative("vWithoutBeingAbleToHaveDoneSo_S", withoutBeingAbleToHaveDoneSo);

        public MorphemeState vDeYeRoot_S = MorphemeState.Builder("vDeYeRoot_S", verb).PosRoot().Build();

        private void ConnectVerbs()
        {

            // Imperative.
            verbRoot_S.AddEmpty(vImp_S);

            vImp_S
                .AddEmpty(vA2sg_ST)       // oku
                .Add(vA2sg_ST, "sAnA")    // oku
                .Add(vA3sg_ST, "sIn")     // okusun
                .Add(vA2pl_ST, "+yIn")    // okuyun
                .Add(vA2pl_ST, "+yInIz")  // okuyunuz
                .Add(vA2pl_ST, "sAnIzA")  // okuyunuz
                .Add(vA3pl_ST, "sInlAr"); // okusunlar

            // Causative suffixes
            // Causes Verb-Verb derivation. There are three forms: "t", "tIr" and "Ir".
            // 1- "t" form is used if verb ends with a vowel, or immediately after "tIr" Causative.
            // 2- "tIr" form is used if verb ends with a consonant or immediately after "t" Causative.
            // 3- "Ir" form appears after some specific verbs but currently we treat them as separate verb.
            // such as "pişmek - pişirmek". Oflazer parses them as causative.

            verbRoot_S.Add(vCausT_S, "t", Conditions.Has(RootAttribute.Causative_t)
                .Or(new Conditions.LastDerivationIs(vCausTır_S))
                .AndNot(new Conditions.LastDerivationIsAny(vCausT_S, vPass_S, vAble_S)));

            verbRoot_S.Add(vCausTır_S, ">dIr",
                Conditions.Has(PhoneticAttribute.LastLetterConsonant)
                    .AndNot(new Conditions.LastDerivationIsAny(vCausTır_S, vPass_S, vAble_S)));

            vCausT_S.AddEmpty(verbRoot_S);
            vCausTır_S.AddEmpty(verbRoot_S);

            // Progressive1 suffix. "-Iyor"
            // if last letter is a vowel, this is handled with verbRoot_VowelDrop_S root.
            verbRoot_S.Add(vProgYor_S, "Iyor", Conditions.NotHave(PhoneticAttribute.LastLetterVowel));

            // For "aramak", the modified root "ar" connects to verbRoot_VowelDrop_S. Here it is connected to
            // progressive "Iyor" suffix. We use a separate root state for these for convenience.
            verbRoot_VowelDrop_S.Add(vProgYor_S, "Iyor");
            vProgYor_S
                .Add(vA1sg_ST, "um")
                .Add(vA2sg_ST, "sun")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "uz")
                .Add(vA2pl_ST, "sunuz")
                .Add(vA3pl_ST, "lar")
                .Add(vCond_S, "sa")
                .Add(vPastAfterTense_S, "du")
                .Add(vNarrAfterTense_S, "muş")
                .Add(vCopBeforeA3pl_S, "dur")
                .Add(vWhile_S, "ken");

            // Progressive - 2 "-mAktA"
            verbRoot_S.Add(vProgMakta_S, "mAktA");
            vProgMakta_S
                .Add(vA1sg_ST, "yIm")
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "yIz")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr")
                .Add(vCond_S, "ysA")
                .Add(vPastAfterTense_S, "ydI")
                .Add(vNarrAfterTense_S, "ymIş")
                .Add(vCopBeforeA3pl_S, "dIr")
                .Add(vWhile_S, "yken");

            // Positive Aorist Tense.
            // For single syllable words, it forms as "ar-er". For others "ir-ır-ur-ür"
            // However there are exceptions to it as well. So dictionary items are marked as Aorist_I and
            // Aorist_A.
            verbRoot_S.Add(vAor_S, "Ir",
                Conditions.Has(RootAttribute.Aorist_I).Or(Conditions.HAS_SURFACE));
            verbRoot_S.Add(vAor_S, "Ar",
                Conditions.Has(RootAttribute.Aorist_A).And(Conditions.HAS_NO_SURFACE));
            vAor_S
                .Add(vA1sg_ST, "Im")
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "Iz")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr")
                .Add(vPastAfterTense_S, "dI")
                .Add(vNarrAfterTense_S, "mIş")
                .Add(vCond_S, "sA")
                .Add(vCopBeforeA3pl_S, "dIr")
                .Add(vWhile_S, "ken");

            // Negative
            verbRoot_S
                .Add(vNeg_S, "mA", Conditions.PreviousMorphemeIsNot(able));

            vNeg_S.AddEmpty(vImp_S)
                .Add(vPast_S, "dI")
                .Add(vFut_S, "yAcA~k")
                .Add(vFut_S, "yAcA!ğ")
                .Add(vNarr_S, "mIş")
                .Add(vProgMakta_S, "mAktA")
                .Add(vOpt_S, "yA")
                .Add(vDesr_S, "sA")
                .Add(vNeces_S, "mAlI")
                .Add(vInf1_S, "mAk")
                .Add(vInf2_S, "mA")
                .Add(vInf3_S, "yIş")
                .Add(vActOf_S, "mAcA")
                .Add(vPastPart_S, "dI~k")
                .Add(vPastPart_S, "dI!ğ")
                .Add(vFutPart_S, "yAcA~k")
                .Add(vFutPart_S, "yAcA!ğ")
                .Add(vPresPart_S, "yAn")
                .Add(vNarrPart_S, "mIş")
                .Add(vSinceDoingSo_S, "yAlI")
                .Add(vByDoingSo_S, "yArAk")
                .Add(vHastily_S, "yIver")
                .Add(vEverSince_S, "yAgör")
                .Add(vAfterDoing_S, "yIp")
                .Add(vWhen_S, "yIncA")
                .Add(vAsLongAs_S, "dIkçA")
                .Add(vNotState_S, "mAzlI~k")
                .Add(vNotState_S, "mAzlI!ğ")
                .Add(vFeelLike_S, "yAsI");

            // Negative form is "m" before progressive "Iyor" because last vowel drops.
            // We use a separate negative state for this.
            verbRoot_S.Add(vNegProg1_S, "m");
            vNegProg1_S.Add(vProgYor_S, "Iyor");

            // Negative Aorist
            // Aorist tense forms differently after negative. It can be "z" or empty.
            vNeg_S.Add(vAorNeg_S, "z");
            vNeg_S.AddEmpty(vAorNegEmpty_S);
            vAorNeg_S
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr")
                .Add(vPastAfterTense_S, "dI")
                .Add(vNarrAfterTense_S, "mIş")
                .Add(vCond_S, "sA")
                .Add(vCopBeforeA3pl_S, "dIr")
                .Add(vWhile_S, "ken");
            vAorNegEmpty_S
                .Add(vA1sg_ST, "m")
                .Add(vA1pl_ST, "yIz");
            // oku-maz-ım TODO: not sure here.
            vNeg_S.Add(vAorPartNeg_S, "z");
            vAorPartNeg_S.AddEmpty(adjAfterVerb_ST);

            //Positive Ability.
            // This makes a Verb-Verb derivation.
            verbRoot_S.Add(vAble_S, "+yAbil", LastDerivationIsCondition(vAble_S).Not());

            vAble_S.AddEmpty(verbRoot_S);

            // Also for ability that comes before negative, we add a new root state.
            // From there only negative connections is possible.
            vAbleNeg_S.AddEmpty(vAbleNegDerivRoot_S);
            vAbleNegDerivRoot_S.Add(vNeg_S, "mA");
            vAbleNegDerivRoot_S.Add(vNegProg1_S, "m");

            // it is possible to have abil derivation after negative.
            vNeg_S.Add(vAble_S, "yAbil");

            // Unable.
            verbRoot_S
                .Add(vUnable_S, "+yAmA", Conditions.PreviousMorphemeIsNot(able));
            // careful here. We copy all outgoing transitions to "unable"
            vUnable_S.CopyOutgoingTransitionsFrom(vNeg_S);
            verbRoot_S.Add(vUnableProg1_S, "+yAm");
            vUnableProg1_S.Add(vProgYor_S, "Iyor");

            // Infinitive 1 "mAk"
            // Causes Verb to Noun derivation. It is connected to a special noun root state.
            verbRoot_S.Add(vInf1_S, "mA~k");
            vInf1_S.AddEmpty(nounInf1Root_S);

            // Infinitive 2 "mA"
            // Causes Verb to Noun derivation.
            verbRoot_S.Add(vInf2_S, "mA");
            vInf2_S.AddEmpty(noun_S);

            // Infinitive 3 "+yUş"
            // Causes Verb to Noun derivation.
            verbRoot_S.Add(vInf3_S, "+yIş");
            vInf3_S.AddEmpty(noun_S);

            // Agt 3 "+yIcI"
            // Causes Verb to Noun and Adj derivation.
            verbRoot_S.Add(vAgt_S, "+yIcI");
            vAgt_S.AddEmpty(noun_S);
            vAgt_S.AddEmpty(adjAfterVerb_ST);

            // ActOf "mAcA"
            // Causes Verb to Noun and Adj derivation.
            verbRoot_S.Add(vActOf_S, "mAcA");
            vActOf_S.AddEmpty(nounActOfRoot_S);

            // PastPart "oku-duğ-um"
            verbRoot_S.Add(vPastPart_S, ">dI~k");
            verbRoot_S.Add(vPastPart_S, ">dI!ğ");
            vPastPart_S.AddEmpty(noun_S);
            vPastPart_S.AddEmpty(adjAfterVerb_S);

            // FutPart "oku-yacağ-ım kitap"
            verbRoot_S.Add(vFutPart_S, "+yAcA~k");
            verbRoot_S.Add(vFutPart_S, "+yAcA!ğ");
            vFutPart_S.AddEmpty(noun_S, Conditions.HAS_TAIL);
            vFutPart_S.AddEmpty(adjAfterVerb_S);

            // FutPart "oku-yacağ-ım kitap"
            verbRoot_S.Add(vNarrPart_S, "mIş");
            vNarrPart_S.AddEmpty(adjectiveRoot_ST);

            // AorPart "okunabilir-lik"
            verbRoot_S.Add(vAorPart_S, "Ir",
                Conditions.Has(RootAttribute.Aorist_I).Or(Conditions.HAS_SURFACE));
            verbRoot_S.Add(vAorPart_S, "Ar",
                Conditions.Has(RootAttribute.Aorist_A).And(Conditions.HAS_NO_SURFACE));
            vAorPart_S.AddEmpty(adjAfterVerb_ST);

            // PresPart
            verbRoot_S.Add(vPresPart_S, "+yAn");
            vPresPart_S.AddEmpty(noun_S, Conditions.HAS_TAIL);
            vPresPart_S.AddEmpty(adjAfterVerb_ST); // connect to terminal Adj

            // FeelLike
            verbRoot_S.Add(vFeelLike_S, "+yAsI");
            vFeelLike_S.AddEmpty(noun_S, Conditions.HAS_TAIL);
            vFeelLike_S.AddEmpty(adjAfterVerb_ST); // connect to terminal Adj

            // NotState
            verbRoot_S.Add(vNotState_S, "mAzlI~k");
            verbRoot_S.Add(vNotState_S, "mAzlI!ğ");
            vNotState_S.AddEmpty(noun_S);

            // reciprocal
            // TODO: for reducing ambiguity for now remove reciprocal
            /*
                verbRoot_S.Add(vRecip_S, "Iş", notHaveAny(RootAttribute.Reciprocal, RootAttribute.NonReciprocal)
                    .AndNot(new Conditions.ContainsMorpheme(recip)));
            */
            vRecip_S.AddEmpty(verbRoot_S);
            vImplicitRecipRoot_S.AddEmpty(vRecip_S);

            // reflexive
            vImplicitReflexRoot_S.AddEmpty(vReflex_S);
            vReflex_S.AddEmpty(verbRoot_S);

            // Passive
            // Causes Verb-Verb derivation. Passive morpheme has three forms.
            // 1- If Verb ends with a vowel: "In"
            // 2- If Verb ends with letter 'l' : "InIl"
            // 3- If Verb ends with other consonants: "nIl"
            // When loading dictionary, first and second case items are marked with Passive_In

            verbRoot_S.Add(vPass_S, "In", Conditions.Has(RootAttribute.Passive_In)
                .AndNot(new Conditions.ContainsMorpheme(pass)));
            verbRoot_S.Add(vPass_S, "InIl", Conditions.Has(RootAttribute.Passive_In)
                .AndNot(new Conditions.ContainsMorpheme(pass)));
            verbRoot_S.Add(vPass_S, "+nIl",
                new Conditions.PreviousStateIsAny(vCausT_S, vCausTır_S)
                    .Or(Conditions.NotHave(RootAttribute.Passive_In).AndNot(new Conditions.ContainsMorpheme(pass))));
            vPass_S.AddEmpty(verbRoot_S);

            // Condition "oku-r-sa"
            vCond_S
                .Add(vA1sg_ST, "m")
                .Add(vA2sg_ST, "n")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "k")
                .Add(vA2pl_ST, "nIz")
                .Add(vA3pl_ST, "lAr");

            // Past "oku-du"
            verbRoot_S.Add(vPast_S, ">dI");
            vPast_S
                .Add(vA1sg_ST, "m")
                .Add(vA2sg_ST, "n")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "k")
                .Add(vA2pl_ST, "nIz")
                .Add(vA3pl_ST, "lAr");
            vPast_S.Add(vCond_S, "ysA");

            // Narrative "oku-muş"
            verbRoot_S.Add(vNarr_S, "mIş");
            vNarr_S
                .Add(vA1sg_ST, "Im")
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "Iz")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr");
            vNarr_S.Add(vCond_S, "sA");
            vNarr_S.Add(vPastAfterTense_S, "tI");
            vNarr_S.Add(vCopBeforeA3pl_S, "tIr");
            vNarr_S.Add(vWhile_S, "ken");
            vNarr_S.Add(vNarrAfterTense_S, "mIş");

            // Past after tense "oku-muş-tu"
            vPastAfterTense_S
                .Add(vA1sg_ST, "m")
                .Add(vA2sg_ST, "n")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "k")
                .Add(vA2pl_ST, "nIz")
                .Add(vA3pl_ST, "lAr");

            // Narrative after tense "oku-r-muş"
            vNarrAfterTense_S
                .Add(vA1sg_ST, "Im")
                .Add(vA2sg_ST, "sIn")
                // for preventing yap+ar+lar(A3pl)+mış+A3sg
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "Iz")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr");
            vNarrAfterTense_S.Add(vWhile_S, "ken");
            vNarrAfterTense_S.Add(vCopBeforeA3pl_S, "tIr");

            // Future "oku-yacak"
            verbRoot_S.Add(vFut_S, "+yAcA~k");
            verbRoot_S.Add(vFut_S, "+yAcA!ğ");

            vFut_S
                .Add(vA1sg_ST, "Im")
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "Iz")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr");
            vFut_S.Add(vCond_S, "sA");
            vFut_S.Add(vPastAfterTense_S, "tI");
            vFut_S.Add(vNarrAfterTense_S, "mIş");
            vFut_S.Add(vCopBeforeA3pl_S, "tIr");
            vFut_S.Add(vWhile_S, "ken");

            // `demek` and `yemek` are special because they are the only two verbs with two letters
            // and ends with a vowel.
            // Their root transform as:
            // No change: de-di, de-miş, de-dir
            // Change : di-yecek di-yor de-r
            // "ye" has similar behavior but not the same. Such as "yi-yin" but for "de", "de-yin"
            // TODO: this can be achieved with less repetition.
            RootSurfaceIsAny diYiCondition = new RootSurfaceIsAny("di", "yi");
            RootSurfaceIsAny deYeCondition = new RootSurfaceIsAny("de", "ye");
            ICondition cMultiVerb = new Conditions.PreviousMorphemeIsAny(
                everSince, repeat, almost, hastily, stay, start).Not();

            vDeYeRoot_S
                .Add(vFut_S, "yece~k", diYiCondition)
                .Add(vFut_S, "yece!ğ", diYiCondition)
                .Add(vProgYor_S, "yor", diYiCondition)
                .Add(vAble_S, "yebil", diYiCondition)
                .Add(vAbleNeg_S, "ye", diYiCondition)
                .Add(vInf3_S, "yiş", new RootSurfaceIsAny("yi"))
                .Add(vFutPart_S, "yece~k", diYiCondition)
                .Add(vFutPart_S, "yece!ğ", diYiCondition)
                .Add(vPresPart_S, "yen", diYiCondition)
                .Add(vEverSince_S, "yegel", diYiCondition.And(cMultiVerb))
                .Add(vRepeat_S, "yedur", diYiCondition.And(cMultiVerb))
                .Add(vRepeat_S, "yegör", diYiCondition.And(cMultiVerb))
                .Add(vAlmost_S, "yeyaz", diYiCondition.And(cMultiVerb))
                .Add(vStart_S, "yekoy", diYiCondition.And(cMultiVerb))
                .Add(vSinceDoingSo_S, "yeli", diYiCondition)
                .Add(vByDoingSo_S, "yerek", diYiCondition)
                .Add(vFeelLike_S, "yesi", diYiCondition)
                .Add(vAfterDoing_S, "yip", diYiCondition)
                .Add(vWithoutBeingAbleToHaveDoneSo_S, "yemeden", diYiCondition)
                .Add(vOpt_S, "ye", diYiCondition);

            vDeYeRoot_S
                .Add(vCausTır_S, "dir", deYeCondition)
                .Add(vPass_S, "n", deYeCondition)
                .Add(vPass_S, "nil", deYeCondition)
                .Add(vPast_S, "di", deYeCondition)
                .Add(vNarr_S, "miş", deYeCondition)
                .Add(vAor_S, "r", deYeCondition)
                .Add(vNeg_S, "me", deYeCondition)
                .Add(vNegProg1_S, "m", deYeCondition)
                .Add(vProgMakta_S, "mekte", deYeCondition)
                .Add(vDesr_S, "se", deYeCondition)
                .Add(vInf1_S, "mek", deYeCondition)
                .Add(vInf2_S, "me", deYeCondition)
                .Add(vInf3_S, "yiş", new RootSurfaceIsAny("de"))
                .Add(vPastPart_S, "di~k", deYeCondition)
                .Add(vPastPart_S, "di!ğ", deYeCondition)
                .Add(vNarrPart_S, "miş", deYeCondition)
                .Add(vHastily_S, "yiver", diYiCondition.And(cMultiVerb))
                .Add(vAsLongAs_S, "dikçe")
                .Add(vWithoutHavingDoneSo_S, "meden")
                .Add(vWithoutHavingDoneSo_S, "meksizin")
                .Add(vNeces_S, "meli")
                .Add(vNotState_S, "mezli~k")
                .Add(vNotState_S, "mezli!ğ")
                .AddEmpty(vImp_S, new RootSurfaceIs("de"))
                .AddEmpty(vImpYemekYe_S, new RootSurfaceIs("ye"))
                .AddEmpty(vImpYemekYi_S, new RootSurfaceIs("yi"));

            // verb `yemek` has an exception case for some imperatives.
            vImpYemekYi_S
                .Add(vA2pl_ST, "yin")
                .Add(vA2pl_ST, "yiniz");
            vImpYemekYe_S
                .AddEmpty(vA2sg_ST)
                .Add(vA2sg_ST, "sene")
                .Add(vA3sg_ST, "sin")
                .Add(vA2pl_ST, "senize")
                .Add(vA3pl_ST, "sinler");

            // Optative (gel-e, gel-eyim gel-me-ye-yim)
            verbRoot_S.Add(vOpt_S, "+yA");
            vOpt_S
                .Add(vA1sg_ST, "yIm")
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "lIm")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr")
                .Add(vPastAfterTense_S, "ydI")
                .Add(vNarrAfterTense_S, "ymIş");

            // Desire (gel-se, gel-se-m gel-me-se-m)
            verbRoot_S.Add(vDesr_S, "sA");
            vDesr_S
                .Add(vA1sg_ST, "m")
                .Add(vA2sg_ST, "n")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "k")
                .Add(vA2pl_ST, "nIz")
                .Add(vA3pl_ST, "lAr")
                .Add(vPastAfterTense_S, "ydI")
                .Add(vNarrAfterTense_S, "ymIş");

            verbRoot_S.Add(vNeces_S, "mAlI");
            vNeces_S
                .Add(vA1sg_ST, "yIm")
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "yIz")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr")
                .Add(vPastAfterTense_S, "ydI")
                .Add(vCond_S, "ysA")
                .Add(vNarrAfterTense_S, "ymIş")
                .Add(vCopBeforeA3pl_S, "dIr")
                .Add(vWhile_S, "yken");

            // A3pl exception case.
            // A3pl can appear before or after some tense suffixes.
            // "yapar-lar-dı" - "yapar-dı-lar"
            // For preventing "yapar-dı-lar-dı", conditions are added.
            ICondition previousNotPastNarrCond = new PreviousStateIsAny(
                vPastAfterTense_S, vNarrAfterTense_S, vCond_S).Not();
            vA3pl_ST.Add(vPastAfterTense_ST, "dI", previousNotPastNarrCond);
            vA3pl_ST.Add(vNarrAfterTense_ST, "mIş", previousNotPastNarrCond);
            vA3pl_ST.Add(vCond_ST, "sA", previousNotPastNarrCond);

            ICondition a3plCopWhile =
                new PreviousMorphemeIsAny(prog1, prog2, neces, fut, narr, aor);
            vA3pl_ST.Add(vCop_ST, "dIr", a3plCopWhile);
            vA3pl_ST.Add(vWhile_S, "ken", a3plCopWhile);

            ICondition a3sgCopWhile =
                new PreviousMorphemeIsAny(prog1, prog2, neces, fut, narr, aor);
            vA1sg_ST.Add(vCop_ST, "dIr", a3sgCopWhile);
            vA2sg_ST.Add(vCop_ST, "dIr", a3sgCopWhile);
            vA3sg_ST.Add(vCop_ST, ">dIr", a3sgCopWhile);
            vA1pl_ST.Add(vCop_ST, "dIr", a3sgCopWhile);
            vA2pl_ST.Add(vCop_ST, "dIr", a3sgCopWhile);

            vCopBeforeA3pl_S.Add(vA3pl_ST, "lAr");

            // Allow Past+A2pl+Cond  Past+A2sg+Cond (geldinse, geldinizse)
            ICondition previousPast = new Conditions.PreviousMorphemeIs(past)
                .AndNot(new Conditions.ContainsMorpheme(cond, desr));
            vA2pl_ST.Add(vCondAfterPerson_ST, "sA", previousPast);
            vA2sg_ST.Add(vCondAfterPerson_ST, "sA", previousPast);
            vA1sg_ST.Add(vCondAfterPerson_ST, "sA", previousPast);
            vA1pl_ST.Add(vCondAfterPerson_ST, "sA", previousPast);

            verbRoot_S.Add(vEverSince_S, "+yAgel", cMultiVerb);
            verbRoot_S.Add(vRepeat_S, "+yAdur", cMultiVerb);
            verbRoot_S.Add(vRepeat_S, "+yAgör", cMultiVerb);
            verbRoot_S.Add(vAlmost_S, "+yAyaz", cMultiVerb);
            verbRoot_S.Add(vHastily_S, "+yIver", cMultiVerb);
            verbRoot_S.Add(vStay_S, "+yAkal", cMultiVerb);
            verbRoot_S.Add(vStart_S, "+yAkoy", cMultiVerb);

            vEverSince_S.AddEmpty(verbRoot_S);
            vRepeat_S.AddEmpty(verbRoot_S);
            vAlmost_S.AddEmpty(verbRoot_S);
            vHastily_S.AddEmpty(verbRoot_S);
            vStay_S.AddEmpty(verbRoot_S);
            vStart_S.AddEmpty(verbRoot_S);

            vA3sg_ST.Add(vAsIf_S, ">cAsInA", new Conditions.PreviousMorphemeIsAny(aor, narr));

            verbRoot_S.Add(vWhen_S, "+yIncA");
            verbRoot_S.Add(vSinceDoingSo_S, "+yAlI");
            verbRoot_S.Add(vByDoingSo_S, "+yArAk");
            verbRoot_S.Add(vAdamantly_S, "+yAsIyA");
            verbRoot_S.Add(vAfterDoing_S, "+yIp");
            verbRoot_S.Add(vWithoutBeingAbleToHaveDoneSo_S, "+yAmAdAn");
            verbRoot_S.Add(vAsLongAs_S, ">dIkçA");
            verbRoot_S.Add(vWithoutHavingDoneSo_S, "mAdAn");
            verbRoot_S.Add(vWithoutHavingDoneSo_S, "mAksIzIn");

            vAsIf_S.AddEmpty(advRoot_ST);
            vSinceDoingSo_S.AddEmpty(advRoot_ST);
            vByDoingSo_S.AddEmpty(advRoot_ST);
            vAdamantly_S.AddEmpty(advRoot_ST);
            vAfterDoing_S.AddEmpty(advRoot_ST);
            vWithoutBeingAbleToHaveDoneSo_S.AddEmpty(advRoot_ST);
            vAsLongAs_S.AddEmpty(advRoot_ST);
            vWithoutHavingDoneSo_S.AddEmpty(advRoot_ST);
            vWhile_S.AddEmpty(advRoot_ST);
            vWhen_S.AddEmpty(advNounRoot_ST);
        }

        //-------- Question (mi) -----------------------------------------------

        MorphemeState qPresent_S = MorphemeState.NonTerminal("qPresent_S", pres);
        MorphemeState qPast_S = MorphemeState.NonTerminal("qPast_S", past);
        MorphemeState qNarr_S = MorphemeState.NonTerminal("qNarr_S", narr);
        MorphemeState qA1sg_ST = MorphemeState.Terminal("qA1sg_ST", a1sg);
        MorphemeState qA2sg_ST = MorphemeState.Terminal("qA2sg_ST", a2sg);
        MorphemeState qA3sg_ST = MorphemeState.Terminal("qA3sg_ST", a3sg);
        MorphemeState qA1pl_ST = MorphemeState.Terminal("qA1pl_ST", a1pl);
        MorphemeState qA2pl_ST = MorphemeState.Terminal("qA2pl_ST", a2pl);
        MorphemeState qA3pl_ST = MorphemeState.Terminal("qA3pl_ST", a3pl);

        MorphemeState qCopBeforeA3pl_S = MorphemeState.NonTerminal("qCopBeforeA3pl_S", cop);
        MorphemeState qCop_ST = MorphemeState.Terminal("qCop_ST", cop);

        MorphemeState questionRoot_S = MorphemeState.Builder("questionRoot_S", ques).PosRoot().Build();

        private void ConnectQuestion()
        {
            //mı
            questionRoot_S.AddEmpty(qPresent_S);
            // mıydı
            questionRoot_S.Add(qPast_S, "ydI");
            // mıymış
            questionRoot_S.Add(qNarr_S, "ymIş");

            // mıyım
            qPresent_S.Add(qA1sg_ST, "yIm");
            // mısın
            qPresent_S.Add(qA2sg_ST, "sIn");
            // mı
            qPresent_S.AddEmpty(qA3sg_ST);

            // mıydım
            qPast_S.Add(qA1sg_ST, "m");
            // mıymışım
            qNarr_S.Add(qA1sg_ST, "Im");

            //mıydın
            qPast_S.Add(qA2sg_ST, "n");
            //mıymışsın
            qNarr_S.Add(qA2sg_ST, "sIn");

            //mıydık
            qPast_S.Add(qA1pl_ST, "k");
            //mıymışız
            qNarr_S.Add(qA1pl_ST, "Iz");
            // mıyız
            qPresent_S.Add(qA1pl_ST, "+yIz");

            // mıydınız
            qPast_S.Add(qA2pl_ST, "InIz");
            // mıymışsınız
            qNarr_S.Add(qA2pl_ST, "sInIz");
            // mısınız
            qPresent_S.Add(qA2pl_ST, "sInIz");

            // mıydılar
            qPast_S.Add(qA3pl_ST, "lAr");
            // mıymışlar
            qNarr_S.Add(qA3pl_ST, "lAr");

            // mıydı
            qPast_S.AddEmpty(qA3sg_ST);
            // mıymış
            qNarr_S.AddEmpty(qA3sg_ST);

            // for not allowing "mı-ydı-m-dır"
            ICondition rejectNoCopula = new CurrentGroupContainsAny(qPast_S).Not();

            // mıyımdır
            qA1sg_ST.Add(qCop_ST, "dIr", rejectNoCopula);
            // mısındır
            qA2sg_ST.Add(qCop_ST, "dIr", rejectNoCopula);
            // mıdır
            qA3sg_ST.Add(qCop_ST, ">dIr", rejectNoCopula);
            // mıyızdır
            qA1pl_ST.Add(qCop_ST, "dIr", rejectNoCopula);
            // mısınızdır
            qA2pl_ST.Add(qCop_ST, "dIr", rejectNoCopula);

            // Copula can come before A3pl.
            qPresent_S.Add(pvCopBeforeA3pl_S, "dIr");
            qCopBeforeA3pl_S.Add(qA3pl_ST, "lAr");
        }

        //-------- Verb `imek` -----------------------------------------------

        public MorphemeState imekRoot_S = MorphemeState.Builder("imekRoot_S", verb).PosRoot().Build();

        MorphemeState imekPast_S = MorphemeState.NonTerminal("imekPast_S", past);
        MorphemeState imekNarr_S = MorphemeState.NonTerminal("imekNarr_S", narr);

        MorphemeState imekCond_S = MorphemeState.NonTerminal("imekCond_S", cond);

        MorphemeState imekA1sg_ST = MorphemeState.Terminal("imekA1sg_ST", a1sg);
        MorphemeState imekA2sg_ST = MorphemeState.Terminal("imekA2sg_ST", a2sg);
        MorphemeState imekA3sg_ST = MorphemeState.Terminal("imekA3sg_ST", a3sg);
        MorphemeState imekA1pl_ST = MorphemeState.Terminal("imekA1pl_ST", a1pl);
        MorphemeState imekA2pl_ST = MorphemeState.Terminal("imekA2pl_ST", a2pl);
        MorphemeState imekA3pl_ST = MorphemeState.Terminal("imekA3pl_ST", a3pl);

        MorphemeState imekCop_ST = MorphemeState.Terminal("qCop_ST", cop);

        private void ConnectImek()
        {
            // idi
            imekRoot_S.Add(imekPast_S, "di");
            // imiş
            imekRoot_S.Add(imekNarr_S, "miş");
            // ise
            imekRoot_S.Add(imekCond_S, "se");

            // idim, idin, idi, idik, idiniz, idiler
            imekPast_S.Add(imekA1sg_ST, "m");
            imekPast_S.Add(imekA2sg_ST, "n");
            imekPast_S.AddEmpty(imekA3sg_ST);
            imekPast_S.Add(imekA1pl_ST, "k");
            imekPast_S.Add(imekA2pl_ST, "niz");
            imekPast_S.Add(imekA3pl_ST, "ler");

            // imişim, imişsin, imiş, imişiz, imişsiniz, imişler
            imekNarr_S.Add(imekA1sg_ST, "im");
            imekNarr_S.Add(imekA2sg_ST, "sin");
            imekNarr_S.AddEmpty(imekA3sg_ST);
            imekNarr_S.Add(imekA1pl_ST, "iz");
            imekNarr_S.Add(imekA2pl_ST, "siniz");
            imekNarr_S.Add(imekA3pl_ST, "ler");

            imekPast_S.Add(imekCond_S, "yse");
            imekNarr_S.Add(imekCond_S, "se");

            imekCond_S.Add(imekA1sg_ST, "m");
            imekCond_S.Add(imekA2sg_ST, "n");
            imekCond_S.AddEmpty(imekA3sg_ST);
            imekCond_S.Add(imekA1pl_ST, "k");
            imekCond_S.Add(imekA2pl_ST, "niz");
            imekCond_S.Add(imekA3pl_ST, "ler");

            // for not allowing "i-di-m-dir"
            ICondition rejectNoCopula = new CurrentGroupContainsAny(imekPast_S).Not();

            // imişimdir, imişsindir etc.
            imekA1sg_ST.Add(imekCop_ST, "dir", rejectNoCopula);
            imekA2sg_ST.Add(imekCop_ST, "dir", rejectNoCopula);
            imekA3sg_ST.Add(imekCop_ST, "tir", rejectNoCopula);
            imekA1pl_ST.Add(imekCop_ST, "dir", rejectNoCopula);
            imekA2pl_ST.Add(imekCop_ST, "dir", rejectNoCopula);
            imekA3pl_ST.Add(imekCop_ST, "dir", rejectNoCopula);
        }

        private void HandlePostProcessingConnections()
        {

            // Passive has an exception for some verbs like `kavurmak` or `savurmak`.
            // add passive state connection to modified root `kavr` etc.
            verbLastVowelDropModRoot_S.Add(vPass_S, "Il");
            // for not allowing `kavur-ul` add all verb connections to
            // unmodified `kavur` root and remove only the passive.
            verbLastVowelDropUnmodRoot_S.CopyOutgoingTransitionsFrom(verbRoot_S);
            verbLastVowelDropUnmodRoot_S.RemoveTransitionsTo(pass);
        }

        //--------------------------------------------------------

        Dictionary<string, MorphemeState> itemRootStateMap = new Dictionary<string, MorphemeState>();

        void MapSpecialItemsToRootStates()
        {
            itemRootStateMap.Add("değil_Verb", nVerbDegil_S);
            itemRootStateMap.Add("imek_Verb", imekRoot_S);
            itemRootStateMap.Add("su_Noun", nounSuRoot_S);
            itemRootStateMap.Add("akarsu_Noun", nounSuRoot_S);
            itemRootStateMap.Add("öyle_Adv", advForVerbDeriv_ST);
            itemRootStateMap.Add("böyle_Adv", advForVerbDeriv_ST);
            itemRootStateMap.Add("şöyle_Adv", advForVerbDeriv_ST);
        }

        public MorphemeState GetRootState(
            DictionaryItem item,
            AttributeSet<PhoneticAttribute> phoneticAttributes)
        {

            MorphemeState root = itemRootStateMap.GetValueOrDefault(item.id);
            if (root != null)
            {
                return root;
            }

            // Verbs like "aramak" drops their last vowel when  connected to "Iyor" Progressive suffix.
            // those modified roots are connected to a separate root state called verbRoot_VowelDrop_S.
            if (phoneticAttributes.Contains(PhoneticAttribute.LastLetterDropped))
            {
                return verbRoot_VowelDrop_S;
            }

            if (item.HasAttribute(RootAttribute.Reciprocal))
            {
                return vImplicitRecipRoot_S;
            }

            if (item.HasAttribute(RootAttribute.Reflexive))
            {
                return vImplicitReflexRoot_S;
            }

            switch (item.primaryPos.LongForm)
            {
                case PrimaryPos.Constants.Noun:

                    switch (item.secondaryPos.LongForm)
                    {
                        case SecondaryPos.Constants.ProperNoun:
                            return nounProper_S;
                        case SecondaryPos.Constants.Abbreviation:
                            return nounAbbrv_S;
                        case SecondaryPos.Constants.Email:
                        case SecondaryPos.Constants.Url:
                        case SecondaryPos.Constants.HashTag:
                        case SecondaryPos.Constants.Mention:
                            return nounProper_S;
                        case SecondaryPos.Constants.Emoticon:
                        case SecondaryPos.Constants.RomanNumeral:
                            return nounNoSuffix_S;
                        default:
                            break;
                    }

                    if (item.HasAttribute(RootAttribute.CompoundP3sgRoot))
                    {
                        return nounCompoundRoot_S;
                    }
                    else
                    {
                        return noun_S;
                    }

                case PrimaryPos.Constants.Adjective:
                    return adjectiveRoot_ST;
                case PrimaryPos.Constants.Pronoun:
                    switch (item.secondaryPos.LongForm)
                    {
                        case SecondaryPos.Constants.PersonalPron:
                            return pronPers_S;
                        case SecondaryPos.Constants.DemonstrativePron:
                            return pronDemons_S;
                        case SecondaryPos.Constants.QuantitivePron:
                            return pronQuant_S;
                        case SecondaryPos.Constants.QuestionPron:
                            return pronQues_S;
                        case SecondaryPos.Constants.ReflexivePron:
                            return pronReflex_S;
                        default:
                            return pronQuant_S;
                            //throw new IllegalStateException("Cannot find root for Pronoun " + dictionaryItem);
                    }
                case PrimaryPos.Constants.Adverb:
                    return advRoot_ST;
                case PrimaryPos.Constants.Conjunction:
                    return conjRoot_ST;
                case PrimaryPos.Constants.Question:
                    return questionRoot_S;
                case PrimaryPos.Constants.Interjection:
                    return interjRoot_ST;
                case PrimaryPos.Constants.Verb:
                    return verbRoot_S;
                case PrimaryPos.Constants.Punctuation:
                    return puncRoot_ST;
                case PrimaryPos.Constants.Determiner:
                    return detRoot_ST;
                case PrimaryPos.Constants.PostPositive:
                    return postpRoot_ST;
                case PrimaryPos.Constants.Numeral:
                    return numeralRoot_ST;
                case PrimaryPos.Constants.Duplicator:
                    return dupRoot_ST;
                default:
                    return noun_S;
            }
        }
    }
}
