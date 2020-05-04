using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Data;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.NER
{
    /// <summary>
    /// Multi-class averaged perceptron training.
    /// </summary>
    public class PerceptronNer
    {
        private Dictionary<string, ClassModel> model;
        private TurkishMorphology morphology;
        public Dictionary<string, ClassModel> Model { get => model; set => model = value; }
        public TurkishMorphology Morphology { get => morphology; set => morphology = value; }

        public PerceptronNer(Dictionary<string, ClassModel> model, TurkishMorphology morphology)
        {
            this.Model = model;
            this.Morphology = morphology;
        }

        public void SaveModelAsText(string modelRoot)
        {
            foreach (string key in Model.Keys)
            {
                Model.GetValueOrDefault(key).SaveText(modelRoot);
            }
            File.WriteAllLines(
                Path.Combine(modelRoot, "types"),
                Model.Keys.OrderBy(e => e).ToList());
        }

        public void SaveModelCompressed(string modelRoot)
        {
            foreach (string key in Model.Keys)
            {
                Model.GetValueOrDefault(key).CompressAndSave(modelRoot);
            }
            File.WriteAllLines(
                Path.Combine(modelRoot, "types"),
                Model.Keys.OrderBy(e => e).ToList());
        }

        public static PerceptronNer LoadModelFromResources(string name, TurkishMorphology morphology)
        {
            string resourceRoot = "Resources/ner/model/" + name;
            try
            {
                List<string> types = TextIO.LoadLines(resourceRoot + "/types");
                Dictionary<string, ClassModel> weightsMap = new Dictionary<string, ClassModel>();
                foreach (string type in types)
                {
                    string resourcePath = resourceRoot + "/" + type + ".ner.model";
                    ClassModel weights = ClassModel.Load(type, resourcePath);
                    weightsMap[weights.Id] = weights;
                }
                return new PerceptronNer(weightsMap, morphology);
            }
            catch (IOException e)
            {
                throw new ApplicationException("An exception occured while Loading Model From Resources", e);
            }
        }

        public static PerceptronNer LoadModel(string modelRoot, TurkishMorphology morphology)
        {
            Dictionary<string, ClassModel> weightsMap = new Dictionary<string, ClassModel>();
            List<string> files = Directory.GetFiles(modelRoot)
                .Where(s => Path.GetFileName(s).EndsWith(".ner.model")).ToList();
            foreach (string file in files)
            {
                ClassModel weights = ClassModel.Load(file);
                weightsMap[weights.Id] = weights;
            }
            return new PerceptronNer(weightsMap, morphology);
        }

        public static ScoredItem<string> PredictTypeAndPosition(
            Dictionary<string, ClassModel> model,
            List<string> sparseKeys)
        {

            List<ScoredItem<string>> scores = new List<ScoredItem<string>>();

            // find score for each class.
            foreach (string output in model.Keys)
            {
                ClassModel o = model.GetValueOrDefault(output);
                float score = 0f;
                foreach (string s in sparseKeys)
                {
                    score += o.SparseWeights.Get(s);
                }
                scores.Add(new ScoredItem<string>(output, score));
            }

            // return max.
            return scores.OrderByDescending(e => e.Score).FirstOrDefault();
        }

        public NerDataSet Evaluate(NerDataSet set)
        {
            List<NerSentence> resultSentences = new List<NerSentence>();

            foreach (NerSentence sentence in set.Sentences)
            {
                List<NerToken> predictedTokens = new List<NerToken>();

                for (int i = 0; i < sentence.tokens.Count; i++)
                {

                    NerToken currentToken = sentence.tokens[i];

                    FeatureData data = new FeatureData(Morphology, sentence, i);
                    List<string> sparseInputs = data.GetTextualFeatures();

                    if (i > 0)
                    {
                        sparseInputs.Add("PreType=" + predictedTokens[i - 1].tokenId);
                    }
                    if (i > 1)
                    {
                        sparseInputs.Add("2PreType=" + predictedTokens[i - 2].tokenId);
                    }
                    if (i > 2)
                    {
                        sparseInputs.Add("3PreType=" + predictedTokens[i - 3].tokenId);
                    }

                    ScoredItem<string> predicted = PerceptronNer.PredictTypeAndPosition(Model, sparseInputs);

                    NerToken predictedToken = NerToken.FromTypePositionString(
                        currentToken.index, currentToken.word, currentToken.normalized, predicted.Item);
                    predictedTokens.Add(predictedToken);

                }
                NerSentence predictedSentence = new NerSentence(sentence.content, predictedTokens);
                resultSentences.Add(predictedSentence);
            }
            return new NerDataSet(resultSentences);
        }

        public NerSentence FindNamedEntities(string sentence, List<string> words)
        {
            List<NerToken> tokens = new List<NerToken>();
            int index = 0;
            foreach (string word in words)
            {
                NerToken token = new NerToken(
                    index,
                    word,
                    NerDataSet.NormalizeForNer(word), NerDataSet.OutTokenType, NePosition.OUTSIDE);
                tokens.Add(token);
                index++;
            }

            NerSentence nerSentence = new NerSentence(sentence, tokens);

            List<NerToken> predictedTokens = new List<NerToken>();

            for (int i = 0; i < nerSentence.tokens.Count; i++)
            {

                NerToken currentToken = nerSentence.tokens[i];

                FeatureData data = new FeatureData(Morphology, nerSentence, i);
                List<string> sparseInputs = data.GetTextualFeatures();

                if (i > 0)
                {
                    sparseInputs.Add("PreType=" + predictedTokens[i - 1].tokenId);
                }
                if (i > 1)
                {
                    sparseInputs.Add("2PreType=" + predictedTokens[i - 2].tokenId);
                }
                if (i > 2)
                {
                    sparseInputs.Add("3PreType=" + predictedTokens[i - 3].tokenId);
                }

                ScoredItem<string> predicted = PredictTypeAndPosition(Model, sparseInputs);

                NerToken predictedToken = NerToken.FromTypePositionString(
                    currentToken.index, currentToken.word, currentToken.normalized, predicted.Item);
                predictedTokens.Add(predictedToken);

            }
            return new NerSentence(nerSentence.content, predictedTokens);
        }

        public NerSentence FindNamedEntities(string sentence)
        {
            return FindNamedEntities(sentence, TurkishTokenizer.Default.TokenizeToStrings(sentence));
        }

        public class ClassModel
        {
            string id;
            IWeightLookup sparseWeights = new Weights();
            List<DenseWeights> denseWeights = new List<DenseWeights>();

            public string Id { get => id; set => id = value; }
            public IWeightLookup SparseWeights { get => sparseWeights; set => sparseWeights = value; }
            public List<DenseWeights> DenseWeights { get => denseWeights; set => denseWeights = value; }

            public ClassModel(string id)
            {
                this.Id = id;
            }

            public ClassModel(string id, IWeightLookup sparseWeights)
            {
                this.Id = id;
                this.SparseWeights = sparseWeights;
            }

            internal void UpdateSparse(List<string> inputs, float value)
            {
                if (SparseWeights is CompressedWeights)
                {
                    throw new InvalidOperationException("Weights seems to be compressed. Cannot update weights.");
                }
                Weights w = (Weights)SparseWeights;
                foreach (string input in inputs)
                {
                    w.Increment(input, value);
                }
            }

            internal ClassModel Copy()
            {
                if (SparseWeights is CompressedWeights)
                {
                    throw new InvalidOperationException("Weights seems to be compressed. Cannot copy.");
                }
                Weights w = (Weights)SparseWeights;
                ClassModel model = new ClassModel(Id);
                model.SparseWeights = w.Copy();
                List<DenseWeights> copy = new List<DenseWeights>();
                foreach (DenseWeights denseWeight in DenseWeights)
                {
                    copy.Add(new DenseWeights(denseWeight.Id, (float[])denseWeight.Weights.Clone()));
                }
                model.DenseWeights = copy;
                return model;
            }

            public void SaveText(string outRoot)
            {
                if (SparseWeights is CompressedWeights)
                {
                    throw new InvalidOperationException("Weights seems to be compressed. Cannot copy.");
                }
                Weights w = (Weights)SparseWeights;
                string file = Path.Combine(outRoot, Id + ".ner.model");
                w.SaveAsText(file);
            }

            public void CompressAndSave(string outRoot)
            {
                if (SparseWeights is CompressedWeights)
                {
                    throw new InvalidOperationException(
                        "Weights seems to be compressed. Cannot compress it again.");
                }
                Weights w = (Weights)SparseWeights;
                CompressedWeights cw = w.Compress();
                string file = Path.Combine(outRoot, Id + ".ner.model");
                cw.Serialize(file);
            }

            public static ClassModel Load(string modelFile, string baseDirectory = "")
            {
                if (!string.IsNullOrEmpty(baseDirectory))
                {
                    modelFile = Path.Combine(baseDirectory, modelFile);
                }
                string id = Path.GetFileName(modelFile).Replace(".ner.model", "");
                IWeightLookup weightLookup;
                if (CompressedWeights.IsCompressed(modelFile))
                {
                    weightLookup = CompressedWeights.Deserialize(modelFile);
                }
                else
                {
                    weightLookup = Weights.LoadFromFile(modelFile);
                }
                return new ClassModel(id, weightLookup);
            }
        }

        public class DenseWeights
        {
            string id;
            float[] weights;

            public DenseWeights(string id, float[] weights)
            {
                this.Id = id;
                this.Weights = weights;
            }

            public string Id { get => id; set => id = value; }
            public float[] Weights { get => weights; set => weights = value; }
        }

        internal class FeatureData
        {
            static WordAnalysisSurfaceFormatter formatter = new WordAnalysisSurfaceFormatter();
            TurkishMorphology morphology;
            string currentWord;
            string currentWordOrig;
            string nextWord;
            string nextWordOrig;
            string nextWord2;
            string nextWord2Orig;
            string previousWord;
            string previousWordOrig;
            string previousWord2;
            string previousWord2Orig;

            internal FeatureData(
                TurkishMorphology morphology,
                NerSentence sentence,
                int index)
            {
                this.morphology = morphology;
                List<NerToken> tokens = sentence.tokens;
                this.currentWord = tokens[index].normalized;
                this.currentWordOrig = tokens[index].word;
                if (index == tokens.Count - 1)
                {
                    //this.nextWord = "</s>";
                    //this.nextWord2 = "</s>";
                    //this.nextWordOrig = "</s>";
                    //this.nextWord2Orig = "</s>";
                }
                else if (index == tokens.Count - 2)
                {
                    this.nextWord = tokens[index + 1].normalized;
                    //this.nextWord2 = "</s>";
                    this.nextWordOrig = tokens[index + 1].word;
                    this.nextWord2Orig = tokens[index + 1].word;
                }
                else
                {
                    this.nextWord = tokens[index + 1].normalized;
                    this.nextWord2 = tokens[index + 2].normalized;
                    this.nextWordOrig = tokens[index + 1].word;
                    this.nextWord2Orig = tokens[index + 1].word;
                }
                if (index == 0)
                {
                    //this.previousWord = "<s>";
                    //this.previousWord2 = "<s>";
                    //this.previousWordOrig = "<s>";
                    //this.previousWord2Orig = "<s>";
                }
                else if (index == 1)
                {
                    this.previousWord = tokens[index - 1].normalized;
                    //this.previousWord2 = "<s>";
                    this.previousWordOrig = tokens[index - 1].word;
                    this.previousWord2Orig = tokens[index - 1].word;
                }
                else
                {
                    this.previousWord = tokens[index - 1].normalized;
                    this.previousWord2 = tokens[index - 2].normalized;
                    this.previousWordOrig = tokens[index - 1].word;
                    this.previousWord2Orig = tokens[index - 1].word;
                }
            }

            internal void MorphologicalFeatures(string word, string featurePrefix, List<string> features)
            {
                if (word == null)
                {
                    return;
                }
                WordAnalysis analyses = morphology.Analyze(word);
                SingleAnalysis longest =
                    analyses.AnalysisCount() > 0 ?
                        analyses.GetAnalysisResults()[analyses.AnalysisCount() - 1] :
                        SingleAnalysis.Unknown(word);
                foreach (SingleAnalysis analysis in analyses)
                {
                    if (analysis.IsUnknown())
                    {
                        return;
                    }
                    if (analysis == longest)
                    {
                        continue;
                    }
                    List<string> cLemmas = analysis.GetLemmas();
                    List<string> lLemmas = longest.GetLemmas();

                    if (cLemmas[cLemmas.Count - 1].Length >
                        lLemmas[lLemmas.Count - 1].Length)
                    {
                        longest = analysis;
                    }
                }
                List<string> lemmas = longest.GetLemmas();
                features.Add(featurePrefix + "Stem:" + longest.GetStem());
                string ending = longest.GetEnding();
                if (ending.Length > 0)
                {
                    features.Add(featurePrefix + "Ending:" + ending);
                }
                features.Add(featurePrefix + "LongLemma:" + lemmas[lemmas.Count - 1]);
                features.Add(featurePrefix + "POS:" + longest.GetPos());
                features.Add(featurePrefix + "LastIg:" + longest.GetLastGroup().LexicalForm());
                //features.Add(featurePrefix + "ContainsProper:" + containsProper);
            }

            internal List<string> GetTextualFeatures()
            {

                List<string> features = new List<string>();
                features.Add("CW:" + currentWord);
                features.Add("NW:" + nextWord);
                features.Add("CW-NW:" + currentWord + nextWord);
                features.Add("2NW:" + nextWord2);
                features.Add("CW+NW+2NW:" + currentWord + nextWord + nextWord2);

                features.Add("PW:" + previousWord);
                features.Add("2PW:" + previousWord2);
                features.Add("CW-PW:" + currentWord + previousWord);
                features.Add("CW-PW-2PW:" + currentWord + previousWord + previousWord);

                features.Add("PW-CW-NW:" + previousWord + currentWord + nextWord);

                WordFeatures(currentWordOrig, "CW", features);
                WordFeatures(previousWordOrig, "PW", features);
                WordFeatures(nextWordOrig, "NW", features);

                MorphologicalFeatures(currentWordOrig, "CW", features);
                MorphologicalFeatures(previousWordOrig, "PW", features);
                MorphologicalFeatures(nextWordOrig, "NW", features);

                string cwLast2 =
                    currentWord.Length > 2 ? currentWord.Substring(currentWord.Length - 2) : "";
                if (cwLast2.Length > 0)
                {
                    features.Add("CWLast2:" + cwLast2);
                }
                string cwLast3 =
                    currentWord.Length > 3 ? currentWord.Substring(currentWord.Length - 3) : "";
                if (cwLast3.Length > 0)
                {
                    features.Add("CWLast3:" + cwLast3);
                }

                string cwFirst2 = currentWord.Length > 2 ? currentWord.Substring(0, 2) : "";
                if (cwFirst2.Length > 0)
                {
                    features.Add("CWFirst2:" + cwFirst2);
                }
                string cwFirst3 = currentWord.Length > 3 ? currentWord.Substring(0, 3) : "";
                if (cwFirst3.Length > 0)
                {
                    features.Add("CWFirst3:" + cwFirst3);
                }

                return features;
            }

            internal void WordFeatures(string word, string featurePrefix, List<string> features)
            {
                if (word == null)
                {
                    return;
                }
                features.Add(featurePrefix + "Upper:" + char.IsUpper(word[0]));
                features.Add(featurePrefix + "Punct:" + (word.Length == 1));
                bool allCap = true;
                foreach (char c in word.ToCharArray())
                {
                    if (!char.IsUpper(c))
                    {
                        allCap = false;
                        break;
                    }
                }
                features.Add(featurePrefix + "AllCap:" + allCap);
                string s = TextUtil.NormalizeApostrophes(word);
                int apostropheIndex = s.IndexOf('\'');
                features.Add(featurePrefix + "Apost:" + (apostropheIndex >= 0));
                if (apostropheIndex >= 0)
                {
                    string stem = word.Substring(0, apostropheIndex);
                    string ending = word.Substring(apostropheIndex + 1);
                    features.Add(featurePrefix + "Stem:" + stem);
                    features.Add(featurePrefix + "Ending:" + ending);
                }
            }
        }
    }
}
