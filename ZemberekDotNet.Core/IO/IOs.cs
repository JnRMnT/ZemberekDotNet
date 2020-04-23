using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Contracts = System.Diagnostics.Contracts;

namespace ZemberekDotNet.Core.IO
{
    /// <summary>
    /// this class has IO operations
    /// </summary>
    public class IOs
    {
        public static readonly string LineSeperator = Environment.NewLine;
        public static readonly int CharBufferSize = 1 << 20;
        private static readonly int BYTE_BUFFER_SIZE = 1 << 20;
        private static readonly byte[] bomBytes = new byte[] { (byte)0xef, (byte)0xbb, (byte)0xbf };

        private IOs()
        {
        }

        /**
         * Reads a buffered reader as a single string. If there are multi lines, it appends a LINE
         * SEPARATOR.
         *
         * @param reader a reader
         * @return simple string representation of the entire reader. careful with the memory usage.
         * @throws NullPointerException: if reader is null.
         * @throws java.io.IOException if an IO error occurs.
         */
        public static string ReadAsString(StreamReader reader)
        {
            try
            {
                Contracts.Contract.Requires(reader != null, "reader cannot be null");
                StringBuilder sb = new StringBuilder();
                string s;
                while ((s = reader.ReadLine()) != null)
                {
                    sb.Append(s).Append(LineSeperator);
                }
                if (sb.Length >= LineSeperator.Length)
                {
                    sb.Remove(sb.Length - LineSeperator.Length, sb.Length);
                }
                return sb.ToString();
            }
            finally
            {
                CloseSilently(reader);
            }
        }

        /// <summary>
        /// Reads a reader as a list of strings. each item represents one line in the reader.
        /// </summary>
        /// <param name="reader">a reader</param>
        /// <returns>List of strings.</returns>
        public static List<string> ReadAsStringList(StreamReader reader)

        {
            try
            {
                Contracts.Contract.Requires(reader != null, "reader cannot be null");
                string s;
                List<string> res = new List<string>();
                while ((s = reader.ReadLine()) != null)
                {
                    res.Add(s);
                }
                return res;
            }
            finally
            {
                CloseSilently(reader);
            }
        }

        /// <summary>
        /// Reads a reader as a list of strings. each item represents one line in the reader which passes
        /// the Filters.
        /// </summary>
        /// <param name="reader">a reader</param>
        /// <param name="trim">trims the lines if set</param>
        /// <param name="filters">zero or more stringFilter. if there are more than one all filters needs to pass
        /// the string.</param>
        /// <returns>List of strings.</returns>
        public static List<string> ReadAsStringList(StreamReader reader, bool trim, params IFilter<string>[] filters)

        {
            try
            {
                Contracts.Contract.Requires(reader != null, "reader cannot be null");
                string s;
                List<string> res = new List<string>();
                while ((s = reader.ReadLine()) != null)
                {
                    if (trim)
                    {
                        s = s.Trim();
                    }
                    if (filters.Length == 0 || StringFilters.CanPassAll(s, filters))
                    {
                        res.Add(s);
                    }
                }
                return res;
            }
            finally
            {
                CloseSilently(reader);
            }
        }

        /// <summary>
        /// closes the<code>closeables</code> silently, meaning that if the Closeable is null, or if it
        /// throws an exception during close() operation it only creates a system error output, does not
        /// throw an exception. this is especially useful when you need to close one or more resources in
        /// finally blocks.This method should only be called in readonlyize{ } blocks or wherever it really
        /// makes sense.
        /// </summary>
        /// <param name="closeables">zero or more closeable.</param>
        public static void CloseSilently(params IDisposable[] closeables)
        {
            // if closeables is null, return silently.
            if (closeables == null)
            {
                return;
            }

            foreach (IDisposable closeable in closeables)
            {
                try
                {
                    if (closeable != null)
                    {
                        closeable.Dispose();
                    }
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine("IO Exception during closing stream (" + closeable + ")." + e);
                }
            }
        }

        /// <summary>
        /// Returns a StreamReader for the input stream.
        /// </summary>
        /// <param name="stream">input stream</param>
        /// <returns>a bufferedReader for the input stream.</returns>
        public static StreamReader GetReader(Stream stream)
        {
            return new StreamReader(stream, Encoding.Default, true, CharBufferSize);
        }

        /// <summary>
        /// Returns a Buffered reader for the given input stream and charset. if charset is UTF-8 it
        /// explicitly checks for UTF-8 BOM information.
        /// </summary>
        /// <param name="stream">input stream for the reader.</param>
        /// <param name="charset">charset string, if null,empty or has only whitespace, system uses default</param>
        /// <returns>StreamReader</returns>
        public static StreamReader GetReader(Stream stream, string charset)
        {
            Contracts.Contract.Requires(stream != null, "input stream cannot be null");
            if (!Strings.HasText(charset))
            {
                return GetReader(stream);
            }
            if (charset.Trim().Equals("utf-8", StringComparison.InvariantCultureIgnoreCase))
            {
                return new StreamReader(stream, Encoding.UTF8, true, CharBufferSize);
            }
            else
            {
                return new StreamReader(stream, Encoding.GetEncoding(charset), true, CharBufferSize);
            }

        }

        /// <summary>
        /// returns a BufferedWriter for the output stream.
        /// </summary>
        /// <param name="os">output stream</param>
        /// <param name="encoding"></param>
        /// <returns>a bufferedReader for the output stream.</returns>
        public static StreamWriter GetBufferedWriter(Stream os, string encoding)
        {
            try
            {
                return new StreamWriter(os, Encoding.GetEncoding(encoding), CharBufferSize);
            }
            catch (ArgumentException e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// returns a BufferedWriter for the output stream.
        /// </summary>
        /// <param name="os">output stream</param>
        /// <param name="charset">charset encoding string.</param>
        /// <returns>a bufferedReader for the output stream.</returns>
        public static StreamWriter GetWriter(Stream os, string charset)
        {
            return new StreamWriter(os, Encoding.GetEncoding(charset), CharBufferSize);

        }
        ///**
        // * returns a PrintWriter for the output stream.
        // *
        // * @param os output stream
        // * @return a bufferedReader for the output stream.
        // */
        //public static PrintWriter getPrintWriter(OutputStream os)
        //{
        //    return new PrintWriter(new BufferedWriter(new OutputStreamWriter(os), CharBufferSize));
        //}

        //      /**
        //       * returns a PrintWriter for the output stream.
        //       *
        //       * @param os output stream
        //       * @param charset encoding string.
        //       * @return a PrintWriter for the output stream.
        //       * @throws java.io.UnsupportedEncodingException if encoding is not supported.
        //       */
        //      public static PrintWriter getPrintWriter(OutputStream os, string charset) 
        //      {
        //  return new PrintWriter(
        //      new BufferedWriter(new OutputStreamWriter(os, charset), CHAR_BUFFER_SIZE));
        //}


        /// <summary>
        /// returns an IterableLineReader backed by a LineIterator.Can be used directly in enhanced for
        /// loops.Please note that if not all the lines are read, reader will not be closed.So, it is
        /// suggested to close the IterableLineReader in a finally block using {@link
        /// #closeSilently(java.io.Closeable...)}
        /// </summary>
        /// <param name="stream">an input stream.</param>
        /// <returns>an IterableLineReader that can be iterated for lines.</returns>
        public static IterableLineReader GetIterableReader(Stream stream)
        {
            return new IterableLineReader(GetReader(stream));
        }

        /// <summary>
        /// returns an IterableLineReader backed by a LineIterator.Can be used directly in enhanced for
        /// loops.Please note that if not all the lines are read, reader will not be closed.So, it is
        /// suggested to close the IterableLineReader in a finally block using {@link
        /// #closeSilently(java.io.Closeable...)}
        /// </summary>
        /// <param name="stream">input stream</param>
        /// <param name="charset">charset the charset.</param>
        /// <returns>an IterableLineReader that can be iterated for lines.</returns>
        public static IterableLineReader GetIterableReader(Stream stream, string charset)

        {
            return new IterableLineReader(GetReader(stream, charset));
        }

        /**
         * Copies oan input stream content to an output stream. Once the copy is finished streams will be
         * closed.
         *
         * @param is input stream
         * @param os output stream
         * @return copied byte count.
         * @throws java.io.IOException if an IO error occurs.
         */
        public static long Copy(BinaryReader inputStream, BinaryWriter outputStream)
        {
            return Copy(inputStream, outputStream, false);
        }

        /// <summary>
        /// Copies oan input stream content to an output stream. Once the copy is finished only the input
        /// stream is closed by default. Closing of the output stream depends on the bool parameter.
        /// </summary>
        /// <param name="inputStream">input stream</param>
        /// <param name="outputStream">output stream</param>
        /// <param name="keepOutputOpen">keepOutputOpen if true, output stream will not be closed.</param>
        /// <returns></returns>
        internal static long Copy(BinaryReader inputStream, BinaryWriter outputStream, bool keepOutputOpen)
        {
            long total = 0;
            try
            {
                Contracts.Contract.Requires(inputStream != null, "Input stream cannot be null.");
                Contracts.Contract.Requires(outputStream != null, "Output stream cannot be null.");
                byte[] buf = new byte[BYTE_BUFFER_SIZE];
                int i;
                while ((i = inputStream.Read(buf)) != -1)
                {
                    outputStream.Write(buf, 0, i);
                    total += i;
                }
            }
            finally
            {
                CloseSilently(inputStream);
                if (!keepOutputOpen)
                {
                    CloseSilently(outputStream);
                }
            }
            return total;
        }

        /// <summary>
        /// compares two input stream contents. Streams will be closed after the operation is ended or
        /// interrupted.
        /// <p/>
        /// copied and modified from Apache Commons-io
        /// </summary>
        /// <param name="is1">first input stream</param>
        /// <param name="is2">second input stream.</param>
        /// <returns>true if contents of two streams are equal.</returns>
        public static bool contentEquals(StreamReader is1, StreamReader is2)
        {
            try
            {
                Contracts.Contract.Requires(is1 != null, "Input stream 1 cannot be null.");
                Contracts.Contract.Requires(is2 != null, "Input stream 2 cannot be null.");

                int ch = is1.Read();
                int ch2;
                while (-1 != ch)
                {
                    ch2 = is2.Read();
                    if (ch != ch2)
                    {
                        return false;
                    }
                    ch = is1.Read();
                }
                ch2 = is2.Read();
                return (ch2 == -1);
            }
            finally
            {
                CloseSilently(is1, is2);
            }
        }

        /**
         * Calculates the MD5 of a stream.
         *
         * @param is a non null stream
         * @return MD5 of the stream as byte array.
         * @throws java.io.IOException if an error occurs during read of the stream.
         * @throws NullPointerException if input stream is null
         */
        public static byte[] calculateMD5(BinaryReader inputStream)
        {
            try
            {
                Contracts.Contract.Requires(inputStream != null, "input stream cannot be null.");
                using (MD5 md5Hash = MD5.Create())
                {
                    byte[] buffer = new byte[BYTE_BUFFER_SIZE];
                    int read;

                    while ((read = inputStream.Read(buffer)) > 0)
                    {
                        md5Hash.TransformBlock(buffer, 0, read, null, 0);
                    }
                    md5Hash.TransformFinalBlock(new byte[0], 0, 0);
                    return md5Hash.Hash;
                }
            }
            finally
            {
                CloseSilently(inputStream);
            }
        }

        /**
         * converts an input stream data to byte array. careful with memory usage here. if aim is to
         * transfer bytes from one stream to another, use {@link #copy(java.io.InputStream,
         * java.io.OutputStream)} instead.
         *
         * @param is , an input stream
         * @return a byte array representing the stream data.
         * @throws java.io.IOException if an error occurs during the read or write of the streams.
         * @throws NullPointerException if input stream is null
         */
        public static byte[] ReadAsByteArray(Stream stream)
        {
            try
            {
                using (var streamReader = new MemoryStream())
                {
                    stream.CopyTo(streamReader);
                    return streamReader.ToArray();
                }
            }
            finally
            {
                CloseSilently(stream);
            }
        }

        /**
         * Writes the value of each item in a collection to an <code>OutputStream</code> line by line,
         * using the default character encoding of the platform. A Line separator will be added to each
         * line. If there is a null value, an empty line will be added. The output stream will Not be
         * closed once the operation is finished.
         * <p/>
         * copied and modified from Apache Commons-io
         *
         * @param lines the lines to write, null entries produce blank lines
         * @param output the <code>OutputStream</code> to write to, not null, not closed
         * @throws NullPointerException if the output is null
         * @throws java.io.IOException if an I/O error occurs
         */
        public static void WriteLines(ICollection<string> lines, StreamWriter output)
        {
            if (lines == null)
            {
                return;
            }

            foreach (string line in lines)
            {
                if (line != null)
                {
                    output.Write(line);
                }
                output.Write(LineSeperator);
            }
        }

        /**
         * Writes the string to <code>OutputStream</code> The stream will Not be closed once the operation
         * is finished.
         * <p/>
         *
         * @param s string to write.
         * @param output the <code>OutputStream</code> to write to, not null, not closed
         * @param encoding character encoding.
         * @throws NullPointerException if the output is null
         * @throws java.io.IOException if an I/O error occurs
         */
        public static void WriteString(string s,
            StreamWriter output,
            string encoding)
        {
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            if (encoding == null)
            {
                encoding = Encoding.Default.BodyName;
            }
            output.Write(Encoding.GetEncoding(encoding).GetBytes(s));
        }

        /**
         * retrieves a classpath resource as stream.
         *
         * @param resource resource name. may or may not contain a / symbol.
         * @return an InputStrean obtained from the resource
         */
        public static StreamReader GetResourceAsStream(string resource)
        {
            return new StreamReader(new FileStream(resource, FileMode.Open, FileAccess.Read));
        }

        /**
         * Writes the <code>ToString()</code> value of each item in a collection to an
         * <code>OutputStream</code> line by line, using the default character encoding of the platform.
         * if an element is null, nothing is written for it. The stream will Not be closed once the
         * operation is finished.
         * <p/>
         * copied and modified from commons-io
         *
         * @param lines the lines to write, null entries produce blank lines
         * @throws NullPointerException if the output is null
         * @throws java.io.IOException if an I/O error occurs
         */
        public static void WriteTostringLines(
            ICollection<object> lines,
            StreamWriter writer)
        {

            if (lines == null)
            {
                return;
            }

            long i = 0;
            foreach (object line in lines)
            {
                string l = "";
                if (line != null)
                {
                    l = line.ToString();
                }

                if (!string.IsNullOrEmpty(l))
                {
                    writer.Write(l);
                }
                else
                {
                    writer.Write(LineSeperator);
                    continue;
                }
                if (++i < lines.Count)
                {
                    writer.Write(LineSeperator);
                }
            }
        }

        /**
         * returns a LineIterator. it is suggested to close th eiterator in a finally block.
         *
         * @param is input stream to read.
         * @return an IterableLineReader that can be iterated for lines.
         */
        public LineIterator GetLineIterator(Stream inputStream)
        {
            return new LineIterator(GetReader(inputStream));
        }

        /**
         * returns a LineIterator. it is suggested to close th eiterator in a finally block.
         *
         * @param is input stream to read.
         * @param charset charset
         * @return an IterableLineReader that can be iterated for lines.
         * @throws java.io.IOException if charset is not available, or there is an error ocurred during
         * utf-8 test.
         */
        public LineIterator GetLineIterator(Stream inputStream, string charset)
        {
            return new LineIterator(GetReader(inputStream, charset));
        }
    }
}
