using System;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Embeddings
{
    public class Meter
    {
        internal long nexamples_;
        internal IntMap<Metrics> labelMetrics_ = new IntMap<Metrics>();
        internal Metrics metrics_ = new Metrics();

        public double Precision(int i)
        {
            Metrics metrics = labelMetrics_.Get(i);
            return metrics.Precision();
        }

        public double Recall(int i)
        {
            return labelMetrics_.Get(i).Recall();
        }

        public double F1Score(int i)
        {
            return labelMetrics_.Get(i).F1Score();
        }

        public double Precision()
        {
            return metrics_.Precision();
        }

        public double Recall()
        {
            return metrics_.Recall();
        }

        public void Log(
            IntVector labels,
            List<Model.FloatIntPair> predictions)
        {
            nexamples_++;
            metrics_.gold += labels.Size();
            metrics_.predicted += predictions.Count;

            foreach (Model.FloatIntPair prediction in predictions)
            {
                labelMetrics_.Get(prediction.second).predicted++;

                if (labels.Contains(prediction.second))
                {
                    labelMetrics_.Get(prediction.second).predictedGold++;
                    metrics_.predictedGold++;
                }
            }

            foreach (int label in labels.CopyOf())
            {
                labelMetrics_.Get(label).gold++;
            }
        }

        public void WriteGeneralMetrics(StreamWriter streamWriter, int k)
        {
            streamWriter.WriteLine("N\t" + nexamples_);
            streamWriter.WriteLine(String.Format("P@{0}\t{1:F3}", k, metrics_.Precision()));
            streamWriter.WriteLine(String.Format("R@{0}\t{1:F3}", k, metrics_.Recall()));
        }

        public class Metrics
        {
            internal long gold;
            internal long predicted;
            internal long predictedGold;

           public double Precision()
            {
                return predictedGold / (double)predicted;
            }

            public double Recall()
            {
                return predictedGold / (double)gold;
            }

            public double F1Score()
            {
                return 2 * predictedGold / (double)(predicted + gold);
            }
        }
    }
}
