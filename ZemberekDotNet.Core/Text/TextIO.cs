using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.Core.Text
{
    public class TextIO
    {
        public static string LoadUtfAsstring(string filePath)
        {
            return string.Join("\n", File.ReadAllLines(filePath, Encoding.UTF8));
        }

        /// <summary>
        /// Loads lines from a UTF-8 encoded text file. Ignores empty lines.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> LoadLines(string path)
        {
            return File.ReadAllLines(path, Encoding.UTF8).Where(e => e.Trim().Length > 0).ToList();
        }

        public static long LineCount(string p)
        {
            using (StreamReader streamReader = new StreamReader(p, Encoding.UTF8))
            {
                int count = 1;
                for (int c = 0; c != -1; c = streamReader.Read())
                {
                    count += c == '\n' ? 1 : 0;
                }
                return count;
            }
        }

        public static List<string> LoadLines(string path, string ignorePrefix)
        {
            return File.ReadAllLines(path, Encoding.UTF8).Where(s => s.Trim().Length > 0 &&
                    (ignorePrefix == null || !s.Trim().StartsWith(ignorePrefix))).ToList();
        }

        public static List<string> LoadLinesFromCompressed(string path)
        {
            using (FileStream originalFileStream = File.OpenRead(path))
            {
                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    using (StreamReader streamReader = new StreamReader(decompressionStream))
                    {
                        List<string> lines = new List<string>();
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (line.Trim().Length > 0)
                            {
                                lines.Add(line);
                            }
                        }

                        return lines;
                    }
                }
            }
        }

        public static long CharCount(string path, Encoding charset)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(path), charset);
            char[]
            buf = new char[4096];
            long count = 0;
            while (true)
            {
                int k = reader.Read(buf);
                if (k == -1)
                {
                    break;
                }
                count += k;
            }
            return count;
        }

        public static string CreateTempFile(string content)
        {
            return CreateTempFile(content);
        }

        public static string CreateTempFile(params string[] content)
        {
            return CreateTempFile(content.ToList());
        }

        public static string CreateTempFile(List<string> content)
        {
            string temp = Path.Combine(Path.GetTempPath(), "tmp.tmp");
            File.Create(temp);
            File.WriteAllLines(temp, content, Encoding.UTF8);
            return temp;
        }
    }
}
