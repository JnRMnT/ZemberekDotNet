using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ZemberekDotNet.Normalization.Deasciifier
{
    /// <summary>
    /// This class provides functionality to deasciify a given ASCII based Turkish text. <p> <p> Note:
    /// Adapted from Emre Sevinc's Turkish deasciifier for Python which was influenced from Deniz Yuret's
    /// Emacs Turkish Mode implementation which is was inspired by Gokhan Tur's Turkish Text Deasciifier.
    /// </p> <p> <p> See: <a href = "http://denizyuret.blogspot.com/2006/11/emacs-turkish-mode.html" > Deniz
    /// Yuret's Emacs Turkish Mode</a><br /> <a href="http://ileriseviye.org/blog/?p=3274">Turkish
    /// Deasciifier on Emre Sevinc's Blog</a><br /> <a href="http://github.com/emres/turkish-deasciifier/">Turkish
    /// Deasciifier for Python on Emre Sevinc's Github Repo</a><br /> </p> <p> <p> <h3>Usage</h3> <p>
    /// <pre>
    /// Deasciifier d = new Deasciifier();
    /// d.setAsciiString(&quot; Hadi bir masal uyduralim, icinde mutlu, doygun, telassiz
    ///* durdugumuz.&quot;);
    /// System.out.println(d.convertToTurkish());
    /// </pre>
    /// <p> </p>
    ///
    /// @author Ahmet Alp Balkan<ahmet at ahmetalpbalkan.com>
    /// </summary>
    public class Deasciifier
    {
        static int turkishContextSize = 10;

        static Dictionary<string, Dictionary<string, int>> turkishPatternTable = null;

        static Dictionary<string, string> turkishAsciifyTable = new Dictionary<string, string>();
        static string[] uppercaseLetters = {"A", "B", "C", "D", "E", "F", "G",
      "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
      "U", "V", "W", "X", "Y", "Z"};
        static Dictionary<string, string> turkishDowncaseAsciifyTable = new Dictionary<string, string>();
        static Dictionary<string, string> turkishUpcaseAccentsTable = new Dictionary<string, string>();
        static Dictionary<string, string> turkishToggleAccentTable = new Dictionary<string, string>();

        static Deasciifier()
        {
            turkishAsciifyTable.Add("ç", "c");
            turkishAsciifyTable.Add("Ç", "C");
            turkishAsciifyTable.Add("ğ", "g");
            turkishAsciifyTable.Add("Ğ", "G");
            turkishAsciifyTable.Add("ö", "o");
            turkishAsciifyTable.Add("Ö", "O");
            turkishAsciifyTable.Add("ı", "i");
            turkishAsciifyTable.Add("İ", "I");
            turkishAsciifyTable.Add("ş", "s");
            turkishAsciifyTable.Add("Ş", "S");

            foreach (string c in uppercaseLetters)
            {
                turkishDowncaseAsciifyTable.Add(c, c.ToLowerInvariant());
                turkishDowncaseAsciifyTable.Add(c.ToLowerInvariant(), c.ToLowerInvariant());
            }

            turkishDowncaseAsciifyTable.Add("ç", "c");
            turkishDowncaseAsciifyTable.Add("Ç", "c");
            turkishDowncaseAsciifyTable.Add("ğ", "g");
            turkishDowncaseAsciifyTable.Add("Ğ", "g");
            turkishDowncaseAsciifyTable.Add("ö", "o");
            turkishDowncaseAsciifyTable.Add("Ö", "o");
            turkishDowncaseAsciifyTable.Add("ı", "i");
            turkishDowncaseAsciifyTable.Add("İ", "i");
            turkishDowncaseAsciifyTable.Add("ş", "s");
            turkishDowncaseAsciifyTable.Add("Ş", "s");
            turkishDowncaseAsciifyTable.Add("ü", "u");
            turkishDowncaseAsciifyTable.Add("Ü", "u");


            foreach (string c in uppercaseLetters)
            {
                turkishUpcaseAccentsTable.Add(c, c.ToLowerInvariant());
                turkishUpcaseAccentsTable.Add(c.ToLowerInvariant(), c.ToLowerInvariant());
            }


            turkishUpcaseAccentsTable.Add("ç", "C");
            turkishUpcaseAccentsTable.Add("Ç", "C");
            turkishUpcaseAccentsTable.Add("ğ", "G");
            turkishUpcaseAccentsTable.Add("Ğ", "G");
            turkishUpcaseAccentsTable.Add("ö", "O");
            turkishUpcaseAccentsTable.Add("Ö", "O");
            turkishUpcaseAccentsTable.Add("ı", "I");
            turkishUpcaseAccentsTable.Add("İ", "i");
            turkishUpcaseAccentsTable.Add("ş", "S");
            turkishUpcaseAccentsTable.Add("Ş", "S");
            turkishUpcaseAccentsTable.Add("ü", "U");
            turkishUpcaseAccentsTable.Add("Ü", "U");

            turkishToggleAccentTable.Add("c", "ç"); // initial direction
            turkishToggleAccentTable.Add("C", "Ç");
            turkishToggleAccentTable.Add("g", "ğ");
            turkishToggleAccentTable.Add("G", "Ğ");
            turkishToggleAccentTable.Add("o", "ö");
            turkishToggleAccentTable.Add("O", "Ö");
            turkishToggleAccentTable.Add("u", "ü");
            turkishToggleAccentTable.Add("U", "Ü");
            turkishToggleAccentTable.Add("i", "ı");
            turkishToggleAccentTable.Add("I", "İ");
            turkishToggleAccentTable.Add("s", "ş");
            turkishToggleAccentTable.Add("S", "Ş");
            turkishToggleAccentTable.Add("ç", "c"); // other direction
            turkishToggleAccentTable.Add("Ç", "C");
            turkishToggleAccentTable.Add("ğ", "g");
            turkishToggleAccentTable.Add("Ğ", "G");
            turkishToggleAccentTable.Add("ö", "o");
            turkishToggleAccentTable.Add("Ö", "O");
            turkishToggleAccentTable.Add("ü", "u");
            turkishToggleAccentTable.Add("Ü", "U");
            turkishToggleAccentTable.Add("ı", "i");
            turkishToggleAccentTable.Add("İ", "I");
            turkishToggleAccentTable.Add("ş", "s");
            turkishToggleAccentTable.Add("Ş", "S");
        }

        private string asciiString;
        private string turkishString;

        public Deasciifier()
        {
            LoadPatternTable();
        }

        public Deasciifier(string asciiString) : this()
        {
            this.asciiString = asciiString;
            this.turkishString = asciiString;
        }

        public static string SetCharAt(string mystr, int pos, string c)
        {
            return string.Concat(mystr.Substring(0, pos).Concat(c).Concat(
                mystr.Substring(pos + 1, mystr.Length - pos - 1)));
        }

        public static string RepeatString(string haystack, int times)
        {
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < times; i++)
            {
                tmp.Append(haystack);
            }

            return tmp.ToString();
        }

        public static string CharAt(string source, int index)
        {
            return char.ToString(source[index]);
        }

        public static string ReadFromFile(string filePath)
        {
            StringBuilder s = new StringBuilder();

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line != null)
                        {
                            s.Append(line); // + "\n" ?
                        }
                    }
                }
            }

            return s.ToString();
        }

        public static void SavePatternTable(string filename)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fileStream, turkishPatternTable);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        public void SetAsciiString(string asciiString)
        {
            this.asciiString = asciiString;
            this.turkishString = asciiString;
        }

        public void PrintTurkishString()
        {
            Console.Write(turkishString); // with a trailing new line
        }

        public void PrintTurkishString(StreamWriter writer)
        {
            writer.WriteLine(turkishString); // without a trailing new line
        }

        public string TurkishToggleAccent(string c)
        {
            string result = turkishToggleAccentTable.GetValueOrDefault(c);
            return (result == null) ? c : result;
        }

        public bool TurkishMatchPattern(Dictionary<string, int> dlist, int point)
        {
            int rank = dlist.Count * 2;
            string str = TurkishGetContext(turkishContextSize, point);

            int start = 0;
            int end = 0;
            int _len = str.Length;

            while (start <= turkishContextSize)
            {
                end = turkishContextSize + 1;
                while (end <= _len)
                {
                    string s = str.Substring(start, end - start);

                    int? r = dlist.ContainsKey(s) ? (int?)dlist.GetValueOrDefault(s) : null;

                    if (r != null && Math.Abs((int)r) < Math.Abs(rank))
                    {
                        rank = (int)r;
                    }
                    end++;
                }
                start++;
            }
            return rank > 0;
        }

        public bool TurkishMatchPattern(Dictionary<string, int> dlist)
        {
            return TurkishMatchPattern(dlist, 0);
        }

        public string TurkishGetContext(int size, int point)
        {
            string s = RepeatString(" ", (1 + (2 * size)));
            s = SetCharAt(s, size, "X");

            int i = size + 1;
            bool space = false;
            int index = point;
            index++;

            string currentChar;

            while (i < s.Length && !space && index < asciiString.Length)
            {
                currentChar = CharAt(turkishString, index);

                string x = turkishDowncaseAsciifyTable.GetValueOrDefault(currentChar);

                if (x == null)
                {
                    if (!space)
                    {
                        i++;
                        space = true;
                    }
                }
                else
                {
                    s = SetCharAt(s, i, x);
                    i++;
                    space = false;
                }
                index++;
            }

            s = s.Substring(0, i);

            index = point;
            i = size - 1;
            space = false;

            index--;

            while (i >= 0 && index >= 0)
            {
                currentChar = CharAt(turkishString, index);
                string x = turkishUpcaseAccentsTable.GetValueOrDefault(currentChar);

                if (x == null)
                {
                    if (!space)
                    {
                        i--;
                        space = true;
                    }
                }
                else
                {
                    s
                        = SetCharAt(s, i, x);
                    i--;
                    space = false;
                }
                index--;
            }

            return s;
        }

        public bool TurkishNeedCorrection(string c, int point)
        {

            string tr = turkishAsciifyTable.GetValueOrDefault(c);
            if (tr == null)
            {
                tr = c;
            }

            Dictionary<string, int> pl = turkishPatternTable.GetValueOrDefault(tr.ToLowerInvariant());

            bool m = false;
            if (pl != null)
            {
                m = TurkishMatchPattern(pl, point);
            }

            if (tr.Equals("I"))
            {
                if (c.Equals(tr))
                {
                    return !m;
                }
                else
                {
                    return m;
                }
            }
            else
            {
                if (c.Equals(tr))
                {
                    return m;
                }
                else
                {
                    return !m;
                }
            }
        }

        public bool TurkishNeedCorrection(string c)
        {
            return TurkishNeedCorrection(c, 0);
        }

        /// <summary>
        /// Convert a string with ASCII-only letters into one with Turkish letters.
        /// </summary>
        /// <returns>Deasciified text.</returns>
        public string ConvertToTurkish()
        {
            for (int i = 0; i < turkishString.Length; i++)
            {
                string c = CharAt(turkishString, i);

                if (TurkishNeedCorrection(c, i))
                {
                    turkishString = SetCharAt(turkishString, i,
                        TurkishToggleAccent(c));
                }
                else
                {
                    turkishString = SetCharAt(turkishString, i, c);
                }
            }
            return turkishString;
        }

        public string TurkishGetContext()
        {
            return TurkishGetContext(turkishContextSize, 0);
        }

        private void LoadPatternTable()
        {
            if (turkishPatternTable != null)
            {
                return;
            }
            turkishPatternTable = new Dictionary<string, Dictionary<string, int>>();
            using (FileStream fileStream = File.OpenRead("Resources/patterns/turkishPatternTable"))
            {
                try
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    turkishPatternTable = (Dictionary<string, Dictionary<string, int>>)binaryFormatter.Deserialize(fileStream);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
        }

    }
}
