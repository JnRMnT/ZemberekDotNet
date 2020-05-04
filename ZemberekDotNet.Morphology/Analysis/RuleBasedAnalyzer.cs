using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;
using static ZemberekDotNet.Morphology.Analysis.AnalysisDebugData;
using static ZemberekDotNet.Morphology.Analysis.SurfaceTransition;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// This is a Morphological Analyzer implementation.Instances of this class are not thread safe if
    /// instantiated with forDebug() factory constructor method.
    /// </summary>
    public class RuleBasedAnalyzer
    {
        private static readonly int MaxRepeatingSuffixTypeCount = 3;

        private RootLexicon lexicon;
        private IStemTransitions stemTransitions;
        private bool debugMode = false;
        private AnalysisDebugData debugData;
        private bool asciiTolerant = false;
        private TurkishMorphotactics morphotactics;

        private RuleBasedAnalyzer(TurkishMorphotactics morphotactics)
        {
            this.lexicon = morphotactics.GetRootLexicon();
            this.stemTransitions = morphotactics.GetStemTransitions();
            this.morphotactics = morphotactics;
        }

        public TurkishMorphotactics GetMorphotactics()
        {
            return morphotactics;
        }

        public static RuleBasedAnalyzer Instance(TurkishMorphotactics morphotactics)
        {
            return new RuleBasedAnalyzer(morphotactics);
        }

        /// <summary>
        /// Generates a RuleBasedAnalyzer instance that ignores the diacritic marks from the input. As a
        /// result, for input `siraci` or `şıraçi`  it generates both analyses "sıracı, şıracı"
        /// </summary>
        /// <param name="morphotactics"></param>
        /// <returns></returns>
        public static RuleBasedAnalyzer IgnoreDiacriticsInstance(
            TurkishMorphotactics morphotactics)
        {
            RuleBasedAnalyzer analyzer = RuleBasedAnalyzer.Instance(morphotactics);
            analyzer.asciiTolerant = true;
            return analyzer;
        }

        /// <summary>
        /// Method returns an RuleBasedAnalyzer instance. But when this factory constructor is used, an
        /// AnalysisDebugData object is generated after each call to generation methods. That object cen be
        /// retrieved with getDebugData method.
        /// </summary>
        /// <param name="morphotactics"></param>
        /// <returns></returns>
        public static RuleBasedAnalyzer ForDebug(TurkishMorphotactics morphotactics)
        {
            RuleBasedAnalyzer analyzer = RuleBasedAnalyzer.Instance(morphotactics);
            analyzer.debugMode = true;
            return analyzer;
        }

        public static RuleBasedAnalyzer ForDebug(
            TurkishMorphotactics morphotactics,
            bool asciiTolerant)
        {
            RuleBasedAnalyzer analyzer = RuleBasedAnalyzer
                .Instance(morphotactics);
            analyzer.debugMode = true;
            analyzer.asciiTolerant = asciiTolerant;
            return analyzer;
        }

        public IStemTransitions GetStemTransitions()
        {
            return stemTransitions;
        }

        public RootLexicon GetLexicon()
        {
            return lexicon;
        }

        public AnalysisDebugData GetDebugData()
        {
            return debugData;
        }

        public List<SingleAnalysis> Analyze(string input)
        {
            if (debugMode)
            {
                debugData = new AnalysisDebugData();
            }
            // get stem candidates.
            List<StemTransition> candidates = stemTransitions.GetPrefixMatches(input, asciiTolerant);

            if (debugMode)
            {
                debugData.input = input;
                debugData.candidateStemTransitions.AddRange(candidates);
            }

            // generate initial search paths.
            List<SearchPath> paths = new List<SearchPath>();
            foreach (StemTransition candidate in candidates)
            {
                int length = candidate.surface.Length;
                String tail = input.Substring(length);
                paths.Add(SearchPath.InitialPath(candidate, tail));
            }

            // search graph.
            List<SearchPath> resultPaths = Search(paths);

            // generate results from successful paths.
            List<SingleAnalysis> result = new List<SingleAnalysis>(resultPaths.Count);
            foreach (SearchPath path in resultPaths)
            {
                SingleAnalysis analysis = SingleAnalysis.FromSearchPath(path);
                result.Add(analysis);
                if (debugMode)
                {
                    debugData.results.Add(analysis);
                }
            }
            return result;
        }

        // searches through morphotactics graph.
        private List<SearchPath> Search(List<SearchPath> currentPaths)
        {

            if (currentPaths.Count > 30)
            {
                currentPaths = PruneCyclicPaths(currentPaths);
            }

            List<SearchPath> result = new List<SearchPath>(3);
            // new Paths are generated with matching transitions.
            while (currentPaths.Count > 0)
            {

                List<SearchPath> allNewPaths = new List<SearchPath>();

                foreach (SearchPath path in currentPaths)
                {

                    // if there are no more letters to consume and path can be terminated, we accept this
                    // path as a correct result.
                    if (path.tail.Length == 0)
                    {
                        if (path.IsTerminal() &&
                            !path.ContainsPhoneticAttribute(PhoneticAttribute.CannotTerminate))
                        {
                            result.Add(path);
                            if (debugMode)
                            {
                                debugData.finishedPaths.Add(path);
                            }
                            continue;
                        }
                        if (debugMode)
                        {
                            debugData.failedPaths.Add(path, "Finished but Path not terminal");
                        }
                    }

                    // Creates new paths with outgoing and matching transitions.
                    List<SearchPath> newPaths = Advance(path);
                    allNewPaths.AddRange(newPaths);

                    if (debugMode)
                    {
                        if (newPaths.IsEmpty())
                        {
                            debugData.failedPaths[path] = "No Transition";
                        }
                        debugData.paths.AddRange(newPaths);
                    }
                }
                currentPaths = allNewPaths;
            }

            if (debugMode)
            {
                debugData.resultPaths.AddRange(result);
            }

            return result;
        }

        /// <summary>
        /// for all allowed matching outgoing transitions, new paths are generated.
        /// Transition `conditions` are used for checking if a `search path`
        /// is allowed to pass a transition.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<SearchPath> Advance(SearchPath path)
        {
            List<SearchPath> newPaths = new List<SearchPath>(2);

            // for all outgoing transitions.
            foreach (MorphemeTransition transition in path.currentState.GetOutgoing())
            {

                SuffixTransition suffixTransition = (SuffixTransition)transition;

                // if tail is empty and this transitions surface is not empty, no need to check.
                if (path.tail.IsEmpty() && suffixTransition.HasSurfaceForm())
                {
                    if (debugMode)
                    {
                        debugData.rejectedTransitions.Add(
                            path,
                            new RejectedTransition(suffixTransition, "Empty surface expected."));
                    }
                    continue;
                }

                string surface = SurfaceTransition.GenerateSurface(
                    suffixTransition,
                    path.phoneticAttributes);

                // no need to go further if generated surface form is not a prefix of the paths's tail.
                bool tailStartsWith =
                    asciiTolerant ?
                        TurkishAlphabet.Instance.StartsWithIgnoreDiacritics(path.tail, surface) :
                        path.tail.StartsWith(surface);
                if (!tailStartsWith)
                {
                    if (debugMode)
                    {
                        debugData.rejectedTransitions.Add(
                            path,
                            new RejectedTransition(suffixTransition, "Surface Mismatch:" + surface));
                    }
                    continue;
                }

                // if transition condition fails, add it to debug data.
                if (debugMode && suffixTransition.GetCondition() != null)
                {
                    ICondition condition = suffixTransition.GetCondition();
                    ICondition failed;
                    if (condition is CombinedCondition)
                    {
                        failed = ((CombinedCondition)condition).GetFailingCondition(path);
                    }
                    else
                    {
                        failed = condition.Accept(path) ? null : condition;
                    }
                    if (failed != null)
                    {
                        debugData.rejectedTransitions.Add(
                            path,
                            new RejectedTransition(suffixTransition, "Condition → " + failed.ToString()));
                    }
                }

                // check conditions.
                if (!suffixTransition.CanPass(path))
                {
                    continue;
                }

                // epsilon (empty) transition. Add and continue. Use existing attributes.
                if (!suffixTransition.HasSurfaceForm())
                {
                    newPaths.Add(path.GetCopy(
                        new SurfaceTransition("", suffixTransition),
                        path.phoneticAttributes));
                    continue;
                }

                SurfaceTransition surfaceTransition = new SurfaceTransition(surface, suffixTransition);

                //if tail is equal to surface, no need to calculate phonetic attributes.
                bool tailEqualsSurface = asciiTolerant ?
                    TurkishAlphabet.Instance.EqualsIgnoreDiacritics(path.tail, surface)
                    : path.tail.Equals(surface);
                AttributeSet<PhoneticAttribute> attributes = tailEqualsSurface ?
                    path.phoneticAttributes.Copy() :
                    AttributesHelper.GetMorphemicAttributes(surface.ToCharArray(), path.phoneticAttributes);

                // This is required for suffixes like `cik` and `ciğ`
                // an extra attribute is added if "cik" or "ciğ" is generated and matches the tail.
                // if "cik" is generated, ExpectsConsonant attribute is added, so only a consonant starting
                // suffix can follow. Likewise, if "ciğ" is produced, a vowel starting suffix is allowed.
                attributes.Remove(PhoneticAttribute.CannotTerminate);
                SuffixTemplateToken lastToken = suffixTransition.GetLastTemplateToken();
                if (lastToken?.type == TemplateTokenType.LAST_VOICED)
                {
                    attributes.Add(PhoneticAttribute.ExpectsConsonant);
                }
                else if (lastToken?.type == TemplateTokenType.LAST_NOT_VOICED)
                {
                    attributes.Add(PhoneticAttribute.ExpectsVowel);
                    attributes.Add(PhoneticAttribute.CannotTerminate);
                }

                SearchPath p = path.GetCopy(
                    surfaceTransition,
                    attributes);
                newPaths.Add(p);
            }
            return newPaths;
        }

        // for preventing excessive branching during search, we remove paths that has more than
        // MAX_REPEATING_SUFFIX_TYPE_COUNT morpheme-state types.
        private List<SearchPath> PruneCyclicPaths(List<SearchPath> tokens)
        {
            List<SearchPath> result = new List<SearchPath>();
            foreach (SearchPath token in tokens)
            {
                bool remove = false;
                IntValueMap<string> typeCounts = new IntValueMap<string>(10);
                foreach (SurfaceTransition node in token.transitions)
                {
                    if (typeCounts.AddOrIncrement(node.GetState().id) > MaxRepeatingSuffixTypeCount)
                    {
                        remove = true;
                        break;
                    }
                }
                if (!remove)
                {
                    result.Add(token);
                }
            }
            return result;
        }
    }
}
