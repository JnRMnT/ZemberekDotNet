using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.NER;

namespace ZemberekDotNet.Examples.NER
{
    /// <summary>
    /// Demonstrates ZemberekDotNet.NER (Named Entity Recognition):
    ///   1. Annotate a small training corpus in OpenNLP format
    ///   2. Train a Perceptron NER model in memory (no file I/O needed at runtime)
    ///   3. Run inference to find PERSON, LOCATION and ORGANIZATION entities
    ///
    /// No Java or JVM required — pure .NET Standard 2.1.
    /// </summary>
    public class NERExamples
    {
        public static void Main()
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            TrainAndIdentify();
            Console.ReadLine();
        }

        // ------------------------------------------------------------------
        // Train a Perceptron NER model on annotated sentences; then find
        // named entities in unseen sentences and print them with their type.
        // ------------------------------------------------------------------
        static void TrainAndIdentify()
        {
            Console.WriteLine("=== Named Entity Recognition ===");

            // Training data: OpenNLP bracket format
            // <START:TYPE> tokens <END>
            string[] trainingLines =
            {
                "<START:PERSON> Ahmet Yılmaz <END> dün İstanbul'a gitti .",
                "<START:LOCATION> Ankara <END> Türkiye'nin başkentidir .",
                "<START:PERSON> Mehmet Demir <END> yeni bir proje başlattı .",
                "<START:LOCATION> İzmir <END> Ege'nin incisidir .",
                "<START:PERSON> Ayşe Kaya <END> konferansa katıldı .",
                "<START:ORGANIZATION> TBMM <END> yeni yasayı onayladı .",
                "<START:LOCATION> İstanbul <END> en kalabalık şehirdir .",
                "<START:PERSON> Ali Çelik <END> şirketi kurdu .",
                "<START:ORGANIZATION> Türk Telekom <END> duyurusunu yaptı .",
                "<START:LOCATION> Bursa <END> önemli bir sanayi şehridir .",
                "<START:PERSON> Fatma Şahin <END> belediye başkanlığı görevini sürdürüyor .",
                "<START:ORGANIZATION> Türkiye Büyük Millet Meclisi <END> toplantı yaptı .",
            };

            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(tempFile, trainingLines);
                NerDataSet trainingSet = NerDataSet.Load(tempFile, NerDataSet.AnnotationStyle.OPEN_NLP);

                TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
                PerceptronNerTrainer trainer = new PerceptronNerTrainer(morphology);
                PerceptronNer ner = trainer.Train(trainingSet, trainingSet, 20, 0.1f);

                IEnumerable<string> entityTypes = trainingSet.TypeIds
                    .Where(t => !t.Contains("OUT"))
                    .Select(t => t.Split('-')[0])
                    .Distinct()
                    .OrderBy(t => t);

                Console.WriteLine($"  Trained on {trainingSet.Sentences.Count} sentences.");
                Console.WriteLine($"  Entity types: {string.Join(", ", entityTypes)}");
                Console.WriteLine();

                string[] testSentences =
                {
                    "Mustafa gün boyu Ankara'da çalıştı .",
                    "TBMM yeni bütçeyi görüşüyor .",
                    "Ali ve Ayşe İzmir'e gidiyorlar .",
                };

                foreach (string sentence in testSentences)
                {
                    Console.WriteLine($"  Input : {sentence}");
                    NerSentence result = ner.FindNamedEntities(sentence);
                    List<NamedEntity> entities = result.GetNamedEntities();

                    if (entities.Count == 0)
                    {
                        Console.WriteLine("    → (no named entities detected)");
                    }
                    else
                    {
                        foreach (NamedEntity entity in entities)
                        {
                            string type = entity.GetTokens()[0].GetTokenType();
                            Console.WriteLine($"    → [{type,-14}] \"{entity.Content()}\"");
                        }
                    }
                    Console.WriteLine();
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
