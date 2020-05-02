using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Tokenization
{
    public class PerceptronSegmenter
    {
        private static readonly ISet<string> webWords =
      new HashSet<string> { "http:", ".html", "www", ".tr", ".edu", ".com", ".net", ".gov", ".org", "@" };
        internal static ISet<string> TurkishAbbreviationSet = new HashSet<string>();
        private static CultureInfo localeTr = CultureInfo.GetCultureInfo("tr");
        private static string lowerCaseVowels = "aeıioöuüâîû";
        private static string upperCaseVowels = "AEIİOÖUÜÂÎÛ";

        static PerceptronSegmenter()
        {
            try
            {
                List<string> lines = TextIO.LoadLines("Resources/tokenization/abbreviations.txt");
                foreach (string line in lines)
                {
                    if (line.Trim().Length > 0)
                    {
                        string abbr = Regex.Replace(line.Trim(), "\\s+", ""); // erase spaces
                        TurkishAbbreviationSet.Add(Regex.Replace(abbr, "\\.$", "")); // erase last dot and add.
                        TurkishAbbreviationSet
                            .Add(Regex.Replace(abbr.ToLower(localeTr), "\\.$", "")); // lowercase and add.
                    }
                }
            }
            catch (IOException e)
            {
                Console.Error.Write(e);
            }
        }

        protected FloatValueMap<string> weights = new FloatValueMap<string>();

        protected static FloatValueMap<string> Load(BinaryReader dis)
        {
            int size = dis.ReadInt32().EnsureEndianness();
            FloatValueMap<string> features = new FloatValueMap<string>((int)(size * 1.5));
            for (int i = 0; i < size; i++)
            {
                features.Set(dis.ReadUTF(), dis.ReadSingle().EnsureEndianness());
            }
            return features;
        }

        public static bool PotentialWebSite(string s)
        {
            foreach (string urlWord in webWords)
            {
                if (s.Contains(urlWord))
                {
                    return true;
                }
            }
            return false;
        }

        private static char GetMetaChar(char letter)
        {
            char c;
            if (char.IsUpper(letter))
            {
                c = upperCaseVowels.IndexOf(letter) > 0 ? 'V' : 'C';
            }
            else if (char.IsLower(letter))
            {
                c = lowerCaseVowels.IndexOf(letter) > 0 ? 'v' : 'c';
            }
            else if (char.IsDigit(letter))
            {
                c = 'd';
            }
            else if (char.IsWhiteSpace(letter))
            {
                c = ' ';
            }
            else if (letter == '.' || letter == '!' || letter == '?')
            {
                return letter;
            }
            else
            {
                c = '-';
            }
            return c;
        }

        internal static string GetMetaChars(string str)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                sb.Append(GetMetaChar(str[i]));
            }
            return sb.ToString();
        }

        public void SaveBinary(string path)
        {
            using (BinaryWriter dos = IOUtil.GetDataOutputStream(path))
            {
                dos.Write(weights.Size().EnsureEndianness());
                foreach (string feature in weights)
                {
                    dos.WriteUTF(feature);
                    dos.Write(weights.Get(feature).EnsureEndianness());
                }
            }
        }
    }
}
