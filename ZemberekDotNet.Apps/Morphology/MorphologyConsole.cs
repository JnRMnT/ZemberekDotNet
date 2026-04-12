using Commander.NET.Attributes;
using System;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Apps.Morphology
{
    public class MorphologyConsole : ConsoleApp<MorphologyConsole>
    {
        [Parameter("--disableUnknownAnalysis", "-u",
            Description = "If used, unknown words will not be analyzed.")]
        bool disableUnknownAnalysis;

        [Parameter("--enableInformalWordAnalysis", "-f",
            Description = "If used, informal word analysis results will be included.")]
        bool enableInformalWordAnalysis;

        public override string Description()
        {
            return "Applies morphological analysis and disambiguation to user entries.";
        }

        public override void Run()
        {
            TurkishMorphology.TurkishMorphologyBuilder builder = TurkishMorphology.Builder(RootLexicon.GetDefault());

            if (disableUnknownAnalysis)
            {
                builder.DisableUnidentifiedTokenAnalyzer();
            }

            if (enableInformalWordAnalysis)
            {
                builder.UseInformalAnalysis();
            }

            TurkishMorphology morphology = builder.Build();

            Console.WriteLine("Enter word or sentence. Type `exit` or `quit` to finish.");
            string input = Console.ReadLine();
            while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(input, "quit", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Empty line cannot be processed.");
                    input = Console.ReadLine();
                    continue;
                }

                SentenceAnalysis sentenceAnalysis = morphology.AnalyzeAndDisambiguate(input);
                foreach (SentenceWordAnalysis wordAnalysis in sentenceAnalysis)
                {
                    WordAnalysis analyses = wordAnalysis.GetWordAnalysis();
                    Console.WriteLine(analyses.GetInput());

                    SingleAnalysis best = wordAnalysis.GetBestAnalysis();
                    foreach (SingleAnalysis analysis in analyses)
                    {
                        bool isBest = analysis.Equals(best);
                        if (analyses.AnalysisCount() == 1)
                        {
                            Console.WriteLine(analysis.FormatLong());
                        }
                        else
                        {
                            Console.WriteLine(analysis.FormatLong() + (isBest ? "*" : ""));
                        }
                    }
                }

                Console.WriteLine();
                input = Console.ReadLine();
            }
        }

        public static void Main(string[] args)
        {
            new MorphologyConsole().Execute(args);
        }
    }
}