using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Lexicon.TR;
using ZemberekDotNet.Morphology.Morphotactics;
using static ZemberekDotNet.Morphology.Analysis.SingleAnalysis;

namespace ZemberekDotNet.Morphology.Tests.Analysis
{
    public class AnalyzerTestBase
    {
        public static readonly bool PRINT_RESULTS_TO_SCREEN = false;

        public static TurkishMorphotactics GetMorphotactics(params string[] dictionaryLines)
        {
            RootLexicon lexicon = TurkishDictionaryLoader.Load(dictionaryLines);
            return new TurkishMorphotactics(lexicon);
        }

        internal static RuleBasedAnalyzer GetAnalyzer(params string[] dictionaryLines)
        {
            return RuleBasedAnalyzer.ForDebug(GetMorphotactics(dictionaryLines));
        }

        internal static RuleBasedAnalyzer GetAnalyzer(TurkishMorphotactics morphotactics)
        {
            return RuleBasedAnalyzer.ForDebug(morphotactics);
        }

        internal static AnalysisTester GetTester(params string[] dictionaryLines)
        {
            return new AnalysisTester(RuleBasedAnalyzer.ForDebug(GetMorphotactics(dictionaryLines)));
        }

        internal static AnalysisTester GetTester(TurkishMorphotactics morphotactics)
        {
            return new AnalysisTester(RuleBasedAnalyzer.ForDebug(morphotactics));
        }

        internal bool ContainsMorpheme(SingleAnalysis result, string morphemeName)
        {
            foreach (MorphemeData forms in result.GetMorphemeDataList())
            {
                if (forms.morpheme.Id.Equals(morphemeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool LastMorphemeIs(SingleAnalysis result, string morphemeName)
        {
            List<MorphemeData> morphemes = result.GetMorphemeDataList();
            if (morphemes.Count == 0)
            {
                return false;
            }
            MorphemeData last = morphemes[morphemes.Count - 1];
            return last.morpheme.Id.Equals(morphemeName, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool NotContains(SingleAnalysis result, string morphemeName)
        {
            foreach (MorphemeData forms in result.GetMorphemeDataList())
            {
                if (forms.morpheme.Id.Equals(morphemeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        internal static void PrintAndSort(string input, List<SingleAnalysis> results)
        {
            results.Sort((e, x) =>
            {
                return e.ToString().CompareTo(x.ToString());
            });
            if (!PRINT_RESULTS_TO_SCREEN)
            {
                return;
            }
            foreach (SingleAnalysis result in results)
            {
                Console.WriteLine(input + " = " + result + " = " + FormatSurfaceAndLexical(result));
            }
        }

        internal static void ExpectFail(RuleBasedAnalyzer analyzer, params string[] words)
        {
            foreach (string word in words)
            {
                List<SingleAnalysis> results = analyzer.Analyze(word);
                if (results.Count != 0)
                {
                    PrintDebug(analyzer, word);
                    Assert.Fail("[" + word + "] is expected to fail but passed.");
                }
            }
        }

        internal static void ExpectSuccess(RuleBasedAnalyzer analyzer, params string[] words)
        {
            foreach (string word in words)
            {
                List<SingleAnalysis> results = analyzer.Analyze(word);
                if (results.Count == 0)
                {
                    PrintDebug(analyzer, word);
                    Assert.Fail("[" + word + "] is expected to pass but failed.");
                }
                else
                {
                    PrintAndSort(word, results);
                }
            }
        }

        internal static void ExpectSuccess(RuleBasedAnalyzer analyzer, int solutionCount, params string[] words)
        {
            foreach (string word in words)
            {
                List<SingleAnalysis> results = analyzer.Analyze(word);
                if (results.Count != solutionCount)
                {
                    PrintDebug(analyzer, word);
                    Assert.Fail("[" + word + "] is expected to pass with solution count " + solutionCount +
                        " but failed with solution count " + results.Count);
                }
                else
                {
                    PrintAndSort(word, results);
                }
            }
        }

        internal static SingleAnalysis GetSingleAnalysis(RuleBasedAnalyzer analyzer, string input)
        {
            List<SingleAnalysis> results = analyzer.Analyze(input);
            if (results.Count != 1)
            {
                PrintDebug(analyzer, input);
                if (results.Count == 0)
                {
                    Assert.Fail("[" + input + "] cannot be analyzed");
                }
                else
                {
                    Assert.Fail("[" + input + "] is expected to have single solution but " +
                        " it has " + results.Count + " solutions");
                }
            }
            PrintAndSort(input, results);
            return results[0];
        }

        internal static List<SingleAnalysis> GetMultipleAnalysis(
            RuleBasedAnalyzer analyzer, int count, string input)
        {
            List<SingleAnalysis> results = analyzer.Analyze(input);
            if (results.Count != count)
            {
                PrintDebug(analyzer, input);
                if (results.Count == 0)
                {
                    Assert.Fail(input + " cannot be analyzed");
                }
                else
                {
                    Assert.Fail("[" + input + "] is expected to have single solution but " +
                        " it has " + results.Count + " solutions");
                }
            }
            PrintAndSort(input, results);
            return results;
        }

        internal static List<SingleAnalysis> GetMultipleAnalysis(RuleBasedAnalyzer analyzer, string input)
        {
            List<SingleAnalysis> results = analyzer.Analyze(input);
            if (results.Count == 0)
            {
                PrintDebug(analyzer, input);
                Assert.Fail(input + " cannot be analyzed");
            }
            PrintAndSort(input, results);
            return results;
        }


        private static void PrintDebug(
            RuleBasedAnalyzer analyzer,
            string input)
        {
            analyzer.Analyze(input);
            AnalysisDebugData debugData = analyzer.GetDebugData();
            debugData.DumpToConsole();
        }

        internal class AnalysisTester
        {
            RuleBasedAnalyzer analyzer;

            public AnalysisTester(RuleBasedAnalyzer analyzer)
            {
                this.analyzer = analyzer;
            }

            internal void ExpectFail(params string[] words)
            {
                AnalyzerTestBase.ExpectFail(analyzer, words);
            }

            internal void ExpectSuccess(params string[] words)
            {
                AnalyzerTestBase.ExpectSuccess(analyzer, words);
            }

            internal void ExpectSuccess(int solutionCount, params string[] words)
            {
                AnalyzerTestBase.ExpectSuccess(analyzer, solutionCount, words);
            }

            internal void ExpectSingle(string input, Predicate<SingleAnalysis> predicate)
            {
                SingleAnalysis result = GetSingleAnalysis(analyzer, input);
                if (!predicate(result))
                {
                    PrintDebug(analyzer, input);
                    Assert.Fail("Anaysis Failed for [" + input + "]");
                }
            }

            internal void ExpectSingle(string input, AnalysisMatcher matcher)
            {
                SingleAnalysis result = GetSingleAnalysis(analyzer, input);
                if (!matcher.Predicate(result))
                {
                    PrintDebug(analyzer, input);
                    Assert.Fail("Anaysis Failed for [" + input + "]. Predicate Input = " + matcher.Expected);
                }
            }

            internal void ExpectAny(string input, AnalysisMatcher matcher)
            {
                List<SingleAnalysis> result = GetMultipleAnalysis(analyzer, input);
                foreach (SingleAnalysis analysisResult in result)
                {
                    if (matcher.Predicate(analysisResult))
                    {
                        return;
                    }
                }
                PrintDebug(analyzer, input);
                Assert.Fail("Anaysis Failed for [" + input + "]. Predicate Input = " + matcher.Expected);
            }

            internal void ExpectFalse(string input, Predicate<SingleAnalysis> predicate)
            {
                SingleAnalysis result = GetSingleAnalysis(analyzer, input);
                if (predicate(result))
                {
                    PrintDebug(analyzer, input);
                    Assert.Fail("Anaysis Failed for [" + input + "]");
                }
            }

            internal void ExpectFalse(string input, AnalysisMatcher matcher)
            {
                List<SingleAnalysis> results = GetMultipleAnalysis(analyzer, input);
                foreach (SingleAnalysis result in results)
                {
                    if (matcher.Predicate(result))
                    {
                        PrintDebug(analyzer, input);
                        Assert.Fail("Anaysis Failed for [" + input + "]");
                    }
                }
            }
        }

        public static string FormatSurfaceAndLexical(SingleAnalysis analysis)
        {
            return AnalysisFormatters.SurfaceAndLexicalSequence.Format(analysis);
        }

        public static Predicate<SingleAnalysis> MatchesShortForm(string shortForm)
        {
            return p => FormatSurfaceAndLexical(p).Equals(shortForm, StringComparison.InvariantCultureIgnoreCase);
        }

        public static Predicate<SingleAnalysis> MatchesShortFormTail(string shortFormTail)
        {
            return p => FormatSurfaceAndLexical(p).EndsWith(shortFormTail);
        }


        public static string FormatLexicalSequence(SingleAnalysis s)
        {
            return AnalysisFormatters.LexicalSequence.Format(s);
        }

        public static AnalysisMatcher MatchesTailLex(string tail)
        {
            return new AnalysisMatcher(p => FormatLexicalSequence(p).EndsWith(tail), tail);
        }

        public class AnalysisMatcher
        {
            string expected;
            Predicate<SingleAnalysis> predicate;

            public AnalysisMatcher(Predicate<SingleAnalysis> predicate, string expected)
            {
                this.Predicate = predicate;
                this.Expected = expected;
            }

            public string Expected { get => expected; set => expected = value; }
            public Predicate<SingleAnalysis> Predicate { get => predicate; set => predicate = value; }
        }
    }
}
