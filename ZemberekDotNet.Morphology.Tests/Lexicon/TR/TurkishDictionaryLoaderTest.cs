using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Lexicon.TR;

namespace ZemberekDotNet.Morphology.Tests.Lexicon.TR
{
    [TestClass]
    public class TurkishDictionaryLoaderTest
    {
        private static ItemAttrPair TestPair(string s, params RootAttribute[] attrs)
        {
            return new ItemAttrPair(s, attrs.ToHashSet());
        }

        [TestMethod]
        public void LoadNounsFromFileTest()
        {
            RootLexicon items = TurkishDictionaryLoader
        .Load("Resources/test-lexicon-nouns.txt");

            Assert.IsFalse(items.IsEmpty());
            foreach (DictionaryItem item in items)
            {
                Assert.AreSame(item.primaryPos, PrimaryPos.Noun);
            }
        }

        [TestMethod]
        public void nounInferenceTest()
        {
            DictionaryItem item = GetItem("elma");
            Assert.AreEqual("elma", item.lemma);
            Assert.AreEqual("elma", item.root);
            Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);

            item = GetItem("elma [P:Noun]");
            Assert.AreEqual("elma", item.lemma);
            Assert.AreEqual("elma", item.root);
            Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
        }

        private DictionaryItem GetItem(string itemStr)
        {
            return TurkishDictionaryLoader.LoadFromString(itemStr);
        }

        private DictionaryItem GetLastItem(params string[] itemStr)
        {
            string last = Strings.SubstringUntilFirst(itemStr[itemStr.Length - 1], " ");
            return TurkishDictionaryLoader.Load(itemStr).GetMatchingItems(last)[0];
        }

        [TestMethod]
        public void VerbInferenceTest()
        {
            DictionaryItem item = GetItem("gelmek");
            Assert.AreEqual("gel", item.root);
            Assert.AreEqual("gelmek", item.lemma);
            Assert.AreEqual(PrimaryPos.Verb, item.primaryPos);

            string[] verbs = {"germek", "yarmak", "salmak", "yermek [P:Verb]", "etmek [P:Verb; A:Voicing]",
        "etmek [A:Voicing]",
        "yıkanmak [A:Reflexive]", "küfretmek [A:Voicing, Aorist_A]"};
            foreach (string verb in verbs)
            {
                item = GetItem(verb);
                Assert.AreEqual(PrimaryPos.Verb, item.primaryPos);
            }
        }

        [TestMethod]
        public void CompoundTest()
        {
            DictionaryItem item = GetLastItem("kuyruk", "atkuyruğu [A:CompoundP3sg; Roots:at-kuyruk]");
            Assert.AreEqual("atkuyruğu", item.lemma);
            Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
        }

        [TestMethod]
        public void VoicingInferenceTest()
        {
            DictionaryItem item = TurkishDictionaryLoader.LoadFromString("aort [A:NoVoicing]");
            Assert.AreEqual("aort", item.root);
            Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
            Assert.IsTrue(item.HasAttribute(RootAttribute.NoVoicing));
            Assert.IsFalse(item.HasAttribute(RootAttribute.Voicing));

            item = TurkishDictionaryLoader.LoadFromString("at");
            Assert.AreEqual("at", item.root);
            Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
            Assert.IsTrue(item.HasAttribute(RootAttribute.NoVoicing));
            Assert.IsFalse(item.HasAttribute(RootAttribute.Voicing));
        }

        [TestMethod]
        public void PunctuationTest()
        {
            DictionaryItem item = TurkishDictionaryLoader.LoadFromString("… [P:Punc]");
            Assert.AreEqual("…", item.root);
            Assert.AreEqual(PrimaryPos.Punctuation, item.primaryPos);
        }

        [TestMethod]
        public void ProperNounsShouldNotHaveVoicingAutomaticallyTest()
        {
            DictionaryItem item = TurkishDictionaryLoader.LoadFromString("Tokat");
            Assert.AreEqual("tokat", item.root);
            Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
            Assert.AreEqual(SecondaryPos.ProperNoun, item.secondaryPos);
            Assert.IsFalse(item.HasAttribute(RootAttribute.Voicing));

            item = TurkishDictionaryLoader.LoadFromString("Dink");
            Assert.AreEqual("dink", item.root);
            Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
            Assert.AreEqual(SecondaryPos.ProperNoun, item.secondaryPos);
            Assert.IsFalse(item.HasAttribute(RootAttribute.Voicing));
        }

        [TestMethod]
        public void NounVoicingTest()
        {
            string[] voicing = {"kabak", "kabak [A:Voicing]", "psikolog", "havuç", "turp [A:Voicing]",
        "galip", "nohut", "cenk", "kükürt"};
            foreach (string s in voicing)
            {
                DictionaryItem item = TurkishDictionaryLoader.LoadFromString(s);
                Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
                Assert.IsTrue(item.HasAttribute(RootAttribute.Voicing), "error in:" + s);
            }

            string[] novoicing = { "kek", "link [A:NoVoicing]", "top", "kulp", "takat [A:NoVoicing]" };
            foreach (string s in novoicing)
            {
                DictionaryItem item = TurkishDictionaryLoader.LoadFromString(s);
                Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
                Assert.IsTrue(item.HasAttribute(RootAttribute.NoVoicing), "error in:" + s);
            }
        }

        [TestMethod]
        public void ReferenceTest1()
        {
            string[] reference = {"ad", "ad [A:Doubling,InverseHarmony]", "soy",
        "soyadı [A:CompoundP3sg; Roots:soy-ad]"};
            RootLexicon lexicon = TurkishDictionaryLoader.Load(reference);
            DictionaryItem item = lexicon.GetItemById("soyadı_Noun");
            Assert.IsNotNull(item);
            Assert.IsFalse(item.attributes.Contains(RootAttribute.Doubling));
        }

        [TestMethod]
        public void ReferenceTest2()
        {
            string[] reference = {"ad", "ad [A:Doubling,InverseHarmony;Index:1]", "soy",
        "soyadı [A:CompoundP3sg; Roots:soy-ad]"};
            RootLexicon lexicon = TurkishDictionaryLoader.Load(reference);
            DictionaryItem item = lexicon.GetItemById("soyadı_Noun");
            Assert.IsNotNull(item);
            Assert.IsFalse(item.attributes.Contains(RootAttribute.Doubling));
        }

        [TestMethod]
        public void Pronunciation1()
        {
            string[] reference = {
        "VST [P:Noun, Abbrv; Pr:viesti]",
        "VST [P:Noun, Abbrv; Pr:vesete; Ref:VST_Noun_Abbrv; Index:2]"};
            RootLexicon lexicon = TurkishDictionaryLoader.Load(reference);
            DictionaryItem item = lexicon.GetItemById("VST_Noun_Abbrv");
            Assert.IsNotNull(item);
            DictionaryItem item2 = lexicon.GetItemById("VST_Noun_Abbrv_2");
            Assert.IsNotNull(item2);
            Assert.AreEqual(item, item2.GetReferenceItem());
        }

        [TestMethod]
        public void ImplicitP3sgTest()
        {
            string[] lines = {
        "üzeri [A:CompoundP3sg;Roots:üzer]"};
            RootLexicon lexicon = TurkishDictionaryLoader.Load(lines);
            Assert.AreEqual(2, lexicon.Size());
        }

        [TestMethod]
        public void NounAttributesTest()
        {
            List<ItemAttrPair> testList = new List<ItemAttrPair> {
                TestPair("takat [A:NoVoicing, InverseHarmony]", RootAttribute.NoVoicing, RootAttribute.InverseHarmony),
                TestPair("nakit [A: LastVowelDrop]", RootAttribute.Voicing, RootAttribute.LastVowelDrop),
                TestPair("ret [A:Voicing, Doubling]", RootAttribute.Voicing, RootAttribute.Doubling)
            };
            foreach (ItemAttrPair pair in testList)
            {
                DictionaryItem item = TurkishDictionaryLoader.LoadFromString(pair.Str);
                Assert.AreEqual(PrimaryPos.Noun, item.primaryPos);
                Assert.IsTrue(pair.Attrs.SetEquals(item.attributes), "error in:" + pair.Str);
            }
        }

        [TestMethod]
        [Ignore("Not a unit Test. Converts word histogram to word list")]
        public void PrepareWordListFromHistogram()
        {
            List<string> hist = SimpleTextReader
                .TrimmingUTF8Reader("test/data/all-turkish-noproper.txt.tr").AsStringList();
            List<string> all = new List<string>();
            foreach (string s in hist)
            {
                all.Add(Strings.SubstringUntilFirst(s, " ").Trim());
            }
            SimpleTextWriter.OneShotUTF8Writer("Resources/z2-vocab.tr")
                .WriteLines(all);
        }

        [TestMethod]
        [Ignore("Not a unit test")]
        public void ShouldPrintItemsInDevlDictionary()
        {
            RootLexicon items = TurkishDictionaryLoader
                .Load("Resources/dev-lexicon.txt");
            foreach (DictionaryItem item in items)
            {
                Console.WriteLine(item);
            }
        }

        [TestMethod]
        [Ignore("Not a unit test")]
        public void SaveFullAttributes()
        {
            RootLexicon items = TurkishDictionaryLoader.LoadDefaultDictionaries();
            StreamWriter p = new StreamWriter(File.OpenWrite("dictionary-all-attributes.txt"), Encoding.UTF8);
            foreach (DictionaryItem item in items)
            {
                p.WriteLine(item.ToString());
            }
        }

        private class ItemAttrPair
        {
            string str;
            ISet<RootAttribute> attrs;

            internal ItemAttrPair(string str, ISet<RootAttribute> attrs)
            {
                this.Str = str;
                this.Attrs = attrs;
            }

            public ISet<RootAttribute> Attrs { get => attrs; set => attrs = value; }
            public string Str { get => str; set => str = value; }
        }
    }
}
