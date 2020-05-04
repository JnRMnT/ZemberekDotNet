using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class QuestionTest : AnalyzerTestBase
    {
        [TestMethod]
        public void MıTest()
        {
            AnalysisTester tester = GetTester("mı [P:Ques]");

            tester.ExpectSingle("mı", MatchesTailLex("Ques + Pres + A3sg"));
            tester.ExpectSingle("mısın", MatchesTailLex("Ques + Pres + A2sg"));
            tester.ExpectSingle("mıydı", MatchesTailLex("Ques + Past + A3sg"));
            tester.ExpectSingle("mıymışsın", MatchesTailLex("Ques + Narr + A2sg"));

            tester.ExpectFail("mıymışsak");
        }

        [TestMethod]
        public void MıCopulaTest()
        {
            AnalysisTester tester = GetTester("mı [P:Ques]");

            tester.ExpectSingle("mıdır", MatchesTailLex("Ques + Pres + A3sg + Cop"));
            tester.ExpectSingle("mısındır", MatchesTailLex("Ques + Pres + A2sg + Cop"));
            tester.ExpectSingle("mıyımdır", MatchesTailLex("Ques + Pres + A1sg + Cop"));
            tester.ExpectSingle("mıyızdır", MatchesTailLex("Ques + Pres + A1pl + Cop"));
            tester.ExpectSingle("mıymışımdır", MatchesTailLex("Ques + Narr + A1sg + Cop"));
            tester.ExpectSingle("mıymışsındır", MatchesTailLex("Ques + Narr + A2sg + Cop"));
            tester.ExpectSingle("mıymıştır", MatchesTailLex("Ques + Narr + A3sg + Cop"));
            tester.ExpectSingle("mıdırlar", MatchesTailLex("Ques + Pres + Cop + A3pl"));

            tester.ExpectFail("mıydıdır");
        }
    }
}
