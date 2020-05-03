using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Collections;
using System.Linq;
using ZemberekDotNet.Core.IO;
using System.Diagnostics.Contracts;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.Text
{
    public class TextUtil
    {
        public static readonly Func<string, string[]> SpaceSplitter = (string input) =>
        {
            return input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToArray();
        };
        public static readonly Func<string, string[]> TabSplitter = (string input) =>
        {
            return Regex.Split(input, @"\t").Select(e => e.ToStringOrEmpty().Trim()).Where(e => !string.IsNullOrEmpty(e)).ToArray();
        };
        public static readonly Func<string, string[]> CommaSplitter = (string input) =>
        {
            return input.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToArray();
        };

        public static readonly string HtmlStart = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
        public static readonly string MetaCharsetUtf8 = "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>";
        public static Regex HtmlTagContentPattern = Regexps.DefaultPattern("<[^>]+>");
        public static Regex HtmlCommentContentPattern = Regexps.DefaultPattern("<!--.+?-->");
        public static Regex HtmlNewlinePattern = Regexps.DefaultPattern("•|\u8286|<strong>|</strong>|</p>|<p>|</br>|<br />|<br>|</span>|<span>|<li>|</li>|<b>|</b>");
        private static readonly Regex separationPattern = new Regex("(^[^ .,!?;:0-9]+)([.,!?;:]+)([^ .,!?;:0-9]+$)");
        private static readonly Regex punctPattern = new Regex("[.,!?;:]");
        private static readonly Regex digit = new Regex("\\d+", RegexOptions.Singleline);
        private static readonly Regex onlyDigit = new Regex("^\\d+$", RegexOptions.Singleline);
        private static readonly Regex htmlBody = Regexps.DefaultPattern("<body.+?</body>");
        private static readonly Regex script = Regexps.DefaultPattern("<script.+?</script>");
        private static readonly Regex htmlMetaContentTag = Regexps.DefaultPattern("<meta http-equiv=\"content-type\".+?>");
        private static readonly Regex attributePattern = new Regex("([\\w\\-]+)([ ]*=[ ]*\")(.+?)(\")"); // catches all xml attributes in a line.
        private static readonly Dictionary<string, string> htmlstringToCharMapFull = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> htmlstringToCharMapCommon = new Dictionary<string, string>();
        private static readonly Dictionary<char, char> specialCharToSimple = new Dictionary<char, char>();
        private static readonly Regex ampersandPattern = new Regex("&[^ ]{2,6};");

        static TextUtil()
        {
            InitializeHtmlCharMap(htmlstringToCharMapFull, "Resources/Text/html-char-map-full.txt");
            InitializeHtmlCharMap(htmlstringToCharMapCommon, "Resources/Text/html-char-map-common.txt");
            initializeToSimplifiedChars();
        }

        public static List<string> GetElementChunks(string allContent, string elementName)
        {
            elementName = elementName.Trim().Replace("<>", "");
            Regex p = new Regex("(<" + elementName + ")" + "(.+?)" + "(</" + elementName + ">)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return Regexps.AllMatches(p, allContent);
        }

        public static List<string> GetSingleLineElementData(string allContent, string elementName)
        {
            elementName = elementName.Trim().Replace("<>", "");
            Regex p = new Regex("(<" + elementName + ")" + "(.+?)" + "(>)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return Regexps.AllMatches(p, allContent);
        }


        /// <summary>
        /// * returns a map with attributes of an xml line. For example if [content] is `<Foo a="one"
        /// * b="two">` and [element] is `Foo` it returns [a:one b:two] Map. It only check the first match in
        /// * the content.
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetAttributes(string content, string elementName)
        {
            elementName = elementName.Trim();
            Regex p = new Regex("(<" + elementName + ")" + "(.+?)" + "(>)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string elementLine = Regexps.FirstMatch(p, content);

            Dictionary<string, string> attributes = new Dictionary<string, string>();
            if (elementLine == null)
            {
                return attributes;
            }

            foreach (Match match in attributePattern.Matches(elementLine))
            {
                attributes.TryAdd(match.Groups[1].Value, match.Groups[3].Value);
            }

            return attributes;
        }

        /// <summary>
        /// returns a map with attributes of an xml line.For example if [content] is `<Foo a = "one"
        /// b = "two" >` it returns[a:one b:two] Map.It only checks the first match in the content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Dictionary<string, string> getAttributes(string content)
        {
            return GetAttributes(content, "");
        }

        public static FixedBitVector GenerateBitLookup(string characters)
        {
            int max = 0;
            foreach (char c in characters.ToCharArray())
            {
                if (c > max)
                {
                    max = c;
                }
            }

            FixedBitVector result = new FixedBitVector(max + 1);
            foreach (char c in characters.ToCharArray())
            {
                result.Set(c);
            }
            return result;
        }

        /// <summary>
        /// #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
        /// </summary>
        /// <param name="input">input string</param>
        /// <param name="replacement"></param>
        /// <returns>input stream with CDATA illegal characters replaces by a space character.</returns>
        public static string CleanCdataIllegalChars(string input, string replacement)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            foreach (char c in input.ToCharArray())
            {
                if ((c >= 0x20 && c <= 0xD7ff) || c == 0x9 || c == 0xa || c == 0xd || (c >= 0x10000
                    && c <= 0x10FFFF))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(replacement);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// This method converts different single and double quote symbols to a unified form.also it
        /// reduces two connected single quotes to a one double quote.
        /// </summary>
        /// <param name="input">input string.</param>
        /// <returns>cleaned input string.</returns>
        public static string NormalizeQuotesHyphens(string input)
        {
            // rdquo, ldquo, laquo, raquo, Prime sybols in unicode.
            return input
                .Replace("[\u201C\u201D\u00BB\u00AB\u2033\u0093\u0094]|''", "\"")
                .Replace("[\u0091\u0092\u2032´`’‘]", "'")
                .Replace("[\u0096\u0097–]", "-");
        }

        /// <summary>
        /// This method converts different apostrophe symbols to a unified form.
        /// </summary>
        /// <param name="input">input string.</param>
        /// <returns>cleaned input string.</returns>
        public static string NormalizeApostrophes(string input)
        {
            // rdquo, ldquo, laquo, raquo, Prime sybols in unicode.
            return Regex.Replace(input, "[\u0091\u0092\u2032´`’‘]", "'");
        }

        public static int CountChars(string s, char c)
        {
            int cnt = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c)
                {
                    cnt++;
                }
            }
            return cnt;
        }

        public static int countChars(string s, params char[] chars)
        {
            int cnt = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char cs = s[i];
                foreach (char c in chars)
                {
                    if (cs == c)
                    {
                        cnt++;
                        break;
                    }
                }
            }
            return cnt;
        }

        /// <summary>
        /// separates from punctuations. only for strings without spaces. abc.adf -> abc. adf
        /// `minWordLength` is the minimum word length to separate. For the above example, it should be >2
        /// </summary>
        /// <param name="input"></param>
        /// <param name="minWordLength"></param>
        /// <returns></returns>
        public static string SeparatePunctuationConnectedWords(string input, int minWordLength)
        {
            List<string> k = new List<string>();
            foreach (string s in input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()))
            {
                k.Add(SeparateWords(s, minWordLength));
            }
            return string.Join(" ", k);
        }

        private static string SeparateWords(string s, int wordLength)
        {
            if (!punctPattern.IsMatch(s))
            {
                return s;
            }
            Match m = separationPattern.Match(s);
            if (m.Success)
            {
                if (m.Groups[1].Value.Length >= wordLength && m.Groups[3].Value.Length >= wordLength)
                {
                    return m.Groups[1].Value + m.Groups[2].Value + " " + m.Groups[3].Value;

                }
            }
            return s;
        }

        public static double DigitRatio(string s)
        {
            if (s.Trim().Length == 0)
            {
                return 0;
            }
            int d = 0;
            foreach (char c in s)
            {
                if (char.IsDigit(c))
                {
                    d++;
                }
            }
            return (d * 1d) / s.Length;
        }

        public static bool ContainsDigit(string s)
        {
            return digit.IsMatch(s);
        }

        public static bool ContainsOnlyDigit(string s)
        {
            return onlyDigit.IsMatch(s);
        }

        public static double UppercaseRatio(string s)
        {
            if (s.Trim().Length == 0)
            {
                return 0;
            }
            int d = 0;
            foreach (char c in s)
            {
                if (char.IsUpper(c))
                {
                    d++;
                }
            }
            return (d * 1d) / s.Length;
        }

        public static string EscapeQuotesApostrpohes(string input)
        {
            return input.Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        private static void InitializeHtmlCharMap(Dictionary<string, string> map, string resource)
        {
            try
            {
                StreamReader stream = new StreamReader(new FileStream(resource, FileMode.Open, FileAccess.Read));
                Dictionary<string, string> fullMap = new KeyValueReader(":", "!").LoadFromStream(stream, "utf-8");
                foreach (string key in fullMap.Keys)
                {
                    string value = fullMap.GetValueOrDefault(key);
                    if (value.Length != 0)
                    {
                        if (value.Length > 1)
                        {
                            throw new ArgumentException(
                                "I was expecting a single or no character but:" + value);
                        }
                        map.Add("&" + key + ";", value);
                    }
                }
                // add nbrsp manually.
                map.Add("&nbsp;", " ");
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private static void initializeToSimplifiedChars()
        {
            try
            {
                Dictionary<string, string> fullMap = new KeyValueReader(":", "#")
                    .LoadFromStream(IOs.GetResourceAsStream(
                        "Resources/Text/special-char-to-simple-char.txt"), "utf-8");
                foreach (string key in fullMap.Keys)
                {
                    specialCharToSimple.Add(key[0], fullMap.GetValueOrDefault(key)[0]);
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        /// <summary>
        /// replaces all special html strings such as(&....; or &#dddd;) with their original characters.
        /// </summary>
        /// <param name="input">input which may contain html specific strings.</param>
        /// <returns>cleaned input.</returns>
        public static string ConvertAmpersandStrings(string input)
        {
            return Regexps.ReplaceMap(ampersandPattern, input, htmlstringToCharMapFull);
        }

        /// <summary>
        /// This method removes all &....; type strings form html.
        /// </summary>
        /// <param name="input"> input string</param>
        /// <returns>cleaned input.</returns>
        public static string RemoveAmpresandStrings(string input)
        {
            // TODO: Check
            // remove rest.
            return ampersandPattern.Replace(input, (match) =>
            {
                if (match.Groups[0].Value.Length < 8)
                {
                    return "";
                }
                else
                {
                    return match.Groups[0].Value;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>input, all html comment and tags are cleaned.</returns>
        public static string CleanHtmlTagsAndComments(string input)
        {
            return HtmlTagContentPattern.Replace(HtmlCommentContentPattern.Replace(input, ""), "");
        }

        public static string CleanAllHtmlRelated(string input)
        {
            return CleanHtmlTagsAndComments(RemoveAmpresandStrings(ConvertAmpersandStrings(input)));
        }

        /// <summary>
        /// it replaces several paragraph html tags with desired string.
        /// </summary>
        /// <param name="input">input string.</param>
        /// <param name="replacement">replacement.</param>
        /// <returns>content, new line html tags are replaced with a given string.</returns>
        public static string generateLineBreaksFromHtmlTags(string input, string replacement)
        {
            return HtmlNewlinePattern.Replace(input, replacement);
        }

        public static string GetHtmlBody(string html)
        {
            Contract.Requires(html != null, "input cannot be null.");
            return Regexps.FirstMatch(htmlBody, html);
        }

        public static string CleanScripts(string html)
        {
            Contract.Requires(html != null, "input cannot be null.");
            return script.Replace(html, " ");
        }

        /// <summary>
        /// it generates an HTML only containing bare head and meta tags with utf-8 charset. and body
        /// content. it also eliminates all script tags.
        /// </summary>
        /// <param name="htmlToReduce">html file to reduce.</param>
        /// <returns>reduced html file. charset is set to utf-8.</returns>
        public static string ReduceHtmlFixedUTF8Charset(string htmlToReduce)
        {
            return HtmlStart + "<html><head>" + MetaCharsetUtf8 + "</head>\n" +
                CleanScripts(GetHtmlBody(htmlToReduce)) + "</html>";
        }

        public static string ReduceHtml(string htmlToReduce)
        {
            string htmlBody = GetHtmlBody(htmlToReduce);
            if (htmlBody == null)
            {
                Log.Warn("Cannot get html body.");
                return htmlToReduce;
            }
            List<string> parts = Regexps.AllMatches(htmlMetaContentTag, htmlToReduce);
            return HtmlStart + "<html><head>" + string.Join(" ", parts) +
                "</head>\n" + CleanScripts(htmlBody) + "</html>";
        }

        /// <summary>
        /// Replaces all unicode space like characters with " " and
        /// replaces soft hyphens [u00ad].
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>normalized input.</returns>
        public static string NormalizeSpacesAndSoftHyphens(string input)
        {
            return input
                .Replace("[\\s\\u00a0\\u200b]+", " ")
                .Replace("[\\u00ad]", "").Trim();
        }

        /// <summary>
        /// Returns true iff input contains Combining Diacritics symbols.
        /// These characters sometimes appear in documents when accented or dotted
        /// non ascii characters are used (like çşğ).
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static bool ContainsCombiningDiacritics(string sentence)
        {
            for (int i = 0; i < sentence.Length; i++)
            {
                char c = sentence[i];
                if (c >= 0x300 && c <= 0x036f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
