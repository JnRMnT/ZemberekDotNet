using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class AdjectivesTest: AnalyzerTestBase
    {
        [TestMethod]
        public void ExpectsSingleResult()
        {
            AnalysisTester tester = GetTester("mavi [P:Adj]");
            tester.ExpectSuccess(
                1,
                "mavi",
                "maviye",
                "mavilere",
                "mavilerime",
                "mavicik",
                "mavili",
                "mavicikli",
                "mavicikliye"
            );
        }

        [TestMethod]
        public void Expects2Results()
        {
            AnalysisTester tester = GetTester("mavi [P:Adj]");
            tester.ExpectSuccess(
                2,
                "maviler"
            );
        }
    }
}
