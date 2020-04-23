using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZemberekDotNet.Core.Text
{
    public class Regexps
    {
        public static readonly Regex WhitespacePattern = new Regex("\\s+");

        public static List<string> FirstGroupMatches(Regex p, string s)
        {
            List<string> matches = new List<string>();
            MatchCollection matchCollection = p.Matches(s);
            foreach (Match match in matchCollection)
            {
                matches.Add(match.Groups[1].Value.Trim());
            }

            return matches;
        }

        public static List<string> AllMatches(Regex p, string s)
        {
            List<string> matches = new List<string>();
            MatchCollection matchCollection = p.Matches(s);
            foreach (Match match in matchCollection)
            {
                matches.Add(match.Groups[0].Value);
            }

            return matches;
        }

        public static string FirstMatchFirstGroup(Regex p, string s)
        {
            Match match = p.Match(s);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        public static string FirstMatch(Regex p, string s, int group)
        {
            Match match = p.Match(s);
            if (match.Success)
            {
                return match.Groups[group].Value;
            }
            else
            {
                return null;
            }
        }

        public static string FirstMatch(Regex p, string s)
        {
            Match match = p.Match(s);
            if (match.Success)
            {
                return match.Groups[0].Value;
            }
            else
            {
                return null;
            }
        }

        public static bool MatchesAny(Regex p, string s)
        {
            return p.IsMatch(s);
        }

        public static bool MatchesAny(string regexp, string s)
        {
            return new Regex(regexp).IsMatch(s);
        }

        public static Regex DefaultPattern(string regexp)
        {
            return new Regex(regexp, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        /// <summary>
        /// checks the matches if they exist as a key in the map. if it exists, replaces the match with the
        /// "value" in the map.
        /// </summary>
        /// <param name="pattern">pattern</param>
        /// <param name="map">map to replace matches with values.</param>
        /// <returns>string after the replacement.</returns>
        public static string ReplaceMap(Regex pattern, string text, Dictionary<string, string> map)
        {
            MatchEvaluator matchEvaluator = new MatchEvaluator((Match match) =>
            {
                if (map.ContainsKey(match.Groups[0].Value))
                {
                    return map.GetValueOrDefault(match.Groups[0].Value);
                }
                else
                {
                    return match.Groups[0].Value;
                }
            });
            return pattern.Replace(text, matchEvaluator);
        }

        public static List<string> GetMatchesForGroup(string str, Regex pattern, int groupIndex)
        {
            List<string> result = new List<string>();
            MatchCollection matches = pattern.Matches(str);
            foreach (Match match in matches)
            {
                result.Add(match.Groups[groupIndex].Value);
            }

            return result;
        }
    }
}
