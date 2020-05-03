using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Morphology.Analysis.TR
{
    public class TurkishNumbers
    {
        public static readonly long MAX_NUMBER = 999999999999999999L;
        public static readonly long MIN_NUMBER = -999999999999999999L;
        private static Dictionary<String, long> stringToNumber = new Dictionary<string, long>();
        private static Dictionary<long, string> NUMBER_TABLE = new Dictionary<long, string>();
        private static Dictionary<string, string> ordinalMap = new Dictionary<string, string>();

        // fill the NUMBER_TABLE and stringToNumber map.
        static TurkishNumbers()
        {
            Add(0, "sıfır");
            Add(1, "bir");
            Add(2, "iki");
            Add(3, "üç");
            Add(4, "dört");
            Add(5, "beş");
            Add(6, "altı");
            Add(7, "yedi");
            Add(8, "sekiz");
            Add(9, "dokuz");
            Add(10, "on");
            Add(20, "yirmi");
            Add(30, "otuz");
            Add(40, "kırk");
            Add(50, "elli");
            Add(60, "altmış");
            Add(70, "yetmiş");
            Add(80, "seksen");
            Add(90, "doksan");
            Add(100, "yüz");
            Add(1000, "bin");
            Add(1000000, "milyon");
            Add(1000000000L, "milyar");
            Add(1000000000000L, "trilyon");
            Add(1000000000000000L, "katrilyon");

            foreach (long s in NUMBER_TABLE.Keys)
            {
                stringToNumber.Add(NUMBER_TABLE.GetValueOrDefault(s), s);
                // TODO: we should not assume "atmış" -> "altmış"
                stringToNumber["atmış"] = 60L;
            }

            // read ordinal readings.
            try
            {
                KeyValueReader reader = new KeyValueReader(":", "#");
                ordinalMap = reader
                    .LoadFromStream(
                        IOs.GetResourceAsStream("Resources/tr/turkish-ordinal-numbers.txt"), "utf-8");
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private static string[] singleDigitNumbers = {"", "bir", "iki", "üç", "dört", "beş", "altı",
      "yedi", "sekiz", "dokuz"};
        private static string[] tenToNinety = {"", "on", "yirmi", "otuz", "kırk", "elli", "altmış",
      "yetmiş", "seksen", "doksan"};
        private static string[] thousands = { "", "bin", "milyon", "milyar", "trilyon", "katrilyon" };
        private static Regex NUMBER_SEPARATION = new Regex("[0-9]+|[^0-9 ]+");
        private static Regex NOT_NUMBER = new Regex("[^0-9]");
        private static Regex NUMBER = new Regex("[0-9]");

        private static void Add(long number, string text)
        {
            NUMBER_TABLE.Add(number, text);
        }

        /// <summary>
        /// converts a given three digit number.
        /// </summary>
        /// <param name="threeDigitNumber">a three digit number.</param>
        /// <returns>turkish string representation of the input number.</returns>
        private static String ConvertThreeDigit(int threeDigitNumber)
        {
            String sonuc = "";
            int hundreds = threeDigitNumber / 100;
            int tens = threeDigitNumber / 10 % 10;
            int singleDigit = threeDigitNumber % 10;

            if (hundreds != 0)
            {
                sonuc = "yüz";
            }
            if (hundreds > 1)
            {
                sonuc = singleDigitNumbers[hundreds] + " " + sonuc;
            }
            sonuc = sonuc + " " + tenToNinety[tens] + " " + singleDigitNumbers[singleDigit];
            return sonuc.Trim();
        }

        public static Dictionary<string, string> GetOrdinalMap()
        {
            return ordinalMap;
        }

        /// <summary>
        /// returns the Turkish representation of the input. if negative "eksi" string is prepended.
        /// </summary>
        /// <param name="input">input. must be between (including both) -999999999999999999L to
        /// 999999999999999999L</param>
        /// <returns>Turkish representation of the input. if negative "eksi" string is prepended.</returns>
        public static string ConvertToString(long input)
        {
            if (input == 0)
            {
                return "sıfır";
            }
            if (input < MIN_NUMBER || input > MAX_NUMBER)
            {
                throw new ArgumentException("number is out of bounds:" + input);
            }
            String result = "";
            long girisPos = Math.Abs(input);
            int sayac = 0;
            while (girisPos > 0)
            {
                int uclu = (int)(girisPos % 1000);
                if (uclu != 0)
                {
                    if (uclu == 1 && sayac == 1)
                    {
                        result = thousands[sayac] + " " + result;
                    }
                    else
                    {
                        result = ConvertThreeDigit(uclu) + " " + thousands[sayac] + " " + result;
                    }
                }
                sayac++;
                girisPos /= 1000;
            }
            if (input < 0)
            {
                return "eksi " + result.Trim();
            }
            else
            {
                return result.Trim();
            }

        }

        /// <summary>
        /// Methods converts a String containing an integer to a Strings.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertNumberToString(string input)
        {
            if (input.StartsWith("+"))
            {
                input = input.Substring(1);
            }
            List<String> sb = new List<string>();
            int i;
            for (i = 0; i < input.Length; i++)
            {
                if (input[i] == '0')
                {
                    sb.Add("sıfır");
                }
                else
                {
                    break;
                }
            }
            string rest = input.Substring(i);
            if (rest.Length > 0)
            {
                sb.Add(ConvertToString(long.Parse(rest)));
            }

            return string.Join(" ", sb);
        }

        /// <summary>
        /// Returns the value of a single word number value. those values are limited. Word should not
        /// contain any spaces and must be in lowercase.
        /// </summary>
        /// <param name="word">the Turkish representation of a single key number string.</param>
        /// <returns></returns>
        public static long SingleWordNumberValue(string word)
        {
            if (!stringToNumber.ContainsKey(word))
            {
                throw new ArgumentException(
                    "this is not a valid number string (check case and spaces.): " + word);
            }
            return stringToNumber.GetValueOrDefault(word);
        }

        /// <summary>
        /// replaces all number strings with actual numbers. Such as:
        /// <pre>
        /// ["hello bir on iki nokta otuz beş hello"] -> ["hello 1 10 2 nokta 30 5 hello"]
        /// </pre>
        /// </summary>
        /// <param name="inputSequence">a sequence of words.</param>
        /// <returns>same as input but string representations of numbers are replaced with numbers.</returns>
        public static List<string> ReplaceNumberStrings(List<string> inputSequence)
        {
            List<string> output = new List<string>(inputSequence.Count);
            foreach (string s in inputSequence)
            {
                if (stringToNumber.ContainsKey(s))
                {
                    output.Add(stringToNumber.GetValueOrDefault(s).ToString());
                }
                else
                {
                    output.Add(s);
                }
            }
            return output;
        }

        /// <summary>
        /// seperates connected number texts. such as
        /// <pre>
        /// ["oniki","otuzbeş","ikiiii"] -> ["on","iki","otuz","beş","ikiiii"]
        /// </pre>
        /// </summary>
        /// <param name="inputSequence">a sequence of words.</param>
        /// <returns>same list with strings where connected number strings are separated.</returns>
        public static List<string> SeperateConnectedNumbers(List<string> inputSequence)
        {
            List<string> output = new List<string>(inputSequence.Count);
            foreach (string s in inputSequence)
            {
                if (stringToNumber.ContainsKey(s))
                {
                    output.Add(stringToNumber.GetValueOrDefault(s).ToString());
                    continue;
                }
                output.AddRange(SeperateConnectedNumbers(s));
            }
            return output;
        }

        /// <summary>
        /// seperates connected number texts. such as
        /// <pre>
        /// ["oniki","otuzbes","ikiiii"] -> ["on","iki","otuz","bes","ikiiii"]
        /// </pre>
        /// </summary>
        /// <param name="input">a single key.</param>
        /// <returns>same list with strings where connected number strings are separated.</returns>
        public static List<string> SeperateConnectedNumbers(string input)
        {
            StringBuilder str = new StringBuilder();
            List<string> words = new List<string>(2);
            bool numberFound = false;
            for (int i = 0; i < input.ToCharArray().Length; i++)
            {
                str.Append(input.ToCharArray()[i]);
                if (stringToNumber.ContainsKey(str.ToString()))
                {
                    words.Add(str.ToString());
                    str.Remove(0, str.Length);
                    numberFound = true;
                }
                else
                {
                    numberFound = false;
                }
            }
            if (!numberFound)
            {
                words.Clear();
                words.Add(input);
            }
            return words;
        }

        private static TurkishTextToNumberConverter textToNumber = new TurkishTextToNumberConverter();

        /// <summary>
        /// Converts an array of number strings to number, if possible. Returns -1 if conversion is not possible.
        /// <pre>
        ///   "bir" -> 1
        ///   "on", "bir" -> 11
        ///   "bir", "bin" -> -1
        ///   "bir", "armut" -> -1
        /// </pre>
        /// </summary>
        /// <param name="words">Array of number strings.</param>
        /// <returns>number representation, or -1 if not possible to convert.</returns>
        public static long ConvertToNumber(params string[] words)
        {
            return textToNumber.Convert(words);
        }

        /// <summary>
        /// Converts a text to number, if possible. Returns -1 if conversion is not possible.
        /// <pre>
        ///   "bir" -> 1
        ///   "on bir" -> 11
        ///   "bir bin" -> -1
        ///   "bir armut" -> -1
        /// </pre>
        /// </summary>
        /// <param name="text">text a string.</param>
        /// <returns>number representation of input string, or -1 if not possible to convert.</returns>
        public static long ConvertToNumber(string text)
        {
            return textToNumber.Convert(Regex.Split(text, "[ ]+"));
        }

        public static string ConvertOrdinalNumberString(string input)
        {
            string numberPart = input;
            if (input.EndsWith("."))
            {
                numberPart = Strings.SubstringUntilFirst(input, ".");
            }

            long number = long.Parse(numberPart);
            string text = ConvertToString(number);
            string[] words = Regex.Split(text.Trim(), "[ ]+");
            string lastNumber = words[words.Length - 1];

            if (ordinalMap.ContainsKey(lastNumber))
            {
                lastNumber = ordinalMap.GetValueOrDefault(lastNumber);
            }
            else
            {
                throw new ApplicationException("Cannot find ordinal reading for:" + lastNumber);
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < words.Length - 1; i++)
            {
                sb.Append(words[i]).Append(" ");
            }
            sb.Append(lastNumber);
            return sb.ToString();
        }

        /// <summary>
        /// Separate digits and non digits as Strings. Such as:
        /// <pre>
        ///  A12 -> "A" "12"
        ///  1A12'ye -> "1" "A" "12" "'ye"
        ///  </pre>
        /// </summary>
        /// <param name="s">input.</param>
        /// <returns>separated list of numerical and non numerical tokens.</returns>
        public static List<String> SeparateNumbers(string s)
        {
            return Regexps.AllMatches(NUMBER_SEPARATION, s);
        }

        public static string GetOrdinal(string input)
        {
            return ordinalMap.GetValueOrDefault(input);
        }

        public static bool hasNumber(String s)
        {
            return NUMBER.IsMatch(s);
        }

        public static bool HasOnlyNumber(String s)
        {
            return !NOT_NUMBER.IsMatch(s);
        }


        static readonly Regex romanNumeralPattern =
      new Regex("^(M{0,3})(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$",
         RegexOptions.IgnoreCase);

        /// <summary>
        /// Convert a roman numeral to decimal numbers. Copied from public domain source
        /// (https://stackoverflow.com/a/19392801).
        /// </summary>
        /// <param name="s">roman numeral</param>
        /// <returns>decimal equivalent. if it cannot be converted, -1.</returns>
        public static int RomanToDecimal(String s)
        {
            if (s == null ||
                s.IsEmpty() ||
                !romanNumeralPattern.IsMatch(s))
            {
                return -1;
            }

            MatchCollection matches = new Regex("M|CM|D|CD|C|XC|L|XL|X|IX|V|IV|I").Matches(s);
            int[] decimalValues = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            String[] romanNumerals = {
                "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V",
        "IV", "I"};
            int result = 0;

            foreach (Match match in matches)
            {
                for (int i = 0; i < romanNumerals.Length; i++)
                {
                    if (romanNumerals[i].Equals(match.Groups[0].Value))
                    {
                        result += decimalValues[i];
                    }
                }
            }

            return result;
        }
    }
}
