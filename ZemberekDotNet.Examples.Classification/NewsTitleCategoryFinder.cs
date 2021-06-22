using log4net.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Apps.FastText;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Examples.Classification
{
    public class NewsTitleCategoryFinder : ClassificationExampleBase
    {
        public static readonly int TestSize = 1000;

        public static void Main(string[] args)
        {
            NewsTitleCategoryFinder experiment = new NewsTitleCategoryFinder();
            // Download data set `news-title-category-set`
            // from https://drive.google.com/drive/folders/1JBPExAeRctAXL2oGW2U6CbqfwIJ84BG7
            // and change the line below.
            string set = @"C:\Users\ozank\Desktop\ZemberekDotNet\data\classification\news-title-category-set";

            morphology = TurkishMorphology.Builder()
            .SetLexicon(RootLexicon.GetDefault())
            .Build();

            DirectoryInfo root = Directory.GetParent(set);
            if (root == null || !root.Exists)
            {
                //TODO:Check the original intention
                root = new DirectoryInfo("");
            }
            List<string> lines = File.ReadAllLines(set, Encoding.UTF8).ToList();
            string name = Path.GetFileName(set);
            experiment.DataInfo(lines);
            Log.Info("------------ Evaluation with raw data ------------------");
            experiment.Evaluate(set, TestSize);

            string tokenizedPath = Path.Combine(root.FullName, name + ".tokenized");
            Log.Info("------------ Evaluation with tokenized - lowercase data ------------");
            experiment.GenerateSetTokenized(lines, tokenizedPath);
            experiment.Evaluate(tokenizedPath, TestSize);

            string lemmasPath = Path.Combine(root.FullName, name + ".lemmas");
            Log.Info("------------ Evaluation with lemma - lowercase data ------------");
            if (!File.Exists(lemmasPath))
            {
                experiment.GenerateSetWithLemmas(lines, lemmasPath);
            }
            experiment.Evaluate(lemmasPath, TestSize);

            string splitPath = Path.Combine(root.FullName, name + ".split");
            Log.Info("------------ Evaluation with Stem-Ending - lowercase data ------------");
            if (!File.Exists(splitPath))
            {
                experiment.GenerateSetWithSplit(lines, splitPath);
            }
            experiment.Evaluate(splitPath, TestSize);

        }

        private void Evaluate(string set, int testSize)
        {

            // Create training and test sets.
            List<string> lines = File.ReadAllLines(set, Encoding.UTF8).ToList();
            DirectoryInfo root = Directory.GetParent(set);
            if (root == null)
            {
                //TODO:Check the original intention
                root = new DirectoryInfo("");
            }
            string name = Path.GetFileName(set);

            string train = Path.Combine(root.FullName, name + ".train");
            string testPath = Path.Combine(root.FullName, name + ".test");

            File.WriteAllLines(train, lines.Skip(testSize).Take(lines.Count - testSize));
            File.WriteAllLines(testPath, lines.Take(testSize));

            //Create model if it does not exist.
            string modelPath = Path.Combine(root.FullName, name + ".model");
            if (!File.Exists(modelPath))
            {
                new TrainClassifier().Execute(
                    "-i", train,
                    "-o", modelPath,
                    "--learningRate", "0.1",
                    "--epochCount", "70",
                    "--dimension", "100",
                    "--wordNGrams", "2"/*,
          "--applyQuantization",
          "--cutOff", "25000"*/
                );
            }
            Log.Info("Testing...");
            Test(testPath, Path.Combine(root.FullName, name + ".predictions"), modelPath);
            // test quantized models.
            /*    Log.info("Testing with quantized model...");
                test(testPath, root.resolve(name + ".predictions.q"), root.resolve(name + ".model.q"));*/
        }

        private void Test(string testPath, string predictionsPath, string modelPath)
        {
            new EvaluateClassifier().Execute(
                "-i", testPath,
                "-m", modelPath,
                "-o", predictionsPath,
                "-k", "1"
            );
        }

        protected void DataInfo(List<string> lines)
        {
            Log.Info("Total lines = " + lines.Count);
            Histogram<string> hist = new Histogram<string>();
            lines.Select(s => s.Substring(0, s.IndexOf(' '))).ToList()
                .ForEach(e => hist.Add(e));
            Log.Info("Categories :");
            foreach (string s in hist.GetSortedList())
            {
                Log.Info(s + " " + hist.GetCount(s));
            }
        }
    }
}
