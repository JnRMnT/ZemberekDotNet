using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Lexicon.TR;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class InformalTurkishAnalysisTest : AnalyzerTestBase
    {
        public static TurkishMorphotactics GetSpokenMorphotactics(params string[] dictionaryLines)
        {
            RootLexicon lexicon = TurkishDictionaryLoader.Load(dictionaryLines);
            return new InformalTurkishMorphotactics(lexicon);
        }

        static new AnalysisTester GetTester(params string[] dictionaryLines)
        {
            return new AnalysisTester(
                RuleBasedAnalyzer.ForDebug(GetSpokenMorphotactics(dictionaryLines)));
        }

        static AnalysisTester GetTesterAscii(params string[] dictionaryLines)
        {
            return new AnalysisTester(RuleBasedAnalyzer
                .ForDebug(GetSpokenMorphotactics(dictionaryLines), true));
        }

        [TestMethod]
        public void TestProgressiveDeformation()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazıyom", MatchesTailLex("Verb + Prog1_Informal + A1sg"));
            t.ExpectSingle("yazıyon", MatchesTailLex("Verb + Prog1_Informal + A2sg"));
            t.ExpectSingle("yazıyosun", MatchesTailLex("Verb + Prog1_Informal + A2sg"));
            t.ExpectSingle("yazıyo", MatchesTailLex("Verb + Prog1_Informal + A3sg"));
            t.ExpectSingle("yazıyoz", MatchesTailLex("Verb + Prog1_Informal + A1pl"));
            t.ExpectSingle("yazıyosunuz", MatchesTailLex("Verb + Prog1_Informal + A2pl"));
            t.ExpectSingle("yazıyonuz", MatchesTailLex("Verb + Prog1_Informal + A2pl"));
            t.ExpectSingle("yazıyolar", MatchesTailLex("Verb + Prog1_Informal + A3pl"));

            t.ExpectSingle("yazıyosa", MatchesTailLex("Verb + Prog1_Informal + Cond + A3sg"));
            t.ExpectSingle("yazıyomuş", MatchesTailLex("Verb + Prog1_Informal + Narr + A3sg"));

            t = GetTester("bilmek [A:Aorist_I]");
            t.ExpectSingle("biliyosun", MatchesTailLex("Verb + Prog1_Informal + A2sg"));

            t.ExpectFail(
                "gitiyo",
                "gidyo"
            );
        }

        [TestMethod]
        public void TestProgressiveDeformation2()
        {
            AnalysisTester t = GetTester("okumak");
            
            t.ExpectSingle("okuyom", MatchesTailLex("Verb + Prog1_Informal + A1sg"));
            t.ExpectSingle("okuyon", MatchesTailLex("Verb + Prog1_Informal + A2sg"));
            t.ExpectSingle("okuyosun", MatchesTailLex("Verb + Prog1_Informal + A2sg"));
        }

        [TestMethod]
        public void ProgressiveDeformationDrop()
        {
            AnalysisTester t = GetTester("aramak");

            t.ExpectSingle("arıyom", MatchesTailLex("Verb + Prog1_Informal + A1sg"));
            t.ExpectSingle("arıyo", MatchesTailLex("Verb + Prog1_Informal + A3sg"));

            t.ExpectFail(
                "arayom",
                "ar",
                "ardım"
            );

            t = GetTester("yürümek");

            t.ExpectSingle("yürüyom", MatchesTailLex("Verb + Prog1_Informal + A1sg"));
            t.ExpectSingle("yürüyo", MatchesTailLex("Verb + Prog1_Informal + A3sg"));

            t = GetTester("denemek");

            t.ExpectSingle("deniyom", MatchesTailLex("Verb + Prog1_Informal + A1sg"));
            t.ExpectSingle("deniyo", MatchesTailLex("Verb + Prog1_Informal + A3sg"));
        }

        [TestMethod]
        public void ProgressiveDeformationNegative()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazmıyom", MatchesTailLex("Verb + Neg + Prog1_Informal + A1sg"));
            t.ExpectSingle("yazmıyosun", MatchesTailLex("Verb + Neg + Prog1_Informal + A2sg"));
            t.ExpectSingle("yazmıyo", MatchesTailLex("Verb + Neg + Prog1_Informal + A3sg"));

            t.ExpectSingle("yazamıyom", MatchesTailLex("Verb + Unable + Prog1_Informal + A1sg"));
            t.ExpectSingle("yazamıyosun", MatchesTailLex("Verb + Unable + Prog1_Informal + A2sg"));
            t.ExpectSingle("yazamıyo", MatchesTailLex("Verb + Unable + Prog1_Informal + A3sg"));

            t = GetTester("aramak");

            t.ExpectSingle("aramıyoz", MatchesTailLex("Verb + Neg + Prog1_Informal + A1pl"));
            t.ExpectSingle("aramıyosunuz", MatchesTailLex("Verb + Neg + Prog1_Informal + A2pl"));
            t.ExpectSingle("aramıyolar", MatchesTailLex("Verb + Neg + Prog1_Informal + A3pl"));
            
            t.ExpectSingle("arayamıyoz", MatchesTailLex("Verb + Unable + Prog1_Informal + A1pl"));
            t.ExpectSingle("arayamıyosunuz", MatchesTailLex("Verb + Unable + Prog1_Informal + A2pl"));
            t.ExpectSingle("arayamıyolar", MatchesTailLex("Verb + Unable + Prog1_Informal + A3pl"));

            t = GetTester("affetmek [A:Voicing]");
            t.ExpectSingle("affetmiyo", MatchesTailLex("Verb + Neg + Prog1_Informal + A3sg"));
            t.ExpectSingle("affedemiyo", MatchesTailLex("Verb + Unable + Prog1_Informal + A3sg"));
            
            t = GetTester("demek");
            t.ExpectSingle("diyo", MatchesTailLex("Verb + Prog1_Informal + A3sg"));
        }

        [TestMethod]
        public void OptativeP1plDeformation()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazak", MatchesTailLex("Verb + Opt + A1pl_Informal"));
            t.ExpectSingle("yazmayak", MatchesTailLex("Verb + Neg + Opt + A1pl_Informal"));
            t.ExpectSingle("yazamayak", MatchesTailLex("Verb + Unable + Opt + A1pl_Informal"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectSingle("edek", MatchesTailLex("Verb + Opt + A1pl_Informal"));

            t.ExpectAny("etmeyek", MatchesTailLex("Verb + Neg + Opt + A1pl_Informal"));
            t.ExpectAny("edemeyek", MatchesTailLex("Verb + Unable + Opt + A1pl_Informal"));
        }

        [TestMethod]
        public void FutureDeformation()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazacam", MatchesTailLex("Verb + Fut_Informal + A1sg"));
            t.ExpectAny("yazcam", MatchesTailLex("Verb + Fut_Informal + A1sg"));
            t.ExpectAny("yazıcam", MatchesTailLex("Verb + Fut_Informal + A1sg"));

            t.ExpectAny("yazmıycam", MatchesTailLex("Verb + Neg_Informal + Fut_Informal + A1sg"));
            t.ExpectAny("yazamıycam", MatchesTailLex("Verb + Unable_Informal + Fut_Informal + A1sg"));

            t = GetTester("eğlenmek");
            t.ExpectAny("eğlenicem", MatchesTailLex("Verb + Fut_Informal + A1sg"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectAny("edicem", MatchesTailLex("Verb + Fut_Informal + A1sg"));

        }

        [TestMethod]
        public void FutureDeformation2()
        {
            AnalysisTester t = GetTester("gitmek [A:Voicing]");
            t.ExpectAny("gidicem", MatchesTailLex("Verb + Fut_Informal + A1sg"));
        }


        [TestMethod]
        public void AsciiTolerant1()
        {
            AnalysisTester t = GetTesterAscii("eğlenmek");
            t.ExpectAny("eglenicem", MatchesTailLex("Verb + Fut_Informal + A1sg"));

            t = GetTesterAscii("etmek [A:Voicing]");
            t.ExpectAny("edıcem", MatchesTailLex("Verb + Fut_Informal + A1sg"));

        }
    }
}
