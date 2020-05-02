using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Tokenization.Tests
{
    [TestClass]
    public class TurkishTokenizerTest
    {
        private void MatchToken(
      TurkishTokenizer tokenizer,
      String input,
      Token.Type? tokenType,
      params string[] expectedTokens)
        {
            List<Token> tokens = tokenizer.Tokenize(input);
            Assert.IsNotNull(tokens, "Token list is null.");
            Assert.IsTrue(tokens.Count > 0);
            Assert.AreEqual(expectedTokens.Length, tokens.Count, "Token count is not equal to expected Token count for input " + input);
            int i = 0;
            foreach (string expectedToken in expectedTokens)
            {
                Token token = tokens[i];
                Assert.AreEqual(expectedToken, token.GetText(), expectedToken + " is not equal to " + token.GetText());
                if (tokenType != null)
                {
                    Assert.AreEqual(tokenType, token.GetTokenType());
                }
                i++;
            }
        }

        private string GetTokensAsString(TurkishTokenizer tokenizer, String input)
        {
            List<string> elements = tokenizer.TokenizeToStrings(input);
            return string.Join(" ", elements);
        }

        private void MatchToken(TurkishTokenizer tokenizer, string input, params string[] expectedTokens)
        {
            MatchToken(tokenizer, input, null, expectedTokens);
        }

        private void MatchSentences(
            TurkishTokenizer tokenizer,
            string input,
            string expectedJoinedTokens)
        {
            string actual = GetTokensAsString(tokenizer, input);
            Assert.AreEqual(expectedJoinedTokens, actual);
        }

        [TestMethod]
        public void TestInstances()
        {
            // default ignores white spaces and new lines.
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "a b \t c   \n \r", "a", "b", "c");
            // ALL tokenizer catches all tokens.
            t = TurkishTokenizer.All;
            MatchToken(t, " a b\t\n\rc", " ", "a", " ", "b", "\t", "\n", "\r", "c");
            // A tokenizer only catches Number type (not date or times).
            t = TurkishTokenizer.Builder().IgnoreAll().AcceptTypes(Token.Type.Number).Build();
            MatchToken(t, "www.foo.bar 12,4'ü a@a.v ; ^% 2 adf 12 \r \n ", "12,4'ü", "2", "12");
        }

        [TestMethod]
        public void TestNumbers()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "1", Token.Type.Number, "1");
            MatchToken(t, "12", Token.Type.Number, "12");
            MatchToken(t, "3.14", Token.Type.Number, "3.14");
            MatchToken(t, "-1", Token.Type.Number, "-1");
            MatchToken(t, "-1.34", Token.Type.Number, "-1.34");
            MatchToken(t, "-3,14", Token.Type.Number, "-3,14");
            MatchToken(t, "100'e", Token.Type.Number, "100'e");
            MatchToken(t, "3.14'ten", Token.Type.Number, "3.14'ten");
            MatchToken(t, "%2.5'ten", Token.Type.PercentNumeral, "%2.5'ten");
            MatchToken(t, "%2", Token.Type.PercentNumeral, "%2");
            MatchToken(t, "2.5'a", Token.Type.Number, "2.5'a");
            MatchToken(t, "2.5’a", Token.Type.Number, "2.5’a");
        }

        [TestMethod]
        public void TestWords()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "kedi", Token.Type.Word, "kedi");
            MatchToken(t, "Kedi", "Kedi");
            MatchToken(t, "Ahmet'e", "Ahmet'e");
        }

        [TestMethod]
        public void TestTags()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "<kedi>", Token.Type.MetaTag, "<kedi>");
            MatchToken(
                t,
                "<kedi><eti><7>",
                Token.Type.MetaTag,
                "<kedi>", "<eti>", "<7>");
        }

        [TestMethod]
        public void TestAlphaNumerical()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t,
                "F-16'yı, (H1N1) H1N1'den.",
                "F-16'yı , ( H1N1 ) H1N1'den .");
        }

        [TestMethod]
        public void TestCapitalWords()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "TCDD", "TCDD");
            MatchToken(t, "I.B.M.", "I.B.M.");
            MatchToken(t, "TCDD'ye", "TCDD'ye");
            MatchToken(t, "I.B.M.'nin", "I.B.M.'nin");
            MatchToken(t, "I.B.M'nin", "I.B.M'nin");
            MatchSentences(t, "İ.Ö,Ğ.Ş", "İ.Ö , Ğ.Ş");
            MatchSentences(t, "İ.Ö,", "İ.Ö ,");
            MatchSentences(t, "İ.Ö.,Ğ.Ş.", "İ.Ö. , Ğ.Ş.");
        }

        [TestMethod]
        public void TestAbbreviations()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "Prof.", "Prof.");
            MatchToken(t, "yy.", "yy.");
            MatchSentences(t, "kedi.", "kedi .");
        }

        [TestMethod]
        public void TestApostrophes()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "foo'f", "foo'f");
            MatchToken(t, "foo’f", "foo’f");
            MatchSentences(t, "’foo", "’ foo");
            MatchSentences(t, "’’foo’", "’ ’ foo ’");
            MatchSentences(t, "'foo'", "' foo '");
            MatchSentences(t, "'foo'fo", "' foo'fo");
            MatchSentences(t, "‘foo'fo’", "‘ foo'fo ’");
        }

        [TestMethod]
        public void TestTokenBoundaries()
        {
            TurkishTokenizer t = TurkishTokenizer.All;
            List<Token> tokens = t.Tokenize("bir av. geldi.");
            Token t0 = tokens[0];
            Assert.AreEqual("bir", t0.GetText());
            Assert.AreEqual(0, t0.GetStart());
            Assert.AreEqual(2, t0.GetEnd());


            Token t1 = tokens[1];
            Assert.AreEqual(" ", t1.GetText());
            Assert.AreEqual(3, t1.GetStart());
            Assert.AreEqual(3, t1.GetEnd());


            Token t2 = tokens[2];
            Assert.AreEqual("av.", t2.GetText());
            Assert.AreEqual(4, t2.GetStart());
            Assert.AreEqual(6, t2.GetEnd());

            Token t3 = tokens[3];
            Assert.AreEqual(" ", t3.GetText());
            Assert.AreEqual(7, t3.GetStart());
            Assert.AreEqual(7, t3.GetEnd());


            Token t4 = tokens[4];
            Assert.AreEqual("geldi", t4.GetText());
            Assert.AreEqual(8, t4.GetStart());
            Assert.AreEqual(12, t4.GetEnd());


            Token t5 = tokens[5];
            Assert.AreEqual(".", t5.GetText());
            Assert.AreEqual(13, t5.GetStart());
            Assert.AreEqual(13, t5.GetEnd());
        }

        [TestMethod]
        public void TestAbbreviations2()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            MatchSentences(t,
                "Prof. Dr. Ahmet'e git! dedi Av. Mehmet.",
                "Prof. Dr. Ahmet'e git ! dedi Av. Mehmet .");
        }

        [TestMethod]
        public void TestCapitalLettersAfterQuotesIssue64()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            MatchSentences(t, "Ankaraya.", "Ankaraya .");
            MatchSentences(t, "Ankara'ya.", "Ankara'ya .");
            MatchSentences(t, "ANKARA'ya.", "ANKARA'ya .");
            MatchSentences(t, "ANKARA'YA.", "ANKARA'YA .");
            MatchSentences(t, "Ankara'YA.", "Ankara'YA .");
            MatchSentences(t, "Ankara'Ya.", "Ankara'Ya .");
        }

        [TestMethod]
        public void TestUnknownWord1()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t, "زنبورك", "زنبورك");
        }

        [TestMethod]
        public void TestUnderscoreWords()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t, "__he_llo__", "__he_llo__");
        }

        [TestMethod]
        public void TestDotInMiddle()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t, "Ali.gel.", "Ali . gel .");
        }

        [TestMethod]
        public void TestPunctuation()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t,
                ".,!:;$%\"\'()[]{}&@®™©℠",
                ". , ! : ; $ % \" \' ( ) [ ] { } & @ ® ™ © ℠");
            MatchToken(t, "...", "...");
            MatchToken(t, "(!)", "(!)");
        }

        [TestMethod]
        public void TestTokenizeSentence()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t, "Ali gel.", "Ali gel .");
            MatchSentences(t, "(Ali gel.)", "( Ali gel . )");
            MatchSentences(t, "Ali'ye, gel...", "Ali'ye , gel ...");
            MatchSentences(t, "\"Ali'ye\", gel!...", "\" Ali'ye \" , gel ! ...");
            MatchSentences(t, "[Ali]{gel}", "[ Ali ] { gel }");
        }

        [TestMethod]
        public void TestTokenizeDoubleQuote()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            MatchSentences(t, "\"Soner\"'e boyle dedi", "\" Soner \" ' e boyle dedi");
            MatchSentences(t, "Hey \"Ali\" gel.", "Hey \" Ali \" gel .");
            MatchSentences(t, "\"Soner boyle dedi\"", "\" Soner boyle dedi \"");
        }

        [TestMethod]
        public void TestNewline()
        {
            TurkishTokenizer tokenizer = TurkishTokenizer.All;
            MatchToken(tokenizer, "Hey \nAli naber\n", "Hey", " ", "\n", "Ali", " ", "naber", "\n");
            MatchToken(tokenizer, "Hey\n\r \n\rAli\n \n\n \n naber\n",
                "Hey", "\n", "\r", " ", "\n", "\r", "Ali", "\n", " ", "\n", "\n", " ", "\n", " ", "naber",
                "\n");
        }

        //TODO: failing.
        [TestMethod]
        [Ignore]
        public void TestUnknownWord()
        {
            TurkishTokenizer tokenizer = TurkishTokenizer.Default;
            MatchSentences(tokenizer, "L'Oréal", "L'Oréal");
        }

        [TestMethod]
        public void TestUnknownWord2()
        {
            TurkishTokenizer tokenizer = TurkishTokenizer.Default;
            MatchSentences(tokenizer, "Bjørn", "Bjørn");
        }

        [TestMethod]
        public void TestTimeToken()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t,
                "Saat, 10:20 ile 00:59 arasinda.",
                "Saat , 10:20 ile 00:59 arasinda .");
            MatchToken(t, "10:20", Token.Type.Time, "10:20");
        }

        [TestMethod]
        public void TestTimeTokenSeconds()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchToken(t, "10:20:53", Token.Type.Time, "10:20:53");
            MatchToken(t, "10.20.00'da", Token.Type.Time, "10.20.00'da");
        }

        [TestMethod]
        public void testDateToken()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            MatchSentences(t,
                "1/1/2011 02.12.1998'de.",
                "1/1/2011 02.12.1998'de .");
            MatchToken(t, "1/1/2011", Token.Type.Date, "1/1/2011");
            MatchToken(t, "02.12.1998'de", Token.Type.Date, "02.12.1998'de");
        }

        [TestMethod]
        public void TestEmoticonToken()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            string[] emoticons = {
        ":)", ":-)", ":-]", ":D", ":-D", "8-)", ";)", ";‑)", ":(", ":-(",
        ":'(", ":‑/", ":/", ":^)", "¯\\_(ツ)_/¯", "O_o", "o_O", "O_O", "\\o/"
    };
            foreach (string s in emoticons)
            {
                MatchToken(t, s, Token.Type.Emoticon, s);
            }
        }

        [TestMethod]
        public void TestUrl()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            string[] urls = {
        "http://t.co/gn32szS9",
        "http://foo.im/lrıvn",
        "http://www.fo.bar",
        "http://www.fo.bar'da",
        "https://www.fo.baz.zip",
        "www.fo.tar.kar",
        "www.fo.bar",
        "fo.com",
        "fo.com.tr",
        "fo.com.tr/index.html",
        "fo.com.tr/index.html?",
        "foo.net",
        "foo.net'e",
        "www.foo.net'te",
        "http://www.foo.net/showthread.php?134628-ucreti",
        "http://www.foo.net/showthread.php?1-34--628-ucreti+",
        "https://www.hepsiburada.com'dan",
    };
            foreach (string s in urls)
            {
                MatchToken(t, s, Token.Type.URL, s);
            }
        }

        [TestMethod]
        public void TestUrl2()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            string[] urls = {
        "https://www.google.com.tr/search?q=bla+foo&oq=blah+net&aqs=chrome.0.0l6",
        "https://www.google.com.tr/search?q=bla+foo&oq=blah+net&aqs=chrome.0.0l6.5486j0j4&sourceid=chrome&ie=UTF-8"
    };
            foreach (string s in urls)
            {
                MatchToken(t, s, Token.Type.URL, s);
            }
        }

        [TestMethod]
        public void TestEmail()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            string[] emails = {
        "fo@bar.baz",
        "fo.bar@bar.baz",
        "fo_.bar@bar.baz",
        "ali@gmail.com'u"
    };
            foreach (string s in emails)
            {
                MatchToken(t, s, Token.Type.Email, s);
            }
        }

        [TestMethod]
        public void MentionTest()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            string[] ss = {
        "@bar",
        "@foo_bar",
        "@kemal'in"
    };
            foreach (string s in ss)
            {
                MatchToken(t, s, Token.Type.Mention, s);
            }
        }

        [TestMethod]
        public void HashTagTest()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;

            String[] ss = {
        "#foo",
        "#foo_bar",
        "#foo_bar'a"
    };
            foreach (string s in ss)
            {
                MatchToken(t, s, Token.Type.HashTag, s);
            }
        }

        [TestMethod]
        public void TestEllipsis()
        {
            TurkishTokenizer t = TurkishTokenizer.Default;
            MatchSentences(t, "Merhaba, Kaya Ivır ve Tunç Zıvır…", "Merhaba , Kaya Ivır ve Tunç Zıvır …");
        }

        [TestMethod]
        [Ignore("Not an actual test. Requires external data.")]
        public void Performance()
        {
            TurkishTokenizer tokenizer = TurkishTokenizer.Default;

            // load a hundred thousand lines.
            for (int it = 0; it < 5; it++)
            {
                string[] lines = File.ReadAllLines("/media/aaa/Data/aaa/corpora/dunya.100k");
                Stopwatch clock = Stopwatch.StartNew();
                long tokenCount = 0;
                foreach (string line in lines)
                {
                    List<Token> tokens = tokenizer.Tokenize(line);
                    tokenCount += tokens.Count;
                }
                long elapsed = clock.ElapsedMilliseconds;
                Log.Info("Token count = {0} ", tokenCount);
                Log.Info("Speed (tps) = {0:F1}", tokenCount * 1000d / elapsed);
            }
        }
    }
}
