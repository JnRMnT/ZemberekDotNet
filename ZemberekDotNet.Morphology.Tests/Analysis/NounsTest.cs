using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class NounsTest : AnalyzerTestBase
    {
        [TestMethod]
        public void ImplicitDative_1()
        {
            AnalysisTester t = GetTester("içeri [A:ImplicitDative]");
            t.ExpectAny("içeri", MatchesTailLex("Noun + A3sg + Dat"));
            t.ExpectAny("içeri", MatchesTailLex("Noun + A3sg"));
        }

        [TestMethod]
        public void ImplicitPLural_1()
        {
            AnalysisTester t = GetTester("hayvanat [A:ImplicitPlural]");
            t.ExpectSingle("hayvanat", MatchesTailLex("Noun + A3pl"));
        }

        [TestMethod]
        public void Voicing_1()
        {
            AnalysisTester t = GetTester("kitap");
            t.ExpectSingle("kitap", MatchesTailLex("Noun + A3sg"));
            t.ExpectAny("kitaplar", MatchesTailLex("Noun + A3pl"));
            t.ExpectAny("kitabım", MatchesTailLex("Noun + A3sg + P1sg"));
            t.ExpectAny("kitaba", MatchesTailLex("Noun + A3sg + Dat"));
            t.ExpectAny("kitapta", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectAny("kitapçık",
                MatchesTailLex("Noun + A3sg + Dim + Noun + A3sg"));

            t.ExpectFail("kitapım", "kitab", "kitabcık", "kitapa", "kitablar");
        }

        [TestMethod]
        public void LastVowelDropExceptionTest()
        {
            AnalysisTester t = GetTester("içeri [A:ImplicitDative]");

            t.ExpectAny("içeri", MatchesTailLex("Noun + A3sg + Dat"));
            t.ExpectAny("içeride", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectAny("içerilerde", MatchesTailLex("Noun + A3pl + Loc"));
            t.ExpectAny("içerde", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectAny("içerlerde", MatchesTailLex("Noun + A3pl + Loc"));

            t.ExpectFail("içer");
            t.ExpectFail("içerdim");

            t = GetTester("bura");
            t.ExpectAny("burada", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectAny("burda", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectAny("burlarda", MatchesTailLex("Noun + A3pl + Loc"));
            t.ExpectAny("burdan", MatchesTailLex("Noun + A3sg + Abl"));

            t.ExpectFail("burd");
            t.ExpectFail("burdum");
        }


        [TestMethod]
        public void SuTest()
        {
            AnalysisTester t = GetTester("su");

            t.ExpectSingle("su", MatchesTailLex("Noun + A3sg"));
            t.ExpectSingle("sulara", MatchesTailLex("Noun + A3pl + Dat"));
            t.ExpectSingle("suyuma", MatchesTailLex("Noun + A3sg + P1sg + Dat"));
            t.ExpectAny("suyun", MatchesTailLex("Noun + A3sg + P2sg"));
            t.ExpectAny("suyun", MatchesTailLex("Noun + A3sg + Gen"));
            t.ExpectSingle("suyumuz", MatchesTailLex("Noun + A3sg + P1pl"));

            t.ExpectFail(
                "sunun",
                "susu",
                "sum",
                "sun"
            );
        }

        [TestMethod]
        public void P2pl()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectAny("eviniz", MatchesTailLex("Noun + A3sg + P2pl"));
            t.ExpectSingle("evinize", MatchesTailLex("Noun + A3sg + P2pl + Dat"));
            t.ExpectSingle("evinizi", MatchesTailLex("Noun + A3sg + P2pl + Acc"));
            t.ExpectAny("evleriniz", MatchesTailLex("Noun + A3pl + P2pl"));
            t.ExpectSingle("evlerinize", MatchesTailLex("Noun + A3pl + P2pl + Dat"));
            t.ExpectSingle("evlerinizi", MatchesTailLex("Noun + A3pl + P2pl + Acc"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağınız", MatchesTailLex("Noun + A3sg + P2pl"));
            t.ExpectSingle("zeytinyağınıza", MatchesTailLex("Noun + A3sg + P2pl + Dat"));
            t.ExpectAny("zeytinyağlarınız", MatchesTailLex("Noun + A3pl + P2pl"));
            t.ExpectSingle("zeytinyağlarınıza", MatchesTailLex("Noun + A3pl + P2pl + Dat"));
        }

        [TestMethod]
        public void Dative()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectSingle("eve", MatchesTailLex("Noun + A3sg + Dat"));
            t.ExpectSingle("evlere", MatchesTailLex("Noun + A3pl + Dat"));
            t.ExpectSingle("evime", MatchesTailLex("Noun + A3sg + P1sg + Dat"));
            t.ExpectSingle("evimize", MatchesTailLex("Noun + A3sg + P1pl + Dat"));

            t = GetTester("kitap");

            t.ExpectSingle("kitaba", MatchesTailLex("Noun + A3sg + Dat"));
            t.ExpectSingle("kitabıma", MatchesTailLex("Noun + A3sg + P1sg + Dat"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağına", MatchesTailLex("Noun + A3sg + Dat"));
            t.ExpectSingle("zeytinyağıma", MatchesTailLex("Noun + A3sg + P1sg + Dat"));
            t.ExpectSingle("zeytinyağlarımıza", MatchesTailLex("Noun + A3pl + P1pl + Dat"));
            t.ExpectSingle("zeytinyağlarınıza", MatchesTailLex("Noun + A3pl + P2pl + Dat"));
        }

        [TestMethod]
        public void Ablative()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectAny("evden", MatchesTailLex("Noun + A3sg + Abl"));
            t.ExpectSingle("evlerden", MatchesTailLex("Noun + A3pl + Abl"));
            t.ExpectSingle("evimden", MatchesTailLex("Noun + A3sg + P1sg + Abl"));
            t.ExpectAny("evimizden", MatchesTailLex("Noun + A3sg + P1pl + Abl"));

            t = GetTester("kitap");

            t.ExpectAny("kitaptan", MatchesTailLex("Noun + A3sg + Abl"));
            t.ExpectAny("kitabımdan", MatchesTailLex("Noun + A3sg + P1sg + Abl"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağından", MatchesTailLex("Noun + A3sg + Abl"));
            t.ExpectSingle("zeytinyağımdan", MatchesTailLex("Noun + A3sg + P1sg + Abl"));
            t.ExpectSingle("zeytinyağlarımızdan", MatchesTailLex("Noun + A3pl + P1pl + Abl"));
            t.ExpectSingle("zeytinyağlarınızdan", MatchesTailLex("Noun + A3pl + P2pl + Abl"));
        }

        [TestMethod]
        public void Locative()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectAny("evde", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectSingle("evlerde", MatchesTailLex("Noun + A3pl + Loc"));
            t.ExpectSingle("evimde", MatchesTailLex("Noun + A3sg + P1sg + Loc"));
            t.ExpectAny("evimizde", MatchesTailLex("Noun + A3sg + P1pl + Loc"));

            t = GetTester("kitap");

            t.ExpectAny("kitapta", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectAny("kitabımda", MatchesTailLex("Noun + A3sg + P1sg + Loc"));

            t = GetTester("elma");

            t.ExpectAny("elmada", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectAny("elmanda", MatchesTailLex("Noun + A3sg + P2sg + Loc"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağında", MatchesTailLex("Noun + A3sg + Loc"));
            t.ExpectSingle("zeytinyağımda", MatchesTailLex("Noun + A3sg + P1sg + Loc"));
            t.ExpectSingle("zeytinyağlarımızda", MatchesTailLex("Noun + A3pl + P1pl + Loc"));
            t.ExpectSingle("zeytinyağlarınızda", MatchesTailLex("Noun + A3pl + P2pl + Loc"));
        }

        [TestMethod]
        public void Locative2()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectAny("evdeyim", MatchesTailLex("Noun + A3sg + Loc + Zero + Verb + Pres + A1sg"));
        }

        [TestMethod]
        public void Instrumental()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectAny("evle", MatchesTailLex("Noun + A3sg + Ins"));
            t.ExpectSingle("evlerle", MatchesTailLex("Noun + A3pl + Ins"));
            t.ExpectSingle("evimle", MatchesTailLex("Noun + A3sg + P1sg + Ins"));
            t.ExpectAny("evimizle", MatchesTailLex("Noun + A3sg + P1pl + Ins"));

            t = GetTester("kitap");

            t.ExpectAny("kitapla", MatchesTailLex("Noun + A3sg + Ins"));
            t.ExpectAny("kitabımla", MatchesTailLex("Noun + A3sg + P1sg + Ins"));

            t = GetTester("elma");

            t.ExpectAny("elmayla", MatchesTailLex("Noun + A3sg + Ins"));
            t.ExpectAny("elmanla", MatchesTailLex("Noun + A3sg + P2sg + Ins"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağıyla", MatchesTailLex("Noun + A3sg + Ins"));
            t.ExpectSingle("zeytinyağımla", MatchesTailLex("Noun + A3sg + P1sg + Ins"));
            t.ExpectSingle("zeytinyağlarımızla", MatchesTailLex("Noun + A3pl + P1pl + Ins"));
            t.ExpectSingle("zeytinyağlarınızla", MatchesTailLex("Noun + A3pl + P2pl + Ins"));
        }

        [TestMethod]
        public void Genitive()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectAny("evin", MatchesTailLex("Noun + A3sg + Gen"));
            t.ExpectAny("evlerin", MatchesTailLex("Noun + A3pl + Gen"));
            t.ExpectSingle("evimin", MatchesTailLex("Noun + A3sg + P1sg + Gen"));
            t.ExpectSingle("evimizin", MatchesTailLex("Noun + A3sg + P1pl + Gen"));

            t = GetTester("kitap");

            t.ExpectAny("kitabın", MatchesTailLex("Noun + A3sg + Gen"));
            t.ExpectSingle("kitabımın", MatchesTailLex("Noun + A3sg + P1sg + Gen"));

            t = GetTester("elma");

            t.ExpectSingle("elmamın", MatchesTailLex("Noun + A3sg + P1sg + Gen"));
            t.ExpectAny("elmanın", MatchesTailLex("Noun + A3sg + P2sg + Gen"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağının", MatchesTailLex("Noun + A3sg + Gen"));
            t.ExpectSingle("zeytinyağımın", MatchesTailLex("Noun + A3sg + P1sg + Gen"));
            t.ExpectSingle("zeytinyağlarımızın", MatchesTailLex("Noun + A3pl + P1pl + Gen"));
            t.ExpectSingle("zeytinyağlarınızın", MatchesTailLex("Noun + A3pl + P2pl + Gen"));
        }


        [TestMethod]
        public void Equ()
        {
            AnalysisTester t = GetTester("ev");

            t.ExpectAny("evce", MatchesTailLex("Noun + A3sg + Equ"));
            t.ExpectAny("evlerce", MatchesTailLex("Noun + A3pl + Equ"));
            t.ExpectSingle("evimce", MatchesTailLex("Noun + A3sg + P1sg + Equ"));
            t.ExpectSingle("evimizce", MatchesTailLex("Noun + A3sg + P1pl + Equ"));
            t.ExpectAny("evlerince", MatchesTailLex("Noun + A3pl + P3sg + Equ"));

            t = GetTester("kitap");

            t.ExpectAny("kitapça", MatchesTailLex("Noun + A3sg + Equ"));
            t.ExpectAny("kitaplarınca", MatchesTailLex("Noun + A3pl + P3pl + Equ"));
            t.ExpectSingle("kitabımca", MatchesTailLex("Noun + A3sg + P1sg + Equ"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağınca", MatchesTailLex("Noun + A3sg + Equ"));
        }

        [TestMethod]
        public void P3pl()
        {
            AnalysisTester t = GetTester("ev");

            // P3pl typically generates 4 analysis
            t.ExpectAny("evleri", MatchesTailLex("Noun + A3pl + Acc"));
            t.ExpectAny("evleri", MatchesTailLex("Noun + A3pl + P3sg"));
            t.ExpectAny("evleri", MatchesTailLex("Noun + A3sg + P3pl"));
            t.ExpectAny("evleri", MatchesTailLex("Noun + A3pl + P3pl"));

            t.ExpectAny("evlerine", MatchesTailLex("Noun + A3sg + P3pl + Dat"));
            t.ExpectAny("evlerinde", MatchesTailLex("Noun + A3sg + P3pl + Loc"));
            t.ExpectAny("evlerinden", MatchesTailLex("Noun + A3sg + P3pl + Abl"));
            t.ExpectAny("evleriyle", MatchesTailLex("Noun + A3sg + P3pl + Ins"));
            t.ExpectAny("evlerini", MatchesTailLex("Noun + A3sg + P3pl + Acc"));

            t = GetTester("kitap");
            t.ExpectAny("kitapları", MatchesTailLex("Noun + A3pl + P3sg"));

            t = GetTester(
                "zeytin",
                "yağ",
                "zeytinyağı [A:CompoundP3sg; Roots:zeytin-yağ]");

            t.ExpectAny("zeytinyağları", MatchesTailLex("Noun + A3pl"));
            t.ExpectAny("zeytinyağları", MatchesTailLex("Noun + A3pl + P3pl"));
            t.ExpectAny("zeytinyağları", MatchesTailLex("Noun + A3pl + P3sg"));
            t.ExpectAny("zeytinyağları", MatchesTailLex("Noun + A3sg + P3pl"));
            t.ExpectAny("zeytinyağlarına", MatchesTailLex("Noun + A3pl + P3sg + Dat"));
        }

        [TestMethod]
        public void Family1()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "annemler [A:ImplicitPlural,ImplicitP1sg,FamilyMember]");
            ExpectFail(analyzer, "annemlerler", "annemlerim");
        }

        [TestMethod]
        public void Family2()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "annemler [A:ImplicitPlural,ImplicitP1sg,FamilyMember]");
            ExpectSuccess(analyzer, 1, "annemler", "annemlere", "annemleri");
        }

        [TestMethod]
        public void Family3()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "annemler [A:ImplicitPlural,ImplicitP1sg,FamilyMember]");
            string input = "annemleri";
            List<SingleAnalysis> results = analyzer.Analyze(input);
            PrintAndSort(input, results);
            Assert.AreEqual(1, results.Count);
            SingleAnalysis first = results[0];
            Assert.IsTrue(ContainsMorpheme(first, "Acc"));
            Assert.IsTrue(!ContainsMorpheme(first, "P3sg"));
        }

        [TestMethod]
        public void ProperNoun1()
        {
            AnalysisTester t = GetTester("Ankara");
            t.ExpectAny("ankara", MatchesTailLex("Noun + A3sg"));
        }

        [TestMethod]
        public void AbbreviationShouldNotGetPossessive()
        {
            AnalysisTester t = GetTester("Tdk [Pr:tedeka]");
            t.ExpectAny("tdk", MatchesTailLex("Noun + A3sg"));
            t.ExpectAny("tdkya", MatchesTailLex("Noun + A3sg + Dat"));
            t.ExpectAny("tdknın", MatchesTailLex("Noun + A3sg + Gen"));

            t.ExpectFail(
                "Tdkm",
                "Tdkn",
                "Tdksı",
                "Tdkmız",
                "Tdknız"
            );
        }

        [TestMethod]
        public void Uzeri()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer(
                "üzeri [A:CompoundP3sg;Roots:üzer]");
            string input = "üzeri";
            List<SingleAnalysis> results = analyzer.Analyze(input);
            Assert.AreEqual(2, results.Count);
        }
    }
}
