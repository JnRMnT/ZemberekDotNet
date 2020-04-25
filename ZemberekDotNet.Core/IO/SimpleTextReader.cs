using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using static ZemberekDotNet.Core.IO.SimpleTextReader.Template;

namespace ZemberekDotNet.Core.IO
{
    /// <summary>
    /// SimpleTextReader can be used reading text sources with ease.
    /// </summary>
    public class SimpleTextReader
    {
        private readonly StreamReader streamReader;
        private readonly string encoding;
        private List<IFilter<string>> filters = new List<IFilter<string>>();
        private bool trim = false;
        private Template template;

        public SimpleTextReader(StreamReader textReader, Template template)
        {
            this.template = template;
            this.streamReader = textReader;
            this.encoding = template.encoding;

            List<IFilter<string>> filterz = new List<IFilter<string>>();
            if (template.ignoreWhiteSpaceLines)
            {
                filterz.Add(StringFilters.PassOnlyText);
            }
            if (template.regexp != null)
            {
                filterz.Add(StringFilters.NewRegexpFilter(template.regexp));
            }
            if (template.ignorePrefix != null)
            {
                filterz.Add(new IgnorePrefixFilter(template.ignorePrefix));
            }

            this.filters = filterz;

            this.trim = template.trim;
        }

        /// <summary>
        /// Creates a FileReader using the file path.
        /// </summary>
        /// <param name="file"></param>
        public SimpleTextReader(string filePath)
        {
            Contract.Requires(filePath != null, "File name cannot be null..");
            streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            encoding = Encoding.Default.BodyName;
            this.template = new Template(encoding);
        }
        
        public SimpleTextReader(Stream stream)
        {
            Contract.Requires(stream != null, "Stream cannot be null..");
            streamReader = new StreamReader(stream);
            encoding = Encoding.Default.BodyName;
            this.template = new Template(encoding);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">a file.</param>
        /// <param name="encoding"></param>
        public SimpleTextReader(string filePath, string encoding)
        {
            Contract.Requires(filePath != null, "File name cannot be null..");
            streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            if (encoding == null)
            {
                this.encoding = Encoding.Default.BodyName;
            }
            else
            {
                this.encoding = encoding;
            }
            this.template = new Template(encoding);
        }

        public SimpleTextReader(StreamReader streamReader)
        {
            Contract.Requires(streamReader != null, "Input Stream cannot be null..");
            this.streamReader = streamReader;
            this.encoding = Encoding.Default.BodyName;
            this.template = new Template(encoding);
        }

        public SimpleTextReader(StreamReader streamReader, string encoding)
        {
            Contract.Requires(streamReader != null, "Input Stream cannot be null..");
            this.streamReader = streamReader;
            if (encoding == null)
            {
                this.encoding = Encoding.Default.BodyName;
            }
            else
            {
                this.encoding = encoding;
            }
            this.template = new Template(encoding);
        }


        /// <summary>
        /// a new SimpleTextReader that skips the whitespace lines and trims lines.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static SimpleTextReader TrimmingUTF8Reader(string filePath)
        {
            return new Builder(filePath).Encoding("utf-8").Trim().IgnoreWhiteSpaceLines().Build();
        }

        /**
         * Returns a new UTF-8 LineIterator that skips the whitespace lines and trims lines.
         *
         * @param file file
         * @return a new LineIterator
         * @throws java.io.IOException if a porblem occurs while accessing file.
         */
        public static LineIterator TrimmingUTF8LineIterator(string file)
        {
            return new Builder(file).Encoding("utf-8").Trim().IgnoreWhiteSpaceLines().Build()
                .GetLineIterator();
        }

        /**
         * Returns a new UTF-8 IterableLineReader that skips the whitespace lines and trims lines.
         *
         * @param file file
         * @return a new IterableLineReader
         * @throws java.io.IOException if a porblem occurs while accessing file.
         */
        public static IterableLineReader TrimmingUTF8IterableLineReader(string file)
        {
            return new Builder(file).Trim().IgnoreWhiteSpaceLines().Build().GetIterableReader();
        }

        /**
         * Returns a new SimpleTextReader that skips the whitespace lines and trims lines.
         *
         * @param is input stream to read.
         * @param encoding character encoding. if null, default encoding is used.
         * @return a new SimpleTextReader
         * @throws java.io.IOException if a porblem occurs while accessing file.
         */
        public static SimpleTextReader TrimmingReader(StreamReader streamReader, string encoding)

        {
            return new Builder(streamReader).Encoding(encoding).Trim().IgnoreWhiteSpaceLines().Build();
        }

        /**
         * Returns a new LineIterator that skips the whitespace lines and trims lines.
         *
         * @param is input stream to read.
         * @param encoding character encoding. if null, default encoding is used.
         * @return a new LineIterator
         * @throws java.io.IOException if a porblem occurs while accessing file.
         */
        public static LineIterator TrimmingLineIterator(StreamReader streamReader, string encoding)

        {
            return new Builder(streamReader).Encoding(encoding).Trim().IgnoreWhiteSpaceLines().Build()
                .GetLineIterator();
        }

        /**
         * Returns a new IterableLineReader that skips the whitespace lines and trims lines.
         *
         * @param is input stream to read.
         * @param encoding character encoding.
         * @return a new IterableLineReader
         * @throws java.io.IOException if a porblem occurs while accessing file.
         */
        public static IterableLineReader trimmingIterableLineReader(StreamReader streamReader, string encoding)

        {
            return new Builder(streamReader).Encoding(encoding).Trim().IgnoreWhiteSpaceLines().Build()
                .GetIterableReader();
        }

        /**
         * returns the current encoding.
         *
         * @return current encoding.
         */
        public string GetEncoding()
        {
            return encoding;
        }

        /**
         * it generates a new SimpleTextReader using this.
         *
         * @param is an input stream for reader.
         * @return a new SimpleTextReader having the same attribues of this one.
         * @throws java.io.IOException if there is an error while accessing the input stream.
         */
        public SimpleTextReader CloneForStream(StreamReader streamReader)
        {
            return this.template.GenerateReader(streamReader);
        }

        /**
         * it generates a new SimpleTextReader using this.
         *
         * @param file File for the new Reader.
         * @return a new SimpleTextReader having the same attribues of this one.
         * @throws java.io.IOException if there is an error while accessing the file.
         */
        public SimpleTextReader CloneForFile(string file)
        {
            return this.template.GenerateReader(file);
        }

        /**
         * converts an input stream data to byte array. careful with memory usage here.
         *
         * @return a byte array representing the stream data.
         * @throws java.io.IOException if an error occurs during the read or write of the streams.
         * @throws NullPointerException if filename is null
         */
        public byte[] AsByteArray()
        {
            return IOs.ReadAsByteArray(streamReader.BaseStream);
        }

        /**
         * Reads the entire file as a single string. Use with caution for big files.
         *
         * @return simgle string representation.
         * @throws java.io.IOException if an IO error occurs
         */
        public string AsString()
        {
            string res = IOs.ReadAsString(GetReader());
            if (trim)
            {
                return res.Trim();
            }
            else
            {
                return res;
            }
        }

        /**
         * Reads a reader as a list of strings. each item represents one line in the reader.
         *
         * @return a list of strings from the reader.
         * @throws java.io.IOException if an io error occurs
         */
        public List<string> AsStringList()
        {
            return IOs.ReadAsStringList(GetReader(), trim, filters.ToArray());
        }

        /**
         * return a buffered reader for the file
         *
         * @return buffered reader
         * @throws RuntimeException if file does not exist
         * @throws java.io.IOException if file does not exist or encoding is not available
         */
        StreamReader GetReader()
        {
            return IOs.GetReader(streamReader.BaseStream, encoding);

        }

        /**
         * returns an IterableLineReader. This is expecially useful to use in enhanced for loops. if all
         * the elements are consumed, the resources will be closed automatically.
         *
         * @return a new IterableLineReader instance.
         * @throws java.io.IOException if file does not exist, or encoding is not supported.
         */
        public IterableLineReader GetIterableReader()
        {
            return new IterableLineReader(GetReader(), trim, filters);
        }

        /**
         * returns a LineIterator. it is suggested to close th iterator in a readonlyly block.
         *
         * @return an IterableLineReader that can be iterated for lines.
         * @throws java.io.IOException if file does not exist, or encoding is not supported.
         */
        public LineIterator GetLineIterator()
        {
            return new LineIterator(GetReader(), trim, filters);
        }

        /**
         * counts the lines. if there are constraints while creating the reader (eg: not reading empty
         * lines), it counts ONLY the lines that are allowed to be read.
         *
         * @return line count.
         * @throws java.io.IOException if there is a problem while accesing the file.
         */
        public long CountLines()
        {
            long i;
            LineIterator li = GetLineIterator();
            i = 0;
            while (li.HasNext())
            {
                i++;
                li.MoveNext();
            }
            return i;
        }

        /// <summary>
        /// Closes the stream silently.
        /// </summary>
        public void Close()
        {
            streamReader.Close();
        }


        public class Builder
        {

            private StreamReader streamReader;
            private Template template = new Template();

            public Builder(string fileName)
            {
                Contract.Requires(fileName != null, "File name cannot be null..");
                this.streamReader = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            }

            public Builder(StreamReader streamReader)
            {
                Contract.Requires(streamReader != null, "File name cannot be null..");
                this.streamReader = streamReader;
            }

            public Builder Encoding(string encoding)
            {
                if (encoding != null)
                {
                    this.template.encoding = encoding;
                }
                return this;
            }

            public Builder IgnoreWhiteSpaceLines()
            {
                this.template.ignoreWhiteSpaceLines = true;
                return this;
            }

            public Builder IgnoreIfStartsWith(params string[] prefix)
            {
                this.template.ignorePrefix = prefix;
                return this;
            }

            public Builder AllowMatchingRegexp(string regexp)
            {
                this.template.regexp = regexp;
                return this;
            }

            public SimpleTextReader Build()
            {
                return new SimpleTextReader(streamReader, template);
            }

            public Builder Trim()
            {
                this.template.trim = true;
                return this;
            }
        }

        public class Template
        {
            internal string encoding;
            internal bool trim = false;
            internal bool ignoreWhiteSpaceLines = false;
            internal string regexp;
            internal string[] ignorePrefix;

            public Template()
            {
                encoding = System.Text.Encoding.Default.BodyName;
            }

            public Template(string encoding)
            {
                this.encoding = encoding;
            }

            public Template Encoding(string encoding)
            {
                if (encoding == null)
                {
                    this.encoding = System.Text.Encoding.Default.BodyName;
                }
                this.encoding = encoding;
                return this;
            }

            public Template IgnoreWhiteSpaceLines()
            {
                this.ignoreWhiteSpaceLines = true;
                return this;
            }

            public Template IgnoreIfStartsWith(params string[] prefix)
            {
                this.ignorePrefix = prefix;
                return this;
            }

            public Template AllowMatchingRegexp(string regexp)
            {
                this.regexp = regexp;
                return this;
            }

            public SimpleTextReader GenerateReader(StreamReader streamReader)
            {
                return new SimpleTextReader(streamReader, this);
            }

            public SimpleTextReader GenerateReader(string fileName)
            {
                return new SimpleTextReader(new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)), this);
            }

            public Template Trim()
            {
                this.trim = true;
                return this;
            }


            internal class IgnorePrefixFilter : IFilter<string>
            {

                string[] tokens;

                internal IgnorePrefixFilter(params string[] token)
                {
                    Contract.Requires(token != null, "Cannot initialize Filter with null string.");
                    this.tokens = token;
                }

                public bool CanPass(string s)
                {
                    foreach (string token in tokens)
                    {
                        if (s.StartsWith(token))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
    }
}