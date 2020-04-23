using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ZemberekDotNet.Core.IO
{
    public class IterableLineReader : IEnumerable<string>, IDisposable
    {
        private readonly StreamReader bufferedReader;
        private readonly bool trim;
        private readonly List<IFilter<string>> filters = new List<IFilter<string>>();

        public IterableLineReader(StreamReader reader)
        {
            this.bufferedReader = reader;
        }

        public IterableLineReader(StreamReader reader, bool trim, List<IFilter<string>> filters)
        {
            this.bufferedReader = reader;
            this.filters = filters;
            this.trim = trim;
        }

        public void Dispose()
        {
            IOs.CloseSilently(bufferedReader);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new LineIterator(bufferedReader, trim, filters);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new LineIterator(bufferedReader, trim, filters);
        }
    }
}