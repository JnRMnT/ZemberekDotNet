using System;

namespace ZemberekDotNet.NER
{
    /// <summary>
    ///  BILOU style NER position information.
    /// </summary>
    public class NePosition
    {
        public static readonly NePosition BEGIN = new NePosition("B");  // beginning token of a NE
        public static readonly NePosition INSIDE = new NePosition("I"); // Inside token of a NE
        public static readonly NePosition LAST = new NePosition("L");   // Last token of a NE
        public static readonly NePosition OUTSIDE = new NePosition("O");// Not a NE token
        public static readonly NePosition UNIT = new NePosition("U");   // A single NE token

        internal string shortForm;

        public NePosition(string s)
        {
            this.shortForm = s;
        }

        public static NePosition FromString(string s)
        {
            switch (s)
            {
                case "B":
                    return BEGIN;
                case "I":
                    return INSIDE;
                case "L":
                    return LAST;
                case "O":
                    return OUTSIDE;
                case "U":
                    return UNIT;
                default:
                    throw new ArgumentException("Unidentified ner position " + s);
            }
        }
    }
}
