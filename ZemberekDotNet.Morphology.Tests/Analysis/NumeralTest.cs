using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class NumeralTest: AnalyzerTestBase
    {
        [TestMethod]
        public void OrdinalTest()
        {
            AnalysisTester t = GetTester("bir [P:Num,Ord]");
            t.ExpectAny("bir", MatchesTailLex("Num"));
            t.ExpectAny("bire", MatchesTailLex("Num + Zero + Noun + A3sg + Dat"));
            t.ExpectAny("birmiş", MatchesTailLex("Num + Zero + Verb + Narr + A3sg"));
        }
    }
}
