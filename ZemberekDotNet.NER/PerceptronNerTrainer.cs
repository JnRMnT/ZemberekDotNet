using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Data;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Morphology;
using static ZemberekDotNet.NER.PerceptronNer;

namespace ZemberekDotNet.NER
{
    public class PerceptronNerTrainer
    {
        private TurkishMorphology morphology;

        public PerceptronNerTrainer(TurkishMorphology morphology)
        {
            this.morphology = morphology;
        }

        public PerceptronNer Train(
            NerDataSet trainingSet,
            NerDataSet devSet,
            int iterationCount,
            float learningRate)
        {
            Dictionary<string, ClassModel> averages = new Dictionary<string, ClassModel>();
            Dictionary<string, ClassModel> model = new Dictionary<string, ClassModel>();
            IntValueMap<string> counts = new IntValueMap<string>();

            //initialize model weights for all classes.
            foreach (string typeId in trainingSet.TypeIds)
            {
                model[typeId] = new ClassModel(typeId);
                averages[typeId] = new ClassModel(typeId);
            }

            for (int it = 0; it < iterationCount; it++)
            {

                int errorCount = 0;
                int tokenCount = 0;

                trainingSet.Shuffle();

                foreach (NerSentence sentence in trainingSet.Sentences)
                {

                    for (int i = 0; i < sentence.tokens.Count; i++)
                    {

                        tokenCount++;
                        NerToken currentToken = sentence.tokens[i];
                        string currentId = currentToken.tokenId;

                        FeatureData data = new FeatureData(morphology, sentence, i);
                        List<string> sparseFeatures = data.GetTextualFeatures();

                        if (i > 0)
                        {
                            sparseFeatures.Add("PreType=" + sentence.tokens[i - 1].tokenId);
                        }
                        if (i > 1)
                        {
                            sparseFeatures.Add("2PreType=" + sentence.tokens[i - 2].tokenId);
                        }
                        if (i > 2)
                        {
                            sparseFeatures.Add("3PreType=" + sentence.tokens[i - 3].tokenId);
                        }

                        ScoredItem<string> predicted = PerceptronNer
                            .PredictTypeAndPosition(model, sparseFeatures);
                        string predictedId = predicted.Item;

                        if (predictedId.Equals(currentId))
                        {
                            // do nothing
                            counts.AddOrIncrement(predictedId);
                            continue;
                        }

                        counts.AddOrIncrement(currentId);
                        counts.AddOrIncrement(predictedId);
                        errorCount++;

                        model.GetValueOrDefault(currentId).UpdateSparse(sparseFeatures, +learningRate);
                        model.GetValueOrDefault(predictedId).UpdateSparse(sparseFeatures, -learningRate);

                        averages.GetValueOrDefault(currentId)
                            .UpdateSparse(sparseFeatures, counts.Get(currentId) * learningRate);
                        averages.GetValueOrDefault(predictedId)
                            .UpdateSparse(sparseFeatures, -counts.Get(predictedId) * learningRate);
                    }
                }
                Log.Info("Iteration {0}, Token error = {1:F6}", it + 1, (errorCount * 1d) / tokenCount);

                Dictionary<string, ClassModel> copyModel = CopyModel(model);
                AverageWeights(averages, copyModel, counts);
                PerceptronNer ner = new PerceptronNer(copyModel, morphology);
                if (devSet != null)
                {
                    NerDataSet result = ner.Evaluate(devSet);
                    Log.Info(CollectEvaluationData(devSet, result).Dump());
                }
            }

            AverageWeights(averages, model, counts);

            Log.Info("Training finished.");
            return new PerceptronNer(model, morphology);
        }

        private static Dictionary<string, ClassModel> CopyModel(Dictionary<string, ClassModel> model)
        {
            Dictionary<string, ClassModel> copy = new Dictionary<string, ClassModel>();
            foreach (string s in model.Keys)
            {
                copy[s] = model.GetValueOrDefault(s).Copy();
            }
            return copy;
        }

        private static void AverageWeights(
            Dictionary<string, ClassModel> averages,
            Dictionary<string, ClassModel> model,
            IntValueMap<string> counts)
        {
            foreach (string typeId in model.Keys)
            {
                Weights w = (Weights)model.GetValueOrDefault(typeId).SparseWeights;
                Weights a = (Weights)averages.GetValueOrDefault(typeId).SparseWeights;
                foreach (string s in w)
                {
                    w.Put(s, w.Get(s) - a.Get(s) / counts.Get(typeId));
                }
            }
        }

        public class TestResult
        {
            private int errorCount = 0;
            private int tokenCount = 0;
            private int truePositives = 0;
            private int falsePositives = 0;
            private int falseNegatives = 0;
            private int testNamedEntityCount = 0;
            private int correctNamedEntityCount = 0;

            public int ErrorCount { get => errorCount; set => errorCount = value; }
            public int TokenCount { get => tokenCount; set => tokenCount = value; }
            public int TruePositives { get => truePositives; set => truePositives = value; }
            public int FalsePositives { get => falsePositives; set => falsePositives = value; }
            public int FalseNegatives { get => falseNegatives; set => falseNegatives = value; }
            public int TestNamedEntityCount { get => testNamedEntityCount; set => testNamedEntityCount = value; }
            public int CorrectNamedEntityCount { get => correctNamedEntityCount; set => correctNamedEntityCount = value; }

            double TokenErrorRatio()
            {
                return (ErrorCount * 1d) / TokenCount;
            }

            double TokenPrecision()
            {
                return (TruePositives * 1d) / (TruePositives + FalsePositives);
            }

            //TODO: check this
            double TokenRecall()
            {
                return (TruePositives * 1d) / (TruePositives + FalseNegatives);
            }

            double ExactMatch()
            {
                return (CorrectNamedEntityCount * 1d) / TestNamedEntityCount;
            }

            public string Dump()
            {
                List<string> lines = new List<string>();
                lines.Add(string.Format("Token Error ratio   = {0:F6}", TokenErrorRatio()));
                lines.Add(string.Format("NE Token Precision  = {0:F6}", TokenPrecision()));
                lines.Add(string.Format("NE Token Recall     = {0:F6}", TokenRecall()));
                lines.Add(string.Format("Exact NE match      = {0:F6}", ExactMatch()));
                return string.Join("\n", lines);
            }

        }

        public static void EvaluationReport(
            NerDataSet reference,
            NerDataSet prediction,
            string reportPath)
        {
            using (FileStream fileStream = File.OpenWrite(reportPath))
            {
                using (StreamWriter pw = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    pw.WriteLine("Evaluation Data Information:");
                    pw.WriteLine(reference.Info());

                    TestResult result = CollectEvaluationData(reference, prediction);
                    pw.WriteLine("Summary:");
                    pw.WriteLine(result.Dump());
                    pw.WriteLine();
                    pw.WriteLine("Detailed Sentence Analysis:");

                    List<NerSentence> testSentences = reference.Sentences;
                    for (int i = 0; i < testSentences.Count; i++)
                    {
                        NerSentence ts = testSentences[i];
                        pw.WriteLine(ts.content);
                        NerSentence ps = prediction.Sentences[i];
                        for (int j = 0; j < ts.tokens.Count; j++)
                        {
                            NerToken tt = ts.tokens[j];
                            NerToken pt = ps.tokens[j];
                            if (tt.word.Equals(tt.normalized))
                            {
                                pw.WriteLine(string.Format("{0} {1} -> {2}", tt.word, tt.tokenId, pt.tokenId));
                            }
                            else
                            {
                                pw.WriteLine(
                                    string.Format("{0}:{1} {2} -> {3}", tt.word, tt.normalized, tt.tokenId, pt.tokenId));
                            }
                        }
                        pw.WriteLine();
                    }
                }
            }
        }

        public static TestResult CollectEvaluationData(NerDataSet reference, NerDataSet prediction)
        {
            int errorCount = 0;
            int tokenCount = 0;
            int truePositives = 0;
            int falsePositives = 0;
            int falseNegatives = 0;
            int testNamedEntityCount = 0;
            int correctNamedEntityCount = 0;

            List<NerSentence> testSentences = reference.Sentences;
            for (int i = 0; i < testSentences.Count; i++)
            {
                NerSentence ts = testSentences[i];
                NerSentence ps = prediction.Sentences[i];
                for (int j = 0; j < ts.tokens.Count; j++)
                {
                    NerToken tt = ts.tokens[j];
                    NerToken pt = ps.tokens[j];
                    if (!tt.tokenId.Equals(pt.tokenId))
                    {
                        errorCount++;
                        if (tt.position == NePosition.OUTSIDE)
                        {
                            falsePositives++;
                        }
                        if (pt.position == NePosition.OUTSIDE)
                        {
                            falseNegatives++;
                        }
                    }
                    else
                    {
                        if (tt.position != NePosition.OUTSIDE)
                        {
                            truePositives++;
                        }
                    }
                    tokenCount++;
                }
                List<NamedEntity> namedEntities = ts.GetNamedEntities();
                testNamedEntityCount += namedEntities.Count;
                correctNamedEntityCount += ps.MatchingNEs(namedEntities).Count;
            }

            TestResult result = new TestResult();
            result.CorrectNamedEntityCount = correctNamedEntityCount;
            result.ErrorCount = errorCount;
            result.FalseNegatives = falseNegatives;
            result.FalsePositives = falsePositives;
            result.TestNamedEntityCount = testNamedEntityCount;
            result.TokenCount = tokenCount;
            result.TruePositives = truePositives;
            return result;
        }
    }
}
