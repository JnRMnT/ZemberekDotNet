using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.LangID.Model;

namespace ZemberekDotNet.LangID.Train
{
    public class Trainer
    {
        string[] trainingDataDirs;
        string countModelDir;

        ModelGenerator modelGenerator = new ModelGenerator();

        MultiMap<string, string> langFileMap = new MultiMap<string, string>();

        int[] cutOffs;
        int order;

        public Trainer(string[] trainingDataDirs, string countModelDir, int order, int[] cutOffs)
        {
            Console.WriteLine("Order:" + order);
            this.order = order;
            this.cutOffs = cutOffs;
            this.trainingDataDirs = trainingDataDirs;
            foreach (string training in trainingDataDirs)
            {
                if (!File.Exists(training))
                {
                    throw new ArgumentException("Training data directory does not exist:" + training);
                }
                if (!Directory.Exists(training))
                {
                    throw new ArgumentException(training + "is not a directory");
                }
            }
            this.countModelDir = countModelDir;
            MkDir(this.countModelDir);
            foreach (string trainingDataDir in trainingDataDirs)
            {
                string[] allFiles = Directory.GetFiles(trainingDataDir);
                if (allFiles == null || allFiles.Length == 0)
                {
                    throw new ArgumentException("There is no file in training dir:" + trainingDataDir);
                }
                foreach (string file in allFiles)
                {
                    string fileName = Path.GetFileName(file);
                    if (File.Exists(file) && fileName.Contains("train"))
                    {
                        string langStr = fileName.Substring(0, fileName.IndexOf("-"));
                        langFileMap.Add(langStr.ToLowerInvariant(), file);
                    }
                }
            }
            if (langFileMap.Keys.Count() == 0)
            {
                throw new ArgumentException("There is no training files in training dirs");
            }
        }

        private static void Train3gram()
        {
            string[]
            trainingDirs = {
                "/home/kodlab/data/language-data/subtitle",
                 "/home/kodlab/data/language-data/wiki" };
            ISet<string> large = new HashSet<string> { "JA", "KO", "ZH", "ML", "HI", "KM", "MY", "EL", "AR" };
            ISet<string> all = new HashSet<string>(Language.AllLanguages());
            all.Remove(large);

            string countModelDir = "/home/kodlab/data/language-data/models/counts3";
            Trainer trainer = new Trainer(
                trainingDirs,
                countModelDir,
                3,
                new int[] { 50, 3, 3 });

            trainer.TrainParallel(all);

            Trainer trainer2 = new Trainer(
                trainingDirs,
                countModelDir,
                3,
                new int[] { 50, 30, 30 });
            trainer2.TrainParallel(large);

            trainer = new Trainer(
                trainingDirs,
                countModelDir,
                3,
                new int[] { 30, 2, 1 });
            trainer.TrainParallel(new List<string> { "tr" });

            string compressedModelDir = "/home/kodlab/data/language-data/models/compressed3";
            string[] languages = { "tr", "en", "az", "ku", "ky", "de", "uz" };
            trainer.GenerateModelsToDir(countModelDir, compressedModelDir, languages, true);

            compressedModelDir = "/home/kodlab/data/language-data/models/compressedAll";
            trainer.GenerateModelsToDir(countModelDir, compressedModelDir, Language.AllLanguages(), true);
        }

        private static void Train2gram()
        {
            string[]
            trainingDirs = {
                "/home/kodlab/data/language-data/subtitle",
                "/home/kodlab/data/language-data/wiki"};

            ISet<string> large = new HashSet<string> { "JA", "KO", "ZH", "ML", "HI", "KM", "MY", "EL", "AR" };
            ISet<string> all = new HashSet<string>(Language.AllLanguages());
            all.Remove(large);

            string countModelDir = "/home/kodlab/data/language-data/models/counts2";
            Trainer trainer = new Trainer(
                trainingDirs,
                countModelDir,
                2,
                new int[] { 20, 2 });

            trainer.TrainParallel(all);

            Trainer trainer2 = new Trainer(
                trainingDirs,
                countModelDir,
                2,
                new int[] { 50, 40 });
            trainer2.TrainParallel(large);

            string compressedModelDir = "/home/kodlab/data/language-data/models/compressed2";
            string[] languages = { "tr", "en" };
            trainer.GenerateModelsToDir(countModelDir, compressedModelDir, languages, true);
        }

        public static void TrainSingle(string lang)
        {
            string[]
            trainingDirs = {
                "/home/kodlab/data/language-data/subtitle",
                "/home/kodlab/data/language-data/wiki"  };
            string countModelDir = "/home/kodlab/data/language-data/models/counts3";
            Trainer trainer = new Trainer(
                trainingDirs,
                countModelDir,
                3,
                new int[] { 50, 3, 3 });
            trainer.Train(new List<string> { lang });
        }

        public static void Main(String[] args)
        {
            //train3gram();
            TrainSingle("ky");
        }

        private void MkDir(string dir)
        {
            if (!Directory.Exists(dir))
            {
                if (Directory.CreateDirectory(dir) == null)
                {
                    throw new SystemException("Cannot create dir:" + dir);
                }
                else
                {
                    Console.WriteLine(dir + " is created.");
                }
            }
        }

        public List<string> GetFilesForModel(List<string> languageIds)
        {
            List<string> filesInGroup = new List<string>();
            foreach (string languageId in languageIds)
            {
                string key = languageId.ToLowerInvariant();
                if (langFileMap.ContainsKey(key))
                {
                    filesInGroup.AddRange(langFileMap[languageId]);
                }
                else
                {
                    Console.WriteLine("Language " + languageId + " Does not exist in training data ");
                }
            }
            return filesInGroup;
        }

        public List<string> GetFilesForModel(params string[] languageIds)
        {
            return GetFilesForModel(new List<string>(languageIds));
        }

        public List<string> GetGarbageModelFiles(List<string> excludedLangIds)
        {
            List<string> lowercaseIds = new List<string>();
            foreach (string modelId in excludedLangIds)
            {
                lowercaseIds.Add(modelId.ToLowerInvariant());
            }
            List<string> garbageIds = new List<string>();
            foreach (string id in langFileMap.Keys)
            {
                if (!lowercaseIds.Contains(id))
                {
                    garbageIds.Add(id);
                }
            }
            return garbageIds;
        }

        public void Train(List<string> modelIds)
        {
            if (modelIds.IsEmpty())
            {
                Console.WriteLine("There are no id's provided for training.");
            }
            Console.WriteLine("Order:" + order);
            foreach (string modelId in modelIds)
            {
                string lowerModelId = modelId.ToLowerInvariant();
                Console.WriteLine("Model:" + lowerModelId);
                List<string> filesForModel = GetFilesForModel(lowerModelId);
                Console.WriteLine("Files for model:" + filesForModel);
                ModelGenerator.ModelTrainData td = new ModelGenerator.ModelTrainData(order, lowerModelId,
                    filesForModel, cutOffs);
                Train(td);
            }
        }

        public void TrainParallel(IEnumerable<string> modelIds)
        {
            Parallel.ForEach(modelIds, (modelId) =>
            {
                string id = modelId.ToLowerInvariant();
                Console.WriteLine("Model:" + id);
                List<string> filesForModel = GetFilesForModel(id);
                Console.WriteLine("Files for model:" + filesForModel);
                ModelGenerator.ModelTrainData td = new ModelGenerator.ModelTrainData(order, id,
                    filesForModel, cutOffs);
                Train(td);
                Console.WriteLine("Done:" + id);
            });
        }

        private void Train(ModelGenerator.ModelTrainData td)
        {
            string countFile = Path.Combine(countModelDir, td.modelId + ".count");
            CharNgramCountModel cm = modelGenerator.GetCountModel(td);
            cm.Save(countFile);
        }

        private void TrainWithGarbage(List<string> languages)
        {
            List<string> garbageFiles = GetFilesForModel(GetGarbageModelFiles(languages));
            ModelGenerator.ModelTrainData garbageData = new ModelGenerator.ModelTrainData(3, "unk",
                garbageFiles, cutOffs);
            Train(languages);
            Train(garbageData);
        }

        private void GenerateModelsToDir(string countDir, string modelDir, string[] languages,
            bool compressed)
        {
            LanguageIdentifier identifier = LanguageIdentifier.GenerateFromCounts(countDir, languages);
            List<ICharNgramLanguageModel> models = identifier.GetModels();
            MkDir(modelDir);
            foreach (ICharNgramLanguageModel model in models)
            {
                Console.WriteLine("Generating model for:" + model.GetId());
                MapBasedCharNgramLanguageModel mbm = (MapBasedCharNgramLanguageModel)model;
                if (compressed)
                {
                    string modelFile = Path.Combine(modelDir, model.GetId() + ".clm");
                    CompressedCharNgramModel.Compress(mbm, modelFile);
                }
                else
                {
                    string modelFile = Path.Combine(modelDir, model.GetId() + ".lm");
                    mbm.SaveCustom(modelFile);
                }
            }
        }
    }
}
