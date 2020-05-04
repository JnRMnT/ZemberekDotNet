using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Enums;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Native.Helpers;
using ZemberekDotNet.Core.Native.Text;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis.TR;

namespace ZemberekDotNet.Morphology.Lexicon.TR
{
    public class TurkishDictionaryLoader
    {
        public static readonly IReadOnlyCollection<string> DefaultDictionaryResources = new List<string> {
     "Resources/tr/master-dictionary.dict",
     "Resources/tr/non-tdk.dict",
     "Resources/tr/proper.dict",
     "Resources/tr/proper-from-corpus.dict",
     "Resources/tr/abbreviations.dict",
     "Resources/tr/person-names.dict"
 }.AsReadOnly();
        static readonly Regex DashQuoteMatcher = new Regex("[\\-']");
        private static readonly Func<string, string[]> MetadataSplitter = (e) =>
         {
             return e.Split(";").Select(e => e.Trim()).Where(e => !string.IsNullOrEmpty(e)).ToArray();
         };
        private static readonly Func<string, string[]> PosSplitter = (e) =>
        {
            return e.Split(",").Select(e => e.Trim()).ToArray();
        };
        private static readonly Func<string, string[]> AttributeSplitter = (e) =>
        {
            return e.Split(",").Select(e => e.Trim()).ToArray();
        };

        public static RootLexicon LoadDefaultDictionaries()
        {
            return Load(DefaultDictionaryResources);
        }

        public static RootLexicon LoadFromResources(params string[] resourcePaths)
        {
            return Load(new List<string>(resourcePaths));
        }

        public static RootLexicon Load(IReadOnlyCollection<string> resourcePaths)
        {
            List<string> lines = new List<string>(); ;
            foreach (string resourcePath in resourcePaths)
            {
                lines.AddRange(TextIO.LoadLines(resourcePath, "##"));
            }
            return Load(lines);
        }

        public static RootLexicon Load(string input)
        {
            return FileHelper.ReadAllLines(input, Encoding.UTF8, new TextLexiconProcessor());
        }

        public static RootLexicon LoadInto(RootLexicon lexicon, string input)
        {
            return FileHelper.ReadAllLines(input, Encoding.UTF8, new TextLexiconProcessor(lexicon));
        }

        public static DictionaryItem LoadFromString(string dictionaryLine)
        {
            string lemma = dictionaryLine;
            if (dictionaryLine.Contains(" "))
            {
                lemma = dictionaryLine.Substring(0, dictionaryLine.IndexOf(" "));
            }
            return Load(new string[] { dictionaryLine }).GetMatchingItems(lemma)[0];
        }

        public static RootLexicon Load(params string[] dictionaryLines)
        {
            TextLexiconProcessor processor = new TextLexiconProcessor();
            try
            {
                foreach (string s in dictionaryLines)
                {
                    processor.ProcessLine(s);
                }
                return processor.GetResult();
            }
            catch (Exception e)
            {
                throw new LexiconException(
                    "Cannot parse lines [" + Arrays.ToString(dictionaryLines) + "] with reason: "
                        + e.Message);
            }
        }

        public static RootLexicon Load(IEnumerable<string> dictionaryLines)
        {
            TextLexiconProcessor processor = new TextLexiconProcessor();
            foreach (string s in dictionaryLines)
            {
                try
                {
                    processor.ProcessLine(s);
                }
                catch (Exception e)
                {
                    throw new LexiconException("Cannot load line '" + s + "' with reason: " + e.Message);
                }
            }
            return processor.GetResult();
        }

        internal class MetaDataId : IStringEnum
        {
            internal static readonly MetaDataId POS = new MetaDataId("P");
            internal static readonly MetaDataId ATTRIBUTES = new MetaDataId("A");
            internal static readonly MetaDataId REF_ID = new MetaDataId("Ref");
            internal static readonly MetaDataId ROOTS = new MetaDataId("Roots");
            internal static readonly MetaDataId PRONUNCIATION = new MetaDataId("Pr");
            internal static readonly MetaDataId SUFFIX = new MetaDataId("S");
            internal static readonly MetaDataId INDEX = new MetaDataId("Index");

            internal static StringEnumMap<MetaDataId> ToEnum = StringEnumMap<MetaDataId>.Get();
            internal string form;

            internal MetaDataId(string form)
            {
                this.form = form;
            }

            public string GetStringForm()
            {
                return form;
            }

            public static IEnumerable<MetaDataId> Values
            {
                get
                {
                    yield return POS;
                    yield return ATTRIBUTES;
                    yield return REF_ID;
                    yield return ROOTS;
                    yield return PRONUNCIATION;
                    yield return SUFFIX;
                    yield return INDEX;
                }
            }
        }

        // A simple class that holds raw word and metadata information. Represents a single line in dictionary.
        internal class LineData
        {
            internal readonly string word;
            private readonly Dictionary<MetaDataId, string> metaData;

            internal LineData(string line)
            {
                this.word = Strings.SubstringUntilFirst(line, " ");
                if (word.Length == 0)
                {
                    throw new LexiconException("Line " + line + " has no word data!");
                }
                this.metaData = ReadMetadata(line);
            }

            internal string GetMetaData(MetaDataId id)
            {
                return metaData == null ? null : metaData.GetValueOrDefault(id);
            }

            internal Dictionary<MetaDataId, string> ReadMetadata(string line)
            {
                string meta = line.Substring(word.Length).Trim();
                // No metadata defines, return.
                if (meta.IsEmpty())
                {
                    return null;
                }
                // Check brackets.
                if (!meta.StartsWith("[") || !meta.EndsWith("]"))
                {
                    throw new LexiconException(
                        "Malformed metadata, missing brackets. Should be: [metadata]. Line: " + line);
                }
                // Strip brackets.
                meta = meta.Substring(1, meta.Length - 2);
                Dictionary<MetaDataId, string> metadataIds = new Dictionary<MetaDataId, string>();
                foreach (string chunk in MetadataSplitter(meta))
                {
                    if (!chunk.Contains(":"))
                    {
                        throw new LexiconException("Line " + line + " has malformed meta-data chunk" + chunk
                            + " it should have a ':' symbol.");
                    }
                    string tokenIdStr = Strings.SubstringUntilFirst(chunk, ":").Trim();
                    if (!MetaDataId.ToEnum.EnumExists(tokenIdStr))
                    {
                        throw new LexiconException(
                            "Line " + line + " has malformed meta-data chunk" + chunk + " unknown chunk id:"
                                + tokenIdStr);
                    }
                    MetaDataId id = MetaDataId.ToEnum.GetEnum(tokenIdStr);
                    String chunkData = Strings.SubstringAfterFirst(chunk, ":").Trim();
                    if (chunkData.Length == 0)
                    {
                        throw new LexiconException("Line " + line + " has malformed meta-data chunk" + chunk
                            + " no chunk data available");
                    }
                    metadataIds.Add(id, chunkData);
                }
                return metadataIds;
            }

            internal bool ContainsMetaData(MetaDataId metaDataId)
            {
                return metaData != null && metaData.ContainsKey(metaDataId);
            }
        }

        internal class TextLexiconProcessor : ILineProcessor<RootLexicon>
        {
            static readonly TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            RootLexicon rootLexicon = new RootLexicon();
            List<LineData> lateEntries = new List<LineData>();

            public TextLexiconProcessor()
            {
            }

            public TextLexiconProcessor(RootLexicon lexicon)
            {
                rootLexicon = lexicon;
            }

            public bool ProcessLine(string line)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("##"))
                {
                    return true;
                }
                try
                {
                    LineData lineData = new LineData(line);
                    // if a line contains references to other lines, we add them to lexicon later.
                    if (!lineData.ContainsMetaData(MetaDataId.REF_ID) &&
                        !lineData.ContainsMetaData(MetaDataId.ROOTS))
                    {
                        rootLexicon.Add(GetItem(lineData));
                    }
                    else
                    {
                        lateEntries.Add(lineData);
                    }
                }
                catch (Exception e)
                {
                    Log.Info("Exception in line:" + line);
                    throw new IOException("Exception in line:" + line, e);
                }
                return true;
            }

            public RootLexicon GetResult()
            {
                foreach (LineData lateEntry in lateEntries)
                {
                    if (lateEntry.ContainsMetaData(MetaDataId.REF_ID))
                    {
                        String referenceId = lateEntry.GetMetaData(MetaDataId.REF_ID);
                        if (!referenceId.Contains("_"))
                        {
                            referenceId = referenceId + "_Noun";
                        }
                        DictionaryItem refItem = rootLexicon.GetItemById(referenceId);
                        if (refItem == null)
                        {
                            Log.Warn("Cannot find reference item id " + referenceId);
                        }
                        DictionaryItem item = GetItem(lateEntry);
                        item.SetReferenceItem(refItem);
                        rootLexicon.Add(item);
                    }
                    // this is a compound lemma with P3sg in it. Such as atkuyruğu
                    if (lateEntry.ContainsMetaData(MetaDataId.ROOTS))
                    {
                        PosInfo posInfo = GetPosData(lateEntry.GetMetaData(MetaDataId.POS), lateEntry.word);
                        DictionaryItem item = rootLexicon
                            .GetItemById(lateEntry.word + "_" + posInfo.primaryPos.shortForm);
                        if (item == null)
                        {
                            item = GetItem(lateEntry); // we generate an item and add it.
                            rootLexicon.Add(item);
                        }
                        String r = lateEntry.GetMetaData(MetaDataId.ROOTS); // at-kuyruk
                        String root = r.Replace("-", ""); // atkuyruk

                        if (r.Contains("-"))
                        { // r = kuyruk
                            r = r.Substring(r.IndexOf('-') + 1);
                        }
                        List<DictionaryItem> refItems = rootLexicon
                            .GetMatchingItems(r); // check lexicon for [kuyruk]

                        ISet<RootAttribute> attrSet = new HashSet<RootAttribute>();
                        DictionaryItem refItem;
                        if (refItems.Count > 0)
                        {
                            // use the item with lowest index value.
                            refItems.OrderBy((a) => a.index);
                            // grab the first Dictionary item matching to kuyruk. We will use it's attributes.
                            refItem = refItems[0];
                            attrSet = ((HashSet<RootAttribute>)(refItem.attributes)).Clone();
                        }
                        else
                        {
                            InferMorphemicAttributes(root, posInfo, attrSet);
                        }
                        attrSet.Add(RootAttribute.CompoundP3sgRoot);
                        if (item.attributes.Contains(RootAttribute.Ext))
                        {
                            attrSet.Add(RootAttribute.Ext);
                        }

                        int index = 0;
                        if (rootLexicon.GetItemById(root + "_" + item.primaryPos.shortForm) != null)
                        {
                            index = 1;
                        }
                        // generate a fake lemma for atkuyruk, use kuyruk's attributes.
                        // But do not allow voicing.
                        DictionaryItem fakeRoot = new DictionaryItem(
                            root,
                            root,
                            root,
                            item.primaryPos,
                            item.secondaryPos,
                            attrSet,
                            index);
                        fakeRoot.attributes.Add(RootAttribute.Dummy);
                        fakeRoot.attributes.Remove(RootAttribute.Voicing);
                        fakeRoot.SetReferenceItem(item);
                        rootLexicon.Add(fakeRoot);
                    }
                }
                return rootLexicon;
            }

            PronunciationGuesser pronunciationGuesser = new PronunciationGuesser();

            internal DictionaryItem GetItem(LineData data)
            {
                PosInfo posInfo = GetPosData(data.GetMetaData(MetaDataId.POS), data.word);
                String attributesString = data.GetMetaData(MetaDataId.ATTRIBUTES);

                CultureInfo locale = Turkish.Locale;
                if (attributesString != null && attributesString.Contains(RootAttribute.LocaleEn.GetStringForm()))
                {
                    locale = CultureInfo.GetCultureInfo("en");
                }

                string cleanWord = GenerateRoot(data.word, posInfo, locale);

                String indexStr = data.GetMetaData(MetaDataId.INDEX);
                int index = 0;
                if (indexStr != null)
                {
                    index = int.Parse(indexStr);
                }

                String pronunciation = data.GetMetaData(MetaDataId.PRONUNCIATION);
                bool pronunciationGuessed = false;
                SecondaryPos secondaryPos = posInfo.secondaryPos;
                if (pronunciation == null)
                {
                    pronunciationGuessed = true;
                    if (posInfo.primaryPos == PrimaryPos.Punctuation)
                    {
                        //TODO: what to do with pronunciations of punctuations? For now we give them a generic one.
                        pronunciation = "a";
                    }
                    else if (secondaryPos == SecondaryPos.Abbreviation)
                    {
                        pronunciation = pronunciationGuesser.GuessForAbbreviation(cleanWord);
                    }
                    else if (alphabet.ContainsVowel(cleanWord))
                    {
                        pronunciation = cleanWord;
                    }
                    else
                    {
                        pronunciation = pronunciationGuesser.ToTurkishLetterPronunciations(cleanWord);
                    }
                }
                else
                {
                    pronunciation = pronunciation.ToLower(locale);
                }

                ISet<RootAttribute> attributes = MorphemicAttributes(
                    attributesString,
                    pronunciation,
                    posInfo);

                if (pronunciationGuessed &&
                    (secondaryPos == SecondaryPos.ProperNoun || secondaryPos == SecondaryPos.Abbreviation))
                {
                    attributes.Add(RootAttribute.PronunciationGuessed);
                }

                // here if there is an item with same lemma and pos values but attributes are different,
                // we increment the index.
                while (true)
                {
                    String id = DictionaryItem.GenerateId(data.word, posInfo.primaryPos, secondaryPos, index);
                    DictionaryItem existingItem = rootLexicon.GetItemById(id);
                    if (existingItem != null && existingItem.id.Equals(id))
                    {
                        if (attributes.Equals(existingItem.attributes))
                        {
                            Log.Warn("Item already defined : {0}", existingItem);
                            break;
                        }
                        else
                        {
                            index++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return new DictionaryItem(
                    data.word,
                    cleanWord,
                    pronunciation,
                    posInfo.primaryPos,
                    secondaryPos,
                    attributes,
                    index);
            }

            internal string GenerateRoot(string word, PosInfo posInfo, CultureInfo locale)
            {
                if (posInfo.primaryPos == PrimaryPos.Punctuation)
                {
                    return word;
                }
                // Strip -mek -mak from verbs.
                if (posInfo.primaryPos == PrimaryPos.Verb && IsVerb(word))
                {
                    word = word.Substring(0, word.Length - 3);
                }
                //TODO: not sure if we should remove diacritics or convert to lowercase.
                // Lowercase and normalize diacritics.
                word = alphabet.NormalizeCircumflex(word.ToLower(locale));
                // Remove dashes
                return DashQuoteMatcher.Replace(word, "");
            }

            internal PosInfo GetPosData(string posStr, string word)
            {
                if (posStr == null)
                {
                    //infer the type.
                    return new PosInfo(InferPrimaryPos(word), InferSecondaryPos(word));
                }
                else
                {
                    PrimaryPos primaryPos = null;
                    SecondaryPos secondaryPos = null;
                    string[] tokens = PosSplitter(posStr);

                    if (tokens.Length > 2)
                    {
                        throw new ApplicationException("Only two POS tokens are allowed in data chunk:" + posStr);
                    }

                    foreach (string token in tokens)
                    {
                        if (!PrimaryPos.Exists(token) && !SecondaryPos.Exists(token))
                        {
                            throw new ApplicationException(
                                "Unrecognized pos data [" + token + "] in data chunk:" + posStr);
                        }
                    }

                    // Ques POS causes some trouble here. Because it is defined in both primary and secondary pos.
                    foreach (string token in tokens)
                    {

                        if (PrimaryPos.Exists(token))
                        {
                            if (primaryPos == null)
                            {
                                primaryPos = PrimaryPos.Converter().GetEnum(token);
                                continue;
                            }
                            else if (!SecondaryPos.Exists(token))
                            {
                                throw new ApplicationException("Multiple primary pos in data chunk:" + posStr);
                            }
                        }

                        if (SecondaryPos.Exists(token))
                        {
                            if (secondaryPos == null)
                            {
                                secondaryPos = SecondaryPos.Converter().GetEnum(token);
                            }
                            else if (!PrimaryPos.Exists(token))
                            {
                                throw new ApplicationException("Multiple secondary pos in data chunk:" + posStr);
                            }
                        }
                    }
                    // If there are no primary or secondary pos defined, try to infer them.
                    if (primaryPos == null)
                    {
                        primaryPos = InferPrimaryPos(word);
                    }
                    if (secondaryPos == null)
                    {
                        secondaryPos = InferSecondaryPos(word);
                    }
                    return new PosInfo(primaryPos, secondaryPos);
                }

            }

            private PrimaryPos InferPrimaryPos(String word)
            {
                return IsVerb(word) ? PrimaryPos.Verb : PrimaryPos.Noun;
            }

            private bool IsVerb(string word)
            {
                return char.IsLower(word[0])
                    && word.Length > 3
                    && (word.EndsWith("mek") || word.EndsWith("mak"));
            }

            private SecondaryPos InferSecondaryPos(string word)
            {
                if (char.IsUpper(word[0]))
                {
                    return SecondaryPos.ProperNoun;
                }
                else
                {
                    return SecondaryPos.None;
                }
            }

            private ISet<RootAttribute> MorphemicAttributes(string data, String word, PosInfo posData)
            {
                HashSet<RootAttribute> attributesList = new HashSet<RootAttribute>();
                if (data == null)
                {
                    //  if (!posData.primaryPos.equals(PrimaryPos.Punctuation))
                    InferMorphemicAttributes(word, posData, attributesList);
                }
                else
                {
                    foreach (string s in AttributeSplitter(data))
                    {
                        if (!RootAttribute.Converter().EnumExists(s))
                        {
                            throw new ApplicationException(
                                "Unrecognized attribute data [" + s + "] in data chunk :[" + data + "]");
                        }
                        RootAttribute rootAttribute = RootAttribute.Converter().GetEnum(s);
                        attributesList.Add(rootAttribute);
                    }
                    InferMorphemicAttributes(word, posData, attributesList);
                }
                return attributesList.Clone();
            }

            private void InferMorphemicAttributes(
                string word,
                PosInfo posData,
                ISet<RootAttribute> attributes)
            {

                char last = word[word.Length - 1];
                bool lastCharIsVowel = alphabet.IsVowel(last);

                int vowelCount = alphabet.VowelCount(word);
                switch (posData.primaryPos.LongForm)
                {
                    case PrimaryPos.Constants.Verb:
                        // if a verb ends with a wovel, and -Iyor suffix is appended, last vowel drops.
                        if (lastCharIsVowel)
                        {
                            attributes.Add(RootAttribute.ProgressiveVowelDrop);
                            attributes.Add(RootAttribute.Passive_In);
                        }
                        // if verb has more than 1 syllable and there is no Aorist_A label, add Aorist_I.
                        if (vowelCount > 1 && !attributes.Contains(RootAttribute.Aorist_A))
                        {
                            attributes.Add(RootAttribute.Aorist_I);
                        }
                        // if verb has 1 syllable and there is no Aorist_I label, add Aorist_A
                        if (vowelCount == 1 && !attributes.Contains(RootAttribute.Aorist_I))
                        {
                            attributes.Add(RootAttribute.Aorist_A);
                        }
                        if (last == 'l')
                        {
                            attributes.Add(RootAttribute.Passive_In);
                        }
                        if (lastCharIsVowel || (last == 'l' || last == 'r') && vowelCount > 1)
                        {
                            attributes.Add(RootAttribute.Causative_t);
                        }
                        break;
                    case PrimaryPos.Constants.Noun:
                    case PrimaryPos.Constants.Adjective:
                    case PrimaryPos.Constants.Duplicator:
                        // if a noun or adjective has more than one syllable and last letter is a stop consonant, add voicing.
                        if (vowelCount > 1
                            && alphabet.IsStopConsonant(last)
                            && posData.secondaryPos != SecondaryPos.ProperNoun
                            && posData.secondaryPos != SecondaryPos.Abbreviation
                            && !attributes.Contains(RootAttribute.NoVoicing)
                            && !attributes.Contains(RootAttribute.InverseHarmony))
                        {
                            attributes.Add(RootAttribute.Voicing);
                        }
                        if (word.EndsWith("nk") || word.EndsWith("og"))
                        {
                            if (!attributes.Contains(RootAttribute.NoVoicing)
                                && posData.secondaryPos != SecondaryPos.ProperNoun)
                            {
                                attributes.Add(RootAttribute.Voicing);
                            }
                        }
                        else if (vowelCount < 2 && !attributes.Contains(RootAttribute.Voicing))
                        {
                            attributes.Add(RootAttribute.NoVoicing);
                        }
                        break;
                }
            }

        }

        internal class PosInfo
        {
            internal PrimaryPos primaryPos;
            internal SecondaryPos secondaryPos;

            internal PosInfo(PrimaryPos primaryPos, SecondaryPos secondaryPos)
            {
                this.primaryPos = primaryPos;
                this.secondaryPos = secondaryPos;
            }

            public override string ToString()
            {
                return primaryPos.shortForm + "-" + secondaryPos.shortForm;
            }
        }
    }
}
