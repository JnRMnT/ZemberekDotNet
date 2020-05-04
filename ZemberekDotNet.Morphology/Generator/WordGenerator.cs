using System;
using System.Collections.Generic;
using System.Linq;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;
using static ZemberekDotNet.Morphology.Analysis.AnalysisDebugData;
using static ZemberekDotNet.Morphology.Analysis.SurfaceTransition;

namespace ZemberekDotNet.Morphology.Generator
{
    /// <summary>
    /// This class is used for generating words from lexical information. For example, a word can be
    /// constructed with only stem and morpheme id's
    /// <p>
    /// Generation algorithm automatically searches through empty morphemes if the are not provided by
    /// the user. For example, for
    /// <pre>
    /// Stem: elma
    /// Morphemes:[Dat]
    /// </pre>
    /// word "elmaya" is generated with elma:Noun+A3sg+ya:Dat analysis.
    /// <p>
    /// This class is not thread-safe if instantiated with forDebug() factory constructor method.
    /// </summary>
    public class WordGenerator
    {
        private IStemTransitions stemTransitions;
        private TurkishMorphotactics morphotactics;
        private bool debugMode = false;
        private AnalysisDebugData debugData;

        public WordGenerator(TurkishMorphotactics morphotactics)
        {
            this.morphotactics = morphotactics;
            this.stemTransitions = morphotactics.GetStemTransitions();
        }

        /// <summary>
        /// Method returns a WordGenerator instance. But when this factory constructor is used, an
        /// AnalysisDebugData object is generated after each call to generation methods. That object cen be
        /// retrieved with getDebugData method.
        /// </summary>
        /// <param name="morphotactics"></param>
        /// <returns></returns>
        public static WordGenerator ForDebug(TurkishMorphotactics morphotactics)
        {
            WordGenerator generator = new WordGenerator(morphotactics);
            generator.debugMode = true;
            return generator;
        }

        public AnalysisDebugData GetDebugData()
        {
            return debugData;
        }

        public List<Result> Generate(string stem, List<string> morphemeIds)
        {
            List<Morpheme> morphemes = new List<Morpheme>();
            foreach (string morphemeId in morphemeIds)
            {
                Morpheme morpheme = TurkishMorphotactics.GetMorpheme(morphemeId);
                morphemes.Add(morpheme);
            }
            List<StemTransition> candidates = stemTransitions.GetPrefixMatches(stem, false);
            return Generate(stem, candidates, morphemes);
        }

        public List<Result> Generate(DictionaryItem item, List<Morpheme> morphemes)
        {
            List<StemTransition> candidates = stemTransitions.GetTransitions(item);
            return Generate(item.id, candidates, morphemes);
        }

        public List<Result> Generate(DictionaryItem item, params Morpheme[] morphemes)
        {
            List<StemTransition> candidates = stemTransitions.GetTransitions(item);
            return Generate(item.id, candidates, morphemes.ToList());
        }

        public List<Result> Generate(DictionaryItem item, params string[] morphemeIds)
        {
            List<StemTransition> candidates = stemTransitions.GetTransitions(item);
            return Generate(item.id, candidates, morphotactics.GetMorphemes(morphemeIds));
        }

        private List<Result> Generate(string input, List<StemTransition> candidates,
            List<Morpheme> morphemes)
        {
            // get stem candidates.

            if (debugMode)
            {
                debugData = new AnalysisDebugData();
                debugData.input = input;
                debugData.candidateStemTransitions.AddRange(candidates);
            }

            // generate initial search paths.
            List<GenerationPath> paths = new List<GenerationPath>();
            foreach (StemTransition candidate in candidates)
            {
                // we set the tail as " " because in morphotactics, some conditions look for tail's size
                // during graph walk. Because this is generation we let that condition pass always.
                SearchPath searchPath = SearchPath.InitialPath(candidate, " ");
                List<Morpheme> morphemesInPath;
                // if input morpheme starts with a POS Morpheme such as Noun etc,
                // we skip it if it matches with the initial morpheme of the graph visiting SearchPath object.
                if (morphemes.Count > 0)
                {
                    if (morphemes[0].Equals(searchPath.GetCurrentState().morpheme))
                    {
                        morphemesInPath = morphemes.GetRange(1, morphemes.Count - 1);
                    }
                    else
                    {
                        morphemesInPath = new List<Morpheme>(morphemes);
                    }
                }
                else
                {
                    morphemesInPath = new List<Morpheme>(0);
                }

                paths.Add(new GenerationPath(searchPath, morphemesInPath));
            }

            // search graph.
            List<GenerationPath> resultPaths = Search(paths);
            // generate results from successful paths.
            List<Result> result = new List<Result>(resultPaths.Count);
            foreach (GenerationPath path in resultPaths)
            {
                SingleAnalysis analysis = SingleAnalysis.FromSearchPath(path.Path);
                result.Add(new Result(analysis.SurfaceForm(), analysis));
                if (debugMode)
                {
                    debugData.results.Add(analysis);
                }
            }
            return result;
        }

        // searches through morphotactics graph.
        private List<GenerationPath> Search(List<GenerationPath> currentPaths)
        {

            List<GenerationPath> result = new List<GenerationPath>(3);
            // new Paths are generated with matching transitions.
            while (currentPaths.Count > 0)
            {

                List<GenerationPath> allNewPaths = new List<GenerationPath>();

                foreach (GenerationPath path in currentPaths)
                {

                    // if there are no more letters to consume and path can be terminated, we accept this
                    // path as a correct result.
                    if (path.Morphemes.Count == 0)
                    {
                        if (path.Path.IsTerminal() &&
                            !path.Path.GetPhoneticAttributes().Contains(PhoneticAttribute.CannotTerminate))
                        {
                            result.Add(path);
                            if (debugMode)
                            {
                                debugData.finishedPaths.Add(path.Path);
                            }
                            continue;
                        }
                        if (debugMode)
                        {
                            debugData.failedPaths.Add(path.Path, "Finished but Path not terminal");
                        }
                    }

                    // Creates new paths with outgoing and matching transitions.
                    List<GenerationPath> newPaths = Advance(path);
                    allNewPaths.AddRange(newPaths);

                    if (debugMode)
                    {
                        if (newPaths.IsEmpty())
                        {
                            debugData.failedPaths.Add(path.Path, "No Transition");
                        }
                        debugData.paths.AddRange(newPaths.Select(s => s.Path));
                    }
                }
                currentPaths = allNewPaths;
            }

            if (debugMode)
            {
                debugData.resultPaths.AddRange(result.Select(s => s.Path));
            }

            return result;
        }

        // for all allowed matching outgoing transitions, new paths are generated.
        // Transition conditions are used for checking if a search path is allowed to pass a transition.
        private List<GenerationPath> Advance(GenerationPath gPath)
        {

            List<GenerationPath> newPaths = new List<GenerationPath>(2);

            // for all outgoing transitions.
            foreach (MorphemeTransition transition in gPath.Path.GetCurrentState().GetOutgoing())
            {
                SuffixTransition suffixTransition = (SuffixTransition)transition;

                // if there are no morphemes and this transitions surface is not empty, no need to check.
                if (gPath.Morphemes.IsEmpty() && suffixTransition.HasSurfaceForm())
                {
                    if (debugMode)
                    {
                        debugData.rejectedTransitions.Add(
                            gPath.Path,
                            new AnalysisDebugData.RejectedTransition(suffixTransition, "Empty surface expected."));
                    }
                    continue;
                }

                // check morpheme match.
                // if transition surface is empty, here will pass.
                if (!gPath.Matches(suffixTransition))
                {
                    if (debugMode)
                    {
                        debugData.rejectedTransitions.Add(
                            gPath.Path,
                            new RejectedTransition(suffixTransition,
                                "Morpheme mismatch." + suffixTransition.to.morpheme));
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
                        failed = ((CombinedCondition)condition).GetFailingCondition(gPath.Path);
                    }
                    else
                    {
                        failed = condition.Accept(gPath.Path) ? null : condition;
                    }
                    if (failed != null)
                    {
                        debugData.rejectedTransitions.Add(
                            gPath.Path,
                            new RejectedTransition(suffixTransition, "Condition → " + failed.ToString()));
                    }
                }

                // check conditions.
                if (!suffixTransition.CanPass(gPath.Path))
                {
                    continue;
                }

                // epsilon transition. Add and continue. Use existing attributes.
                if (!suffixTransition.HasSurfaceForm())
                {
                    SearchPath pCopy = gPath.Path.GetCopyForGeneration(
                        new SurfaceTransition("", suffixTransition),
                        gPath.Path.GetPhoneticAttributes());
                    newPaths.Add(gPath.Copy(pCopy));
                    continue;
                }

                string surface = SurfaceTransition.GenerateSurface(
                    suffixTransition,
                    gPath.Path.GetPhoneticAttributes());

                SurfaceTransition surfaceTransition = new SurfaceTransition(surface, suffixTransition);

                //if tail is equal to surface, no need to calculate phonetic attributes.
                AttributeSet<PhoneticAttribute> attributes =
                    AttributesHelper.GetMorphemicAttributes(surface.ToCharArray(), gPath.Path.GetPhoneticAttributes());

                // This is required for suffixes like `cik` and `ciğ`
                // an extra attribute is added if "cik" or "ciğ" is generated and matches the tail.
                // if "cik" is generated, ExpectsConsonant attribute is added, so only a consonant starting
                // suffix can follow. Likewise, if "ciğ" is produced, a vowel starting suffix is allowed.
                attributes.Remove(PhoneticAttribute.CannotTerminate);
                SuffixTemplateToken lastToken = suffixTransition.GetLastTemplateToken();
                if (lastToken.GetTokenType() == TemplateTokenType.LAST_VOICED)
                {
                    attributes.Add(PhoneticAttribute.ExpectsConsonant);
                }
                else if (lastToken.GetTokenType() == TemplateTokenType.LAST_NOT_VOICED)
                {
                    attributes.Add(PhoneticAttribute.ExpectsVowel);
                    attributes.Add(PhoneticAttribute.CannotTerminate);
                }

                SearchPath p = gPath.Path.GetCopyForGeneration(
                    surfaceTransition,
                    attributes);
                newPaths.Add(gPath.Copy(p));
            }
            return newPaths;
        }

        public class Result
        {
            public readonly string surface;
            public readonly SingleAnalysis analysis;

            public Result(string surface, SingleAnalysis analysis)
            {
                this.surface = surface;
                this.analysis = analysis;
            }

            public override string ToString()
            {
                return surface + "-" + analysis;
            }
        }

        internal class GenerationPath
        {
            SearchPath path;
            List<Morpheme> morphemes;

            public GenerationPath(SearchPath path,
                List<Morpheme> morphemes)
            {
                this.Path = path;
                this.Morphemes = morphemes;
            }

            public SearchPath Path { get => path; set => path = value; }
            public List<Morpheme> Morphemes { get => morphemes; set => morphemes = value; }

            public GenerationPath Copy(SearchPath path)
            {
                SurfaceTransition lastTransition = path.GetLastTransition();
                Morpheme m = lastTransition.GetMorpheme();

                if (lastTransition.surface.IsEmpty())
                {
                    if (Morphemes.Count == 0)
                    {
                        return new GenerationPath(path, Morphemes);
                    }
                    if (m.Equals(Morphemes[0]))
                    {
                        return new GenerationPath(path, Morphemes.GetRange(1, Morphemes.Count - 1));
                    }
                    else
                    {
                        return new GenerationPath(path, Morphemes);
                    }
                }
                if (!m.Equals(Morphemes[0]))
                {
                    throw new InvalidOperationException(
                        "Cannot generate Generation copy because transition morpheme and first morpheme to consume"
                            + " does not match.");
                }
                return new GenerationPath(path, Morphemes.GetRange(1, Morphemes.Count - 1));

            }

            internal bool Matches(SuffixTransition transition)
            {
                if (!transition.HasSurfaceForm())
                {
                    return true;
                }
                if (Morphemes.Count > 0 && transition.to.morpheme.Equals(Morphemes[0]))
                {
                    return true;
                }
                return false;
            }

        }
    }
}
