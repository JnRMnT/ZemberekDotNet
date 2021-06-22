using Commander.NET.Attributes;
using System;
using System.IO;
using ZemberekDotNet.Classification;
using ZemberekDotNet.Core.Logging;
using static ZemberekDotNet.Classification.FastTextClassifierTrainer;

namespace ZemberekDotNet.Apps.FastText
{
    public class TrainClassifier : FastTextAppBase<TrainClassifier>
    {
        [Parameter("--input", "-i",
      Required = Required.Yes,
      Description = "Classifier training file. each line should contain a single document and "
          + "one or more class labels. "
          + "Document class label needs to have __label__ prefix attached to it.")]
        string input;

        [Parameter("--output", "-o",
      Required = Required.Yes,
      Description = "Output model file.")]
        string output;

        [Parameter("--lossType", "-l",
      Description = "Model type.")]
        LossType lossType = LossType.Softmax;

        [Parameter("--applyQuantization", "-q",
      Description = "If used, applies quantization to model. This way model files will be "
          + " smaller. Underlying algorithm uses 8 bit values for weights instead of 32 bit floats."
          + " Quantized model will be saved same place of output with name [output].q ")]
        bool applyQuantization = false;

        [Parameter("--cutOff", "-c",
      Description = "Reduces dictionary size with given threshold value. "
          + "Dictionary entries are sorted with l2-norm values and top `cutOff` are selected. "
          + "This greatly reduces model size. This option is only available if"
          + "applyQuantization flag is used.")]
        int cutOff = -1;

        [Parameter("--epochCount", "ec",
      Description = "Epoch Count.")]
        int epochCount = FastTextClassifierTrainer.DefaultEpoch;

        [Parameter("--learningRate", "lr",
      Description = "Learning rate. Should be between 0.01-2.0")]
        float learningRate = FastTextClassifierTrainer.DefaultLR;

        public override string Description()
        {
            return "Generates a text classification model from a training set. Classification algorithm"
                + " is based on Java port of fastText library. It is usually suggested to apply "
                + "tokenization, lower-casing and other specific text operations to the training set"
                + " before training the model. "
                + "Algorithm may be more suitable for sentence and short paragraph"
                + " level texts rather than long documents.\n "
                + "In the training set, each line should contain a single document. Document class "
                + "label needs to have __label__ prefix attached to it. Such as "
                + "[__label__sports Match ended in a draw.]\n"
                + "Each line (document) may contain more than one label.\n"
                + "If there are a lot of labels, LossType can be chosen `HIERARCHICAL_SOFTMAX`. "
                + "This way training and runtime speed will be faster with a small accuracy loss.\n "
                + "For generating compact models, use -applyQuantization and -cutOff [dictionary-cut-off] "
                + "parameters.";
        }

        public override void Run()
        {
            Log.Info("Generating classification model from {0}", input);

            FastTextClassifierTrainer trainer = FastTextClassifierTrainer.Builder()
            .EpochCount(epochCount)
            .LearningRate(learningRate)
            .LossType(lossType)
            .QuantizationCutOff(cutOff)
            .MinWordCount(minWordCount)
            .ThreadCount(threadCount)
            .WordNgramOrder(wordNGrams)
            .Dimension(dimension)
            .ContextWindowSize(contextWindowSize)
            .Build();

            trainer.OnProgress += TrainingProgress;

            FastTextClassifier classifier = trainer.Train(input);

            Log.Info("Saving classification model to {0}", output);
            Core.Embeddings.FastText fastText = classifier.GetFastText();
            fastText.SaveModel(output);

            if (applyQuantization)
            {
                Log.Info("Applying quantization.");
                if (cutOff > 0)
                {
                    Log.Info("Quantization dictionary cut-off value = {0}", cutOff);
                }
                DirectoryInfo parent = Directory.GetParent(output);
                string name = Path.GetFileName(output) + ".q";
                string quantizedModel = parent == null ? name : Path.GetFullPath(name, parent.FullName);
                Log.Info("Saving quantized classification model to {0}", quantizedModel);
                Core.Embeddings.FastText quantized = fastText.Quantize(output, fastText.GetArgs());
                quantized.SaveModel(quantizedModel);
            }
        }

        public static void Main(string[] args)
        {
            new TrainClassifier().Execute(args);
        }
    }
}
