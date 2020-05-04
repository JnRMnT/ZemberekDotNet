using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class NounDerivationTest : AnalyzerTestBase
    {
        [TestMethod]
        public void WithTest()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectSingle("meyveli",
                MatchesTailLex("A3sg + With + Adj"));

            tester = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            tester.ExpectAny("zeytinyağlı",
                MatchesTailLex("Noun + A3sg + With + Adj"));

        }

        [TestMethod]
        public void WithoutTest()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectSingle("meyvesiz",
                MatchesTailLex("A3sg + Without + Adj"));
            tester.ExpectSingle("meyvesizdi",
                MatchesTailLex("A3sg + Without + Adj + Zero + Verb + Past + A3sg"));

            tester.ExpectFail(
                "meyvemsiz",
                "meyvelersiz",
                "meyvedesiz",
                "meyvesizli",
                "meyvelisiz"
            );

            tester = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            tester.ExpectAny("zeytinyağsız",
                MatchesTailLex("Noun + A3sg + Without + Adj"));
        }

        [TestMethod]
        public void JustlikeTest()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectSingle("meyvemsi",
                MatchesTailLex("A3sg + JustLike + Adj"));
            tester = GetTester("odun");
            tester.ExpectSingle("odunsu",
                MatchesTailLex("A3sg + JustLike + Adj"));
            tester.ExpectSingle("odunumsu",
                MatchesTailLex("A3sg + JustLike + Adj"));
            tester = GetTester("kitap");
            tester.ExpectSingle("kitabımsı",
                MatchesTailLex("A3sg + JustLike + Adj"));

            tester = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            tester.ExpectAny("zeytinyağımsı",
                MatchesTailLex("Noun + A3sg + JustLike + Adj"));
            tester.ExpectAny("zeytinyağsı",
                MatchesTailLex("Noun + A3sg + JustLike + Adj"));
        }

        // check for
        // incorrect P1sg analysis for meyvemsi.
        // incorrect JustLike analysis for meyvesi.
        [TestMethod]
        public void JustLikeFalseTest()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectFalse("meyvemsi",
                MatchesTailLex("P1sg + JustLike + Adj"));
            tester.ExpectFalse("meyvesi",
                MatchesTailLex("A3sg + JustLike + Adj"));
        }

        [TestMethod]
        public void Incorrect1()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectFail("meyvelili");
            tester.ExpectFail("meyvelerli");
            tester.ExpectFail("meyvemli");
            tester.ExpectFail("meyveyeli");
            tester.ExpectFail("meyvelersi");
            tester.ExpectFail("meyveyemsi");
            tester.ExpectFail("meyvensi");
            tester = GetTester("armut");
            tester.ExpectFail("armudsu");
            tester.ExpectFail("armutumsu");
            tester.ExpectFail("armutlarımsı");
            tester.ExpectFail("armutlarsı");
        }

        [TestMethod]
        public void Rel1()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectSingle("meyvedeki",
                MatchesTailLex("Noun + A3sg + Loc + Rel + Adj"));
            tester.ExpectAny("meyvendeki",
                MatchesTailLex("Noun + A3sg + P2sg + Loc + Rel + Adj"));
            tester.ExpectAny("meyvelerdeki",
                MatchesTailLex("Noun + A3pl + Loc + Rel + Adj"));
            tester.ExpectSingle("meyvedekiydi",
                MatchesTailLex("Noun + A3sg + Loc + Rel + Adj + Zero + Verb + Past + A3sg"));

            tester.ExpectFail(
                "meyveki",
                "meyveyeki",
                "meyvedekideki",
                "meyvemki"
            );

            tester = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            tester.ExpectAny("zeytinyağındaki",
                MatchesTailLex("Noun + A3sg + Loc + Rel + Adj"));
        }

        [TestMethod]
        public void RelTime()
        {
            AnalysisTester tester = GetTester("dün [P:Time]");
            tester.ExpectSingle("dünkü",
                MatchesTailLex("Noun + A3sg + Rel + Adj"));
            tester.ExpectSingle("dünküydü",
                MatchesTailLex("Noun + A3sg + Rel + Adj + Zero + Verb + Past + A3sg"));
            tester = GetTester("akşam [P:Time]");
            tester.ExpectSingle("akşamki",
                MatchesTailLex("Noun + A3sg + Rel + Adj"));
            tester.ExpectSingle("akşamkiydi",
                MatchesTailLex("Noun + A3sg + Rel + Adj + Zero + Verb + Past + A3sg"));

            // Unlike Oflazer, we allow thıs:
            tester.ExpectSingle("akşamdaki",
                MatchesTailLex("Noun + A3sg + Loc + Rel + Adj"));
            tester.ExpectAny("akşamındaki",
                MatchesTailLex("Noun + A3sg + P2sg + Loc + Rel + Adj"));

            tester = GetTester("ileri");
            tester.ExpectSingle("ileriki",
                MatchesTailLex("Noun + A3sg + Rel + Adj"));
            tester.ExpectSingle("ilerikiydi",
                MatchesTailLex("Noun + A3sg + Rel + Adj + Zero + Verb + Past + A3sg"));

            tester.ExpectFail(
                "dünki",
                "akşamkü",
                "akşamkı"
            );
        }

        [TestMethod]
        public void Dim1()
        {

            AnalysisTester tester = GetTester("kitap");
            tester.ExpectSingle("kitapçık",
                MatchesTailLex("Noun + A3sg + Dim + Noun + A3sg"));
            tester.ExpectSingle("kitapçıkta",
                MatchesTailLex("Noun + A3sg + Dim + Noun + A3sg + Loc"));
            tester.ExpectSingle("kitapçığa",
                MatchesTailLex("Noun + A3sg + Dim + Noun + A3sg + Dat"));

            tester.ExpectFail(
                "kitaplarcık", "kitapçıklarcık", "kitapçığ", "kitapcık", "kitabımcık",
                "kitaptacık", "kitapçıkçık", "kitabcığ", "kitabçığ", "kitabçık", "kitapçığ"
            );

            tester = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            tester.ExpectAny("zeytinyağcık",
                MatchesTailLex("Noun + A3sg + Dim + Noun + A3sg"));
        }


        [TestMethod]
        public void Noun2NounIncorrect_1()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            ExpectFail(analyzer,
                "kitaplarcık", "kitapçıklarcık", "kitapçığ", "kitapcık", "kitabımcık",
                "kitaptacık", "kitapçıkçık", "kitabcığ", "kitabçığ", "kitabçık", "kitapçığ"
            );
        }

        [TestMethod]
        public void NessTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectAny("elmalık",
                MatchesTailLex("Noun + A3sg + Ness + Noun + A3sg"));
            tester.ExpectAny("elmalığı",
                MatchesTailLex("Noun + A3sg + Ness + Noun + A3sg + Acc"));
            tester.ExpectAny("elmalığa",
                MatchesTailLex("Noun + A3sg + Ness + Noun + A3sg + Dat"));
            tester.ExpectAny("elmasızlık",
                MatchesTailLex(
                    "Noun + A3sg + Without + Adj + Ness + Noun + A3sg"));

            tester.ExpectFail(
                "elmalarlık",
                "elmamlık",
                "elmlığ",
                "elmayalık",
                "elmadalık"
            );
        }

        [TestMethod]
        public void AgtTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectAny("elmacı",
                MatchesTailLex("Noun + A3sg + Agt + Noun + A3sg"));
            tester.ExpectAny("elmacıyı",
                MatchesTailLex("Noun + A3sg + Agt + Noun + A3sg + Acc"));
            tester.ExpectAny("elmacıya",
                MatchesTailLex("Noun + A3sg + Agt + Noun + A3sg + Dat"));
            tester.ExpectAny("elmacıkçı",
                MatchesTailLex(
                    "Noun + A3sg + Dim + Noun + A3sg + Agt + Noun + A3sg"));
            tester.ExpectAny("elmacılık",
                MatchesTailLex(
                    "Noun + A3sg + Agt + Noun + A3sg + Ness + Noun + A3sg"));
            tester.ExpectAny("elmacılığı",
                MatchesTailLex(
                    "Noun + A3sg + Agt + Noun + A3sg + Ness + Noun + A3sg + Acc"));

            tester.ExpectFail(
                "elmalarcı",
                "elmamcı",
                "elmayacı",
                "elmadacı"
            );
        }


        [TestMethod]
        public void Become()
        {
            AnalysisTester t = GetTester("tahta");

            t.ExpectAny("tahtalaş",
                MatchesTailLex("Noun + A3sg + Become + Verb + Imp + A2sg"));
            t.ExpectAny("tahtalaştık",
                MatchesTailLex("Noun + A3sg + Become + Verb + Past + A1pl"));
            t.ExpectAny("tahtalaşacak",
                MatchesTailLex("Noun + A3sg + Become + Verb + Fut + A3sg"));

            t.ExpectFail(
                "tahtamlaş",
                "tahtalarlaştı",
                "tahtayalaştı"
            );

            t = GetTester("kitap");
            t.ExpectAny("kitaplaştı",
                MatchesTailLex("Noun + A3sg + Become + Verb + Past + A3sg"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağlaştık",
                MatchesTailLex("Noun + A3sg + Become + Verb + Past + A1pl"));
        }

        [TestMethod]
        public void Acquire()
        {
            AnalysisTester t = GetTester("tahta");

            t.ExpectAny("tahtalan",
                MatchesTailLex("Noun + A3sg + Acquire + Verb + Imp + A2sg"));
            t.ExpectAny("tahtalandık",
                MatchesTailLex("Noun + A3sg + Acquire + Verb + Past + A1pl"));
            t.ExpectAny("tahtalanacak",
                MatchesTailLex("Noun + A3sg + Acquire + Verb + Fut + A3sg"));

            t.ExpectFail(
                "tahtamlan",
                "tahtalarlandı",
                "tahtayaland"
            );

            t = GetTester("kitap");
            t.ExpectAny("kitaplandı",
                MatchesTailLex("Noun + A3sg + Acquire + Verb + Past + A3sg"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağlandık",
                MatchesTailLex("Noun + A3sg + Acquire + Verb + Past + A1pl"));
        }

        [TestMethod]
        public void WhileTest()
        {
            AnalysisTester t = GetTester("tahta");

            t.ExpectAny("tahtayken",
                MatchesTailLex("Noun + A3sg + Zero + Verb + While + Adv"));
            t.ExpectAny("tahtamken",
                MatchesTailLex("Noun + A3sg + P1sg + Zero + Verb + While + Adv"));
        }

        [TestMethod]
        public void ExpectsSingleResult2()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("elma");
            ExpectSuccess(analyzer, 1, "elmayım");
            ExpectSuccess(analyzer, 1, "elmaydım");
            ExpectSuccess(analyzer, 1, "elmayımdır");
            ExpectSuccess(analyzer, 1, "elmaydı");
            ExpectSuccess(analyzer, 1, "elmadır");
            ExpectSuccess(analyzer, 1, "elmayadır");

            // this has two analyses.
            ExpectSuccess(analyzer, 2, "elmalar");
        }

        [TestMethod]
        public void Incorrect2()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("elma");
            ExpectFail(analyzer,
                "elmaydıdır",
                "elmayıdır",
                "elmamdırım",
                "elmamdımdır",
                "elmalarlar", // Oflazer accepts this.
                "elmamım", // Oflazer accepts this.
                "elmamımdır", // Oflazer accepts this.
                "elmaydılardır"
            );
        }

        [TestMethod]
        public void DegilTest()
        {
            AnalysisTester tester = GetTester("değil [P:Verb]");
            tester.ExpectSingle("değil", MatchesTailLex("Neg + Pres + A3sg"));
            tester.ExpectSingle("değildi", MatchesTailLex("Neg + Past + A3sg"));
            tester.ExpectSingle("değilim", MatchesTailLex("Neg + Pres + A1sg"));
            tester.ExpectSingle("değildir", MatchesTailLex("Neg + Pres + A3sg + Cop"));
            tester.ExpectSingle("değilimdir", MatchesTailLex("Neg + Pres + A1sg + Cop"));
            tester.ExpectSingle("değilsindir", MatchesTailLex("Neg + Pres + A2sg + Cop"));
            tester.ExpectSingle("değilsinizdir", MatchesTailLex("Neg + Pres + A2pl + Cop"));
            tester.ExpectSingle("değilmişsinizdir", MatchesTailLex("Neg + Narr + A2pl + Cop"));

            tester.ExpectFail(
                "değildinizdir"
            );
        }

        [TestMethod]
        public void NounVerbZeroTest()
        {
            AnalysisTester tester = GetTester("elma");
            tester.ExpectSingle("elmayım", MatchesTailLex("Zero + Verb + Pres + A1sg"));
            tester.ExpectSingle("elmanım", MatchesTailLex("Zero + Verb + Pres + A1sg"));
        }

        [TestMethod]
        public void NarrTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectSingle("elmaymış", MatchesTailLex("Zero + Verb + Narr + A3sg"));
            tester.ExpectSingle("elmaymışız", MatchesTailLex("Zero + Verb + Narr + A1pl"));
            tester.ExpectSingle("elmaymışım", MatchesTailLex("Zero + Verb + Narr + A1sg"));
            tester.ExpectSingle("elmaymışımdır", MatchesTailLex("Zero + Verb + Narr + A1sg + Cop"));
            tester.ExpectSingle("elmaymışsam", MatchesTailLex("Zero + Verb + Narr + Cond + A1sg"));

            tester.ExpectFail(
                "elmaymışmış"
            );
        }

        [TestMethod]
        public void PastTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectSingle("elmaydı", MatchesTailLex("Zero + Verb + Past + A3sg"));
            tester.ExpectSingle("elmaydık", MatchesTailLex("Zero + Verb + Past + A1pl"));
            tester.ExpectSingle("elmaydım", MatchesTailLex("Zero + Verb + Past + A1sg"));
            tester.ExpectSingle("elmaydılar", MatchesTailLex("Zero + Verb + Past + A3pl"));

            tester.ExpectFail(
                "elmaydıysa",
                "elmaydıyız",
                "elmaydılardır"
            );
        }

        [TestMethod]
        public void CondTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectSingle("elmaysa", MatchesTailLex("Zero + Verb + Cond + A3sg"));
            tester.ExpectSingle("elmaysak", MatchesTailLex("Zero + Verb + Cond + A1pl"));
            tester.ExpectSingle("elmaymışsa", MatchesTailLex("Zero + Verb + Narr + Cond + A3sg"));
            tester.ExpectSingle("elmaymışsam", MatchesTailLex("Zero + Verb + Narr + Cond + A1sg"));

            tester.ExpectFail(
                "elmaydıysa",
                "elmaysadır",
                "elmaysalardır"
            );
        }

        [TestMethod]
        public void A2plTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectSingle("elmasınız", MatchesTailLex("Zero + Verb + Pres + A2pl"));
            tester.ExpectSingle("elmaydınız", MatchesTailLex("Zero + Verb + Past + A2pl"));
            tester.ExpectSingle("elmaymışsınız", MatchesTailLex("Zero + Verb + Narr + A2pl"));
            tester.ExpectSingle("elmaysanız", MatchesTailLex("Zero + Verb + Cond + A2pl"));
            tester.ExpectSingle("elmaymışsanız", MatchesTailLex("Zero + Verb + Narr + Cond + A2pl"));
        }

        [TestMethod]
        public void A3plAfterZeroVerbDerivationTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectAny("elmalar", MatchesTailLex("Zero + Verb + Pres + A3pl"));
            tester.ExpectAny("elmalardır", MatchesTailLex("Zero + Verb + Pres + A3pl + Cop"));
            tester.ExpectAny("elmadırlar", MatchesTailLex("A3sg + Zero + Verb + Pres + Cop + A3pl"));
            tester.ExpectAny("elmayadırlar", MatchesTailLex("Dat + Zero + Verb + Pres + Cop + A3pl"));
            tester.ExpectAny("elmasındalar", MatchesTailLex("Loc + Zero + Verb + Pres + A3pl"));
        }

        [TestMethod]
        public void AfterLocTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectSingle("elmadayım", MatchesTailLex("Loc + Zero + Verb + Pres + A1sg"));
            tester.ExpectSingle("elmadasın", MatchesTailLex("Loc + Zero + Verb + Pres + A2sg"));
            tester.ExpectSingle("elmadaydı", MatchesTailLex("Loc + Zero + Verb + Past + A3sg"));
            tester.ExpectSingle("elmadaymışsınız", MatchesTailLex("Loc + Zero + Verb + Narr + A2pl"));
            tester.ExpectSingle("elmadaysak", MatchesTailLex("Loc + Zero + Verb + Cond + A1pl"));
        }

        [TestMethod]
        public void CopulaBeforeA3plTest()
        {
            AnalysisTester tester = GetTester("elma");

            tester.ExpectSingle("elmadırlar",
                MatchesTailLex("Zero + Verb + Pres + Cop + A3pl"));
            tester.ExpectAny("elmadalardır",
                MatchesTailLex("A3sg + Loc + Zero + Verb + Pres + A3pl + Cop"));
            tester.ExpectSingle("elmamdadırlar",
                MatchesTailLex("P1sg + Loc + Zero + Verb + Pres + Cop + A3pl"));

            tester.ExpectFail(
                "elmadalardırlar",
                "elmadadırlardır"
            );
        }

        [TestMethod]
        public void Related()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectSingle("meyvesel",
                MatchesTailLex("Noun + A3sg + Related + Adj"));
            tester.ExpectAny("meyveseldi",
                MatchesTailLex("Noun + A3sg + Related + Adj + Zero + Verb + Past + A3sg"));

            tester.ExpectFail(
                "meyvemsel",
                "meyvedesel",
                "meyveselsel",
                "meyveselki"
            );
        }

        [TestMethod]
        public void RelPronDerivationTest()
        {
            AnalysisTester tester = GetTester("meyve");
            tester.ExpectAny("meyveninki",
                MatchesTailLex("Noun + A3sg + Gen + Rel + Pron + A3sg"));
            tester.ExpectAny("meyveninkine",
                MatchesTailLex("Noun + A3sg + Gen + Rel + Pron + A3sg + Dat"));
            tester.ExpectAny("meyveminkine",
                MatchesTailLex("Noun + A3sg + P1sg + Gen + Rel + Pron + A3sg + Dat"));
            tester.ExpectAny("meyveminkinde",
                MatchesTailLex("Noun + A3sg + P1sg + Gen + Rel + Pron + A3sg + Loc"));
            tester.ExpectAny("meyveminkindeymiş",
                MatchesTailLex(
                    "Noun + A3sg + P1sg + Gen + Rel + Pron + A3sg + Loc + Zero + Verb + Narr + A3sg"));
        }


        [TestMethod]
        public void Noun2VerbAsIfTest()
        {
            AnalysisTester tester = GetTester("dost");
            tester.ExpectSingle("dostmuşçasına",
                MatchesTailLex("Zero + Verb + Narr + A3sg + AsIf + Adv"));
            tester.ExpectSingle("dostmuşlarcasına",
                MatchesTailLex("Zero + Verb + Narr + A3pl + AsIf + Adv"));
        }

        /**
         * Test for Issue 170. After justlike derivation, P2sg should not be allowed. Such as: "güzelsin"
         */
        [TestMethod]
        public void JustlikeTest_Issue_170()
        {
            AnalysisTester tester = GetTester("güzel [P:Adj]");
            // no Justlike+Noun+A3sg+P2sg allowed
            tester.ExpectSingle("güzelsin", MatchesTailLex("Zero + Verb + Pres + A2sg"));
            tester = GetTester("odun");
            // no Justlike+Adj+Zero+A3sg+P2sg allowed
            tester.ExpectSingle("odunsun", MatchesTailLex("Noun + A3sg + Zero + Verb + Pres + A2sg"));
        }

        /**
         * Test for Issue 167. For adjective to noun derivation like `mor-luk` two analysis was produced.
         * One was redundant.
         */
        [TestMethod]
        public void NessTest_Issue_167()
        {
            AnalysisTester tester = GetTester("mor [P:Adj]");
            // no Adj|Zero→Noun+A3sg|luk:Ness→Noun+A3sg
            tester.ExpectSingle("morluk", MatchesTailLex("Adj + Ness + Noun + A3sg"));
        }

        /**
         * Test for Issue 184 : Cannot analyze `abimsin` or any Noun+..+P1sg+..+Verb+..+A2sg
         */
        [TestMethod]
        public void A2sgVerbAfterP1sgNounTest_Issue_184()
        {
            AnalysisTester tester = GetTester("abi");
            tester.ExpectSingle(
                "abimsin", MatchesTailLex("Noun + A3sg + P1sg + Zero + Verb + Pres + A2sg"));
            tester.ExpectSingle(
                "abimsiniz", MatchesTailLex("Noun + A3sg + P1sg + Zero + Verb + Pres + A2pl"));
            tester.ExpectSingle(
                "abinim", MatchesTailLex("Noun + A3sg + P2sg + Zero + Verb + Pres + A1sg"));
        }
    }
}
