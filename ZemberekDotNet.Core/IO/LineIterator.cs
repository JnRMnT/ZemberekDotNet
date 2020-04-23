using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace ZemberekDotNet.Core.IO
{
    /// <summary>
    /// An Iterator over the lines in a <code>Reader</code>.
    /// <p/>
    /// <code>LineIterator</code> holds a reference to an open <code>Reader</code>. if there hasNext()
    /// returns false, it automatically closes the reader. if somewhat an early return is possible
    /// iterator or the reader should be closed by calling {@link #close()} method.
    /// <p/>
    /// The recommended usage pattern is:
    /// <pre>
    /// ;
    /// try(LineIterator it = new LineIterator(Files.getReader("filename", "UTF-8"))) {
    ///   while (it.hasNext()) {
    ///     string line = it.next();
    ///     /// do something with line
    ///   }
    /// }
    /// </pre>
    /// <p/>
    /// This class uses code from Apache commons io LineIterator class. however, it's behavior is
    /// slightly different.
    /// </summary>
    public class LineIterator: IEnumerator<string>, IDisposable
    {
        private readonly StreamReader bufferedReader;
        /**
         * The current line.
         */
        private string cachedLine;
        /**
         * A flag indicating if the iterator has been fully read.
         */
        private bool finished = false;

        private bool trim = false;

        private List<IFilter<string>> filters = new List<IFilter<string>>();

        public string Current { get; set; }

        object IEnumerator.Current => Current;

        public StreamReader BufferedReader => bufferedReader;

        public LineIterator(Stream stream)
        {
            Contract.Requires(stream != null, "InputStream cannot be null!");
            this.bufferedReader = IOs.GetReader(stream);
        }

        public LineIterator(StreamReader reader)
        {
            Contract.Requires(reader != null, "Reader cannot be null!");
            this.bufferedReader = reader;
        }

        public LineIterator(StreamReader reader, bool trim, List<IFilter<string>> filters)
        {
            Contract.Requires(reader != null, "Reader cannot be null!");
            this.bufferedReader = reader;
            this.filters = new List<IFilter<string>>(filters);
            this.trim = trim;
        }

        public bool HasNext()
        {
            if (cachedLine != null)
            {
                return true;
            }
            else if (finished)
            {
                Dispose();
                return false;
            }
            else
            {
                try
                {
                    string line;
                    do
                    {
                        line = BufferedReader.ReadLine();
                        if (line != null && trim)
                        {
                            line = line.Trim();
                        }
                    }
                    while (line != null && filters.Count > 0 && !StringFilters.CanPassAll(line, filters));

                    if (line == null)
                    {
                        finished = true;
                        Dispose();
                        return false;
                    }
                    else
                    {
                        cachedLine = line;
                        return true;
                    }
                }
                catch (IOException ioe)
                {
                    Dispose();
                    throw new InvalidOperationException(ioe.ToString());
                }
            }
        }
        
        public void Dispose()
        {
            IOs.CloseSilently(BufferedReader);
        }

        public bool MoveNext()
        {
            if (!HasNext())
            {
                Dispose();
                return false;
            }
            string currentLine = cachedLine;
            cachedLine = null;
            Current = currentLine;
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
