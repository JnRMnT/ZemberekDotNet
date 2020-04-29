using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZemberekDotNet.Core.Embeddings
{
    public class Args
    {
        public double lr;
        public int lrUpdateRate;
        public int dim;
        public int ws;
        public int epoch;
        public int minCount;
        public int minCountLabel;
        public int neg;
        public int wordNgrams;
        public LossName loss;
        public ModelName model;
        public int bucket;
        public int minn;
        public int maxn;
        public int thread;
        public double t;
        public String label;
        public int verbose;
        public String pretrainedVectors;
        public ISubWordHashProvider subWordHashProvider;

        public bool qout = false;
        public bool retrain = false;
        public bool qnorm = false;
        public int cutoff = 0;
        public int dsub = 2;

        private Args()
        {
            dim = 100;
            ws = 5;
            epoch = 5;
            minCount = 5;
            minCountLabel = 0;
            neg = 5;
            bucket = 2_000_000;
            thread = Environment.ProcessorCount / 2;
            lrUpdateRate = 100;
            t = 1e-4;
            label = "__label__";
            verbose = 2;
            pretrainedVectors = "";
        }

        public static Args ForWordVectors(ModelName modelName)
        {
            Args args = new Args();
            args.minn = 3;
            args.maxn = 6;
            args.subWordHashProvider = new EmbeddingHashProviders.CharacterNgramHashProvider(args.minn, args.maxn);
            args.lr = 0.05;
            args.loss = LossName.NegativeSampling;
            args.model = modelName;
            args.wordNgrams = 1;
            return args;
        }

        public static Args ForSupervised()
        {
            Args args = new Args();
            args.minn = 0;
            args.maxn = 0;
            args.maxn = 0;
            args.subWordHashProvider =
                new EmbeddingHashProviders.EmptySubwordHashProvider();
            args.lr = 0.1;
            args.loss = LossName.Softmax;
            args.model = ModelName.Supervised;
            args.wordNgrams = 2;
            return args;
        }

        public static Args load(BinaryReader binaryReader)
        {
            Args args = new Args();
            args.dim = binaryReader.ReadInt32().EnsureEndianness();
            args.ws = binaryReader.ReadInt32().EnsureEndianness();
            args.epoch = binaryReader.ReadInt32().EnsureEndianness();
            args.minCount = binaryReader.ReadInt32().EnsureEndianness();
            args.neg = binaryReader.ReadInt32().EnsureEndianness();
            args.wordNgrams = binaryReader.ReadInt32().EnsureEndianness();
            int loss = binaryReader.ReadInt32().EnsureEndianness();
            if (loss == (int)LossName.HierarchicalSoftmax)
            {
                args.loss = LossName.HierarchicalSoftmax;
            }
            else if (loss == (int)LossName.NegativeSampling)
            {
                args.loss = LossName.NegativeSampling;
            }
            else if (loss == (int)LossName.Softmax)
            {
                args.loss = LossName.Softmax;
            }
            else
            {
                throw new InvalidOperationException("Unknown loss type.");
            }
            int model = binaryReader.ReadInt32().EnsureEndianness();
            if (model == (int)ModelName.Cbow)
            {
                args.model = ModelName.Cbow;
            }
            else if (model == (int)ModelName.SkipGram)
            {
                args.model = ModelName.SkipGram;
            }
            else if (model == (int)ModelName.Supervised)
            {
                args.model = ModelName.Supervised;
            }
            else
            {
                throw new InvalidOperationException("Unknown model type.");
            }
            args.bucket = binaryReader.ReadInt32().EnsureEndianness();
            args.minn = binaryReader.ReadInt32().EnsureEndianness();
            args.maxn = binaryReader.ReadInt32().EnsureEndianness();
            args.lrUpdateRate = binaryReader.ReadInt32().EnsureEndianness();
            args.t = binaryReader.ReadDouble().EnsureEndianness();

            if (args.minn != 0)
            {
                args.subWordHashProvider = new EmbeddingHashProviders.CharacterNgramHashProvider(args.minn,
                    args.maxn);
            }
            else
            {
                args.subWordHashProvider = new EmbeddingHashProviders.EmptySubwordHashProvider();
            }

            return args;
        }

        public void Save(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(dim.EnsureEndianness());
            binaryWriter.Write(ws.EnsureEndianness());
            binaryWriter.Write(epoch.EnsureEndianness());
            binaryWriter.Write(minCount.EnsureEndianness());
            binaryWriter.Write(neg.EnsureEndianness());
            binaryWriter.Write(wordNgrams.EnsureEndianness());
            binaryWriter.Write(((int)loss).EnsureEndianness());
            binaryWriter.Write(((int)model).EnsureEndianness());
            binaryWriter.Write(bucket.EnsureEndianness());
            binaryWriter.Write(minn.EnsureEndianness());
            binaryWriter.Write(maxn.EnsureEndianness());
            binaryWriter.Write(lrUpdateRate.EnsureEndianness());
            binaryWriter.Write(t.EnsureEndianness());
        }

        public enum ModelName
        {
            Cbow = 1,
            SkipGram = 2,
            Supervised = 3
        }

        public enum LossName
        {
            HierarchicalSoftmax = 1,
            NegativeSampling = 2,
            Softmax = 3
        }
    }
}
