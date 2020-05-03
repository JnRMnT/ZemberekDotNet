using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Data;
using ZemberekDotNet.Core.Dynamic;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.Morphology.Ambiguity
{
    /// <summary>
    /// This is a class for applying morphological ambiguity resolution for Turkish sentences. Algorithm
    /// is based on "Haşim Sak, Tunga Güngör, and Murat Saraçlar. Morphological disambiguation of Turkish
    /// text with perceptron algorithm. In CICLing 2007, volume LNCS 4394, pages 107-118, 2007".
    /// 
    /// @see <a href="http://www.cmpe.boun.edu.tr/~hasim">Haşim Sak</a>
    /// <p>
    /// This code is adapted from the Author's original Perl implementation. However, this is not a
    /// direct port, many changes needed to be applied for Zemberek integration and it has a cleaner
    /// and faster design.
    /// <p>
    /// For Training, use PerceptronAmbiguityResolverTrainer class.
    /// </summary>
    public class PerceptronAmbiguityResolver : IAmbiguityResolver
    {
        private Decoder decoder;

        public PerceptronAmbiguityResolver(IWeightLookup averagedModel, FeatureExtractor extractor)
        {
            this.decoder = new Decoder(averagedModel, extractor);
        }

        public IWeightLookup GetModel()
        {
            return decoder.Model;
        }

        public Decoder GetDecoder()
        {
            return decoder;
        }
        public static PerceptronAmbiguityResolver FromModelFile(string modelFile)
        {
            IWeightLookup lookup;
            if (CompressedWeights.IsCompressed(modelFile))
            {
                lookup = CompressedWeights.Deserialize(modelFile);
            }
            else
            {
                lookup = Weights.LoadFromFile(modelFile);
            }
            FeatureExtractor extractor = new FeatureExtractor(false);
            return new PerceptronAmbiguityResolver(lookup, extractor);
        }

        public SentenceAnalysis Disambiguate(string sentence, List<WordAnalysis> allAnalyses)
        {
            DecodeResult best = decoder.BestPath(allAnalyses);
            List<SentenceWordAnalysis> l = new List<SentenceWordAnalysis>();
            for (int i = 0; i < allAnalyses.Count; i++)
            {
                WordAnalysis wordAnalysis = allAnalyses[i];
                SingleAnalysis analysis = best.BestParse[i];
                l.Add(new SentenceWordAnalysis(analysis, wordAnalysis));
            }
            return new SentenceAnalysis(sentence, l);
        }

        internal class WordData
        {
            string lemma;
            List<string> igs;

            public WordData(string lemma, List<string> igs)
            {
                this.Lemma = lemma;
                this.Igs = igs;
            }

            public string Lemma { get => lemma; set => lemma = value; }
            public List<string> Igs { get => igs; set => igs = value; }

            public static WordData FromAnalysis(SingleAnalysis sa)
            {
                string lemma = sa.GetDictionaryItem().lemma;
                SecondaryPos secPos = sa.GetDictionaryItem().secondaryPos;
                string sp = secPos == SecondaryPos.None ? "" : secPos.GetStringForm();

                List<string> igs = new List<string>(sa.GroupCount());
                for (int i = 0; i < sa.GroupCount(); i++)
                {
                    string s = sa.GetGroup(0).LexicalForm();
                    if (i == 0)
                    {
                        s = sp + s;
                    }
                    igs.Add(s);
                }
                return new WordData(lemma, igs);
            }

            public string LastGroup()
            {
                return Igs[Igs.Count - 1];
            }
        }

        public class FeatureExtractor
        {
            bool useCache;

            ConcurrentDictionary<SingleAnalysis[], IntValueMap<string>> featureCache =
                new ConcurrentDictionary<SingleAnalysis[], IntValueMap<string>>();

            public FeatureExtractor(bool useCache)
            {
                this.useCache = useCache;
            }

            // This is used for training. Extracts feature counts from current best analysis sequence.
            // Trainer then uses this counts to update weights for those features.
            public IntValueMap<string> ExtractFeatureCounts(List<SingleAnalysis> bestSequence)
            {
                List<SingleAnalysis> seq = new List<SingleAnalysis> { sentenceBegin, sentenceBegin };
                seq.AddRange(bestSequence);
                seq.Add(sentenceEnd);
                IntValueMap<string> featureCounts = new IntValueMap<string>();
                for (int i = 2; i < seq.Count; i++)
                {
                    SingleAnalysis[] trigram = {
                        seq[i - 2],
                        seq[i - 1],
                        seq[i]
                    };
                    IntValueMap<string> trigramFeatures = ExtractFromTrigram(trigram);
                    foreach (IntValueMap<string>.Entry<string> s in trigramFeatures.GetAsEntryList())
                    {
                        featureCounts.IncrementByAmount(s.key, s.count);
                    }
                }
                return featureCounts;
            }

            public IntValueMap<string> ExtractFromTrigram(SingleAnalysis[] trigram)
            {
                if (useCache)
                {
                    IntValueMap<String> cached = featureCache.GetValueOrDefault(trigram);
                    if (cached != null)
                    {
                        return cached;
                    }
                }

                IntValueMap<string> feats = new IntValueMap<string>();
                WordData w1 = WordData.FromAnalysis(trigram[0]);
                WordData w2 = WordData.FromAnalysis(trigram[1]);
                WordData w3 = WordData.FromAnalysis(trigram[2]);

                string r1 = w1.Lemma;
                string r2 = w2.Lemma;
                string r3 = w3.Lemma;

                string ig1 = string.Join("+", w1.Igs);
                string ig2 = string.Join("+", w2.Igs);
                string ig3 = string.Join("+", w3.Igs);

                string r1Ig1 = r1 + "+" + ig1;
                string r2Ig2 = r2 + "+" + ig2;
                string r3Ig3 = r3 + "+" + ig3;

                //feats.addOrIncrement("1:" + r1Ig1 + "-" + r2Ig2 + "-" + r3Ig3);
                feats.AddOrIncrement("2:" + r1 + ig2 + r3Ig3);
                feats.AddOrIncrement("3:" + r2Ig2 + "-" + r3Ig3);
                feats.AddOrIncrement("4:" + r3Ig3);
                //feats.addOrIncrement("5:" + r2 + ig2 + "-" + ig3);
                //feats.addOrIncrement("6:" + r1 + ig1 + "-" + ig3);

                //feats.addOrIncrement("7:" + r1 + "-" + r2 + "-" + r3);
                //feats.addOrIncrement("8:" + r1 + "-" + r3);
                feats.AddOrIncrement("9:" + r2 + "-" + r3);
                feats.AddOrIncrement("10:" + r3);
                feats.AddOrIncrement("10b:" + r2);
                feats.AddOrIncrement("10c:" + r1);

                //feats.addOrIncrement("11:" + ig1 + "-" + ig2 + "-" + ig3);
                //feats.addOrIncrement("12:" + ig1 + "-" + ig3);
                //feats.addOrIncrement("13:" + ig2 + "-" + ig3);
                //feats.addOrIncrement("14:" + ig3);

                string w1LastGroup = w1.LastGroup();
                string w2LastGroup = w2.LastGroup();

                foreach (string ig in w3.Igs)
                {
                    feats.AddOrIncrement("15:" + w1LastGroup + "-" + w2LastGroup + "-" + ig);
                    //feats.addOrIncrement("16:" + w1LastGroup + "-" + ig);
                    feats.AddOrIncrement("17:" + w2LastGroup + ig);
                    //feats.addOrIncrement("18:" + ig);
                }

                //      for (int k = 0; k < w3.igs.size() - 1; k++) {
                //        feats.addOrIncrement("19:" + w3.igs.get(k) + "-" + w3.igs.get(k + 1));
                //      }

                for (int k = 0; k < w3.Igs.Count; k++)
                {
                    feats.AddOrIncrement("20:" + k + "-" + w3.Igs[k]);
                }

                /*      if (Character.isUpperCase(r3.charAt(0))
                          && trigram[2].getDictionaryItem().secondaryPos == SecondaryPos.ProperNoun) {
                        feats.addOrIncrement("21:PROPER-"+r3);
                      } else {
                        feats.addOrIncrement("21b:NOT_PROPER-" + r3);
                      }*/

                feats.AddOrIncrement("22:" + trigram[2].GroupCount());
                //
                /*
                      if ((trigram[2] == sentenceEnd || trigram[2].getDictionaryItem().lemma.equals("."))
                          && trigram[2].getDictionaryItem().primaryPos == PrimaryPos.Verb) {
                        feats.addOrIncrement("23:ENDSVERB");
                      }
                */
                if (useCache)
                {
                    featureCache.TryAdd(trigram, feats);
                }
                return feats;
            }
        }

        private static readonly SingleAnalysis sentenceBegin = SingleAnalysis.Unknown("<s>");
        private static readonly SingleAnalysis sentenceEnd = SingleAnalysis.Unknown("</s>");

        /// <summary>
        /// Decoder finds the best path from multiple word analyses using Viterbi search algorithm.
        /// </summary>
        public class Decoder
        {
            IWeightLookup model;
            FeatureExtractor extractor;

            public Decoder(IWeightLookup model,
                FeatureExtractor extractor)
            {
                this.Model = model;
                this.Extractor = extractor;
            }

            public IWeightLookup Model { get => model; set => model = value; }
            internal FeatureExtractor Extractor { get => extractor; set => extractor = value; }

            public DecodeResult BestPath(List<WordAnalysis> sentence)
            {
                if (sentence.Count == 0)
                {
                    throw new ArgumentException("bestPath cannot be called with empty sentence.");
                }

                // holds the current active paths. initially it contains a single empty Hypothesis.
                ActiveList<Hypothesis> currentList = new ActiveList<Hypothesis>();
                currentList.Add(new Hypothesis(sentenceBegin, sentenceBegin, null, 0));

                foreach (WordAnalysis analysisData in sentence)
                {

                    ActiveList<Hypothesis> nextList = new ActiveList<Hypothesis>();

                    // this is necessary because word analysis may contain zero SingleAnalysis
                    // So we add an unknown SingleAnalysis to it.
                    List<SingleAnalysis> analyses = analysisData.GetAnalysisResults();
                    if (analyses.Count == 0)
                    {
                        analyses = new List<SingleAnalysis>(1);
                        analyses.Add(SingleAnalysis.Unknown(analysisData.GetInput()));
                    }

                    foreach (SingleAnalysis analysis in analyses)
                    {
                        foreach (Hypothesis h in currentList)
                        {
                            SingleAnalysis[] trigram = { h.Prev, h.Current, analysis };
                            IntValueMap<String> features = Extractor.ExtractFromTrigram(trigram);

                            float trigramScore = 0;
                            foreach (string key in features)
                            {
                                trigramScore += (Model.Get(key) * features.Get(key));
                            }

                            Hypothesis newHyp = new Hypothesis(
                                h.Current,
                                analysis,
                                h,
                                h.Score + trigramScore);
                            nextList.Add(newHyp);
                        }
                    }
                    currentList = nextList;
                }

                // score for sentence end. No need to create new hypotheses.
                foreach (Hypothesis h in currentList)
                {
                    SingleAnalysis[] trigram = { h.Prev, h.Current, sentenceEnd };
                    IntValueMap<string> features = Extractor.ExtractFromTrigram(trigram);

                    float trigramScore = 0;
                    foreach (string key in features)
                    {
                        trigramScore += (Model.Get(key) * features.Get(key));
                    }
                    h.Score += trigramScore;
                }

                Hypothesis best = currentList.GetBest();
                float bestScore = best.Score;
                List<SingleAnalysis> result = new List<SingleAnalysis>();

                // backtrack. from end to begin, we add words from Hypotheses.
                while (best.Previous != null)
                {
                    result.Add(best.Current);
                    best = best.Previous;
                }

                // because we collect from end to begin, reverse is required.
                result.Reverse();
                return new DecodeResult(result, bestScore);
            }
        }

        public class DecodeResult
        {
            List<SingleAnalysis> bestParse;
            float score;

            internal DecodeResult(List<SingleAnalysis> bestParse, float score)
            {
                this.BestParse = bestParse;
                this.Score = score;
            }

            public List<SingleAnalysis> BestParse { get => bestParse; set => bestParse = value; }
            public float Score { get => score; set => score = value; }
        }

        internal class Hypothesis : IScorable
        {
            SingleAnalysis prev; // previous word analysis result String
            SingleAnalysis current; // current word analysis result String
            Hypothesis previous; // previous Hypothesis.
            float score;

            public SingleAnalysis Prev { get => prev; set => prev = value; }
            public SingleAnalysis Current { get => current; set => current = value; }
            internal Hypothesis Previous { get => previous; set => previous = value; }
            public float Score { get => score; set => score = value; }

            internal Hypothesis(
                SingleAnalysis prev,
                SingleAnalysis current,
                Hypothesis previous,
                float score)
            {
                this.Prev = prev;
                this.Current = current;
                this.Previous = previous;
                this.Score = score;
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || !GetType().Equals(o.GetType()))
                {
                    return false;
                }

                Hypothesis that = (Hypothesis)o;

                if (!Prev.Equals(that.Prev))
                {
                    return false;
                }
                return Current.Equals(that.Current);
            }

            public override int GetHashCode()
            {
                int result = Prev.GetHashCode();
                result = 31 * result + Current.GetHashCode();
                return result;
            }

            public override string ToString()
            {
                return "Hypothesis{" +
                    "prev='" + Prev + '\'' +
                    ", current='" + Current + '\'' +
                    ", score=" + Score +
                    '}';
            }

            public float GetScore()
            {
                return Score;
            }
        }
    }
}
