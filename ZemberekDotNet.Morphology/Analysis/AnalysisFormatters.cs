using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;
using static ZemberekDotNet.Morphology.Analysis.SingleAnalysis;

namespace ZemberekDotNet.Morphology.Analysis
{
    public class AnalysisFormatters
    {
        /// <summary>
        /// Default morphological Analysis formatter. Pipe `|` represents derivation boundary. Left side of
        /// `→` represents derivation causing morpheme, right side represents the derivation result.
        /// <pre>
        /// kitap -> [kitap:Noun] kitap:Noun+A3s
        /// kitaplarda -> [kitap:Noun] kitap:Noun+lar:A3pl+da:Loc
        /// okut -> [okumak:Verb] oku:Verb|t:Caus→Verb+Imp+A2sg
        /// </pre>
        /// </summary>
        public static IAnalysisFormatter Default = DefaultFormatter.SurfaceAndLexical();

        /// <summary>
        /// Default lexical morphological analysis formatter.
        /// <pre>
        /// kitap -> [kitap:Noun] Noun+A3s
        /// kitaplarda -> [kitap:Noun] Noun+A3pl+Loc
        /// okut -> [okumak:Verb] Verb|Caus→Verb+Imp+A2sg
        /// </pre>
        /// </summary>
        public static IAnalysisFormatter DefaultLexical = DefaultFormatter.OnlyLexical();

        /// <summary>
        /// Default lexical morphological analysis formatter. But it will not contain Dictionary item
        /// related data.
        /// <pre>
        /// kitap -> Noun+A3sg
        /// kitaplarda -> Noun+A3pl+Loc
        /// okut -> Verb|Caus→Verb+Imp+A2sg
        /// </pre>
        /// </summary>
        public static IAnalysisFormatter DefaultLexicalOnlyMorphemes = MorphemesFormatter.Lexical();

        /// <summary>
        /// Format's analysis result similar to tools developed by Kemal Oflazer. However, this does not
        /// generate exactly same outputs as morpheme names and morphotactics are slightly different. For
        /// example output will not contain "Pnon" or "Nom" morphemes.
        /// <pre>
        ///   kitaplarda -> kitap+Noun+A3pl+Loc
        ///   kitapsız -> kitap+Noun+A3sg^DB+Adj+Without
        ///   kitaplardaymış -> kitap+Noun+A3pl+Loc^DB+Verb+Zero+Narr+A3sg
        /// </pre>
        /// </summary>
        public static IAnalysisFormatter OflazerStyle = new OflazerStyleFormatter();

        /// <summary>
        /// Generates " + " separated Morpheme ids. Such as:
        /// <pre>
        ///   kitap -> Noun + A3sg
        ///   kitaba -> Noun + A3sg + Dat
        ///   kitapçığa -> Noun + A3sg + Dim + Noun + A3sg + Dat
        /// </pre>
        /// </summary>
        public static IAnalysisFormatter LexicalSequence = LexicalSequenceFormatter();

        /// <summary>
        /// Generates " + " separated Surface forms and Morpheme ids:
        /// <pre>
        ///   kitap -> kitap:Noun + A3sg
        ///   kitaba -> kitab:Noun + A3sg + a:Dat
        ///   kitapçığa -> kitap:Noun + A3sg + çığ:Dim + Noun + A3sg + a:Dat
        /// </pre>
        /// </summary>
        public static IAnalysisFormatter SurfaceAndLexicalSequence = SurfaceSequenceFormatter();

        /// <summary>
        /// Generates space separated surface forms. Such as:
        /// <pre>
        ///   kitap -> kitap
        ///   kitaba -> kitab a
        ///   kitabımızdan -> kitab ımız dan
        /// </pre>
        /// </summary>
        public static IAnalysisFormatter SurfaceSequence = new OnlySurfaceFormatter();

        internal static IAnalysisFormatter LexicalSequenceFormatter()
        {
            return new LexicalSeqFormatter();
        }

        internal static IAnalysisFormatter SurfaceSequenceFormatter()
        {
            return new SurfaceSeqFormatter();
        }

        private class SurfaceSeqFormatter : IAnalysisFormatter
        {
            public string Format(SingleAnalysis analysis)
            {
                return string.Join(" + ", analysis.GetMorphemeDataList().Select(s => s.ToMorphemeString()));
            }
        }

        private class LexicalSeqFormatter : IAnalysisFormatter
        {
            public string Format(SingleAnalysis analysis)
            {
                return string.Join(" + ", analysis.GetMorphemeDataList().Select(s => s.morpheme.Id));
            }
        }

        public class OflazerStyleFormatter : IAnalysisFormatter
        {
            private bool useRoot = false;
            public OflazerStyleFormatter()
            {
            }

            private OflazerStyleFormatter(bool useRoot)
            {
                this.useRoot = useRoot;
            }

            public static OflazerStyleFormatter usingDictionaryRoot()
            {
                return new OflazerStyleFormatter(true);
            }

            public string Format(SingleAnalysis analysis)
            {
                List<MorphemeData> surfaces = analysis.GetMorphemeDataList();

                StringBuilder sb = new StringBuilder(surfaces.Count * 4);

                // root and suffix formatting

                String stemStr = useRoot ? analysis.GetDictionaryItem().root : analysis.GetStem();
                sb.Append(stemStr).Append('+');
                DictionaryItem item = analysis.GetDictionaryItem();
                PrimaryPos pos = item.primaryPos;

                String posStr = pos == PrimaryPos.Adverb ? "Adverb" : pos.shortForm;

                sb.Append(posStr);
                if (item.secondaryPos != SecondaryPos.None && item.secondaryPos != SecondaryPos.UnknownSec)
                {
                    sb.Append('+').Append(item.secondaryPos.shortForm);
                }
                if (surfaces.Count > 1 && !surfaces[1].morpheme.Derivational1)
                {
                    sb.Append("+");
                }

                for (int i = 1; i < surfaces.Count; i++)
                {
                    MorphemeData s = surfaces[i];
                    if (s.morpheme.Derivational1)
                    {
                        sb.Append("^DB+");
                        sb.Append(surfaces[i + 1].morpheme.Id)
                            .Append("+"); // Oflazer first puts the derivation result morpheme.
                        sb.Append(s.morpheme.Id);
                        i++;
                    }
                    else
                    {
                        sb.Append(s.morpheme.Id);
                    }
                    if (i < surfaces.Count - 1 && !surfaces[i + 1].morpheme.Derivational1)
                    {
                        sb.Append("+");
                    }

                }
                return sb.ToString();
            }
        }

        internal class MorphemesFormatter : IAnalysisFormatter
        {
            bool addSurface;

            internal static MorphemesFormatter Lexical()
            {
                return new MorphemesFormatter(false);
            }

            internal static MorphemesFormatter Surface()
            {
                return new MorphemesFormatter(true);
            }

            public MorphemesFormatter(bool addSurface)
            {
                this.addSurface = addSurface;
            }

            public string Format(SingleAnalysis analysis)
            {
                List<MorphemeData> surfaces = analysis.GetMorphemeDataList();

                StringBuilder sb = new StringBuilder(surfaces.Count * 4);

                // root and suffix formatting
                if (addSurface)
                {
                    sb.Append(analysis.GetStem()).Append(':');
                }
                sb.Append(surfaces[0].morpheme.Id);
                if (surfaces.Count > 1 && !surfaces[1].morpheme.Derivational1)
                {
                    sb.Append("+");
                }
                for (int i = 1; i < surfaces.Count; i++)
                {
                    MorphemeData s = surfaces[i];
                    Morpheme morpheme = s.morpheme;
                    if (morpheme.Derivational1)
                    {
                        sb.Append('|');
                    }
                    if (addSurface && s.surface.Length > 0)
                    {
                        sb.Append(s.surface).Append(':');
                    }
                    sb.Append(s.morpheme.Id);
                    if (s.morpheme.Derivational1)
                    {
                        sb.Append('→');
                    }
                    else if (i < surfaces.Count - 1 && !surfaces[i + 1].morpheme.Derivational1)
                    {
                        sb.Append('+');
                    }
                }
                return sb.ToString();
            }
        }

        internal class DefaultFormatter : IAnalysisFormatter
        {
            bool addSurface;
            MorphemesFormatter morphemesFormatter;

            internal static DefaultFormatter OnlyLexical()
            {
                return new DefaultFormatter(false);
            }

            internal static DefaultFormatter SurfaceAndLexical()
            {
                return new DefaultFormatter(true);
            }

            DefaultFormatter(bool addSurface)
            {
                this.addSurface = addSurface;
                morphemesFormatter = addSurface ? MorphemesFormatter.Surface() :
                    MorphemesFormatter.Lexical();
            }

            public string Format(SingleAnalysis analysis)
            {
                List<MorphemeData> surfaces = analysis.GetMorphemeDataList();

                StringBuilder sb = new StringBuilder(surfaces.Count * 5);

                // dictionary item formatting
                sb.Append('[');
                DictionaryItem item = analysis.GetDictionaryItem();
                sb.Append(item.lemma).Append(':').Append(item.primaryPos.shortForm);
                if (item.secondaryPos != SecondaryPos.None)
                {
                    sb.Append(',').Append(item.secondaryPos.shortForm);
                }
                sb.Append("] ");

                // root and suffix formatting
                sb.Append(morphemesFormatter.Format(analysis));
                return sb.ToString();
            }
        }

        class OnlySurfaceFormatter : IAnalysisFormatter
        {
            public string Format(SingleAnalysis analysis)
            {
                List<string> tokens = new List<string>(3);

                foreach (MorphemeData mSurface in analysis.GetMorphemeDataList())
                {
                    if (mSurface.surface.Length > 0)
                    {
                        tokens.Add(mSurface.surface);
                    }
                }
                return String.Join(" ", tokens);
            }
        }
    }
}
