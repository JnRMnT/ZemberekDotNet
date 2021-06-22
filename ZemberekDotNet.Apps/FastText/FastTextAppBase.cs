using Commander.NET.Attributes;
using System;
using System.Globalization;
using ZemberekDotNet.Core.Embeddings;

namespace ZemberekDotNet.Apps.FastText
{
    public abstract class FastTextAppBase<T> : ConsoleApp<T> where T : new()
    {
        [Parameter("--wordNGrams", "wng",
      Description = "Word N-Gram order.")]
        protected int wordNGrams = WordVectorsTrainer.DefaultWordNGram;

        [Parameter("--dimension", "dim",
      Description = "Vector dimension.")]
        protected int dimension = WordVectorsTrainer.DefaultDimension;

        [Parameter("--contextWindowSize", "ws",
       Description = "Context window size.")]
        protected int contextWindowSize = WordVectorsTrainer.DefaultContextWindowSize;

        [Parameter("--threadCount", "tc",
      Description = "Thread Count.")]
        protected int threadCount = WordVectorsTrainer.DefaultTC;

        [Parameter("--minWordCount", "minc",
      Description = "Words with lower than this count will be ignored..")]
        protected int minWordCount = WordVectorsTrainer.DefaultMinWordCount;

        public void TrainingProgress(FastTextTrainer.Progress progress)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} of {1},  lr: {2:N6}",
                progress.current,
                progress.total,
                progress.learningRate));
        }
    }
}
