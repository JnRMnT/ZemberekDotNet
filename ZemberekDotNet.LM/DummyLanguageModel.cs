namespace ZemberekDotNet.LM
{
    public class DummyLanguageModel : INgramLanguageModel
    {
        readonly LmVocabulary vocabulary;

        public DummyLanguageModel()
        {
            vocabulary = new LmVocabulary.LmVocabularyBuilder().Generate();
        }

        public float GetUnigramProbability(int id)
        {
            return 0;
        }

        public bool NGramExists(params int[] wordIndexes)
        {
            return false;
        }

        public float GetProbability(params int[] ids)
        {
            return 0;
        }

        public float GetTriGramProbability(int id0, int id1, int id2)
        {
            return 0;
        }

        public float GetTriGramProbability(int id0, int id1, int id2, int fingerPrint)
        {
            return 0;
        }

        public int GetOrder()
        {
            return 0;
        }
        
        public LmVocabulary GetVocabulary()
        {
            return vocabulary;
        }
    }
}
