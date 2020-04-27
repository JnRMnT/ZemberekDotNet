using System;

namespace ZemberekDotNet.Core.Text.Distance
{
    public class CharDistance : IStringDistance
    {
        public int SourceSize(TokenSequence sourceSequence)
        {
            return sourceSequence.AsString().Length;
        }

        public double Distance(String sourceString, String targetString)
        {
            int n;
            double[] p; //'previous' cost array, horizontally
            double[] d; // cost array, horizontally
            double[] _d; //placeholder to assist in swapping p and d

            n = sourceString.Length;
            p = new double[n + 1];
            d = new double[n + 1];

            int m = targetString.Length;
            if (n == 0 || m == 0)
            {
                return System.Math.Abs(n - m);
            }

            // indexes into strings s and t
            int i; // iterates through s
            int j; // iterates through t

            char t_j; // jth char

            double cost; // cost

            for (i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (j = 1; j <= m; j++)
            {
                t_j = targetString[j - 1];
                d[0] = j;

                for (i = 1; i <= n; i++)
                {
                    cost = sourceString[i - 1] == t_j ? 0 : 1;
                    // minimum of cell to the left+1, to the top+1, diagonally left and up +cost
                    d[i] = System.Math.Min(System.Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                _d = p;
                p = d;
                d = _d;
            }

            // our last action in the above loop was to switch d and p, so p now
            // actually has the most recent cost counts
            return p[n];
        }
    }
}
