using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Core.Turkish.Hyphenation;

namespace ZemberekDotNet.Morphology.Analysis.TR
{
    public class PronunciationGuesser
    {
        public static readonly TurkishAlphabet alphabet = TurkishAlphabet.Instance;
        private static Dictionary<string, string> turkishLetterProns;
        private static Dictionary<string, string> englishLetterProns;
        private static Dictionary<string, string> englishPhonesToTurkish;

        static PronunciationGuesser()
        {
            turkishLetterProns = LoadMap("Resources/tr/phonetics/turkish-letter-names.txt");
            englishLetterProns = LoadMap("Resources/tr/phonetics/english-letter-names.txt");
            englishPhonesToTurkish = LoadMap("Resources/tr/phonetics/english-phones-to-turkish.txt");
        }

        private static Dictionary<string, string> LoadMap(string resource)
        {
            try
            {
                return new KeyValueReader("=", "##").LoadFromFile(resource, "utf-8");
            }
            catch (IOException e)
            {
                throw new ApplicationException(e.Message);
            }
        }

        public string ToTurkishLetterPronunciations(string w)
        {
            if (alphabet.ContainsDigit(w))
            {
                return ToTurkishLetterPronunciationWithDigit(w);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < w.Length; i++)
            {
                char c = w[i];
                if (c == '-')
                {
                    continue;
                }
                string key = c.ToString();
                if (turkishLetterProns.ContainsKey(key))
                {
                    // most abbreviations ends with k uses `ka` sounds.
                    if (i == w.Length - 1 && key.Equals("k"))
                    {
                        sb.Append("ka");
                    }
                    else
                    {
                        sb.Append(turkishLetterProns.GetValueOrDefault(key));
                    }
                }
                else
                {
                    Log.Debug("Cannot guess pronunciation of letter [" + key + "] in :[" + w + "]");
                }
            }
            return sb.ToString();
        }

        private string ToTurkishLetterPronunciationWithDigit(String input)
        {
            List<string> pieces = TurkishNumbers.SeparateNumbers(input);
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (string piece in pieces)
            {
                if (alphabet.ContainsDigit(piece))
                {
                    sb.Append(TurkishNumbers.ConvertNumberToString(piece));
                    i++;
                    continue;
                }
                if (i < pieces.Count - 1)
                {
                    sb.Append(ToTurkishLetterPronunciations(piece));
                }
                else
                {
                    sb.Append(ReplaceEnglishSpecificChars(piece));
                }
                i++;
            }
            return Regex.Replace(sb.ToString(), "[ ]+", "");
        }

        public string ReplaceEnglishSpecificChars(string w)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < w.Length; i++)
            {
                char c = w[i];
                switch (c)
                {
                    case 'w':
                        sb.Append("v");
                        break;
                    case 'q':
                        sb.Append("k");
                        break;
                    case 'x':
                        sb.Append("ks");
                        break;
                    case '-':
                    case '\'':
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private static TurkishSyllableExtractor extractorForAbbrv = TurkishSyllableExtractor.Strict;

        /// <summary>
        /// Tries to guess turkish abbreviation pronunciation.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string GuessForAbbreviation(string input)
        {
            List<string> syllables = extractorForAbbrv.GetSyllables(input);

            bool firstTwoCons = false;
            if (input.Length > 2)
            {
                if (!alphabet.ContainsVowel(input.Substring(0, 2)))
                {
                    firstTwoCons = true;
                }
            }

            if (syllables.Count == 0 || input.Length < 3 || firstTwoCons)
            {
                return ToTurkishLetterPronunciations(input);
            }
            else
            {
                return ReplaceEnglishSpecificChars(input);
            }

        }
    }
}
