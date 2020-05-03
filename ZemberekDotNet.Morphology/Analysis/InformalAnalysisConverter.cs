using System.Collections.Generic;
using ZemberekDotNet.Morphology.Generator;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// Used for converting informal morphological analysis results to formal analysis using mappings and
    /// word generation.
    /// </summary>
    public class InformalAnalysisConverter
    {
        private WordGenerator generator;

        public InformalAnalysisConverter(WordGenerator generator)
        {
            this.generator = generator;
        }

        /// <summary>
        /// Converts the input and it's analysis SingleAnalysis to formal surface form and SingleAnalysis
        /// object.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="a"></param>
        /// <returns>converted single analysis object and new surface form. If input does not contain any
        /// informal morpheme, it returns input without any changes. If generation does not work, returns
        /// null.
        /// </returns>
        public WordGenerator.Result Convert(string input, SingleAnalysis a)
        {
            if (!a.ContainsInformalMorpheme())
            {
                return new WordGenerator.Result(input, a);
            }
            List<Morpheme> formalMorphemes = ToFormalMorphemeNames(a);
            List<WordGenerator.Result> generations =
                generator.Generate(a.GetDictionaryItem(), formalMorphemes);
            if (generations.Count > 0)
            {
                return generations[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts informal morphemes to formal morphemes.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static List<Morpheme> ToFormalMorphemeNames(SingleAnalysis a)
        {
            List<Morpheme> transform = new List<Morpheme>();
            foreach (Morpheme m in a.GetMorphemes())
            {
                if (m.Informal && m.MappedMorpheme != null)
                {
                    transform.Add(m.MappedMorpheme);
                }
                else
                {
                    transform.Add(m);
                }
            }
            return transform;
        }
    }
}
