using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// This class holds the result of a morphological analysis and disambiguation results of words in
    /// a sentence.
    /// </summary>
    public class SentenceAnalysis : IEnumerable<SentenceWordAnalysis>
    {
        private String sentence;
        private List<SentenceWordAnalysis> wordAnalyses;

        public SentenceAnalysis(
            String sentence,
            List<SentenceWordAnalysis> wordAnalyses)
        {
            this.sentence = sentence;
            this.wordAnalyses = wordAnalyses;
        }

        public int Size()
        {
            return wordAnalyses.Count;
        }

        public string GetSentence()
        {
            return sentence;
        }

        public IEnumerator<SentenceWordAnalysis> GetEnumerator()
        {
            return wordAnalyses.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns a list of SentenceWordAnalysis objects. Objects holds all and best analysis of a token
        /// in the sentence.
        /// </summary>
        /// <returns>SentenceWordAnalysis list.</returns>
        public List<SentenceWordAnalysis> GetWordAnalyses()
        {
            return wordAnalyses;
        }

        /// <summary>
        /// Returns a list of SentenceWordAnalysis objects. Objects holds all and best analysis of a token
        /// in the sentence.
        /// </summary>
        /// <returns>SentenceWordAnalysis list.</returns>
        [Obsolete("Use GetWordAnalyses(). This will be removed in 0.16.0", false)]
        public List<SentenceWordAnalysis> ParseEntries()
        {
            return wordAnalyses;
        }

        /// <summary>
        /// Returns only the best SingleAnalysis results for each token in the sentence.
        /// If used wants to access word string,
        /// </summary>
        /// <returns></returns>
        public List<SingleAnalysis> BestAnalysis()
        {
            return wordAnalyses.Select(s => s.bestAnalysis).ToList();
        }

        /// <summary>
        /// Returns all analyses of all words as a list.
        /// </summary>
        /// <returns></returns>
        public List<WordAnalysis> AmbiguousAnalysis()
        {
            return wordAnalyses.Select(s => s.wordAnalysis).ToList();
        }

        /// <summary>
        /// Returns all analyses of all words as a list.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use AmbiguousAnalysis(). This will be removed in 0.16.0", false)]
        public List<WordAnalysis> AllAnalyses()
        {
            return wordAnalyses.Select(s => s.wordAnalysis).ToList();
        }
    }
}
