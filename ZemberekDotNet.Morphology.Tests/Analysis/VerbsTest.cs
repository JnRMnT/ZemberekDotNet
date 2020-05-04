using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class VerbsTest : AnalyzerTestBase
    {
        [TestMethod]
        public void Imp()
        {
            AnalysisTester t = GetTester("okumak");

            t.ExpectSingle("oku", MatchesTailLex("Verb + Imp + A2sg"));
            t.ExpectSingle("okusana", MatchesTailLex("Verb + Imp + A2sg"));
            t.ExpectSingle("okusun", MatchesTailLex("Verb + Imp + A3sg"));
            t.ExpectSingle("okuyun", MatchesTailLex("Verb + Imp + A2pl"));
            t.ExpectSingle("okuyunuz", MatchesTailLex("Verb + Imp + A2pl"));
            t.ExpectSingle("okusanıza", MatchesTailLex("Verb + Imp + A2pl"));
            t.ExpectSingle("okusunlar", MatchesTailLex("Verb + Imp + A3pl"));

            t = GetTester("yazmak");

            t.ExpectSingle("yaz", MatchesTailLex("Verb + Imp + A2sg"));
            t.ExpectSingle("yazsın", MatchesTailLex("Verb + Imp + A3sg"));
            t.ExpectSingle("yazın", MatchesTailLex("Verb + Imp + A2pl"));
            t.ExpectSingle("yazınız", MatchesTailLex("Verb + Imp + A2pl"));
            t.ExpectSingle("yazsınlar", MatchesTailLex("Verb + Imp + A3pl"));
        }

        [TestMethod]
        public void ImpNeg()
        {
            AnalysisTester t = GetTester("okumak");

            t.ExpectAny("okuma", MatchesTailLex("Verb + Neg + Imp + A2sg"));
            t.ExpectAny("okumasın", MatchesTailLex("Verb + Neg + Imp + A3sg"));
            t.ExpectSingle("okumayın", MatchesTailLex("Verb + Neg + Imp + A2pl"));
            t.ExpectSingle("okumayınız", MatchesTailLex("Verb + Neg + Imp + A2pl"));
            t.ExpectSingle("okumasınlar", MatchesTailLex("Verb + Neg + Imp + A3pl"));
        }


        [TestMethod]
        public void ProgressivePositive()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazıyorum", MatchesTailLex("Verb + Prog1 + A1sg"));
            t.ExpectSingle("yazıyorsun", MatchesTailLex("Verb + Prog1 + A2sg"));
            t.ExpectSingle("yazıyor", MatchesTailLex("Verb + Prog1 + A3sg"));
            t.ExpectSingle("yazıyoruz", MatchesTailLex("Verb + Prog1 + A1pl"));
            t.ExpectSingle("yazıyorsunuz", MatchesTailLex("Verb + Prog1 + A2pl"));
            t.ExpectSingle("yazıyorlar", MatchesTailLex("Verb + Prog1 + A3pl"));

            t = GetTester("gitmek [A:Voicing]");
            t.ExpectSingle("gidiyorum", MatchesTailLex("Verb + Prog1 + A1sg"));
            t.ExpectSingle("gidiyorsun", MatchesTailLex("Verb + Prog1 + A2sg"));
            t.ExpectSingle("gidiyor", MatchesTailLex("Verb + Prog1 + A3sg"));

            t = GetTester("bilmek [A:Aorist_I]");
            t.ExpectSingle("biliyorsun", MatchesTailLex("Verb + Prog1 + A2sg"));

            t.ExpectFail(
                "gitiyor",
                "gidyor"
            );
        }

        [TestMethod]
        public void ProgressivePositive2()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazıyordum", MatchesTailLex("Verb + Prog1 + Past + A1sg"));
            t.ExpectSingle("yazıyorsan", MatchesTailLex("Verb + Prog1 + Cond + A2sg"));
            t.ExpectSingle("yazıyormuş", MatchesTailLex("Verb + Prog1 + Narr + A3sg"));
            t.ExpectSingle("yazıyorduk", MatchesTailLex("Verb + Prog1 + Past + A1pl"));

            t.ExpectFail(
                "yazıyormuşsak"
            );
        }

        [TestMethod]
        public void ProgressiveDrop()
        {
            AnalysisTester t = GetTester("aramak");

            t.ExpectSingle("arıyorum", MatchesTailLex("Verb + Prog1 + A1sg"));
            t.ExpectSingle("arıyor", MatchesTailLex("Verb + Prog1 + A3sg"));

            t.ExpectFail(
                "arayorum",
                "ar",
                "ardım"
            );

            t = GetTester("yürümek");

            t.ExpectSingle("yürüyorum", MatchesTailLex("Verb + Prog1 + A1sg"));
            t.ExpectSingle("yürüyor", MatchesTailLex("Verb + Prog1 + A3sg"));

            t = GetTester("denemek");

            t.ExpectSingle("deniyorum", MatchesTailLex("Verb + Prog1 + A1sg"));
            t.ExpectSingle("deniyor", MatchesTailLex("Verb + Prog1 + A3sg"));
        }

        [TestMethod]
        public void ProgressiveNegative()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazmıyorum", MatchesTailLex("Verb + Neg + Prog1 + A1sg"));
            t.ExpectSingle("yazmıyorsun", MatchesTailLex("Verb + Neg + Prog1 + A2sg"));
            t.ExpectSingle("yazmıyor", MatchesTailLex("Verb + Neg + Prog1 + A3sg"));

            t.ExpectFail(
                "yazmayorum",
                "yazm",
                "yazmz"
            );

            t = GetTester("aramak");

            t.ExpectSingle("aramıyoruz", MatchesTailLex("Verb + Neg + Prog1 + A1pl"));
            t.ExpectSingle("aramıyorsunuz", MatchesTailLex("Verb + Neg + Prog1 + A2pl"));
            t.ExpectSingle("aramıyorlar", MatchesTailLex("Verb + Neg + Prog1 + A3pl"));

            t.ExpectFail(
                "aramayoruz",
                "armıyoruz",
                "armıyor"
            );

            t = GetTester("affetmek [A:Voicing]");
            t.ExpectSingle("affetmiyor", MatchesTailLex("Verb + Neg + Prog1 + A3sg"));

        }

        [TestMethod]
        public void ProgressiveNegative2()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazmıyordum", MatchesTailLex("Verb + Neg + Prog1 + Past + A1sg"));
            t.ExpectSingle("yazmıyorsan", MatchesTailLex("Verb + Neg + Prog1 + Cond + A2sg"));
            t.ExpectSingle("yazmıyormuş", MatchesTailLex("Verb + Neg + Prog1 + Narr + A3sg"));
            t.ExpectSingle("yazmıyorduk", MatchesTailLex("Verb + Neg + Prog1 + Past + A1pl"));

            t = GetTester("aramak");

            t.ExpectSingle("aramıyorduk", MatchesTailLex("Verb + Neg + Prog1 + Past + A1pl"));
            t.ExpectSingle("aramıyorsam", MatchesTailLex("Verb + Neg + Prog1 + Cond + A1sg"));
            t.ExpectSingle("aramıyormuşuz", MatchesTailLex("Verb + Neg + Prog1 + Narr + A1pl"));
        }

        [TestMethod]
        public void Aorist()
        {
            AnalysisTester t = GetTester("yazmak"); // Aorist_A attribute is inferred.

            t.ExpectSingle("yazarım", MatchesTailLex("Verb + Aor + A1sg"));
            t.ExpectSingle("yazarsın", MatchesTailLex("Verb + Aor + A2sg"));
            t.ExpectAny("yazar", MatchesTailLex("Verb + Aor + A3sg"));
            t.ExpectAny("yazarız", MatchesTailLex("Verb + Aor + A1pl"));
            t.ExpectSingle("yazarlar", MatchesTailLex("Verb + Aor + A3pl"));

            t.ExpectAny("yazdırır",
                MatchesTailLex("Verb + Caus + Verb + Aor + A3sg"));
            t.ExpectAny("yazdırtır",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Aor + A3sg"));
            t.ExpectAny("yazdırttırır",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Caus + Verb + Aor + A3sg"));

            t = GetTester("semirmek");
            t.ExpectSingle("semiririm", MatchesTailLex("Verb + Aor + A1sg"));
            t.ExpectSingle("semirirsin", MatchesTailLex("Verb + Aor + A2sg"));
            t.ExpectAny("semirir", MatchesTailLex("Verb + Aor + A3sg"));

            t.ExpectSingle("semirtirim",
                MatchesTailLex("Verb + Caus + Verb + Aor + A1sg"));
            t.ExpectSingle("semirttiririm",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Aor + A1sg"));
        }

        [TestMethod]
        public void Aorist2()
        {
            AnalysisTester t = GetTester("yazmak"); // Aorist_A attribute is inferred.

            t.ExpectSingle("yazardım", MatchesTailLex("Verb + Aor + Past + A1sg"));
            t.ExpectSingle("yazmazdım", MatchesTailLex("Verb + Neg + Aor + Past + A1sg"));
            t.ExpectSingle("yazardık", MatchesTailLex("Verb + Aor + Past + A1pl"));
            t.ExpectSingle("yazarmışsın", MatchesTailLex("Verb + Aor + Narr + A2sg"));
            t.ExpectSingle("yazarsa", MatchesTailLex("Verb + Aor + Cond + A3sg"));
            t.ExpectSingle("yazmazsa", MatchesTailLex("Verb + Neg + Aor + Cond + A3sg"));
            t.ExpectSingle("yazmazmışız", MatchesTailLex("Verb + Neg + Aor + Narr + A1pl"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectAny("eder", MatchesTailLex("Verb + Aor + A3sg"));
            t.ExpectSingle("edermiş", MatchesTailLex("Verb + Aor + Narr + A3sg"));
            t.ExpectSingle("etmezmiş", MatchesTailLex("Verb + Neg + Aor + Narr + A3sg"));
            t.ExpectSingle("ederdik", MatchesTailLex("Verb + Aor + Past + A1pl"));
            t.ExpectSingle("etmezsek", MatchesTailLex("Verb + Neg + Aor + Cond + A1pl"));
        }

        [TestMethod]
        public void AoristNegative()
        {
            AnalysisTester t = GetTester("yazmak"); // Aorist_A attribute is inferred.

            t.ExpectAny("yazmam", MatchesTailLex("Verb + Neg + Aor + A1sg"));
            t.ExpectAny("yazmam", MatchesTailLex("Verb + Inf2 + Noun + A3sg + P1sg"));
            t.ExpectSingle("yazmazsın", MatchesTailLex("Verb + Neg + Aor + A2sg"));
            t.ExpectAny("yazmaz", MatchesTailLex("Verb + Neg + Aor + A3sg"));
            t.ExpectAny("yazmayız", MatchesTailLex("Verb + Neg + Aor + A1pl"));
            t.ExpectSingle("yazmazsınız", MatchesTailLex("Verb + Neg + Aor + A2pl"));
            t.ExpectSingle("yazmazlar", MatchesTailLex("Verb + Neg + Aor + A3pl"));

            t.ExpectAny("yazdırmaz",
                MatchesTailLex("Verb + Caus + Verb + Neg + Aor + A3sg"));
            t.ExpectAny("yazdırtmaz",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Neg + Aor + A3sg"));
            t.ExpectSingle("yazdırttırmazsınız",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Caus + Verb + Neg + Aor + A2pl"));

            t = GetTester("semirmek");
            t.ExpectAny("semirmem", MatchesTailLex("Verb + Neg + Aor + A1sg"));
            t.ExpectSingle("semirmezsin", MatchesTailLex("Verb + Neg + Aor + A2sg"));
            t.ExpectAny("semirmez", MatchesTailLex("Verb + Neg + Aor + A3sg"));

            t.ExpectAny("semirtmem",
                MatchesTailLex("Verb + Caus + Verb + Neg + Aor + A1sg"));
            t.ExpectAny("semirttirmem",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Neg + Aor + A1sg"));
        }

        [TestMethod]
        public void AbilityPositive()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazabil", MatchesTailLex("Verb + Able + Verb + Imp + A2sg"));
            t.ExpectSingle("yazabiliyor", MatchesTailLex("Verb + Able + Verb + Prog1 + A3sg"));

            t.ExpectFail(
                "yazabildir",
                "yazabilebil"
            );

            t = GetTester("okumak");

            t.ExpectSingle("okuyabil", MatchesTailLex("Verb + Able + Verb + Imp + A2sg"));
            t.ExpectAny("okuyabilir", MatchesTailLex("Verb + Able + Verb + Aor + A3sg"));
        }

        [TestMethod]
        public void AbilityAfterCausative()
        {

            AnalysisTester t = GetTester("okumak");

            t.ExpectSingle("okutabil",
                MatchesTailLex("Verb + Caus + Verb + Able + Verb + Imp + A2sg"));
            t.ExpectSingle("okutturabil",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Able + Verb + Imp + A2sg"));
        }

        [TestMethod]
        public void Unable()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazama", MatchesTailLex("Verb + Unable + Imp + A2sg"));
            t.ExpectAny("yazamaz", MatchesTailLex("Verb + Unable + Aor + A3sg"));
            t.ExpectSingle("yazamıyor", MatchesTailLex("Verb + Unable + Prog1 + A3sg"));

            t = GetTester("okumak");

            t.ExpectSingle("okuyama", MatchesTailLex("Verb + Unable + Imp + A2sg"));
            t.ExpectSingle("okuyamadım", MatchesTailLex("Verb + Unable + Past + A1sg"));
            t.ExpectAny("okutmayabilir",
                MatchesTailLex("Verb + Caus + Verb + Neg + Able + Verb + Aor + A3sg"));
            t.ExpectAny("okutamayabilir",
                MatchesTailLex("Verb + Caus + Verb + Unable + Able + Verb + Aor + A3sg"));
            t.ExpectAny("okutamayabilirdik",
                MatchesTailLex("Verb + Caus + Verb + Unable + Able + Verb + Aor + Past + A1pl"));

            t.ExpectAny("okuyamayabilir",
                MatchesTailLex("Verb + Unable + Able + Verb + Aor + A3sg"));

            t = GetTester("yakmak");
            t.ExpectSingle("yakamadım", MatchesTailLex("Verb + Unable + Past + A1sg"));
        }

        [TestMethod]
        public void Passive1()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazıl", MatchesTailLex("Verb + Pass + Verb + Imp + A2sg"));
            t.ExpectSingle("yazılıyor", MatchesTailLex("Verb + Pass + Verb + Prog1 + A3sg"));
            t.ExpectSingle("yazdırılıyor",
                MatchesTailLex("Verb + Caus + Verb + Pass + Verb + Prog1 + A3sg"));
            t.ExpectSingle("yazdırtılıyor",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Pass + Verb + Prog1 + A3sg"));

            t.ExpectFail(
                "yazınıyor",
                "yazınıl",
                "yazılınıl",
                "yazıldır",
                "yazdırınıyor",
                "yazdırıldır"
            );

            t = GetTester("okumak");

            t.ExpectSingle("okun", MatchesTailLex("Verb + Pass + Verb + Imp + A2sg"));
            t.ExpectSingle("okunul", MatchesTailLex("Verb + Pass + Verb + Imp + A2sg"));
            t.ExpectAny("okunulabilir",
                MatchesTailLex("Verb + Pass + Verb + Able + Verb + Aor + A3sg"));
        }

        [TestMethod]
        public void Passive2()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazılmasın", MatchesTailLex("Verb + Pass + Verb + Neg + Imp + A3sg"));
            t.ExpectSingle("yazılmıyor", MatchesTailLex("Verb + Pass + Verb + Neg + Prog1 + A3sg"));
            t.ExpectSingle("yazdırılmıyor",
                MatchesTailLex("Verb + Caus + Verb + Pass + Verb + Neg + Prog1 + A3sg"));
            t.ExpectSingle("yazdırtılmıyor",
                MatchesTailLex("Verb + Caus + Verb + Caus + Verb + Pass + Verb + Neg + Prog1 + A3sg"));

            t.ExpectFail(
                "yazınmıyor",
                "yazınılma",
                "yazılınılma",
                "yazıldırma",
                "yazdırınmıyor",
                "yazdırıldırma"
            );

            t = GetTester("okumak");

            t.ExpectSingle("okunmayın", MatchesTailLex("Verb + Pass + Verb + Neg + Imp + A2pl"));
            t.ExpectSingle("okunulmasın", MatchesTailLex("Verb + Pass + Verb + Neg + Imp + A3sg"));
            t.ExpectAny("okunulmayabilir",
                MatchesTailLex("Verb + Pass + Verb + Neg + Able + Verb + Aor + A3sg"));
            t.ExpectAny("okunulamayabilir",
                MatchesTailLex("Verb + Pass + Verb + Unable + Able + Verb + Aor + A3sg"));
        }

        [TestMethod]
        public void PassiveVazgecmek()
        {
            AnalysisTester t = GetTester("vazgeçmek [A:Aorist_A]", "yenmek");
            t.ExpectSingle("vazgeçil", MatchesTailLex("Verb + Pass + Verb + Imp + A2sg"));
            t.ExpectAny("vazgeçilmez", MatchesTailLex("Verb + Pass + Verb + Neg + AorPart + Adj"));
            t.ExpectAny("yenilmez", MatchesTailLex("Verb + Pass + Verb + Neg + AorPart + Adj"));
        }

        [TestMethod]
        public void Past()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazdım", MatchesTailLex("Verb + Past + A1sg"));
            t.ExpectSingle("yazmadım", MatchesTailLex("Verb + Neg + Past + A1sg"));
            t.ExpectAny("yazdık", MatchesTailLex("Verb + Past + A1pl"));
            t.ExpectAny("yazmadık", MatchesTailLex("Verb + Neg + Past + A1pl"));
            t.ExpectSingle("yazdıysan", MatchesTailLex("Verb + Past + Cond + A2sg"));
            t.ExpectSingle("yazmadıysan", MatchesTailLex("Verb + Neg + Past + Cond + A2sg"));
            t.ExpectSingle("yazdır", MatchesTailLex("Verb + Caus + Verb + Imp + A2sg"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectSingle("etti", MatchesTailLex("Verb + Past + A3sg"));
            t.ExpectSingle("etmedi", MatchesTailLex("Verb + Neg + Past + A3sg"));
            t.ExpectAny("ettik", MatchesTailLex("Verb + Past + A1pl"));
            t.ExpectAny("etmedik", MatchesTailLex("Verb + Neg + Past + A1pl"));
        }

        [TestMethod]
        public void Narrative()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazmışım", MatchesTailLex("Verb + Narr + A1sg"));
            t.ExpectAny("yazmamışım", MatchesTailLex("Verb + Neg + Narr + A1sg"));
            t.ExpectAny("yazmışız", MatchesTailLex("Verb + Narr + A1pl"));
            t.ExpectAny("yazmamışız", MatchesTailLex("Verb + Neg + Narr + A1pl"));
            t.ExpectAny("yazmışsan", MatchesTailLex("Verb + Narr + Cond + A2sg"));
            t.ExpectAny("yazmamışsan", MatchesTailLex("Verb + Neg + Narr + Cond + A2sg"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectAny("etmiş", MatchesTailLex("Verb + Narr + A3sg"));
            t.ExpectAny("etmemiş", MatchesTailLex("Verb + Neg + Narr + A3sg"));
            t.ExpectAny("etmişiz", MatchesTailLex("Verb + Narr + A1pl"));
            t.ExpectAny("etmemişiz", MatchesTailLex("Verb + Neg + Narr + A1pl"));
            t.ExpectAny("etmişmiş", MatchesTailLex("Verb + Narr + Narr + A3sg"));
        }

        [TestMethod]
        public void Future()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazacağım", MatchesTailLex("Verb + Fut + A1sg"));
            t.ExpectAny("yazmayacağım", MatchesTailLex("Verb + Neg + Fut + A1sg"));
            t.ExpectAny("yazacağız", MatchesTailLex("Verb + Fut + A1pl"));
            t.ExpectAny("yazmayacağız", MatchesTailLex("Verb + Neg + Fut + A1pl"));
            t.ExpectAny("yazacaksan", MatchesTailLex("Verb + Fut + Cond + A2sg"));
            t.ExpectAny("yazmayacaksan", MatchesTailLex("Verb + Neg + Fut + Cond + A2sg"));
            t.ExpectAny("yazmayacaktın", MatchesTailLex("Verb + Neg + Fut + Past + A2sg"));

            t.ExpectFail(
                "yazmayacağ",
                "yazmayacakım",
                "yazacakım",
                "yazacağsın"
            );

            t = GetTester("etmek [A:Voicing]");
            t.ExpectAny("edecek", MatchesTailLex("Verb + Fut + A3sg"));
            t.ExpectAny("etmeyecek", MatchesTailLex("Verb + Neg + Fut + A3sg"));
            t.ExpectSingle("edeceğiz", MatchesTailLex("Verb + Fut + A1pl"));
            t.ExpectSingle("etmeyeceğiz", MatchesTailLex("Verb + Neg + Fut + A1pl"));
        }

        [TestMethod]
        public void Future2()
        {
            AnalysisTester t = GetTester("aramak");

            t.ExpectAny("arayacağım", MatchesTailLex("Verb + Fut + A1sg"));
            t.ExpectSingle("aratacağız", MatchesTailLex("Verb + Caus + Verb + Fut + A1pl"));
            t.ExpectSingle("arayabileceğiz", MatchesTailLex("Verb + Able + Verb + Fut + A1pl"));
            t.ExpectSingle("aratabileceğiz",
                MatchesTailLex("Verb + Caus + Verb + Able + Verb + Fut + A1pl"));
            t.ExpectSingle("aratmayabileceğiz",
                MatchesTailLex("Verb + Caus + Verb + Neg + Able + Verb + Fut + A1pl"));
            t.ExpectSingle("arattıramayabileceğiz",
                MatchesTailLex(
                    "Verb + Caus + Verb + Caus + Verb + Unable + Able + Verb + Fut + A1pl"));
        }

        [TestMethod]
        public void ProgressiveMakta()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazmakta", MatchesTailLex("Verb + Prog2 + A3sg"));
            t.ExpectAny("yazmaktayım", MatchesTailLex("Verb + Prog2 + A1sg"));
            t.ExpectAny("yazmamaktayım", MatchesTailLex("Verb + Neg + Prog2 + A1sg"));
            t.ExpectAny("yazmaktayız", MatchesTailLex("Verb + Prog2 + A1pl"));
            t.ExpectAny("yazmamaktayız", MatchesTailLex("Verb + Neg + Prog2 + A1pl"));
            t.ExpectSingle("yazmaktaysan", MatchesTailLex("Verb + Prog2 + Cond + A2sg"));
            t.ExpectSingle("yazmaktaymışsınız", MatchesTailLex("Verb + Prog2 + Narr + A2pl"));
            // awkward but ok.
            t.ExpectAny("yazmamaktaydım", MatchesTailLex("Verb + Neg + Prog2 + Past + A1sg"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectAny("etmekte", MatchesTailLex("Verb + Prog2 + A3sg"));
            t.ExpectAny("etmemekte", MatchesTailLex("Verb + Neg + Prog2 + A3sg"));
            t.ExpectAny("etmekteyiz", MatchesTailLex("Verb + Prog2 + A1pl"));
        }

        [TestMethod]
        public void ProgressiveMakta2()
        {
            AnalysisTester t = GetTester("aramak");

            t.ExpectAny("aramaktayım", MatchesTailLex("Verb + Prog2 + A1sg"));
            t.ExpectAny("aratmaktayız", MatchesTailLex("Verb + Caus + Verb + Prog2 + A1pl"));
            t.ExpectAny("arayabilmekteyiz", MatchesTailLex("Verb + Able + Verb + Prog2 + A1pl"));
            t.ExpectAny("aratabilmekteyiz",
                MatchesTailLex("Verb + Caus + Verb + Able + Verb + Prog2 + A1pl"));
            t.ExpectAny("aratmayabilmekteyiz",
                MatchesTailLex("Verb + Caus + Verb + Neg + Able + Verb + Prog2 + A1pl"));
            t.ExpectAny("arattıramayabilmekteyiz",
                MatchesTailLex(
                    "Verb + Caus + Verb + Caus + Verb + Unable + Able + Verb + Prog2 + A1pl"));
        }


        [TestMethod]
        public void DemekYemek()
        {
            AnalysisTester t = GetTester("demek", "yemek");

            t.ExpectSingle("de", MatchesTailLex("Verb + Imp + A2sg"));
            t.ExpectAny("deme", MatchesTailLex("Verb + Neg + Imp + A2sg"));
            t.ExpectSingle("dedi", MatchesTailLex("Verb + Past + A3sg"));
            t.ExpectAny("demiş", MatchesTailLex("Verb + Narr + A3sg"));
            t.ExpectSingle("den", MatchesTailLex("Verb + Pass + Verb + Imp + A2sg"));
            t.ExpectSingle("denil", MatchesTailLex("Verb + Pass + Verb + Imp + A2sg"));
            t.ExpectAny("diyecek", MatchesTailLex("Verb + Fut + A3sg"));
            t.ExpectAny("diyecek", MatchesTailLex("Verb + FutPart + Adj"));
            t.ExpectAny("diyebilir", MatchesTailLex("Verb + Able + Verb + Aor + A3sg"));
            t.ExpectAny("deme", MatchesTailLex("Verb + Neg + Imp + A2sg"));
            t.ExpectSingle("diyor", MatchesTailLex("Verb + Prog1 + A3sg"));
            t.ExpectSingle("demiyor", MatchesTailLex("Verb + Neg + Prog1 + A3sg"));
            t.ExpectSingle("der", MatchesTailLex("Verb + Aor + A3sg"));
            t.ExpectAny("demez", MatchesTailLex("Verb + Neg + Aor + A3sg"));
            t.ExpectSingle("dedir", MatchesTailLex("Verb + Caus + Verb + Imp + A2sg"));
            t.ExpectAny("dedirme", MatchesTailLex("Verb + Caus + Verb + Neg + Imp + A2sg"));
            t.ExpectSingle("diye", MatchesTailLex("Verb + Opt + A3sg"));
            t.ExpectAny("demeye", MatchesTailLex("Verb + Neg + Opt + A3sg"));
            t.ExpectSingle("dese", MatchesTailLex("Verb + Desr + A3sg"));
            t.ExpectSingle("demese", MatchesTailLex("Verb + Neg + Desr + A3sg"));

            t.ExpectSingle("ye", MatchesTailLex("Verb + Imp + A2sg"));
            t.ExpectSingle("yesin", MatchesTailLex("Verb + Imp + A3sg"));
            t.ExpectSingle("yiyin", MatchesTailLex("Verb + Imp + A2pl"));
            t.ExpectSingle("yiyiniz", MatchesTailLex("Verb + Imp + A2pl"));
            t.ExpectSingle("yesinler", MatchesTailLex("Verb + Imp + A3pl"));
            t.ExpectSingle("yiyesim", MatchesTailLex("Verb + FeelLike + Noun + A3sg + P1sg"));

            t.ExpectFail(
                "dir",
                "dimez",
                "di",
                "din",
                "didir",
                "deyor",
                "deyecek",
                "didi",
                "yeyiş",
                "diyiş",
                "dimek",
                "dime",
                "yime",
                "yimek",
                "dimiş",
                "dimiyor"
            );

            t.ExpectFail(
                "yir",
                "yimez",
                "yi",
                "yin",
                "yidir",
                "yeyor",
                "yeyecek",
                "yidi",
                "yeyiş",
                "yimek",
                "yime",
                "yime",
                "yeyin",
                "yeyiniz",
                "yisin",
                "yisinler",
                "yimez",
                "yimek",
                "yimek",
                "yimiş",
                "yimiyor"
            );
        }


        [TestMethod]
        public void Optative()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazayım", MatchesTailLex("Verb + Opt + A1sg"));
            t.ExpectAny("yazmayayım", MatchesTailLex("Verb + Neg + Opt + A1sg"));
            t.ExpectAny("yazasın", MatchesTailLex("Verb + Opt + A2sg"));
            t.ExpectAny("yazmayasın", MatchesTailLex("Verb + Neg + Opt + A2sg"));
            t.ExpectSingle("yaza", MatchesTailLex("Verb + Opt + A3sg"));
            t.ExpectAny("yazmaya", MatchesTailLex("Verb + Neg + Opt + A3sg"));
            t.ExpectSingle("yazalım", MatchesTailLex("Verb + Opt + A1pl"));
            t.ExpectAny("yazasınız", MatchesTailLex("Verb + Opt + A2pl"));
            t.ExpectSingle("yazalar", MatchesTailLex("Verb + Opt + A3pl"));
            t.ExpectSingle("yazaydı", MatchesTailLex("Verb + Opt + Past + A3sg"));
            t.ExpectSingle("yazaymış", MatchesTailLex("Verb + Opt + Narr + A3sg"));
            t.ExpectSingle("yazaymışlar", MatchesTailLex("Verb + Opt + Narr + A3pl"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectSingle("edeyim", MatchesTailLex("Verb + Opt + A1sg"));
            t.ExpectSingle("ede", MatchesTailLex("Verb + Opt + A3sg"));
            t.ExpectAny("etmeye", MatchesTailLex("Verb + Neg + Opt + A3sg"));
        }

        [TestMethod]
        public void Desire()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazsam", MatchesTailLex("Verb + Desr + A1sg"));
            t.ExpectSingle("yazmasam", MatchesTailLex("Verb + Neg + Desr + A1sg"));
            t.ExpectSingle("yazsan", MatchesTailLex("Verb + Desr + A2sg"));
            t.ExpectSingle("yazmasan", MatchesTailLex("Verb + Neg + Desr + A2sg"));
            t.ExpectSingle("yazsa", MatchesTailLex("Verb + Desr + A3sg"));
            t.ExpectAny("yazmasa", MatchesTailLex("Verb + Neg + Desr + A3sg"));
            t.ExpectSingle("yazsak", MatchesTailLex("Verb + Desr + A1pl"));
            t.ExpectSingle("yazsanız", MatchesTailLex("Verb + Desr + A2pl"));
            t.ExpectSingle("yazsalar", MatchesTailLex("Verb + Desr + A3pl"));
            t.ExpectSingle("yazsaydı", MatchesTailLex("Verb + Desr + Past + A3sg"));
            t.ExpectSingle("yazsaymışlar", MatchesTailLex("Verb + Desr + Narr + A3pl"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectSingle("etsem", MatchesTailLex("Verb + Desr + A1sg"));
            t.ExpectSingle("etse", MatchesTailLex("Verb + Desr + A3sg"));
            t.ExpectSingle("etmese", MatchesTailLex("Verb + Neg + Desr + A3sg"));
        }

        [TestMethod]
        public void Necessity()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazmalıyım", MatchesTailLex("Verb + Neces + A1sg"));
            t.ExpectAny("yazmamalıyım", MatchesTailLex("Verb + Neg + Neces + A1sg"));
            t.ExpectSingle("yazmalısın", MatchesTailLex("Verb + Neces + A2sg"));
            t.ExpectSingle("yazmamalısın", MatchesTailLex("Verb + Neg + Neces + A2sg"));
            t.ExpectAny("yazmalı", MatchesTailLex("Verb + Neces + A3sg"));
            t.ExpectAny("yazmamalı", MatchesTailLex("Verb + Neg + Neces + A3sg"));
            t.ExpectAny("yazmamalıysa", MatchesTailLex("Verb + Neg + Neces + Cond + A3sg"));
            t.ExpectAny("yazmalıyız", MatchesTailLex("Verb + Neces + A1pl"));
            t.ExpectSingle("yazmalısınız", MatchesTailLex("Verb + Neces + A2pl"));
            t.ExpectAny("yazmalılar", MatchesTailLex("Verb + Neces + A3pl"));
            t.ExpectAny("yazmalıydı", MatchesTailLex("Verb + Neces + Past + A3sg"));
            t.ExpectAny("yazmalıymışlar", MatchesTailLex("Verb + Neces + Narr + A3pl"));

            t = GetTester("etmek [A:Voicing]");
            t.ExpectAny("etmeliyim", MatchesTailLex("Verb + Neces + A1sg"));
            t.ExpectAny("etmeli", MatchesTailLex("Verb + Neces + A3sg"));
            t.ExpectAny("etmemeli", MatchesTailLex("Verb + Neg + Neces + A3sg"));
        }

        [TestMethod]
        public void A3plExceptionTest()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectSingle("yazarlardı", MatchesTailLex("Verb + Aor + A3pl + Past"));
            t.ExpectSingle("yazardılar", MatchesTailLex("Verb + Aor + Past + A3pl"));
            t.ExpectSingle("yazarlarmış", MatchesTailLex("Verb + Aor + A3pl + Narr"));
            t.ExpectSingle("yazarmışlar", MatchesTailLex("Verb + Aor + Narr + A3pl"));
            t.ExpectSingle("yazarsalar", MatchesTailLex("Verb + Aor + Cond + A3pl"));
            t.ExpectSingle("yazarlarsa", MatchesTailLex("Verb + Aor + A3pl + Cond"));

            t.ExpectFail(
                "yazarlardılar",
                "yazardılardı",
                "yazarlarmışlar",
                "yazarmışlarmış",
                "yazarsalarsa",
                "yazarlarsalar",
                "yazardılarsa",
                "yazarsalarmış",
                "yazarsalardı",
                "yazarmışlarsa"
            );
        }

        [TestMethod]
        public void Copula()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazardır", MatchesTailLex("Verb + Aor + A3sg + Cop"));
            t.ExpectAny("yazmazdır", MatchesTailLex("Verb + Neg + Aor + A3sg + Cop"));
            t.ExpectAny("yazacaktır", MatchesTailLex("Verb + Fut + A3sg + Cop"));
            t.ExpectAny("yazacağımdır", MatchesTailLex("Verb + Fut + A1sg + Cop"));
            t.ExpectAny("yazacaksındır", MatchesTailLex("Verb + Fut + A2sg + Cop"));
            t.ExpectAny("yazacağızdır", MatchesTailLex("Verb + Fut + A1pl + Cop"));
            t.ExpectAny("yazacaksınızdır", MatchesTailLex("Verb + Fut + A2pl + Cop"));
            t.ExpectAny("yazacaklardır", MatchesTailLex("Verb + Fut + A3pl + Cop"));
            t.ExpectSingle("yazıyordur", MatchesTailLex("Verb + Prog1 + A3sg + Cop"));
            t.ExpectAny("yazmaktadır", MatchesTailLex("Verb + Prog2 + A3sg + Cop"));
            t.ExpectAny("yazmalıdır", MatchesTailLex("Verb + Neces + A3sg + Cop"));

            t.ExpectFail(
                "yazsadır",
                "yazdıdır"
            );
        }

        [TestMethod]
        public void CopulaBeforeA3pl()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazardırlar", MatchesTailLex("Verb + Aor + Cop + A3pl"));
            t.ExpectAny("yazmazdırlar", MatchesTailLex("Verb + Neg + Aor + Cop + A3pl"));
            t.ExpectAny("yazacaktırlar", MatchesTailLex("Verb + Fut + Cop + A3pl"));
            t.ExpectSingle("yazıyordurlar", MatchesTailLex("Verb + Prog1 + Cop + A3pl"));
            t.ExpectAny("yazmaktadırlar", MatchesTailLex("Verb + Prog2 + Cop + A3pl"));
            t.ExpectAny("yazmalıdırlar", MatchesTailLex("Verb + Neces + Cop + A3pl"));

            t.ExpectFail(
                "yazacaktırlardır"
            );
        }


        [TestMethod]
        public void CondAfterPastPerson()
        {
            AnalysisTester t = GetTester("yazmak");

            t.ExpectAny("yazdınsa", MatchesTailLex("Verb + Past + A2sg + Cond"));
            t.ExpectAny("yazdımsa", MatchesTailLex("Verb + Past + A1sg + Cond"));
            t.ExpectAny("yazdınızsa", MatchesTailLex("Verb + Past + A2pl + Cond"));
            t.ExpectAny("yazmadınızsa", MatchesTailLex("Verb + Neg + Past + A2pl + Cond"));

            t.ExpectFail(
                "yazdınsaydın",
                "yazsaydınsa"
            );
        }

        [TestMethod]
        public void LastVowelDropTest()
        {
            AnalysisTester t = GetTester("kavurmak [A:LastVowelDrop]");

            t.ExpectAny("kavur", MatchesTailLex("Verb + Imp + A2sg"));
            t.ExpectAny("kavuruyor", MatchesTailLex("Verb + Prog1 + A3sg"));
            t.ExpectAny("kavuracak", MatchesTailLex("Verb + Fut + A3sg"));
            t.ExpectAny("kavurur", MatchesTailLex("Verb + Aor + A3sg"));
            t.ExpectAny("kavurmuyor", MatchesTailLex("Verb + Neg + Prog1 + A3sg"));
            t.ExpectAny("kavurt", MatchesTailLex("Verb + Caus + Verb + Imp + A2sg"));
            t.ExpectAny("kavrul", MatchesTailLex("Verb + Pass + Verb + Imp + A2sg"));
            t.ExpectAny("kavurabil", MatchesTailLex("Verb + Able + Verb + Imp + A2sg"));
            t.ExpectAny("kavrulabil", MatchesTailLex("Verb + Pass + Verb + Able + Verb + Imp + A2sg"));

            t.ExpectFail(
                "kavr",
                "kavurulacak",
                "kavurul",
                "kavracak",
                "kavrıyor",
                "kavruyor",
                "kavurturacak"
            );
        }

        [TestMethod]
        [Ignore]
        // Reciprocal suffix is not included for now.
        public void ReciprocalTest()
        {
            AnalysisTester t = GetTester("kaçmak");

            t.ExpectAny("kaçış", MatchesTailLex("Verb + Recip + Verb + Imp + A2sg"));
            t.ExpectAny("kaçışma", MatchesTailLex("Verb + Recip + Verb + Neg + Imp + A2sg"));
            t.ExpectAny("kaçıştık", MatchesTailLex("Verb + Recip + Verb + Past + A1pl"));

            // Implicit Reciprocal
            t = GetTester("dövüşmek [A:Reciprocal]");
            t.ExpectAny("dövüştük", MatchesTailLex("Verb + Recip + Verb + Past + A1pl"));
        }

        [TestMethod]
        public void TestImek()
        {
            AnalysisTester t = GetTester("imek");

            t.ExpectAny("idi", MatchesTailLex("Verb + Past + A3sg"));
            t.ExpectAny("ise", MatchesTailLex("Verb + Cond + A3sg"));
            t.ExpectAny("imiş", MatchesTailLex("Verb + Narr + A3sg"));

            t.ExpectFail(
                "i",
                "iyim"
            );
        }

        /**
         * For Issue https://github.com/ahmetaa/zemberek-nlp/issues/173
         */
        [TestMethod]
        public void NegativeShouldNotComeAfterAbility_173()
        {
            AnalysisTester t = GetTester("yazmak");
            t.ExpectSingle("yazabil", MatchesTailLex("Verb + Able + Verb + Imp + A2sg"));
            // only analysis. There should not be a "Neg" solution.
            t.ExpectSingle("yazabilme", MatchesTailLex("Verb + Able + Verb + Inf2 + Noun + A3sg"));
        }
    }
}
