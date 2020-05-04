using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class PronounsTest : AnalyzerTestBase
    {
        [TestMethod]
        public void BenSenTest1()
        {
            AnalysisTester tester = GetTester("ben [P:Pron,Pers]");
            tester.ExpectSingle("ben", MatchesTailLex("Pron + A1sg"));
            tester.ExpectSingle("bana", MatchesTailLex("Pron + A1sg + Dat"));
            tester.ExpectSingle("beni", MatchesTailLex("Pron + A1sg + Acc"));
            tester.ExpectAny("benim", MatchesTailLex("Pron + A1sg + Gen"));
            tester.ExpectAny("benken", MatchesTailLex("Pron + A1sg + Zero + Verb + While + Adv"));
            tester.ExpectSingle("benimle", MatchesTailLex("Pron + A1sg + Ins"));

            tester.ExpectFail(
                "ban",
                "bene",
                "banı",
                "benin",
                "beniz",
                "benler"
            );

            tester = GetTester("sen [P:Pron,Pers]");
            tester.ExpectSingle("sen", MatchesTailLex("Pron + A2sg"));
            tester.ExpectSingle("sana", MatchesTailLex("Pron + A2sg + Dat"));
            tester.ExpectSingle("seni", MatchesTailLex("Pron + A2sg + Acc"));
            tester.ExpectSingle("senin", MatchesTailLex("Pron + A2sg + Gen"));
            tester.ExpectSingle("seninle", MatchesTailLex("Pron + A2sg + Ins"));

            tester.ExpectFail(
                "san",
                "sene",
                "sanı",
                "senler"
            );
        }

        [TestMethod]
        public void OTest1()
        {
            AnalysisTester tester = GetTester("o [P:Pron,Pers]");
            tester.ExpectSingle("o", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("ona", MatchesTailLex("Pron + A3sg + Dat"));
            tester.ExpectSingle("onu", MatchesTailLex("Pron + A3sg + Acc"));
            tester.ExpectSingle("onlar", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("onlara", MatchesTailLex("Pron + A3pl + Dat"));
            tester.ExpectSingle("onunla", MatchesTailLex("Pron + A3sg + Ins"));

        }

        [TestMethod]
        public void FalanFalancaTest1()
        {
            AnalysisTester tester = GetTester("falan [P:Pron,Pers]");
            tester.ExpectSingle("falan", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("falana", MatchesTailLex("Pron + A3sg + Dat"));
            tester.ExpectSingle("falanı", MatchesTailLex("Pron + A3sg + Acc"));
            tester.ExpectSingle("falanlar", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("falanlara", MatchesTailLex("Pron + A3pl + Dat"));

            tester = GetTester("falanca [P:Pron,Pers]");
            tester.ExpectSingle("falanca", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("falancaya", MatchesTailLex("Pron + A3sg + Dat"));
            tester.ExpectSingle("falancayı", MatchesTailLex("Pron + A3sg + Acc"));
            tester.ExpectSingle("falancalar", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("falancalara", MatchesTailLex("Pron + A3pl + Dat"));
        }

        [TestMethod]
        public void BizSizTest()
        {
            AnalysisTester tester = GetTester("biz [P:Pron,Pers]");
            tester.ExpectSingle("biz", MatchesTailLex("Pron + A1pl"));
            tester.ExpectSingle("bize", MatchesTailLex("Pron + A1pl + Dat"));
            tester.ExpectSingle("bizi", MatchesTailLex("Pron + A1pl + Acc"));
            tester.ExpectSingle("bizim", MatchesTailLex("Pron + A1pl + Gen"));
            tester.ExpectSingle("bizce", MatchesTailLex("Pron + A1pl + Equ"));
            tester.ExpectSingle("bizimle", MatchesTailLex("Pron + A1pl + Ins"));


            tester.ExpectFail(
                "bizin"
            );

            tester = GetTester("siz [P:Pron,Pers]");
            tester.ExpectSingle("siz", MatchesTailLex("Pron + A2pl"));
            tester.ExpectSingle("size", MatchesTailLex("Pron + A2pl + Dat"));
            tester.ExpectSingle("sizi", MatchesTailLex("Pron + A2pl + Acc"));
            tester.ExpectSingle("sizin", MatchesTailLex("Pron + A2pl + Gen"));
            tester.ExpectSingle("sizce", MatchesTailLex("Pron + A2pl + Equ"));
            tester.ExpectSingle("sizle", MatchesTailLex("Pron + A2pl + Ins"));
            tester.ExpectSingle("sizinle", MatchesTailLex("Pron + A2pl + Ins"));
        }

        [TestMethod]
        public void BuTest1()
        {
            AnalysisTester tester = GetTester("bu [P:Pron, Demons]");
            tester.ExpectSingle("bu", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("buna", MatchesTailLex("Pron + A3sg + Dat"));
            tester.ExpectSingle("bunu", MatchesTailLex("Pron + A3sg + Acc"));
            tester.ExpectSingle("bunlar", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("bunları", MatchesTailLex("Pron + A3pl + Acc"));
            tester.ExpectSingle("bununla", MatchesTailLex("Pron + A3sg + Ins"));


            tester.ExpectSingle("bunlaraymış",
                MatchesTailLex("Pron + A3pl + Dat + Zero + Verb + Narr + A3sg"));
        }

        [TestMethod]
        public void BiriTest1()
        {
            AnalysisTester tester = GetTester("biri [P:Pron,Quant]");
            // both are same
            tester.ExpectSingle("biri", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("birisi", MatchesTailLex("Pron + A3sg + P3sg"));
            // both are same
            tester.ExpectSingle("birine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("birisine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            // both are same
            tester.ExpectSingle("birini", MatchesTailLex("Pron + A3sg + P3sg + Acc"));
            tester.ExpectSingle("birisini", MatchesTailLex("Pron + A3sg + P3sg + Acc"));

            tester.ExpectSingle("birimiz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("biriniz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("birileri", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("birilerine", MatchesTailLex("Pron + A3pl + P3pl + Dat"));
            tester.ExpectSingle("birilerini", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            tester.ExpectFail(
                "biriler",
                "birilerim"
            );
        }

        [TestMethod]
        public void HerbiriTest1()
        {
            AnalysisTester tester = GetTester("herbiri [P:Pron,Quant]");
            // both are same
            tester.ExpectSingle("herbiri", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("herbirisi", MatchesTailLex("Pron + A3sg + P3sg"));
            // both are same
            tester.ExpectSingle("herbirine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("herbirisine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            // both are same

            tester.ExpectSingle("herbirimiz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("herbiriniz", MatchesTailLex("Pron + A2pl + P2pl"));

            tester.ExpectSingle("herbirini", MatchesTailLex("Pron + A3sg + P3sg + Acc"));
            tester.ExpectSingle("herbirisini", MatchesTailLex("Pron + A3sg + P3sg + Acc"));

            tester.ExpectFail(
                "herbiriler",
                "herbirileri",
                "herbirilerine",
                "herbirilerim",
                "herbirilerin"
            );
        }

        [TestMethod]
        public void HerkesTest()
        {
            AnalysisTester tester = GetTester("herkes [P:Pron,Quant]");

            tester.ExpectSingle("herkes", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("herkese", MatchesTailLex("Pron + A3pl + Dat"));
            tester.ExpectSingle("herkesi", MatchesTailLex("Pron + A3pl + Acc"));
        }

        [TestMethod]
        public void umumTest()
        {
            AnalysisTester tester = GetTester("umum [P:Pron,Quant]");

            tester.ExpectSingle("umum", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("umuma", MatchesTailLex("Pron + A3pl + Dat"));
            tester.ExpectSingle("umumu", MatchesTailLex("Pron + A3pl + Acc"));

            tester.ExpectFail(
                "umumlar"
            );
        }

        [TestMethod]
        public void BirbiriTest()
        {
            AnalysisTester tester = GetTester("birbiri [P:Pron,Quant]");

            tester.ExpectSingle("birbiri", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("birbirine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("birbirimiz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("birbiriniz", MatchesTailLex("Pron + A2pl + P2pl"));

            tester.ExpectSingle("birbirileri", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("birbirleri", MatchesTailLex("Pron + A3pl + P3pl"));

            tester.ExpectFail(
                "birbir",
                "birbire",
                "birbirler",
                "birbiriler",
                "birbirlere"
            );
        }

        [TestMethod]
        public void HepTest()
        {
            AnalysisTester tester = GetTester("hep [P:Pron,Quant]");

            tester.ExpectSingle("hepimiz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("hepimize", MatchesTailLex("Pron + A1pl + P1pl + Dat"));
            tester.ExpectSingle("hepiniz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("hepinizi", MatchesTailLex("Pron + A2pl + P2pl + Acc"));

            tester.ExpectFail(
                "hep", // only [hep+Adv] is allowed.
                "hepler",
                "hepleri",
                "hepe",
                "hepim"
            );
        }

        [TestMethod]
        public void TumuTest()
        {
            AnalysisTester tester = GetTester("tümü [P:Pron,Quant]");

            tester.ExpectSingle("tümü", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("tümümüz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("tümümüze", MatchesTailLex("Pron + A1pl + P1pl + Dat"));
            tester.ExpectSingle("tümünüz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("tümünüzü", MatchesTailLex("Pron + A2pl + P2pl + Acc"));

            tester.ExpectFail(
                "tümler",
                "tümüler",
                "tümleri",
                "tümüleri",
                "tümüm"
            );
        }

        [TestMethod]
        public void TopuTest()
        {
            AnalysisTester tester = GetTester("topu [P:Pron,Quant]");

            tester.ExpectSingle("topu", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("topumuz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("topumuza", MatchesTailLex("Pron + A1pl + P1pl + Dat"));
            tester.ExpectSingle("topunuz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("topunuzu", MatchesTailLex("Pron + A2pl + P2pl + Acc"));

            tester.ExpectFail(
                "topular",
                "topuları",
                "topum" // no Pron analysis.
            );
        }

        [TestMethod]
        public void BirkaciTest()
        {
            AnalysisTester tester = GetTester("birkaçı [P:Pron,Quant]");

            tester.ExpectSingle("birkaçı", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("birkaçımız", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("birkaçımıza", MatchesTailLex("Pron + A1pl + P1pl + Dat"));
            tester.ExpectSingle("birkaçınız", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("birkaçınızı", MatchesTailLex("Pron + A2pl + P2pl + Acc"));

            tester.ExpectFail(
                "birkaçılar",
                "birkaçlar",
                "birkaçım"
            );
        }


        [TestMethod]
        public void HepsiTest()
        {
            AnalysisTester tester = GetTester("hepsi [P:Pron,Quant]");

            tester.ExpectSingle("hepsi", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("hepsine", MatchesTailLex("Pron + A3pl + P3pl + Dat"));
            tester.ExpectSingle("hepsini", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            tester.ExpectFail(
                "hepsiler",
                "hepsim",
                "hepsin",
                "hepsisi",
                "hepsimiz",
                "hepsiniz",
                "hepsileri"
            );
        }

        [TestMethod]
        public void CumlesiTest()
        {
            AnalysisTester tester = GetTester("cümlesi [P:Pron,Quant]");
            tester.ExpectSingle("cümlesi", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("cümlesine", MatchesTailLex("Pron + A3pl + P3pl + Dat"));
            tester.ExpectSingle("cümlesini", MatchesTailLex("Pron + A3pl + P3pl + Acc"));
        }

        [TestMethod]
        public void KimiTest()
        {
            AnalysisTester tester = GetTester("kimi [P:Pron,Quant]");

            tester.ExpectSingle("kimi", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("kimimiz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("kiminiz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("kimileri", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("kimine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("kimisi", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("kimimize", MatchesTailLex("Pron + A1pl + P1pl + Dat"));

            tester.ExpectFail(
                "kimiler",
                "kimim",
                "kimin"
            );
        }

        [TestMethod]
        public void CoguTest()
        {
            AnalysisTester tester = GetTester("çoğu [P:Pron,Quant]");
            tester.ExpectSingle("çoğu", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("çoğumuz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("çoğunuz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("çokları", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("çoğuna", MatchesTailLex("Pron + A3pl + P3pl + Dat"));
            tester.ExpectSingle("çoklarını", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            tester.ExpectFail(
                "çoğular",
                "çokumuz",
                "çoğum"
            );
        }

        [TestMethod]
        public void BaziTest()
        {
            AnalysisTester tester = GetTester("bazı [P:Pron,Quant]");

            tester.ExpectSingle("bazımız", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("bazınız", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("bazıları", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("bazısına", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("bazımızdan", MatchesTailLex("Pron + A1pl + P1pl + Abl"));
            tester.ExpectSingle("bazınıza", MatchesTailLex("Pron + A2pl + P2pl + Dat"));
            tester.ExpectSingle("bazılarımızdan", MatchesTailLex("Pron + A1pl + P1pl + Abl"));
            tester.ExpectSingle("bazılarını", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            tester.ExpectFail(
                "bazı",// oflazer does not solve this for Pron+Quant
                "bazına",
                "bazım",
                "bazın",
                "bazılar"
            );
        }

        [TestMethod]
        public void BircoguTest()
        {
            AnalysisTester tester = GetTester("birçoğu [P:Pron,Quant]");
            tester.ExpectSingle("birçoğu", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("birçoğumuz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("birçoğunuz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("birçokları", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("birçoğuna", MatchesTailLex("Pron + A3pl + P3pl + Dat"));
            tester.ExpectSingle("birçoklarını", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            tester.ExpectFail(
                "birçoğular",
                "birçokumuz",
                "birçoğum"
            );
        }

        [TestMethod]
        public void HicbiriTest()
        {
            AnalysisTester tester = GetTester("hiçbiri [P:Pron,Quant]");
            // both are same
            tester.ExpectSingle("hiçbiri", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("hiçbirisi", MatchesTailLex("Pron + A3sg + P3sg"));
            // both are same
            tester.ExpectSingle("hiçbirine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("hiçbirisine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            // both are same
            tester.ExpectSingle("hiçbirini", MatchesTailLex("Pron + A3sg + P3sg + Acc"));
            tester.ExpectSingle("hiçbirisini", MatchesTailLex("Pron + A3sg + P3sg + Acc"));

            tester.ExpectSingle("hiçbirimiz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("hiçbiriniz", MatchesTailLex("Pron + A2pl + P2pl"));

            tester.ExpectFail(
                "hiçbiriler",
                "hiçbirileri",
                "hiçbirilerine",
                "hiçbirilerim"
            );
        }

        [TestMethod]
        public void OburuTest()
        {
            AnalysisTester tester = GetTester("öbürü [P:Pron,Quant]");
            tester.ExpectSingle("öbürü", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("öbürüne", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("öbürünü", MatchesTailLex("Pron + A3sg + P3sg + Acc"));
            tester.ExpectSingle("öbürleri", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("öbürlerini", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            tester.ExpectFail(
                "öbürüler",
                "öbürümüz",
                "öbürünüz",
                "öbürün",
                "öbürüm",
                "öbürüleri",
                "öbürülerine",
                "öbürülerim"
            );
        }

        [TestMethod]
        public void OburkuTest()
        {
            AnalysisTester tester = GetTester("öbürkü [P:Pron,Quant]");
            tester.ExpectSingle("öbürkü", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("öbürküne", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("öbürkünü", MatchesTailLex("Pron + A3sg + P3sg + Acc"));
            tester.ExpectSingle("öbürküler", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("öbürkülerini", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            // Multiple solutions for öbürküleri
            tester.ExpectAny("öbürküleri", MatchesTailLex("Pron + A3pl + Acc"));
            tester.ExpectAny("öbürküleri", MatchesTailLex("Pron + A3pl + P3pl"));

            tester.ExpectFail(
                "öbürkümüz",
                "öbürkünüz",
                "öbürkün",
                "öbürküm"
            );
        }

        [TestMethod]
        public void BerikiTest()
        {
            AnalysisTester tester = GetTester("beriki [P:Pron,Quant]");
            tester.ExpectSingle("beriki", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("berikine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectSingle("berikini", MatchesTailLex("Pron + A3sg + P3sg + Acc"));
            tester.ExpectSingle("berikiler", MatchesTailLex("Pron + A3pl"));
            tester.ExpectSingle("berikilerini", MatchesTailLex("Pron + A3pl + P3pl + Acc"));

            // Multiple solutions for berikileri
            tester.ExpectAny("berikileri", MatchesTailLex("Pron + A3pl + Acc"));
            tester.ExpectAny("berikileri", MatchesTailLex("Pron + A3pl + P3pl"));

            tester.ExpectFail(
                "berikimiz",
                "berikiniz",
                "berikin",
                "berikim"
            );
        }

        [TestMethod]
        public void KimseQuantTest()
        {
            AnalysisTester tester = GetTester("kimse [P:Pron,Quant]");

            tester.ExpectSingle("kimse", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("kimsem", MatchesTailLex("Pron + A3sg + P1sg"));
            tester.ExpectSingle("kimseye", MatchesTailLex("Pron + A3sg + Dat"));

            // two analysis.
            tester.ExpectAny("kimseler", MatchesTailLex("Pron + A3pl"));
        }

        [TestMethod]
        public void KimTest()
        {
            AnalysisTester tester = GetTester("kim [P:Pron,Ques]");

            tester.ExpectSingle("kim", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("kime", MatchesTailLex("Pron + A3sg + Dat"));
            tester.ExpectSingle("kimlere", MatchesTailLex("Pron + A3pl + Dat"));
            tester.ExpectSingle("kimimiz", MatchesTailLex("Pron + A3sg + P1pl"));
            tester.ExpectSingle("kiminle", MatchesTailLex("Pron + A3sg + P2sg + Ins"));
            tester.ExpectSingle("kimimize", MatchesTailLex("Pron + A3sg + P1pl + Dat"));
            tester.ExpectSingle("kimsiniz",
                MatchesTailLex("Pron + A3sg + Zero + Verb + Pres + A2pl"));

            tester.ExpectAny("kimim", MatchesTailLex("Pron + A3sg + P1sg"));
            tester.ExpectAny("kimler", MatchesTailLex("Pron + A3pl"));
            tester.ExpectAny("kimi", MatchesTailLex("Pron + A3sg + Acc"));
        }

        [TestMethod]
        public void NeTest()
        {
            AnalysisTester tester = GetTester("ne [P:Pron,Ques]");

            tester.ExpectSingle("neyimiz", MatchesTailLex("Pron + A3sg + P1pl"));
            tester.ExpectSingle("ne", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("neye", MatchesTailLex("Pron + A3sg + Dat"));
            tester.ExpectSingle("nelere", MatchesTailLex("Pron + A3pl + Dat"));
            tester.ExpectSingle("neyimize", MatchesTailLex("Pron + A3sg + P1pl + Dat"));

            tester.ExpectAny("neler", MatchesTailLex("Pron + A3pl"));
            tester.ExpectAny("neyim", MatchesTailLex("Pron + A3sg + P1sg"));
            tester.ExpectAny("neyi", MatchesTailLex("Pron + A3sg + Acc"));
        }

        [TestMethod]
        public void NereTest()
        {
            AnalysisTester tester = GetTester("nere [P:Pron,Ques]");

            tester.ExpectSingle("nere", MatchesTailLex("Pron + A3sg"));
            tester.ExpectSingle("nereye", MatchesTailLex("Pron + A3sg + Dat"));
            tester.ExpectSingle("nerelere", MatchesTailLex("Pron + A3pl + Dat"));
            tester.ExpectSingle("nerem", MatchesTailLex("Pron + A3sg + P1sg"));
            tester.ExpectSingle("neremiz", MatchesTailLex("Pron + A3sg + P1pl"));
            tester.ExpectSingle("neremize", MatchesTailLex("Pron + A3sg + P1pl + Dat"));

            // TODO: consider below. For now it does not pass. Oflazer accepts.
            //tester.ExpectSingle("nereyim", MatchesTailLex("Pron + A3sg + Zero + Verb + Pres + A1sg"));

            tester.ExpectAny("nereler", MatchesTailLex("Pron + A3pl"));
            tester.ExpectAny("nereyi", MatchesTailLex("Pron + A3sg + Acc"));
            tester.ExpectAny("nereli", MatchesTailLex("Pron + A3sg + With + Adj"));
        }

        [TestMethod]
        public void KendiTest()
        {
            AnalysisTester tester = GetTester("kendi [P:Pron,Reflex]");

            tester.ExpectSingle("kendi", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("kendileri", MatchesTailLex("Pron + A3pl + P3pl"));
            tester.ExpectSingle("kendilerine", MatchesTailLex("Pron + A3pl + P3pl + Dat"));
            tester.ExpectSingle("kendim", MatchesTailLex("Pron + A1sg + P1sg"));
            tester.ExpectSingle("kendin", MatchesTailLex("Pron + A2sg + P2sg"));
            tester.ExpectSingle("kendisi", MatchesTailLex("Pron + A3sg + P3sg"));
            tester.ExpectSingle("kendimiz", MatchesTailLex("Pron + A1pl + P1pl"));
            tester.ExpectSingle("kendiniz", MatchesTailLex("Pron + A2pl + P2pl"));
            tester.ExpectSingle("kendisiyle", MatchesTailLex("Pron + A3sg + P3sg + Ins"));
            tester.ExpectSingle("kendimizle", MatchesTailLex("Pron + A1pl + P1pl + Ins"));

            // kendine has 2 analyses
            tester.ExpectAny("kendine", MatchesTailLex("Pron + A3sg + P3sg + Dat"));
            tester.ExpectAny("kendine", MatchesTailLex("Pron + A2sg + P2sg + Dat"));
        }

        /**
         * Test for issues
         * <a href="https://github.com/ahmetaa/zemberek-nlp/issues/171">171</a>
         * <a href="https://github.com/ahmetaa/zemberek-nlp/issues/172">172</a>
         */
        [TestMethod]
        public void KendiTest_issues_171_172()
        {
            AnalysisTester tester = GetTester("kendi [P:Pron,Reflex]");

            tester.ExpectSingle("kendime", MatchesTailLex("Pron + A1sg + P1sg + Dat"));
            tester.ExpectSingle("kendimde", MatchesTailLex("Pron + A1sg + P1sg + Loc"));
            tester.ExpectSingle("kendimden", MatchesTailLex("Pron + A1sg + P1sg + Abl"));
            tester.ExpectSingle("kendimce", MatchesTailLex("Pron + A1sg + P1sg + Equ"));
            tester.ExpectSingle("kendimle", MatchesTailLex("Pron + A1sg + P1sg + Ins"));

            // These also have A3sg analyses.
            tester.ExpectAny("kendine", MatchesTailLex("Pron + A2sg + P2sg + Dat"));
            tester.ExpectAny("kendinde", MatchesTailLex("Pron + A2sg + P2sg + Loc"));
            tester.ExpectAny("kendinden", MatchesTailLex("Pron + A2sg + P2sg + Abl"));
            tester.ExpectAny("kendince", MatchesTailLex("Pron + A2sg + P2sg + Equ"));
            tester.ExpectAny("kendinle", MatchesTailLex("Pron + A2sg + P2sg + Ins"));

        }

        /**
         * Test for issue
         * <a href="https://github.com/ahmetaa/zemberek-nlp/issues/178">178</a>
         */
        [TestMethod]
        public void HerkesteTest_issue_178()
        {
            AnalysisTester tester = GetTester("herkes [P:Pron,Quant]");

            tester.ExpectSingle("herkese", MatchesTailLex("Pron + A3pl + Dat"));
            tester.ExpectSingle("herkeste", MatchesTailLex("Pron + A3pl + Loc"));
            tester.ExpectSingle("herkesten", MatchesTailLex("Pron + A3pl + Abl"));
            tester.ExpectSingle("herkesçe", MatchesTailLex("Pron + A3pl + Equ"));
            tester.ExpectSingle("herkesle", MatchesTailLex("Pron + A3pl + Ins"));
        }

        /**
         * Test for issue
         * <a href="https://github.com/ahmetaa/zemberek-nlp/issues/188">178</a>
         * Cannot analyze sendeki, bendeki etc.
         */
        [TestMethod]
        public void SendekiTest_issue_188()
        {
            AnalysisTester tester = GetTester("ben [P:Pron,Pers]",
                "sen [P:Pron,Pers]",
                "o [P:Pron,Pers]",
                "biz [P:Pron,Pers]",
                "siz [P:Pron,Pers]"
            );

            tester.ExpectSingle("bendeki", MatchesTailLex("Pron + A1sg + Loc + Rel + Adj"));
            tester.ExpectSingle("sendeki", MatchesTailLex("Pron + A2sg + Loc + Rel + Adj"));
            tester.ExpectSingle("ondaki", MatchesTailLex("Pron + A3sg + Loc + Rel + Adj"));
            tester.ExpectSingle("bizdeki", MatchesTailLex("Pron + A1pl + Loc + Rel + Adj"));
            tester.ExpectSingle("sizdeki", MatchesTailLex("Pron + A2pl + Loc + Rel + Adj"));
            tester.ExpectSingle("onlardaki", MatchesTailLex("Pron + A3pl + Loc + Rel + Adj"));
        }
    }
}
