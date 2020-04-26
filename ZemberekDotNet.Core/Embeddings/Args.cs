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
        public loss_name loss;
        public model_name model;
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

        public static Args ForWordVectors(model_name modelName)
        {
            Args args = new Args();
            args.minn = 3;
            args.maxn = 6;
            args.subWordHashProvider = new EmbeddingHashProviders.CharacterNgramHashProvider(args.minn, args.maxn);
            args.lr = 0.05;
            args.loss = loss_name.NegativeSampling;
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
            args.loss = loss_name.Softmax;
            args.model = model_name.Supervised;
            args.wordNgrams = 2;
            return args;
        }

        public static Args load(BinaryReader binaryReader)
        {
            Args args = new Args();
            args.dim = binaryReader.ReadInt32();
            args.ws = binaryReader.ReadInt32();
            args.epoch = binaryReader.ReadInt32();
            args.minCount = binaryReader.ReadInt32();
            args.neg = binaryReader.ReadInt32();
            args.wordNgrams = binaryReader.ReadInt32();
            int loss = binaryReader.ReadInt32();
            if (loss == (int)loss_name.HierarchicalSoftmax)
            {
                args.loss = loss_name.HierarchicalSoftmax;
            }
            else if (loss == (int)loss_name.NegativeSampling)
            {
                args.loss = loss_name.NegativeSampling;
            }
            else if (loss == (int)loss_name.Softmax)
            {
                args.loss = loss_name.Softmax;
            }
            else
            {
                throw new InvalidOperationException("Unknown loss type.");
            }
            int model = binaryReader.ReadInt32();
            if (model == (int)model_name.Cbow)
            {
                args.model = model_name.Cbow;
            }
            else if (model == (int)model_name.SkipGram)
            {
                args.model = model_name.SkipGram;
            }
            else if (model == (int)model_name.Supervised)
            {
                args.model = model_name.Supervised;
            }
            else
            {
                throw new InvalidOperationException("Unknown model type.");
            }
            args.bucket = binaryReader.ReadInt32();
            args.minn = binaryReader.ReadInt32();
            args.maxn = binaryReader.ReadInt32();
            args.lrUpdateRate = binaryReader.ReadInt32();
            args.t = binaryReader.ReadDouble();

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
            binaryWriter.Write(dim);
            binaryWriter.Write(ws);
            binaryWriter.Write(epoch);
            binaryWriter.Write(minCount);
            binaryWriter.Write(neg);
            binaryWriter.Write(wordNgrams);
            binaryWriter.Write((int)loss);
            binaryWriter.Write((int)model);
            binaryWriter.Write(bucket);
            binaryWriter.Write(minn);
            binaryWriter.Write(maxn);
            binaryWriter.Write(lrUpdateRate);
            binaryWriter.Write(t);
        }

        public enum model_name
        {
            Cbow = 1,
            SkipGram = 2,
            Supervised = 3
        }

        public enum loss_name
        {
            HierarchicalSoftmax = 1,
            NegativeSampling = 2,
            Softmax = 3
        }
    }
}
