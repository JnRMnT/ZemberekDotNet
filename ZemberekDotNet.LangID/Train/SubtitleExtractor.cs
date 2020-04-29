using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using ZemberekDotNet.Core.Native.Collections;

namespace ZemberekDotNet.LangID.Train
{
    public class SubtitleExtractor
    {
        /// <summary>
        /// Question mark? :)
        /// </summary>
        /// <param name="args"></param>
        public static void DummyMain(string[] args)
        {
            new SubtitleExtractor().GenerateSets("/home/kodlab/Downloads/OpenSubtitles/OpenSubtitles",
                "/home/kodlab/data/language-data/subtitle");
        }

        public List<string> ExtractFromStream(Stream inputStream, int minLength)
        {
            List<string> lines = new List<string>();
            using (XmlReader reader = XmlReader.Create(inputStream))
            {
                List<string> line = new List<string>();
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        string elName = reader.Name;
                        if (elName.Equals("s"))
                        {
                            string sentence = string.Join(" ", line)
                                .Replace("[\"\\(\\)+-]", "")
                                .Replace("([*]|[#]|[{]).+?([*]|[#]|[}])", " ")
                                .Replace("'[ ]+", "'")
                                .Replace("[.]+", ".")
                                .Replace("( )([,.?!])", "$2").Trim();
                            if (sentence.Length >= minLength)
                            {
                                lines.Add(sentence);
                            }
                            line = new List<string>();
                        }
                        if (elName.Equals("w"))
                        {
                            line.Add(reader.ReadContentAsString().Trim());
                        }
                    }
                }
                return lines;
            }
        }

        public void GenerateSets(string root, string outDir)
        {
            string[] dirs = Directory.GetDirectories(root);
            foreach (string dir in dirs)
            {
                ExtractDir(dir, outDir);
            }
        }

        private void ExtractDir(string dir, string outDir)
        {
            string name = Path.GetDirectoryName(dir).ToLowerInvariant();
            LinkedHashSet<string> all = new LinkedHashSet<string>();
            string[] gzFiles = Directory.GetFiles(dir).Where(e => e.EndsWith(".gz")).ToArray();
            foreach (string gzFile in gzFiles)
            {
                using (FileStream fileStream = File.OpenRead(gzFile))
                {
                    using (GZipStream gis = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        List<string> lines = ExtractFromStream(gis, 7);
                        all.AddRange(lines);
                    }
                }
            }
            List<string> allAsList = new List<string>(all);
            allAsList.Shuffle();
            List<String> train = allAsList.GetRange(1000, all.Count - 1);
            List<String> test = allAsList.GetRange(0, 1000);

            Console.WriteLine("Lang:" + name + " Train:" + train.Count + " Test:" + test.Count);
            File.WriteAllLines(Path.Combine(outDir, name + "-train"), train);
            File.WriteAllLines(Path.Combine(outDir, name + "-test"), test);
        }
    }
}
