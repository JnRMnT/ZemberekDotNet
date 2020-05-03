using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    public class AnalysisDebugData
    {
        public string input;
        public List<StemTransition> candidateStemTransitions = new List<StemTransition>();
        public List<SearchPath> paths = new List<SearchPath>();
        public Dictionary<SearchPath, string> failedPaths = new Dictionary<SearchPath, string>();
        public LinkedHashSet<SearchPath> finishedPaths = new LinkedHashSet<SearchPath>();
        public MultiMap<SearchPath, RejectedTransition> rejectedTransitions = new MultiMap<SearchPath, RejectedTransition>();
        public List<SingleAnalysis> results = new List<SingleAnalysis>();
        public List<SearchPath> resultPaths = new List<SearchPath>();

        List<string> DetailedInfo()
        {
            List<string> l = new List<string>();
            l.Add("----------------------");
            l.Add("Debug data for input = " + input);
            if (candidateStemTransitions.Count == 0)
            {
                l.Add("No Stem Candidates. Analysis Failed.");
            }
            l.Add("Stem Candidate Transitions: ");
            foreach (StemTransition c in candidateStemTransitions)
            {
                l.Add("  " + c.DebugForm());
            }
            l.Add("All paths:");
            foreach (SearchPath path in paths)
            {
                if (failedPaths.ContainsKey(path))
                {
                    l.Add(string.Format("  {0} Fail → {1}", path, failedPaths.GetValueOrDefault(path)));
                }
                else if (finishedPaths.Contains(path))
                {
                    l.Add(string.Format("  {0} Accepted", path));
                }
                else
                {
                    l.Add(string.Format("  {0}", path));
                }
                if (rejectedTransitions.ContainsKey(path))
                {
                    l.Add("    Failed Transitions:");
                    foreach (RejectedTransition r in rejectedTransitions[path])
                    {
                        l.Add("    " + r);
                    }
                }
            }
            l.Add("Paths    [" + input + "] (Surface + Morpheme State):");
            foreach (SearchPath result in resultPaths)
            {
                l.Add("  " + result.ToString());
            }
            l.Add("Analyses [" + input + "] (Surface + Morpheme):");
            foreach (SingleAnalysis result in results)
            {
                l.Add("  " + AnalysisFormatters.SurfaceSequenceFormatter().Format(result));
            }
            return l;
        }

        public void DumpToConsole()
        {
            List<string> l = DetailedInfo();
            l.ForEach(e => Console.WriteLine(e));
        }

        public void DumpToFile(string path)
        {
            File.WriteAllLines(path, DetailedInfo(), Encoding.UTF8);
        }

        public class RejectedTransition
        {
            SuffixTransition transition;
            string reason;

            public RejectedTransition(SuffixTransition transition, String reason)
            {
                this.transition = transition;
                this.reason = reason;
            }

          public override string ToString()
            {
                return transition.ToString() + " " + reason;
            }
        }

    }
}
