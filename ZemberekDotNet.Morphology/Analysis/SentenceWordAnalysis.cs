namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// This is a container method used for result of the morphological disambiguation result.
    /// Use can access best analysis and all analyses and input word information through this class.
    /// </summary>
    public class SentenceWordAnalysis
    {
        /// <summary>
        /// Contains the best analysis.Other analyses are in the wordAnalysis parameter.If there are no
        /// analysis, this will hold an Unknown SingleAnalysis result.
        /// </summary>
        public readonly SingleAnalysis bestAnalysis;

        /// <summary>
        /// This object contains input word information and all analyses of it.Input can be accessed from
        /// wordAnalysis.getInput() If there is no analysis, wordAnalysis will have 0 SingleAnalysis item
        /// in it.
        /// </summary>
        public readonly WordAnalysis wordAnalysis;

        public SentenceWordAnalysis(SingleAnalysis bestAnalysis,
            WordAnalysis wordAnalysis)
        {
            this.bestAnalysis = bestAnalysis;
            this.wordAnalysis = wordAnalysis;
        }

        /// <summary>
        /// Returns the best analysis.Other analyses can be are in the getWordAnalysis() method. If there
        /// are no analysis for the word, this will hold an Unknown SingleAnalysis result.
        /// </summary>
        /// <returns></returns>
        public SingleAnalysis GetBestAnalysis()
        {
            return bestAnalysis;
        }

        /// <summary>
        /// This method returns WordAnalysis object that contains word information and all analyses of it.
        /// Input can be accessed from wordAnalysis.getInput() If there is no analysis, wordAnalysis will
        /// have 0 SingleAnalysis item in it.
        /// </summary>
        /// <returns></returns>
        public WordAnalysis GetWordAnalysis()
        {
            return wordAnalysis;
        }
    }
}
