using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace ZemberekDotNet.Core.IO
{
    /// <summary>
    /// SimpleTextWriter is generally used for writing information to the files easily.
    /// <p/>
    /// This class will close the file internally after the operation is done by default. However, if it
    /// is initiated using the Builder and keepOpen() or static "keepOpen" factories are used, Writer
    /// needs to be defined in an ARM block.
    /// </summary>
    public class SimpleTextWriter : IDisposable
    {
        private readonly string encoding;
        private readonly bool keepOpen;
        private readonly FileStream fileStream;
        private readonly StreamWriter writer;
        private bool addNewLineBeforeClose;


        private SimpleTextWriter(
            StreamWriter writer,
            FileStream fileStream,
            string encoding,
            bool keepOpen,
            bool addNewLineBeforeClose)
        {
            this.writer = writer;
            this.encoding = encoding;
            this.keepOpen = keepOpen;
            this.fileStream = fileStream;
            this.addNewLineBeforeClose = addNewLineBeforeClose;
        }

        public FileStream GetFileStream()
        {
            return fileStream;
        }


        /// <summary>
        /// Creates a SimpleFileWriter using default encoding.it does not append to the File by default
        /// and it closes the underlying output stream once any class method is called by default. If a
        /// different behavior is required, SimpleFileWriter.Builder class needs to be used.Please note
        /// that this constructor throws a runtime exception if file is not found instead of a
        /// FileNotFoundException
        /// </summary>
        /// <param name="fileName"></param>
        public SimpleTextWriter(string fileName) : this(fileName, Encoding.Default.BodyName)
        {

        }

        /// <summary>
        /// Creates a SimpleFileWriter using default encoding.it does not append to the File by default
        /// and it closes the underlying output stream once any class method is called by default. If a
        /// different behavior is required, SimpleFileWriter.Builder class needs to be used.Please note
        /// that this constructor throws a runtime exception if file is not found instead of a
        /// FileNotFoundException
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding"></param>
        public SimpleTextWriter(string fileName, string encoding)
        {
            Contract.Requires(fileName != null, "File name cannot be null..");
            this.fileStream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
            this.writer = IOs.GetBufferedWriter(fileStream, encoding);
            this.encoding = encoding;
            keepOpen = false;
        }

        public static Builder UTFt8Builder(string file)
        {
            return new Builder(file).Encoding("UTF-8");
        }

        /// <summary>
        /// creates a one shot writer, meaning that writer will be closed automatically after any write
        /// method call.
        /// </summary>
        /// <param name="file">a file path</param>
        /// <returns></returns>
        public static SimpleTextWriter KeepOpenUTF8Writer(string file)
        {
            return new Builder(file).Encoding("utf-8").KeepOpen().Build();
        }

        /// <summary>
        /// creates a one shot writer, meaning that writer will be closed automatically after any wrte
        /// method call.
        /// </summary>
        /// <param name="file">file to write</param>
        /// <returns>a SimpleTextWriter</returns>
        public static SimpleTextWriter OneShotUTF8Writer(string file)
        {
            return new Builder(file).Encoding("utf-8").Build();
        }

        /// <summary>
        /// creates a one shot writer, meaning that writer will be closed automatically after any wrte
        /// method call.
        /// </summary>
        /// <param name="os">output stream</param>
        /// <param name="encoding">encoding</param>
        /// <returns></returns>
        public static SimpleTextWriter KeepOpenWriter(FileStream os, string encoding)

        {
            return new Builder(os).Encoding(encoding).KeepOpen().Build();
        }

        /// <summary>
        /// creates a one shot writer, meaning that writer will be closed automatically after any wrte
        /// method call.
        /// </summary>
        /// <param name="os">output stream</param>
        /// <param name="encoding">encoding</param>
        /// <returns></returns>
        public static SimpleTextWriter OneShotWriter(FileStream os, string encoding)

        {
            return new Builder(os).Encoding(encoding).Build();
        }

        /// <summary>
        /// creates a one shot writer, meaning that writer will be closed automatically after any write
        /// method call. Uses the default character encoding
        /// </summary>
        /// <param name="os">output stream</param>
        /// <returns>a SimpleTextWriter</returns>
        public static SimpleTextWriter KeepOpenWriter(FileStream os)
        {
            return new Builder(os).KeepOpen().Build();
        }

        /// <summary>
        /// returns the current encoding.
        /// </summary>
        /// <returns>current encoding.</returns>
        public string GetEncoding()
        {
            return encoding;
        }

        public bool IsKeepOpen()
        {
            return keepOpen;
        }


        /**
         * Writes value of each string in a collection to
         *
         * @param lines : lines to write, null entries produce blank lines
         * @return returns the current instance. keep in mind that if instance is not constructed with
         * keepopen, chaining other write methods will throw an exception.
         * @throws java.io.IOException if an I/O error occurs
         */
        public SimpleTextWriter WriteLines(ICollection<string> lines)
        {
            try
            {
                IOs.WriteLines(lines, writer);
                return this;
            }
            finally
            {
                if (!keepOpen)
                {
                    Dispose();
                }
            }
        }

        /**
         * Writes the value of each item in a string Array with the writer.
         *
         * @param lines : lines to write, null entries produce blank lines
         * @return returns the current instance. keep in mind that if instance is not constructed with
         * keepopen, chaining other write methods will throw an exception.
         * @throws java.io.IOException if an I/O error occurs
         */
        public SimpleTextWriter WriteLines(params string[] lines)
        {
            return WriteLines(new List<string>(lines));
        }

        /**
         * Writes the <code>tostring()</code> value of each item in a collection
         *
         * @param objects : lines to write, null entries produce blank lines
         * @return returns the current instance. keep in mind that if instance is not constructed with
         * keepopen, chaining other write methods will throw an exception.
         * @throws java.io.IOException if an I/O error occurs
         */
        public SimpleTextWriter WriteToStringLines<T>(ICollection<T> objects)
        {
            try
            {
                IOs.WriteTostringLines(objects, writer);
                return this;
            }
            finally
            {
                if (!keepOpen)
                {
                    Dispose();
                }
            }
        }

        /**
         * Writes a string to the file.
         *
         * @param s : string to write.
         * @return returns the current instance. keep in mind that if instance is not constructed with
         * keepOpen(), chaining other write methods will throw an exception.
         * @throws java.io.IOException if an I/O error occurs
         */
        public SimpleTextWriter Write(string s)
        {
            try
            {
                if (s == null || s.Length == 0)
                {
                    return this;
                }
                writer.Write(s);
                return this;
            }
            finally
            {
                if (!keepOpen)
                {
                    Dispose();
                }
            }
        }

        /**
         * Writes a string to the file after appending a line separator to it.
         *
         * @param s : string to write.
         * @return returns the current instance. keep in mind that if instance is not constructed with
         * keepOpen(), chaining other write methods will throw an exception.
         * @throws java.io.IOException if an I/O error occurs
         */
        public SimpleTextWriter WriteLine(string s)
        {
            return Write(s + IOs.LineSeperator);
        }

        /// <summary>
        /// Writes a LineSeperator.
        /// </summary>
        /// <returns>returns the current instance.</returns>
        public SimpleTextWriter WriteLine()
        {
            return Write("" + IOs.LineSeperator);
        }

        /**
         * Writes tostring() of an object the file after appending a line separator to it.
         *
         * @param obj : object to write.
         * @return returns the current instance. keep in mind that if instance is not constructed with
         * keepOpen(), chaining other write methods will throw an exception.
         * @throws java.io.IOException if an I/O error occurs
         */
        public SimpleTextWriter WriteLine(Object obj)
        {
            return Write(obj.ToString() + IOs.LineSeperator);
        }

        /**
         * copies an input stream contents to the writer target.
         *
         * @param is input stream
         * @return the text writer.
         * @throws java.io.IOException if an I/O error occurs
         */
        public SimpleTextWriter CopyFromStream(BinaryReader inputStream)
        {
            IOs.Copy(inputStream, writer.BaseStream, keepOpen);
            return this;
        }

        /// <summary>
        /// copies an input stream contents to the writer target.
        /// </summary>
        /// <param name="urlStr"></param>
        /// <returns>this</returns>
        public SimpleTextWriter CopyFromURL(string urlStr)
        {
            IOs.Copy(new BinaryReader(File.OpenRead(urlStr), Encoding.GetEncoding(encoding), keepOpen), writer.BaseStream, keepOpen);
            return this;
        }

        /// <summary>
        /// closes the output stream opened for this writer. if the stream is already closed, it returns
        /// silently.
        /// </summary>
        public void Dispose()
        {
            if (addNewLineBeforeClose)
            {
                writer.WriteLine();
            }
            writer.Flush();
            IOs.CloseSilently(writer);
        }

        /// <summary>
        /// This class provides a flexible way of constructing a SimpleFileWriter instance.Caller can set
        /// the encoding, if write operations will append to the file or if the underlying output stream
        /// needs to be kept open after operations.build() method will create a SimpleFileWriter with the
        /// set parameters.
        /// </summary>
        public class Builder
        {
            private string _encoding;
            private bool _keepOpen;
            private StreamWriter _writer;
            private FileStream _os;
            private bool _addNewLineBeforeClose;

            public Builder(string fileName)
            {
                Contract.Requires(fileName != null, "File name cannot be null..");
                _encoding = System.Text.Encoding.Default.BodyName;
                _os = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
                this._writer = IOs.GetBufferedWriter(_os, _encoding);
            }

            public Builder(FileStream os)
            {
                Contract.Requires(os != null, "Output Stream cannot be null..");
                _encoding = System.Text.Encoding.Default.BodyName;
                _os = os;
                this._writer = IOs.GetBufferedWriter(os, _encoding);
            }

            public Builder Encoding(string encoding)
            {
                if (encoding == null)
                {
                    encoding = System.Text.Encoding.Default.BodyName;
                }
                this._encoding = encoding;
                return this;
            }

            public Builder AddNewLineBeforClose()
            {
                _addNewLineBeforeClose = true;
                return this;
            }

            public Builder KeepOpen()
            {
                this._keepOpen = true;
                return this;
            }

            public SimpleTextWriter Build()
            {
                return new SimpleTextWriter(_writer, _os, _encoding, _keepOpen, _addNewLineBeforeClose);
            }
        }
    }
}
