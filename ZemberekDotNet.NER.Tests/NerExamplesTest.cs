using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.NER;

namespace ZemberekDotNet.NER.Tests
{
    [TestClass]
    public class NerExamplesTest
    {
        private static NerDataSet GetTrainingSet()
        {
            string[] trainingLines =
            {
                "<START:PERSON> Ahmet Yılmaz <END> dün İstanbul'a gitti .",
                "<START:LOCATION> Ankara <END> Türkiye'nin başkentidir .",
                "<START:PERSON> Mehmet Demir <END> yeni bir proje başlattı .",
                "<START:LOCATION> İzmir <END> Ege'nin incisidir .",
                "<START:ORGANIZATION> TBMM <END> yeni yasayı onayladı .",
                "<START:ORGANIZATION> Türk Telekom <END> duyurusunu yaptı .",
            };

            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(tempFile, trainingLines);
                return NerDataSet.Load(tempFile, NerDataSet.AnnotationStyle.OPEN_NLP);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void TrainingDataContainsExpectedEntityTypes()
        {
            NerDataSet training = GetTrainingSet();
            bool hasPerson = training.TypeIds.Any(t => t.Contains("PERSON"));
            bool hasLocation = training.TypeIds.Any(t => t.Contains("LOCATION"));
            bool hasOrganization = training.TypeIds.Any(t => t.Contains("ORGANIZATION"));

            Assert.IsTrue(hasPerson);
            Assert.IsTrue(hasLocation);
            Assert.IsTrue(hasOrganization);
        }

        [TestMethod]
        public void TrainedModelCanRunInferenceForSimpleSentence()
        {
            NerDataSet training = GetTrainingSet();
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            PerceptronNer ner = new PerceptronNerTrainer(morphology)
                .Train(training, training, 12, 0.1f);

            NerSentence result = ner.FindNamedEntities("Ahmet Ankara'ya gitti .");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.GetAllEntities().Count > 0);
        }

        [TestMethod]
        public void TrainedModelProducesTokenLevelPredictions()
        {
            NerDataSet training = GetTrainingSet();
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            PerceptronNer ner = new PerceptronNerTrainer(morphology)
                .Train(training, training, 12, 0.1f);

            NerSentence result = ner.FindNamedEntities("Mehmet İzmir'e gitti .");
            Assert.IsNotNull(result);
            Assert.IsTrue(result.GetAllEntities().Count > 0);

            foreach (NamedEntity entity in result.GetAllEntities())
            {
                Assert.IsTrue(entity.GetTokens().Count > 0);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entity.GetTokens()[0].GetTokenType()));
            }
        }

        [TestMethod]
        public void TrainedModelDetectsOrganizationToken()
        {
            NerDataSet training = GetTrainingSet();
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            PerceptronNer ner = new PerceptronNerTrainer(morphology)
                .Train(training, training, 12, 0.1f);

            NerSentence result = ner.FindNamedEntities("TBMM bugün toplandı .");
            List<string> detectedTypes = result.GetNamedEntities()
                .Select(ne => ne.GetTokens()[0].GetTokenType())
                .ToList();

            CollectionAssert.Contains(detectedTypes, "ORGANIZATION");
        }

        [TestMethod]
        public void TrainedModelReturnsNoEntitiesForNeutralSentence()
        {
            NerDataSet training = GetTrainingSet();
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            PerceptronNer ner = new PerceptronNerTrainer(morphology)
                .Train(training, training, 12, 0.1f);

            NerSentence result = ner.FindNamedEntities("hava bugün serin .");
            Assert.AreEqual(0, result.GetNamedEntities().Count);
        }
    }
}
