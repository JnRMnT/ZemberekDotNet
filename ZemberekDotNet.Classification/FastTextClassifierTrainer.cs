using System;
using ZemberekDotNet.Core.Concurrency;
using ZemberekDotNet.Core.Embeddings;
using static ZemberekDotNet.Core.Embeddings.EmbeddingHashProviders;
using static ZemberekDotNet.Core.Embeddings.FastTextTrainer;

namespace ZemberekDotNet.Classification
{
    public class FastTextClassifierTrainer
    {
        public event OnProgressDelegate OnProgress;
        public static readonly int DefaultDimension = 50;
        public static readonly float DefaultLR = 0.2f;
        public static readonly int DefaultEpoch = 25;
        public static readonly int DefaultMinWordCount = 1;
        public static readonly int DefaultWordNGram = 1;
        public static readonly int DefaultContextWindowSize = 5;
        public static readonly int DefaultTC = Environment.ProcessorCount / 2;

        private readonly FastTextClassifierTrainerBuilder builder;

        FastTextClassifierTrainer(FastTextClassifierTrainerBuilder builder)
        {
            this.builder = builder;
        }

        public static FastTextClassifierTrainerBuilder Builder()
        {
            return new FastTextClassifierTrainerBuilder();
        }

        public enum LossType
        {
            Softmax,
            HierarchicalSoftMax
        }

        public class FastTextClassifierTrainerBuilder
        {
            internal LossType type = FastTextClassifierTrainer.LossType.Softmax;
            internal int wordNgramOrder = DefaultWordNGram;
            internal ISubWordHashProvider subWordHashProvider = new EmptySubwordHashProvider();
            internal float learningRate = DefaultLR;
            internal int threadCount = DefaultTC;
            internal int contextWindowSize = DefaultContextWindowSize;
            internal int epochCount = DefaultEpoch;
            internal int dimension = DefaultDimension;
            internal int minWordCount = DefaultMinWordCount;
            internal int quantizationCutOff = -1;

            public FastTextClassifierTrainerBuilder LossType(LossType type)
            {
                this.type = type;
                return this;
            }

            public FastTextClassifierTrainerBuilder WordNgramOrder(int order)
            {
                this.wordNgramOrder = order;
                return this;
            }

            public FastTextClassifierTrainerBuilder LearningRate(float lr)
            {
                this.learningRate = lr;
                return this;
            }

            public FastTextClassifierTrainerBuilder MinWordCount(int minWordCount)
            {
                this.minWordCount = minWordCount;
                return this;
            }

            public FastTextClassifierTrainerBuilder ThreadCount(int threadCount)
            {
                this.threadCount = ConcurrencyUtil.ValidateCpuThreadCount(threadCount);
                return this;
            }

            public FastTextClassifierTrainerBuilder EpochCount(int epochCount)
            {
                this.epochCount = epochCount;
                return this;
            }

            public FastTextClassifierTrainerBuilder Dimension(int dimension)
            {
                this.dimension = dimension;
                return this;
            }

            public FastTextClassifierTrainerBuilder ContextWindowSize(int contextWindowSize)
            {
                this.contextWindowSize = contextWindowSize;
                return this;
            }

            public FastTextClassifierTrainerBuilder SubWordHashProvider(ISubWordHashProvider provider)
            {
                this.subWordHashProvider = provider;
                return this;
            }

            public FastTextClassifierTrainerBuilder QuantizationCutOff(int quantizationCutOff)
            {
                this.quantizationCutOff = quantizationCutOff;
                return this;
            }

            public FastTextClassifierTrainer Build()
            {
                return new FastTextClassifierTrainer(this);
            }
        }

        public FastTextClassifier Train(string corpus)
        {
            Args args = Args.ForSupervised();
            args.loss = builder.type == LossType.Softmax ?
                Args.loss_name.Softmax : Args.loss_name.HierarchicalSoftmax;
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
            args.cutoff = builder.quantizationCutOff;

            FastTextTrainer trainer = new FastTextTrainer(args);

            // for catching and forwarding progress events.
            trainer.OnProgress+= TrainingProgress;

            try
            {
                return new FastTextClassifier(trainer.Train(corpus));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw new SystemException("An exception occured during training with FastTextClassifier", e);
            }
        }

        public void TrainingProgress(FastTextTrainer.Progress progress)
        {
            this.OnProgress(progress);
        }
    }
}
