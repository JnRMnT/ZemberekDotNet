using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.LM.Tests
{
    [TestClass]
    public class LmVocabularyTest
    {
        [TestMethod]
        public void EmptyVocabularyTest()
        {
            LmVocabulary vocabulary = new LmVocabulary();
            Assert.IsTrue(vocabulary.Size() == 3);
            Assert.AreEqual(
                vocabulary.GetUnknownWord() + " " + vocabulary.GetUnknownWord(),
                vocabulary.GetWordsString(-1, -1));
        }

        [TestMethod]
        public void ArrayConstructorTest()
        {
            LmVocabulary vocabulary = new LmVocabulary("Hello", "World");
            SimpleCheck(vocabulary);
        }

        [TestMethod]
        public void SpecialWordsTest()
        {
            LmVocabulary vocabulary = new LmVocabulary("<S>", "Hello", "</S>");
            vocabulary.ContainsAll("<S>", "Hello", "</S>", "<unk>");
        }

        [TestMethod]
        public void BinaryFileGenerationTest()
        {
            string tmp = GetBinaryVocFile();
            LmVocabulary vocabulary = LmVocabulary.LoadFromBinary(tmp);
            SimpleCheck(vocabulary);
        }

        [TestMethod]
        public void Utf8FileGenerationTest()
        {
            string tmp = GetUtf8VocFile();
            LmVocabulary vocabulary = LmVocabulary.LoadFromUtf8File(tmp);
            SimpleCheck(vocabulary);
        }

        [TestMethod]
        public void StreamGenerationTest()
        {
            string tmp = GetBinaryVocFile();
            using (FileStream fileStream = File.OpenRead(tmp))
            {
                using (BinaryReader dis = new BinaryReader(fileStream))
                {
                    LmVocabulary vocabulary = LmVocabulary.LoadFromBinaryReader(dis);
                    SimpleCheck(vocabulary);
                }
            }
        }

        [TestMethod]
        public void RandomAccessGenerationTest()
        {
            string tmp = GetBinaryVocFile();
            using (FileStream raf = new FileStream(tmp, FileMode.Open, FileAccess.Read))
            {
                LmVocabulary vocabulary = LmVocabulary.LoadFromRandomAccessFile(raf);
                SimpleCheck(vocabulary);
            }
        }

        private void SimpleCheck(LmVocabulary vocabulary)
        {
            Assert.IsTrue(vocabulary.Size() == 5);
            Assert.AreEqual("Hello World",
                vocabulary.GetWordsString(vocabulary.ToIndexes("Hello", "World")));
            Assert.AreEqual("Hello " + vocabulary.GetUnknownWord(),
                vocabulary.GetWordsString(vocabulary.ToIndexes("Hello", vocabulary.GetUnknownWord())));
            Assert.IsTrue(vocabulary.Contains("Hello"));
            Assert.AreEqual(vocabulary.GetUnknownWordIndex(), vocabulary.IndexOf("Foo"));
        }

        private string GetBinaryVocFile()
        {
            string filePath = Path.Combine(Path.GetTempPath(), "voc_test.foo");
            FileStream tmp = File.Create(filePath);
            using (BinaryWriter dos = new BinaryWriter(tmp))
            {
                dos.Write(2.EnsureEndianness());
                dos.WriteUTF("Hello");
                dos.WriteUTF("World");
            }
            return filePath;
        }

        private string GetUtf8VocFile()
        {
            string filePath = Path.Combine(Path.GetTempPath(), "utf8_voc_test.foo");
            FileStream tmp = File.Create(filePath);
            using (StreamWriter streamWriter = new StreamWriter(tmp, Encoding.UTF8))
            {
                streamWriter.Write(string.Format("Hello{0}{1}      {2}\t{3}World", Environment.NewLine, Environment.NewLine, Environment.NewLine, Environment.NewLine));
            }
            return filePath;
        }

        [TestMethod]
        public void CollectionConstructorTest()
        {
            LmVocabulary vocabulary = new LmVocabulary(new List<string> { "Hello", "World" });
            SimpleCheck(vocabulary);
        }

        [TestMethod]
        public void Contains()
        {
            LmVocabulary vocabulary = new LmVocabulary("Hello", "World");

            int helloIndex = vocabulary.IndexOf("Hello");
            int worldIndex = vocabulary.IndexOf("World");
            Assert.IsTrue(vocabulary.Contains(helloIndex));
            Assert.IsTrue(vocabulary.Contains(worldIndex));

            int unkIndex = vocabulary.IndexOf("Foo");
            Assert.AreEqual(vocabulary.GetUnknownWordIndex(), unkIndex);

            Assert.IsTrue(vocabulary.ContainsAll(helloIndex, worldIndex));
            Assert.IsFalse(vocabulary.ContainsAll(-1, 2));

            Assert.IsTrue(vocabulary.Contains("Hello"));
            Assert.IsTrue(vocabulary.Contains("World"));
            Assert.IsFalse(vocabulary.Contains("Foo"));
            Assert.IsFalse(vocabulary.ContainsAll("Hello", "Foo"));
            Assert.IsTrue(vocabulary.ContainsAll("Hello", "World"));
        }

        [TestMethod]
        public void EncodedTrigramTest()
        {
            LmVocabulary vocabulary = new LmVocabulary("a", "b", "c", "d", "e");
            long k = ((1L << 21 | 2L) << 21) | 3L;
            Assert.AreEqual(k, vocabulary.EncodeTrigram(3, 2, 1));
            Assert.AreEqual(k, vocabulary.EncodeTrigram(3, 2, 1));
        }

        [TestMethod]
        public void ToWordsTest()
        {
            LmVocabulary vocabulary = new LmVocabulary("a", "b", "c", "d", "e");
            int[] indexes = vocabulary.ToIndexes("a", "e", "b");
            Assert.AreEqual("a e b", string.Join(" ", vocabulary.ToWords(indexes)));
            indexes = vocabulary.ToIndexes("a", "e", "foo");
            Assert.AreEqual("a e <unk>", string.Join(" ", vocabulary.ToWords(indexes)));
        }

        [TestMethod]
        public void ToIndexTest()
        {
            LmVocabulary vocabulary = new LmVocabulary("a", "b", "c", "d", "e");

            int[] indexes = { vocabulary.IndexOf("a"), vocabulary.IndexOf("e"), vocabulary.IndexOf("b") };
            Assert.IsTrue(Enumerable.SequenceEqual(indexes, vocabulary.ToIndexes("a", "e", "b")));

            int[] indexes2 = {vocabulary.IndexOf("a"), vocabulary.IndexOf("<unk>"),
        vocabulary.IndexOf("b")};
            Assert.IsTrue(Enumerable.SequenceEqual(indexes2, vocabulary.ToIndexes("a", "foo", "b")));
        }

        [TestMethod]
        public void BuilderTest()
        {
            LmVocabulary.LmVocabularyBuilder builder = LmVocabulary.Builder();
            string[]
            words = { "elma", "çilek", "karpuz", "armut", "elma", "armut" };
            foreach (String word in words)
            {
                builder.Add(word);
            }
            Assert.AreEqual(4, builder.Size());
            Assert.AreEqual(0, builder.IndexOf("elma"));
            Assert.AreEqual(1, builder.IndexOf("çilek"));
            Assert.AreEqual(2, builder.IndexOf("karpuz"));
            Assert.AreEqual(-1, builder.IndexOf("mango"));

            List<int> list = new List<int>(builder.AlphabeticallySortedWordsIds());
            Assert.IsTrue(Enumerable.SequenceEqual(new List<int> { 3, 0, 2, 1 }, list));

            list = new List<int>(builder.AlphabeticallySortedWordsIds(CultureInfo.GetCultureInfo("tr")));
            Assert.IsTrue(Enumerable.SequenceEqual(new List<int> { 3, 1, 0, 2 }, list));

            LmVocabulary vocab = builder.Generate();
            Assert.AreEqual(7, vocab.Size());
        }

        [TestMethod]
        public void ContainsAffixesTest()
        {
            LmVocabulary vocabulary = new LmVocabulary("a", "_b", "c", "d", "e");
            Assert.IsTrue(vocabulary.ContainsSuffix());
            vocabulary = new LmVocabulary("a", "-b", "c", "d", "e");
            Assert.IsTrue(vocabulary.ContainsSuffix());
            vocabulary = new LmVocabulary("a-", "-b", "c", "d", "e");
            Assert.IsTrue(vocabulary.ContainsPrefix());
            vocabulary = new LmVocabulary("a_", "b", "c", "d", "e");
            Assert.IsTrue(vocabulary.ContainsPrefix());
            vocabulary = new LmVocabulary("a", "b", "c", "d", "e");
            Assert.IsFalse(vocabulary.ContainsSuffix());
            Assert.IsFalse(vocabulary.ContainsPrefix());
        }
    }
}
