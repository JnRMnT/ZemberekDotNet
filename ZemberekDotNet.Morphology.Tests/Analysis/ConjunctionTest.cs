using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class ConjunctionTest : AnalyzerTestBase
    {
        [TestMethod]
        public void ConjTest()
        {
            AnalysisTester tester = GetTester("ve [P:Conj]");
            tester.ExpectSingle("ve", MatchesTailLex("Conj"));
        }
    }
}
