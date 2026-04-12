using Commander.NET.Attributes;
using System;
using System.IO;
using ZemberekDotNet.NER;

namespace ZemberekDotNet.Apps.NER
{
    public class TrainNerModel : NerAppBase<TrainNerModel>
    {
        [Parameter("--trainData", "-t", Required = Required.Yes,
            Description = "Annotated training data file path.")]
        string trainDataPath;

        [Parameter("--devData", "-d", Required = Required.No,
            Description = "Annotated development data file path. Optional.")]
        string developmentPath;

        [Parameter("--output", "-o", Required = Required.Yes,
            Description = "Output directory path for generated NER models.")]
        string outputRoot;

        [Parameter("--iterationCount", "-n",
            Description = "Training iteration count. Default is 7.")]
        int iterationCount = 7;

        [Parameter("--learningRate", "-l",
            Description = "Learning rate. Default is 0.1")]
        float learningRate = 0.1f;

        public override string Description()
        {
            return "Generates Turkish Named Entity Recognition model. There will be two model sets in "
                + "the output directory, one is text models (in [model] directory), "
                + "other is compressed lossy model (in [model-compressed] directory).";
        }

        public override void Run()
        {
            if (!File.Exists(trainDataPath))
            {
                throw new FileNotFoundException("Training data file not found: " + trainDataPath);
            }

            NerDataSet.AnnotationStyle style = GetAnnotationStyle();
            NerDataSet trainingSet = NerDataSet.Load(trainDataPath, style);
            NerDataSet devSet = null;

            if (!string.IsNullOrWhiteSpace(developmentPath))
            {
                if (!File.Exists(developmentPath))
                {
                    throw new FileNotFoundException("Development data file not found: " + developmentPath);
                }
                devSet = NerDataSet.Load(developmentPath, style);
            }

            Directory.CreateDirectory(outputRoot);
            string textModelRoot = Path.Combine(outputRoot, "model");
            string compressedModelRoot = Path.Combine(outputRoot, "model-compressed");
            Directory.CreateDirectory(textModelRoot);
            Directory.CreateDirectory(compressedModelRoot);

            PerceptronNerTrainer trainer = new PerceptronNerTrainer(CreateMorphology());
            PerceptronNer ner = trainer.Train(trainingSet, devSet, iterationCount, learningRate);

            ner.SaveModelAsText(textModelRoot);
            ner.SaveModelCompressed(compressedModelRoot);

            Console.WriteLine("NER training completed.");
            Console.WriteLine("Text model root       : " + textModelRoot);
            Console.WriteLine("Compressed model root : " + compressedModelRoot);
        }

        public static void Main(string[] args)
        {
            new TrainNerModel().Execute(args);
        }
    }
}