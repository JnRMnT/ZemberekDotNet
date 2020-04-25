using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZemberekDotNet.Core.IO
{
    public class Dumper
    {
        /// <summary>
        /// dumps the contents of an input stream in hex format to an output stream. both stream are closed
        /// at the end of method call
        /// </summary>
        /// <param name="inputStream">input Stream</param>
        /// <param name="os">output stream to write hex values</param>
        /// <param name="column">the column size of the hex numbers.</param>
        /// <param name="amount">amount of bytes to write.</param>
        public static void HexDump(Stream inputStream, TextWriter outputStream, int column, long amount)
        {
            try
            {
                byte[] bytes = new byte[column];
                int i;
                long total = 0;
                while ((i = inputStream.Read(bytes)) != -1)
                {
                    for (int j = 0; j < i; j++)
                    {
                        outputStream.Write(Bytes.ToHexWithZeros(bytes[j]) + " ");
                    }
                    for (int j = 0; j < i; j++)
                    {
                        char c = (char)bytes[j];
                        if (!char.IsWhiteSpace(c))
                        {
                            outputStream.Write((char)bytes[j]);
                        }
                        else
                        {
                            outputStream.Write(" ");
                        }
                    }
                    outputStream.WriteLine();
                    total += i;
                    if (total >= amount && amount > -1)
                    {
                        break;
                    }
                }
            }
            finally
            {
                IOs.CloseSilently(inputStream, outputStream);
            }
        }
    }
}
