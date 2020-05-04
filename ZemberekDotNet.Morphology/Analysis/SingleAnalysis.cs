using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// This class represents a single morphological analysis result.
    /// </summary>
    public class SingleAnalysis
    {
        // Dictionary Item of the analysis.
        private DictionaryItem item;

        // Contains Morphemes and their surface form (actual appearance in the normalized input)
        // List also contain the root (unchanged or modified) of the Dictionary item.
        // For example, for normalized input "kedilere"
        // This list may contain "kedi:Noun, ler:A3pl , e:Dat" information.
        private List<MorphemeData> morphemeDataList;

        // groupBoundaries holds the index values of morphemes.
        private int[] groupBoundaries;

        // cached hash value.
        private int hash;

        public SingleAnalysis(
            DictionaryItem item,
            List<MorphemeData> morphemeDataList,
            int[] groupBoundaries)
        {
            this.item = item;
            this.morphemeDataList = morphemeDataList;
            this.groupBoundaries = groupBoundaries;
            this.hash = GetHashCode();
        }

        public static SingleAnalysis Unknown(string input)
        {
            DictionaryItem item = DictionaryItem.UNKNOWN;
            MorphemeData s = new MorphemeData(Morpheme.UNKNOWN, input);
            int[] boundaries = { 0 };
            return new SingleAnalysis(item, new List<MorphemeData> { s }, boundaries);
        }

        public static SingleAnalysis Dummy(string input, DictionaryItem item)
        {
            MorphemeData s = new MorphemeData(Morpheme.UNKNOWN, input);
            int[] boundaries = { 0 };
            return new SingleAnalysis(item, new List<MorphemeData> { s }, boundaries);
        }

        public string SurfaceForm()
        {
            return GetStem() + GetEnding();
        }

        public class MorphemeGroup
        {
            internal List<MorphemeData> morphemes;

            public MorphemeGroup(List<MorphemeData> morphemes)
            {
                this.morphemes = morphemes;
            }

            public List<MorphemeData> GetMorphemes()
            {
                return morphemes;
            }

            public PrimaryPos GetPos()
            {
                foreach (MorphemeData mSurface in morphemes)
                {
                    if (mSurface.morpheme.Pos != null && mSurface.morpheme.Pos != PrimaryPos.Unknown)
                    {
                        return mSurface.morpheme.Pos;
                    }
                }
                return PrimaryPos.Unknown;
            }

            public string SurfaceForm()
            {
                StringBuilder sb = new StringBuilder();
                foreach (MorphemeData mSurface in morphemes)
                {
                    sb.Append(mSurface.surface);
                }
                return sb.ToString();
            }

            public string SurfaceFormSkipPosRoot()
            {
                StringBuilder sb = new StringBuilder();
                foreach (MorphemeData mSurface in morphemes)
                {
                    if (mSurface.morpheme.Pos != null)
                    {
                        continue;
                    }
                    sb.Append(mSurface.surface);
                }
                return sb.ToString();
            }

            public string LexicalForm()
            {
                StringBuilder sb = new StringBuilder();
                foreach (MorphemeData mSurface in morphemes)
                {
                    sb.Append(mSurface.morpheme.Id);
                }
                return sb.ToString();
            }

        }

        public bool ContainsInformalMorpheme()
        {
            return GetMorphemes().Any(e => e.Informal);
        }

        public int GetMorphemeGroupCount()
        {
            return groupBoundaries.Length;
        }

        /// <summary>
        /// Returns the concatenated suffix surfaces.
        /// <pre>
        ///   "elmalar"      -> "lar"
        ///   "elmalara"     -> "lara"
        ///   "okutturdular" -> "tturdular"
        ///   "arıyor"       -> "ıyor"
        /// </pre>        /// 
        /// </summary>
        /// <returns></returns>
        public string GetEnding()
        {
            StringBuilder sb = new StringBuilder();
            // skip the root.
            for (int i = 1; i < morphemeDataList.Count; i++)
            {
                MorphemeData mSurface = morphemeDataList[i];
                sb.Append(mSurface.surface);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns the stem of the word. Stem may be different than the lemma of the word.
        /// <pre>
        ///   "elmalar"      -> "elma"
        ///   "kitabımız"    -> "kitab"
        ///   "okutturdular" -> "oku"
        ///   "arıyor"       -> "ar"
        /// </pre>
        /// TODO: decide for inputs like "12'ye and "Ankara'da"
        /// </summary>
        /// <returns> concatenated suffix surfaces.</returns>
        public string GetStem()
        {
            return morphemeDataList[0].surface;
        }

        public bool ContainsMorpheme(Morpheme morpheme)
        {
            foreach (MorphemeData morphemeData in morphemeDataList)
            {
                if (morphemeData.morpheme == morpheme)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Splits the parse into stem and ending. Such as:
        /// <pre>
        /// "kitaplar" -> "kitap-lar"
        /// "kitabımdaki" -> "kitab-ımdaki"
        /// "kitap" -> "kitap-"
        /// </pre> 
        /// </summary>
        /// <returns>a StemAndEnding instance carrying stem and ending. If ending has no surface content
        /// empty string is used.
        /// </returns>
        public StemAndEnding GetStemAndEnding()
        {
            return new StemAndEnding(GetStem(), GetEnding());
        }

        public DictionaryItem GetDictionaryItem()
        {
            return item;
        }

        public bool IsUnknown()
        {
            return item.IsUnknown();
        }

        public bool IsRuntime()
        {
            return item.HasAttribute(RootAttribute.Runtime);
        }

        public List<MorphemeData> GetMorphemeDataList()
        {
            return morphemeDataList;
        }

        public List<Morpheme> GetMorphemes()
        {
            return morphemeDataList.Select(e => e.morpheme).ToList();
        }

        public MorphemeGroup GetGroup(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= groupBoundaries.Length)
            {
                throw new ArgumentException("There are only " + groupBoundaries.Length +
                    " morpheme groups. But input is " + groupIndex);
            }
            int endIndex = groupIndex == groupBoundaries.Length - 1 ?
                morphemeDataList.Count : groupBoundaries[groupIndex + 1];

            return new MorphemeGroup(morphemeDataList.GetRange(groupBoundaries[groupIndex], endIndex - groupBoundaries[groupIndex]));
        }

        // container for Morphemes and their surface forms.
        public class MorphemeData
        {
            public readonly Morpheme morpheme;
            public readonly string surface;

            public MorphemeData(Morpheme morpheme, String surface)
            {
                this.morpheme = morpheme;
                this.surface = surface;
            }

            public override string ToString()
            {
                return ToMorphemeString();
            }

            public string ToMorphemeString()
            {
                return SurfaceString() + morpheme.Id;
            }

            private string SurfaceString()
            {
                return surface.IsEmpty() ? "" : surface + ":";
            }

            public override bool Equals(Object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || !GetType().Equals(o.GetType()))
                {
                    return false;
                }

                MorphemeData that = (MorphemeData)o;

                if (!morpheme.Equals(that.morpheme))
                {
                    return false;
                }
                return surface.Equals(that.surface);
            }

            public override int GetHashCode()
            {
                int result = morpheme.GetHashCode();
                result = 31 * result + surface.GetHashCode();
                return result;
            }
        }

        public MorphemeGroup GetLastGroup()
        {
            return GetGroup(groupBoundaries.Length - 1);
        }

        public MorphemeGroup[] GetGroups()
        {
            MorphemeGroup[] groups = new MorphemeGroup[groupBoundaries.Length];
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = GetGroup(i);
            }
            return groups;
        }


        private static readonly ConcurrentDictionary<Morpheme, MorphemeData> emptyMorphemeCache =
            new ConcurrentDictionary<Morpheme, MorphemeData>();

        // Here we generate a SingleAnalysis from a search path.
        public static SingleAnalysis FromSearchPath(SearchPath searchPath)
        {

            List<MorphemeData> morphemes = new List<MorphemeData>(searchPath.transitions.Count);

            int derivationCount = 0;

            foreach (SurfaceTransition transition in searchPath.GetTransitions())
            {
                if (transition.IsDerivative())
                {
                    derivationCount++;
                }

                Morpheme morpheme = transition.GetMorpheme();

                // we skip these two morphemes as they create visual noise and does not carry much information.
                if (morpheme == TurkishMorphotactics.nom || morpheme == TurkishMorphotactics.pnon)
                {
                    continue;
                }

                // if empty, use the cache.
                if (transition.surface.IsEmpty())
                {
                    MorphemeData morphemeData = emptyMorphemeCache.GetValueOrDefault(morpheme);
                    if (morphemeData == null)
                    {
                        morphemeData = new MorphemeData(morpheme, "");
                        emptyMorphemeCache.TryAdd(morpheme, morphemeData);
                    }
                    morphemes.Add(morphemeData);
                    continue;
                }

                MorphemeData suffixSurface = new MorphemeData(morpheme, transition.surface);
                morphemes.Add(suffixSurface);
            }

            int[] groupBoundaries = new int[derivationCount + 1];
            groupBoundaries[0] = 0; // we assume there is always an IG

            int morphemeCounter = 0, derivationCounter = 1;
            foreach (MorphemeData morphemeData in morphemes)
            {
                if (morphemeData.morpheme.Derivational1)
                {
                    groupBoundaries[derivationCounter] = morphemeCounter;
                    derivationCounter++;
                }
                morphemeCounter++;
            }

            // if dictionary item is `Dummy`, use the referenced item.
            // `Dummy` items are usually generated for some compound words. For example for `zeytinyağı`
            // a DictionaryItem is generated with root "zeytinyağ". But here we switch to the original.
            DictionaryItem item = searchPath.GetDictionaryItem();
            if (item.HasAttribute(RootAttribute.Dummy))
            {
                item = item.GetReferenceItem();
            }
            return new SingleAnalysis(item, morphemes, groupBoundaries);
        }

        public bool ContainsAnyMorpheme(params Morpheme[] morphemes)
        {
            foreach (Morpheme morpheme in morphemes)
            {
                if (ContainsMorpheme(morpheme))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method is used for modifying the dictionary item and stem of an analysis without changing
        ///  the suffix morphemes. This is used for generating result for inputs like "5'e"
        /// </summary>
        /// <param name="item">new DictionaryItem</param>
        /// <param name="stem">new stem</param>
        /// <returns>new SingleAnalysis object with given DictionaryItem and stem.</returns>
        internal SingleAnalysis CopyFor(DictionaryItem item, string stem)
        {
            // copy morpheme-surface list.
            List<MorphemeData> data = new List<MorphemeData>(morphemeDataList);
            // replace the stem surface. it is in the first morpheme.
            data[0] = new MorphemeData(data[0].morpheme, stem);
            return new SingleAnalysis(item, data, (int[])groupBoundaries.Clone());
        }

        /// <summary>
        /// Returns surface forms list of all root and derivational roots of a parse. Examples:
        /// <pre>
        /// "kitaplar"  ->["kitap"]
        /// "kitabım"   ->["kitab"]
        /// "kitaplaşır"->["kitap", "kitaplaş"]
        /// "kavrulduk" ->["kavr","kavrul"]
        /// </pre>
        /// </summary>
        /// <returns></returns>
        public List<string> GetStems()
        {
            List<string> stems = new List<string>(2);
            stems.Add(GetStem());
            string previousStem = GetGroup(0).SurfaceForm();
            if (groupBoundaries.Length > 1)
            {
                for (int i = 1; i < groupBoundaries.Length; i++)
                {
                    MorphemeGroup ig = GetGroup(i);
                    MorphemeData suffixData = ig.morphemes[0];

                    string surface = suffixData.surface;
                    string stem = previousStem + surface;
                    if (!stems.Contains(stem))
                    {
                        stems.Add(stem);
                    }
                    previousStem = previousStem + ig.SurfaceForm();
                }
            }
            return stems;
        }

        /// <summary>
        /// Returns list of all lemmas of a parse. Examples: 
        /// "kitaplar"  ->["kitap"]
        /// "kitabım"   ->["kitap"]
        /// "kitaplaşır"->["kitap", "kitaplaş"]
        /// "kitaplaş"  ->["kitap", "kitaplaş"]
        /// "arattıragörür" -> ["ara","arat","arattır","arattıragör"]
        /// </summary>
        /// <returns></returns>
        public List<string> GetLemmas()
        {
            List<string> lemmas = new List<string>(2);
            lemmas.Add(item.root);

            String previousStem = GetGroup(0).SurfaceForm();
            if (!previousStem.Equals(item.root))
            {
                if (previousStem.EndsWith("ğ"))
                {
                    previousStem = previousStem.Substring(0, previousStem.Length - 1) + "k";
                }
            }

            if (groupBoundaries.Length > 1)
            {
                for (int i = 1; i < groupBoundaries.Length; i++)
                {
                    MorphemeGroup ig = GetGroup(i);
                    MorphemeData suffixData = ig.morphemes[0];

                    String surface = suffixData.surface;
                    String stem = previousStem + surface;
                    if (stem.EndsWith("ğ"))
                    {
                        stem = stem.Substring(0, stem.Length - 1) + "k";
                    }
                    if (!lemmas.Contains(stem))
                    {
                        lemmas.Add(stem);
                    }
                    previousStem = previousStem + ig.SurfaceForm();
                }
            }
            return lemmas;
        }

        public override string ToString()
        {
            return FormatLong();
        }

        public string FormatLexical()
        {
            return AnalysisFormatters.DefaultLexical.Format(this);
        }

        /// <summary>
        /// Formats only the morphemes. Dictionary item information is not included.
        /// </summary>
        /// <returns>formatted</returns>
        public string FormatMorphemesLexical()
        {
            return AnalysisFormatters.DefaultLexicalOnlyMorphemes.Format(this);
        }

        public PrimaryPos GetPos()
        {
            return GetGroup(GroupCount() - 1).GetPos();
        }

        public string FormatLong()
        {
            return AnalysisFormatters.Default.Format(this);
        }

        public int GroupCount()
        {
            return groupBoundaries.Length;
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

            SingleAnalysis that = (SingleAnalysis)o;

            if (hash != that.hash)
            {
                return false;
            }
            if (!item.Equals(that.item))
            {
                return false;
            }
            return morphemeDataList.Equals(that.morphemeDataList);
        }

        public override int GetHashCode()
        {
            if (hash != 0)
            {
                return hash;
            }
            int result = item.GetHashCode();
            result = 31 * result + morphemeDataList.GetHashCode();
            result = 31 * result + hash;
            return result;
        }
    }
}
