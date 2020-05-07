using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZemberekDotNet.Core;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Math;
using ZemberekDotNet.Core.Native.Collections;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Text.Distance;
using ZemberekDotNet.Core.Turkish;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Normalization
{
    /// <summary>
    ///  A modified implementation of Hassan and Menezes's 2013 paper "Social Text Normalization using
    ///  Contextual Graph Random Walks".
    ///  <p></p>
    ///  Algorithm basically works like this:
    ///  <p></p>
    ///  First, we need to have two vocabularies from a corpus. One vocabulary is correct words, other is
    ///  noisy words. This operation is actually quite tricky as how to decide if a word is noisy is not
    ///  easy. For Turkish we use morphological analysis but it may actually fail for some proper nouns
    ///  and for inputs where Turkish characters are not used. For example in sentence "öle olmaz" word
    ///  "öle" passes morphological analysis but it is actually "öyle".
    ///  <p></p>
    ///  Second, a bipartite graph is generated from the corpus. There are two sides in the graph. One
    ///  represents contexts, other represents words. For example:
    ///  <pre>
    ///   context(bu * eve) -> (sabah:105, zabah:3, akşam:126, aksam:7, mavi:2 ...)
    ///   context(sabah * geldim) -> (eve:56, işe:78, okula:64, okulua:2 ...)
    ///   word(sabah) -> ([bu eve]:105, [bu kahvaltıda]:23, [her olmaz]:7 ...)
    ///   word(zabah) -> ([bu eve]:3 ...)
    ///  </pre>
    ///  (bu * eve) represents a context. And sabah:105 means from this context, "sabah" appeared in the
    ///  middle 105 times. And noisy "zabah" appeared 3 times.
    ///  <p></p>
    ///  Here we do something different from original paper, when building contextual similarity graph, we
    ///  use 32 bit hash values of the contexts instead of the contexts itself. This reduces memory and
    ///  calculation cost greatly.
    ///  <p></p>
    ///  After this graph is constructed, For every noisy word in the graph several random walks are done
    ///  as below:
    ///  <pre>
    ///  - Start from a noisy word.
    ///  - repeat k times (such as k = 100)
    ///  -- Select one of the contexts of this word randomly. But, random is not uniform.
    ///    Context is selected proportional to occurrence counts.
    ///  -- From context, similarly randomly hop to a word.
    ///  -- If word is noisy, continue hops.
    ///  -- If word is not noisy or hop count reaches to a certain value (such as 4), stop.
    ///     Store average-hitting-time
    ///  - Calculate contextual and lexical similarity and prune. Lexical similarity is calculated
    ///    with modified edit distance and longest common substring ratio (from Contractor et al. 2010
    ///    Unsupervised cleansing of noisy text)
    /// 
    ///  </pre>
    ///  If random walks are repeated for many times, All candidates that may be the correct version can
    ///  be collected. After that, a Viterbi search using a language model can be performed for better
    ///  accuracy.
    /// </summary>
    public class NoisyWordsLexiconGenerator
    {
        public static readonly double LogBaseForCounts = 1.4;
        public static readonly int WalkCount = 300;
        public static readonly int MaxHopCount = 7;
        public static readonly double OverallScoreThreshold = 0.41;
        public static readonly double LexicalSimilarityScoreThreshold = 0.4;
        public static readonly TurkishAlphabet alphabet = TurkishAlphabet.Instance;

        NormalizationVocabulary vocabulary;
        int threadCount;

        public NoisyWordsLexiconGenerator(
            NormalizationVocabulary vocabulary, int threadCount)
        {
            this.vocabulary = vocabulary;
            this.threadCount = threadCount;
        }

        public static void DummyMain(string[] args)
        {

            int threadCount = Environment.ProcessorCount / 2;
            if (threadCount > 22)
            {
                threadCount = 22;
            }

            string corporaRoot = "/home/aaa/data/normalization/corpus";
            string workDir = "/home/aaa/data/normalization/test-large";
            string corpusDirList = Path.Combine(corporaRoot, "all-list");

            Directory.CreateDirectory(workDir);

            string correct = Path.Combine(workDir, "correct");
            string incorrect = Path.Combine(workDir, "incorrect");
            string maybeIncorrect = Path.Combine(workDir, "possibly-incorrect");

            NormalizationVocabulary vocabulary = new NormalizationVocabulary(
                correct, incorrect, maybeIncorrect, 1, 3, 1);

            NoisyWordsLexiconGenerator generator = new NoisyWordsLexiconGenerator(vocabulary, threadCount);

            BlockTextLoader corpusProvider = BlockTextLoader
                .FromDirectoryRoot(corporaRoot, corpusDirList, 50_000);

            // create graph
            string graphPath = Path.Combine(workDir, "graph");
            generator.CreateGraph(corpusProvider, graphPath);

            Histogram<string> incorrectWords = Histogram<string>.LoadFromUtf8File(incorrect, ' ');
            incorrectWords.Add(Histogram<string>.LoadFromUtf8File(maybeIncorrect, ' '));
            generator.CreateCandidates(graphPath, workDir, incorrectWords);

            Log.Info("Done");
        }

        internal void CreateCandidates(string graphPath, string outRoot, Histogram<string> noisyWords)
        {
            Stopwatch sw = Stopwatch.StartNew();

            // create Random Walker
            Log.Info("Constructing random walk graph from {0}", graphPath);
            RandomWalker walker = RandomWalker.FromGraphFile(vocabulary, graphPath);

            Log.Info("Collecting candidates data.");

            WalkResult walkResult = walker.Walk(WalkCount, MaxHopCount, threadCount);
            string allCandidates = Path.Combine(outRoot, "all-candidates");
            string lookup = Path.Combine(outRoot, "lookup-from-graph");

            Log.Info("Saving candidates.");

            using (StreamWriter pw = new StreamWriter(File.OpenWrite(allCandidates), Encoding.UTF8))
            {
                using (StreamWriter pwLookup = new StreamWriter(File.OpenWrite(lookup), Encoding.UTF8))
                {
                    List<string> words = new List<string>(walkResult.AllCandidates.Keys);

                    words.Sort((a, b) => noisyWords.GetCount(b).CompareTo(noisyWords.GetCount(a)));

                    foreach (string s in words)
                    {
                        List<WalkScore> scores = new List<WalkScore>(walkResult.AllCandidates[s]);
                        float lambda1 = 1f;
                        float lambda2 = 1f;
                        scores.Sort(
                            (a, b) => b.GetScore(lambda1, lambda2).CompareTo(a.GetScore(lambda1, lambda2)));
                        scores = scores.Where(w => w.GetScore(lambda1, lambda2) >= OverallScoreThreshold)
                            .ToList();
                        pw.WriteLine(s);
                        foreach (WalkScore score in scores)
                        {
                            pw.WriteLine(string.Format("{0}:{1:F3} ({2:F3} - {3:F3})",
                                score.Candidate,
                                score.GetScore(lambda1, lambda2),
                                score.ContextualSimilarity,
                                score.LexicalSimilarity));
                        }
                        pw.WriteLine();

                        if (scores.Count == 0)
                        {
                            continue;
                        }

                        List<string> candidates = new List<string>();
                        foreach (WalkScore score in scores)
                        {
                            if (score.Candidate.Equals(s))
                            {
                                continue;
                            }
                            // if there is an ascii equivalent (but not the same), only return that.
                            if (vocabulary.IsMaybeIncorrect(s) &&
                                alphabet.ToAscii(s).Equals(alphabet.ToAscii(score.Candidate)))
                            {
                                candidates = new List<string>(1);
                                candidates.Add(score.Candidate);
                                break;
                            }

                            if (score.LexicalSimilarity * lambda2 < LexicalSimilarityScoreThreshold)
                            {
                                continue;
                            }
                            candidates.Add(score.Candidate);
                        }

                        if (candidates.Count > 0 && vocabulary.IsMaybeIncorrect(s) && !candidates.Contains(s))
                        {
                            candidates.Add(s);
                        }
                        if (!candidates.IsEmpty())
                        {
                            pwLookup.WriteLine(s + "=" + string.Join(",", candidates));
                        }
                    }
                }
            }
            Log.Info("Candidates collected in {0:F3} seconds.",
                    sw.ElapsedMilliseconds / 1000d);
        }

        internal void CreateGraph(BlockTextLoader corpusProvider, string graphPath)
        {
            Stopwatch sw = Stopwatch.StartNew();

            ContextualSimilarityGraph graph = BuildGraph(corpusProvider, 1);
            Log.Info("Serializing graph for random walk structure.");
            graph.SerializeForRandomWalk(graphPath);
            Log.Info("Serialized to %s", graphPath);
            Log.Info("Graph created in {0:F3} seconds.",
                sw.ElapsedMilliseconds / 1000d);
        }

        internal class RandomWalkNode
        {
            int[] keysCounts;
            int totalCounts;

            internal RandomWalkNode(int[] keysCounts, int totalCounts)
            {
                this.KeysCounts = keysCounts;
                this.TotalCounts = totalCounts;
            }

            public int[] KeysCounts { get => keysCounts; set => keysCounts = value; }
            public int TotalCounts { get => totalCounts; set => totalCounts = value; }
        }

        internal class RandomWalker
        {
            UIntMap<RandomWalkNode> contextHashesToWords;
            UIntMap<RandomWalkNode> wordsToContextHashes;
            NormalizationVocabulary vocabulary;

            object lockObject = new object();
            private static readonly Random rnd = new Random(1);

            internal UIntMap<RandomWalkNode> ContextHashesToWords { get => contextHashesToWords; set => contextHashesToWords = value; }
            internal UIntMap<RandomWalkNode> WordsToContextHashes { get => wordsToContextHashes; set => wordsToContextHashes = value; }
            internal NormalizationVocabulary Vocabulary { get => vocabulary; set => vocabulary = value; }

            RandomWalker(
                NormalizationVocabulary vocabulary,
                UIntMap<RandomWalkNode> contextHashesToWords,
                UIntMap<RandomWalkNode> wordsToContextHashes)
            {
                this.Vocabulary = vocabulary;
                this.ContextHashesToWords = contextHashesToWords;
                this.WordsToContextHashes = wordsToContextHashes;
            }

            internal static RandomWalker FromGraphFile(NormalizationVocabulary vocabulary, string path)

            {
                using (BinaryReader dis = IOUtil.GetDataInputStream(path))
                {
                    UIntMap<RandomWalkNode> contextHashesToWords = LoadNodes(dis, "context");
                    UIntMap<RandomWalkNode> wordsToContextHashes = LoadNodes(dis, "word");
                    return new RandomWalker(vocabulary, contextHashesToWords, wordsToContextHashes);
                }
            }

            private static UIntMap<RandomWalkNode> LoadNodes(BinaryReader dis, string info)
            {
                int nodeCount = dis.ReadInt32().EnsureEndianness();
                Log.Info("There are {0} {1} nodes.", nodeCount, info);
                UIntMap<RandomWalkNode> edgeMap = new UIntMap<RandomWalkNode>(nodeCount / 2);
                for (int i = 0; i < nodeCount; i++)
                {
                    int key = dis.ReadInt32().EnsureEndianness();
                    int size = dis.ReadInt32().EnsureEndianness();
                    int[] keysCounts = new int[size * 2];
                    int totalCount = 0;
                    for (int j = 0; j < size * 2; j++)
                    {
                        int val = dis.ReadInt32().EnsureEndianness();

                        if ((j & 0x01) == 1)
                        {
                            val = (int)LogMath.Log(LogBaseForCounts, val);
                            if (val <= 0)
                            {
                                val = 1;
                            }
                            totalCount += val;
                        }
                        keysCounts[j] = val;

                    }
                    edgeMap.Put(key, new RandomWalkNode(keysCounts, totalCount));
                    if (i > 0 && i % 500_000 == 0)
                    {
                        Log.Info("{0} {1} node loaded.", i, info);
                    }
                }
                return edgeMap;
            }

            internal WalkResult Walk(int walkCount, int maxHopCount, int threadCount)
            {

                // prepare work items for threads. Each work item contains 5000 words.
                List<Work> workList = new List<Work>();
                int batchSize = 5_000;
                IntVector vector = new IntVector(batchSize);
                foreach (int wordIndex in WordsToContextHashes.GetKeys())
                {
                    // only noisy or maybe-noisy words
                    if (Vocabulary.IsCorrect(wordIndex))
                    {
                        continue;
                    }
                    vector.Add(wordIndex);
                    if (vector.Size() == batchSize)
                    {
                        workList.Add(new Work(vector.CopyOf()));
                        vector = new IntVector(batchSize);
                    }
                }
                // for remaining data.
                if (vector.Size() > 0)
                {
                    workList.Add(new Work(vector.CopyOf()));
                }

                WalkResult globalResult = new WalkResult();

                Parallel.ForEach(workList, new ParallelOptions
                {
                    MaxDegreeOfParallelism = threadCount
                }, (Work work) =>
                {
                    WalkResult result = new WalkResult();
                    CharDistance distanceCalculator = new CharDistance();
                    foreach (int wordIndex in work.WordIndexes)
                    {
                        // Only incorrect and maybe-incorrect words. Check anyway, to be sure.
                        if (Vocabulary.IsCorrect(wordIndex))
                        {
                            continue;
                        }

                        Dictionary<string, WalkScore> scores = new Dictionary<string, WalkScore>();

                        for (int i = 0; i < walkCount; i++)
                        {
                            int nodeIndex = wordIndex;
                            bool atWordNode = true;
                            for (int j = 0; j < maxHopCount; j++)
                            {

                                RandomWalkNode node = atWordNode ?
                                    WordsToContextHashes.Get(nodeIndex) :
                                    ContextHashesToWords.Get(nodeIndex);

                                nodeIndex = SelectFromDistribution(node);

                                atWordNode = !atWordNode;

                                // if we reach to a valid word ([...] --> [Context node] --> [Valid word node] )
                                bool maybeIncorrect = Vocabulary.IsMaybeIncorrect(nodeIndex);
                                if (atWordNode && (nodeIndex != wordIndex || maybeIncorrect)
                                    && (Vocabulary.IsCorrect(nodeIndex) || maybeIncorrect))
                                {
                                    string currentWord = Vocabulary.GetWord(nodeIndex);
                                    WalkScore score = scores.GetValueOrDefault(currentWord);
                                    if (score == null)
                                    {
                                        score = new WalkScore(currentWord);
                                        scores.Add(currentWord, score);
                                    }
                                    score.Update(j + 1);
                                    break;
                                }
                            }
                        }

                        // calculate contextual similarity probabilities.
                        float totalAverageHittingTime = 0;
                        foreach (WalkScore score in scores.Values)
                        {
                            totalAverageHittingTime += score.GetAverageHittingTime();
                        }

                        foreach (string s in scores.Keys)
                        {
                            WalkScore score = scores.GetValueOrDefault(s);
                            score.ContextualSimilarity =
                                              score.GetAverageHittingTime() /
                                                  (totalAverageHittingTime - score.GetAverageHittingTime());
                        }

                        // calculate lexical similarity cost. This is slow for now.
                        // convert to ascii and remove vowels and repetitions.
                        string word = Vocabulary.GetWord(wordIndex);
                        string reducedSource = ReduceWord(word);
                        string asciiSource = TurkishAlphabet.Instance.ToAscii(word);

                        foreach (string s in scores.Keys)
                        {
                            string reducedTarget = ReduceWord(s);
                            string asciiTarget = TurkishAlphabet.Instance.ToAscii(s);
                            float editDistance =
                                (float)distanceCalculator.Distance(reducedSource, reducedTarget) + 1;
                            float asciiEditDistance =
                                (float)distanceCalculator.Distance(asciiSource, asciiTarget) + 1;

                            // longest commons substring ratio
                            float lcsr = LongestCommonSubstring(asciiSource, asciiTarget, true).Length * 1f /
                                Math.Max(s.Length, word.Length);

                            WalkScore score = scores.GetValueOrDefault(s);
                            float l1 = lcsr / editDistance;
                            float l2 = lcsr / asciiEditDistance;

                            score.LexicalSimilarity = Math.Max(l1, l2);
                        }
                        result.AllCandidates.Add(word, scores.Values);
                    }
                    lock (lockObject)
                    {
                        globalResult.AllCandidates.Add(result.AllCandidates);
                        Log.Info("{0} words processed.", globalResult.AllCandidates.Keys.Count());
                    }
                });

                return globalResult;
            }

            /// <summary>
            /// From a node, selects a connected node randomly.Randomness is not uniform, it is proportional
            /// to the occurrence counts attached to the edges.
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            internal int SelectFromDistribution(RandomWalkNode node)
            {
                int dice = rnd.Next(node.TotalCounts + 1);
                int accumulator = 0;
                int[] keysCounts = node.KeysCounts;
                for (int i = 0; i < keysCounts.Length; i += 2)
                {
                    accumulator += keysCounts[i + 1];
                    if (accumulator >= dice)
                    {
                        return keysCounts[i];
                    }
                }
                throw new InvalidOperationException("Unreachable.");
            }
        }

        internal class WalkResult
        {
            MultiMap<string, WalkScore> allCandidates = new MultiMap<string, WalkScore>();

            internal MultiMap<string, WalkScore> AllCandidates { get => allCandidates; set => allCandidates = value; }
        }

        internal class WalkScore
        {
            string candidate;
            int hitCount;
            int hopCount;
            float contextualSimilarity;
            float lexicalSimilarity;

            public string Candidate { get => candidate; set => candidate = value; }
            public int HitCount { get => hitCount; set => hitCount = value; }
            public int HopCount { get => hopCount; set => hopCount = value; }
            public float ContextualSimilarity { get => contextualSimilarity; set => contextualSimilarity = value; }
            public float LexicalSimilarity { get => lexicalSimilarity; set => lexicalSimilarity = value; }

            internal void Update(int hopeCount)
            {
                this.HitCount++;
                this.HopCount += hopeCount;
            }

            internal float GetAverageHittingTime()
            {
                return HopCount * 1f / HitCount;
            }

            internal float GetScore()
            {
                return ContextualSimilarity + LexicalSimilarity;
            }

            internal float GetScore(float lambda1, float lambda2)
            {
                return lambda1 * ContextualSimilarity + lambda2 * LexicalSimilarity;
            }

            internal WalkScore(string candidate)
            {
                this.Candidate = candidate;
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

                WalkScore walkScore = (WalkScore)o;

                return Candidate.Equals(walkScore.Candidate);
            }

            public override int GetHashCode()
            {
                return Candidate.GetHashCode();
            }
        }

        private class Work
        {
            int[] wordIndexes;

            internal Work(int[] wordIndexes)
            {
                this.WordIndexes = wordIndexes;
            }

            public int[] WordIndexes { get => wordIndexes; set => wordIndexes = value; }
        }

        private static ConcurrentDictionary<string, string> reducedWords = new ConcurrentDictionary<string, string>(Environment.ProcessorCount, 100_000);

        private static string ReduceWord(string input)
        {
            string cached = reducedWords.GetValueOrDefault(input);
            if (cached != null)
            {
                return cached;
            }
            string s = TurkishAlphabet.Instance.ToAscii(input);
            if (input.Length < 3)
            {
                return s;
            }
            StringBuilder sb = new StringBuilder(input.Length - 2);
            char previous = (char)0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (TurkishAlphabet.Instance.IsVowel(c) || c == 'ğ')
                {
                    previous = (char)0;
                    continue;
                }
                if (previous == c)
                {
                    continue;
                }
                sb.Append(c);
                previous = c;
            }
            string reduced = sb.ToString();

            if (reduced.Length == 0)
            {
                return input;
            }

            reducedWords.TryAdd(input, reduced);
            return reduced;
        }

        public class NormalizationVocabulary
        {
            List<string> words;
            UIntValueMap<string> indexes = new UIntValueMap<string>();
            int noisyWordStart;
            int maybeIncorrectWordStart;

            internal NormalizationVocabulary(
                string correct,
                string incorrect,
                string maybeIncorrect,
                int correctMinCount,
                int incorrectMinCount,
                int maybeIncorrectMinCount)
            {
                Histogram<string> correctWords = Histogram<string>.LoadFromUtf8File(correct, ' ');
                Histogram<string> noisyWords = Histogram<string>.LoadFromUtf8File(incorrect, ' ');
                Histogram<string> maybeIncorrectWords = new Histogram<string>();
                if (maybeIncorrect != null)
                {
                    maybeIncorrectWords = Histogram<string>.LoadFromUtf8File(maybeIncorrect, ' ');
                }
                correctWords.RemoveSmaller(correctMinCount);
                noisyWords.RemoveSmaller(incorrectMinCount);
                maybeIncorrectWords.RemoveSmaller(maybeIncorrectMinCount);
                this.noisyWordStart = correctWords.Size();

                this.words = new List<string>(correctWords.GetSortedList());
                words.AddRange(noisyWords.GetSortedList());

                this.maybeIncorrectWordStart = words.Count;
                words.AddRange(maybeIncorrectWords.GetSortedList());

                int i = 0;
                foreach (string word in words)
                {
                    indexes.Put(word, i);
                    i++;
                }
            }

            internal int TotalSize()
            {
                return words.Count;
            }

            internal bool IsCorrect(int id)
            {
                return id >= 0 && id < noisyWordStart;
            }

            internal bool IsCorrect(string id)
            {
                return IsCorrect(GetIndex(id));
            }

            internal bool IsMaybeIncorrect(string id)
            {
                return IsMaybeIncorrect(GetIndex(id));
            }

            internal bool IsMaybeIncorrect(int id)
            {
                return id >= maybeIncorrectWordStart;
            }

            internal bool IsIncorrect(string id)
            {
                return IsIncorrect(GetIndex(id));
            }

            internal bool IsIncorrect(int id)
            {
                return id >= noisyWordStart && id < maybeIncorrectWordStart;
            }

            internal int GetIndex(string word)
            {
                return indexes.Get(word);
            }

            internal string GetWord(int id)
            {
                return words[id];
            }
        }

        /// <summary>
        /// Generates and serializes a bipartite graph that represents contextual similarity.
        /// </summary>
        /// <param name="corpora"></param>
        /// <param name="contextSize"></param>
        /// <returns></returns>
        internal ContextualSimilarityGraph BuildGraph(
            BlockTextLoader corpora,
            int contextSize)
        {
            ContextualSimilarityGraph graph = new ContextualSimilarityGraph(vocabulary, contextSize);
            graph.Build(corpora, threadCount);
            Log.Info("Context hash count before pruning (no singletons) = " + graph.ContextHashCount());
            graph.PruneContextNodes();
            Log.Info("Context hash count after pruning  = " + graph.ContextHashCount());
            Log.Info("Edge count = %d", graph.EdgeCount());
            Log.Info("Creating Words -> Context counts.");
            return graph;
        }

        private static readonly string SENTENCE_START = "<s>";
        private static readonly string SENTENCE_END = "</s>";

        internal class ContextualSimilarityGraph
        {

            // this holds context hashes as keys.
            // values are words and their counts for context hash keys.
            UIntMap<IntIntMap> contextHashToWordCounts = new UIntMap<IntIntMap>(5_000_000);

            // This is for memory optimization. It holds <hash, wordIndex> values.
            // Context occurs only once and with count 1 stays in this.
            // This may be discarded during pruning.
            IntIntMap singletons = new IntIntMap(5_000_000);

            NormalizationVocabulary vocabulary;
            object lockObject = new object();

            int contextSize;

            internal ContextualSimilarityGraph(
                 NormalizationVocabulary vocabulary,
                 int contextSize)
            {

                if (contextSize < 1 || contextSize > 2)
                {
                    throw new InvalidOperationException("Context must be 1 or 2 but it is " + contextSize);
                }
                this.contextSize = contextSize;
                this.vocabulary = vocabulary;
            }

            internal void Build(
                 BlockTextLoader corpora,
                 int threadCount)
            {
                Parallel.ForEach(corpora, new ParallelOptions
                {
                    MaxDegreeOfParallelism = threadCount
                }, (TextChunk chunk) =>
                {
                    Log.Info("Processing {0}", chunk);
                    UIntMap<IntIntMap> localContextCounts = new UIntMap<IntIntMap>(100_000);
                    IntIntMap localSingletons = new IntIntMap(100_000);

                    List<string> sentences = TextCleaner.CleanAndExtractSentences(chunk.GetData());
                    foreach (string sentence in sentences)
                    {
                        List<string> tokens = GetTokens(sentence);

                        // context array will be reused.
                        string[] context = new string[contextSize * 2];

                        for (int i = contextSize; i < tokens.Count - contextSize; i++)
                        {

                            int wordIndex = vocabulary.GetIndex(tokens[i]);

                            // if current word is out of vocabulary (neither valid nor noisy) , continue.
                            if (wordIndex == -1)
                            {
                                continue;
                            }

                            // gather context and calculate hash
                            if (contextSize == 1)
                            {
                                context[0] = tokens[i - 1];
                                context[1] = tokens[i + 1];
                            }
                            else
                            {
                                context[0] = tokens[i - 2];
                                context[1] = tokens[i - 1];
                                context[2] = tokens[i + 1];
                                context[3] = tokens[i + 2];
                            }
                            int hash = Hash(context);

                            //first check singletons.
                            if (localSingletons.ContainsKey(hash))
                            {
                                int val = localSingletons.Get(hash);
                                localSingletons.Remove(hash);
                                IntIntMap m = new IntIntMap(2);
                                m.Increment(val, 1);
                                m.Increment(wordIndex, 1);
                                localContextCounts.Put(hash, m);
                            }
                            else
                            {
                                // update context -> word counts
                                IntIntMap wordCounts = localContextCounts.Get(hash);
                                if (wordCounts != null)
                                {
                                    wordCounts = new IntIntMap(1);
                                    localContextCounts.Put(hash, wordCounts);
                                    wordCounts.Increment(wordIndex, 1);
                                }
                                else
                                {
                                    localSingletons.Put(hash, wordIndex);
                                }
                            }
                        }
                    }

                    lock (lockObject)
                    {
                        foreach (int key in localContextCounts.GetKeys())
                        {
                            IntIntMap localMap = localContextCounts.Get(key);
                            IntIntMap globalMap = contextHashToWordCounts.Get(key);
                            if (globalMap == null)
                            {
                                // remove it from global singletons if exist.
                                if (singletons.ContainsKey(key))
                                {
                                    int wordIndex = singletons.Get(key);
                                    singletons.Remove(key);
                                    localMap.Increment(wordIndex, 1);
                                }
                                contextHashToWordCounts.Put(key, localMap);
                            }
                            else
                            {
                                foreach (int word in localMap.GetKeys())
                                {
                                    int localCount = localMap.Get(word);
                                    globalMap.Increment(word, localCount);
                                }
                            }
                        }
                        // now put singletons.
                        foreach (int key in localSingletons.GetKeys())
                        {
                            int wordIndex = localSingletons.Get(key);
                            IntIntMap mm = contextHashToWordCounts.Get(key);
                            if (mm == null)
                            {
                                if (singletons.ContainsKey(key))
                                {
                                    int w = singletons.Get(key);
                                    singletons.Remove(key);
                                    mm = new IntIntMap(1);
                                    mm.Increment(w, 1);
                                    mm.Increment(wordIndex, 1);
                                    contextHashToWordCounts.Put(key, mm);
                                }
                                else
                                {
                                    singletons.Put(key, wordIndex);
                                }
                            }
                            else
                            {
                                mm.Increment(wordIndex, 1);
                            }
                        }
                        long contextCount = contextHashToWordCounts.Size() + singletons.Size();
                        Log.Info("Context count = {0}, Singleton context count = ",
                                        contextCount, singletons.Size());
                    }
                });
            }

            private List<string> GetTokens(string sentence)
            {
                List<string> tokens = new List<string>();

                for (int i = 0; i < contextSize; i++)
                {
                    tokens.Add(SENTENCE_START);
                }

                sentence = sentence.ToLower(Turkish.Locale);
                List<Token> raw = TurkishTokenizer.Default.Tokenize(sentence);

                // use substitute values for numbers, urls etc.
                foreach (Token token in raw)
                {
                    if (token.GetTokenType() == Token.Type.Punctuation)
                    {
                        continue;
                    }
                    string text = token.GetText();
                    switch (token.GetTokenType())
                    {
                        case Token.Type.Time:
                        case Token.Type.PercentNumeral:
                        case Token.Type.Number:
                        case Token.Type.Date:
                            text = Regex.Replace(text, "\\d+", "_d");
                            break;
                        case Token.Type.URL:
                            text = "<url>";
                            break;
                        case Token.Type.HashTag:
                            text = "<hashtag>";
                            break;
                        case Token.Type.Email:
                            text = "<email>";
                            break;
                    }
                    tokens.Add(text);
                }

                for (int i = 0; i < contextSize; i++)
                {
                    tokens.Add(SENTENCE_END);
                }
                return tokens;
            }

            internal void PruneContextNodes()
            {
                // remove all singletons.
                singletons = new IntIntMap();

                UIntSet keysToPrune = new UIntSet();
                foreach (int contextHash in contextHashToWordCounts.GetKeys())
                {
                    IntIntMap m = contextHashToWordCounts.Get(contextHash);

                    // prune if a context only points to a single word.
                    if (m.Size() <= 1)
                    {
                        keysToPrune.Add(contextHash);
                        continue;
                    }

                    // prune if a context is only connected to noisy words. For speed we only check nodes with
                    // at most five connections.
                    if (m.Size() < 5)
                    {
                        int noisyCount = 0;
                        foreach (int wordIndex in m.GetKeys())
                        {
                            if (vocabulary.IsIncorrect(wordIndex))
                            {
                                noisyCount++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (noisyCount == m.Size())
                        {
                            keysToPrune.Add(contextHash);
                        }
                    }
                }
                foreach (int keyToRemove in keysToPrune.GetKeys())
                {
                    contextHashToWordCounts.Remove(keyToRemove);
                }
            }

            internal void SerializeForRandomWalk(string p)
            {

                UIntMap<IntIntMap> wordToContexts = new UIntMap<IntIntMap>();
                foreach (int contextHash in contextHashToWordCounts.GetKeys())
                {
                    IntIntMap m = contextHashToWordCounts.Get(contextHash);
                    foreach (int worIndex in m.GetKeys())
                    {
                        int count = m.Get(worIndex);
                        IntIntMap contextCounts = wordToContexts.Get(worIndex);
                        if (contextCounts == null)
                        {
                            contextCounts = new IntIntMap(1);
                            wordToContexts.Put(worIndex, contextCounts);
                        }
                        contextCounts.Put(contextHash, count);
                    }
                }

                using (BinaryWriter dos = IOUtil.GetDataOutputStream(p))
                {
                    SerializeEdges(dos, contextHashToWordCounts);
                    SerializeEdges(dos, wordToContexts);
                }
            }

            private void SerializeEdges(BinaryWriter dos, UIntMap<IntIntMap> edgesMap)
            {
                dos.Write(edgesMap.Size().EnsureEndianness());

                foreach (int nodeIndex in edgesMap.GetKeys())
                {
                    dos.Write(nodeIndex.EnsureEndianness());
                    IntIntMap map = edgesMap.Get(nodeIndex);
                    if (map == null)
                    {
                        throw new InvalidOperationException("edge map is null!");
                    }
                    dos.Write(map.Size().EnsureEndianness());
                    IntPair[] pairs = map.GetAsPairs();
                    Array.Sort(pairs, (a, b) => b.Second.CompareTo(a.Second));
                    foreach (IntPair pair in pairs)
                    {
                        dos.Write(pair.First.EnsureEndianness());
                        dos.Write(pair.Second.EnsureEndianness());
                    }
                }
            }

            internal int ContextHashCount()
            {
                return contextHashToWordCounts.Size();
            }

            internal long EdgeCount()
            {
                long i = 0;
                foreach (IntIntMap m in contextHashToWordCounts)
                {
                    i += m.Size();
                }
                return i;
            }
        }

        /// <summary>
        /// calculates a non negative 31 bit hash value of a string array.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static int Hash(params string[] context)
        {
            int d = unchecked((int)0x811C9DC5);
            foreach (string s in context)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    d = (d ^ s[i]) * 16777619;
                }
            }
            return d & 0x7fffffff;
        }

        /// <summary>
        /// Finds the longest common substring of two strings.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="asciiTolerant"></param>
        /// <returns></returns>
        private static string LongestCommonSubstring(string a, string b, bool asciiTolerant)
        {
            int[][] lengths = new int[a.Length + 1][];

            // row 0 and column 0 are initialized to 0 already

            for (int i = 0; i < a.Length; i++)
            {
                lengths[i] = new int[b.Length + 1];
                for (int j = 0; j < b.Length; j++)
                {
                    bool b1 = asciiTolerant ?
                        TurkishAlphabet.Instance.IsAsciiEqual(a[i], b[j]) :
                        a[i] == b[j];
                    if (b1)
                    {
                        lengths[i + 1][j + 1] = lengths[i][j] + 1;
                    }
                    else
                    {
                        lengths[i + 1][j + 1] =
                            Math.Max(lengths[i + 1][j], lengths[i][j + 1]);
                    }
                }
            }

            // read the substring out from the matrix
            StringBuilder sb = new StringBuilder();
            for (int x = a.Length, y = b.Length;
                x != 0 && y != 0;)
            {
                if (lengths[x][y] == lengths[x - 1][y])
                {
                    x--;
                }
                else if (lengths[x][y] == lengths[x][y - 1])
                {
                    y--;
                }
                else
                {
                    Contract.Assert(a[x - 1] == b[y - 1]);
                    sb.Append(a[x - 1]);
                    x--;
                    y--;
                }
            }

            return sb.ToString().Reverse().ToString();
        }
    }
}
