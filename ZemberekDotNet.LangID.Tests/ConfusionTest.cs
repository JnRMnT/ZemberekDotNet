using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.LangID.Tests
{
    public class ConfusionTest
    {
        LanguageIdentifier identifier;
        public ConfusionTest(LanguageIdentifier identifier)
        {
            this.identifier = identifier;
        }

        public static void DummyMain()
        {
            Stopwatch sw = Stopwatch.StartNew();
            //        LanguageIdentifier identifier = LanguageIdentifier.fromCompressedModelsDir(new File("/home/kodlab/data/language-data/models/compressed"));
            string[] langs = { "tr", "en" };
            //string[]  langs = {"tr", "ar", "az", "hy", "bg", "en", "el", "ka", "ku", "fa", "de","fr","nl","diq"};
            // string[] langs = Language.allLanguages();
            //LanguageIdentifier identifier = LanguageIdentifier.generateFromCounts(new File("/home/kodlab/data/language-data/models/counts3"),Language.allLanguages());
            LanguageIdentifier identifier = LanguageIdentifier.FromInternalModelGroup("tr_group");
            //LanguageIdentifier identifier = LanguageIdentifier.fromInternalModelGroup("tr_group", langs);
            //LanguageIdentifier identifier = LanguageIdentifier.generateFromCounts(Language.allLanguages());
            Console.WriteLine("Model generation: " + sw.ElapsedMilliseconds);
            ConfusionTest confusionTest = new ConfusionTest(identifier);
            //confusionTest.testAll();
            confusionTest.TestContains();
            // confusionTest.testOher();
        }

        public void TestAll()
        {

            int sliceLength = 1000;
            int maxSliceCount = 1000;
            List<TestSet> sets = AllSets(maxSliceCount, sliceLength);
            ISet<string> languages = identifier.GetLanguages();
            foreach (string language in languages)
            {
                Console.WriteLine(language);
                Stopwatch sw = Stopwatch.StartNew();
                int falsePositives = 0;
                int totalCount = 0;
                int correctlyFound = 0;
                int correctAmount = 0;
                foreach (TestSet set in sets)
                {
                    /*                if(!set.modelId.equals("tr"))
                                        continue;*/
                    totalCount += set.Size();
                    Histogram<string> result = new Histogram<string>();
                    foreach (string s in set.testPieces)
                    {
                        /*
                                            LanguageIdentifier.IdResult idResult = identifier.identifyFullConf(s);
                                            result.add(idResult.id);
                        */
                        string t = identifier.Identify(s);
                        if (set.modelId.Equals(language) && !t.Equals(language))
                        {
                            /* if (identifier.containsLanguage(s, "tr", 100, -1))
                                 Console.WriteLine("Has tr slice!");
                             Console.WriteLine(t + " " + s);*/
                        }
                        result.Add(t);
                        //result.add(identifier.identifyWithSampling(s,sliceLength));
                        //result.add(identifier.identifyWithSampling(s, 4));
                    }
                    if (set.modelId.Equals(language))
                    {
                        Console.WriteLine("Lang test size:" + set.Size());
                        correctlyFound = result.GetCount(language);
                        correctAmount = set.Size();
                        List<string> sorted = result.GetSortedList();
                        foreach (string s in sorted)
                        {
                            Console.WriteLine(s + " : " + result.GetCount(s));
                        }
                        continue;
                    }
                    else
                    {
                        int fpcount = result.GetCount(language);
                        if (fpcount > 0)
                        {
                            Console.WriteLine(set.modelId + " " + fpcount);
                        }
                    }
                    falsePositives += result.GetCount(language);
                }
                double elapsed = sw.ElapsedMilliseconds;
                Console.WriteLine(string.Format("Id per second: {0:F2}", (1000d * totalCount / elapsed)));
                Console.WriteLine("False positive count: " + falsePositives);
                Console.WriteLine("All: " + totalCount);
                Console.WriteLine(string.Format("Precision:{0:F2} ", (100d * correctlyFound / correctAmount)));
                Console.WriteLine(
                    string.Format("Recall: {0:F2}", (100d * (totalCount - falsePositives) / totalCount)));
            }
        }

        public void TestContains()
        {
            int sliceLength = 1000;
            int maxSliceCount = 1000;
            List<TestSet> sets = AllSets(maxSliceCount, sliceLength);
            ISet<string> languages = identifier.GetLanguages();
            foreach (string language in languages)
            {
                Console.WriteLine(language);
                Stopwatch sw = Stopwatch.StartNew();
                int falsePositives = 0;
                int totalCount = 0;
                int correctlyFound = 0;
                int correctAmount = 0;
                foreach (TestSet set in sets)
                {
                    /*                if(!set.modelId.equals("tr"))
                                        continue;*/
                    totalCount += set.Size();
                    Histogram<string> result = new Histogram<string>();
                    foreach (string s in set.testPieces)
                    {
                        /*
                                            LanguageIdentifier.IdResult idResult = identifier.identifyFullConf(s);
                                            result.add(idResult.id);
                        */
                        //string t = identifier.identify(s, 100);
                        //string t = identifier.identify(s);
                        string t = "tr";

                        identifier.ContainsLanguage(s, "tr", 100, -1);

                        if (set.modelId.Equals(language) && !t.Equals(language))
                        {
                            /* if (identifier.containsLanguage(s, "tr", 100, -1))
                                 Console.WriteLine("Has tr slice!");
                             Console.WriteLine(t + " " + s);*/
                        }
                        result.Add(t);
                        //result.add(identifier.identifyWithSampling(s,sliceLength));
                        //result.add(identifier.identifyWithSampling(s, 4));
                    }
                    if (set.modelId.Equals(language))
                    {
                        Console.WriteLine("Lang test size:" + set.Size());
                        correctlyFound = result.GetCount(language);
                        correctAmount = set.Size();
                        List<string> sorted = result.GetSortedList();
                        foreach (string s in sorted)
                        {
                            Console.WriteLine(s + " : " + result.GetCount(s));
                        }
                        continue;
                    }
                    else
                    {
                        int fpcount = result.GetCount(language);
                        if (fpcount > 0)
                        {
                            Console.WriteLine(set.modelId + " " + fpcount);
                        }
                    }
                    falsePositives += result.GetCount(language);
                }
                double elapsed = sw.ElapsedMilliseconds;
                Console.WriteLine(string.Format("Id per second: {0:F2}", (1000d * totalCount / elapsed)));
                Console.WriteLine("False positive count: " + falsePositives);
                Console.WriteLine("All: " + totalCount);
                Console.WriteLine(string.Format("Precision:{0:F2} ", (100d * correctlyFound / correctAmount)));
                Console.WriteLine(
                    string.Format("Recall: {0:F2}", (100d * (totalCount - falsePositives) / totalCount)));
            }
        }

        public List<string> Slice(string chunk, int sliceCount, int sliceSize)
        {
            int point;
            List<string> teststrings = new List<string>();
            for (int i = 0; i < sliceCount; i++)
            {
                point = i * sliceSize;
                if (point + sliceSize > chunk.Length)
                {
                    break;
                }
                string s = chunk.Substring(point, sliceSize);
                teststrings.Add(s);
            }
            return teststrings;
        }

        List<TestSet> AllSets(int maxSliceCount, int sliceLength)
        {
            List<string> files = Directory.GetFiles("/home/kodlab/data/language-data/subtitle", "*", SearchOption.AllDirectories).ToList();
            files.AddRange(Directory.GetFiles("/home/kodlab/data/language-data/wiki", "*", SearchOption.AllDirectories));
            Dictionary<string, TestSet> testSets = new Dictionary<string, TestSet>();
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (fileName.Contains("test"))
                {
                    Console.WriteLine(file);
                    string langStr = fileName.Substring(0, fileName.IndexOf("-"));
                    string chunk = SimpleTextReader.TrimmingUTF8Reader(file).AsString();
                    chunk = Strings.WhiteSpacesToSingleSpace(chunk);
                    List<string> test = Slice(chunk, maxSliceCount, sliceLength);
                    //Console.WriteLine(langStr);
                    if (testSets.ContainsKey(langStr))
                    {
                        testSets.GetValueOrDefault(langStr).testPieces.AddRange(test);
                    }
                    else
                    {
                        testSets.Add(langStr, new TestSet(langStr, test));
                    }
                }
            }
            foreach (TestSet testSet in testSets.Values)
            {
                if (testSet.testPieces.Count > maxSliceCount)
                {
                    testSet.testPieces = testSet.testPieces.GetRange(0, maxSliceCount);
                }
            }
            return new List<TestSet>(testSets.Values);
        }

        internal class TestSet
        {
            internal string modelId;
            internal List<string> testPieces;

            internal TestSet(string modelId, List<string> testPieces)
            {
                this.modelId = modelId;
                this.testPieces = testPieces;
            }
            internal int Size()
            {
                return testPieces.Count;
            }
        }
    }
}