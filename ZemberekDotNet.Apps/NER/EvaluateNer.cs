using Commander.NET.Attributes;
using System;
using System.IO;
using ZemberekDotNet.NER;

namespace ZemberekDotNet.Apps.NER
{
    public class EvaluateNer : NerAppBase<EvaluateNer>
    {
        [Parameter("--reference", "-r", Required = Required.Yes,
            Description = "Reference annotated NER data file path.")]
        string referencePath;

        [Parameter("--modelRoot", "-m", Required = Required.No,
            Description = "Model root path. Required if hypothesis is not supplied.")]
        string modelRoot;

        [Parameter("--hypothesis", "-y", Required = Required.No,
            Description = "Optional hypothesis annotated NER file path. If provided, modelRoot is not required.")]
        string hypothesisPath;

        [Parameter("--outputDir", "-o",
            Description = "Output directory for detailed evaluation report.")]
        string outputDir = "ner-eval-output";

        public override string Description()
        {
            return "Evaluates an annotated NER data set by either running NER with a given model "
                + "or against an already generated hypothesis data.";
        }

        public override void Run()
        {
            if (!File.Exists(referencePath))
            {
                throw new FileNotFoundException("Reference file not found: " + referencePath);
            }

            NerDataSet.AnnotationStyle style = GetAnnotationStyle();
            NerDataSet reference = NerDataSet.Load(referencePath, style);

            NerDataSet hypothesis;
            if (string.IsNullOrWhiteSpace(hypothesisPath))
            {
                if (string.IsNullOrWhiteSpace(modelRoot) || !Directory.Exists(modelRoot))
                {
                    throw new DirectoryNotFoundException("Model root is required and must exist when hypothesis is not provided.");
                }

                PerceptronNer ner = PerceptronNer.LoadModel(modelRoot, CreateMorphology());
                hypothesis = ner.Evaluate(reference);
            }
            else
            {
                if (!File.Exists(hypothesisPath))
                {
                    throw new FileNotFoundException("Hypothesis file not found: " + hypothesisPath);
                }

                hypothesis = NerDataSet.Load(hypothesisPath, style);
            }

            Directory.CreateDirectory(outputDir);
            string reportPath = Path.Combine(outputDir, "eval-report.txt");
            PerceptronNerTrainer.EvaluationReport(reference, hypothesis, reportPath);

            PerceptronNerTrainer.TestResult result =
                PerceptronNerTrainer.CollectEvaluationData(reference, hypothesis);

            Console.WriteLine("Reference :");
            Console.WriteLine(reference.Info());
            Console.WriteLine();
            Console.WriteLine("Hypothesis :");
            Console.WriteLine(hypothesis.Info());
            Console.WriteLine();
            Console.WriteLine("Evaluation Result:");
            Console.WriteLine(result.Dump());
            Console.WriteLine();
            Console.WriteLine("Detailed evaluation report is written to " + reportPath);
        }

        public static void Main(string[] args)
        {
            new EvaluateNer().Execute(args);
        }
    }
}