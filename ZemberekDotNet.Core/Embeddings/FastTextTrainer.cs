using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using static ZemberekDotNet.Core.Text.BlockTextLoader._SingleLoader;

namespace ZemberekDotNet.Core.Embeddings
{
    public class FastTextTrainer
    {
        public delegate void OnProgressDelegate(Progress progress);
        public event OnProgressDelegate OnProgress;

        private Args args_;

        public FastTextTrainer(Args args)
        {
            this.args_ = args;
        }

        /**
         * Trains a model for the input with given arguments, returns a FastText instance. Input can be a
         * text corpus, or a corpus with text and labels.
         */
        public FastText Train(string input)
        {

            Dictionary dict_ = Dictionary.ReadFromFile(input, args_);
            Matrix input_ = null;
            if (args_.pretrainedVectors.Length != 0)
            {
                //TODO: implement this.
                //loadVectors(args_->pretrainedVectors);
            }
            else
            {
                input_ = new Matrix(dict_.NWords() + args_.bucket, args_.dim);
                input_.Uniform(1.0f / args_.dim);
            }

            Matrix output_;
            if (args_.model == Args.model_name.Supervised)
            {
                output_ = new Matrix(dict_.NLabels(), args_.dim);
            }
            else
            {
                output_ = new Matrix(dict_.NWords(), args_.dim);
            }

            Model model_ = new Model(input_, output_, args_, 0);
            if (args_.model == Args.model_name.Supervised)
            {
                model_.SetTargetCounts(dict_.GetCounts(Dictionary.TypeLabel));
            }
            else
            {
                model_.SetTargetCounts(dict_.GetCounts(Dictionary.TypeWord));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            long tokenCount = 0;
            long charCount = TextIO.CharCount(input, Encoding.UTF8);
            Log.Info("Training started.");
            Stopwatch sw = Stopwatch.StartNew();

            Parallel.For(0, args_.thread - 1,
                () =>
                {
                    return tokenCount;
                },
                (int i, ParallelLoopState state, long localTokenCount) =>
            {
                // Here a model per thread is generated. It uses references to global model's input and output matrices.
                // AFAIK, original Fasttext does not care about thread safety of those matrices.
                Model threadModel = new Model(model_, i);
                TrainTask trainTask = new TrainTask(
                        i,
                        input,
                        (int)(i * charCount / args_.thread),
                              threadModel,
                              stopwatch,
                              dict_,
                              args_,
                              localTokenCount,
                              this);
                trainTask.Call();
                return trainTask.tokenCount;
            },
                (long localTokenCount) =>
                {
                    Interlocked.Add(ref tokenCount, localTokenCount);
                });
            return new FastText(args_, dict_, model_);
        }

        public class Progress
        {
            public long total;
            public long current;
            public float percentProgress;
            public float wordsPerSecond;
            public float learningRate;
            public float loss;
            public String eta;

            public Progress(float percentProgress, float wordsPerSecond, float learningRate, float loss,
                string eta)
            {
                this.percentProgress = percentProgress;
                this.wordsPerSecond = wordsPerSecond;
                this.learningRate = learningRate;
                this.loss = loss;
                this.eta = eta;
            }
        }

        internal class TrainTask
        {
            internal int threadId;
            internal string input;
            internal int startCharIndex;
            internal Stopwatch stopwatch;
            internal Model model;
            internal Dictionary dictionary;
            internal Args args_;
            internal long tokenCount;
            private FastTextTrainer owner;

            internal TrainTask(int threadId,
                string input,
                int startCharIndex,
                Model model,
                Stopwatch stopwatch,
                Dictionary dictionary,
                Args args_,
                long tokenCount,
                FastTextTrainer owner)
            {
                this.threadId = threadId;
                this.input = input;
                this.startCharIndex = startCharIndex;
                this.model = model;
                this.stopwatch = stopwatch;
                this.dictionary = dictionary;
                this.args_ = args_;
                this.tokenCount = tokenCount;
                this.owner = owner;
            }

            private void PrintInfo(float progress, float loss)
            {
                float t = stopwatch.ElapsedMilliseconds / 1000f;
                float wst = (float)tokenCount / t;
                float lr = (float)(args_.lr * (1.0f - progress));
                int eta = (int)(t / progress * (1 - progress) / args_.thread);
                int etah = eta / 3600;
                int etam = (eta - etah * 3600) / 60;

                Progress p = new Progress(
                    100 * progress,
                    wst,
                    lr,
                    loss,
                    string.Format("%dh%dm", etah, etam)
                );
                p.total = args_.epoch * dictionary.NTokens();
                p.current = tokenCount;
                owner.OnProgress(p);
            }

            private void Supervised(Model model,
                float lr,
                int[] line,
                int[] labels)
            {
                if (labels.Length == 0 || line.Length == 0)
                {
                    return;
                }
                int i = model.GetRng().Next(labels.Length);
                model.Update(line, labels[i], lr);
            }

            private void CBow(Model model, float lr, int[] line)
            {
                for (int w = 0; w < line.Length; w++)
                {
                    int boundary = model.GetRng().Next(args_.ws) + 1; // [1..args.ws]
                    IntVector bow = new IntVector();
                    for (int c = -boundary; c <= boundary; c++)
                    {
                        if (c != 0 && w + c >= 0 && w + c < line.Length)
                        {
                            int[] ngrams = dictionary.GetSubWords(line[w + c]);
                            bow.AddAll(ngrams);
                        }
                    }
                    model.Update(bow.CopyOf(), line[w], lr);
                }
            }

            private void SkipGram(Model model, float lr, int[] line)
            {
                for (int w = 0; w < line.Length; w++)
                {
                    int boundary = model.GetRng().Next(args_.ws) + 1; // [1..args.ws]
                    int[] ngrams = dictionary.GetSubWords(line[w]);
                    for (int c = -boundary; c <= boundary; c++)
                    {
                        if (c != 0 && w + c >= 0 && w + c < line.Length)
                        {
                            model.Update(ngrams, line[w + c], lr);
                        }
                    }
                }
            }

            public Model Call()
            {
                if (args_.model == Args.model_name.Supervised)
                {
                    model.SetTargetCounts(dictionary.GetCounts(Dictionary.TypeLabel));
                }
                else
                {
                    model.SetTargetCounts(dictionary.GetCounts(Dictionary.TypeWord));
                }
                long ntokens = dictionary.NTokens();
                long localTokenCount = 0;

                TextIterator it = (TextIterator)BlockTextLoader
                    .IteratorFromCharIndex(input, 1000, startCharIndex);
                float progress = 0f;
                while (true)
                {
                    while (it.HasNext())
                    {
                        it.MoveNext();
                        List<string> lines = it.Current.GetData();
                        foreach (string lineStr in lines)
                        {
                            if (tokenCount >= args_.epoch * ntokens)
                            {
                                if (threadId == 0 && args_.verbose > 0)
                                {
                                    PrintInfo(1.0f, model.GetLoss());
                                }
                                return model;
                            }
                            IntVector line = new IntVector(15);
                            progress = (float)((1.0 * tokenCount) / (args_.epoch * ntokens));
                            float lr = (float)(args_.lr * (1.0 - progress));

                            if (args_.model == Args.model_name.Supervised)
                            {
                                IntVector labels = new IntVector();
                                localTokenCount += dictionary.GetLine(lineStr, line, labels);
                                Supervised(model, lr, line.CopyOf(), labels.CopyOf());
                            }
                            else if (args_.model == Args.model_name.Cbow)
                            {
                                localTokenCount += dictionary.GetLine(lineStr, line, model.GetRng());
                                CBow(model, lr, line.CopyOf());
                            }
                            else if (args_.model == Args.model_name.SkipGram)
                            {
                                localTokenCount += dictionary.GetLine(lineStr, line, model.GetRng());
                                SkipGram(model, lr, line.CopyOf());
                            }
                            if (localTokenCount > args_.lrUpdateRate)
                            {
                                for (int i = 0; i < localTokenCount; i++)
                                {
                                    Interlocked.Increment(ref tokenCount);
                                }
                                localTokenCount = 0;
                            }
                        }
                        if (threadId == 0 && args_.verbose > 1)
                        {
                            PrintInfo(progress, model.GetLoss());
                        }
                    }
                    // start from the beginning again.
                    it = (TextIterator)BlockTextLoader.SinglePathIterator(input, 1000);
                }
            }
        }
    }
}
