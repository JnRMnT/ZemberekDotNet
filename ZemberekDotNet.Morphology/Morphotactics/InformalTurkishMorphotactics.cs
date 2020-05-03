using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using static ZemberekDotNet.Morphology.Morphotactics.Conditions;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    public class InformalTurkishMorphotactics : TurkishMorphotactics
    {
        public InformalTurkishMorphotactics(RootLexicon lexicon)
        {
            this.lexicon = lexicon;
            MakeGraph();
            AddGraph();
            this.stemTransitions = new StemTransitionsMapBased(lexicon, this);
        }

        public static readonly Morpheme a1plInformal = AddMorpheme(
            Morpheme.Builder("A1pl_Informal", "A1pl_Informal")
          .Informal().SetMappedMorpheme(a1pl).Build());

        public static readonly Morpheme a1sgInformal = AddMorpheme(
            Morpheme.Builder("A1sg_Informal", "A1sg_Informal")
                .Informal().SetMappedMorpheme(a1sg).Build());

        public static readonly Morpheme prog1Informal = AddMorpheme(
            Morpheme.Builder("Prog1_Informal", "Prog1_Informal")
                .Informal().SetMappedMorpheme(prog1).Build());

        public static readonly Morpheme futInformal = AddMorpheme(
            Morpheme.Builder("Fut_Informal", "Fut_Informal")
                .Informal().SetMappedMorpheme(fut).Build());

        // TODO: not used yet.
        public static readonly Morpheme quesSuffixInformal = AddMorpheme(
            Morpheme.Builder("QuesSuffix_Informal", "QuesSuffix_Informal")
                .Informal().SetMappedMorpheme(ques).Build());

        public static readonly Morpheme negInformal = AddMorpheme(
            Morpheme.Builder("Neg_Informal", "Neg_Informal")
                .Informal().SetMappedMorpheme(neg).Build());

        public static readonly Morpheme unableInformal = AddMorpheme(
            Morpheme.Builder("Unable_Informal", "Unable_Informal")
                .Informal().SetMappedMorpheme(unable).Build());

        public static readonly Morpheme optInformal = AddMorpheme(
            Morpheme.Builder("Opt_Informal", "Opt_Informal")
                .Informal().SetMappedMorpheme(opt).Build());

        MorphemeState vA1pl_ST_Inf = MorphemeState.Terminal("vA1pl_ST_Inf", a1plInformal);
        MorphemeState vA1sg_ST_Inf = MorphemeState.Terminal("vA1sg_ST_Inf", a1sgInformal);
        MorphemeState vProgYor_S_Inf = MorphemeState.NonTerminal("vProgYor_S_Inf", prog1Informal);

        MorphemeState vFut_S_Inf = MorphemeState.NonTerminal("vFut_S_Inf", futInformal);
        MorphemeState vFut_S_Inf2 = MorphemeState.NonTerminal("vFut_S_Inf2", futInformal);
        MorphemeState vFut_S_Inf3 = MorphemeState.NonTerminal("vFut_S_Inf3", futInformal);

        MorphemeState vQues_S_Inf = MorphemeState.NonTerminal("vQues_S_Inf", quesSuffixInformal);

        MorphemeState vNeg_S_Inf = MorphemeState.NonTerminal("vNeg_S_Inf", negInformal);
        MorphemeState vUnable_S_Inf = MorphemeState.NonTerminal("vUnable_S_Inf", unableInformal);

        MorphemeState vOpt_S_Inf = MorphemeState.NonTerminal("vOpt_S_Inf", optInformal);
        MorphemeState vOpt_S_Empty_Inf = MorphemeState.NonTerminal("vOpt_S_Empty_Inf", optInformal);
        MorphemeState vOpt_S_Empty_Inf2 = MorphemeState.NonTerminal("vOpt_S_Empty_Inf2", optInformal);

        public void AddGraph()
        {
            // yap-ıyo
            verbRoot_S.Add(vProgYor_S_Inf, "Iyo", Conditions.NotHave(PhoneticAttribute.LastLetterVowel));
            verbRoot_VowelDrop_S.Add(vProgYor_S_Inf, "Iyo");
            vProgYor_S_Inf
                .Add(vA1sg_ST, "m")
                .Add(vA2sg_ST, "sun")
                .Add(vA2sg_ST, "n")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "z")
                .Add(vA2pl_ST, "sunuz")
                .Add(vA2pl_ST, "nuz")
                .Add(vA3pl_ST, "lar")
                .Add(vCond_S, "sa")
                .Add(vPastAfterTense_S, "du")
                .Add(vNarrAfterTense_S, "muş")
                .Add(vCopBeforeA3pl_S, "dur")
                .Add(vWhile_S, "ken");

            vNegProg1_S.Add(vProgYor_S_Inf, "Iyo");
            vUnableProg1_S.Add(vProgYor_S_Inf, "Iyo");

            RootSurfaceIsAny diYiCondition = new Conditions.RootSurfaceIsAny("di", "yi");
            vDeYeRoot_S
                .Add(vProgYor_S_Inf, "yo", diYiCondition);

            // yap-a-k

            vOpt_S
                .Add(vA1pl_ST_Inf, "k");

            // Future tense deformation
            // yap-ıca-m yap-aca-m yap-ca-m

            verbRoot_S.Add(vNeg_S_Inf, "mI");
            verbRoot_S.Add(vUnable_S_Inf, "+yAmI");

            verbRoot_S
                .Add(vFut_S_Inf, "+ycA~k")  // yap-cak-sın oku-ycak
                .Add(vFut_S_Inf, "+ycA!ğ")  // yap-cağ-ım
                .Add(vFut_S_Inf2, "+ycA")   // yap-ca-m oku-yca-m
                .Add(vFut_S_Inf2, "+yIcA")  // yap-ıca-m oku-yuca-m
                .Add(vFut_S_Inf2, "+yAcA");  // yap-aca-m oku-yaca-m

            vNeg_S_Inf
                .Add(vFut_S, "yAcA~k")  // yap-mı-yacak-sın
                .Add(vFut_S, "yAcA!ğ")  // yap-mı-yacağ-ım
                .Add(vFut_S_Inf, "ycA~k")   // yap-mı-ycağ-ım
                .Add(vFut_S_Inf, "ycA!ğ")   // yap-mı-ycak-sın
                .Add(vFut_S_Inf2, "ycA");   // yap-mı-ycak

            vUnable_S_Inf
                .Add(vFut_S, "yAcA~k")  // yap-amı-yacak-sın
                .Add(vFut_S, "yAcA!ğ")  // yap-amı-yacağ-ım
                .Add(vFut_S_Inf, "ycA~k")   // yap-amı-ycağ-ım
                .Add(vFut_S_Inf, "ycA!ğ")   // yap-amı-ycak-sın
                .Add(vFut_S_Inf2, "ycA");   // yap-amı-ycak

            vNeg_S
                .Add(vFut_S_Inf, "yAcA")   // yap-ma-yaca-m
                .Add(vFut_S_Inf, "yAcAk");   // yap-ma-yacak-(A3sg|A3pl)

            vUnable_S
                .Add(vFut_S_Inf, "yAcA")   // yap-ama-yaca-m
                .Add(vFut_S_Inf, "yAcAk");   // yap-ama-yacak-(A3sg|A3pl)

            vFut_S_Inf
                .Add(vA1sg_ST, "+Im")
                .Add(vA2sg_ST, "sIn")
                .AddEmpty(vA3sg_ST)
                .Add(vA1pl_ST, "Iz")
                .Add(vA2pl_ST, "sInIz")
                .Add(vA3pl_ST, "lAr");

            vFut_S_Inf2
                .Add(vA1sg_ST, "m") // yap-ca-m
                .Add(vA2sg_ST, "n") // yap-ca-n
                .Add(vA1pl_ST, "z") // yap-ca-z
                .Add(vA1pl_ST, "nIz"); // yap-ca-nız

            vFut_S_Inf.Add(vCond_S, "sA");
            vFut_S_Inf.Add(vPastAfterTense_S, "tI");
            vFut_S_Inf.Add(vNarrAfterTense_S, "mIş");
            vFut_S_Inf.Add(vCopBeforeA3pl_S, "tIr");
            vFut_S_Inf.Add(vWhile_S, "ken");

            // Handling of yapıyim, okuyim, bakıyim. TODO: not yet finished

            verbRoot_S.Add(vOpt_S_Inf, "I", Conditions.Has(PhoneticAttribute.LastLetterConsonant));

            // for handling "arıyim, okuyim"
            verbRoot_VowelDrop_S.Add(vOpt_S_Inf, "I");

            verbRoot_S.AddEmpty(vOpt_S_Empty_Inf, Conditions.Has(PhoneticAttribute.LastLetterVowel));

            vOpt_S_Inf.Add(vA1sg_ST_Inf, "+yIm");
            vOpt_S_Inf.Add(vA1sg_ST_Inf, "+yim");
            vOpt_S_Empty_Inf.Add(vA1sg_ST_Inf, "+yim");

            // handling of 'babacım, kuzucum' or 'babacıım, kuzucuum'

            // handling of 'abim-gil' 'Ahmet'gil'

            // yap-tı-mı Connected Question word.
            // After past and narrative, a person suffix is required.
            // yap-tı-m-mı
            // After progressive, future, question can come before.
            // yap-ıyor-mu-yum yap-acak-mı-yız

        }
    }
}
