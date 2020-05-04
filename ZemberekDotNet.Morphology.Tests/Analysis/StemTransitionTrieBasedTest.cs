using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Lexicon.TR;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    [TestClass]
    public class StemTransitionTrieBasedTest
    {
        [TestMethod]
        public void TestPrefix()
        {
            RootLexicon lexicon = GetLexicon();
            StemTransitionsTrieBased t = new StemTransitionsTrieBased(
                lexicon,
                new TurkishMorphotactics(lexicon));

            List<StemTransition> matches = t.GetPrefixMatches("kabağa", false);
            Assert.AreEqual(3, matches.Count);
            ISet<string> lemmas = matches.Select(s => s.item.lemma).ToHashSet();
            Assert.IsTrue(TestUtil.ContainsAll(lemmas, "kaba", "kabağ", "kabak"));

            matches = t.GetPrefixMatches("kabak", false);
            Assert.AreEqual(2, matches.Count);
            lemmas = matches.Select(s => s.item.lemma).ToHashSet();
            Assert.IsTrue(TestUtil.ContainsAll(lemmas, "kaba", "kabak"));

            matches = t.GetPrefixMatches("kapak", false);
            Assert.AreEqual(3, matches.Count);
            lemmas = matches.Select(s => s.item.lemma).ToHashSet();
            Assert.IsTrue(TestUtil.ContainsAll(lemmas, "kapak"));
        }

        [TestMethod]
        public void TestItem()
        {
            RootLexicon lexicon = GetLexicon();
            StemTransitionsTrieBased t = new StemTransitionsTrieBased(
                lexicon,
                new TurkishMorphotactics(lexicon));

            DictionaryItem item = lexicon.GetItemById("kapak_Noun");
            List<StemTransition> transitions = t.GetTransitions(item);
            Assert.AreEqual(2, transitions.Count);
            ISet<string> surfaces = transitions.Select(s => s.surface).ToHashSet();
            Assert.IsTrue(TestUtil.ContainsAll(surfaces, "kapak", "kapağ"));
        }

        private RootLexicon GetLexicon()
        {
            return TurkishDictionaryLoader.Load(
                "kapak",
                "kapak [P:Adj]",
                "kapak [A:InverseHarmony]",
                "kabak",
                "kapaklı",
                "kabağ", // <-- only for testing.
                "kaba",
                "aba",
                "aba [P:Adj]"
            );
        }
    }
}
