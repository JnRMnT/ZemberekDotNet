using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Embeddings;
using static ZemberekDotNet.Core.Embeddings.FastText;

namespace ZemberekDotNet.Classification
{
    public class FastTextClassifier
    {
        private readonly FastText fastText;

        public FastTextClassifier(FastText fastText)
        {
            this.fastText = fastText;
        }

        public FastText GetFastText()
        {
            return fastText;
        }

        public static FastTextClassifier Load(string modelPath)
        {
            Contract.Requires(File.Exists(modelPath), $"{modelPath} does not exist.");
            FastText fastText = FastText.Load(modelPath);
            return new FastTextClassifier(fastText);
        }

        public EvaluationResult Evaluate(string testPath, int k)
        {
            return fastText.Test(testPath, k);
        }

        public EvaluationResult Evaluate(string testPath, int k, float threshold)
        {
            return fastText.Test(testPath, k, threshold);
        }

        public List<ScoredItem<string>> Predict(string input, int k)
        {
            return fastText.Predict(input, k);
        }

        public List<ScoredItem<string>> Predict(string input, int k, float threshold)
        {
            return fastText.Predict(input, k, threshold);
        }


        public List<string> GetLabels()
        {
            return fastText.GetLabels();
        }
    }
}
