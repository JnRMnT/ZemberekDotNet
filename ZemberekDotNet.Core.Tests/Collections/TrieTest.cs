using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Turkish;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class TrieTest
    {
        private static Random r = new Random(Convert.ToInt32("0xbeefcafe", 16));
        private static TurkishAlphabet alphabet = TurkishAlphabet.Instance;
        private Trie<Item> lt;

        private class Item
        {
            internal readonly string surfaceForm;
            internal readonly string payload;

            public Item(string surfaceForm, string payload)
            {
                this.surfaceForm = surfaceForm;
                this.payload = payload;
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || !GetType().Equals(o.GetType()))
                {
                    return false;
                }
                Item item = (Item)o;
                return Objects.Equal(surfaceForm, item.surfaceForm) &&
                    Objects.Equal(payload, item.payload);
            }

            public override int GetHashCode()
            {
                return Objects.HashCode(surfaceForm, payload);
            }

            public override string ToString()
            {
                return surfaceForm;
            }
        }

        [TestInitialize]
        public void SetUp()
        {
            lt = new Trie<Item>();
        }

        private void Additems(List<Item> items)
        {
            foreach (Item item in items)
            {
                lt.Add(item.surfaceForm, item);
            }
        }

        private List<Item> CreateItems(params string[] stems)
        {
            List<Item> items = new List<Item>();
            foreach (string s in stems)
            {
                Item Item = new Item(s, "surface form:" + s);
                items.Add(Item);
            }
            return items;
        }

        private void CheckitemsExist(List<Item> items)
        {
            foreach (Item item in items)
            {
                List<Item> stems = lt.GetPrefixMatchingItems(item.surfaceForm);
                Assert.IsTrue(stems.Contains(item), "Should have contained: " + item);
            }
        }

        private void CheckitemsMatches(string input, List<Item> items)
        {
            List<Item> stems = lt.GetPrefixMatchingItems(input);
            foreach (Item Item in items)
            {
                Assert.IsTrue(stems.Contains(Item), "Should have contained: " + Item);
            }
        }

        private void CheckitemsMustNotMatch(string input, List<Item> items)
        {
            List<Item> stems = lt.GetPrefixMatchingItems(input);
            foreach (Item Item in items)
            {
                Assert.IsFalse(stems.Contains(Item), "Must not have contained: " + Item);
            }
        }


        [TestMethod]
        public void Empty()
        {
            List<Item> stems = lt.GetPrefixMatchingItems("foo");
            Assert.AreEqual(stems.Count, 0);
        }

        [TestMethod]
        public void SingleItem()
        {
            List<Item> items = CreateItems("elma");
            Additems(items);
            CheckitemsExist(items);
        }

        [TestMethod]
        public void DistinctStems()
        {
            List<Item> items = CreateItems("elma", "armut");
            Additems(items);
            CheckitemsExist(items);
        }

        [TestMethod]
        public void GetAllTest()
        {
            List<Item> items = CreateItems("elma", "el", "arm", "armut", "a", "elmas");
            Additems(items);
            List<Item> all = lt.GetAll();
            Assert.AreEqual(6, all.Count);
        }

        [TestMethod]
        public void StemsSharingSamePrefixOrder1()
        {
            List<Item> items = CreateItems("elmas", "elma");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMatches("elma", CreateItems("elma"));
            CheckitemsMatches("elmas", CreateItems("elma", "elmas"));
        }

        [TestMethod]
        public void stemsSharingSamePrefixOrder2()
        {
            List<Item> items = CreateItems("elma", "elmas");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMatches("elma", CreateItems("elma"));
            CheckitemsMatches("elmas", CreateItems("elma", "elmas"));
        }

        [TestMethod]
        public void StemsSharingSamePrefix3Stems()
        {
            List<Item> items = CreateItems("el", "elmas", "elma");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMatches("elma", CreateItems("el", "elma"));
            CheckitemsMatches("el", CreateItems("el"));
            CheckitemsMatches("elmas", CreateItems("el", "elma", "elmas"));
            CheckitemsMatches("elmaslar", CreateItems("el", "elma", "elmas"));
        }

        [TestMethod]
        public void StemsForLongerInputs()
        {
            List<Item> items = CreateItems("el", "elmas", "elma", "ela");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMatches("e", CreateItems());
            CheckitemsMatches("el", CreateItems("el"));
            CheckitemsMatches("elif", CreateItems("el"));
            CheckitemsMatches("ela", CreateItems("el", "ela"));
            CheckitemsMatches("elastik", CreateItems("el", "ela"));
            CheckitemsMatches("elmas", CreateItems("el", "elma", "elmas"));
            CheckitemsMatches("elmaslar", CreateItems("el", "elma", "elmas"));
        }

        [TestMethod]
        public void StemsWrongMatchTest()
        {
            List<Item> items = CreateItems("e", "elma", "elmas");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMustNotMatch("elms", CreateItems("elma", "elmas"));
        }

        [TestMethod]
        public void StemsWrongMatchTest2()
        {
            List<Item> items = CreateItems("airport", "airports");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMustNotMatch("airpods", CreateItems("airports"));
        }

        [TestMethod]
        public void StemsWrongMatchTest3()
        {
            List<Item> items = CreateItems("comple", "complete");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMustNotMatch("complutense", CreateItems("complete"));
        }

        [TestMethod]
        public void RemoveStems()
        {
            List<Item> items = CreateItems("el", "elmas", "elma", "ela");
            Additems(items);
            CheckitemsExist(items);
            CheckitemsMatches("el", CreateItems("el"));
            // Remove el
            CheckitemsMatches("el", CreateItems());
            // Remove elmas
            lt.Remove(items[1].surfaceForm, items[1]);
            CheckitemsMatches("elmas", CreateItems());
            CheckitemsMatches("e", CreateItems());
            CheckitemsMatches("ela", CreateItems("ela"));
            CheckitemsMatches("elastik", CreateItems("ela"));
            CheckitemsMatches("elmas", CreateItems("el", "elma"));
            CheckitemsMatches("elmaslar", CreateItems("el", "elma"));
        }

        [TestMethod]
        public void StemsSharingPartialPrefix1()
        {
            List<Item> items = CreateItems("fix", "foobar", "foxes");
            Additems(items);
            CheckitemsExist(items);
        }

        private List<string> GenerateRandomWords(int number)
        {
            List<string> randomWords = new List<string>();
            string letters = alphabet.GetLowercaseLetters();
            for (int i = 0; i < number; i++)
            {
                int len = r.Next(20) + 1;
                char[] chars = new char[len];
                for (int j = 0; j < len; j++)
                {
                    chars[j] = letters[(r.Next(29) + 1)];
                }
                randomWords.Add(new string(chars));
            }
            return randomWords;
        }

        [TestMethod]
        public void TestBigNumberOfBigWords()
        {
            List<string> words = GenerateRandomWords(10000);
            int uniqueSize = new HashSet<string>(words).Count;
            Trie<Item> testTrie = new Trie<Item>();
            List<Item> items = new List<Item>();
            foreach (string s in words)
            {
                Item item = new Item(s, "s: " + s);
                testTrie.Add(item.surfaceForm, item);
                items.Add(item);
            }
            Assert.AreEqual(uniqueSize, testTrie.Size());
            foreach (Item item in items)
            {
                List<Item> res = testTrie.GetPrefixMatchingItems(item.surfaceForm);
                Assert.IsTrue(res.Contains(item));
                Assert.AreEqual(res[res.Count - 1].surfaceForm, item.surfaceForm);
                foreach (Item n in res)
                {
                    // Check if all stems are a prefix of last one on the tree.
                    Assert.IsTrue(res[res.Count - 1].surfaceForm.StartsWith(n.surfaceForm));
                }
            }
        }
    }
}
