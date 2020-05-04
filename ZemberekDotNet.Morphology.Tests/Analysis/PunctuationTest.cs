using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class PunctuationTest : AnalyzerTestBase
    {
        [TestMethod]
        public void Test1()
        {
            AnalysisTester tester = GetTester("… [P:Punc]");
            tester.ExpectSingle("…", MatchesTailLex("Punc"));
        }
    }
}
