using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace ZemberekDotNet.Core.IO
{
    public class StringFilters
    {
        public static readonly IFilter<string> Passall = new AllPassFilter();
        public static readonly IFilter<string> PassNonNullOrEmpty = new NullOrEmptyFilter();
        public static readonly IFilter<string> PassOnlyText = new HasNoTextFilter();

        public static IFilter<string> NewRegexpFilter(string regexp)
        {
            return new RegexpFilter(regexp, false);
        }

        public static IFilter<string> NewRegexpFilterIgnoreCase(string regexp)
        {
            return new RegexpFilter(regexp, true);
        }

        public static IFilter<string> NewRegexpFilter(Regex pattern)
        {
            return new RegexpFilter(pattern);
        }

        public static IFilter<string> newPrefixFilter(string prefix)
        {
            return new PrefixFilter(prefix);
        }

        public static bool CanPassAll(string s, List<IFilter<string>> filters)
        {
            foreach (IFilter<string> filter in filters)
            {
                if (!filter.CanPass(s))
                {
                    return false;
                }
            }
            return true;

        }

        public static bool CanPassAll(string s, params IFilter<string>[] filters)
        {

            foreach (IFilter<string> filter in filters)
            {
                if (!filter.CanPass(s))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CanPassAny(string s, params IFilter<string>[] filters)
        {
            foreach (IFilter<string> filter in filters)
            {
                if (filter.CanPass(s))
                {
                    return true;
                }
            }
            return false;
        }

        private class AllPassFilter : IFilter<string>
        {

            public bool CanPass(string str)
            {
                return true;
            }
        }

        private class NullOrEmptyFilter : IFilter<string>
        {

            public bool CanPass(string str)
            {
                return !string.IsNullOrEmpty(str);
            }
        }

        private class HasNoTextFilter : IFilter<string>
        {

            public bool CanPass(string str)
            {
                return Strings.HasText(str);
            }
        }

        private class PrefixFilter : IFilter<string>
        {
            internal string token;
            internal PrefixFilter(string token)
            {
                Contract.Requires(token != null, "Cannot initialize Filter with null string.");
                this.token = token;
            }

            public bool CanPass(string s)
            {
                return s != null && s.StartsWith(token);
            }
        }

        private class RegexpFilter : IFilter<string>
        {
            readonly Regex pattern;
            public RegexpFilter(string regExp, bool ignoreCase)
            {
                Contract.Requires(regExp != null, "regexp string cannot be null.");
                Contract.Requires(!string.IsNullOrEmpty(regExp), "regexp string cannot be empty");
                if (ignoreCase)
                {
                    this.pattern = new Regex(regExp, RegexOptions.IgnoreCase);
                }
                else
                {
                    this.pattern = new Regex(regExp);
                }
            }

            public RegexpFilter(Regex pattern)
            {
                this.pattern = pattern;
            }

            public bool CanPass(string s)
            {
                return s != null && pattern.IsMatch(s);
            }
        }
    }
}
