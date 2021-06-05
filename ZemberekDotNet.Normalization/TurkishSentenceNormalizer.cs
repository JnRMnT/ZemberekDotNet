using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Dynamic;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.LM.Compression;
using ZemberekDotNet.Morphology;
using ZemberekDotNet.Morphology.Analysis;
using ZemberekDotNet.Morphology.Generator;
using ZemberekDotNet.Tokenization;
using static ZemberekDotNet.Morphology.Generator.WordGenerator;

namespace ZemberekDotNet.Normalization
{
    /// <summary>
    /// Tries to normalize a sentence by collecting candidate words from
    /// <pre>
    /// - lookup tables(manual and collected from a large context graph)
    /// - split-combine heuristics
    /// - ascii tolerant analysis
    /// - informal morphological analysis
    /// - spell checker
    /// </pre>
    /// It then finds the most likely sequence using Viterbi search algorithm over candidate lists, using
    /// a compressed language model.
    /// </summary>
    public class TurkishSentenceNormalizer
    {
        TurkishMorphology morphology;
        private SmoothLm lm;
        private TurkishSpellChecker spellChecker;

        private MultiMap<string, string> lookupFromGraph;
        private MultiMap<string, string> lookupFromAscii;
        private MultiMap<string, string> lookupManual;
        private TurkishMorphology informalAsciiTolerantMorphology;
        private InformalAnalysisConverter analysisConverter;

        private Dictionary<string, string> commonSplits = new Dictionary<string, string>();
        private Dictionary<string, string> replacements = new Dictionary<string, string>();
        private HashSet<string> commonConnectedSuffixes = new HashSet<string>();
        private HashSet<string> noSplitWords = new HashSet<string>();
        bool alwaysApplyDeasciifier = false;

        public TurkishSentenceNormalizer(
            TurkishMorphology morphology,
            string dataRoot,
            string languageModelPath)
        {

            this.morphology = morphology;
            this.analysisConverter = new InformalAnalysisConverter(morphology.GetWordGenerator());
            SmoothLm languageModel = SmoothLm.Builder(languageModelPath).LogBase(Math.E).Build();
            Log.Info("Language model = {0}", languageModel.Info());
            this.lm = languageModel;

            // TODO: spell checker should be an external parameter.
            StemEndingGraph graph = new StemEndingGraph(morphology);
            CharacterGraphDecoder decoder = new CharacterGraphDecoder(graph.StemGraph);
            this.spellChecker = new TurkishSpellChecker(
                morphology,
                decoder,
                CharacterGraphDecoder.DIACRITICS_IGNORING_MATCHER);

            this.lookupFromGraph = LoadMultiMap(Path.Combine(dataRoot, "lookup-from-graph"));
            this.lookupFromAscii = LoadMultiMap(Path.Combine(dataRoot, "ascii-map"));
            List<string> manualLookup =
                TextIO.LoadLines("Resources/normalization/candidates-manual");
            this.lookupManual = LoadMultiMap(manualLookup);

            // remove words that exists in lookupManual from lookupFromGraph
            foreach (var key in lookupManual.Keys)
            {
                lookupFromGraph.RemoveAll(key);
            }

            this.informalAsciiTolerantMorphology = TurkishMorphology.Builder()
                .SetLexicon(morphology.GetLexicon())
                .UseInformalAnalysis()
                .DiacriticsInAnalysisIgnored()
                .Build();

            string[] splitLines = File.ReadAllLines(Path.Combine(dataRoot, "split"), Encoding.UTF8);
            foreach (string splitLine in splitLines)
            {
                string[] tokens = splitLine.Split("=");
                commonSplits[tokens[0].Trim()] = tokens[1].Trim();
            }

            this.commonConnectedSuffixes.AddRange(TextIO.LoadLines(
                "Resources/normalization/question-suffixes"));
            this.commonConnectedSuffixes.AddRange(new List<string> { "de", "da", "ki" });

            this.noSplitWords.AddRange(TextIO.LoadLines(
                "Resources/normalization/no-split"));

            List<string> replaceLines = TextIO.LoadLines(
                "Resources/normalization/multi-word-replacements");
            foreach (string replaceLine in replaceLines)
            {
                string[] tokens = replaceLine.Split("=");
                replacements[tokens[0].Trim()] = tokens[1].Trim();
            }
        }

        public void SetAlwaysApplyDeasciifier(bool alwaysApplyDeasciifier)
        {
            this.alwaysApplyDeasciifier = alwaysApplyDeasciifier;
        }

        // load data with line format: "key=val1,val2"
        private MultiMap<string, string> LoadMultiMap(string path)
        {
            List<string> lines = TextIO.LoadLines(path);
            return LoadMultiMap(lines);
        }

        private MultiMap<string, string> LoadMultiMap(List<string> lines)
        {
            MultiMap<string, string> result = new MultiMap<string, string>();
            foreach (string line in lines)
            {
                int index = line.IndexOf("=");
                if (index < 0)
                {
                    throw new InvalidOperationException("Line needs to have `=` symbol. But it is:" +
                        line);
                }
                string key = line.Substring(0, index).Trim();
                string value = line.Substring(index + 1).Trim();
                if (value.IndexOf(',') >= 0)
                {
                    foreach (string token in value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        result.Add(key, token);
                    }
                }
                else
                {
                    result.Add(key, value);
                }
            }
            return result;
        }

        public string Normalize(string sentence)
        {
            if (sentence.Trim().Length == 0)
            {
                return sentence;
            }
            string processed = PreProcess(sentence);

            List<Token> tokens = TurkishTokenizer.Default.Tokenize(processed);

            List<Candidates> candidatesList = new List<Candidates>();

            for (int i = 0; i < tokens.Count; i++)
            {

                Token currentToken = tokens[i];
                string current = currentToken.GetText();
                string next = i == tokens.Count - 1 ? null : tokens[i + 1].GetText();
                string previous = i == 0 ? null : tokens[i - 1].GetText();

                LinkedHashSet<string> candidates = new LinkedHashSet<string>(2);

                // add matches from manual lookup
                candidates.AddRange(lookupManual[current]);

                // add matches from random walk
                candidates.AddRange(lookupFromGraph[current]);

                // add matches from ascii equivalents.
                // TODO: this may decrease accuracy. Also, this can be eliminated with ascii tolerant analyzer.
                candidates.AddRange(lookupFromAscii[current]);

                // add matches from informal analysis to formal surface conversion.

                WordAnalysis analyses = informalAsciiTolerantMorphology.Analyze(current);

                foreach (SingleAnalysis analysis in analyses)
                {
                    if (analysis.ContainsInformalMorpheme())
                    {
                        Result infrMorphemeResult = analysisConverter.Convert(current, analysis);
                        if (infrMorphemeResult != null)
                        {
                            candidates.Add(infrMorphemeResult.surface);
                        }
                    }
                    else
                    {
                        List<WordGenerator.Result> results = morphology.GetWordGenerator().Generate(
                            analysis.GetDictionaryItem(),
                            analysis.GetMorphemes());
                        foreach (Result result in results)
                        {
                            candidates.Add(result.surface);
                        }
                    }
                }

                // if there is no formal analysis and length is larger than 5,
                // get top 3 1 distance matches.
                if ((analyses.AnalysisCount() == 0) && current.Length > 3)
                {

                    List<string> spellCandidates = spellChecker
                        .SuggestForWord(current, previous, next, lm);
                    if (spellCandidates.Count > 3)
                    {
                        spellCandidates = new List<string>(spellCandidates.GetRange(0, 3));
                    }
                    candidates.AddRange(spellCandidates);
                }

                // if still there is no match, add the word itself.
                if (candidates.IsEmpty() || morphology.Analyze(current).IsCorrect())
                {
                    candidates.Add(current);
                }

                Candidates finalResult = new Candidates(
                    currentToken.GetText(),
                    candidates.Select(e => new Candidate(e)).ToList());

                candidatesList.Add(finalResult);
            }
            // Apply Viterbi decoding and return result.
            return string.Join(" ", Decode(candidatesList));

        }

        private bool HasAnalysis(WordAnalysis w)
        {
            foreach (SingleAnalysis s in w)
            {
                if (!s.IsRuntime() && !s.IsUnknown())
                {
                    return true;
                }
            }
            return false;
        }

        private class Hypothesis : IScorable
        {
            // for a three gram model, holds the 2 history words.
            Candidate[] history;
            Candidate current;

            // required for back tracking.
            Hypothesis previous;

            float score;

            internal Candidate[] History { get => history; set => history = value; }
            internal Candidate Current { get => current; set => current = value; }
            internal Hypothesis Previous { get => previous; set => previous = value; }
            internal float Score { get => score; set => score = value; }

            public float GetScore()
            {
                return Score;
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

                Hypothesis that = (Hypothesis)o;

                if (!Enumerable.SequenceEqual(History, that.History))
                {
                    return false;
                }
                return Current.Equals(that.Current);
            }

            public override int GetHashCode()
            {
                int result = History.GetHashCode();
                result = 31 * result + Current.GetHashCode();
                return result;
            }

            public override string ToString()
            {
                return "Hypothesis{" +
                    "history=" + Arrays.ToString(History) +
                    ", current=" + Current +
                    ", score=" + Score +
                    '}';
            }
        }

        /// <summary>
        /// Represents a candidate word.
        /// </summary>
        private class Candidate
        {
            readonly string content;
            readonly float score;

            internal Candidate(string content)
            {
                this.content = content;
                score = 1f;
            }

            internal string Content => content;

            internal float Score => score;

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

                Candidate candidate = (Candidate)o;

                return Content.Equals(candidate.Content);
            }

            public override int GetHashCode()
            {
                return Content.GetHashCode();
            }

            public override string ToString()
            {
                return "Candidate{" +
                    "content='" + Content + '\'' +
                    ", score=" + Score +
                    '}';
            }
        }

        private class Candidates
        {
            string word;
            List<Candidate> candidates;

            internal Candidates(string word,
                List<Candidate> candidates)
            {
                this.Word = word;
                this.InnerCandidates = candidates;
            }

            internal string Word { get => word; set => word = value; }
            internal List<Candidate> InnerCandidates { get => candidates; set => candidates = value; }

            public override string ToString()
            {
                return "Candidates{" +
                    "word='" + Word + '\'' +
                    ", candidates=" + InnerCandidates +
                    '}';
            }

        }

        private static Candidate START = new Candidate("<s>");
        private static Candidate END = new Candidate("</s>");
        private static Candidates END_CANDIDATES =
            new Candidates("</s>", new List<Candidate> { END });

        private List<string> Decode(List<Candidates> candidatesList)
        {

            ActiveList<Hypothesis> current = new ActiveList<Hypothesis>();
            ActiveList<Hypothesis> next = new ActiveList<Hypothesis>();

            // Pad with END tokens.
            candidatesList.Add(END_CANDIDATES);

            Hypothesis initial = new Hypothesis();
            int lmOrder = lm.GetOrder();
            initial.History = new Candidate[lmOrder - 1];
            Array.Fill(initial.History, START);
            initial.Current = START;
            initial.Score = 0f;
            current.Add(initial);

            foreach (Candidates candidates in candidatesList)
            {

                foreach (Hypothesis h in current)
                {
                    foreach (Candidate c in candidates.InnerCandidates)
                    {
                        Hypothesis newHyp = new Hypothesis();
                        Candidate[] hist = new Candidate[lmOrder - 1];
                        if (lmOrder > 2)
                        {
                            Array.Copy(h.History, 1, hist, 0, lmOrder - 1);
                        }
                        hist[hist.Length - 1] = h.Current;
                        newHyp.Current = c;
                        newHyp.History = hist;
                        newHyp.Previous = h;

                        // score calculation.
                        int[] indexes = new int[lmOrder];
                        for (int j = 0; j < lmOrder - 1; j++)
                        {
                            indexes[j] = lm.GetVocabulary().IndexOf(hist[j].Content);
                        }
                        indexes[lmOrder - 1] = lm.GetVocabulary().IndexOf(c.Content);
                        float score = lm.GetProbability(indexes);

                        newHyp.Score = h.Score + score;
                        next.Add(newHyp);
                    }
                }
                current = next;
                next = new ActiveList<Hypothesis>();
            }

            // back track to find best sequence.
            Hypothesis best = current.GetBest();
            List<string> seq = new List<string>();
            Hypothesis bestH = best;
            // skip </s>
            bestH = bestH.Previous;
            while (bestH != null && bestH.Current != START)
            {
                seq.Add(bestH.Current.Content);
                bestH = bestH.Previous;
            }
            seq.Reverse();
            return seq;
        }

        internal string PreProcess(string sentence)
        {
            sentence = sentence.ToLower(Turkish.Locale);
            List<Token> tokens = TurkishTokenizer.Default.Tokenize(sentence);
            string s = ReplaceCommon(tokens);
            tokens = TurkishTokenizer.Default.Tokenize(s);
            s = CombineNecessaryWords(tokens);
            tokens = TurkishTokenizer.Default.Tokenize(s);
            s = SplitNecessaryWords(tokens, false);
            if (alwaysApplyDeasciifier || ProbablyRequiresDeasciifier(s))
            {
                Deasciifier.Deasciifier deasciifier = new Deasciifier.Deasciifier(s);
                s = deasciifier.ConvertToTurkish();
            }
            tokens = TurkishTokenizer.Default.Tokenize(s);
            s = CombineNecessaryWords(tokens);
            tokens = TurkishTokenizer.Default.Tokenize(s);
            return SplitNecessaryWords(tokens, true);
        }

        /// <summary>
        /// Tries to combine words that are written separately using heuristics. If it cannot combine,
        /// returns empty string.
        /// 
        /// Such as:
        /// <pre>
        /// göndere bilirler -> göndere bilirler
        /// elma lar -> elmalar
        /// ankara 'ya -> ankara'ya
        /// </pre>
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        internal string CombineCommon(string i1, string i2)
        {
            string combined = i1 + i2;
            if (i2.StartsWith("'") || i2.StartsWith("bil"))
            {
                WordAnalysis w = morphology.Analyze(combined);
                if (HasAnalysis(w))
                {
                    return combined;
                }
            }
            if (!HasRegularAnalysis(i2))
            {
                WordAnalysis w = morphology.Analyze(combined);
                if (HasAnalysis(w))
                {
                    return combined;
                }
            }
            return "";
        }

        /// <summary>
        /// Returns true if only word is analysed with internal dictionary and analysis dictionary item is
        /// not proper noun.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal bool HasRegularAnalysis(string s)
        {
            WordAnalysis a = morphology.Analyze(s);
            return a.Any(k => !k.IsUnknown() && !k.IsRuntime() &&
                k.GetDictionaryItem().secondaryPos != SecondaryPos.ProperNoun &&
                k.GetDictionaryItem().secondaryPos != SecondaryPos.Abbreviation
            );
        }

        /// <summary>
        /// Tries to separate question words, conjunctions and common mistakes by looking from a lookup or
        /// using heuristics. Such as:
        /// <pre>
        /// gelecekmisin -> gelecek misin
        /// tutupda -> tutup da
        /// öyleki -> öyle ki
        /// olurya -> olur ya
        /// </pre>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="useLookup"></param>
        /// <returns></returns>
        internal string SeparateCommon(string input, bool useLookup)
        {
            if (noSplitWords.Contains(input))
            {
                return input;
            }
            if (useLookup && commonSplits.ContainsKey(input))
            {
                return commonSplits.GetValueOrDefault(input);
            }
            if (!HasRegularAnalysis(input))
            {
                for (int i = 1; i < input.Length - 1; i++)
                {
                    string tail = input.Substring(i);
                    if (commonConnectedSuffixes.Contains(tail))
                    {
                        string head = input.Substring(0, i);
                        if (tail.Length < 3)
                        {
                            if (!lm.NGramExists(lm.GetVocabulary().ToIndexes(head, tail)))
                            {
                                return input;
                            }
                        }
                        if (HasRegularAnalysis(head))
                        {
                            return head + " " + tail;
                        }
                        else
                        {
                            return input;
                        }
                    }
                }
            }
            return input;
        }

        internal string SeparateBrute(string input, int minSize)
        {
            if (!HasRegularAnalysis(input))
            {
                for (int i = minSize; i < input.Length - minSize; i++)
                {
                    string head = input.Substring(0, i);
                    string tail = input.Substring(i);
                    if (HasRegularAnalysis(head) && HasRegularAnalysis(tail))
                    {
                        return head + " " + tail;
                    }
                }
            }
            return input;
        }

        /// <summary>
        /// Makes a guess if input sentence requires deasciifier.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        internal static bool ProbablyRequiresDeasciifier(string sentence)
        {
            int turkishSpecCount = 0;
            for (int i = 0; i < sentence.Length; i++)
            {
                char c = sentence[i];
                if (c == 'ı' || c == 'I')
                {
                    continue;
                }
                if (TurkishAlphabet.Instance.IsTurkishSpecific(c))
                {
                    turkishSpecCount++;
                }
            }
            double ratio = turkishSpecCount * 1d / sentence.Length;
            return ratio < 0.1;
        }

        internal string CombineNecessaryWords(List<Token> tokens)
        {
            List<string> result = new List<string>();
            bool combined = false;
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                Token first = tokens[i];
                Token second = tokens[i + 1];
                string firstS = first.GetText();
                string secondS = second.GetText();
                if (!IsWord(first) || !IsWord(second))
                {
                    combined = false;
                    result.Add(firstS);
                    continue;
                }
                if (combined)
                {
                    combined = false;
                    continue;
                }
                string c = CombineCommon(firstS, secondS);
                if (c.Length > 0)
                {
                    result.Add(c);
                    combined = true;
                }
                else
                {
                    result.Add(first.GetText());
                    combined = false;
                }
            }
            if (!combined)
            {
                result.Add(tokens[tokens.Count - 1].GetText());
            }
            return string.Join(" ", result);
        }

        internal static bool IsWord(Token token)
        {
            Token.Type type = token.GetTokenType();
            return type == Token.Type.Word
                || type == Token.Type.WordWithSymbol
                || type == Token.Type.WordAlphanumerical
                || type == Token.Type.UnknownWord;
        }

        internal string SplitNecessaryWords(List<Token> tokens, bool useLookup)
        {
            List<string> result = new List<string>();
            foreach (Token token in tokens)
            {
                string text = token.GetText();
                if (IsWord(token))
                {
                    result.Add(SeparateCommon(text, useLookup));
                }
                else
                {
                    result.Add(text);
                }
            }
            return string.Join(" ", result);
        }

        internal string SplitBruteForce(List<Token> tokens, int minSize)
        {
            List<string> result = new List<string>();
            foreach (Token token in tokens)
            {
                string text = token.GetText();
                if (IsWord(token))
                {
                    result.Add(SeparateBrute(text, minSize));
                }
                else
                {
                    result.Add(text);
                }
            }
            return string.Join(" ", result);
        }

        internal string ReplaceCommon(List<Token> tokens)
        {
            List<string> result = new List<string>();
            foreach (Token token in tokens)
            {
                string text = token.GetText();
                result.Add(replacements.GetValueOrDefault(text, text));
            }
            return string.Join(" ", result);
        }
    }
}
