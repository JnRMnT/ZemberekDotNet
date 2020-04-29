using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Math;
using ZemberekDotNet.Core.Native.Collections;

namespace ZemberekDotNet.Core.Embeddings
{
    public class Model
    {
        private static readonly int NegativeTableSize = 10000000;
        private static readonly int SigmoidTableSize = 512;
        private static readonly int MaxSigmoid = 8;
        private static readonly int LogTableSize = 512;
        private static readonly Func<FloatIntPair, FloatIntPair, int> PairComparer = (l, r) =>
        {
            return l.first.CompareTo(r.first);
        };
        private static float[] t_sigmoid;
        private static float[] t_log;

        static Model()
        {
            InitLog();
            InitSigmoid();
        }

        internal Matrix wi_;
        internal Matrix wo_;
        internal QMatrix qwi_;
        internal QMatrix qwo_;
        internal bool quant_;
        private Args args_;
        private Vector output_;
        private int hsz_;
        private int osz_; // output size
        private float loss_; // models loss value. This is updated during training,
        private long nexamples_;

        private NegativeSampler negativeSampler;

        private HierarchicalSoftMax hierarchicalSoftmax;

        private Random rng;

        public Model(Matrix wi,
            Matrix wo,
            Args args,
            int seed)
        {
            output_ = new Vector(wo.m_);
            rng = new Random(seed);
            wi_ = wi;
            wo_ = wo;
            args_ = args;
            osz_ = wo.m_;
            hsz_ = args.dim;
            loss_ = 0.0f;
            nexamples_ = 1;
        }

        public Model(Model model, int seed) : this(model.wi_, model.wo_, model.args_, seed)
        {

        }

        public static Model Load(BinaryReader dis, Args args_)
        {
            bool quant_input = dis.ReadBoolean();

            Matrix input_;
            Matrix output_;
            QMatrix qInput_ = null;
            QMatrix qOutput_ = null;

            if (quant_input)
            {
                qInput_ = new QMatrix();
                qInput_.Load(dis);
                input_ = Matrix.Empty;
            }
            else
            {
                input_ = Matrix.Load(dis);
            }

            //TODO: I dont like this. we should not override Args value like this.
            args_.qout = dis.ReadBoolean();

            if (quant_input && args_.qout)
            {
                qOutput_ = new QMatrix();
                qOutput_.Load(dis);
                output_ = Matrix.Empty;
            }
            else
            {
                output_ = Matrix.Load(dis);
            }

            Model model_ = new Model(input_, output_, args_, 0);
            model_.quant_ = quant_input;
            model_.SetQuantizePointer(qInput_, qOutput_, args_.qout);

            return model_;
        }

        private static void InitSigmoid()
        {
            t_sigmoid = new float[SigmoidTableSize + 1];
            for (int i = 0; i < SigmoidTableSize + 1; i++)
            {
                float x = i * 2f * MaxSigmoid / SigmoidTableSize - MaxSigmoid;
                t_sigmoid[i] = (float)(1.0d / (1.0d + (float)System.Math.Exp(-x)));
            }
        }

        private static void InitLog()
        {
            t_log = new float[LogTableSize + 1];
            for (int i = 0; i < LogTableSize + 1; i++)
            {
                float x = (i + 1e-5f) / LogTableSize;
                t_log[i] = (float)System.Math.Log(x);
            }
        }

        public void SetQuantizePointer(QMatrix qwi, QMatrix qwo, bool qout)
        {
            qwi_ = qwi;
            qwo_ = qwo;
            if (qout)
            {
                osz_ = qwo_.GetM();
            }
        }

        public Random GetRng()
        {
            return rng;
        }

        private float BinaryLogistic(
            Vector grad,
            Vector hidden,
            int target,
            bool label,
            float lr)
        {
            // calculates hidden layer and output node activation with Sigmoid
            float score = Sigmoid(wo_.DotRow(hidden, target));

            float alpha = lr * ((label ? 1f : 0f) - score);
            grad.AddRow(wo_, target, alpha);
            wo_.AddRow(hidden, target, alpha);

            if (label)
            {
                return -Log(score);
            }
            else
            {
                return -Log((float)(1.0 - score));
            }
        }

        private float NegativeSampling(
            Vector grad,
            Vector hidden,
            int target,
            float lr)
        {

            float loss = 0.0f;
            for (int n = 0; n <= args_.neg; n++)
            {
                if (n == 0)
                {
                    loss += BinaryLogistic(grad, hidden, target, true, lr);
                }
                else
                {
                    loss += BinaryLogistic(grad, hidden, negativeSampler.GetSample(target), false, lr);
                }
            }
            return loss;
        }

        private float HierarchicalSoftmax(
            Vector grad_,
            Vector hidden,
            int target,
            float lr)
        {

            float loss = 0.0f;
            IntVector binaryCode = hierarchicalSoftmax.codes[target];
            IntVector pathToRoot = hierarchicalSoftmax.paths[target];
            for (int i = 0; i < pathToRoot.Size(); i++)
            {
                loss += BinaryLogistic(grad_, hidden, pathToRoot.Get(i), binaryCode.Get(i) == 1, lr);
            }
            return loss;
        }

        private void ComputeOutputSoftmax(Vector hidden, Vector output)
        {
            if (quant_ && args_.qout)
            {
                output.Mul(qwo_, hidden);
            }
            else
            {
                output.Mul(wo_, hidden);
            }
            float max = FloatArrays.Max(output_.data_);
            float z = 0.0f;

            for (int i = 0; i < osz_; i++)
            {
                output.data_[i] = (float)System.Math.Exp(output.data_[i] - max);
                z += output.data_[i];
            }
            for (int i = 0; i < osz_; i++)
            {
                output.data_[i] /= z;
            }
        }

        private float Softmax(
            Vector grad_,
            Vector hidden_,
            int target,
            float lr)
        {
            ComputeOutputSoftmax(hidden_, output_);
            for (int i = 0; i < osz_; i++)
            {
                float label = (i == target) ? 1.0f : 0.0f;
                float alpha = lr * (label - output_.data_[i]);
                grad_.AddRow(wo_, i, alpha);
                wo_.AddRow(hidden_, i, alpha);
            }
            return -Log(output_.data_[target]);
        }

        // input[] is the word indexes.
        // it creates a vector for hidden weights with size of [dimension]
        // then sums all current word embeddings of input[] to this vector and averages it.
        public Vector ComputeHidden(int[] input)
        {
            Vector hidden = new Vector(hsz_);
            foreach (int i in input)
            {
                if (quant_)
                {
                    hidden.AddRow(qwi_, i);
                }
                else
                {
                    hidden.AddRow(wi_, i);
                }
            }
            hidden.Mul((float)(1.0 / input.Length));
            return hidden;
        }

        public List<FloatIntPair> Predict(
            int k,
            float threshold,
            Vector hidden,
            Vector output)
        {

            if (k <= 0)
            {
                throw new ArgumentException("k needs to be 1 or higher! Value = " + k);
            }

            if (args_.model != Args.ModelName.Supervised)
            {
                throw new ArgumentException(
                    "Model needs to be supervised for prediction! Mmodel = " + args_.model);
            }

            PriorityQueue<FloatIntPair> heap = new PriorityQueue<FloatIntPair>(k + 1, PairComparer);
            if (args_.loss == Args.LossName.HierarchicalSoftmax)
            {
                Dfs(k, threshold, 2 * osz_ - 2, 0.0f, heap, hidden);
            }
            else
            {
                FindKBest(k, threshold, heap, hidden, output);
            }

            List<FloatIntPair> result = new List<FloatIntPair>(heap);
            result.Sort();
            return result;
        }

        public List<FloatIntPair> Predict(int[] input, float threshold, int k)
        {
            Vector hidden_ = ComputeHidden(input);
            return Predict(k, threshold, hidden_, output_);
        }

        private void FindKBest(
            int k,
            float threshold,
            PriorityQueue<FloatIntPair> heap,
            Vector hidden,
            Vector output)
        {
            ComputeOutputSoftmax(hidden, output);
            for (int i = 0; i < osz_; i++)
            {
                if (output.data_[i] < threshold)
                {
                    continue;
                }
                if (heap.Count() == k && StdLog(output.data_[i]) < heap.Peek().first)
                {
                    continue;
                }
                heap.Enqueue(new FloatIntPair(StdLog(output.data_[i]), i));
                if (heap.Count() > k)
                {
                    heap.Dequeue();
                }
            }
        }

        //todo: check here.
        private void Dfs(
            int k,
            float threshold,
            int node,
            float score,
            PriorityQueue<FloatIntPair> heap,
            Vector hidden)
        {

            if (score < StdLog(threshold))
            {
                return;
            }
            if (heap.Count() == k && score < heap.Peek().first)
            {
                return;
            }

            Node[] tree = hierarchicalSoftmax.tree;

            if (tree[node].left == -1 && tree[node].right == -1)
            {
                heap.Enqueue(new FloatIntPair(score, node));
                if (heap.Count() > k)
                {
                    heap.Dequeue();
                }
                return;
            }
            float f;
            if (quant_ && args_.qout)
            {
                f = qwo_.DotRow(hidden, node - osz_);
            }
            else
            {
                f = wo_.DotRow(hidden, node - osz_);
            }
            f = (float)(1f / (1 + System.Math.Exp(-f)));

            Dfs(k, threshold, tree[node].left, score + StdLog(1.0f - f), heap, hidden);
            Dfs(k, threshold, tree[node].right, score + StdLog(f), heap, hidden);
        }


        // input is word indexes of the sentence
        // target is the label
        // lr is the current learning rate.
        public void Update(int[] input, int target, float lr)
        {
            Contract.Assert(target >= 0);
            Contract.Assert(target < osz_);
            if (input.Length == 0)
            {
                return;
            }
            Vector hidden_ = ComputeHidden(input);
            Vector grad_ = new Vector(hsz_);
            if (args_.loss == Args.LossName.NegativeSampling)
            {
                loss_ += NegativeSampling(grad_, hidden_, target, lr);
            }
            else if (args_.loss == Args.LossName.HierarchicalSoftmax)
            {
                loss_ += HierarchicalSoftmax(grad_, hidden_, target, lr);
            }
            else
            {
                loss_ += Softmax(grad_, hidden_, target, lr);
            }
            nexamples_ += 1;

            if (args_.model == Args.ModelName.Supervised)
            {
                grad_.Mul((float)(1.0 / input.Length));
            }

            foreach (int i in input)
            {
                wi_.AddRow(grad_, i, 1.0f);
            }
        }

        public void SetTargetCounts(int[] counts)
        {
            Contract.Assert(counts.Length == osz_);
            if (args_.loss == Args.LossName.NegativeSampling)
            {
                negativeSampler = NegativeSampler.Instantiate(counts, rng);
            }
            if (args_.loss == Args.LossName.HierarchicalSoftmax)
            {
                hierarchicalSoftmax = HierarchicalSoftMax.BuildTree(counts, osz_);
            }
        }


        private class NegativeSampler
        {
            internal int[] negatives;
            internal int currentIndex = 0;

            internal NegativeSampler(int[] negatives)
            {
                this.negatives = negatives;
            }

            // input is an array that carries counts of words or labels.
            // counts[W-index] = count of W
            // this will return a large array where system can draw negative samples for training.
            // Samples are word or label indexes.
            // amount of items in the array will be proportional with their counts.
            internal static NegativeSampler Instantiate(int[] counts, Random rng)
            {
                IntVector vec = new IntVector(counts.Length * 10);

                float z = 0.0f; // z will hold the sum of square roots of all counts
                foreach (int count in counts)
                {
                    z += (float)System.Math.Sqrt(count);
                }

                for (int i = 0; i < counts.Length; i++)
                {
                    float c = (float)System.Math.Sqrt(counts[i]);
                    for (int j = 0; j < c * NegativeTableSize / z; j++)
                    {
                        vec.Add(i);
                    }
                }
                vec.Shuffle(rng);
                int[] negatives = vec.CopyOf();
                return new NegativeSampler(negatives);
            }

            // gets a negative sample
            internal int GetSample(int target)
            {
                int negative;
                do
                {
                    negative = negatives[currentIndex];
                    currentIndex = (currentIndex + 1) % negatives.Length;
                } while (target == negative);
                return negative;
            }
        }

        private class HierarchicalSoftMax
        {

            internal Node[] tree;
            internal List<IntVector> paths;
            internal List<IntVector> codes;

            public HierarchicalSoftMax(
                Node[] tree,
                List<IntVector> paths,
                List<IntVector> codes)
            {
                this.tree = tree;
                this.paths = paths;
                this.codes = codes;
            }

            /// <summary>
            /// This is used for hierarchical softmax calculation.
            /// </summary>
            /// <param name="counts"></param>
            /// <param name="osz_"></param>
            /// <returns></returns>
            public static HierarchicalSoftMax BuildTree(int[] counts, int osz_)
            {
                int nodeCount = 2 * osz_ - 1;
                Node[] tree = new Node[nodeCount];
                List<IntVector> paths = new List<IntVector>();
                List<IntVector> codes = new List<IntVector>();

                for (int i = 0; i < nodeCount; i++)
                {
                    tree[i] = new Node();
                    tree[i].parent = -1;
                    tree[i].left = -1;
                    tree[i].right = -1;
                    tree[i].count = (long)1e15;
                    tree[i].binary = false;
                }
                for (int i = 0; i < osz_; i++)
                {
                    tree[i].count = counts[i];
                }
                int leaf = osz_ - 1;
                int node = osz_;
                for (int i = osz_; i < nodeCount; i++)
                {
                    int[] mini = new int[2];
                    for (int j = 0; j < 2; j++)
                    {
                        if (leaf >= 0 && tree[leaf].count < tree[node].count)
                        {
                            mini[j] = leaf--;
                        }
                        else
                        {
                            mini[j] = node++;
                        }
                    }
                    tree[i].left = mini[0];
                    tree[i].right = mini[1];
                    tree[i].count = tree[mini[0]].count + tree[mini[1]].count;
                    tree[mini[0]].parent = i;
                    tree[mini[1]].parent = i;
                    tree[mini[1]].binary = true;
                }
                for (int i = 0; i < osz_; i++)
                {
                    IntVector path = new IntVector();
                    IntVector code = new IntVector();
                    int j = i;
                    while (tree[j].parent != -1)
                    {
                        path.Add(tree[j].parent - osz_);
                        code.Add(tree[j].binary ? 1 : 0);
                        j = tree[j].parent;
                    }
                    paths.Add(path);
                    codes.Add(code);
                }
                return new HierarchicalSoftMax(tree, paths, codes);
            }
        }

        public float GetLoss()
        {
            return loss_ / nexamples_;
        }

        /**
         * This is a log approximation. Input values larger than 1.0 are truncated. Results are read from
         * a lookup table.
         */
        private float Log(float x)
        {
            if (x > 1.0f)
            {
                return 0.0f;
            }
            int i = (int)(x * LogTableSize);
            return t_log[i];
        }

        /**
         * This applies Math.log() to the input after applying a small offset to prevent log(0)
         *
         * @param x input
         */
        private float StdLog(float x)
        {
            return (float)System.Math.Log(x + 1e-7);
        }

        private float Sigmoid(float x)
        {
            if (x < -MaxSigmoid)
            {
                return 0.0f;
            }
            else if (x > MaxSigmoid)
            {
                return 1.0f;
            }
            else
            {
                int i = (int)((x + MaxSigmoid) * SigmoidTableSize / MaxSigmoid / 2);
                return t_sigmoid[i];
            }
        }

        private class Node
        {
            internal int parent;
            internal int left;
            internal int right;
            internal long count;
            internal bool binary;
        }

        public class FloatIntPair : IComparable<FloatIntPair>
        {
            internal readonly float first;
            internal readonly int second;

            public FloatIntPair(float first, int second)
            {
                this.first = first;
                this.second = second;
            }


            public int CompareTo(FloatIntPair o)
            {
                // descending.
                return o.first.CompareTo(first);
            }
        }
    }
}
