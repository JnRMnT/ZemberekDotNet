using System.IO;
using System.Text;
using ZemberekDotNet.Core.Native.Text;

namespace ZemberekDotNet.Core.Native.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Reads all lines and processes them with given line processor to build the final result object of type T
        /// </summary>
        /// <typeparam name="T">Type of the resulting object</typeparam>
        /// <param name="filePath">Path of the file</param>
        /// <param name="encoding">Encoding of the file</param>
        /// <param name="lineProcessor">Line Processor instance</param>
        /// <returns></returns>
        public static T ReadAllLines<T>(string filePath, Encoding encoding, ILineProcessor<T> lineProcessor)
        {
            using (FileStream fileStream = System.IO.File.OpenRead(filePath))
            {
                using (StreamReader streamReader = new StreamReader(fileStream, encoding))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineProcessor.ProcessLine(line);
                    }
                    return lineProcessor.GetResult();
                }
            }
        }
    }
}
