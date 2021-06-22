using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Embeddings
{
    public class FastText
    {
        public static readonly int FastTextVersion = 11;
        public static readonly int FastTextFileFormatMagicInt32 = 793712314;
        private Args args_;
        private Dictionary dict_;
        private Model model_;

        public FastText(Args args_, Dictionary dict_, Model model)
        {
            this.args_ = args_;
            this.dict_ = dict_;
            this.model_ = model;
        }

        public void AddInputVector(Vector vec, int ind)
        {
            if (model_.quant_)
            {
                vec.AddRow(model_.qwi_, ind);
            }
            else
            {
                vec.AddRow(model_.wi_, ind);
            }
        }

        public Dictionary GetDictionary()
        {
            return dict_;
        }

        public Args GetArgs()
        {
            return args_;
        }

        public Matrix GetInputMatrix()
        {
            return model_.wi_;
        }

        public Matrix GetOutputMatrix()
        {
            return model_.wo_;
        }

        public int GetWordId(String word)
        {
            return dict_.GetId(word);
        }

        public int GetSubwordId(String word)
        {
            int h = Dictionary.Hash(word) % args_.bucket;
            return dict_.NWords() + h;
        }

        public static bool CheckModel(Stream stream)
        {
            BinaryReader dis = IOUtil.GetDataInputStream(stream);
            int magic = dis.ReadInt32().EnsureEndianness();
            if (magic != FastTextFileFormatMagicInt32)
            {
                return false;
            }
            int version = dis.ReadInt32().EnsureEndianness();
            if (version != FastTextVersion)
            {
                return false;
            }

            return true;
        }

        private Vector GetWordVector(String word)
        {
            int[] ngrams = dict_.GetSubWords(word);
            Vector vec = new Vector(args_.dim);
            foreach (int i in ngrams)
            {
                vec.AddRow(model_.wi_, i);
            }
            if (ngrams.Length > 0)
            {
                vec.Mul(1.0f / ngrams.Length);
            }
            return vec;
        }

        private Vector GetSubWordVector(String word)
        {
            int h = Dictionary.Hash(word) % args_.bucket;
            h = h + dict_.NWords();
            Vector vec = new Vector(args_.dim);
            AddInputVector(vec, h);
            return vec;
        }

        public void SaveVectors(string outFilePath)
        {
            StreamWriter pw = new StreamWriter(File.OpenWrite(outFilePath), Encoding.UTF8);
            pw.WriteLine(dict_.NWords() + " " + args_.dim);
            for (int i = 0; i < dict_.NWords(); i++)
            {
                String word = dict_.GetWord(i);
                Vector vector = GetWordVector(word);
                pw.WriteLine(word + " " + vector.AsString());
            }
        }

        public void SaveOutput(string outPath)
        {
            int n = (args_.model == Args.ModelName.Supervised) ? dict_.NLabels() : dict_.NWords();
            StreamWriter pw = new StreamWriter(File.OpenWrite(outPath), Encoding.UTF8);
            pw.WriteLine(dict_.NWords() + " " + args_.dim);
            for (int i = 0; i < n; i++)
            {
                String word = (args_.model == Args.ModelName.Supervised) ?
                    dict_.GetLabel(i) : dict_.GetWord(i);
                Vector vector = new Vector(args_.dim);
                vector.AddRow(model_.wo_, i);
                pw.WriteLine(word + " " + vector.AsString());
            }
        }

        private static bool CheckModel(BinaryReader dis)
        {
            int magic = dis.ReadInt32().EnsureEndianness();
            if (magic != FastTextFileFormatMagicInt32)
            {
                return false;
            }
            int version = dis.ReadInt32().EnsureEndianness();
            return version == FastTextVersion;
        }

        private void SignModel(BinaryWriter dos)
        {
            dos.Write(FastTextFileFormatMagicInt32.EnsureEndianness());
            dos.Write(FastTextVersion.EnsureEndianness());
        }

        public void SaveModel(string outFilePath)
        {
            using (BinaryWriter dos = IOUtil.GetDataOutputStream(outFilePath))
            {
                SignModel(dos);
                args_.Save(dos);
                dict_.Save(dos);
                dos.Write(model_.quant_);
                if (model_.quant_)
                {
                    model_.qwi_.Save(dos);
                }
                else
                {
                    model_.wi_.Save(dos);
                }
                dos.Write(args_.qout);
                if (model_.quant_ && args_.qout)
                {
                    model_.qwo_.Save(dos);
                }
                else
                {
                    model_.wo_.Save(dos);
                }
            }
        }


        public static FastText Load(string path)
        {
            using (BinaryReader dis = IOUtil.GetDataInputStream(path))
            {
                if (!CheckModel(dis))
                {
                    throw new InvalidOperationException("Model file has wrong file format.");
                }
                return Load(dis);
            }
        }

        public static FastText Load(BinaryReader dis)
        {
            Args args_ = Args.load(dis);
            Dictionary dict_ = Dictionary.Load(dis, args_);
            Model model_ = Model.Load(dis, args_);
            if (args_.model == Args.ModelName.Supervised)
            {
                model_.SetTargetCounts(dict_.GetCounts(Dictionary.TypeLabel));
            }
            else
            {
                model_.SetTargetCounts(dict_.GetCounts(Dictionary.TypeWord));
            }
            return new FastText(args_, dict_, model_);
        }

        public FastText Quantize(string path, Args qargs)
        {
            BinaryReader dis = IOUtil.GetDataInputStream(path);
            if (!CheckModel(dis))
            {
                throw new InvalidOperationException("Model file has wrong file format.");
            }
            return Quantize(dis, qargs);
        }

        // selects ids of highest L2Norm valued embeddings.
        // Returns (word - subword) indexes.
        int[] SelectEmbeddings(int cutoff)
        {

            Matrix input_ = model_.wi_;
            List<L2NormData> normIndexes = new List<L2NormData>(input_.m_);
            int eosid = dict_.GetId(Dictionary.EOS); // we want to retain EOS
            for (int i = 0; i < input_.m_; i++)
            {
                if (i == eosid)
                {
                    continue;
                }
                normIndexes.Add(new L2NormData(i, input_.L2NormRow(i)));
            }
            normIndexes.Sort((a, b) => b.l2Norm.CompareTo(a.l2Norm));

            int[] result = new int[cutoff];
            for (int i = 0; i < cutoff - 1; i++)
            {
                result[i] = normIndexes[i].index;
            }
            // add EOS.
            result[cutoff - 1] = eosid;
            return result;
        }

        public List<String> GetLabels()
        {
            return GetDictionary().GetLabels();
        }

        internal class L2NormData
        {
            internal readonly int index;
            internal readonly float l2Norm;

            public L2NormData(int index, float l2Norm)
            {
                this.index = index;
                this.l2Norm = l2Norm;
            }
        }

        FastText Quantize(BinaryReader dis, Args qargs)
        {
            Args args_ = Args.load(dis);
            if (args_.model != Args.ModelName.Supervised)
            {
                throw new InvalidOperationException("Only supervised models can be quantized.");
            }
            Dictionary dict_ = Dictionary.Load(dis, args_);
            Model model_ = Model.Load(dis, args_);

            args_.qout = qargs.qout;

            Matrix input = model_.wi_;
            if (qargs.cutoff > 0 && qargs.cutoff < input.m_)
            {
                int[] idx = SelectEmbeddings(qargs.cutoff);
                idx = dict_.Prune(idx);
                Matrix newInput = new Matrix(idx.Length, args_.dim);
                for (int i = 0; i < idx.Length; i++)
                {
                    for (int j = 0; j < args_.dim; j++)
                    {
                        newInput.Set(i, j, input.At(idx[i], j));
                    }
                }
                model_.wi_ = newInput;
                // TODO: add retraining. It was hard because of the design differences
            }

            QMatrix qwi_ = new QMatrix(model_.wi_, qargs.dsub, qargs.qnorm);

            QMatrix qwo_ = model_.qwo_;
            if (qargs.qout)
            {
                qwo_ = new QMatrix(model_.wo_, 2, qargs.qnorm);
            }

            model_ = new Model(model_.wi_, model_.wo_, args_, 0);
            model_.quant_ = true;
            model_.SetQuantizePointer(qwi_, qwo_, args_.qout);

            if (args_.model == Args.ModelName.Supervised)
            {
                model_.SetTargetCounts(dict_.GetCounts(Dictionary.TypeLabel));
            }
            else
            {
                model_.SetTargetCounts(dict_.GetCounts(Dictionary.TypeWord));
            }
            return new FastText(args_, dict_, model_);
        }

        public EvaluationResult Test(string input, int k)
        {
            return Test(input, k, -100f);
        }

        public class EvaluationResult
        {
            public readonly float precision;
            public readonly float recall;
            public readonly int k;
            public readonly int numberOfExamples;

            public EvaluationResult(
                float precision,
                float recall,
                int k,
                int numberOfExamples)
            {
                this.precision = precision;
                this.recall = recall;
                this.k = k;
                this.numberOfExamples = numberOfExamples;
            }

            public void PrintTo(StreamWriter stream)
            {
                stream.WriteLine(ToString());
            }

            public override string ToString()
            {
                return string.Format(
                    "P@{0}: {1:F3}  R@{2}: {3:F3} F@{4} {5:F3}  Number of examples = {6}",
                    k, precision,
                    k, recall,
                    k, (2 * precision * recall) / (precision + recall),
                    numberOfExamples);
            }
        }

        public EvaluationResult Test(string input, int k, float threshold)
        {
            int nexamples = 0, nlabels = 0;
            float precision = 0.0f;
            String lineStr;
            using (StreamReader reader = new StreamReader(File.OpenRead(input), Encoding.UTF8))
            {
                while ((lineStr = reader.ReadLine()) != null)
                {
                    IntVector words = new IntVector(), labels = new IntVector();
                    dict_.GetLine(lineStr, words, labels);
                    if (labels.Size() > 0 && words.Size() > 0)
                    {
                        List<Model.FloatIntPair> modelPredictions = model_.Predict(words.CopyOf(), threshold, k);
                        foreach (Model.FloatIntPair pair in modelPredictions)
                        {
                            if (labels.Contains(pair.second))
                            {
                                precision += 1.0f;
                            }
                        }
                        nexamples++;
                        nlabels += labels.Size();
                    }
                }
                return new EvaluationResult(
                    precision / (k * nexamples),
                    precision / nlabels,
                    k,
                    nexamples);
            }
        }

        public void Test(string input, int k, float threshold, Meter meter)
        {
            string lineStr;
            using (StreamReader reader = new StreamReader(File.OpenRead(input), Encoding.UTF8))
            {
                while ((lineStr = reader.ReadLine()) != null)
                {
                    IntVector words = new IntVector(), labels = new IntVector();
                    dict_.GetLine(lineStr, words, labels);
                    if (labels.Size() > 0 && words.Size() > 0)
                    {
                        List<Model.FloatIntPair> modelPredictions = model_.Predict(words.CopyOf(), threshold, k);
                        meter.Log(labels, modelPredictions);
                    }
                }
            }
        }

        public Vector TextVectors(List<string> paragraph)
        {
            Vector vec = new Vector(args_.dim);
            foreach (string s in paragraph)
            {
                IntVector line = new IntVector();
                dict_.GetLine(s, line, model_.GetRng());
                if (line.Size() == 0)
                {
                    continue;
                }
                dict_.AddWordNgramHashes(line, args_.wordNgrams);
                foreach (int i in line.CopyOf())
                {
                    vec.AddRow(model_.wi_, i);
                }
                vec.Mul((float)(1.0 / line.Size()));
            }
            return vec;
        }

        public float[] SentenceVector(String s)
        {
            Vector svec = new Vector(args_.dim);
            IntVector line;
            if (args_.model == Args.ModelName.Supervised)
            {
                line = new IntVector();
                dict_.GetLine(s, line, model_.GetRng());
                foreach (int i in line.CopyOf())
                {
                    AddInputVector(svec, i);
                }
                if (line.Size() > 0)
                {
                    svec.Mul(1f / line.Size());
                }
                return svec.GetData();
            }

            line = new IntVector();
            dict_.GetLine(s, line, model_.GetRng());
            dict_.AddWordNgramHashes(line, args_.wordNgrams);
            if (line.Size() == 0)
            {
                return svec.GetData();
            }

            int count = 0;
            foreach (int i in line.CopyOf())
            {

                Vector vec = GetWordVector(dict_.GetWord(i));
                float norm = vec.Norm();

                if (norm > 0)
                {
                    vec.Mul(1f / norm);
                    svec.AddVector(vec);
                    count++;
                }
            }
            if (count > 0)
            {
                svec.Mul(1f / count);
            }
            return svec.GetData();
        }

        public Vector GetSentenceVector(String s)
        {
            Vector svec = new Vector(args_.dim);
            if (args_.model == Args.ModelName.Supervised)
            {
                IntVector line = new IntVector(), labels = new IntVector();
                dict_.GetLine(s, line, labels);
                for (int i = 0; i < line.Size(); i++)
                {
                    AddInputVector(svec, line.Get(i));
                }
                if (!line.IsEmpty())
                {
                    svec.Mul(1.0f / line.Size());
                }
            }
            else
            {
                string[] tokens = s.Split("\\s+");
                int count = 0;
                foreach (string token in tokens)
                {
                    if (token.Length == 0)
                    {
                        continue;
                    }
                    Vector vec = GetWordVector(token);
                    float norm = vec.Norm();
                    if (norm > 0)
                    {
                        vec.Mul(1.0f / norm);
                        svec.AddVector(vec);
                        count++;
                    }
                }
                if (count > 0)
                {
                    svec.Mul(1.0f / count);
                }
            }
            return svec;
        }


        public List<ScoredItem<String>> Predict(String line, int k)
        {
            return Predict(line, k, -100f);
        }

        public List<ScoredItem<String>> Predict(String line, int k, float threshold)
        {
            IntVector words = new IntVector();
            IntVector labels = new IntVector();
            dict_.GetLine(line, words, labels);
            if (words.IsEmpty())
            {
                return new List<ScoredItem<string>>();
            }
            List<Model.FloatIntPair> modelPredictions =
                model_.Predict(words.CopyOf(), threshold, k);
            List<ScoredItem<string>> result = new List<ScoredItem<string>>(modelPredictions.Count);
            foreach (Model.FloatIntPair pair in modelPredictions)
            {
                result.Add(new ScoredItem<string>(dict_.GetLabel(pair.second), pair.first));
            }
            return result;
        }
    }
}
