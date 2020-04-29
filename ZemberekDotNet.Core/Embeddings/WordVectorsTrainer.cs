using System;
using ZemberekDotNet.Core.Concurrency;
using static ZemberekDotNet.Core.Embeddings.FastTextTrainer;

namespace ZemberekDotNet.Core.Embeddings
{
    /// <summary>
    /// With this class word vectors can be trained from a text corpus.
    /// </summary>
    public class WordVectorsTrainer
    {
        public static readonly int DefaultDimension = 50;
        public static readonly float DefaultLR = 0.05f;
        public static readonly int DefaultEpoch = 5;
        public static readonly int DefaultMinWordCount = 1;
        public static readonly int DefaultWordNGram = 1;
        public static readonly int DefaultContextWindowSize = 5;
        public static readonly int DefaultTC = Environment.ProcessorCount / 2;
        private WordVectorsTrainerBuilder builder;
        public event OnProgressDelegate OnProgress;

        WordVectorsTrainer(WordVectorsTrainerBuilder builder)
        {
            this.builder = builder;
        }

        public static WordVectorsTrainerBuilder Builder()
        {
            return new WordVectorsTrainerBuilder();
        }

        public enum ModelType
        {
            SkipGram, CBOW
        }

        public class WordVectorsTrainerBuilder
        {
            internal ModelType type = WordVectorsTrainer.ModelType.SkipGram;
            internal int wordNgramOrder = DefaultWordNGram;
            internal ISubWordHashProvider subWordHashProvider =
                new EmbeddingHashProviders.EmptySubwordHashProvider();
            internal float learningRate = DefaultLR;
            internal int threadCount = DefaultTC;
            internal int contextWindowSize = DefaultContextWindowSize;
            internal int epochCount = DefaultEpoch;
            internal int dimension = DefaultDimension;
            internal int minWordCount = DefaultMinWordCount;

            public WordVectorsTrainerBuilder ModelType(ModelType type)
            {
                this.type = type;
                return this;
            }

            public WordVectorsTrainerBuilder WordNgramOrder(int order)
            {
                this.wordNgramOrder = order;
                return this;
            }

            public WordVectorsTrainerBuilder LearningRate(float lr)
            {
                this.learningRate = lr;
                return this;
            }

            public WordVectorsTrainerBuilder MinWordCount(int minWordCount)
            {
                this.minWordCount = minWordCount;
                return this;
            }

            public WordVectorsTrainerBuilder ThreadCount(int threadCount)
            {
                ConcurrencyUtil.ValidateCpuThreadCount(threadCount);
                return this;
            }

            public WordVectorsTrainerBuilder EpochCount(int epochCount)
            {
                this.epochCount = epochCount;
                return this;
            }

            public WordVectorsTrainerBuilder Dimension(int dimension)
            {
                this.dimension = dimension;
                return this;
            }

            public WordVectorsTrainerBuilder ContextWindowSize(int contextWindowSize)
            {
                this.contextWindowSize = contextWindowSize;
                return this;
            }

            public WordVectorsTrainer Build()
            {
                return new WordVectorsTrainer(this);
            }
        }

        public FastText Train(string corpus)
        {
            Args.ModelName m = builder.type == ModelType.SkipGram ?
                Args.ModelName.SkipGram : Args.ModelName.Cbow;
            Args args = Args.ForWordVectors(m);
            args.dim = builder.dimension;
            args.wordNgrams = builder.wordNgramOrder;
            args.thread = builder.threadCount;
            args.epoch = builder.epochCount;
            args.lr = builder.learningRate;
            args.ws = builder.contextWindowSize;
            ISubWordHashProvider p = builder.subWordHashProvider;
            args.subWordHashProvider = p;
            args.minn = p.GetMinN();
            args.maxn = p.GetMaxN();
            args.minCount = builder.minWordCount;

            FastTextTrainer trainer = new FastTextTrainer(args);

            // for catching and forwarding progress events.
            trainer.OnProgress += TrainingProgress;

            try
            {
                return trainer.Train(corpus);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw new SystemException("An error occured while Training on WordVectorsTrainer", e);
            }
        }

        public void TrainingProgress(FastTextTrainer.Progress progress)
        {
            OnProgress(progress);
        }

    }
}
