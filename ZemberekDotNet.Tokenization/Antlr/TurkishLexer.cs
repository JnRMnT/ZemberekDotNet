using Antlr4.Runtime;
using Antlr4.Runtime.Dfa;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ZemberekDotNet.Tokenization.Antlr
{
    /// <summary>
    /// A simple lexer grammar for Turkish texts.
    /// </summary>
    public class TurkishLexer : BaseTurkishLexer
    {
        public TurkishLexer(ICharStream input)
        : base(input) { }

        public TurkishLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
        {

        }
        static TurkishLexer()
        {
            try
            {
                foreach (string line in File.ReadLines("Resources/tokenization/abbreviations.txt", Encoding.UTF8))
                {
                    if (line.Trim().Length > 0)
                    {
                        string abbr = Regex.Replace(line.Trim(), "\\s+", ""); // erase spaces
                        if (abbr.EndsWith("."))
                        {
                            abbreviations.Add(abbr);
                            abbreviations.Add(abbr.ToLowerInvariant());
                            abbreviations.Add(abbr.ToLower(localeTr));
                        }
                    }
                }

                decisionToDFA = new DFA[_ATN.NumberOfDecisions];
                for (int i = 0; i < _ATN.NumberOfDecisions; i++)
                {
                    decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

    }
}
