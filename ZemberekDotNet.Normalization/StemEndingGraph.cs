using System.Collections.Generic;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Normalization
{
    /// <summary>
    /// This is a data structure that can be used for spell checking purposes.This is a graph consist of
    /// two trie data structures.One for stems, other for endings.Stem leaf nodes are connected to the
    /// ending graph root.
    /// </summary>
    public class StemEndingGraph
    {
        CharacterGraph stemGraph;
        private CharacterGraph endingGraph;

        private TurkishMorphology morphology;

        public CharacterGraph StemGraph { get => stemGraph; set => stemGraph = value; }
        public CharacterGraph EndingGraph { get => endingGraph; set => endingGraph = value; }
        public TurkishMorphology Morphology { get => morphology; set => morphology = value; }

        public StemEndingGraph(TurkishMorphology morphology)
        {
            this.Morphology = morphology;
            List<string> endings = TextIO.LoadLines("Resources/endings");
            this.EndingGraph = GenerateEndingGraph(endings);
            this.StemGraph = GenerateStemGraph();
            ISet<Node> stemWordNodes = StemGraph.GetAllNodes(n => n.word != null);
            foreach (Node node in stemWordNodes)
            {
                node.ConnectEpsilon(EndingGraph.GetRoot());
            }
        }

        public StemEndingGraph(TurkishMorphology morphology, List<string> endings)
        {
            this.Morphology = morphology;
            this.EndingGraph = GenerateEndingGraph(endings);
            this.StemGraph = GenerateStemGraph();
            ISet<Node> leafNodes = StemGraph.GetAllNodes(n => n.word != null);
            foreach (Node leafNode in leafNodes)
            {
                leafNode.ConnectEpsilon(EndingGraph.GetRoot());
            }
        }

        List<string> GetEndingsFromVocabulary(List<string> words)
        {
            Histogram<string> endings = new Histogram<string>(words.Count / 10);
            foreach (string word in words)
            {
                WordAnalysis analyses = Morphology.Analyze(word);
                foreach (SingleAnalysis analysis in analyses)
                {
                    if (analysis.IsUnknown())
                    {
                        continue;
                    }
                    StemAndEnding se = analysis.GetStemAndEnding();
                    if (se.ending.Length > 0)
                    {
                        endings.Add(se.ending);
                    }
                }
            }
            return endings.GetSortedList(Turkish.StringComparatorAsc);
        }

        private CharacterGraph GenerateEndingGraph(List<string> endings)
        {
            CharacterGraph graph = new CharacterGraph();
            foreach (string ending in endings)
            {
                graph.AddWord(ending, Node.TypeEnding);
            }
            return graph;
        }

        private CharacterGraph GenerateStemGraph()
        {
            CharacterGraph stemGraph = new CharacterGraph();
            IStemTransitions stemTransitions = Morphology.GetMorphotactics().GetStemTransitions();
            foreach (StemTransition transition in stemTransitions.GetTransitions())
            {
                if (transition.surface.Length == 0 ||
                    transition.item.primaryPos == PrimaryPos.Punctuation)
                {
                    continue;
                }
                stemGraph.AddWord(transition.surface, Node.TypeWord);
            }
            return stemGraph;
        }
    }
}
