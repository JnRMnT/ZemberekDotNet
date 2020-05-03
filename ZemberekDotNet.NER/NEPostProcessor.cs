using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;

namespace ZemberekDotNet.NER
{
    /// <summary>
    /// Post processes named entities by removing suffixes from last word.
    /// TODO: requires some refactoring.
    /// 
    /// Originally written by Ayça Müge Sevinç. *
    /// </summary>
    public class NEPostProcessor
    {
        public string type;
        public string[] wordList;
        private string lastWord;

        private int relKiCount;

        private string longestLemma;
        private string longFormatforLongestLemma;
        private string longestNLemma;
        private string longFormatforNLongestLemma;
        private string longestNStem;
        private string longFormatforLongestNStem;

        public NamedEntity orginalNE;
        public NamedEntity postProcessedNE;

        public NEPostProcessor(NamedEntity namedEntity)
        {
            longestLemma = "";
            longFormatforLongestLemma = "";
            longestNLemma = "";
            longFormatforNLongestLemma = "";
            longestNStem = "";
            longFormatforLongestNStem = "";
            wordList = namedEntity.Content().Split(" ");
            lastWord = wordList[wordList.Length - 1];
            type = namedEntity.type;
            relKiCount = 0;
            orginalNE = namedEntity;
            postProcessedNE = namedEntity;
        }

        /// Lemmas are lower cased, but we need the named entities not lower-cased, so we upper-case them when needed.
        private string Capitalize(string lemma)
        {
            if (lemma.Length == 0)
            {
                return lemma;
            }
            string first = lemma.Substring(0, 1).ToUpper(Turkish.Locale);
            return lemma.Length < 2 ? first : first + lemma.Substring(1);
        }

        /// Check if there is  apostrophe in the last word,
        // if so remove the part including and following it from the namedEntity
        private NamedEntity ApostropheRemoved()
        {

            string apostropheRemoved = "";
            if (lastWord.Contains("\'"))
            {
                apostropheRemoved = lastWord.Substring(0, lastWord.IndexOf('\''));
            }
            if (lastWord.Contains("’"))
            {
                apostropheRemoved = lastWord.Substring(0, lastWord.IndexOf('’'));
            }
            postProcessedNE = UpdateLastWord(orginalNE, apostropheRemoved);
            return postProcessedNE;
        }

        /// Update the last word of a given named entity
        private NamedEntity UpdateLastWord(NamedEntity namedEntity, string lastWord)
        {
            bool boolLastWord = false;
            List<NerToken> tokens = new List<NerToken>(wordList.Length);

            for (int i = 0; i < wordList.Length; i++)
            {
                string s = wordList[i];
                NePosition position;
                if (wordList.Length == 1)
                {
                    position = NePosition.UNIT;
                    boolLastWord = true;
                }
                else if (i == 0)
                {
                    position = NePosition.BEGIN;
                }
                else if (i == wordList.Length - 1)
                {
                    position = NePosition.LAST;
                    boolLastWord = true;
                }
                else
                {
                    position = NePosition.INSIDE;
                }
                if (boolLastWord)
                {
                    tokens.Add(new NerToken(i, lastWord, type, position));
                }
                else
                {
                    tokens.Add(new NerToken(i, s, type, position));
                }
            }
            return new NamedEntity(type, tokens);
        }

        /// This method checks the derived forms to point out the parses that we want to skip.
        /// It returns true if the derived form is derived from a nominal with the incorrect possesive form, o if it is non-predicate form or if it is a derived nominal with incorrect possesives
        /// It also strips the adjectival suffix -ki
        private bool DerivedForms(string longFormat)
        {

            // check if it is a derived form
            if (longFormat.Contains("|"))
            {
                //find the last inflectional group
                string lastIG = longFormat.Substring(longFormat.LastIndexOf("|") + 1);

                //find the first inflectional group
                string firstIG = longFormat.Substring(1, longFormat.IndexOf("|") - 1);
                firstIG = firstIG.Substring(firstIG.IndexOf("] ") + 1);
                firstIG = firstIG.Substring(firstIG.IndexOf(":") + 1);

                //  check whether its final IG is a noun, or it is a predicate derived from a noun.
                // If these two does not hold, skip that parse.
                bool isLastIGNominalPossessive3rdPerson =
                    lastIG.Contains("Noun") && (lastIG.Contains("P2sg") || lastIG.Contains("P2pl") ||
                        lastIG.Contains("P1pl") ||
                        lastIG.Contains("P1sg"));
                bool isFirstIGNominalPossessive3rdPerson =
                    firstIG.Contains("Noun") && (firstIG.Contains("P2sg") ||
                        firstIG.Contains("P2pl") ||
                        firstIG.Contains("P1pl") ||
                        firstIG.Contains("P1sg"));

                if (isFirstIGNominalPossessive3rdPerson && lastIG.Contains("Verb"))
                {
                    return true;
                }

                if (isLastIGNominalPossessive3rdPerson)
                {
                    return true;
                }

                // if it is derived adjective (rel ki) from the nominal, remove those IGs
                // Ex: Izmirdeki,  [İzmir:Noun,Prop] izmir:Noun+A3sg+de:Loc|ki:Rel→Adj --> [İzmir:Noun,Prop] izmir:Noun+A3sg+de:Loc
                while (longFormat.Contains("ki:Rel→"))
                {
                    longFormat = longFormat.Substring(0, longFormat.LastIndexOf("|"));
                    if (longFormat.Contains("|"))
                    {
                        lastIG = longFormat.Substring(longFormat.LastIndexOf("|") + 1);
                    }
                    else
                    {
                        lastIG = firstIG;
                    }
                    relKiCount++;
                }
            }
            return false;
        }

        // This method returns true if it is not a nominal or if it is a nominal with incorrect possesive markers
        private bool UnInflectedNominalForm(string longFormat)
        {

            // if it is not derived form, check if it is a noun and also check if the appropriate
            // possessive forms , i.e., Pnon, P3sg, P3pl
            bool isNomimalPossesive3rdPerson =
                longFormat.Contains("Noun") && (longFormat.Contains("P2sg") || longFormat.Contains("P2pl")
                    || longFormat.Contains("P1pl") || longFormat.Contains("P1sg"));

            if (!longFormat.Contains("Noun"))
            {
                return true;
            }
            if (isNomimalPossesive3rdPerson)
            {
                return true;
            }

            return false;
        }

        /// This method goes over all the possible parses for the last word of the named entity,
        //  it skips the irrelevant parses for the NE in the list of all morphological parses.
        /// It finds the longest lemmas for the relevant parses which is either the proper noun reading and common noun reading.
        /// Relevant parses also includes the predicate forms which are derived from the nouns.
        /// Then, it processes the required suffix stripping on the last word depending the named entity's type

        private NamedEntity MorphologicalAnalysisForNamedEntity(TurkishMorphology morphology)
        {
            {
                //get all the morphological analysis for the last word in the namedEntity
                WordAnalysis results = morphology.Analyze(lastWord);

                // if there are no parses returned for the morphological analyses, it means that
                // the lastWord is unknown, hence return the namedEntity without changing it
                if (results.AnalysisCount() == 0)
                {
                    //return the the original namedEntity
                    return orginalNE;
                }

                //we will focus on only the analyses for nominals or the predicate of the nominal forms
                foreach (SingleAnalysis result in results)
                {

                    string longFormat = result.FormatLong();
                    // if (log) System.out.print("longFormat:  " + longFormat + '\n');

                    if (DerivedForms(longFormat))
                    {
                        // skip the parses which contain non-predicate forms, or a derived nominal
                        // with incoorect possesives, or a form derived from a nominal with the incorrect possesive markers
                        continue;
                    }
                    else if (UnInflectedNominalForm(longFormat))
                    {
                        continue; // skip the parses which are not nominals or which are nominals with incorrect possesive markers
                    }

                    // get the list of possible lemmas for the lastWord
                    List<String> ListOfLemmas = result.GetLemmas();
                    List<String> ListOfStems = result.GetStems();

                    // if the list of lemmas is empty, return the original namedEntity
                    if (ListOfLemmas == null || ListOfLemmas.IsEmpty())
                    {
                        return orginalNE;
                    }

                    // Get the last lemma on the lemma list since it is the longest one
                    string LastLemma = ListOfLemmas[ListOfLemmas.Count - 1];
                    string LastStem = ListOfStems[ListOfStems.Count - 1];

                    // Check if the last lemma in the list of lemmas is a derived adjective
                    // (with the suffix rel -ki, if so, strip that suffix, then continue checking with the next
                    // longest lemma in the list
                    // ki is recursive it may be added many times: izmirdeki, istanbuldakilerinki, ...
                    int x = 0;
                    while (relKiCount > 0 && LastLemma.EndsWith("ki") && LastStem.EndsWith("ki"))
                    {
                        LastLemma = ListOfLemmas[ListOfLemmas.Count - 2 - x];
                        LastStem = ListOfStems[ListOfStems.Count - 2 - x];
                        relKiCount--;
                        x++;
                    }

                    // Uppercase the lemma, we need the original form in the NE
                    string CLastLemma = Capitalize(LastLemma);
                    string CLastStem = Capitalize(LastStem);

                    // if CLastLemma for the proper noun is longer than the previous longest lemma of any
                    // previous parses, then update longestLemma
                    if (longestLemma.Length <= CLastLemma.Length && (longFormat.Contains("Noun,Prop]")
                        || longFormat.Contains("Noun,Abbrv]")))
                    {
                        longestLemma = CLastLemma;
                        longFormatforLongestLemma = longFormat;
                    }

                    // if CLastLemma for the common noun is longer than the previous longest lemma
                    // for the noun of any previous parses, then update longestNLemma
                    if (longestNLemma.Length <= CLastLemma.Length && longFormat.Contains("Noun]"))
                    {
                        longestNLemma = CLastLemma;
                        longFormatforNLongestLemma = longFormat;
                    }
                    // for any kind of noun update the longestNStem
                    if (longestNStem.Length <= CLastStem.Length && longFormat.Contains("Noun]"))
                    {
                        longestNStem = CLastStem;
                        longFormatforLongestNStem = longFormat;
                    }

                    // if it is an abbreviation, its letters should be all capitalized
                    if (longFormat.Contains("Noun,Abbrv]") || longFormat.Contains("Noun,Prop]"))
                    {
                        int ok = 1;
                        for (int i = 0; i < 2 && i < longestLemma.Length; i++)
                        {
                            //check if the characters are upper cased in the original namedEntity
                            if (!char.IsUpper(lastWord[i]))
                            {
                                ok = 0;
                                break;
                            }
                        }
                        if (ok == 1)
                        {
                            longestLemma = longestLemma.ToUpperInvariant();
                        }
                    }
                }

                // in the case of there is no proper noun reading, make use of the common noun reading
                if (longestLemma.Length == 0 && longestNLemma.Length > 0)
                {
                    longestLemma = longestNLemma;
                    longFormatforLongestLemma = longFormatforNLongestLemma;
                }
                // if there are no parses left after skipping the irrelevant ones,
                // then return the original namedEntity
                if (longestLemma.Length == 0 && longestNLemma.Length == 0)
                {
                    return orginalNE; //return the same as the input named entity
                }

                // Choose one of the above determined longest lemmas based on the named entity type:
                // PERSON, ORGANIZATION, LOCATION
                postProcessedNE = ChooseBestLemmaBasedOnNamedEntityType();
                return postProcessedNE;
            }
        }

        // Based on the named entity type (PERSON, ORGANIZATION, LOCATION),
        // it decides the best lemma to choose
        private NamedEntity ChooseBestLemmaBasedOnNamedEntityType()
        {

            // if there is no  apostrophe and there is only one word in ner
            // (Kaan, Kaana, istanbulda, Komutanlıgı) return the longest lemma
            if (wordList.Length == 1)
            {
                postProcessedNE = UpdateLastWord(orginalNE, longestLemma);
                return postProcessedNE;
            }
            // if there is no  apostrophe and there are more than one word in NE
            // (Toros Daglarinda, Kaan Irmak, Kaan Irmagı)
            else
            {
                // When the type is person we expect it to be in an uninflected form in NE,
                // so we just return the longest lemma (without any inflections)
                //Ex: deneyim, birligi
                if (type.Equals("PERSON"))
                {
                    postProcessedNE = UpdateLastWord(orginalNE, longestLemma);
                    return postProcessedNE;
                }

                // When the type is organization or location,  we return the longest nominal lemma
                // (with the inflections p3sg or p3pl)
                else if (type.Equals("ORGANIZATION") || type.Equals("LOCATION"))
                { //organization or location
                    string lemmaPos = longFormatforLongestLemma
                        .Substring(0, longFormatforLongestLemma.IndexOf(']'));
                    string lemma = lemmaPos.Substring(1, lemmaPos.IndexOf(':') - 1);
                    string pos = lemmaPos.Substring(lemmaPos.IndexOf(':') + 1, lemmaPos.Length - lemmaPos.IndexOf(':') + 1);

                    // if we have a common noun reading in the analysis, we use it for organization & location
                    if (!longFormatforNLongestLemma.Equals(""))
                    {
                        // if the last word  is a common noun
                        string nLemmaPos = longFormatforNLongestLemma
                            .Substring(0, longFormatforNLongestLemma.IndexOf(']'));
                        string nCat = longFormatforNLongestLemma
                            .Substring(longFormatforNLongestLemma.IndexOf(' '));
                        nCat = nCat.Substring(nCat.IndexOf(':') + 1);
                        // Birlik vs Bir, Kurulu vs kuru
                        // check for the regular expression with singular possesive suffix
                        if (nCat.Contains("Noun+A3sg+") && nCat.Contains(":P3sg"))
                        {
                            string suffix = nCat.Substring(nCat.IndexOf("Noun+A3sg+") + "Noun+A3sg+".Length,
                                nCat.IndexOf(":P3sg") - nCat.IndexOf("Noun+A3sg+") - "Noun+A3sg+".Length);
                            postProcessedNE = UpdateLastWord(orginalNE, longestNStem + suffix);
                            return postProcessedNE;

                        }
                        // check for the regular expression like -leri, ları
                        Regex pattern = new Regex("Noun\\+l.r:A3pl\\+.*:P3.*");
                        Match matcher = pattern.Match(nCat);

                        if (matcher.Success)
                        {
                            string suffix1 = nCat
                                .Substring(nCat.IndexOf("Noun+") + "Noun+".Length, nCat.IndexOf(":A3pl") - nCat.IndexOf("Noun+") - "Noun+".Length);
                            string suffix2 = nCat
                                .Substring(nCat.IndexOf("r:A3pl") + "r:A3pl+".Length, nCat.IndexOf(":P3") - nCat.IndexOf("r:A3pl") - "r:A3pl+".Length);
                            postProcessedNE = UpdateLastWord(orginalNE, longestNStem + suffix1 + suffix2);
                            return postProcessedNE;

                        }
                    }
                    // if there is no common noun reading for the location or organization,
                    // then we use the proper noun reading
                    else if (longestNLemma.Equals("") &&
                        (pos.Equals("Noun,Prop") || pos.Equals("Noun,Abbrv")) &&
                        lemma.Equals(longestLemma))
                    {

                        postProcessedNE = UpdateLastWord(orginalNE, longestLemma);
                        return postProcessedNE;
                    }

                }
                return orginalNE;
            }
        }

        /// This method post-process the named entity to strip any suffixes that are attached to them
        public NamedEntity PostProcessNER(TurkishMorphology morphology)
        {

            // check if there is  apostrophe in the last word,
            // if so remove the part including and following it from the namedEntity
            if (lastWord.Contains("\'") || lastWord.Contains("’"))
            {
                return ApostropheRemoved();
            }

            // check for the best nominal analysis depending on the type of the namedEntity,
            // strip the suffixes on the named entity
            return MorphologicalAnalysisForNamedEntity(morphology);
        }
    }
}
