using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Math;
using ZemberekDotNet.LangID.Model;

namespace ZemberekDotNet.LangID
{
    public class LanguageIdentifier
    {
        public static readonly int EliminationSampleStep = 20;
        public static readonly string Unknown = "unk";
        private static readonly Regex removeCharsPattern = new Regex("[0-9\"#$%^&*()_+\\-=/|\\\\<>{}\\[\\];:,]", RegexOptions.Singleline | RegexOptions.Multiline);
        private static readonly Regex whiteSpacePattern = new Regex("\\s+", RegexOptions.Singleline | RegexOptions.Multiline);
        public readonly int order;
        private readonly Dictionary<string, ICharNgramLanguageModel> models = new Dictionary<string, ICharNgramLanguageModel>();
        private readonly string[] modelIdArray;

        internal LanguageIdentifier(Dictionary<string, ICharNgramLanguageModel> models)
        {
            this.models = models;
            modelIdArray = new string[models.Count];
            int i = 0;
            foreach (string s in models.Keys)
            {
                modelIdArray[i++] = s;
            }
            IEnumerator<ICharNgramLanguageModel> enumerator = models.Values.GetEnumerator();
            enumerator.MoveNext();
            this.order = enumerator.Current != null ? enumerator.Current.GetOrder() : 0;
        }

        /// <summary>
        /// Loads internal models from internal compressed resource folder. Such as Resources/Models/langid has a
        /// folder named tr_group. It contains a group of language and unk compressed models. for loading
        /// those models, FromInternalModelGroup("tr_group") should be called.
        /// </summary>
        /// <param name="groupId">internal folder name</param>
        /// <returns>LanguageIdentifier</returns>
        public static LanguageIdentifier FromInternalModelGroup(string groupId)
        {
            ISet<string> languages = new HashSet<string>();
            string languageList = "Resources/Models/" + groupId + "/langs.txt";
            using (FileStream fileStream = File.OpenRead(languageList))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string langLine = SimpleTextReader.TrimmingReader(streamReader, "utf-8").AsString().Trim();
                    foreach (string langStr in langLine.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()))
                    {
                        languages.Add(langStr);
                    }
                }
                if (languages.Count == 0)
                {
                    throw new ArgumentException("No language is provided!");
                }
                Dictionary<string, ICharNgramLanguageModel> map = new Dictionary<string, ICharNgramLanguageModel>();
                foreach (string language in languages)
                {
                    string resourceName = "Resources/Models/" + groupId + "/" + language + ".clm";
                    using (FileStream languageFileStream = File.OpenRead(resourceName))
                    {
                        if (languageFileStream == null)
                        {
                            throw new ArgumentException("No internal model found: " + resourceName);
                        }
                        CompressedCharNgramModel model = CompressedCharNgramModel.Load(languageFileStream);
                        map.Add(language, model);
                    }
                }
                return new LanguageIdentifier(map);
            }
        }

        /// <summary>
        /// Loads all internal models from internal compressed resource folder.
        /// </summary>
        /// <returns>LanguageIdentifier</returns>
        public static LanguageIdentifier FromInternalModels()
        {
            string[] languages = Language.AllLanguages();
            Dictionary<string, ICharNgramLanguageModel> map = new Dictionary<string, ICharNgramLanguageModel>();
            foreach (string language in languages)
            {
                string resourceName = "Resources/Models/compressed/" + language + ".clm";
                using (FileStream fileStream = File.OpenRead(resourceName))
                {
                    if (fileStream == null)
                    {
                        throw new ArgumentException("No internal model found: " + resourceName);
                    }
                    CompressedCharNgramModel model = CompressedCharNgramModel.Load(fileStream);
                    map.Add(language, model);
                }
            }
            return new LanguageIdentifier(map);
        }

        private static Dictionary<string, ICharNgramLanguageModel> GetModelsFromDir(string dir, bool compressed)
        {
            Dictionary<string, ICharNgramLanguageModel> map = new Dictionary<string, ICharNgramLanguageModel>();
            if (!Directory.Exists(dir))
            {
                throw new ArgumentException("Training data directory does not exist:" + dir);
            }
            if (File.Exists(dir) && !Directory.Exists(dir))
            {
                throw new ArgumentException(dir + "is not a directory");
            }
            string[] allFiles = Directory.GetFiles(dir);
            if (allFiles == null || allFiles.Length == 0)
            {
                throw new ArgumentException("There is no file in:" + dir);
            }
            foreach (string file in allFiles)
            {
                string langStr = Path.GetFileNameWithoutExtension(file);
                if (compressed)
                {
                    map.Add(langStr, CompressedCharNgramModel.Load(file));
                }
                else
                {
                    map.Add(langStr, MapBasedCharNgramLanguageModel.LoadCustom(file));
                }
            }
            if (map.Count == 0)
            {
                throw new ArgumentException("There is no model file in dir:" + dir);
            }
            return map;
        }

        public static LanguageIdentifier FromUncompressedModelsDir(string dir)
        {
            return new LanguageIdentifier(GetModelsFromDir(dir, false));
        }

        public static LanguageIdentifier FromCompressedModelsDir(string dir)
        {
            return new LanguageIdentifier(GetModelsFromDir(dir, true));
        }

        public static LanguageIdentifier GenerateFromCounts(string countModelsDir, string[] languages)

        {
            Dictionary<string, string> modelFileMap = new Dictionary<string, string>();
            Dictionary<string, ICharNgramLanguageModel> modelMap = new Dictionary<string, ICharNgramLanguageModel>();
            string[] allFiles = Directory.GetFiles(countModelsDir);
            int order = 3;
            if (allFiles == null || allFiles.Length == 0)
            {
                throw new ArgumentException("There is no file in:" + countModelsDir);
            }
            foreach (string file in allFiles)
            {
                string langStr = Path.GetFileNameWithoutExtension(file);
                modelFileMap.Add(langStr, file);
            }
            // generate models for required models on the fly.
            Log.Info("Generating models for:" + string.Join(',', languages));

            foreach (string language in languages)
            {
                string l = language.ToLowerInvariant();
                if (modelFileMap.ContainsKey(l))
                {
                    CharNgramCountModel countModel = CharNgramCountModel.Load(modelFileMap.GetValueOrDefault(l));
                    order = countModel.order;
                    MapBasedCharNgramLanguageModel lm = MapBasedCharNgramLanguageModel.Train(countModel);
                    modelMap.Add(l, lm);
                    modelFileMap.Remove(l);
                }
                else
                {
                    Log.Warn("Cannot find count model file for language " + language);
                }
            }
            // generate garbage model from the remaining files if any left.
            if (!modelFileMap.IsEmpty())
            {
                Log.Info("Generating garbage model from remaining count models.");
                CharNgramCountModel garbageModel = new CharNgramCountModel("unk", order);
                foreach (string file in modelFileMap.Values)
                {
                    garbageModel.Merge(CharNgramCountModel.Load(file));
                }
                MapBasedCharNgramLanguageModel lm = MapBasedCharNgramLanguageModel.Train(garbageModel);
                modelMap.Add(lm.GetId(), lm);
            }
            return new LanguageIdentifier(modelMap);
        }

        /// <summary>
        /// Apply pre-processing by removing numbers, common punctuations and lowercasing the result.
        /// </summary>
        /// <param name="s">input</param>
        /// <returns>preprocessed value.</returns>
        public static string Preprocess(String s)
        {
            s = removeCharsPattern.Replace(s, "");
            s = whiteSpacePattern.Replace(s, " ");
            return s.ToLowerInvariant();
        }

        public List<ICharNgramLanguageModel> GetModels()
        {
            return new List<ICharNgramLanguageModel>(models.Values);
        }

        private int[] GetSequencial(string content)
        {
            if (content.Length <= order)
            {
                return new int[0];
            }
            int[] vals = new int[content.Length - order + 1];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = i;
            }
            return vals;
        }

        private int[] GetStepping(String content, int gramAmount)
        {
            if (content.Length <= order)
            {
                return new int[0];
            }
            if (gramAmount <= 0)
            {
                return GetSequencial(content);
            }
            int gramIndexLimit = content.Length - order + 1;
            // if gram count value is larger than the limit value, we get the max amount
            int gramCount = gramAmount;
            if (gramCount > gramIndexLimit)
            {
                gramCount = gramIndexLimit;
            }
            int s = gramIndexLimit / gramCount;
            int step = s < 3 ? 3 : s; // by default we make a stepping of 3
            int[] vals = new int[gramCount];

            int samplingPoint = 0;
            int startPoint = 0;

            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = samplingPoint;
                samplingPoint += step;
                if (samplingPoint >= gramIndexLimit)
                {
                    startPoint++;
                    samplingPoint = startPoint;
                }
            }
            return vals;
        }

        private String[] GetGrams(String content, int[] gramStarts)
        {
            String[] grams = new String[gramStarts.Length];
            int i = 0;
            foreach (int gramStart in gramStarts)
            {
                grams[i++] = content.Substring(gramStart, order);
            }
            return grams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The language id's that this identifier can detect.</returns>
        public ISet<string> GetLanguages()
        {
            ISet<string> all = new HashSet<string>(models.Keys);
            all.Remove(BaseCharNgramModel.Unknown);
            return all;
        }

        public bool ModelExists(string modelId)
        {
            return models.ContainsKey(modelId.ToLowerInvariant());
        }

        /// <summary>
        /// Identifies input text's language. It uses all data in the content and calculates score for all
        /// supplied languages.
        /// </summary>
        /// <param name="input">input text</param>
        /// <returns>identified language's id</returns>
        public string Identify(string input)
        {
            string clean = Preprocess(input);
            if (clean.Length < order)
            {
                return Unknown;
            }
            return IdentifySamples(clean, GetStepping(clean, clean.Length - 1));
        }

        /// <summary>
        /// Identifies input text's language using sampling. This methods gets maxSampleCount amount of
        /// samples from the input for detecting the language of the content.
        /// </summary>
        /// <param name="input">content</param>
        /// <param name="maxSampleCount">Max sampling value. Identifier gets this amount of samples from the
        /// content with stepping. if content length is less than maxSampleCount, or maxSampleCount is -1
        /// then sampling is not applied and method behaves like {@link #identify(String) getComponentAt}
        /// method.</param>
        /// <returns>identified language's id</returns>
        public string Identify(string input, int maxSampleCount)
        {
            string clean = Preprocess(input);
            if (clean.Length < order)
            {
                return Unknown;
            }
            return IdentifySamples(clean, GetStepping(clean, maxSampleCount));
        }

        /// <summary>
        /// When more than 5 languages are are tested for identification, this method applies elimination
        /// of some models after every 20 scoring operation. This way method eliminates lower scored
        /// languages from the operation.
        /// </summary>
        /// <param name="input">content</param>
        /// <param name="maxSampleCount">Max sampling value. Identifier gets this amount of samples from the
        /// content with stepping. if content length is less than maxSampleCount, or maxSampleCount is -1
        /// then sampling is not applied and method behaves like {@link #identify(String)} method.
        /// </param>
        /// <returns>identified language's id</returns>
        public string IdentifyFast(String input, int maxSampleCount)
        {
            String clean = Preprocess(input);
            if (input.Length < order)
            {
                return Unknown;
            }
            return ScoreWithElimination(clean, maxSampleCount).FirstOrDefault()?.model?.GetId();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">input data</param>
        /// <param name="maxSampleCount">Max sampling value. Identifier gets this amount of samples from the
        /// content with stepping. if content length is less than maxSampleCount, or maxSampleCount is -1
        /// then sampling is not applied.
        /// </param>
        /// <returns>the identification results in a list for all languages and their respective scores.
        /// List is sorted by score in descending order. So best match is the first item.</returns>
        public List<IdResult> GetScores(String input, int maxSampleCount)
        {
            String clean = Preprocess(input);
            if (input.Length < order)
            {
                return new List<IdResult>();
            }
            return ConvertModelScoresToIdscores(ScoreFull(clean, maxSampleCount));
        }

        /// <summary>
        /// This is similar to GetScores(String, int) method. But it eliminates low scored models
        /// during scoring. Result length may be less than the possible language count. This method is
        /// substantially faster with possible precision loss.
        /// </summary>
        /// <param name="input">input data</param>
        /// <param name="maxSampleCount">Max sampling value. Identifier gets this amount of samples from the
        /// content with stepping. if content length is less than maxSampleCount, or maxSampleCount is -1
        /// then sampling is not applied.
        /// </param>
        /// <returns>the identification results in a list for some languages and their respective scores.
        /// List is sorted by score in descending order. So best match is the first item.</returns>
        public List<IdResult> GetScoresFast(String input, int maxSampleCount)
        {
            String clean = Preprocess(input);
            if (input.Length < order)
            {
                return new List<IdResult>();
            }
            return ConvertModelScoresToIdscores(ScoreWithElimination(clean, maxSampleCount));
        }

        /// <summary>
        /// Returns if any slice of the content contains the input language.
        /// </summary>
        /// <param name="content">content to check</param>
        /// <param name="language"> two letter language id</param>
        /// <param name="sliceSize">slice size (character length)</param>
        /// <returns>true if content contains the input language in any of the slices.</returns>
        public bool ContainsLanguage(String content, String language, int sliceSize)
        {
            return ContainsLanguage(content, language, sliceSize, -1);
        }

        /// <summary>
        /// Returns if any slice of the content contains the input language.
        /// </summary>
        /// <param name="content">content to check</param>
        /// <param name="language">two letter language id</param>
        /// <param name="sliceSize">slice size (character length)</param>
        /// <param name="samplePerSlice">amount of samples to score in each slice.</param>
        /// <returns>true if content contains the input language in any of the slices.</returns>
        public bool ContainsLanguage(String content, String language, int sliceSize,
            int samplePerSlice)
        {
            if (sliceSize < 10)
            {
                throw new ArgumentException("Slice size cannot be less than 10");
            }
            content = Preprocess(content);
            if (sliceSize >= content.Length)
            {
                sliceSize = content.Length;
            }
            if (samplePerSlice < 0 || samplePerSlice > sliceSize)
            {
                samplePerSlice = sliceSize;
            }
            if (content.Length < order)
            {
                return false;
            }
            int sliceIndex = 0;
            int begin, end = 0;
            while (end < content.Length)
            {
                begin = sliceIndex * sliceSize;
                end = begin + sliceSize;
                if (content.Length - end < sliceSize)
                {
                    end = content.Length;
                }
                string slice = content.Substring(begin, end - begin);
                if (ScoreWithElimination(slice, samplePerSlice)[0].model.GetId().Equals(language))
                {
                    return true;
                }
                sliceIndex++;
            }
            return false;
        }

        private string IdentifySamples(string input, int[] samplingPoints)
        {
            string[] grams = GetGrams(input, samplingPoints);
            double max = -double.MaxValue;
            String maxLanguage = null;
            foreach (ICharNgramLanguageModel model in models.Values)
            {
                double prob = 0;
                foreach (string gram in grams)
                {
                    prob += model.GramProbability(gram);
                }
                if (prob > max)
                {
                    max = prob;
                    maxLanguage = model.GetId();
                }
            }
            return maxLanguage;
        }

        private List<ModelScore> ScoreFull(string input, int maxSampleCount)
        {
            int[] samplingPoints;
            if (maxSampleCount <= 0)
            {
                samplingPoints = GetStepping(input, input.Length);
            }
            else
            {
                samplingPoints = GetStepping(input, maxSampleCount);
            }
            List<ModelScore> modelScores = new List<ModelScore>(modelIdArray.Length);
            foreach (ICharNgramLanguageModel model in models.Values)
            {
                modelScores.Add(new ModelScore(model, 0));
            }
            String[] grams = GetGrams(input, samplingPoints);
            int gramCounter = 0;
            while (gramCounter < grams.Length)
            {
                foreach (ModelScore modelScore in modelScores)
                {
                    modelScore.score += modelScore.model.GramProbability(grams[gramCounter]);
                }
                gramCounter++;
            }
            modelScores.Sort();
            return modelScores;
        }

        private List<IdResult> ConvertModelScoresToIdscores(List<ModelScore> modelScores)
        {
            List<IdResult> res = new List<IdResult>(modelScores.Count);
            for (int i = 0; i < modelScores.Count; i++)
            {
                ModelScore modelScore = modelScores[i];
                res.Insert(i, new IdResult(modelScore.model.GetId(), modelScore.score));
            }
            return res;
        }

        private List<ModelScore> ScoreWithElimination(String input, int maxSampleCount)
        {
            int[] samplingPoints;
            if (maxSampleCount <= 0)
            {
                samplingPoints = GetStepping(input, input.Length);
            }
            else
            {
                samplingPoints = GetStepping(input, maxSampleCount);
            }
            List<ModelScore> modelScores = new List<ModelScore>(modelIdArray.Length);
            foreach (ICharNgramLanguageModel model in models.Values)
            {
                modelScores.Add(new ModelScore(model, 0));
            }
            String[] grams = GetGrams(input, samplingPoints);
            int gramCounter = 0;
            int intervalCounter = 0;
            while (gramCounter < grams.Length)
            {
                if (intervalCounter == EliminationSampleStep && modelScores.Count > 2)
                {
                    intervalCounter = 0;
                    modelScores.Sort();
                    modelScores = modelScores.GetRange(0, modelScores.Count / 2 + 1);
                }
                foreach (ModelScore modelScore in modelScores)
                {
                    modelScore.score += modelScore.model.GramProbability(grams[gramCounter]);
                }
                intervalCounter++;
                gramCounter++;
            }
            modelScores.Sort();
            return modelScores;
        }

        // TODO make it public after proper testing
        private string Identify(String input, int maxSampleCount, double threshold)
        {
            String clean = Preprocess(input);
            if (clean.Length < order)
            {
                return Unknown;
            }
            IdResult result = IdentifyConf(clean, GetStepping(clean, maxSampleCount));
            if (result.score >= threshold)
            {
                return result.id;
            }
            else
            {
                return BaseCharNgramModel.Unknown;
            }
        }

        // TODO make it public after proper testing
        private string Identify(string input, double confidenceThreshold)
        {
            string clean = Preprocess(input);
            if (clean.Length < order)
            {
                return Unknown;
            }
            IdResult result = IdentifyConf(clean, GetSequencial(clean));
            if (result.score >= confidenceThreshold)
            {
                return result.id;
            }
            else
            {
                return Unknown;
            }
        }

        private IdResult IdentifyConf(string input, int[] samplingPoints)
        {
            string[] grams = GetGrams(input, samplingPoints);
            double[] scores = new double[models.Count];
            double max = -double.MaxValue;
            int i = 0;
            int best = 0;
            double totalScore = LogMath.LogZero;
            foreach (string modelId in modelIdArray)
            {
                ICharNgramLanguageModel charNgramLanguageModel = models.GetValueOrDefault(modelId);
                double prob = 0;
                foreach (string gram in grams)
                {
                    prob += charNgramLanguageModel.GramProbability(gram);
                }
                scores[i] = prob;
                totalScore = LogMath.LogSum(totalScore, prob);
                if (prob > max)
                {
                    max = prob;
                    best = i;
                }
                i++;
            }
            return new IdResult(modelIdArray[best], Math.Exp(scores[best] - totalScore));
        }

        private class ModelScore : IComparable<ModelScore>
        {
            internal ICharNgramLanguageModel model;
            internal double score;

            internal ModelScore(ICharNgramLanguageModel model, double score)
            {
                this.model = model;
                this.score = score;
            }

            public int CompareTo(ModelScore modelScore)
            {
                return modelScore.score.CompareTo(score);
            }

            public override string ToString()
            {
                return model.GetId() + " : " + score;
            }
        }

        public class IdResult
        {

            public readonly string id;
            public double score;

            public IdResult(string id, double score)
            {
                this.id = id;
                this.score = score;
            }

            public override string ToString()
            {
                return id + " : " + String.Format("{0:F3}", score);
            }
        }
    }
}
