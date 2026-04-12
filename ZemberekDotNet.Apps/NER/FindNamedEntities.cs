using Commander.NET.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.NER;

namespace ZemberekDotNet.Apps.NER
{
    public class FindNamedEntities : NerAppBase<FindNamedEntities>
    {
        [Parameter("--modelRoot", "-m", Required = Required.Yes,
            Description = "NER model root directory.")]
        string modelRoot;

        [Parameter("--input", "-i", Required = Required.Yes,
            Description = "Input text file path.")]
        string inputPath;

        [Parameter("--output", "-o",
            Description = "Output path. If not provided, [input].ner.out is generated.")]
        string outputPath;

        public override string Description()
        {
            return "Finds named entities from a Turkish text file.";
        }

        public override void Run()
        {
            if (!Directory.Exists(modelRoot))
            {
                throw new DirectoryNotFoundException("Model root not found: " + modelRoot);
            }
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException("Input file not found: " + inputPath);
            }

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = inputPath + ".ner.out";
            }

            PerceptronNer ner = PerceptronNer.LoadModel(modelRoot, CreateMorphology());

            using (FileStream stream = File.Open(outputPath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (string line in File.ReadLines(inputPath, Encoding.UTF8))
                {
                    string sentence = line?.Trim();
                    if (string.IsNullOrWhiteSpace(sentence))
                    {
                        continue;
                    }

                    NerSentence result = ner.FindNamedEntities(sentence);
                    List<NamedEntity> entities = result.GetNamedEntities();

                    writer.WriteLine(sentence);
                    if (entities.Count == 0)
                    {
                        writer.WriteLine("(no named entities)");
                    }
                    else
                    {
                        foreach (NamedEntity entity in entities)
                        {
                            string type = entity.GetTokens()[0].GetTokenType();
                            writer.WriteLine("[" + type + "] " + entity.Content());
                        }
                    }
                    writer.WriteLine();
                }
            }

            Console.WriteLine("Named entity extraction completed.");
            Console.WriteLine("Output: " + outputPath);
        }

        public static void Main(string[] args)
        {
            new FindNamedEntities().Execute(args);
        }
    }
}