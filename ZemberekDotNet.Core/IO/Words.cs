using System;
using System.Globalization;
using System.Text;

namespace ZemberekDotNet.Core.IO
{
    public class Words
    {
        private Words()
        {
        }

        // Wrapping
        //-----------------------------------------------------------------------

        /**
         * <p>Wraps a single line of text, identifying words by <code>' '</code>.</p>
         * <p/>
         * <p>New lines will be separated by the system property line separator. Very long words, such as
         * URLs will <i>not</i> be wrapped.</p>
         * <p/>
         * <p>Leading spaces on a new line are stripped. Trailing spaces are not stripped.</p>
         * <p/>
         * <pre>
         * WordUtils.wrap(null, *) = null
         * WordUtils.wrap("", *) = ""
         * </pre>
         *
         * @param str the string to be word wrapped, may be null
         * @param wrapLength the column to wrap the words at, less than 1 is treated as 1
         * @return a line with newlines inserted, <code>null</code> if null input
         */
        public static string Wrap(string str, int wrapLength)
        {
            return Wrap(str, wrapLength, null, false);
        }

        /**
         * <p>Wraps a single line of text, identifying words by <code>' '</code>.</p>
         * <p/>
         * <p>Leading spaces on a new line are stripped. Trailing spaces are not stripped.</p>
         * <p/>
         * <pre>
         * WordUtils.wrap(null, *, *, *) = null
         * WordUtils.wrap("", *, *, *) = ""
         * </pre>
         *
         * @param str the string to be word wrapped, may be null
         * @param wrapLength the column to wrap the words at, less than 1 is treated as 1
         * @param newLineStr the string to insert for a new line, <code>null</code> uses the system
         * property line separator
         * @param wrapLongWords true if long words (such as URLs) should be wrapped
         * @return a line with newlines inserted, <code>null</code> if null input
         */
        public static string Wrap(string str, int wrapLength, string newLineStr, bool wrapLongWords)
        {
            if (str == null)
            {
                return null;
            }
            if (newLineStr == null)
            {
                newLineStr = Environment.NewLine;
            }
            if (wrapLength < 1)
            {
                wrapLength = 1;
            }
            int inputLineLength = str.Length;
            int offset = 0;
            StringBuilder wrappedLine = new StringBuilder(inputLineLength + 32);

            while ((inputLineLength - offset) > wrapLength)
            {
                if (str[offset] == ' ')
                {
                    offset++;
                    continue;
                }
                int spaceToWrapAt = str.LastIndexOf(' ', wrapLength + offset);

                if (spaceToWrapAt >= offset)
                {
                    // normal case
                    wrappedLine.Append(str.Substring(offset, spaceToWrapAt));
                    wrappedLine.Append(newLineStr);
                    offset = spaceToWrapAt + 1;

                }
                else
                {
                    // really long word or URL
                    if (wrapLongWords)
                    {
                        // wrap really long word one line at a time
                        wrappedLine.Append(str.Substring(offset, wrapLength + offset));
                        wrappedLine.Append(newLineStr);
                        offset += wrapLength;
                    }
                    else
                    {
                        // do not wrap really long word, just extend beyond limit
                        spaceToWrapAt = str.IndexOf(' ', wrapLength + offset);
                        if (spaceToWrapAt >= 0)
                        {
                            wrappedLine.Append(str.Substring(offset, spaceToWrapAt));
                            wrappedLine.Append(newLineStr);
                            offset = spaceToWrapAt + 1;
                        }
                        else
                        {
                            wrappedLine.Append(str.Substring(offset));
                            offset = inputLineLength;
                        }
                    }
                }
            }

            // Whatever is left in line is short enough to just pass through
            wrappedLine.Append(str.Substring(offset));

            return wrappedLine.ToString();
        }

        // Capitalizing
        //-----------------------------------------------------------------------

        /**
         * <p>Capitalizes all the whitespace separated words in a string. Only the first letter of each
         * word is changed. To convert the rest of each word to lowercase at the same time, use {@link
         * #capitalizeFully(string)}.</p>
         * <p/>
         * <p>Whitespace is defined by {@link Character#isWhitespace(char)}. A <code>null</code> input
         * string returns <code>null</code>. Capitalization uses the unicode title case, normally
         * equivalent to upper case.</p>
         * <p/>
         * <pre>
         * WordUtils.capitalize(null)        = null
         * WordUtils.capitalize("")          = ""
         * WordUtils.capitalize("i am FINE") = "I Am FINE"
         * </pre>
         *
         * @param str the string to capitalize, may be null
         * @return capitalized string, <code>null</code> if null string input
         * @see #uncapitalize(string)
         * @see #capitalizeFully(string)
         */
        public static string Capitalize(string str)
        {
            return Capitalize(str, null);
        }

        /**
         * <p>Capitalizes all the delimiter separated words in a string. Only the first letter of each
         * word is changed. To convert the rest of each word to lowercase at the same time, use {@link
         * #capitalizeFully(string, char[])}.</p>
         * <p/>
         * <p>The delimiters represent a set of characters understood to separate words. The first string
         * character and the first non-delimiter character after a delimiter will be capitalized. </p>
         * <p/>
         * <p>A <code>null</code> input string returns <code>null</code>. Capitalization uses the unicode
         * title case, normally equivalent to upper case.</p>
         * <p/>
         * <pre>
         * WordUtils.capitalize(null, *)            = null
         * WordUtils.capitalize("", *)              = ""
         * WordUtils.capitalize(*, new char[0])     = *
         * WordUtils.capitalize("i am fine", null)  = "I Am Fine"
         * WordUtils.capitalize("i aM.fine", {'.'}) = "I aM.Fine"
         * </pre>
         *
         * @param str the string to capitalize, may be null
         * @param delimiters set of characters to determine capitalization, null means whitespace
         * @return capitalized string, <code>null</code> if null string input
         * @see #uncapitalize(string)
         * @see #capitalizeFully(string)
         * @since 2.1
         */
        public static string Capitalize(string str, char[] delimiters)
        {
            int delimLen = (delimiters == null ? -1 : delimiters.Length);
            if (str == null || str.Length == 0 || delimLen == 0)
            {
                return str;
            }
            int strLen = str.Length;
            StringBuilder buffer = new StringBuilder(strLen);
            bool capitalizeNext = true;
            for (int i = 0; i < strLen; i++)
            {
                char ch = str[i];

                if (IsDelimiter(ch, delimiters))
                {
                    buffer.Append(ch);
                    capitalizeNext = true;
                }
                else if (capitalizeNext)
                {
                    buffer.Append(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ch.ToString()));
                    capitalizeNext = false;
                }
                else
                {
                    buffer.Append(ch);
                }
            }
            return buffer.ToString();
        }

        //-----------------------------------------------------------------------

        /**
         * <p>Converts all the whitespace separated words in a string into capitalized words, that is each
         * word is made up of a titlecase character and then a series of lowercase characters.  </p>
         * <p/>
         * <p>Whitespace is defined by {@link Character#isWhitespace(char)}. A <code>null</code> input
         * string returns <code>null</code>. Capitalization uses the unicode title case, normally
         * equivalent to upper case.</p>
         * <p/>
         * <pre>
         * WordUtils.capitalizeFully(null)        = null
         * WordUtils.capitalizeFully("")          = ""
         * WordUtils.capitalizeFully("i am FINE") = "I Am Fine"
         * </pre>
         *
         * @param str the string to capitalize, may be null
         * @return capitalized string, <code>null</code> if null string input
         */
        public static string CapitalizeFully(string str)
        {
            return CapitalizeFully(str, null);
        }

        /**
         * <p>Converts all the delimiter separated words in a string into capitalized words, that is each
         * word is made up of a titlecase character and then a series of lowercase characters. </p>
         * <p/>
         * <p>The delimiters represent a set of characters understood to separate words. The first string
         * character and the first non-delimiter character after a delimiter will be capitalized. </p>
         * <p/>
         * <p>A <code>null</code> input string returns <code>null</code>. Capitalization uses the unicode
         * title case, normally equivalent to upper case.</p>
         * <p/>
         * <pre>
         * WordUtils.capitalizeFully(null, *)            = null
         * WordUtils.capitalizeFully("", *)              = ""
         * WordUtils.capitalizeFully(*, null)            = *
         * WordUtils.capitalizeFully(*, new char[0])     = *
         * WordUtils.capitalizeFully("i aM.fine", {'.'}) = "I am.Fine"
         * </pre>
         *
         * @param str the string to capitalize, may be null
         * @param delimiters set of characters to determine capitalization, null means whitespace
         * @return capitalized string, <code>null</code> if null string input
         * @since 2.1
         */
        public static string CapitalizeFully(string str, char[] delimiters)
        {
            int delimLen = (delimiters == null ? -1 : delimiters.Length);
            if (str == null || str.Length == 0 || delimLen == 0)
            {
                return str;
            }
            str = str.ToLowerInvariant();
            return Capitalize(str, delimiters);
        }

        //-----------------------------------------------------------------------

        /**
         * <p>Uncapitalizes all the whitespace separated words in a string. Only the first letter of each
         * word is changed.</p>
         * <p/>
         * <p>Whitespace is defined by {@link Character#isWhitespace(char)}. A <code>null</code> input
         * string returns <code>null</code>.</p>
         * <p/>
         * <pre>
         * WordUtils.uncapitalize(null)        = null
         * WordUtils.uncapitalize("")          = ""
         * WordUtils.uncapitalize("I Am FINE") = "i am fINE"
         * </pre>
         *
         * @param str the string to uncapitalize, may be null
         * @return uncapitalized string, <code>null</code> if null string input
         * @see #capitalize(string)
         */
        public static string Uncapitalize(string str)
        {
            return Uncapitalize(str, null);
        }

        /**
         * <p>Uncapitalizes all the whitespace separated words in a string. Only the first letter of each
         * word is changed.</p>
         * <p/>
         * <p>The delimiters represent a set of characters understood to separate words. The first string
         * character and the first non-delimiter character after a delimiter will be uncapitalized. </p>
         * <p/>
         * <p>Whitespace is defined by {@link Character#isWhitespace(char)}. A <code>null</code> input
         * string returns <code>null</code>.</p>
         * <p/>
         * <pre>
         * WordUtils.uncapitalize(null, *)            = null
         * WordUtils.uncapitalize("", *)              = ""
         * WordUtils.uncapitalize(*, null)            = *
         * WordUtils.uncapitalize(*, new char[0])     = *
         * WordUtils.uncapitalize("I AM.FINE", {'.'}) = "i AM.fINE"
         * </pre>
         *
         * @param str the string to uncapitalize, may be null
         * @param delimiters set of characters to determine uncapitalization, null means whitespace
         * @return uncapitalized string, <code>null</code> if null string input
         * @see #capitalize(string)
         * @since 2.1
         */
        public static string Uncapitalize(string str, char[] delimiters)
        {
            int delimLen = (delimiters == null ? -1 : delimiters.Length);
            if (str == null || str.Length == 0 || delimLen == 0)
            {
                return str;
            }
            int strLen = str.Length;
            StringBuilder buffer = new StringBuilder(strLen);
            bool uncapitalizeNext = true;
            for (int i = 0; i < strLen; i++)
            {
                char ch = str[i];

                if (IsDelimiter(ch, delimiters))
                {
                    buffer.Append(ch);
                    uncapitalizeNext = true;
                }
                else if (uncapitalizeNext)
                {
                    buffer.Append(ch.ToString().ToLowerInvariant());
                    uncapitalizeNext = false;
                }
                else
                {
                    buffer.Append(ch);
                }
            }
            return buffer.ToString();
        }

        //-----------------------------------------------------------------------

        /**
         * <p>Extracts the initial letters from each word in the string.</p>
         * <p/>
         * <p>The first letter of the string and all first letters after whitespace are returned as a new
         * string. Their case is not changed.</p>
         * <p/>
         * <p>Whitespace is defined by {@link Character#isWhitespace(char)}. A <code>null</code> input
         * string returns <code>null</code>.</p>
         * <p/>
         * <pre>
         * WordUtils.initials(null)             = null
         * WordUtils.initials("")               = ""
         * WordUtils.initials("Ben John Lee")   = "BJL"
         * WordUtils.initials("Ben J.Lee")      = "BJ"
         * </pre>
         *
         * @param str the string to get initials from, may be null
         * @return string of initial letters, <code>null</code> if null string input
         * @see #initials(string, char[])
         * @since 2.2
         */
        public static string Initials(string str)
        {
            return Initials(str, null);
        }

        /**
         * <p>Extracts the initial letters from each word in the string.</p>
         * <p/>
         * <p>The first letter of the string and all first letters after the defined delimiters are
         * returned as a new string. Their case is not changed.</p>
         * <p/>
         * <p>If the delimiters array is null, then Whitespace is used. Whitespace is defined by {@link
         * Character#isWhitespace(char)}. A <code>null</code> input string returns <code>null</code>. An
         * empty delimiter array returns an empty string.</p>
         * <p/>
         * <pre>
         * WordUtils.initials(null, *)                = null
         * WordUtils.initials("", *)                  = ""
         * WordUtils.initials("Ben John Lee", null)   = "BJL"
         * WordUtils.initials("Ben J.Lee", null)      = "BJ"
         * WordUtils.initials("Ben J.Lee", [' ','.']) = "BJL"
         * WordUtils.initials(*, new char[0])         = ""
         * </pre>
         *
         * @param str the string to get initials from, may be null
         * @param delimiters set of characters to determine words, null means whitespace
         * @return string of initial letters, <code>null</code> if null string input
         * @see #initials(string)
         * @since 2.2
         */
        public static string Initials(string str, char[] delimiters)
        {
            if (str == null || str.Length == 0)
            {
                return str;
            }
            if (delimiters != null && delimiters.Length == 0)
            {
                return "";
            }
            int strLen = str.Length;
            char[] buf = new char[strLen / 2 + 1];
            int count = 0;
            bool lastWasGap = true;
            for (int i = 0; i < strLen; i++)
            {
                char ch = str[i];

                if (IsDelimiter(ch, delimiters))
                {
                    lastWasGap = true;
                }
                else if (lastWasGap)
                {
                    buf[count++] = ch;
                    lastWasGap = false;
                }
                else
                {
                    // ignore ch
                }
            }
            return new string(buf, 0, count);
        }

        //-----------------------------------------------------------------------

        /**
         * Is the character a delimiter.
         *
         * @param ch the character to check
         * @param delimiters the delimiters
         * @return true if it is a delimiter
         */
        private static bool IsDelimiter(char ch, char[] delimiters)
        {
            if (delimiters == null)
            {
                return char.IsWhiteSpace(ch);
            }
            foreach (char delimiter in delimiters)
            {
                if (ch == delimiter)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
