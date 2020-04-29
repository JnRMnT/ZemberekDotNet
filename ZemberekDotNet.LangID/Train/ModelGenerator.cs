using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.LangID.Model;

namespace ZemberekDotNet.LangID.Train
{
    public class ModelGenerator
    {
        readonly ISet<string> ignoreWords = new HashSet<string> { "://", "wikipedia", ".jpg", "image:", "file:", ".png" };

        public MapBasedCharNgramLanguageModel GenerateModel(ModelTrainData modelData)
        {
            Console.WriteLine("Training for:" + modelData.modelId + " Training files: " + modelData.modelFiles);
            CharNgramCountModel cm = GetCountModel(modelData);
            return MapBasedCharNgramLanguageModel.Train(cm);
        }

        public void CompressModelToFile(MapBasedCharNgramLanguageModel model, string compressedFile)
        {
            Console.WriteLine("Compressing:" + model.id);
            CompressedCharNgramModel.Compress(model, compressedFile);
        }

        public CharNgramCountModel GetCountModel(ModelTrainData modelTrainData)
        {
            CharNgramCountModel countModel = new CharNgramCountModel(modelTrainData.modelId,
                modelTrainData.order);
            foreach (string file in modelTrainData.modelFiles)
            {
                Console.WriteLine("Processing file:" + file);
                int ignoredCount = 0;
                ISet<string> lines = new HashSet<string>(File.ReadAllLines(file, Encoding.UTF8));
                foreach (string line in lines)
                {
                    string lowerLine = line.ToLowerInvariant();
                    bool ignore = false;
                    foreach (string ignoreWord in ignoreWords)
                    {
                        if (lowerLine.Contains(ignoreWord))
                        {
                            ignore = true;
                            ignoredCount++;
                            break;
                        }
                    }
                    if (!ignore)
                    {
                        lowerLine = LanguageIdentifier.Preprocess(lowerLine);
                        countModel.AddGrams(lowerLine);
                    }
                }
                Console.WriteLine("Ignored lines for " + file + " : " + ignoredCount);
            }
            countModel.ApplyCutOffs(modelTrainData.cutOffs);
            countModel.DumpGrams(1);
            return countModel;
        }

        public void GenerateCountModelToDirectory(string outDir, List<ModelTrainData> modelTrainDataList)
        {
            foreach (ModelTrainData modelTrainData in modelTrainDataList)
            {
                CharNgramCountModel countModel = GetCountModel(modelTrainData);
                string modelFile = Path.Combine(outDir, modelTrainData.modelId + ".count");
                countModel.Save(modelFile);
            }
        }

        /// <summary>
        /// Defines the training data of a model. It has a unique id and can have multiple training files.
        /// </summary>
        public class ModelTrainData
        {
            internal int order;
            internal string modelId;
            internal List<string> modelFiles;
            internal int[] cutOffs;

            public ModelTrainData(int order, string modelId, List<string> modelFiles)
            {
                this.order = order;
                this.modelId = modelId;
                this.modelFiles = modelFiles;
            }

            public ModelTrainData(int order, string modelId, List<string> modelFiles, int[] cutOffs)
            {
                this.order = order;
                this.modelId = modelId;
                this.modelFiles = modelFiles;
                this.cutOffs = cutOffs;
            }

            public ModelTrainData(int order, string modelId, params string[] modelFiles) : this(order, modelId, new List<string>(modelFiles))
            {

            }

            public void SetCutOffs(int[] cutOffs)
            {
                this.cutOffs = cutOffs;
            }
        }
    }
}