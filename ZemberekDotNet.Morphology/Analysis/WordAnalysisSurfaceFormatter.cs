using System;
using System.Globalization;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Lexicon;

namespace ZemberekDotNet.Morphology.Analysis
{
    public class WordAnalysisSurfaceFormatter
    {
        /// <summary>
        /// Formats the morphological analysis result's surface form. Zemberek analyzer uses lowercase
        /// letters during operation. This methods creates the properly formatted surface form of an
        /// analysis. For example this method returns [Ankara'ya] for the analysis of [ankaraya]
        /// </summary>
        /// <param name="analysis"></param>
        /// <param name="apostrophe"></param>
        /// <returns>formatted word analysis.</returns>
        public string Format(SingleAnalysis analysis, String apostrophe)
        {
            DictionaryItem item = analysis.GetDictionaryItem();
            string ending = analysis.GetEnding();
            if (apostrophe != null || ApostropheRequired(analysis))
            {
                if (apostrophe == null)
                {
                    apostrophe = "'";
                }
                return ending.Length > 0 ?
                    item.NormalizedLemma() + apostrophe + ending : item.NormalizedLemma();
            }
            else
            {
                // because NoQuote is only used in Proper nouns, we can use the lemma. Otherwise root form is used
                // because it may be different than DictionaryItem. For example lemma is `kitap` but stem in analysis is
                // `kitab`
                if (item.attributes.Contains(RootAttribute.NoQuote))
                {
                    return item.NormalizedLemma() + ending;
                }
                else
                {
                    return analysis.GetStem() + ending;
                }
            }
        }

        public string Format(SingleAnalysis analysis)
        {
            return Format(analysis, null);
        }


        private bool ApostropheRequired(SingleAnalysis analysis)
        {
            DictionaryItem item = analysis.GetDictionaryItem();
            return (item.secondaryPos == SecondaryPos.ProperNoun && !item.attributes
                .Contains(RootAttribute.NoQuote))
                || (item.primaryPos == PrimaryPos.Numeral && item.HasAttribute(RootAttribute.Runtime))
                || item.secondaryPos == SecondaryPos.Date;
        }

        /// <summary>
        /// This method changes the case of the format of the morphological analysis result's surface form.
        /// For example, for inputs ["ankaraya" and CaseType.UPPER_CASE] this method returns [ANKARA'YA]
        /// Only LOWER_CASE, UPPER_CASE, TITLE_CASE and UPPER_CASE_ROOT_LOWER_CASE_ENDING are supported.
        /// For other case options, returns empty string.
        /// </summary>
        /// <param name="analysis"> Morphological analysis result.</param>
        /// <param name="type">case type.</param>
        /// <param name="apostrophe"></param>
        /// <returns>formatted result or empty string.</returns>
        public string FormatToCase(SingleAnalysis analysis, CaseType type, string apostrophe)
        {
            string formatted = Format(analysis, apostrophe);
            CultureInfo locale = analysis.GetDictionaryItem().HasAttribute(RootAttribute.LocaleEn) ?
               CultureInfo.GetCultureInfo("en") : Turkish.Locale;
            switch (type)
            {
                case CaseType.DEFAULT_CASE:
                    return formatted;
                case CaseType.LOWER_CASE:
                    return formatted.ToLower(locale);
                case CaseType.UPPER_CASE:
                    return formatted.ToUpper(locale);
                case CaseType.TITLE_CASE:
                    return Turkish.Capitalize(formatted);
                case CaseType.UPPER_CASE_ROOT_LOWER_CASE_ENDING:
                    string ending = analysis.GetEnding();
                    string lemmaUpper = analysis.GetDictionaryItem().NormalizedLemma().ToUpper(locale);
                    if (ending.Length == 0)
                    {
                        return lemmaUpper;
                    }
                    if (apostrophe != null || ApostropheRequired(analysis))
                    {
                        if (apostrophe == null)
                        {
                            apostrophe = "'";
                        }
                        return lemmaUpper + apostrophe + ending;
                    }
                    else
                    {
                        return lemmaUpper + ending;
                    }
                default:
                    return "";
            }
        }

        public string FormatToCase(SingleAnalysis analysis, CaseType type)
        {
            return FormatToCase(analysis, type, null);
        }


        //TODO: write tests.
        public bool CanBeFormatted(SingleAnalysis analysis, CaseType type)
        {
            bool proper = analysis.GetDictionaryItem().secondaryPos == SecondaryPos.ProperNoun ||
                analysis.GetDictionaryItem().secondaryPos == SecondaryPos.Abbreviation;
            switch (type)
            {
                case CaseType.LOWER_CASE:
                    return !proper;
                case CaseType.UPPER_CASE:
                case CaseType.TITLE_CASE:
                case CaseType.DEFAULT_CASE:
                    return true;
                case CaseType.UPPER_CASE_ROOT_LOWER_CASE_ENDING:
                    return proper;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Guesses the current case type of the word.
        /// <pre>
        /// for example,
        /// "ankaraya"  -> CaseType.LOWER_CASE
        /// "Ankara'ya" -> CaseType.TITLE_CASE
        /// "ANKARAYA"  -> CaseType.UPPER_CASE
        /// "anKAraYA"  -> CaseType.MIXED_CASE
        /// "ANKARA'ya" -> CaseType.UPPER_CASE_ROOT_LOWER_CASE_ENDING
        /// "12"        -> CaseType.DEFAULT_CASE
        /// "12'de"     -> CaseType.LOWER_CASE
        /// "A"         -> CaseType.UPPER_CASE
        /// "A1"        -> CaseType.UPPER_CASE
        /// </pre>
        /// </summary>
        /// <param name="input">input word</param>
        /// <returns>guessed CaseType</returns>
        public CaseType GuessCase(string input)
        {
            bool firstLetterUpperCase = false;
            int lowerCaseCount = 0;
            int upperCaseCount = 0;
            int letterCount = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (!char.IsLetter(c))
                {
                    continue;
                }
                if (i == 0)
                {
                    firstLetterUpperCase = char.IsUpper(c);
                    if (firstLetterUpperCase)
                    {
                        upperCaseCount++;
                    }
                    else
                    {
                        lowerCaseCount++;
                    }
                }
                else
                {
                    if (char.IsUpper(c))
                    {
                        upperCaseCount++;
                    }
                    else if (char.IsLower(c))
                    {
                        lowerCaseCount++;
                    }
                }
                letterCount++;
            }
            if (letterCount == 0)
            {
                return CaseType.DEFAULT_CASE;
            }
            if (letterCount == lowerCaseCount)
            {
                return CaseType.LOWER_CASE;
            }
            if (letterCount == upperCaseCount)
            {
                return CaseType.UPPER_CASE;
            }
            if (firstLetterUpperCase && letterCount == lowerCaseCount + 1)
            {
                return letterCount == 1 ? CaseType.UPPER_CASE : CaseType.TITLE_CASE;
            }
            int apostropheIndex = input.IndexOf('\'');
            if (apostropheIndex > 0 && apostropheIndex < input.Length - 1)
            {
                if (GuessCase(input.Substring(0, apostropheIndex)) == CaseType.UPPER_CASE &&
                    GuessCase(input.Substring(apostropheIndex + 1)) == CaseType.LOWER_CASE)
                {
                    return CaseType.UPPER_CASE_ROOT_LOWER_CASE_ENDING;
                }
            }
            return CaseType.MIXED_CASE;
        }

        public enum CaseType
        {
            DEFAULT_CASE, // numbers are considered default case.
            LOWER_CASE,
            UPPER_CASE,
            TITLE_CASE,
            UPPER_CASE_ROOT_LOWER_CASE_ENDING,
            MIXED_CASE,
        }
    }
}
