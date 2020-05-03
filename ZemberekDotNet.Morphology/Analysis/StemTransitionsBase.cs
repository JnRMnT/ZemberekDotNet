using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Morphology.Morphotactics;

namespace ZemberekDotNet.Morphology.Analysis
{
    public abstract class StemTransitionsBase
    {
        protected TurkishMorphotactics morphotactics;
        private TurkishAlphabet alphabet = TurkishAlphabet.Instance;
        protected RootLexicon lexicon;

        private HashSet<RootAttribute> modifiers = new HashSet<RootAttribute> {
            RootAttribute.Doubling,
            RootAttribute.LastVowelDrop,
            RootAttribute.ProgressiveVowelDrop,
            RootAttribute.InverseHarmony,
            RootAttribute.Voicing,
            RootAttribute.CompoundP3sg,
            RootAttribute.CompoundP3sgRoot
        };

        /// <summary>
        /// Generates StemTransition objects from the dictionary item. <p>Most of the time a single
        /// StemNode is generated.
        /// </summary>
        /// <param name="item">DictionaryItem</param>
        /// <returns>one or more StemTransition objects.</returns>
        public List<StemTransition> Generate(DictionaryItem item)
        {
            if (specialRoots.Contains(item.id))
            {
                return HandleSpecialRoots(item);
            }
            if (HasModifierAttribute(item))
            {
                return GenerateModifiedRootNodes(item);
            }
            else
            {
                AttributeSet<PhoneticAttribute> phoneticAttributes = CalculateAttributes(item.pronunciation.ToCharArray());
                StemTransition transition = new StemTransition(
                    item.root,
                    item,
                    phoneticAttributes,
                    morphotactics.GetRootState(item, phoneticAttributes)
                );

                return new List<StemTransition> { transition };
            }
        }

        private bool HasModifierAttribute(DictionaryItem item)
        {
            foreach (RootAttribute attr in modifiers)
            {
                if (item.attributes.Contains(attr))
                {
                    return true;
                }
            }
            return false;
        }

        private AttributeSet<PhoneticAttribute> CalculateAttributes(char[] input)
        {
            return AttributesHelper.GetMorphemicAttributes(input);
        }

        private List<StemTransition> GenerateModifiedRootNodes(DictionaryItem dicItem)
        {

            StringBuilder modifiedSeq = new StringBuilder(dicItem.pronunciation);

            AttributeSet<PhoneticAttribute> originalAttrs = CalculateAttributes(dicItem.pronunciation.ToCharArray());
            AttributeSet<PhoneticAttribute> modifiedAttrs = originalAttrs.Copy();

            MorphemeState modifiedRootState = null;
            MorphemeState unmodifiedRootState = null;

            foreach (RootAttribute attribute in dicItem.attributes)
            {

                // generate other boundary attributes and modified root state.
                switch (attribute.GetStringForm())
                {
                    case RootAttribute.Constants.Voicing:
                        char last = alphabet.LastChar(modifiedSeq.ToString().ToCharArray());
                        char voiced = alphabet.Voice(last);
                        if (last == voiced)
                        {
                            throw new LexiconException("Voicing letter is not proper in:" + dicItem);
                        }
                        if (dicItem.lemma.EndsWith("nk"))
                        {
                            voiced = 'g';
                        }
                        modifiedSeq[modifiedSeq.Length - 1] = voiced;
                        modifiedAttrs.Remove(PhoneticAttribute.LastLetterVoicelessStop);
                        originalAttrs.Add(PhoneticAttribute.ExpectsConsonant);
                        modifiedAttrs.Add(PhoneticAttribute.ExpectsVowel);
                        // TODO: find a better way for this.
                        modifiedAttrs.Add(PhoneticAttribute.CannotTerminate);
                        break;
                    case RootAttribute.Constants.Doubling:
                        modifiedSeq.Append(alphabet.LastChar(modifiedSeq.ToString().ToCharArray()));
                        originalAttrs.Add(PhoneticAttribute.ExpectsConsonant);
                        modifiedAttrs.Add(PhoneticAttribute.ExpectsVowel);
                        modifiedAttrs.Add(PhoneticAttribute.CannotTerminate);
                        break;
                    case RootAttribute.Constants.LastVowelDrop:
                        TurkicLetter lastLetter = alphabet.GetLastLetter(modifiedSeq.ToString().ToCharArray());
                        if (lastLetter.IsVowel())
                        {
                            modifiedSeq.Remove(modifiedSeq.Length - 1, 1);
                            modifiedAttrs.Add(PhoneticAttribute.ExpectsConsonant);
                            modifiedAttrs.Add(PhoneticAttribute.CannotTerminate);
                        }
                        else
                        {
                            modifiedSeq.Remove(modifiedSeq.Length - 2, 1);
                            if (!dicItem.primaryPos.Equals(PrimaryPos.Verb))
                            {
                                originalAttrs.Add(PhoneticAttribute.ExpectsConsonant);
                            }
                            else
                            {
                                unmodifiedRootState = morphotactics.verbLastVowelDropUnmodRoot_S;
                                modifiedRootState = morphotactics.verbLastVowelDropModRoot_S;
                            }
                            modifiedAttrs.Add(PhoneticAttribute.ExpectsVowel);
                            modifiedAttrs.Add(PhoneticAttribute.CannotTerminate);
                        }
                        break;
                    case RootAttribute.Constants.InverseHarmony:
                        originalAttrs.Add(PhoneticAttribute.LastVowelFrontal);
                        originalAttrs.Remove(PhoneticAttribute.LastVowelBack);
                        modifiedAttrs.Add(PhoneticAttribute.LastVowelFrontal);
                        modifiedAttrs.Remove(PhoneticAttribute.LastVowelBack);
                        break;
                    case RootAttribute.Constants.ProgressiveVowelDrop:
                        if (modifiedSeq.Length > 1)
                        {
                            modifiedSeq.Remove(modifiedSeq.Length - 1, 1);
                            if (alphabet.ContainsVowel(modifiedSeq.ToString().ToCharArray()))
                            {
                                modifiedAttrs = CalculateAttributes(modifiedSeq.ToString().ToCharArray());
                            }
                            modifiedAttrs.Add(PhoneticAttribute.LastLetterDropped);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (unmodifiedRootState == null)
            {
                unmodifiedRootState = morphotactics.GetRootState(dicItem, originalAttrs);
            }
            StemTransition original = new StemTransition(
                dicItem.root,
                dicItem,
                originalAttrs,
                unmodifiedRootState);

            // if modified root state is not defined in the switch block, get it from morphotactics.
            if (modifiedRootState == null)
            {
                modifiedRootState = morphotactics.GetRootState(dicItem, modifiedAttrs);
            }

            StemTransition modified = new StemTransition(
                modifiedSeq.ToString(),
                dicItem,
                modifiedAttrs,
                modifiedRootState);

            if (original.Equals(modified))
            {
                return new List<StemTransition> { original };
            }
            return new List<StemTransition> { original, modified };
        }

        HashSet<string> specialRoots = new HashSet<string> {
            "içeri_Noun", "içeri_Adj", "dışarı_Adj", "şura_Noun", "bura_Noun", "ora_Noun",
            "dışarı_Noun", "dışarı_Postp", "yukarı_Noun", "yukarı_Adj", "ileri_Noun",
            "ben_Pron_Pers", "sen_Pron_Pers", "demek_Verb", "yemek_Verb", "imek_Verb",
            "birbiri_Pron_Quant", "çoğu_Pron_Quant", "öbürü_Pron_Quant", "birçoğu_Pron_Quant"
        };

        private List<StemTransition> HandleSpecialRoots(DictionaryItem item)
        {
            string id = item.GetId();
            AttributeSet<PhoneticAttribute> originalAttrs = CalculateAttributes(item.pronunciation.ToCharArray());
            StemTransition original, modified;
            MorphemeState unmodifiedRootState = morphotactics.GetRootState(item, originalAttrs);

            switch (id)
            {
                case "içeri_Noun":
                case "içeri_Adj":
                case "dışarı_Adj":
                case "dışarı_Noun":
                case "dışarı_Postp":
                case "yukarı_Noun":
                case "ileri_Noun":
                case "yukarı_Adj":
                case "şura_Noun":
                case "bura_Noun":
                case "ora_Noun":
                    original = new StemTransition(item.root, item, originalAttrs, unmodifiedRootState);

                    MorphemeState rootForModified;
                    switch (item.primaryPos.LongForm)
                    {
                        case PrimaryPos.Constants.Noun:
                            rootForModified = morphotactics.nounLastVowelDropRoot_S;
                            break;
                        case PrimaryPos.Constants.Adjective:
                            rootForModified = morphotactics.adjLastVowelDropRoot_S;
                            break;
                        // TODO: check postpositive case. Maybe it is not required.
                        case PrimaryPos.Constants.PostPositive:
                            rootForModified = morphotactics.adjLastVowelDropRoot_S;
                            break;
                        default:
                            throw new InvalidOperationException("No root morpheme state found for " + item);
                    }
                    string m = item.root.Substring(0, item.root.Length - 1);
                    modified = new StemTransition(m, item, CalculateAttributes(m.ToCharArray()), rootForModified);
                    modified.GetPhoneticAttributes().Add(PhoneticAttribute.ExpectsConsonant);
                    modified.GetPhoneticAttributes().Add(PhoneticAttribute.CannotTerminate);
                    return new List<StemTransition> { original, modified };

                case "ben_Pron_Pers":
                case "sen_Pron_Pers":
                    original = new StemTransition(item.root, item, originalAttrs, unmodifiedRootState);
                    if (item.lemma.Equals("ben"))
                    {
                        modified = new StemTransition("ban", item, CalculateAttributes("ban".ToCharArray()),
                            morphotactics.pronPers_Mod_S);
                    }
                    else
                    {
                        modified = new StemTransition("san", item, CalculateAttributes("san".ToCharArray()),
                            morphotactics.pronPers_Mod_S);
                    }
                    original.GetPhoneticAttributes().Add(PhoneticAttribute.UnModifiedPronoun);
                    modified.GetPhoneticAttributes().Add(PhoneticAttribute.ModifiedPronoun);
                    return new List<StemTransition> { original, modified };
                case "demek_Verb":
                case "yemek_Verb":
                    original = new StemTransition(item.root, item, originalAttrs, morphotactics.vDeYeRoot_S);
                    switch (item.lemma)
                    {
                        case "demek":
                            modified = new StemTransition("di", item, CalculateAttributes("di".ToCharArray()),
                                morphotactics.vDeYeRoot_S);
                            break;
                        default:
                            modified = new StemTransition("yi", item, CalculateAttributes("yi".ToCharArray()),
                                morphotactics.vDeYeRoot_S);
                            break;
                    }
                    return new List<StemTransition> { original, modified };
                case "imek_Verb":
                    original = new StemTransition(item.root, item, originalAttrs, morphotactics.imekRoot_S);
                    return new List<StemTransition> { original };
                case "birbiri_Pron_Quant":
                case "çoğu_Pron_Quant":
                case "öbürü_Pron_Quant":
                case "birçoğu_Pron_Quant":
                    original = new StemTransition(item.root, item, originalAttrs, morphotactics.pronQuant_S);

                    switch (item.lemma)
                    {
                        case "birbiri":
                            modified = new StemTransition("birbir", item, CalculateAttributes("birbir".ToCharArray()),
                                morphotactics.pronQuantModified_S);
                            break;
                        case "çoğu":
                            modified = new StemTransition("çok", item, CalculateAttributes("çok".ToCharArray()),
                                morphotactics.pronQuantModified_S);
                            break;
                        case "öbürü":
                            modified = new StemTransition("öbür", item, CalculateAttributes("öbür".ToCharArray()),
                                morphotactics.pronQuantModified_S);
                            break;
                        default:
                            modified = new StemTransition("birçok", item, CalculateAttributes("birçok".ToCharArray()),
                                morphotactics.pronQuantModified_S);
                            break;
                    }
                    original.GetPhoneticAttributes().Add(PhoneticAttribute.UnModifiedPronoun);
                    modified.GetPhoneticAttributes().Add(PhoneticAttribute.ModifiedPronoun);
                    return new List<StemTransition> { original, modified };
                default:
                    throw new ArgumentException(
                        "Lexicon Item with special stem change cannot be handled:" + item);
            }

        }
    }
}
