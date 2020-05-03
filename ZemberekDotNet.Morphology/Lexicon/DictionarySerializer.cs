using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ZemberekDotNet.Core.Enums;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon.TR;
using ZemberekDotNet.Morphology.Proto;

namespace ZemberekDotNet.Morphology.Lexicon
{
    public class DictionarySerializer
    {
        private static EnumConverter<Core.Turkish.PrimaryPos, Proto.PrimaryPos> primaryPosConverter = EnumConverter<Core.Turkish.PrimaryPos, Proto.PrimaryPos>.CreateConverter();
        private static EnumConverter<Core.Turkish.RootAttribute, Proto.RootAttribute> rootAttributeConverter = EnumConverter<Core.Turkish.RootAttribute, Proto.RootAttribute>.CreateConverter();
        private static EnumConverter<Core.Turkish.SecondaryPos, Proto.SecondaryPos> secondaryPosConverter = EnumConverter<Core.Turkish.SecondaryPos, Proto.SecondaryPos>.CreateConverter();

        public static RootLexicon Load(string path)
        {
            return GetDictionaryItems(File.ReadAllBytes(path));
        }

        public static void CreateDefaultDictionary(string path)
        {
            RootLexicon lexicon = RootLexicon.Builder()
                .AddTextDictionaryResources(TurkishDictionaryLoader.DefaultDictionaryResources)
                .Build();
            Save(lexicon, path);
        }

        private static RootLexicon GetDictionaryItems(byte[] bytes)
        {
            long start = DateTime.Now.Ticks;
            Dictionary readDictionary = Dictionary.Parser.ParseFrom(bytes);
            RootLexicon loadedLexicon = new RootLexicon();
            // some items contains references to other items. We need to apply this
            // link after creating the lexicon.
            Dictionary<string, string> referenceItemIdMap = new Dictionary<string, string>();
            foreach (Proto.DictionaryItem item in readDictionary.Items)
            {
                DictionaryItem actual = ConvertToDictionaryItem(item);
                loadedLexicon.Add(actual);
                if (item.Reference != null && !item.Reference.IsEmpty())
                {
                    referenceItemIdMap.Add(actual.id, item.Reference);
                }
            }

            foreach (string itemId in referenceItemIdMap.Keys)
            {
                DictionaryItem item = loadedLexicon.GetItemById(itemId);
                DictionaryItem reference = loadedLexicon.GetItemById(referenceItemIdMap.GetValueOrDefault(itemId));
                item.SetReferenceItem(reference);
            }

            long end = DateTime.Now.Ticks;
            Log.Info("Root lexicon created in %d ms.", new TimeSpan(end - start).TotalMilliseconds);

            return loadedLexicon;
        }

        public static void Save(RootLexicon lexicon, string outPath)
        {
            Dictionary dictionary = new Dictionary();
            foreach (DictionaryItem item in lexicon.GetAllItems())
            {
                dictionary.Items.Add(ConvertToProto(item));
            }

            if (File.Exists(outPath))
            {
                File.Delete(outPath);
            }
            using (FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                using (CodedOutputStream codedOutputStream = new CodedOutputStream(fileStream))
                {
                    dictionary.WriteTo(codedOutputStream);
                }
            }
        }

        public static void DummyMain(String[] args)
        {
            CreateDefaultDictionary("Resources/tr/lexicon.bin");
            SerializeDeserializeTest();
        }

        private static void SerializeDeserializeTest()
        {
            TurkishMorphology morphology = TurkishMorphology.CreateWithDefaults();
            RootLexicon lexicon = morphology.GetLexicon();
            Dictionary dictionary = new Dictionary();
            foreach (DictionaryItem item in lexicon.GetAllItems())
            {
                dictionary.Items.Add(ConvertToProto(item));
            }

            Console.WriteLine("Total size of serialized dictionary: " + dictionary.CalculateSize());
            string f = Path.Combine(Path.GetTempPath(), "lexicon.bin");

            using (FileStream fileStream = File.OpenWrite(f))
            {
                using (StreamWriter bos = new StreamWriter(fileStream))
                {
                    bos.Write(dictionary.ToByteArray());
                }
            }

            long start = DateTime.Now.Ticks;
            byte[] serialized = File.ReadAllBytes(f);
            long end = DateTime.Now.Ticks;
            Log.Info("Dictionary loaded in {0} ms.", new TimeSpan(end - start).TotalMilliseconds);

            start = DateTime.Now.Ticks;
            Dictionary readDictionary = Dictionary.Parser.ParseFrom(serialized);
            end = DateTime.Now.Ticks;
            Log.Info("Dictionary deserialized in {0} ms.", new TimeSpan(end - start).TotalMilliseconds);
            Console.WriteLine("Total size of read dictionary: " + readDictionary.CalculateSize());

            start = DateTime.Now.Ticks;
            RootLexicon loadedLexicon = new RootLexicon();
            foreach (Proto.DictionaryItem item in readDictionary.Items)
            {
                loadedLexicon.Add(ConvertToDictionaryItem(item));
            }
            end = DateTime.Now.Ticks;
            Log.Info("RootLexicon generated in {0} ms.", new TimeSpan(end - start).TotalMilliseconds);
        }

        private static Proto.DictionaryItem ConvertToProto(DictionaryItem item)
        {
            Proto.DictionaryItem dictionaryItem = new Proto.DictionaryItem();
            dictionaryItem.Lemma = item.lemma;
            dictionaryItem.Index = item.index;
            dictionaryItem.PrimaryPos = primaryPosConverter.ConvertTo(item.primaryPos, Proto.PrimaryPos.Unknown);

            string lowercaseLemma = item.lemma.ToLowerInvariant();
            if (item.root != null && !item.root.Equals(lowercaseLemma))
            {
                dictionaryItem.Root = item.root;
            }
            if (item.pronunciation != null && !item.pronunciation.Equals(lowercaseLemma))
            {
                dictionaryItem.Pronunciation = item.pronunciation;
            }
            if (item.secondaryPos != null && item.secondaryPos != Core.Turkish.SecondaryPos.None)
            {
                dictionaryItem.SecondaryPos = secondaryPosConverter.ConvertTo(
                    item.secondaryPos, Proto.SecondaryPos.Unknown);
            }
            if (item.attributes != null && !item.attributes.IsEmpty())
            {
                dictionaryItem.RootAttributes.AddRange(item.attributes.Select(attribute => rootAttributeConverter.ConvertTo(attribute, Proto.RootAttribute.Unknown)));
            }
            if (item.GetReferenceItem() != null)
            {
                dictionaryItem.Reference = item.GetReferenceItem().id;
            }
            return dictionaryItem;
        }

        private static DictionaryItem ConvertToDictionaryItem(Proto.DictionaryItem item)
        {
            HashSet<Core.Turkish.RootAttribute> attributes = new HashSet<Core.Turkish.RootAttribute>();
            foreach (Proto.RootAttribute rootAttribute in item.RootAttributes)
            {
                attributes.Add(rootAttributeConverter.ConvertBack(rootAttribute, Core.Turkish.RootAttribute.Unknown));
            }
            CultureInfo locale = attributes.Contains(Core.Turkish.RootAttribute.LocaleEn) ? CultureInfo.GetCultureInfo("en") : Turkish.Locale;
            string lowercaseLemma = item.Lemma.ToLower(locale);
            return new DictionaryItem(
                item.Lemma,
                item.Root.IsEmpty() ? lowercaseLemma : item.Root,
                item.Pronunciation.IsEmpty() ? lowercaseLemma : item.Pronunciation,
                primaryPosConverter.ConvertBack(item.PrimaryPos, Core.Turkish.PrimaryPos.Unknown),
                item.SecondaryPos == Proto.SecondaryPos.Unknown ? Core.Turkish.SecondaryPos.None : secondaryPosConverter.ConvertBack(item.SecondaryPos, Core.Turkish.SecondaryPos.UnknownSec),
                attributes,
                item.Index);
        }

    }
}
