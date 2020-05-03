using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Turkish;

namespace ZemberekDotNet.Morphology.Lexicon
{
    /// <summary>
    /// DictionaryItem represents an entity from a dictionary.
    /// </summary>
    public class DictionaryItem
    {
        public static readonly DictionaryItem UNKNOWN = new DictionaryItem("UNK", "UNK", "UNK", PrimaryPos.Unknown, SecondaryPos.UnknownSec);

        /// <summary>
        /// the exact surface form of the item used in dictionary.
        /// </summary>
        public readonly string lemma;

        /// <summary>
        /// Form which will be used during graph generation. Such as, dictionary Item [gelmek Verb]'s root
        /// is "gel"
        /// </summary>
        public readonly string root;

        /// <summary>
        /// Primary POS information
        /// </summary>
        public readonly PrimaryPos primaryPos;

        /// <summary>
        /// Secondary POS information
        /// </summary>
        public readonly SecondaryPos secondaryPos;

        /// <summary>
        /// Attributes that this item carries. Such as voicing or vowel drop.
        /// </summary>
        public readonly ISet<RootAttribute> attributes;

        /// <summary>
        /// Pronunciations of the item. TODO: This should be converted to an actual 'Pronunciation' item
        /// </summary>
        public readonly string pronunciation;

        /// <summary>
        /// This is the unique ID of the item. It is generated from Pos and lemma. If there are multiple
        /// items with same POS and Lemma user needs to add an index for distinction. Structure of the ID:
        /// lemma_POS or lemma_POS_index
        /// </summary>
        public string id;

        private DictionaryItem referenceItem;

        public int index;

        public DictionaryItem(string lemma,
            string root,
            string pronunciation,
            PrimaryPos primaryPos,
            SecondaryPos secondaryPos,
            ISet<RootAttribute> attributes)
        {
            this.pronunciation = pronunciation;
            this.lemma = lemma;
            this.primaryPos = primaryPos;
            this.secondaryPos = secondaryPos;
            this.attributes = attributes;
            this.root = root;
            this.index = 0;
            this.id = GenerateId(lemma, primaryPos, secondaryPos, 0);
        }

        public DictionaryItem(
            string lemma,
            string root,
            string pronunciation,
            PrimaryPos primaryPos,
            SecondaryPos secondaryPos,
            ISet<RootAttribute> attributes,
            int index)
        {
            this.pronunciation = pronunciation;
            this.lemma = lemma;
            this.primaryPos = primaryPos;
            this.secondaryPos = secondaryPos;
            this.attributes = attributes;
            this.root = root;
            this.index = index;
            this.id = GenerateId(lemma, primaryPos, secondaryPos, index);
        }

        public DictionaryItem(string lemma,
            string root,
            PrimaryPos primaryPos,
            SecondaryPos secondaryPos,
            ISet<RootAttribute> attributes)
        {
            this.lemma = lemma;
            this.pronunciation = root;
            this.primaryPos = primaryPos;
            this.secondaryPos = secondaryPos;
            this.attributes = attributes;
            this.root = root;
            this.index = 0;
            this.id = GenerateId(lemma, primaryPos, secondaryPos, 0);
        }

        public DictionaryItem(string lemma,
            string root,
            string pronunciation,
            PrimaryPos primaryPos,
            SecondaryPos secondaryPos)
        {
            this.lemma = lemma;
            this.pronunciation = pronunciation;
            this.primaryPos = primaryPos;
            this.secondaryPos = secondaryPos;
            this.attributes = new HashSet<RootAttribute>();
            this.root = root;
            this.index = 0;
            this.id = GenerateId(lemma, primaryPos, secondaryPos, 0);
        }

        public static string GenerateId(string lemma, PrimaryPos pos, SecondaryPos spos, int index)
        {
            StringBuilder sb = new StringBuilder(lemma).Append("_").Append(pos.shortForm);
            if (spos != null && spos != SecondaryPos.None)
            {
                sb.Append("_").Append(spos.shortForm);
            }
            if (index > 0)
            {
                sb.Append("_").Append(index);
            }
            return sb.ToString();
        }

        public DictionaryItem GetReferenceItem()
        {
            return referenceItem;
        }

        public void SetReferenceItem(DictionaryItem referenceItem)
        {
            this.referenceItem = referenceItem;
        }

        public bool IsUnknown()
        {
            return this == UNKNOWN;
        }

        public bool HasAttribute(RootAttribute attribute)
        {
            return attributes.Contains(attribute);
        }

        public bool HasAnyAttribute(params RootAttribute[] attributes)
        {
            foreach (RootAttribute attribute in attributes)
            {
                if (this.attributes.Contains(attribute))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>if this is a Verb, removes -mek -mak suffix. Otherwise returns the `lemma`</returns>
        public string NormalizedLemma()
        {
            return primaryPos == PrimaryPos.Verb ? lemma.Substring(0, lemma.Length - 3) : lemma;
        }

        public string GetId()
        {
            return id;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(lemma + " " + "[P:" + primaryPos.shortForm);
            if (secondaryPos != null && secondaryPos != SecondaryPos.None)
            {
                sb.Append(", ").Append(secondaryPos.shortForm);
            }
            if (attributes != null && attributes.IsEmpty())
            {
                sb.Append("]");
            }
            else
            {
                PrintAttributes(sb, attributes);
            }
            return sb.ToString();
        }

        public bool HasDifferentPronunciation()
        {
            return !pronunciation.Equals(root);
        }

        private void PrintAttributes(StringBuilder sb, ISet<RootAttribute> attrs)
        {
            if (attrs != null && !attrs.IsEmpty())
            {
                sb.Append("; A:");
            }
            else
            {
                return;
            }
            int i = 0;
            foreach (RootAttribute attribute in attrs)
            {
                sb.Append(attribute.GetStringForm());
                if (i++ < attrs.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
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

            DictionaryItem that = (DictionaryItem)o;

            return id.Equals(that.id);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
