using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology.Analysis.TR;
using ZemberekDotNet.Morphology.Lexicon;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// TODO: Code requires serious testing and review.
    /// TODO: For unknown pronouns, do not analyze as regular nouns if apostrophe is not in the
    /// correct place. Such as [obama'ymış] should not have "oba" root solution.
    /// </summary>
    public class UnidentifiedTokenAnalyzer
    {
        public static readonly TurkishAlphabet ALPHABET = TurkishAlphabet.Instance;
        private static Dictionary<string, string> ordinalMap = TurkishNumbers.GetOrdinalMap();

        private RuleBasedAnalyzer analyzer;
        private RootLexicon lexicon;
        private TurkishAlphabet alphabet = TurkishAlphabet.Instance;
        private TurkishNumeralEndingMachine numeralEndingMachine = new TurkishNumeralEndingMachine();

        public UnidentifiedTokenAnalyzer(RuleBasedAnalyzer analyzer)
        {
            this.analyzer = analyzer;
            this.lexicon = analyzer.GetLexicon();
        }

        public static readonly Regex nonLettersPattern =
      new Regex("[^" + TurkishAlphabet.Instance.GetAllLetters() + "]");

        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<SingleAnalysis> Analyze(Token token)
        {
            SecondaryPos sPos = GuessSecondaryPosType(token);
            String word = token.GetText();

            // TODO: for now, for regular words and numbers etc, use the analyze method.
            if (sPos == SecondaryPos.None)
            {
                if (word.Contains("?"))
                {
                    return new List<SingleAnalysis>();
                }
                if (alphabet.ContainsDigit(word))
                {
                    return TryNumeral(token);
                }
                else
                {
                    return AnalyzeWord(word,
                        word.Contains(".") ? SecondaryPos.Abbreviation : SecondaryPos.ProperNoun);
                }
            }

            if (sPos == SecondaryPos.RomanNumeral)
            {
                return GetForRomanNumeral(token);
            }
            if (sPos == SecondaryPos.Date || sPos == SecondaryPos.Clock)
            {
                return TryNumeral(token);
            }

            //TODO: consider returning analysis results without interfering with analyzer.
            string normalized = nonLettersPattern.Replace(word, "");
            DictionaryItem item = new DictionaryItem(word, word, normalized, PrimaryPos.Noun, sPos);

            if (sPos == SecondaryPos.HashTag ||
                sPos == SecondaryPos.Email ||
                sPos == SecondaryPos.Url ||
                sPos == SecondaryPos.Mention)
            {
                return AnalyzeWord(word, sPos);
            }

            bool itemDoesNotExist = !lexicon.ContainsItem(item);
            if (itemDoesNotExist)
            {
                item.attributes.Add(RootAttribute.Runtime);
                analyzer.GetStemTransitions().AddDictionaryItem(item);
            }
            List<SingleAnalysis> results = analyzer.Analyze(word);
            if (itemDoesNotExist)
            {
                analyzer.GetStemTransitions().RemoveDictionaryItem(item);
            }
            return results;
        }

        private SecondaryPos GuessSecondaryPosType(Token token)
        {
            switch (token.GetTokenType())
            {
                case Token.Type.Email:
                    return SecondaryPos.Email;
                case Token.Type.URL:
                    return SecondaryPos.Url;
                case Token.Type.HashTag:
                    return SecondaryPos.HashTag;
                case Token.Type.Mention:
                    return SecondaryPos.Mention;
                case Token.Type.Emoticon:
                    return SecondaryPos.Emoticon;
                case Token.Type.RomanNumeral:
                    return SecondaryPos.RomanNumeral;
                case Token.Type.Abbreviation:
                    return SecondaryPos.Abbreviation;
                case Token.Type.Date:
                    return SecondaryPos.Date;
                case Token.Type.Time: // TODO: consider SecondaryPos.Time -> Temporal and Clock -> Time
                    return SecondaryPos.Clock;

                default:
                    return SecondaryPos.None;
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<SingleAnalysis> AnalyzeWord(string word, SecondaryPos secondaryPos)
        {
            int index = word.IndexOf('\'');
            if (index >= 0)
            {
                return TryWordWithApostrophe(word, secondaryPos);
            }
            else if (secondaryPos == SecondaryPos.ProperNoun
              || secondaryPos == SecondaryPos.Abbreviation)
            {
                // TODO: should we allow analysis of unknown words that starts wıth a capital letter
                // without apostrophe as Proper nouns?
                return new List<SingleAnalysis>();
            }
            else
            {
                return TryWithoutApostrophe(word, secondaryPos);
            }
        }

        private List<SingleAnalysis> TryWithoutApostrophe(string word, SecondaryPos secondaryPos)
        {
            string normalized = null;
            TurkishAlphabet alphabet = TurkishAlphabet.Instance;
            if (alphabet.ContainsForeignDiacritics(word))
            {
                normalized = alphabet.ForeignDiacriticsToTurkish(word);
            }
            normalized = normalized == null ?
                alphabet.Normalize(word) :
                alphabet.Normalize(normalized);

            bool capitalize = secondaryPos == SecondaryPos.ProperNoun ||
                secondaryPos == SecondaryPos.Abbreviation;

            //TODO: should we remove dots with normalization?
            String pronunciation = GuessPronunciation(normalized.Replace("[.]", ""));

            DictionaryItem item = new DictionaryItem(
                capitalize ? Turkish.Capitalize(normalized) : normalized,
                normalized,
                pronunciation,
                PrimaryPos.Noun,
                secondaryPos);

            if (!alphabet.ContainsVowel(pronunciation))
            {
                List<SingleAnalysis> result = new List<SingleAnalysis>(1);
                result.Add(SingleAnalysis.Dummy(word, item));
                return result;
            }

            bool itemDoesNotExist = !lexicon.ContainsItem(item);

            if (itemDoesNotExist)
            {
                item.attributes.Add(RootAttribute.Runtime);
                analyzer.GetStemTransitions().AddDictionaryItem(item);
            }
            List<SingleAnalysis> results = analyzer.Analyze(normalized);
            if (itemDoesNotExist)
            {
                analyzer.GetStemTransitions().RemoveDictionaryItem(item);
            }
            return results;
        }

        private List<SingleAnalysis> TryWordWithApostrophe(string word, SecondaryPos secondaryPos)
        {
            string normalized = TurkishAlphabet.Instance.NormalizeApostrophe(word);

            int index = normalized.IndexOf('\'');
            if (index <= 0 || index == normalized.Length - 1)
            {
                return new List<SingleAnalysis>();
            }

            string stem = normalized.Substring(0, index);
            string ending = normalized.Substring(index + 1);

            StemAndEnding se = new StemAndEnding(stem, ending);
            //TODO: should we remove dots with normalization?
            string stemNormalized = Regex.Replace(TurkishAlphabet.Instance.Normalize(se.stem), "[.]", "");
            string endingNormalized = TurkishAlphabet.Instance.Normalize(se.ending);
            string pronunciation = GuessPronunciation(stemNormalized);

            bool capitalize = secondaryPos == SecondaryPos.ProperNoun ||
                secondaryPos == SecondaryPos.Abbreviation;

            bool pronunciationPossible = alphabet.ContainsVowel(pronunciation);

            DictionaryItem item = new DictionaryItem(
                capitalize ? Turkish.Capitalize(normalized) : (pronunciationPossible ? stem : word),
                stemNormalized,
                pronunciation,
                PrimaryPos.Noun,
                secondaryPos);

            if (!pronunciationPossible)
            {
                List<SingleAnalysis> result = new List<SingleAnalysis>(1);
                result.Add(SingleAnalysis.Dummy(word, item));
                return result;
            }

            bool itemDoesNotExist = !lexicon.ContainsItem(item);
            if (itemDoesNotExist)
            {
                item.attributes.Add(RootAttribute.Runtime);
                analyzer.GetStemTransitions().AddDictionaryItem(item);
            }
            String toParse = stemNormalized + endingNormalized;

            List<SingleAnalysis> noQuotesParses = analyzer.Analyze(toParse);

            if (itemDoesNotExist)
            {
                analyzer.GetStemTransitions().RemoveDictionaryItem(item);
            }

            List<SingleAnalysis> analyses = noQuotesParses.Where(noQuotesParse => noQuotesParse.GetStem().Equals(stemNormalized)).ToList();

            return analyses;
        }

        PronunciationGuesser guesser = new PronunciationGuesser();

        private string GuessPronunciation(string stem)
        {
            if (!Turkish.Alphabet.ContainsVowel(stem))
            {
                return guesser.ToTurkishLetterPronunciations(stem);
            }
            else
            {
                return stem;
            }
        }

        private StemAndEnding GetFromNumeral(string s)
        {
            if (s.Contains("'"))
            {
                int i = s.IndexOf('\'');
                return new StemAndEnding(s.Substring(0, i), s.Substring(i + 1));
            }
            int j = 0;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];
                int k = c - '0';
                if (c == '.')
                { // ordinal
                    break;
                }
                if (k < 0 || k > 9)
                {
                    j++;
                }
                else
                {
                    break;
                }
            }
            int cutPoint = s.Length - j;
            return new StemAndEnding(s.Substring(0, cutPoint), s.Substring(cutPoint));
        }

        private List<SingleAnalysis> GetForRomanNumeral(Token token)
        {
            string content = token.GetText();
            StemAndEnding se;
            if (content.Contains("'"))
            {
                int i = content.IndexOf('\'');
                se = new StemAndEnding(content.Substring(0, i), content.Substring(i + 1));
            }
            else
            {
                se = new StemAndEnding(content, "");
            }
            string ss = se.stem;
            if (se.stem.EndsWith("."))
            {
                ss = se.stem.Substring(0, se.stem.Length - 1);
            }
            int decimalNumber = TurkishNumbers.RomanToDecimal(ss);
            if (decimalNumber == -1)
            {
                return new List<SingleAnalysis>(0);
            }

            string lemma;
            if (se.stem.EndsWith("."))
            {
                lemma = numeralEndingMachine.Find(decimalNumber.ToString());
                lemma = ordinalMap.GetValueOrDefault(lemma);
            }
            else
            {
                lemma = numeralEndingMachine.Find(decimalNumber.ToString());
            }
            List<SingleAnalysis> results = new List<SingleAnalysis>(1);
            string toParse;
            if (se.ending.Length > 0 && lemma.Equals("dört") &&
                ALPHABET.IsVowel(se.ending[0]))
            {
                toParse = "dörd" + se.ending;
            }
            else
            {
                toParse = lemma + se.ending;
            }
            List<SingleAnalysis> res = analyzer.Analyze(toParse);
            foreach (SingleAnalysis re in res)
            {
                if (re.GetDictionaryItem().primaryPos != PrimaryPos.Numeral)
                {
                    continue;
                }
                DictionaryItem runTimeItem = new DictionaryItem(
                    se.stem,
                    se.stem,
                    content + lemma,
                    PrimaryPos.Numeral,
                    SecondaryPos.RomanNumeral);
                runTimeItem.attributes.Add(RootAttribute.Runtime);
                results.Add(re.CopyFor(runTimeItem, se.stem));
            }
            return results;
        }

        private List<SingleAnalysis> TryNumeral(Token token)
        {
            string s = token.GetText();
            s = s.ToLower(TurkishAlphabet.TR);
            StemAndEnding se = GetFromNumeral(s);
            String lemma;
            if (se.stem.EndsWith("."))
            {
                String ss = se.stem.Substring(0, se.stem.Length - 1);
                lemma = numeralEndingMachine.Find(ss);
                lemma = ordinalMap.GetValueOrDefault(lemma);
            }
            else
            {
                lemma = numeralEndingMachine.Find(se.stem);
            }

            List<SingleAnalysis> results = new List<SingleAnalysis>(1);

            foreach (Numerals numerals in Numerals.Values)
            {
                Match m = numerals.pattern.Match(se.stem);
                if (m.Success)
                {
                    string toParse;
                    if (se.ending.Length > 0 && lemma.Equals("dört") &&
                        ALPHABET.IsVowel(se.ending[0]))
                    {
                        toParse = "dörd" + se.ending;
                    }
                    else
                    {
                        toParse = lemma + se.ending;
                    }
                    List<SingleAnalysis> res = analyzer.Analyze(toParse);
                    foreach (SingleAnalysis re in res)
                    {
                        if (re.GetDictionaryItem().primaryPos != PrimaryPos.Numeral)
                        {
                            continue;
                        }
                        DictionaryItem runTimeItem = new DictionaryItem(
                            se.stem,
                            se.stem,
                            s + lemma,
                            PrimaryPos.Numeral,
                            numerals.secondaryPos);
                        runTimeItem.attributes.Add(RootAttribute.Runtime);
                        results.Add(re.CopyFor(runTimeItem, se.stem));
                    }
                }
            }
            return results;
        }

        // TODO: move this functionality to Lexer.
        public class Numerals : IClassEnum
        {
            public struct Constants
            {
                public const string CARDINAL = "CARDINAL";
                public const string ORDINAL = "ORDINAL";
                public const string RANGE = "RANGE";
                public const string RATIO = "RATIO";
                public const string REAL = "REAL";
                public const string DISTRIB = "DISTRIB";
                public const string PERCENTAGE_BEFORE = "PERCENTAGE_BEFORE";
                public const string TIME = "TIME";
                public const string DATE = "DATE";
            }
            public static readonly Numerals CARDINAL = new Numerals(0, Constants.CARDINAL, "#", "^[+\\-]?\\d+$", SecondaryPos.Cardinal);
            public static readonly Numerals ORDINAL = new Numerals(1, Constants.ORDINAL, "#.", "^[+\\-]?[0-9]+[.]$", SecondaryPos.Ordinal);
            public static readonly Numerals RANGE = new Numerals(2, Constants.RANGE, "#-#", "^[+\\-]?[0-9]+-[0-9]+$", SecondaryPos.Range);
            public static readonly Numerals RATIO = new Numerals(3, Constants.RATIO, "#/#", "^[+\\-]?[0-9]+/[0-9]+$", SecondaryPos.Ratio);
            public static readonly Numerals REAL = new Numerals(4, Constants.REAL, "#,#", "^[+\\-]?[0-9]+[,][0-9]+$|^[+\\-]?[0-9]+[.][0-9]+$", SecondaryPos.Real);
            public static readonly Numerals DISTRIB = new Numerals(5, Constants.DISTRIB, "#DIS", "^\\d+[^0-9]+$", SecondaryPos.Distribution);
            public static readonly Numerals PERCENTAGE_BEFORE = new Numerals(6, Constants.PERCENTAGE_BEFORE, "%#", "(^|[+\\-])(%)(\\d+)((([.]|[,])(\\d+))|)$", SecondaryPos.Percentage);
            public static readonly Numerals TIME = new Numerals(7, Constants.TIME, "#:#", "^([012][0-9]|[1-9])([.]|[:])([0-5][0-9])$", SecondaryPos.Clock);
            public static readonly Numerals DATE = new Numerals(8, Constants.DATE, "##.##.####", "^([0-3][0-9]|[1-9])([.]|[/])([01][0-9]|[1-9])([.]|[/])(\\d{4})$",
                SecondaryPos.Date);

            public string lemma;
            public Regex pattern;
            public SecondaryPos secondaryPos;
            public string DefinedName { get; set; }
            public int Index { get; set; }

            public Numerals(int index, string definedName, string lemma, string patternStr, SecondaryPos secondaryPos)
            {
                this.Index = index;
                this.DefinedName = definedName;
                this.lemma = lemma;
                this.pattern = new Regex(patternStr);
                this.secondaryPos = secondaryPos;
            }

            public int GetIndex()
            {
                return Index;
            }


            public static IEnumerable<Numerals> Values
            {
                get
                {
                    yield return CARDINAL;
                    yield return ORDINAL;
                    yield return RANGE;
                    yield return RATIO;
                    yield return REAL;
                    yield return DISTRIB;
                    yield return PERCENTAGE_BEFORE;
                    yield return TIME;
                    yield return DATE;
                }
            }
        }
    }
}
