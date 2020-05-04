using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class AdjectiveDerivationTest : AnalyzerTestBase
    {
        [TestMethod]
        public void Become()
        {
            AnalysisTester t = GetTester("beyaz [P:Adj]");

            t.ExpectSingle("beyazlaş", MatchesTailLex("Adj + Become + Verb + Imp + A2sg"));
            t.ExpectAny("beyazlaştık", MatchesTailLex("Adj + Become + Verb + Past + A1pl"));
            t.ExpectAny("beyazlaşacak", MatchesTailLex("Adj + Become + Verb + Fut + A3sg"));

            t.ExpectFail(
                "beyazımlaş",
                "beyazlarlaştı",
                "beyazyalaştı"
            );
        }

        [TestMethod]
        public void Ly()
        {
            AnalysisTester t = GetTester("beyaz [P:Adj]");
            t.ExpectAny("beyazca", MatchesTailLex("Adj + Ly + Adv"));
        }

        [TestMethod]
        public void JustlikeAdjTest()
        {
            AnalysisTester tester = GetTester("mavi [P:Adj]");
            tester.ExpectSingle("mavimsi",
                MatchesTailLex("Adj + JustLike + Adj"));
            tester = GetTester("siyah [P:Adj]");
            tester.ExpectSingle("siyahsı",
                MatchesTailLex("Adj + JustLike + Adj"));
            tester.ExpectSingle("siyahımsı",
                MatchesTailLex("Adj + JustLike + Adj"));
        }

        [TestMethod]
        public void ExpectsSingleResult()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("mavi [P:Adj]");
            ExpectSuccess(analyzer, 1, "maviyim");
            ExpectSuccess(analyzer, 1, "maviydim");
            ExpectSuccess(analyzer, 1, "maviyimdir");
            ExpectSuccess(analyzer, 1, "maviydi");
            ExpectSuccess(analyzer, 1, "mavidir");
            ExpectSuccess(analyzer, 1, "maviliyimdir");
        }


        [TestMethod]
        public void AgtTest()
        {
            AnalysisTester tester = GetTester("ucuz [P:Adj]");

            tester.ExpectAny("ucuzcu",
                MatchesTailLex("Adj + Agt + Noun + A3sg"));
            tester.ExpectAny("ucuzcuyu",
                MatchesTailLex("Adj + Agt + Noun + A3sg + Acc"));
            tester.ExpectAny("ucuzcuya",
                MatchesTailLex("Adj + Agt + Noun + A3sg + Dat"));
            tester.ExpectAny("ucuzculuk",
                MatchesTailLex("Adj + Agt + Noun + A3sg + Ness + Noun + A3sg"));
            tester.ExpectAny("ucuzculuğu",
                MatchesTailLex("Adj + Agt + Noun + A3sg + Ness + Noun + A3sg + Acc"));


            tester.ExpectFail(
                "ucuzcucu",
                "ucuzlucu",
                "ucuzumsucu"
            );
        }
    }
}
