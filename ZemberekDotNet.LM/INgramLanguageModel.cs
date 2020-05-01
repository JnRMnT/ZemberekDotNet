namespace ZemberekDotNet.LM
{
    /// <summary>
    /// Represents an N-gram language model.
    /// </summary>
    public interface INgramLanguageModel
    {

        /// <summary>
        /// Returns Log uni-gram probability value. id must be in vocabulary limits.
        /// </summary>
        /// <param name="id">word id</param>
        /// <returns>log probability</returns>
        float GetUnigramProbability(int id);

        /// <summary>
        /// Returns If this n-gram exists.
        /// </summary>
        /// <param name="wordIndexes">ngram ids</param>
        /// <returns>log probability</returns>
        bool NGramExists(params int[] wordIndexes);

        /// <summary>
        /// Returns Log N-Gram probability. If this is a back-off model, it makes with necessary back-off
        /// calculations when necessary
        /// </summary>
        /// <param name="ids">word ids.</param>
        /// <returns>log probability</returns>
        float GetProbability(params int[] ids);

        /// <summary>
        /// Returns Log 3-Gram probability. If this is a back-off model, it makes with necessary back-off
        /// calculations when necessary
        /// </summary>
        /// <param name="id0"></param>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        float GetTriGramProbability(int id0, int id1, int id2);

        /// <summary>
        /// Returns Log 3-Gram probability. If this is a back-off model, it makes with necessary back-off
        /// calculations when necessary
        /// </summary>
        /// <param name="id0"></param>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <param name="fingerPrint"></param>
        /// <returns></returns>
        float GetTriGramProbability(int id0, int id1, int id2, int fingerPrint);

        /// <summary>
        /// Order of language model
        /// </summary>
        /// <returns>order value. 1,2,.3 typically.</returns>
        int GetOrder();

        /// <summary>
        /// Vocabulary of this model.
        /// </summary>
        /// <returns>Vocabulary of this model.</returns>
        LmVocabulary GetVocabulary();
    }
}
