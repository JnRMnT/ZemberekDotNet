using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZemberekDotNet.Core.IO
{
    /// <summary>
    /// Helper methods for string operations.
    /// </summary>
    public class Strings
    {
        /// <summary>
        /// Zero length string array.
        /// </summary>
        public static readonly string[] EmptyStringArray = new string[0];

        /// <summary>
        /// <p>The maximum size to which the padding constant(s) can expand.</p>
        /// </summary>
        private static readonly int padLimit = 8192;
        private static readonly Regex multiSpace = new Regex(" +");
        private static readonly Regex WhiteSpaceExceptSpace = new Regex("[\\t\\n\\x0B\\f\\r]");
        private static readonly Regex WhiteSpace = new Regex("\\s");

        private Strings()
        {
        }

        /// <summary>
        /// checks if a string has text content other than white space.
        /// </summary>
        /// <param name="s">value to check</param>
        /// <returns>true if it is not null, or has content other than white space white space</returns>
        public static bool HasText(string s)
        {
            return s != null && s.Length > 0 && s.Trim().Length > 0;
        }

        /// <summary>
        /// checks if all of the strings has text (NOT null, zero length or only whitespace)
        /// </summary>
        /// <param name="strings">arbitary number of strings.</param>
        /// <returns>true if ALL strings contain text.</returns>
        public static bool AllHasText(params string[] strings)
        {
            CheckVarArgString(strings);
            foreach (string s in strings)
            {
                if (!HasText(s))
                {
                    return false;
                }
            }
            return true;
        }

        private static void CheckVarArgString(params string[] strings)
        {
            if (strings == null)
            {
                throw new NullReferenceException("Input array should be non null!");
            }
            if (strings.Length == 0)
            {
                throw new ArgumentException("At least one parameter is required.");
            }
        }

        // ContainsNone
        //-----------------------------------------------------------------------
        /// <summary>
        ///  checks if all of the strings are empty (null or zero length)
        /// </summary>
        /// <param name="strings">arbitrry number of strings.</param>
        /// <returns>true if all strings are empty</returns>
        public static bool AllNullOrEmpty(params string[] strings)
        {
            CheckVarArgString(strings);
            foreach (string s in strings)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Trims white spaces from left side.
        /// </summary>
        /// <param name="s">input string</param>
        /// <returns>a new string left white spaces trimmed. null, if <code>s</code> is null</returns>
        public static string LeftTrim(string s)
        {
            if (s == null)
            {
                return null;
            }
            if (s.Length == 0)
            {
                return string.Empty;
            }
            int j = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    j++;
                }
                else
                {
                    break;
                }
            }
            return s.Substring(j);
        }

        /// <summary>
        /// Trims white spaces from right side.
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>a new string right white spaces trimmed. null, if <code>str</code> is null</returns>
        public static string RightTrim(string str)
        {
            if (str == null)
            {
                return null;
            }
            if (str.Length == 0)
            {
                return string.Empty;
            }
            int j = str.Length;
            for (int i = str.Length - 1; i >= 0; --i)
            {
                if (char.IsWhiteSpace(str[i]))
                {
                    j--;
                }
                else
                {
                    break;
                }
            }
            return str.Substring(0, j);
        }

        /// <summary>
        /// <p>Checks that the string does not contain certain characters.</p>
        /// <p>A<code>null</code> string will return <code>true</code>. A<code>null</code> invalid
        /// character array will return <code>true</code>. An empty string ("") always returns true.</p>
        /// <p/>
        /// <pre>
        ///  strings.containsNone(null, *)       = true
        ///  strings.containsNone(*, null)       = true
        ///  strings.containsNone("", *)         = true
        ///  strings.containsNone("ab", "")      = true
        ///  strings.containsNone("abab", "xyz") = true
        ///  strings.containsNone("ab1", "xyz")  = true
        ///  strings.containsNone("abz", "xyz")  = false
        ///  </pre>
        /// </summary>
        /// <param name="str">the string to check, may be null</param>
        /// <param name="invalidCharsStr">string containing invalid chars</param>
        /// <returns>true if it contains none of the invalid chars, or is null</returns>
        public static bool ContainsNone(string str, string invalidCharsStr)
        {
            if (str == null || invalidCharsStr == null)
            {
                return true;
            }
            int strSize = str.Length;
            int validSize = invalidCharsStr.Length;
            for (int i = 0; i < strSize; i++)
            {
                char ch = str[i];
                for (int j = 0; j < validSize; j++)
                {
                    if (invalidCharsStr[j] == ch)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the input string only contains allowedCharacters.
        /// </summary>
        /// <param name="str">input string.</param>
        /// <param name="allowedChars">allowed characters.</param>
        /// <returns>if string contains only the allowed characters or input values are null, returns true.
        /// false otherwise.</returns>
        public static bool ContainsOnly(string str, string allowedChars)
        {
            if (str == null || allowedChars == null)
            {
                return true;
            }
            char[] allowed = allowedChars.ToCharArray();
            foreach (char c in str.ToCharArray())
            {
                bool found = false;
                foreach (char v in allowed)
                {
                    if (c == v)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Builds a string by repeating a character a specified number of times.
        /// </summary>
        /// <param name="c">the character to use to compose the string</param>
        /// <param name="count">how many times to repeat the character argument</param>
        /// <returns>a string composed of the <code>c</code> character repeated <code>count</code>
        /// times. Empty if <code>count</code> is less then 1</returns>
        public static string Repeat(char c, int count)
        {
            if (count < 1)
            {
                return string.Empty;
            }
            char[] chars = new char[count];
            Array.Fill(chars, c);
            return new string(chars);
        }


        /// <summary>
        /// Builds a string by repeating a string a specified number of times. Author Juan Antonio
        /// </summary>
        /// <param name="str"> the string to use to compose the string</param>
        /// <param name="count">how many times to repeat the string argument</param>
        /// <returns> a string composed of the <code>str</code> string repeated <code>count</code>
        /// times. null, if <code>str</code> is null. Empty if <code>count</code> is less then 1.</returns>
        public static string Repeat(string str, int count)
        {
            if (str == null)
            {
                return null;
            }
            if (count < 1)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(str.Length * count);
            for (int i = 0; i < count; i++)
            {
                builder.Append(str);
            }
            return builder.ToString();
        }

        /// <summary>
        /// reverses a string.
        /// </summary>
        /// <param name="str">input string.</param>
        /// <returns>reversed string. null, if <code>str</code> is null</returns>
        public static string Reverse(string str)
        {
            if (str == null)
            {
                return null;
            }
            if (str.Length == 0)
            {
                return string.Empty;
            }
            return new string(str.Reverse().ToArray());
        }

        /// <summary>
        /// inserts the <code>stringToInsert</code> with given <code>interval</code> starting from left.
        /// </p>
        /// <pre>("0123456", 2, "-") returns "01-23-45-6"</pre>
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="interval">interval amount</param>
        /// <param name="stringToInsert"></param>
        /// <returns>the formatted string. null, if <code>str</code> is null</returns>
        public static string InsertFromLeft(string str, int interval, string stringToInsert)
        {
            if (interval < 0)
            {
                throw new ArgumentException("interval value cannot be negative.");
            }
            if (str == null || interval == 0 || interval >= str.Length || string.IsNullOrEmpty(stringToInsert))
            {
                return str;
            }
            StringBuilder b = new StringBuilder();
            int i = 0;
            foreach (char c in str.ToCharArray())
            {
                b.Append(c);
                i++;
                if (i % interval == 0 && i <= str.Length - 1)
                {
                    b.Append(stringToInsert);
                }
            }
            return b.ToString();
        }

        /// <summary>
        /// inserts the <code>stringToInsert</code> with given <code>interval</code> starting from right.
        /// </p>
        /// <pre>("0123456", 2, "-") returns "0-12-34-56"</pre>
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="interval">interval amount</param>
        /// <param name="stringToInsert">character to insert.</param>
        /// <returns>the formatted string. null, if <code>str</code> is null</returns>
        public static string InsertFromRight(string str, int interval, string stringToInsert)
        {
            if (interval < 0)
            {
                throw new ArgumentException("interval value cannot be negative.");
            }
            if (str == null || interval == 0 || interval >= str.Length || string.IsNullOrEmpty(stringToInsert))
            {
                return str;
            }
            StringBuilder b = new StringBuilder();
            int j = 0;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                b.Append(str[i]);
                j++;
                if (j % interval == 0 && j <= str.Length - 1)
                {
                    b.Append(stringToInsert);
                }
            }
            return Reverse(b.ToString());
        }

        /// <summary>
        /// <p>Right pad a string with spaces (' ').</p>
        ///  <p/>
        ///  <p>The string is padded to the size of <code>size</code>.</p>
        ///  <p/>
        ///  <pre>
        ///  StringUtils.RightPad(null, *)   = null
        ///  StringUtils.RightPad("", 3)     = "   "
        ///  StringUtils.RightPad("bat", 3)  = "bat"
        ///  StringUtils.RightPad("bat", 5)  = "bat  "
        ///  StringUtils.RightPad("bat", 1)  = "bat"
        ///  StringUtils.RightPad("bat", -1) = "bat"
        ///  </pre>
        /// </summary>
        /// <param name="str">the string to pad out, may be null</param>
        /// <param name="size">the size to pad to</param>
        /// <returns> right padded string or original string if no padding is necessary, <code>null</code> if
        /// null string input</returns>
        public static string RightPad(string str, int size)
        {
            return RightPad(str, size, ' ');
        }

        /// <summary>
        /// <p>Right pad a string with a specified character.</p>
        /// <p/>
        /// <p>The string is padded to the size of <code>size</code>.</p>
        /// <p/>
        /// <pre>
        /// StringUtils.RightPad(null, *, *)     = null
        /// StringUtils.RightPad("", 3, 'z')     = "zzz"
        /// StringUtils.RightPad("bat", 3, 'z')  = "bat"
        /// StringUtils.RightPad("bat", 5, 'z')  = "batzz"
        /// StringUtils.RightPad("bat", 1, 'z')  = "bat"
        /// StringUtils.RightPad("bat", -1, 'z') = "bat"
        /// </pre>
        /// </summary>
        /// <param name="str">the string to pad out, may be null</param>
        /// <param name="size">the size to pad to</param>
        /// <param name="padChar">the character to pad with</param>
        /// <returns>right padded string or original string if no padding is necessary, <code>null</code> if
        /// null string input</returns>
        public static string RightPad(string str, int size, char padChar)
        {
            if (str == null)
            {
                return null;
            }
            int pads = size - str.Length;
            if (pads <= 0)
            {
                return str; // returns original string when possible
            }
            if (pads > padLimit)
            {
                return RightPad(str, size, padChar.ToString());
            }
            return string.Concat(str, Repeat(padChar, pads));
        }

        /// <summary>
        /// <p>Right pad a string with a specified string.</p>
        /// <p/>
        /// <p>The string is padded to the size of <code>size</code>.</p>
        /// <p/>
        /// <pre>
        /// StringUtils.RightPad(null, *, *)      = null;
        /// StringUtils.RightPad("", 3, "z")      = "zzz";
        /// StringUtils.RightPad("bat", 3, "yz")  = "bat";
        /// StringUtils.RightPad("bat", 5, "yz")  = "batyz";
        /// StringUtils.RightPad("bat", 8, "yz")  = "batyzyzy";
        /// StringUtils.RightPad("bat", 1, "yz")  = "bat";
        /// StringUtils.RightPad("bat", -1, "yz") = "bat";
        /// StringUtils.RightPad("bat", 5, null)  = "bat  ";
        /// StringUtils.RightPad("bat", 5, "")    = "bat  ";
        /// </pre>
        /// </summary>
        /// <param name="str">the string to pad out, may be null</param>
        /// <param name="size">the size to pad to</param>
        /// <param name="padStr">the string to pad with, null or empty treated as single space</param>
        /// <returns>right padded string or original string if no padding is necessary, <code>null</code> if
        /// null string input</returns>
        public static string RightPad(string str, int size, string padStr)
        {
            if (str == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(padStr))
            {
                padStr = " ";
            }
            int padLen = padStr.Length;
            int strLen = str.Length;
            int pads = size - strLen;
            if (pads <= 0)
            {
                return str; // returns original string when possible
            }
            if (padLen == 1 && pads <= padLimit)
            {
                return RightPad(str, size, padStr[0]);
            }

            if (pads == padLen)
            {
                return string.Concat(str, padStr);
            }
            else if (pads < padLen)
            {
                return string.Concat(str, padStr.Substring(0, pads));
            }
            else
            {
                char[] padding = new char[pads];
                char[] padChars = padStr.ToCharArray();
                for (int i = 0; i < pads; i++)
                {
                    padding[i] = padChars[i % padLen];
                }
                return string.Concat(str, padding);
            }
        }

        /// <summary>
        /// <p>Left pad a string with spaces (' ').</p>
        /// <p/>
        /// <p>The string is padded to the size of <code>size<code>.</p>
        /// <p/>
        /// <pre>
        /// StringUtils.leftPad(null, *)   = null
        /// StringUtils.leftPad("", 3)     = "   "
        /// StringUtils.leftPad("bat", 3)  = "bat"
        /// StringUtils.leftPad("bat", 5)  = "  bat"
        /// StringUtils.leftPad("bat", 1)  = "bat"
        /// StringUtils.leftPad("bat", -1) = "bat"
        /// </pre>
        /// </summary>
        /// <param name="str">the string to pad out, may be null</param>
        /// <param name="size">the size to pad to</param>
        /// <returns>left padded string or original string if no padding is necessary, <code>null</code> if
        ///  null string input</returns>
        public static string LeftPad(string str, int size)
        {
            return LeftPad(str, size, " ");
        }


        /// <summary>
        /// <p>Left pad a string with a specified character.</p>
        /// <p/>
        /// <p>Pad to a size of<code> size</code>.</p>
        /// <p/>
        /// <pre>
        /// StringUtils.leftPad(null, *, *)     = null
        /// StringUtils.leftPad("", 3, 'z')     = "zzz"
        /// StringUtils.leftPad("bat", 3, 'z')  = "bat"
        /// StringUtils.leftPad("bat", 5, 'z')  = "zzbat"
        /// StringUtils.leftPad("bat", 1, 'z')  = "bat"
        /// StringUtils.leftPad("bat", -1, 'z') = "bat"
        /// </pre>
        /// 
        /// </summary>
        /// <param name="str"> the string to pad out, may be null</param>
        /// <param name="size">the size to pad to</param>
        /// <param name="padChar">the character to pad with</param>
        /// <returns>left padded string or original string if no padding is necessary, <code>null</code> if
        /// null string input</returns>
        public static string LeftPad(string str, int size, char padChar)
        {
            if (str == null)
            {
                return null;
            }
            int pads = size - str.Length;
            if (pads <= 0)
            {
                return str; // returns original string when possible
            }
            if (pads > padLimit)
            {
                return LeftPad(str, size, padChar.ToString());
            }
            return string.Concat(Repeat(padChar, pads), str);
        }


        /// <summary>
        /// returns the initial part of a string until the first occurance of a given string. </p>
        /// <pre>
        /// ("hello","lo") -> hel
        /// ("hello", "zo") -> hello
        /// ("hello", "hello") -> "" empty string.
        /// ("hello",null)-> hello
        /// (null,"hello")-> null
        /// (null,null)-> null
        /// </pre>
        /// 
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="s">string to search first occurance.</param>
        /// <returns> the substring</returns>
        public static string SubstringUntilFirst(string str, string s)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(s))
            {
                return str;
            }
            int pos = str.IndexOf(s);
            if (pos < 0)
            {
                return str;
            }
            else
            {
                return str.Substring(0, pos);
            }
        }

        /// <summary>
        /// returns the initial part of a string until the last occurance of a given string. </p>
        /// <pre>
        /// ("hellohello","lo") -> hellohel
        /// ("hellohello","el") -> helloh
        /// ("hellolo", "zo") -> hellolo
        /// ("hello", "hello") -> "" empty string.
        /// ("hello",null)-> hello
        /// (null,"hello")-> null
        /// (null,null)-> null
        /// </pre>
        /// 
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="s">string to search last occurance.</param>
        /// <returns>the substring</returns>
        public static string SubstringUntilLast(string str, string s)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(s))
            {
                return str;
            }
            int pos = str.LastIndexOf(s);
            if (pos < 0)
            {
                return str;
            }
            return str.Substring(0, pos);
        }

        /// <summary>
        /// <p>returns the last part of a string after the first occurance of a given string.</p> </p>
        /// <pre>
        /// ("hello","el") -> lo
        /// ("hellohello","el") -> lohello
        /// ("hello", "zo") -> hello
        /// ("hello", "hello") -> "" empty string.
        /// ("hello",null)-> hello
        /// (null,"hello")-> null
        /// (null,null)-> null
        /// </pre>
        ///
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="s">string to search first occurance.</param>
        /// <returns>the substring</returns>
        public static string SubstringAfterFirst(string str, string s)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(s))
            {
                return str;
            }
            int pos = str.IndexOf(s);
            if (pos < 0)
            {
                return str;
            }
            else
            {
                return str.Substring(pos + s.Length);
            }
        }

        /// <summary>
        /// returns the last part of a string after the last occurance of a given string. </p>
        /// <pre>
        /// ("hello","el") -> lo
        /// ("hellohello","el") -> lo
        /// ("hello", "zo") -> hello
        /// ("hello", "hello") -> "" empty string.
        /// ("hello",null)-> hello
        /// (null,"hello")-> null
        /// (null,null)-> null
        /// </pre>
        /// 
        /// </summary>
        /// <param name="str">input string</param>
        /// <param name="s">string to search first occurance.</param>
        /// <returns>the substring</returns>
        public static string substringAfterLast(string str, string s)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(s))
            {
                return str;
            }
            int pos = str.LastIndexOf(s);
            if (pos < 0)
            {
                return str;
            }
            else
            {
                return str.Substring(pos + s.Length);
            }
        }

        /// <summary>
        /// <p>Left pad a string with a specified string.</p>
        /// <p/>
        /// <p>Pad to a size of<code> size</code>.</p>
        /// <p/>
        /// <pre>
        /// StringUtils.leftPad(null, *, *)      = null
        /// StringUtils.leftPad("", 3, "z")      = "zzz"
        /// StringUtils.leftPad("bat", 3, "yz")  = "bat"
        /// StringUtils.leftPad("bat", 5, "yz")  = "yzbat"
        /// StringUtils.leftPad("bat", 8, "yz")  = "yzyzybat"
        /// StringUtils.leftPad("bat", 1, "yz")  = "bat"
        /// StringUtils.leftPad("bat", -1, "yz") = "bat"
        /// StringUtils.leftPad("bat", 5, null)  = "  bat"
        /// StringUtils.leftPad("bat", 5, "")    = "  bat"
        /// </pre>
        /// 
        /// </summary>
        /// <param name="str">the string to pad out, may be null</param>
        /// <param name="size">the size to pad to</param>
        /// <param name="padStr">the string to pad with, null or empty treated as single space</param>
        /// <returns>left padded string or original string if no padding is necessary, <code>null</code> if
        /// null string input</returns>
        public static string LeftPad(string str, int size, string padStr)
        {
            if (str == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(padStr))
            {
                padStr = " ";
            }
            int padLen = padStr.Length;
            int strLen = str.Length;
            int pads = size - strLen;
            if (pads <= 0)
            {
                return str; // returns original string when possible
            }
            if (pads == padLen)
            {
                return string.Concat(padStr, str);
            }
            else if (pads < padLen)
            {
                return string.Concat(padStr.Substring(0, pads), str);
            }
            else
            {
                char[] padding = new char[pads];
                char[] padChars = padStr.ToCharArray();
                for (int i = 0; i < pads; i++)
                {
                    padding[i] = padChars[i % padLen];
                }
                return string.Concat(padding, str);
            }
        }

        /// <summary>
        /// Converts all white spaces to single space. Also eliminates multiple spaces, </p>
        /// 
        /// <pre>
        /// "  a  aaa \t \n    a\taa  " -> " a aaa a aa "
        /// </pre>
        /// 
        /// @param str input string.
        /// @return all white spaces are converted to space character and multiple space chars reduced to
        /// single space.returns null if <code>str<code> is null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string WhiteSpacesToSingleSpace(string str)
        {
            if (str == null)
            {
                return null;
            }
            if (str.IsEmpty())
            {
                return str;
            }
            return multiSpace.Replace(WhiteSpaceExceptSpace.Replace(str, " "), " ");
        }

        /// <summary>
        /// Eliminates all white spaces.
        /// </summary>
        /// <param name="str">input string.</param>
        /// <returns>returns the string after all white spaces are stripped. null, if str is null</returns>
        public static string EliminateWhiteSpaces(string str)
        {
            if (str == null)
            {
                return null;
            }
            if (str.IsEmpty())
            {
                return str;
            }
            return WhiteSpace.Replace(str, "");
        }

        /// <summary>
        /// Generates 'gram' strings from a given string. Such as, </p>
        /// <pre>
        /// for ("hello",2) it returns["he", "el", "ll", "lo"]
        /// for ("hello",3) it returns["hel", "ell", "llo"]
        /// </pre>
        /// </summary>
        /// <param name="word">input string</param>
        /// <param name="gramSize">size of the gram.</param>
        /// <returns>the grams as an array. if the gram size is larger than the word itself, it retuns an
        /// empty array. gram size cannot be smaller than 1</returns>
        public static string[] separateGrams(string word, int gramSize)
        {
            if (gramSize < 1)
            {
                throw new ArgumentException("Gram size cannot be smaller than 1");
            }
            if (gramSize > word.Length)
            {
                return EmptyStringArray;
            }
            string[] grams = new string[word.Length - gramSize + 1];
            for (int i = 0; i <= word.Length - gramSize; i++)
            {
                grams[i] = word.Substring(i, i + gramSize);
            }
            return grams;
        }
    }
}
