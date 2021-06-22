using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZemberekDotNet.Classification;
using ZemberekDotNet.Core;
using static ZemberekDotNet.Core.Embeddings.FastText;

namespace ZemberekDotNet.Apps.FastText
{
    public class EvaluateClassifier : ConsoleApp<EvaluateClassifier>
    {
        [Parameter("--input", "-i",
      Required = Required.Yes,
      Description = "Input test set with correct labels.")]
        string input;

        [Parameter("--model", "-m",
      Required = Required.Yes,
      Description = "Model file.")]
        string model;

        [Parameter("--output", "-o",
      Description = "Output file where prediction results will be written."
          + " If not provided, [input].predictions will be generated.")]
        string predictions;

        [Parameter("--maxPrediction", "-k",
      Description = "Amount of top predictions. "
          + "Predictions with lower than --threshold values will not be included.")]
        int maxPrediction = 1;

        [Parameter("--threshold", "th",
      Description = "Minimum score threshold. Lower values will not be included in predictions.")]
        float threshold = -100f;

        public override string Description()
        {
            return "Evaluates classifier with a test set.";
        }

        public override void Run()
        {

            Console.WriteLine("Loading classification model...");

            FastTextClassifier classifier = FastTextClassifier.Load(model);
            EvaluationResult result = classifier.Evaluate(input, maxPrediction, threshold);

            Console.WriteLine("Result = " + result.ToString());

            if (predictions == null)
            {
                string name = Path.GetFileName(input);
                predictions = Path.GetFullPath(name + ".predictions");
            }

            string[] testLines = File.ReadAllLines(input, Encoding.UTF8);
            try
            {
                using (FileStream fs = File.OpenWrite(predictions))
                {
                    using (StreamWriter pw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        foreach (string testLine in testLines)
                        {
                            List<ScoredItem<String>> res = classifier.Predict(testLine, maxPrediction);

                            res = res.Where(s => s.Score >= threshold).ToList();

                            List<String> predictedCategories = new List<string>();
                            foreach (ScoredItem<string> re in res)
                            {
                                predictedCategories.Add(string.Format(CultureInfo.InvariantCulture, "{0} ({1:N6})",
                                    re.Item.Replace("__label__", ""),
                                    Math.Exp(re.Score)));
                            }
                            pw.WriteLine(testLine);
                            pw.WriteLine("Predictions   = " + String.Join(", ", predictedCategories));
                            pw.WriteLine();
                        }
                    }
                }
            }
            catch
            {
            }

            Console.WriteLine("Predictions are written to " + predictions);
        }
    }
}
