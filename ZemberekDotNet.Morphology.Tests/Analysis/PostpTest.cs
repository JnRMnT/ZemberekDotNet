using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class PostpTest : AnalyzerTestBase
    {

        [TestMethod]
        public void GibiTest()
        {
            AnalysisTester tester = GetTester("gibi [P:Postp, PCNom]");
            tester.ExpectSingle("gibi", MatchesTailLex("Postp"));
            tester.ExpectSingle("gibisi", MatchesTailLex("Postp + Zero + Noun + A3sg + P3sg"));
            tester.ExpectAny("gibiler", MatchesTailLex("Postp + Zero + Noun + A3pl"));
            tester.ExpectSingle("gibilere", MatchesTailLex("Postp + Zero + Noun + A3pl + Dat"));
            tester.ExpectSingle("gibisine", MatchesTailLex("Postp + Zero + Noun + A3sg + P3sg + Dat"));
            tester.ExpectSingle("gibisiyle", MatchesTailLex("Postp + Zero + Noun + A3sg + P3sg + Ins"));
            tester.ExpectSingle("gibilerle", MatchesTailLex("Postp + Zero + Noun + A3pl + Ins"));
        }

        /**
         * Test for issue
         * <a href="https://github.com/ahmetaa/zemberek-nlp/issues/178">178</a>
         */
        [TestMethod]
        public void GibimeTest_issue_178()
        {
            AnalysisTester tester = GetTester("gibi [P:Postp, PCNom]");
            tester.ExpectSingle("gibime", MatchesTailLex("Postp + Zero + Noun + A3sg + P1sg + Dat"));
            tester.ExpectSingle("gibine", MatchesTailLex("Postp + Zero + Noun + A3sg + P2sg + Dat"));
            tester.ExpectSingle("gibinize", MatchesTailLex("Postp + Zero + Noun + A3sg + P2pl + Dat"));
            tester.ExpectSingle("gibimize", MatchesTailLex("Postp + Zero + Noun + A3sg + P1pl + Dat"));
        }
    }
}
