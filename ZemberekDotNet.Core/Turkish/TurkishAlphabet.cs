using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Text;

namespace ZemberekDotNet.Core.Turkish
{
    public class TurkishAlphabet
    {
        public static readonly CultureInfo TR = new CultureInfo("tr");

        private static readonly string lowercase = "abcçdefgğhıijklmnoöprsştuüvyzxwqâîû";
        private static readonly string uppercase = lowercase.ToUpper(TR);
        private static readonly string allLetters = lowercase + uppercase;
        private static readonly FixedBitVector dictionaryLettersLookup =
            TextUtil.GenerateBitLookup(allLetters);

        private static readonly string vowelsLowercase = "aeıioöuüâîû";
        private static readonly string vowelsUppercase = vowelsLowercase.ToUpper(TR);
        private FixedBitVector vowelLookup =
            TextUtil.GenerateBitLookup(vowelsLowercase + vowelsUppercase);

        private static readonly string circumflex = "âîû";
        private static readonly string circumflexUpper = "ÂÎÛ";
        private FixedBitVector circumflexLookup =
            TextUtil.GenerateBitLookup(circumflex + circumflexUpper);

        private static readonly string apostrophe = "\u2032´`’‘'";
        private FixedBitVector apostropheLookup = TextUtil.GenerateBitLookup(apostrophe);

        private static readonly string stopConsonants = "çkpt";
        private FixedBitVector stopConsonantLookup =
            TextUtil.GenerateBitLookup(stopConsonants + stopConsonants.ToUpper(TR));

        private static readonly string voicelessConsonants = "çfhkpsşt";
        private FixedBitVector voicelessConsonantsLookup =
            TextUtil.GenerateBitLookup(voicelessConsonants + voicelessConsonants.ToUpper(TR));

        private static readonly string turkishSpecific = "çÇğĞıİöÖşŞüÜâîûÂÎÛ";
        private static readonly string turkishAscii = "cCgGiIoOsSuUaiuAIU";
        private IntIntMap turkishToAsciiMap = new IntIntMap();
        private FixedBitVector turkishSpecificLookup = TextUtil.GenerateBitLookup(turkishSpecific);

        private static readonly string asciiEqTr = "cCgGiIoOsSuUçÇğĞıİöÖşŞüÜ";
        private static readonly string asciiEq = "çÇğĞıİöÖşŞüÜcCgGiIoOsSuU";
        private IntIntMap asciiEqualMap = new IntIntMap();
        private FixedBitVector asciiTrLookup = TextUtil.GenerateBitLookup(asciiEqTr);


        private static readonly string foreignDiacritics = "ÀÁÂÃÄÅÈÉÊËÌÍÎÏÑÒÓÔÕÙÚÛàáâãäåèéêëìíîïñòóôõùúû";
        private static readonly string diacriticsToTurkish = "AAAAAAEEEEIIIINOOOOUUUaaaaaaeeeeiiiinoooouuu";
        private IntIntMap foreignDiacriticsMap = new IntIntMap();
        private FixedBitVector foreignDiacriticsLookup = TextUtil.GenerateBitLookup(foreignDiacritics);
        private IntMap<TurkicLetter> letterMap = new IntMap<TurkicLetter>();
        private static readonly IntIntMap voicingMap = new IntIntMap();
        private static readonly IntIntMap devoicingMap = new IntIntMap();
        private static readonly IntIntMap circumflexMap = new IntIntMap();

        public static TurkishAlphabet Instance = new TurkishAlphabet();

        private TurkishAlphabet()
        {
            List<TurkicLetter> letters = GenerateLetters();
            foreach (TurkicLetter letter in letters)
            {
                letterMap.Put(letter.charValue, letter);
            }
            GenerateVoicingDevoicingLookups();

            PopulateCharMap(turkishToAsciiMap, turkishSpecific, turkishAscii);
            PopulateCharMap(foreignDiacriticsMap, foreignDiacritics, diacriticsToTurkish);

            for (int i = 0; i < asciiEqTr.Length; i++)
            {
                char inChar = asciiEqTr[i];
                char outChar = asciiEq[i];
                asciiEqualMap.Put(inChar, outChar);
            }
        }

        public string ToAscii(string inString)
        {
            StringBuilder sb = new StringBuilder(inString.Length);
            for (int i = 0; i < inString.Length; i++)
            {
                char c = inString[i];
                int res = turkishToAsciiMap.Get(c);
                char map = res == IntIntMap.NoResult ? c : (char)res;
                sb.Append(map);
            }
            return sb.ToString();
        }

        public string ForeignDiacriticsToTurkish(string inString)
        {
            StringBuilder sb = new StringBuilder(inString.Length);
            for (int i = 0; i < inString.Length; i++)
            {
                char c = inString[i];
                int res = foreignDiacriticsMap.Get(c);
                char map = res == IntIntMap.NoResult ? c : (char)res;
                sb.Append(map);
            }
            return sb.ToString();
        }

        private void GenerateVoicingDevoicingLookups()
        {
            string voicingIn = "çgkpt";
            string voicingOut = "cğğbd";
            string devoicingIn = "bcdgğ";
            string devoicingOut = "pçtkk";

            PopulateCharMap(voicingMap,
                voicingIn + voicingIn.ToUpper(TR),
                voicingOut + voicingOut.ToUpper(TR));
            PopulateCharMap(devoicingMap,
                devoicingIn + devoicingIn.ToUpper(TR),
                devoicingOut + devoicingOut.ToUpper(TR));

            string circumflexNormalized = "aiu";
            PopulateCharMap(circumflexMap,
                circumflex + circumflex.ToUpper(TR),
                circumflexNormalized + circumflexNormalized.ToUpper(TR));
        }

        private bool Lookup(FixedBitVector vector, char c)
        {
            return c < vector.length && vector.Get(c);
        }

        public bool ContainsAsciiRelated(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < asciiTrLookup.length && asciiTrLookup.Get(c))
                {
                    return true;
                }
            }
            return false;
        }

        public IntIntMap GetTurkishToAsciiMap()
        {
            return turkishToAsciiMap;
        }

        public char GetAsciiEqual(char c)
        {
            int res = turkishToAsciiMap.Get(c);
            return res == IntIntMap.NoResult ? c : (char)res;
        }

        public bool IsAsciiEqual(char c1, char c2)
        {
            if (c1 == c2)
            {
                return true;
            }
            int a1 = asciiEqualMap.Get(c1);
            if (a1 == IntIntMap.NoResult)
            {
                return false;
            }
            return a1 == c2;
        }


        private void PopulateCharMap(IntIntMap map, string inStr, string outStr)
        {
            for (int i = 0; i < inStr.Length; i++)
            {
                char inChar = inStr[i];
                char outChar = outStr[i];
                map.Put(inChar, outChar);
            }
        }

        private List<TurkicLetter> GenerateLetters()
        {
            List<TurkicLetter> letters = new List<TurkicLetter> {
                new TurkicLetter.Builder('a').Vowel().Build(),
                new TurkicLetter.Builder('e').Vowel().FrontalVowel().Build(),
                new TurkicLetter.Builder('ı').Vowel().Build(),
                new TurkicLetter.Builder('i').Vowel().FrontalVowel().Build(),
                new TurkicLetter.Builder('o').Vowel().RoundedVowel().Build(),
                new TurkicLetter.Builder('ö').Vowel().FrontalVowel().RoundedVowel().Build(),
                new TurkicLetter.Builder('u').Vowel().RoundedVowel().Build(),
                new TurkicLetter.Builder('ü').Vowel().RoundedVowel().FrontalVowel().Build(),
                // Circumflexed letters
                new TurkicLetter.Builder('â').Vowel().Build(),
                new TurkicLetter.Builder('î').Vowel().FrontalVowel().Build(),
                new TurkicLetter.Builder('û').Vowel().FrontalVowel().RoundedVowel().Build(),
                // Consonants
                new TurkicLetter.Builder('b').Build(),
                new TurkicLetter.Builder('c').Build(),
                new TurkicLetter.Builder('ç').Voiceless().Build(),
                new TurkicLetter.Builder('d').Build(),
                new TurkicLetter.Builder('f').Continuant().Voiceless().Build(),
                new TurkicLetter.Builder('g').Build(),
                new TurkicLetter.Builder('ğ').Continuant().Build(),
                new TurkicLetter.Builder('h').Continuant().Voiceless().Build(),
                new TurkicLetter.Builder('j').Continuant().Build(),
                new TurkicLetter.Builder('k').Voiceless().Build(),
                new TurkicLetter.Builder('l').Continuant().Build(),
                new TurkicLetter.Builder('m').Continuant().Build(),
                new TurkicLetter.Builder('n').Continuant().Build(),
                new TurkicLetter.Builder('p').Voiceless().Build(),
                new TurkicLetter.Builder('r').Continuant().Build(),
                new TurkicLetter.Builder('s').Continuant().Voiceless().Build(),
                new TurkicLetter.Builder('ş').Continuant().Voiceless().Build(),
                new TurkicLetter.Builder('t').Voiceless().Build(),
                new TurkicLetter.Builder('v').Continuant().Build(),
                new TurkicLetter.Builder('y').Continuant().Build(),
                new TurkicLetter.Builder('z').Continuant().Build(),
                new TurkicLetter.Builder('q').Build(),
                new TurkicLetter.Builder('w').Build(),
                new TurkicLetter.Builder('x').Build()
            };
            List<TurkicLetter> capitals = new List<TurkicLetter>();
            foreach (TurkicLetter letter in letters)
            {
                char upper = letter.charValue.ToString().ToUpper(TR)[0];
                capitals.Add(letter.CopyFor(upper));
            }
            letters.AddRange(capitals);
            return letters;
        }

        public bool AllCapital(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!char.IsUpper(input[i]))
                {
                    return false;
                }
            }
            return true;
        }

        // TODO: this should not be here,
        public string Normalize(string input)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            input = TextUtil.NormalizeApostrophes(input.ToLower(TR));
            foreach (char c in input.ToCharArray())
            {
                if (letterMap.ContainsKey(c) || c == '.' || c == '-')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append("?");
                }
            }
            return sb.ToString();
        }

        public char NormalizeCircumflex(char c)
        {
            int res = circumflexMap.Get(c);
            return res == IntIntMap.NoResult ? c : (char)res;
        }

        public bool ContainsCircumflex(string s)
        {
            return CheckLookup(circumflexLookup, s);
        }

        public bool IsTurkishSpecific(char c)
        {
            return Lookup(turkishSpecificLookup, c);
        }

        public bool ContainsApostrophe(string s)
        {
            return CheckLookup(apostropheLookup, s);
        }

        public bool ContainsForeignDiacritics(string s)
        {
            return CheckLookup(foreignDiacriticsLookup, s);
        }

        private bool CheckLookup(FixedBitVector lookup, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (Lookup(lookup, c))
                {
                    return true;
                }
            }
            return false;
        }

        public string GetAllLetters()
        {
            return allLetters;
        }

        public string GetLowercaseLetters()
        {
            return lowercase;
        }

        public string GetUppercaseLetters()
        {
            return uppercase;
        }

        /// <summary>
        /// Converts Turkish letters with circumflex symbols to letters without circumflexes. â->a î->i
        /// û->u
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string NormalizeCircumflex(string s)
        {
            if (!ContainsCircumflex(s))
            {
                return s;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (Lookup(circumflexLookup, c))
                {
                    sb.Append((char)circumflexMap.Get(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        
        public string NormalizeApostrophe(string s)
        {
            if (!ContainsApostrophe(s))
            {
                return s;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (Lookup(apostropheLookup, c))
                {
                    sb.Append('\'');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// If there is a voiced char for `c`, returns it. Otherwise Returns the original input. ç->c g->ğ
        /// k->ğ p->b t->d
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public char Voice(char c)
        {
            int res = voicingMap.Get(c);
            return res == IntIntMap.NoResult ? c : (char)res;
        }

        /// <summary>
        /// If there is a devoiced char for `c`, returns it. Otherwise Returns the original input. b->p
        /// c->ç d->t g->k ğ->k
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public char devoice(char c)
        {
            int res = devoicingMap.Get(c);
            return res == IntIntMap.NoResult ? c : (char)res;
        }

        /// <summary>
        /// Returns the TurkicLetter object for a character. If it does not exist, returns
        /// TurkicLetter.Undefined
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public TurkicLetter GetLetter(char c)
        {
            TurkicLetter letter = letterMap.Get(c);
            return letter == null ? TurkicLetter.Undefined : letter;
        }

        /// <summary>
        /// Returns the last letter of the input as "TurkicLetter". If input is empty or the last character
        /// does not belong to alphabet, returns TurkicLetter.Undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public TurkicLetter GetLastLetter(char[] s)
        {
            if (s.Length == 0)
            {
                return TurkicLetter.Undefined;
            }
            return GetLetter(s[s.Length - 1]);
        }

        public char LastChar(char[] s)
        {
            return s[s.Length - 1];
        }

        /// <summary>
        /// Returns the first letter of the input as "TurkicLetter". If input is empty or the first
        /// character does not belong to alphabet, returns TurkicLetter.Undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public TurkicLetter GetFirstLetter(char[] s)
        {
            if (s.Length == 0)
            {
                return TurkicLetter.Undefined;
            }

            return GetLetter(s[0]);
        }

        /// <summary>
        /// Returns is `c` is a Turkish vowel. Input can be lower or upper case. Turkish letters with
        /// circumflex are included.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsVowel(char c)
        {
            return Lookup(vowelLookup, c);
        }

        /// <summary>
        /// Returns true if `c` is a member of set Turkish alphabet and three english letters w,x and q.
        /// Turkish letters with circumflex are included. Input can be lower or upper case.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsDictionaryLetter(char c)
        {
            return Lookup(dictionaryLettersLookup, c);
        }

        /// <summary>
        /// Returns true if `c` is a stop consonant. Stop consonants for Turkish are: `ç,k,p,t`. Input can
        /// be lower or upper case.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsStopConsonant(char c)
        {
            return Lookup(stopConsonantLookup, c);
        }

        /// <summary>
        /// Returns true if `c` is a stop consonant. Voiceless consonants for Turkish are:
        /// `ç,f,h,k,p,s,ş,t`. Input can be lower or upper case.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsVoicelessConsonant(char c)
        {
            return Lookup(voicelessConsonantsLookup, c);
        }

        /// <summary>
        /// Returns the last vowel of the input as "TurkicLetter". If input is empty or there is no vowel,
        /// returns TurkicLetter.Undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public TurkicLetter GetLastVowel(string s)
        {
            if (s.Length == 0)
            {
                return TurkicLetter.Undefined;
            }
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];
                if (IsVowel(c))
                {
                    return GetLetter(c);
                }
            }
            return TurkicLetter.Undefined;
        }

        /// <summary>
        /// Returns the first vowel of the input as "TurkicLetter". If input is empty or there is no vowel,
        /// returns TurkicLetter.Undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public TurkicLetter GetFirstVowel(char[] s)
        {
            if (s.Length == 0)
            {
                return TurkicLetter.Undefined;
            }
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (IsVowel(c))
                {
                    return GetLetter(c);
                }
            }
            return TurkicLetter.Undefined;
        }

        /// <summary>
        /// Returns true if target string matches source string with A-Type harmony
        /// <pre>
        ///   elma, ya -> true
        ///   kedi, ye -> true
        ///   kalem, a -> false
        /// </pre>
        /// Returns true if target string matches source string with I-Type harmony
        /// <pre>
        ///   elma, yı  -> true
        ///   kedi, yi  -> true
        ///   kalem, ü  -> false
        ///   yogurt, u -> true
        /// </pre>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CheckVowelHarmonyA(string source, string target)
        {
            TurkicLetter sourceLastVowel = GetLastVowel(source);
            TurkicLetter targetFirstVowel = GetLastVowel(target);
            return CheckVowelHarmonyA(sourceLastVowel, targetFirstVowel);
        }


        public bool CheckVowelHarmonyI(string source, string target)
        {
            TurkicLetter sourceLastVowel = GetLastVowel(source);
            TurkicLetter targetFirstVowel = GetLastVowel(target);
            return CheckVowelHarmonyI(sourceLastVowel, targetFirstVowel);
        }

        /// <summary>
        /// Returns true if target letter matches source letter with A-Type harmony
        /// <pre>
        ///   i, e -> true
        ///   i, a -> false
        ///   u, a -> true
        ///   c, b -> false
        /// </pre>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CheckVowelHarmonyA(TurkicLetter source, TurkicLetter target)
        {
            if (source == TurkicLetter.Undefined || target == TurkicLetter.Undefined)
            {
                return false;
            }
            if (!source.IsVowel() || !target.IsVowel())
            {
                return false;
            }
            return (source.frontal && target.frontal) ||
                (!source.frontal && !target.frontal);
        }

        /// <summary>
        /// Returns true if target letter matches source letter with I-Type harmony
        /// <pre>
        ///   e, i -> true
        ///   a, i -> false
        ///   o, u -> true
        ///   c, b -> false
        /// </pre>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CheckVowelHarmonyI(TurkicLetter source, TurkicLetter target)
        {
            if (source == TurkicLetter.Undefined || target == TurkicLetter.Undefined)
            {
                return false;
            }
            if (!source.IsVowel() || !target.IsVowel())
            {
                return false;
            }
            return ((source.frontal && target.frontal) ||
                (!source.frontal && !target.frontal)) &&
                ((source.rounded && target.rounded) ||
                    (!source.rounded && !target.rounded));
        }

        /// <summary>
        /// Returns true if input contains a vowel.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool ContainsVowel(char[] s)
        {
            if (s.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < s.Length; i++)
            {
                if (IsVowel(s[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the vowel count in a word. It only checks Turkish vowels.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int VowelCount(string s)
        {
            int result = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (IsVowel(s[i]))
                {
                    result++;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if `s` contains a digit. If s is empty or has no digit, returns false.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool ContainsDigit(string s)
        {
            if (s.IsEmpty())
            {
                return false;
            }
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '0' && c <= '9')
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Compares two strings ignoring diacritics symbols.
        /// <pre>
        ///   i, ı -> true
        ///   i, î -> true
        ///   s, ş -> true
        ///   g, ğ -> true
        ///   kişi, kışı -> true
        /// </pre>
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public bool EqualsIgnoreDiacritics(string s1, string s2)
        {
            if (s1 == null || s2 == null)
            {
                return false;
            }
            if (s1.Length != s2.Length)
            {
                return false;
            }
            for (int i = 0; i < s1.Length; i++)
            {
                char c1 = s1[i];
                char c2 = s2[i];
                if (!IsAsciiEqual(c1, c2))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if s1 starts with s2 ignoring diacritics symbols.
        /// <pre>
        ///   kışı, kis -> true
        ///   pîr, pi   -> true
        /// </pre>
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public bool StartsWithIgnoreDiacritics(string s1, string s2)
        {
            if (s1 == null || s2 == null)
            {
                return false;
            }
            if (s1.Length < s2.Length)
            {
                return false;
            }
            for (int i = 0; i < s2.Length; i++)
            {
                char c1 = s1[i];
                char c2 = s2[i];
                if (!IsAsciiEqual(c1, c2))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
