using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class AnalysisFormatterTest : AnalyzerTestBase
    {
        [TestMethod]
        public void DefaultSurfaceFormatterTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            SingleAnalysis analysis = analyzer.Analyze("kitaplarda")[0];

            Assert.AreEqual("[kitap:Noun] kitap:Noun+lar:A3pl+da:Loc",
                AnalysisFormatters.Default.Format(analysis));

            analysis = analyzer.Analyze("kitapsız")[0];
            Assert.AreEqual("[kitap:Noun] kitap:Noun+A3sg|sız:Without→Adj",
                AnalysisFormatters.Default.Format(analysis));

            analysis = analyzer.Analyze("kitaplardaymış")[0];
            Assert.AreEqual("[kitap:Noun] kitap:Noun+lar:A3pl+da:Loc|Zero→Verb+ymış:Narr+A3sg",
                AnalysisFormatters.Default.Format(analysis));

            analyzer = GetAnalyzer("okumak");
            analysis = analyzer.Analyze("okut")[0];
            Assert.AreEqual("[okumak:Verb] oku:Verb|t:Caus→Verb+Imp+A2sg",
                AnalysisFormatters.Default.Format(analysis));
        }

        [TestMethod]
        public void DefaultLexicalFormatterTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            SingleAnalysis analysis = analyzer.Analyze("kitaplarda")[0];

            Assert.AreEqual("[kitap:Noun] Noun+A3pl+Loc",
                AnalysisFormatters.DefaultLexical.Format(analysis));

            analysis = analyzer.Analyze("kitapsız")[0];
            Assert.AreEqual("[kitap:Noun] Noun+A3sg|Without→Adj",
                AnalysisFormatters.DefaultLexical.Format(analysis));

            analysis = analyzer.Analyze("kitaplardaymış")[0];
            Assert.AreEqual("[kitap:Noun] Noun+A3pl+Loc|Zero→Verb+Narr+A3sg",
                AnalysisFormatters.DefaultLexical.Format(analysis));

            analyzer = GetAnalyzer("okumak");
            analysis = analyzer.Analyze("okut")[0];
            Assert.AreEqual("[okumak:Verb] Verb|Caus→Verb+Imp+A2sg",
                AnalysisFormatters.DefaultLexical.Format(analysis));
        }

        [TestMethod]
        public void OflazerStyleFormatterTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            SingleAnalysis analysis = analyzer.Analyze("kitaplarda")[0];

            Assert.AreEqual("kitap+Noun+A3pl+Loc",
                AnalysisFormatters.OflazerStyle.Format(analysis));

            analysis = analyzer.Analyze("kitapsız")[0];
            Assert.AreEqual("kitap+Noun+A3sg^DB+Adj+Without",
                AnalysisFormatters.OflazerStyle.Format(analysis));

            analysis = analyzer.Analyze("kitaplardaymış")[0];
            Assert.AreEqual("kitap+Noun+A3pl+Loc^DB+Verb+Zero+Narr+A3sg",
                AnalysisFormatters.OflazerStyle.Format(analysis));

            analyzer = GetAnalyzer("okumak");
            analysis = analyzer.Analyze("okut")[0];
            Assert.AreEqual("oku+Verb^DB+Verb+Caus+Imp+A2sg",
                AnalysisFormatters.OflazerStyle.Format(analysis));

            analyzer = GetAnalyzer("Ankara");
            analysis = analyzer.Analyze("ankara")[0];
            Assert.AreEqual("ankara+Noun+Prop+A3sg",
                AnalysisFormatters.OflazerStyle.Format(analysis));
        }

        [TestMethod]
        public void OnlySurfaceFormatterTest()
        {
            RuleBasedAnalyzer analyzer = GetAnalyzer("kitap");
            SingleAnalysis analysis = analyzer.Analyze("kitaplarda")[0];

            Assert.AreEqual("kitap lar da",
                AnalysisFormatters.SurfaceSequence.Format(analysis));

            analysis = analyzer.Analyze("kitapsız")[0];
            Assert.AreEqual("kitap sız",
                AnalysisFormatters.SurfaceSequence.Format(analysis));

            analysis = analyzer.Analyze("kitaplardaymış")[0];
            Assert.AreEqual("kitap lar da ymış",
                AnalysisFormatters.SurfaceSequence.Format(analysis));

            analyzer = GetAnalyzer("okumak");
            analysis = analyzer.Analyze("okut")[0];
            Assert.AreEqual("oku t",
                AnalysisFormatters.SurfaceSequence.Format(analysis));

            analyzer = GetAnalyzer("Ankara");
            analysis = analyzer.Analyze("ankara")[0];
            Assert.AreEqual("ankara",
                AnalysisFormatters.SurfaceSequence.Format(analysis));
        }
    }
}
