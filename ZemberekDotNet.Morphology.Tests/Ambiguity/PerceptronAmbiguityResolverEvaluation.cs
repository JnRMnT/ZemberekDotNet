using System;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.Data;
using ZemberekDotNet.Morphology.Ambiguity;
using ZemberekDotNet.Morphology.Lexicon;
using static ZemberekDotNet.Morphology.Ambiguity.PerceptronAmbiguityResolverTrainer;

namespace ZemberekDotNet.Morphology.Tests.Ambiguity
{
    public class PerceptronAmbiguityResolverEvaluation
    {
        public static void DummyMain(string[] args)
        {
            string root = "/media/ahmetaa/depo/ambiguity";

            List<string> paths = new List<string> {
                "data/gold/gold1.txt",
                Path.Combine(root,"www.aljazeera.com.tr-rule-result.txt"),
                Path.Combine(root, "wowturkey.com-rule-result.txt"),
                Path.Combine(root, "open-subtitles-tr-2018-rule-result.txt"),
                Path.Combine(root, "sak.train"),
                Path.Combine(root, "www.haberturk.com-rule-result.txt"),
                Path.Combine(root, "www.cnnturk.com-rule-result.txt")};

            string dev = Path.Combine(root, "sak.dev");
            string model = "morphology/src/main/resources/tr/ambiguity/model";
            string modelCompressed = "morphology/src/main/resources/tr/ambiguity/model-compressed";

            TurkishMorphology morphology = TurkishMorphology.Create(
                RootLexicon.Builder().AddTextDictionaryResources(
                    "Resources/tr/master-dictionary.dict",
                    "Resources/tr/non-tdk.dict",
                    "Resources/tr/proper.dict",
                    "Resources/tr/proper-from-corpus.dict",
                    "Resources/tr/abbreviations.dict",
                    "Resources/tr/person-names.dict"
                ).Build());

            DataSet trainingSet = new DataSet();
            foreach (string path in paths)
            {
                trainingSet.Add(DataSet.Load(path, morphology));
            }
            DataSet devSet = DataSet.Load(dev, morphology);

            PerceptronAmbiguityResolver resolver =
                new PerceptronAmbiguityResolverTrainer(morphology).Train(trainingSet, devSet, 7);
            Weights modelTrained = (Weights)resolver.GetModel();
            modelTrained.PruneNearZeroWeights();
            modelTrained.SaveAsText(model);

            Console.WriteLine("Load model and test");

            PerceptronAmbiguityResolver resolverRead =
                PerceptronAmbiguityResolver.FromModelFile(model);
            string test = Path.Combine(root,"sak.test");
            ((Weights)resolverRead.GetModel()).Compress().Serialize(modelCompressed);

            PerceptronAmbiguityResolverTrainer.Test(test, morphology, resolverRead);

            Console.WriteLine("Load compressed model and test");

            PerceptronAmbiguityResolver comp =
                PerceptronAmbiguityResolver.FromModelFile(modelCompressed);
            PerceptronAmbiguityResolverTrainer.Test(test, morphology, comp);
        }
    }
}
