using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.LM.Backoff;
using ZemberekDotNet.LM.Compression;

namespace ZemberekDotNet.LM.Tests.Compression
{
    [TestClass]
    public class SmoothLmTest
    {
        string TinyArpaPath = "Resources/tiny.arpa";

        [TestMethod]
        public void TestGeneration()
        {
            SmoothLm lm = GetTinyLm();
            Assert.AreEqual(3, lm.GetOrder());
        }

        private SmoothLm GetTinyLm()
        {
            return SmoothLm.Builder(GetTinyLmFile()).Build();
        }

        string GetTinyArpaFile()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "tiny.arpa");
            Log.Info("Temporary test Arpa model file {0}", tempPath);
            File.Copy(TinyArpaPath, tempPath, true);
            return tempPath;
        }

        private string GetTinyLmFile()
        {
            string tmp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string lmFile = Path.Combine(tmp, "tiny.slm");
            Log.Info("Temporary test compressed model file {0}", lmFile);
            if (!File.Exists(lmFile))
            {
                UncompressedToSmoothLmConverter converter = new UncompressedToSmoothLmConverter(lmFile, tmp);
                converter.ConvertSmall(
                    MultiFileUncompressedLm.Generate(GetTinyArpaFile(), tmp, "utf-8", 4).GetLmDir(),
                          new UncompressedToSmoothLmConverter.NgramDataBlock(16, 16, 16));
            }
            return lmFile;
        }

        [TestMethod]
        public void TestNgramKeyExactMatch()
        {
            string lmDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            MultiFileUncompressedLm.Generate(GetTinyArpaFile(), lmDir, "utf-8", 4).GetLmDir();
            string lmFile = Path.Combine(lmDir, "tiny.slm");
            UncompressedToSmoothLmConverter converter = new UncompressedToSmoothLmConverter(lmFile, lmDir);
            converter.ConvertSmall(
                lmDir,
                    new UncompressedToSmoothLmConverter.NgramDataBlock(16, 16, 16));
            SmoothLm slm = SmoothLm.Builder(lmFile).NgramKeyFilesDirectory(lmDir).Build();
            SimpleBackoffNgramModel model = SimpleBackoffNgramModel.FromArpa(GetTinyArpaFile());
            IEnumerator<SimpleBackoffNgramModel.NgramData> it = model.GetAllIndexes();
            while (it.MoveNext())
            {
                Assert.IsTrue(slm.NGramExists(it.Current.GetIndexes()));
            }
        }

        [TestMethod]
        [Ignore("Requires external data")]
        public void TestBigFakeLm()
        {
            int order = 4;
            string lmFile = "/media/ahmetaa/depo/data/lm/fake/fake.slm";
            FakeLm fakeLm;
            if (!File.Exists(lmFile))
            {
                string arpaFile = "/media/ahmetaa/depo/data/lm/fake/fake.arpa";
                string tmp = "/tmp";
                if (!File.Exists(arpaFile))
                {
                    fakeLm = new FakeLm(order);
                    fakeLm.GenerateArpa(arpaFile);
                }
                UncompressedToSmoothLmConverter converter = new UncompressedToSmoothLmConverter(lmFile, tmp);
                converter.ConvertSmall(
                    MultiFileUncompressedLm.Generate(arpaFile, tmp, "utf-8", 4).dir,
                          new UncompressedToSmoothLmConverter.NgramDataBlock(24, 24, 24));
            }
            SmoothLm lm = SmoothLm.Builder(lmFile).Build();

            fakeLm = new FakeLm(order);
            for (int i = 1; i <= fakeLm.order; i++)
            {
                FakeLm.FakeGram[] probs = fakeLm.GetNgramProbs(i);
                foreach (FakeLm.FakeGram prob in probs)
                {
                    Assert.AreEqual(prob.prob,
                        lm.GetProbability(prob.indexes), 0.001, "ouch:" + Arrays.ToString(prob.vals));
                }
            }
        }

        [TestMethod]
        public void TestVocabulary()
        {
            SmoothLm lm = GetTinyLm();
            LmVocabulary vocab = lm.GetVocabulary();
            Assert.IsTrue(vocab.Contains("Ahmet"));
            int i1 = vocab.IndexOf("Ahmet");
            Assert.IsTrue(vocab.Contains("elma"));
            int i2 = vocab.IndexOf("elma");
            Assert.IsTrue(i1 != i2);
            Assert.AreEqual("Ahmet", vocab.GetWord(i1));
            Assert.AreEqual("elma", vocab.GetWord(i2));
        }

        [TestMethod]
        public void TestProbabilities()
        {
            SmoothLm lm = GetTinyLm();
            Console.WriteLine(lm.Info());
            LmVocabulary vocabulary = lm.GetVocabulary();
            int[] indexes = { vocabulary.IndexOf("<s>") };
            Assert.AreEqual(-1.716003, lm.GetProbabilityValue(indexes), 0.0001);
            Assert.AreEqual(-1.716003, lm.GetProbability(indexes), 0.0001);
            //<s> kedi
            int[]
            is2 = { vocabulary.IndexOf("<s>"), vocabulary.IndexOf("kedi") };
            Assert.AreEqual(-0.796249, lm.GetProbabilityValue(is2), 0.0001);
            Assert.AreEqual(-0.796249, lm.GetProbability(is2), 0.0001);
            //Ahmet dondurma yedi
            int[]
            is3 = {
        vocabulary.IndexOf("Ahmet"), vocabulary.IndexOf("dondurma"),
        vocabulary.IndexOf("yedi")};
            Assert.AreEqual(-0.602060, lm.GetProbabilityValue(is3), 0.0001);
            Assert.AreEqual(-0.602060, lm.GetProbability(is3), 0.0001);
        }

        [TestMethod]
        public void TestBackoffcount()
        {
            SmoothLm lm = GetTinyLm();
            LmVocabulary vocabulary = lm.GetVocabulary();
            int[] indexes = { vocabulary.IndexOf("<s>") };
            Assert.AreEqual(0, lm.GetBackoffCount(indexes));
            int[]
            is2 = vocabulary.ToIndexes("<s>", "kedi");
            Assert.AreEqual(0, lm.GetBackoffCount(is2));
            int[]
            is3 = vocabulary.ToIndexes("Ahmet", "dondurma", "yedi");
            Assert.AreEqual(0, lm.GetBackoffCount(is3));
            int[]
            is4 = vocabulary.ToIndexes("Ahmet", "yemez");
            Assert.AreEqual(1, lm.GetBackoffCount(is4));
            int[]
            is5 = vocabulary.ToIndexes("Ahmet", "yemez", "kırmızı");
            Assert.AreEqual(2, lm.GetBackoffCount(is5));
        }

        [TestMethod]
        public void TestExplain()
        {
            SmoothLm lm = GetTinyLm();
            LmVocabulary vocabulary = lm.GetVocabulary();
            int[] indexes = { vocabulary.IndexOf("<s>") };
            Console.WriteLine(lm.Explain(indexes));
            //<s> kedi
            int[]
            is2 = vocabulary.ToIndexes("<s>", "kedi");
            Console.WriteLine(lm.Explain(is2));
            //Ahmet dondurma yedi
            int[]
            is3 = vocabulary.ToIndexes("Ahmet", "dondurma", "yedi");
            Console.WriteLine(lm.Explain(is3));
            int[]
            is4 = vocabulary.ToIndexes("Ahmet", "yemez");
            Console.WriteLine(lm.Explain(is4));
            int[]
            is5 = vocabulary.ToIndexes("Ahmet", "yemez", "kırmızı");
            Console.WriteLine(lm.Explain(is5));
        }

        [TestMethod]
        public void TestLogBaseChange()
        {
            SmoothLm lm10 = GetTinyLm();
            Console.WriteLine(lm10.Info());
            string lmFile = GetTinyLmFile();
            SmoothLm lm = SmoothLm.Builder(lmFile).LogBase(Math.E).Build();
            Console.WriteLine(lm.Info());
            Assert.AreEqual(lm.GetLogBase(), Math.E, 0.00001);
            LmVocabulary vocabulary = lm.GetVocabulary();
            int[] indexes = { vocabulary.IndexOf("<s>") };
            Assert.AreEqual(L(-1.716003), lm.GetProbabilityValue(indexes), 0.0001);
            Assert.AreEqual(L(-1.716003), lm.GetProbability(indexes), 0.0001);
            //<s> kedi
            int[]
            is2 = { vocabulary.IndexOf("<s>"), vocabulary.IndexOf("kedi") };
            Assert.AreEqual(L(-0.796249), lm.GetProbabilityValue(is2), 0.00012);
            Assert.AreEqual(L(-0.796249), lm.GetProbability(is2), 0.00012);
            //Ahmet dondurma yedi
            int[]
            is3 = {
        vocabulary.IndexOf("Ahmet"), vocabulary.IndexOf("dondurma"),
        vocabulary.IndexOf("yedi")};
            Assert.AreEqual(L(-0.602060), lm.GetProbabilityValue(is3), 0.00012);
            Assert.AreEqual(L(-0.602060), lm.GetProbability(is3), 0.00012);
            File.Delete(lmFile);
        }

        private double L(double i)
        {
            return Math.Log(Math.Pow(10, i));
        }

        [TestMethod]
        [Ignore]
        public void TestActualData()
        {
            Stopwatch sw = Stopwatch.StartNew();
            string lmFile = "/home/ahmetaa/data/lm/smoothnlp-test/lm1.slm";
            string tmp = "/tmp";
            if (!File.Exists(lmFile))
            {
                string arpaFile = "/home/ahmetaa/data/lm/smoothnlp-test/lm1.arpa";
                UncompressedToSmoothLmConverter converter = new UncompressedToSmoothLmConverter(lmFile, tmp);
                converter.ConvertLarge(
                    MultiFileUncompressedLm.Generate(arpaFile, tmp, "utf-8", 4).dir,
                          new UncompressedToSmoothLmConverter.NgramDataBlock(2, 1, 1), 20);
            }
            SmoothLm lm = SmoothLm.Builder(lmFile).Build();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();
            int order = 3;
            int gramCount = 1000000;
            int[][] ids = new int[gramCount][];
            for (int j = 0; j < ids.Length; j++)
            {
                ids[j] = new int[order];
            }
            long[] trigrams = new long[gramCount];
            LineIterator li = SimpleTextReader.TrimmingUTF8LineIterator(
                 "/home/ahmetaa/data/lm/smoothnlp-test/corpus-lowercase_1000000_2000000");
            SpaceTabTokenizer tokenizer = new SpaceTabTokenizer();
            int i = 0;
            while (i < gramCount)
            {
                li.MoveNext();
                String line = li.Current;
                String[] tokens = tokenizer.Split(line);
                if (tokens.Length < order)
                {
                    continue;
                }
                for (int j = 0; j < tokens.Length - order - 1; j++)
                {
                    String[] words = new String[order];
                    Array.Copy(tokens, j, words, 0, order);
                    int[] indexes = lm.GetVocabulary().ToIndexes(words);
                    if (!lm.GetVocabulary().ContainsAll(indexes))
                    {
                        continue;
                    }
                    ids[i] = indexes;
                    if (order == 3)
                    {
                        trigrams[i] = lm.GetVocabulary().EncodeTrigram(indexes);
                    }
                    i++;
                    if (i == gramCount)
                    {
                        break;
                    }
                }
            }
            sw.Start();
            double tr = 0;
            foreach (int[] id in ids)
            {
                tr += lm.GetProbability(id);
            }
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.WriteLine("tr = " + tr);
        }

        [TestMethod]
        public void TestProbWithBackoff()
        {
            SmoothLm lm = GetTinyLm();
            LmVocabulary vocabulary = lm.GetVocabulary();
            int ahmet = vocabulary.IndexOf("Ahmet");
            int yemez = vocabulary.IndexOf("yemez");
            // p(yemez|ahmet) = p(yemez) + b(ahmet) if p(yemez|ahmet) does not exist.
            double expected = -1.414973 + -0.316824;
            Assert.AreEqual(expected, lm.GetProbability(ahmet, yemez), 0.0001);
        }

        [TestMethod]
        public void TestTrigramBackoff()
        {
            SmoothLm lm = GetTinyLm();
            LmVocabulary vocabulary = lm.GetVocabulary();
            int ahmet = vocabulary.IndexOf("Ahmet");
            int armut = vocabulary.IndexOf("armut");
            int kirmizi = vocabulary.IndexOf("kırmızı");
            // p(kirmizi | Ahmet,armut) = b(ahmet, armut) + p(kırmızı|armut) if initial trigram prob does not exist.
            // if p(kırmızı|armut) also do not exist, we back off to b(ahmet, armut) + b(armut) + p(kırmızı)
            double backoffAhmetArmut = -0.124939;
            double backoffArmut = -0.492916;
            double probKirmizi = -1.539912;

            double expected = backoffAhmetArmut + backoffArmut + probKirmizi;
            Console.WriteLine("expected = " + expected);
            Console.WriteLine(lm.Explain(ahmet, armut, kirmizi));
            Assert.AreEqual(expected, lm.GetProbability(ahmet, armut, kirmizi), 0.0001);
            Assert.AreEqual(expected, lm.GetTriGramProbability(ahmet, armut, kirmizi), 0.0001);
        }

        [TestMethod]
        public void TestStupifBackoff()
        {
            string lmFile = GetTinyLmFile();
            SmoothLm lm = SmoothLm.Builder(lmFile).UseStupidBackoff().Build();
            LmVocabulary vocabulary = lm.GetVocabulary();
            int ahmet = vocabulary.IndexOf("Ahmet");
            int armut = vocabulary.IndexOf("armut");
            int kirmizi = vocabulary.IndexOf("kırmızı");
            // p(kirmizi | Ahmet,armut) = b(ahmet, armut) + p(kırmızı|armut) if initial trigram prob does not exist.
            // if p(kırmızı|armut) also do not exist, we back off to b(ahmet, armut) + b(armut) + p(kırmızı)
            double probKirmizi = -1.539912;
            double expected = lm.GetStupidBackoffLogAlpha() + lm.GetStupidBackoffLogAlpha() + probKirmizi;
            Console.WriteLine("expected = " + expected);
            Console.WriteLine(lm.Explain(ahmet, armut, kirmizi));
            Assert.AreEqual(expected, lm.GetProbability(ahmet, armut, kirmizi), 0.0001);
            File.Delete(lmFile);
        }

        [TestMethod]
        public void CacheTest()
        {
            SmoothLm lm = GetTinyLm();
            BaseLanguageModel.LookupCache cache = new BaseLanguageModel.LookupCache(lm);
            int[] is3 = lm.GetVocabulary().ToIndexes("Ahmet", "dondurma", "yedi");
            Assert.AreEqual(lm.GetProbability(is3), cache.Get(is3), 0.0001);
            Assert.AreEqual(lm.GetProbability(is3), cache.Get(is3), 0.0001);

            BaseLanguageModel.LookupCache cache2 = new BaseLanguageModel.LookupCache(lm);
            Assert.AreEqual(lm.GetProbability(is3), cache2.Get(is3), 0.0001);
            Assert.AreEqual(lm.GetProbability(is3), cache2.Get(is3), 0.0001);
        }


        [TestMethod]
        public void NgramExistTest()
        {
            SmoothLm lm = GetTinyLm();
            Assert.IsTrue(lm.NGramExists(lm.GetVocabulary().ToIndexes("Ahmet", "elma")));
            Assert.IsTrue(lm.NGramExists(lm.GetVocabulary().ToIndexes("elma")));
            Assert.IsFalse(lm.NGramExists(lm.GetVocabulary().ToIndexes("elma", "Ahmet")));
        }

        [TestMethod]
        [Ignore("Not an actual test.")]
        public void LoadLargeLmAndPrintInfo()
        {
            SmoothLm lm = SmoothLm
                .Builder("/media/depo/data/asr/model/language/tr/makine-sf/lm.slm").Build();
            Console.WriteLine(lm.Info());
            Console.WriteLine(lm.GetVocabulary().Size());
            Console.WriteLine(Arrays.ToString(lm.counts));
            Console.WriteLine(lm.GetVocabulary().IndexOf("<UNK>"));
            Console.WriteLine(lm.GetVocabulary().IndexOf("<unk>"));
        }
    }
}
