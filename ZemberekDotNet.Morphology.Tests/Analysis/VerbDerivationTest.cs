using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class VerbDerivationTest : AnalyzerTestBase
    {
        [TestMethod]
        public void CausativeTest()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectSingle("okut",
                MatchesTailLex("Verb + Caus + Verb + Imp + A2sg"));
            tester.ExpectSingle("okuttur",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Imp + A2sg"));
            tester.ExpectSingle("okutturt",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Caus + Verb + Imp + A2sg"));
            tester.ExpectSingle("okutul",
                MatchesTailLex("Verb + Caus + Verb + Pass + Verb + Imp + A2sg"));

            tester.ExpectFail(
                "okutt",
                "okuturtur"
            );

            // "okutur" should not have a causative analysis
            tester.ExpectFalse("okutur", MatchesTailLex("Verb + Caus + Verb + Imp + A2sg"));

            tester = GetTester("semirmek");
            tester.ExpectSingle("semirt",
                MatchesTailLex("Verb + Caus + Verb + Imp + A2sg"));
            tester.ExpectSingle("semirttir",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Imp + A2sg"));
            tester.ExpectSingle("semirttirt",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Caus + Verb + Imp + A2sg"));

            tester.ExpectFail(
                "semirtt",
                "semirtirtir"
            );
        }

        [TestMethod]
        public void Infinitive1()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectSingle("okumak",
                MatchesTailLex("Verb + Inf1 + Noun + A3sg"));
            tester.ExpectSingle("okumamak",
                MatchesTailLex("Verb + Neg + Inf1 + Noun + A3sg"));
            tester.ExpectSingle("okutmak",
                MatchesTailLex("Verb + Caus + Verb + Inf1 + Noun + A3sg"));
            tester.ExpectSingle("okutmaktan",
                MatchesTailLex("Verb + Caus + Verb + Inf1 + Noun + A3sg + Abl"));

            tester.ExpectFail(
                "okumaka",
                "okumaklar",
                "okumağ",
                "okumağı"
            );
        }

        [TestMethod]
        public void Infinitive2()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okuma",
                MatchesTailLex("Verb + Inf2 + Noun + A3sg"));
            tester.ExpectAny("okumama",
                MatchesTailLex("Verb + Neg + Inf2 + Noun + A3sg"));
            tester.ExpectAny("okumama",
                MatchesTailLex("Verb + Inf2 + Noun + A3sg + P1sg + Dat"));
            tester.ExpectAny("okutma",
                MatchesTailLex("Verb + Caus + Verb + Inf2 + Noun + A3sg"));
            tester.ExpectAny("okutma",
                MatchesTailLex("Verb + Caus + Verb + Neg + Imp + A2sg"));
            tester.ExpectAny("okutmama",
                MatchesTailLex("Verb + Caus + Verb + Inf2 + Noun + A3sg + P1sg + Dat"));
            tester.ExpectAny("okuması",
                MatchesTailLex("Verb + Inf2 + Noun + A3sg + P3sg"));
            tester.ExpectAny("okutması",
                MatchesTailLex("Verb + Caus + Verb + Inf2 + Noun + A3sg + P3sg"));
            tester.ExpectAny("okutulması",
                MatchesTailLex("Verb + Caus + Verb + Pass + Verb + Inf2 + Noun + A3sg + P3sg"));

            tester.ExpectAny("okutmamadan",
                MatchesTailLex("Verb + Caus + Verb + Neg + Inf2 + Noun + A3sg + Abl"));
        }


        [TestMethod]
        public void Infinitive3()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectSingle("okuyuş",
                MatchesTailLex("Verb + Inf3 + Noun + A3sg"));
            tester.ExpectSingle("okumayış",
                MatchesTailLex("Verb + Neg + Inf3 + Noun + A3sg"));
            tester.ExpectSingle("okutmayış",
                MatchesTailLex("Verb + Caus + Verb + Neg + Inf3 + Noun + A3sg"));
            tester.ExpectSingle("okutmayıştan",
                MatchesTailLex("Verb + Caus + Verb + Neg + Inf3 + Noun + A3sg + Abl"));
        }

        [TestMethod]
        public void PastPartTest()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okuduk",
                MatchesTailLex("Verb + PastPart + Noun + A3sg"));
            tester.ExpectAny("okuduk",
                MatchesTailLex("Verb + PastPart + Adj"));
            tester.ExpectAny("okumadık",
                MatchesTailLex("Verb + Neg + PastPart + Noun + A3sg"));
            tester.ExpectAny("okumadık",
                MatchesTailLex("Verb + Neg + PastPart + Adj"));
            tester.ExpectAny("okumadığım",
                MatchesTailLex("Verb + Neg + PastPart + Noun + A3sg + P1sg"));
            tester.ExpectAny("okumadığım",
                MatchesTailLex("Verb + Neg + PastPart + Adj + P1sg"));
            tester.ExpectAny("okuttuğumuz",
                MatchesTailLex("Verb + Caus + Verb + PastPart + Noun + A3sg + P1pl"));
            tester.ExpectAny("okuttuğumuzdu",
                MatchesTailLex(
                    "Verb + Caus + Verb + PastPart + Noun + A3sg + P1pl + Zero + Verb + Past + A3sg"));

            // false positive test
            tester.ExpectFalse("okuduğum",
                MatchesTailLex("Verb + PastPart + Noun + A3sg + Zero + Verb + Pres + A1sg"));
        }

        [TestMethod]
        public void FuturePartTest()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okuyacak",
                MatchesTailLex("Verb + FutPart + Adj"));
            tester.ExpectAny("okumayacak",
                MatchesTailLex("Verb + Neg + FutPart + Adj"));
            tester.ExpectAny("okumayacağım",
                MatchesTailLex("Verb + Neg + FutPart + Noun + A3sg + P1sg"));
            tester.ExpectAny("okumayacağım",
                MatchesTailLex("Verb + Neg + FutPart + Adj + P1sg"));
            tester.ExpectAny("okutmayacağımız",
                MatchesTailLex("Verb + Caus + Verb + Neg + FutPart + Noun + A3sg + P1pl"));
            tester.ExpectAny("okutmayacağımızdı",
                MatchesTailLex(
                    "Verb + Caus + Verb + Neg + FutPart + Noun + A3sg + P1pl + Zero + Verb + Past + A3sg"));
            tester.ExpectAny("okuyacaklarca",
                MatchesTailLex("Verb + FutPart + Noun + A3pl + Equ"));

            // false positive test
            tester.ExpectFalse("okuyacağım",
                MatchesTailLex("Verb + FutPart + Noun + A3sg + Zero + Verb + Pres + A1sg"));
            // Oflazer does not allow Noun+A3sg+Pnon+Nom
            tester.ExpectFalse("okuyacak",
                MatchesTailLex("Verb + FutPart + Noun + A3sg"));
            tester.ExpectFalse("okumayacak",
                MatchesTailLex("Verb + Neg + FutPart + Noun + A3sg"));

            tester = GetTester("cam");
            tester.ExpectAny("camlaşmayabileceği",
                MatchesTailLex("FutPart + Adj + P3sg"));
        }

        [TestMethod]
        public void PresPartTest()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okuyan",
                MatchesTailLex("Verb + PresPart + Adj"));
            tester.ExpectAny("okumayan",
                MatchesTailLex("Verb + Neg + PresPart + Adj"));
            tester.ExpectAny("okumayana",
                MatchesTailLex("Verb + Neg + PresPart + Noun + A3sg + Dat"));
            tester.ExpectAny("okutmayanda",
                MatchesTailLex("Verb + Caus + Verb + Neg + PresPart + Noun + A3sg + Loc"));
            tester.ExpectAny("okutmayanlarca",
                MatchesTailLex("Verb + Caus + Verb + Neg + PresPart + Noun + A3pl + Equ"));

        }

        [TestMethod]
        public void NarrPartTest()
        {
            AnalysisTester tester = GetTester("okumak");
            tester.ExpectAny("okumuş",
                MatchesTailLex("Verb + NarrPart + Adj"));
            tester.ExpectAny("okumuşa",
                MatchesTailLex("Verb + NarrPart + Adj + Zero + Noun + A3sg + Dat"));
        }

        [TestMethod]
        public void AorPartNegativeTest()
        {
            AnalysisTester tester = GetTester("okumak");
            tester.ExpectAny("okumaz",
                MatchesTailLex("Verb + Neg + AorPart + Adj"));
        }

        [TestMethod]
        public void AorPartTest()
        {
            AnalysisTester tester = GetTester("okumak");
            tester.ExpectAny("okur",
                MatchesTailLex("Verb + AorPart + Adj"));
            tester.ExpectAny("okurluk",
                MatchesTailLex("Verb + AorPart + Adj + Ness + Noun + A3sg"));
        }

        [TestMethod]
        public void MultiVerbtoVerbTest()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectSingle("okuyagel",
                MatchesTailLex("Verb + EverSince + Verb + Imp + A2sg"));
            tester.ExpectSingle("okuyadur",
                MatchesTailLex("Verb + Repeat + Verb + Imp + A2sg"));
            tester.ExpectSingle("okuyagör",
                MatchesTailLex("Verb + Repeat + Verb + Imp + A2sg"));
            tester.ExpectSingle("okuyayaz",
                MatchesTailLex("Verb + Almost + Verb + Imp + A2sg"));
            tester.ExpectSingle("okuyuver",
                MatchesTailLex("Verb + Hastily + Verb + Imp + A2sg"));
            tester.ExpectSingle("okuyakal",
                MatchesTailLex("Verb + Stay + Verb + Imp + A2sg"));
            tester.ExpectSingle("okuyakoy",
                MatchesTailLex("Verb + Start + Verb + Imp + A2sg"));
        }

        [TestMethod]
        public void AdverbDerivation()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectSingle("okurcasına",
                MatchesTailLex("Verb + Aor + A3sg + AsIf + Adv"));
            tester.ExpectSingle("okumazcasına",
                MatchesTailLex("Verb + Neg + Aor + A3sg + AsIf + Adv"));
            tester.ExpectAny("okumuşçasına",
                MatchesTailLex("Verb + Narr + A3sg + AsIf + Adv"));
            tester.ExpectSingle("okurmuşçasına",
                MatchesTailLex("Verb + Aor + Narr + A3sg + AsIf + Adv"));
            tester.ExpectSingle("okuyalı",
                MatchesTailLex("Verb + SinceDoingSo + Adv"));
            tester.ExpectSingle("okuyunca",
                MatchesTailLex("Verb + When + Adv"));
            tester.ExpectSingle("okumayınca",
                MatchesTailLex("Verb + Neg + When + Adv"));
            tester.ExpectSingle("okudukça",
                MatchesTailLex("Verb + AsLongAs + Adv"));
            tester.ExpectSingle("okuyarak",
                MatchesTailLex("Verb + ByDoingSo + Adv"));
            tester.ExpectAny("okuyasıya",
                MatchesTailLex("Verb + Adamantly + Adv"));
            tester.ExpectSingle("okuyup",
                MatchesTailLex("Verb + AfterDoingSo + Adv"));
            tester.ExpectAny("okumadan",
                MatchesTailLex("Verb + WithoutHavingDoneSo + Adv"));
            tester.ExpectSingle("okumaksızın",
                MatchesTailLex("Verb + WithoutHavingDoneSo + Adv"));
            tester.ExpectSingle("okuyamadan",
                MatchesTailLex("Verb + WithoutBeingAbleToHaveDoneSo + Adv"));
        }

        [TestMethod]
        public void WhileAdverbDerivation()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectSingle("okurken",
                MatchesTailLex("Verb + Aor + While + Adv"));
            tester.ExpectSingle("okurlarken",
                MatchesTailLex("Verb + Aor + A3pl + While + Adv"));
            tester.ExpectSingle("okumazken",
                MatchesTailLex("Verb + Neg + Aor + While + Adv"));
            tester.ExpectSingle("okuyorken",
                MatchesTailLex("Verb + Prog1 + While + Adv"));
            tester.ExpectAny("okumaktayken",
                MatchesTailLex("Verb + Prog2 + While + Adv"));
            tester.ExpectAny("okuyacakken",
                MatchesTailLex("Verb + Fut + While + Adv"));
            tester.ExpectAny("okumuşken",
                MatchesTailLex("Verb + Narr + While + Adv"));
            tester.ExpectSingle("okuyabilirken",
                MatchesTailLex("Verb + Able + Verb + Aor + While + Adv"));

            tester.ExpectFail(
                "okuduyken",
                "okurumken",
                "okudularken"
            );
        }

        [TestMethod]
        public void Agt()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okuyucu",
                MatchesTailLex("Verb + Agt + Adj"));
            tester.ExpectAny("okuyucu",
                MatchesTailLex("Verb + Agt + Noun + A3sg"));
            tester.ExpectAny("okutucu",
                MatchesTailLex("Verb + Caus + Verb + Agt + Adj"));

            tester.ExpectFail(
                "okuyucucu"
            );
        }

        [TestMethod]
        public void ActOf()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okumaca",
                MatchesTailLex("Verb + ActOf + Noun + A3sg"));
            tester.ExpectAny("okumamaca",
                MatchesTailLex("Verb + Neg + ActOf + Noun + A3sg"));
            tester.ExpectAny("okumacalar",
                MatchesTailLex("Verb + ActOf + Noun + A3pl"));

            tester.ExpectFail(
                "okumacam",
                "okumacaya"
            );
        }

        [TestMethod]
        public void FeelLikeTest()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okuyası",
                MatchesTailLex("Verb + FeelLike + Adj"));
            tester.ExpectAny("okumayası",
                MatchesTailLex("Verb + Neg + FeelLike + Adj"));
            tester.ExpectAny("okuyasım",
                MatchesTailLex("Verb + FeelLike + Noun + A3sg + P1sg"));
            tester.ExpectAny("okuyasıları",
                MatchesTailLex("Verb + FeelLike + Noun + A3pl + P3pl"));
        }

        [TestMethod]
        public void NotStateTest()
        {
            AnalysisTester tester = GetTester("okumak");

            tester.ExpectAny("okumazlık",
                MatchesTailLex("Verb + NotState + Noun + A3sg"));
            tester.ExpectAny("okumamazlık",
                MatchesTailLex("Verb + Neg + NotState + Noun + A3sg"));
            tester.ExpectAny("okumamazlığı",
                MatchesTailLex("Verb + Neg + NotState + Noun + A3sg + Acc"));
        }
    }
}
