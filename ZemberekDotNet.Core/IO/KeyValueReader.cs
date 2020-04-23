using System;
using System.Collections.Generic;
using System.IO;

namespace ZemberekDotNet.Core.IO
{
    public class KeyValueReader
    {
        private readonly string separator;
        private readonly string ignorePrefix;

        public KeyValueReader(string seperator)
        {
            if (string.IsNullOrEmpty(seperator))
            {
                throw new ArgumentException("Separator is null or empty!");
            }
            this.separator = seperator;
            this.ignorePrefix = "#";
        }

        public KeyValueReader(string seperator, string ignorePrefix)
        {
            if (string.IsNullOrEmpty(seperator))
            {
                throw new ArgumentException("Separator is null or empty!");
            }
            this.separator = seperator;
            this.ignorePrefix = ignorePrefix;
        }

        public Dictionary<string, string> LoadFromFile(string file)
        {
            return LoadFromFile(new SimpleTextReader
                 .Builder(file)
                .Trim()
                .IgnoreIfStartsWith(ignorePrefix)
                .IgnoreWhiteSpaceLines()
                .Build());
        }

        public Dictionary<string, string> LoadFromFile(string file, string encoding)
        {
            return LoadFromFile(new SimpleTextReader
                .Builder(file)
                .Trim()
                .IgnoreIfStartsWith(ignorePrefix)
                .IgnoreWhiteSpaceLines()
                .Encoding(encoding)
                .Build());
        }

        public Dictionary<string, string> LoadFromStream(StreamReader stream)
        {
            return LoadFromFile(new SimpleTextReader.
                Builder(stream)
                .Trim()
                .IgnoreIfStartsWith(ignorePrefix)
                .IgnoreWhiteSpaceLines()
                .Build());
        }

        public Dictionary<string, string> LoadFromStream(StreamReader stream, string encoding)
        {
            return LoadFromFile(new SimpleTextReader.
                Builder(stream)
                .Trim()
                .IgnoreIfStartsWith(ignorePrefix)
                .IgnoreWhiteSpaceLines()
                .Encoding(encoding)
                .Build());
        }

        public Dictionary<string, string> LoadFromFile(SimpleTextReader sfr)
        {
            List<string> lines = sfr.AsStringList();

            if (lines.Count == 0)
            {
                return new Dictionary<string, string>();
            }
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                if (!line.Contains(separator))
                {
                    throw new ArgumentException("line: [" + line + "] has no separator:" + separator);
                }
                string key = Strings.SubstringUntilFirst(line, separator).Trim();
                string value = Strings.SubstringAfterFirst(line, separator).Trim();
                result.TryAdd(key, value);
            }
            return result;
        }
    }
}
