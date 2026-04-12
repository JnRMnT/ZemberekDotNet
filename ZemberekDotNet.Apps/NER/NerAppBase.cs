using Commander.NET.Attributes;
using System;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.NER;

namespace ZemberekDotNet.Apps.NER
{
    public abstract class NerAppBase<T> : ConsoleApp<T> where T : new()
    {
        [Parameter("--annotationStyle", "-as",
            Description = "NER annotation style: OPEN_NLP, BRACKET, ENAMEX.")]
        string annotationStyle = "OPEN_NLP";

        protected NerDataSet.AnnotationStyle GetAnnotationStyle()
        {
            if (!Enum.TryParse(annotationStyle, true, out NerDataSet.AnnotationStyle style))
            {
                throw new ArgumentException("Invalid annotation style: " + annotationStyle
                    + ". Allowed values: OPEN_NLP, BRACKET, ENAMEX");
            }
            return style;
        }

        protected TurkishMorphology CreateMorphology()
        {
            return TurkishMorphology.Builder(RootLexicon.GetDefault()).Build();
        }
    }
}