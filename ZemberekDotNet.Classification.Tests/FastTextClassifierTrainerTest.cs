using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Classification;
using ZemberekDotNet.Core;

namespace ZemberekDotNet.Classification.Tests
{
    [TestClass]
    public class FastTextClassifierTrainerTest
    {
        [TestMethod]
        public void TrainerBuilderCreatesTrainer()
        {
            FastTextClassifierTrainer trainer = FastTextClassifierTrainer.Builder()
                .Dimension(10)
                .EpochCount(1)
                .LearningRate(0.1f)
                .Build();
            Assert.IsNotNull(trainer);
        }

        [TestMethod]
        public void TrainProducesClassifier()
        {
            // Build a minimal corpus with two labels
            string corpusPath = Path.Combine(Path.GetTempPath(), "fast_text_test_corpus_" + Guid.NewGuid() + ".txt");
            try
            {
                using (StreamWriter sw = new StreamWriter(corpusPath))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        sw.WriteLine("__label__positive good great excellent wonderful");
                        sw.WriteLine("__label__negative bad terrible awful horrible");
                    }
                }

                FastTextClassifierTrainer trainer = FastTextClassifierTrainer.Builder()
                    .Dimension(10)
                    .EpochCount(5)
                    .LearningRate(0.2f)
                    .ThreadCount(1)
                    .Build();

                FastTextClassifier classifier = trainer.Train(corpusPath);

                Assert.IsNotNull(classifier);
                List<string> labels = classifier.GetLabels();
                Assert.IsTrue(labels.Count > 0, "Classifier should have at least one label.");
            }
            finally
            {
                if (File.Exists(corpusPath))
                {
                    File.Delete(corpusPath);
                }
            }
        }

        [TestMethod]
        [Ignore("Requires external model file.")]
        public void LoadFromFileProducesClassifier()
        {
            string modelPath = @"model.bin";
            FastTextClassifier classifier = FastTextClassifier.Load(modelPath);
            Assert.IsNotNull(classifier);
        }
    }
}
