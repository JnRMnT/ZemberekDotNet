namespace ZemberekDotNet.LangID.Model
{
    /// <summary>
    /// Character NGram model interface
    /// </summary>
    public interface ICharNgramLanguageModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gram">calculates log probability of a gram from this model.</param>
        /// <returns>natural log probability value.</returns>
        double GramProbability(string gram);

        /// <summary>
        /// Order of the model (usually 2,3,.)
        /// </summary>
        /// <returns>order</returns>
        int GetOrder();

        /// <summary>
        /// model identifier String
        /// </summary>
        /// <returns>id.</returns>
        string GetId();
    }
}
