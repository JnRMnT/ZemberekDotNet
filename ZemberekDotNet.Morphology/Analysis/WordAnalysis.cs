using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZemberekDotNet.Morphology.Analysis
{
    public class WordAnalysis : IEnumerable<SingleAnalysis>
    {
        public static readonly WordAnalysis EmptyInputResult =
      new WordAnalysis("", new List<SingleAnalysis>());

        //this is actual input.
        string input;
        // this is the input that is prepared for analysis.
        string normalizedInput;

        List<SingleAnalysis> analysisResults;

        public WordAnalysis(string input, List<SingleAnalysis> analysisResults)
        {
            this.input = input;
            this.normalizedInput = input;
            this.analysisResults = analysisResults;
        }

        public WordAnalysis(string input, string normalizedInput, List<SingleAnalysis> analysisResults)
        {
            this.input = input;
            this.normalizedInput = normalizedInput;
            this.analysisResults = analysisResults;
        }

        public string GetInput()
        {
            return input;
        }

        public string GetNormalizedInput()
        {
            return normalizedInput;
        }

        public bool IsCorrect()
        {
            return analysisResults.Count > 0 && !analysisResults[0].IsUnknown();
        }

        public int AnalysisCount()
        {
            return analysisResults.Count;
        }

        public IEnumerator<SingleAnalysis> GetEnumerator()
        {
            return analysisResults.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IQueryable<SingleAnalysis> Stream()
        {
            return analysisResults.AsQueryable();
        }

        public WordAnalysis CopyFor(List<SingleAnalysis> analyses)
        {
            return new WordAnalysis(this.input, this.normalizedInput, analyses);
        }

        public List<SingleAnalysis> GetAnalysisResults()
        {
            return analysisResults;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || !GetType().Equals(o.GetType()))
            {
                return false;
            }

            WordAnalysis analyses = (WordAnalysis)o;

            if (!input.Equals(analyses.input))
            {
                return false;
            }
            if (!normalizedInput.Equals(analyses.normalizedInput))
            {
                return false;
            }
            return analysisResults.Equals(analyses.analysisResults);
        }

        public override int GetHashCode()
        {
            int result = input.GetHashCode();
            result = 31 * result + normalizedInput.GetHashCode();
            result = 31 * result + analysisResults.GetHashCode();
            return result;
        }

        public override string ToString()
        {
            return "WordAnalysis{" +
                "input='" + input + '\'' +
                ", normalizedInput='" + normalizedInput + '\'' +
                ", analysisResults=" + analysisResults +
                '}';
        }
    }
}
