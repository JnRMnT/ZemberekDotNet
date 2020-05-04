using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class AdverbsTest : AnalyzerTestBase
    {
        [TestMethod]
        public void AdvTest()
        {
            AnalysisTester tester = GetTester("işte [P:Adv]");
            tester.ExpectSingle("işte", MatchesTailLex("Adv"));
        }

        [TestMethod]
        public void AdvTest2()
        {
            AnalysisTester tester = GetTester("olmak");
            tester.ExpectSingle("olunca",
                MatchesTailLex("Verb + When + Adv"));
            tester.ExpectSingle("oluncaya",
                MatchesTailLex("Verb + When + Adv + Zero + Noun + A3sg + Dat"));
        }
    }
}
