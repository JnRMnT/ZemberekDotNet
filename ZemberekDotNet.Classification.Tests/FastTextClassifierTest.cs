using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Classification;
using ZemberekDotNet.Core;

namespace ZemberekDotNet.Classification.Tests
{
    [TestClass]
    public class FastTextClassifierTest
    {
        private static FastTextClassifier classifier;
        private static string corpusPath;
        private static string modelPath;

        [ClassInitialize]
        public static void SetUpClassifier(TestContext context)
        {
            corpusPath = Path.Combine(Path.GetTempPath(), "ft_test_corpus_" + Guid.NewGuid() + ".txt");
            modelPath = Path.Combine(Path.GetTempPath(), "ft_test_model_" + Guid.NewGuid());

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

            classifier = trainer.Train(corpusPath);
        }

        [ClassCleanup]
        public static void TearDownClassifier()
        {
            if (File.Exists(corpusPath)) File.Delete(corpusPath);
            if (File.Exists(modelPath)) File.Delete(modelPath);
        }

        [TestMethod]
        public void GetLabelsReturnsNonEmptyList()
        {
            List<string> labels = classifier.GetLabels();
            Assert.IsNotNull(labels);
            Assert.IsTrue(labels.Count > 0, "Labels list should not be empty.");
        }

        [TestMethod]
        public void PredictReturnsResults()
        {
            List<ScoredItem<string>> results = classifier.Predict("good great", 2);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0, "Predict should return at least one result.");
        }

        [TestMethod]
        public void PredictWithThresholdReturnsResults()
        {
            List<ScoredItem<string>> results = classifier.Predict("good great", 2, 0.0f);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0, "Predict with threshold should return at least one result.");
        }

        [TestMethod]
        public void PredictTopResultHasPositiveLabel()
        {
            List<ScoredItem<string>> results = classifier.Predict("good great", 1);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("__label__positive", results[0].Item);
        }

        [TestMethod]
        public void GetFastTextReturnsNonNull()
        {
            Assert.IsNotNull(classifier.GetFastText());
        }

        [TestMethod]
        [Ignore("Requires external test file.")]
        public void EvaluateRequiresTestFile()
        {
            string testFile = @"test.txt";
            var result = classifier.Evaluate(testFile, 1);
            Assert.IsNotNull(result);
        }
    }
}
