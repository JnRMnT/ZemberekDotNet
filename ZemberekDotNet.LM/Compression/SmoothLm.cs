using System;
using System.IO;
using System.Text;
using ZemberekDotNet.Core.Hash;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Math;
using ZemberekDotNet.Core.Quantization;

namespace ZemberekDotNet.LM.Compression
{
    /// <summary>
    /// SmoothLm is a compressed, optionally quantized, randomized back-off n-gram language model.It
    /// uses Minimal Perfect Hash functions for compression, This means actual n-gram values are not
    /// stored in the model.Implementation is similar with the systems described in Gutthrie and
    /// Hepple's 'Storing the Web in Memory: Space Efficient Language Models with Constant Time Retrieval
    /// (2010)' paper. This is a lossy model because for non existing n-grams it may return an existing
    /// n-gram probability value.Probability of this happening depends on the fingerprint hash length.
    /// 
    /// This value is determined during the model creation.Regularly 8,16 or 24 bit fingerprints are
    /// used and false positive probability for an non existing n-gram is (probability of an n-gram does
    /// not exist in LM)*1/(2^fingerprint bit size). SmoothLm also provides quantization for even more
    /// compactness. So probability and back-off values can be quantized to 8, 16 or 24 bits.
    /// </summary>
    public class SmoothLm : BaseLanguageModel, INgramLanguageModel
    {
        public static readonly float DefaultLogBase = 10;
        public static readonly float DefaultUnigramWeight = 1;
        public static readonly float DefaultUnknownBackoffPenalty = 0;
        public static readonly float DefaultStupidBackoffAlpha = 0.4f;
        public static readonly int DefaultUnknownTokenProbability = -20;

        private readonly int version;

        private readonly IMphf[] mphfs;

        private readonly FloatLookup[] probabilityLookups;
        private readonly FloatLookup[] backoffLookups;
        private readonly GramDataArray[] ngramData;
        public int[] counts;
        MphfType type;
        int falsePositiveCount;
        // used for debug purposes for calculation false-positive ratio.
        NgramIds ngramIds;
        private float[] unigramProbs;
        private float[] unigramBackoffs;
        private float logBase;
        private float unigramWeight;
        private float unknownBackoffPenalty;
        private bool useStupidBackoff = false;
        private float stupidBackoffLogAlpha;
        private float stupidBackoffAlpha;
        private bool countFalsePositives;

        private SmoothLm(
            BinaryReader dis,
            float logBase,
            float unigramWeight,
            float unknownBackoffPenalty,
            bool useStupidBackoff,
            float stupidBackoffAlpha,
            string ngramKeyFileDir) : this(dis) // load the lm data.
        {

            // Now apply necessary transformations and configurations
            this.unigramWeight = unigramWeight;
            this.unknownBackoffPenalty = unknownBackoffPenalty;
            this.useStupidBackoff = useStupidBackoff;
            this.stupidBackoffAlpha = stupidBackoffAlpha;

            if (logBase != DefaultLogBase)
            {
                Log.Debug("Changing log base from " + DefaultLogBase + " to " + logBase);
                ChangeLogBase(logBase);
                this.stupidBackoffLogAlpha = (float)(Math.Log(stupidBackoffAlpha) / Math.Log(logBase));
            }
            else
            {
                this.stupidBackoffLogAlpha = (float)(Math.Log(stupidBackoffAlpha) / Math
                    .Log(DefaultLogBase));
            }

            this.logBase = logBase;

            if (unigramWeight != DefaultUnigramWeight)
            {
                Log.Debug("Applying unigram smoothing with unigram weight: " + unigramWeight);
                ApplyUnigramSmoothing(unigramWeight);
            }

            if (useStupidBackoff)
            {
                Log.Debug("Lm will use stupid back off with alpha value: " + stupidBackoffAlpha);
            }
            if (ngramKeyFileDir != null)
            {
                if (!Directory.Exists(ngramKeyFileDir))
                {
                    Log.Warn("Ngram id file directory {0} does not exist. Continue without loading.",
                        ngramKeyFileDir);
                }
                else
                {
                    Log.Info("Loading actual n-gram id data.");
                    this.ngramIds = new NgramIds(this.order, ngramKeyFileDir, mphfs);
                }
            }
        }

        public static SmoothLmBuilder Builder(string file)
        {
            return new SmoothLmBuilder(file);
        }
        public static SmoothLmBuilder Builder(Stream stream)
        {
            return new SmoothLmBuilder(stream);
        }

        private SmoothLm(BinaryReader dis)
        {
            this.version = dis.ReadInt32().EnsureEndianness();
            int typeInt = dis.ReadInt32().EnsureEndianness();
            if (typeInt == 0)
            {
                type = MphfType.Small;
            }
            else
            {
                type = MphfType.Large;
            }

            this.logBase = (float)dis.ReadDouble().EnsureEndianness();
            this.order = dis.ReadInt32().EnsureEndianness();

            counts = new int[order + 1];
            for (int i = 1; i <= order; i++)
            {
                counts[i] = dis.ReadInt32().EnsureEndianness();
            }

            // probability lookups
            probabilityLookups = new FloatLookup[order + 1];
            for (int i = 1; i <= order; i++)
            {
                // because we serialize values as doubles
                probabilityLookups[i] = FloatLookup.GetLookupFromDouble(dis);
            }
            // backoff lookups
            backoffLookups = new FloatLookup[order + 1];
            for (int i = 1; i < order; i++)
            {
                // because we serialize values as doubles
                backoffLookups[i] = FloatLookup.GetLookupFromDouble(dis);
            }

            //load fingerprint, probability and backoff data.
            ngramData = new GramDataArray[order + 1];
            for (int i = 1; i <= order; i++)
            {
                ngramData[i] = new GramDataArray(dis);
            }

            // we take the unigram probability data out to get rid of rank look-ups for speed.
            int unigramCount = ngramData[1].count;
            unigramProbs = new float[unigramCount];
            unigramBackoffs = order > 1 ? new float[unigramCount] : new float[0];
            for (int i = 0; i < unigramCount; i++)
            {
                int probability = ngramData[1].GetProbabilityRank(i);
                unigramProbs[i] = probabilityLookups[1].Get(probability);
                if (order > 1)
                {
                    int backoff = ngramData[1].GetBackoffRank(i);
                    unigramBackoffs[i] = backoffLookups[1].Get(backoff);
                }
            }

            // load MPHFs
            if (type == MphfType.Large)
            {
                mphfs = new LargeNgramMphf[order + 1];
                for (int i = 2; i <= order; i++)
                {
                    mphfs[i] = LargeNgramMphf.Deserialize(dis);
                }
            }
            else
            {
                mphfs = new MultiLevelMphf[order + 1];
                for (int i = 2; i <= order; i++)
                {
                    mphfs[i] = MultiLevelMphf.Deserialize(dis);
                }
            }

            // load vocabulary
            vocabulary = LmVocabulary.LoadFromBinaryReader(dis);

            // in case special tokens that does not exist in the actual unigrams are added (such as <unk>)
            // we adjust unigram data accordingly.
            int vocabularySize = vocabulary.Size();
            if (vocabularySize > unigramCount)
            {
                ngramData[1].count = vocabularySize;
                unigramProbs = unigramProbs.CopyOf(vocabularySize);
                unigramBackoffs = unigramBackoffs.CopyOf(vocabularySize);
                for (int i = unigramCount; i < vocabularySize; i++)
                {
                    unigramProbs[i] = DefaultUnknownTokenProbability;
                    unigramBackoffs[i] = 0;
                }
            }

            dis.Close();
        }

        /// <summary>
        /// Returns human readable information about the model.
        /// </summary>
        /// <returns></returns>
        public string Info()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Order : {0}{1}", order, Environment.NewLine));

            for (int i = 1; i < ngramData.Length; i++)
            {
                GramDataArray gramDataArray = ngramData[i];
                if (i == 1)
                {
                    sb.Append(string.Format("1 Grams: Count= {0}{1}", unigramProbs.Length, Environment.NewLine));
                    continue;
                }
                sb.Append(string.Format(
                    "{0} Grams: Count= {1}  Fingerprint Bits= {2}  Probabilty Bits= {3}  Back-off bits= {4}",
                    i,
                    gramDataArray.count,
                    gramDataArray.fpSize * 8,
                    gramDataArray.probSize * 8,
                    gramDataArray.backoffSize * 8,
                    Environment.NewLine));
            }
            sb.Append(string.Format("Log Base              : {0:F2}{1}", logBase, Environment.NewLine));
            sb.Append(string.Format("Unigram Weight        : {0:F2}{1}", unigramWeight, Environment.NewLine));
            sb.Append(string.Format("Using Stupid Back-off?: {0}{1}", useStupidBackoff ? "Yes" : "No", Environment.NewLine));
            if (useStupidBackoff)
            {
                sb.Append(string.Format("Stupid Back-off Alpha Value   : {0:F2}{1}", stupidBackoffAlpha, Environment.NewLine));
            }
            sb.Append(string.Format("Unknown Back-off N-gram penalty: {0:F2}{1}", unknownBackoffPenalty, Environment.NewLine));
            return sb.ToString();
        }

        public double GetStupidBackoffLogAlpha()
        {
            return stupidBackoffLogAlpha;
        }

        public int GetVersion()
        {
            return version;
        }

        public float GetUnigramProbability(int id)
        {
            return GetProbability(id);
        }

        public int GetOrder()
        {
            return order;
        }

        public LmVocabulary GetVocabulary()
        {
            return vocabulary;
        }

        /// <summary>
        /// returns an LookupCache instance with 2^16 slots. It is generally faster to use the cache's
        /// check(params int[]) method for getting probability of a word sequence.
        /// </summary>
        /// <returns></returns>
        public LookupCache GetCache()
        {
            return new LookupCache(this);
        }

        /// <summary>
        /// returns an LookupCache instance with 2^[bits] slots. It is generally faster to use the cache's
        /// check(params int[]) method for getting probability of a word sequence.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public LookupCache GetCache(int bits)
        {
            return new LookupCache(this, bits);
        }

        /// <summary>
        /// Gets the count of a particular gram size
        /// </summary>
        /// <param name="n">gram order</param>
        /// <returns>how many items exist for this particular order n-gram</returns>
        public int GetGramCount(int n)
        {
            return ngramData[n].count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordIndexes"></param>
        /// <returns>true if ngram exists in the lm. if actual key data is loaded during the construction of
        /// the compressed lm, the value returned by this function cannot be wrong. If not, the return
        /// value may be a false positive.</returns>
        public bool NGramExists(params int[] wordIndexes)
        {
            if (wordIndexes.Length < 1 || wordIndexes.Length > order)
            {
                throw new ArgumentException("Amount of tokens must be between 1 and " +
                    order + " But it is " + wordIndexes.Length);
            }
            int localOrder = wordIndexes.Length;
            if (localOrder == 1)
            {
                return wordIndexes[0] >= 0 && wordIndexes[0] < unigramProbs.Length;
            }
            int quickHash = MultiLevelMphf.Hash(wordIndexes, -1);
            int index = mphfs[localOrder].Get(wordIndexes, quickHash);
            if (ngramIds == null)
            {
                return ngramData[localOrder].CheckFingerPrint(quickHash, index);
            }
            return ngramIds.Exists(wordIndexes, index);
        }

        /// <summary>
        /// Retrieves the dequantized log probability of an n-gram.
        /// </summary>
        /// <param name="wordIndexes">token-index sequence. A word index is the unigram index value.</param>
        /// <returns>dequantized log probability of the n-gram. if n-gram does not exist, it returns
        /// LOG_ZERO</returns>
        public double GetProbabilityValue(params int[] wordIndexes)
        {
            int ng = wordIndexes.Length;
            if (ng == 1)
            {
                return unigramProbs[wordIndexes[0]];
            }

            int quickHash = MultiLevelMphf.Hash(wordIndexes, -1);

            int index = mphfs[ng].Get(wordIndexes, quickHash);

            if (ngramData[ng].CheckFingerPrint(quickHash, index))
            {
                return probabilityLookups[ng].Get(ngramData[ng].GetProbabilityRank(index));
            }
            else
            {
                return LogMath.LogZero;
            }
        }

        /// <summary>
        /// Retrieves the dequantized log probability of an n-gram.
        /// </summary>
        /// <param name="w0">token-index 0.</param>
        /// <param name="w1">token-index 1.</param>
        /// <returns>dequantized log probability of the n-gram. if n-gram does not exist, it returns
        /// LOG_ZERO</returns>
        public float GetBigramProbabilityValue(int w0, int w1)
        {

            int quickHash = MultiLevelMphf.Hash(w0, w1, -1);

            int index = mphfs[2].Get(w0, w1, quickHash);

            if (ngramData[2].CheckFingerPrint(quickHash, index))
            {
                return probabilityLookups[2].Get(ngramData[2].GetProbabilityRank(index));
            }
            else
            {
                return LogMath.LogZeroFloat;
            }
        }

        private bool IsFalsePositive(params int[] wordIndexes)
        {
            int length = wordIndexes.Length;
            if (length < 2)
            {
                return false;
            }
            int quickHash = MultiLevelMphf.Hash(wordIndexes, -1);
            int index = mphfs[length].Get(wordIndexes, quickHash);
            return ngramData[length].CheckFingerPrint(quickHash, index) // fingerprint matches
                && !ngramIds.Exists(wordIndexes, index); // but not the exact keys.
        }

        /// <summary>
        /// Retrieves the dequantized backoff value of an n-gram.
        /// </summary>
        /// <param name="wordIndexes">word-index sequence. A word index is the unigram index value.</param>
        /// <returns>dequantized log back-off value of the n-gram. if n-gram does not exist, it returns
        /// unknownBackoffPenalty value.</returns>
        public double GetBackoffValue(params int[] wordIndexes)
        {
            if (useStupidBackoff)
            {
                return stupidBackoffLogAlpha;
            }
            int ng = wordIndexes.Length;
            if (ng == 1)
            {
                return unigramBackoffs[wordIndexes[0]];
            }
            int quickHash = MultiLevelMphf.Hash(wordIndexes, -1);

            int nGramIndex = mphfs[ng].Get(wordIndexes, quickHash);

            if (ngramData[ng].CheckFingerPrint(quickHash, nGramIndex))
            {
                return backoffLookups[ng].Get(ngramData[ng].GetBackoffRank(nGramIndex));
            }
            else
            {
                return unknownBackoffPenalty;
            }
        }

        /// <summary>
        /// Retrieves the dequantized backoff value of an n-gram.
        /// </summary>
        /// <param name="w0">token-index 0.</param>
        /// <param name="w1">token-index 1.</param>
        /// <returns>dequantized log back-off value of the n-gram. if n-gram does not exist, it returns
        /// unknownBackoffPenalty value.</returns>
        public float GetBigramBackoffValue(int w0, int w1)
        {
            if (useStupidBackoff)
            {
                return stupidBackoffLogAlpha;
            }
            int quickHash = MultiLevelMphf.Hash(w0, w1, -1);
            int nGramIndex = mphfs[2].Get(w0, w1, quickHash);

            if (ngramData[2].CheckFingerPrint(quickHash, nGramIndex))
            {
                return backoffLookups[2].Get(ngramData[2].GetBackoffRank(nGramIndex));
            }
            else
            {
                return unknownBackoffPenalty;
            }
        }

        /// <summary>
        /// Calculates the dequantized probability value for an n-gram. If n-gram does not exist, it
        /// applies backoff calculation.
        /// </summary>
        /// <param name="wordIndexes">word index sequence. A word index is the unigram index value.</param>
        /// <returns>dequantized log backoff value of the n-gram. if there is no backoff value or n-gram
        /// does not exist, it returns LOG_ZERO. This mostly happens in the condition that words queried
        /// does not exist in the vocabulary.</returns>
        double GetProbabilityRecursive(params int[] wordIndexes)
        {

            if (wordIndexes.Length == 0 || wordIndexes.Length > order)
            {
                throw new ArgumentException(
                    "At least one or max Gram Count" + order + " tokens are required. But it is:"
                        + wordIndexes.Length);
            }

            double result = 0;
            double probability = GetProbabilityValue(wordIndexes);
            if (probability == LogMath.LogZero)
            { // if probability does not exist.
                if (wordIndexes.Length == 1)
                {
                    return LogMath.LogZero;
                }
                double backoffValue =
                    useStupidBackoff ? stupidBackoffLogAlpha : GetBackoffValue(Head(wordIndexes));
                result = result + backoffValue + GetProbabilityRecursive(Tail(wordIndexes));
            }
            else
            {
                result = probability;
            }
            return result;
        }

        /// <summary>
        /// This is the non recursive log probability calculation. It is more complicated but faster.
        /// </summary>
        /// <param name="words">word array</param>
        /// <returns>log probability.</returns>
        public double GetProbability(params string[] words)
        {
            return GetProbability(vocabulary.ToIndexes(words));
        }

        /// <summary>
        /// For Debugging purposes only. Counts false positives generated for an ngram.
        /// </summary>
        /// <param name="wordIndexes">word index array</param>
        public void CountFalsePositives(params int[] wordIndexes)
        {
            if (wordIndexes.Length == 0 || wordIndexes.Length > order)
            {
                throw new ArgumentException(
                    "At least one or max Gram Count" + order + " tokens are required. But it is:"
                        + wordIndexes.Length);
            }
            if (wordIndexes.Length == 1)
            {
                return;
            }
            if (IsFalsePositive(wordIndexes))
            {
                this.falsePositiveCount++;
            }
            if (GetProbability(wordIndexes) == LogMath.LogZero)
            {
                if (IsFalsePositive(Head(wordIndexes))) // check back-off false positive
                {
                    falsePositiveCount++;
                }
                CountFalsePositives(Tail(wordIndexes));
            }
        }

        public bool NGramIdsAvailable()
        {
            return ngramIds != null;
        }

        public int GetFalsePositiveCount()
        {
            return falsePositiveCount;
        }

        /// <summary>
        /// This is the non recursive log probability calculation. It is more complicated but faster.
        /// </summary>
        /// <param name="wordIndexes">word index array</param>
        /// <returns>log probability.</returns>
        public float GetProbability(params int[] wordIndexes)
        {
            int n = wordIndexes.Length;

            switch (n)
            {
                case 1:
                    return unigramProbs[wordIndexes[0]];
                case 2:
                    return GetBigramProbability(wordIndexes[0], wordIndexes[1]);
                case 3:
                    return GetTriGramProbability(wordIndexes);
                default:
                    break;
            }
            int begin = 0;
            float result = 0;
            int gram = n;
            while (gram > 1)
            {
                // try to find P(N|begin..N-1)
                int fingerPrint = MultiLevelMphf.Hash(wordIndexes, begin, n, -1);
                int nGramIndex = mphfs[gram].Get(wordIndexes, begin, n, fingerPrint);
                if (!ngramData[gram].CheckFingerPrint(fingerPrint,
                    nGramIndex))
                { // if there is no probability value, back off to B(begin..N-1)
                    if (useStupidBackoff)
                    {
                        if (gram == 2)
                        {
                            return result + unigramProbs[wordIndexes[n - 1]] + stupidBackoffLogAlpha;
                        }
                        else
                        {
                            result += stupidBackoffLogAlpha;
                        }
                    }
                    else
                    {
                        if (gram
                            == 2)
                        {  // we are already backed off to unigrams because no bigram found. So we return only P(N)+B(N-1)
                            return result + unigramProbs[wordIndexes[n - 1]] + unigramBackoffs[wordIndexes[begin]];
                        }
                        fingerPrint = MultiLevelMphf.Hash(wordIndexes, begin, n - 1, -1);
                        nGramIndex = mphfs[gram - 1].Get(wordIndexes, begin, n - 1, fingerPrint);
                        if (ngramData[gram - 1].CheckFingerPrint(fingerPrint,
                            nGramIndex))
                        { //if backoff available, we add it to result.
                            result += backoffLookups[gram - 1].Get(ngramData[gram - 1].GetBackoffRank(nGramIndex));
                        }
                        else
                        {
                            result += unknownBackoffPenalty;
                        }
                    }
                }
                else
                {
                    // we have found the P(N|begin..N-1) we return the accumulated result.
                    return result + probabilityLookups[gram]
                        .Get(ngramData[gram].GetProbabilityRank(nGramIndex));
                }
                begin++;
                gram = n - begin;
            }
            return result;
        }

        public float GetBigramProbability(int w0, int w1)
        {
            float prob = GetBigramProbabilityValue(w0, w1);
            if (prob == LogMath.LogZeroFloat)
            {
                if (useStupidBackoff)
                {
                    return stupidBackoffLogAlpha + unigramProbs[w1];
                }
                else
                {
                    return unigramBackoffs[w0] + unigramProbs[w1];
                }
            }
            else
            {
                return prob;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w0">token-index 0.</param>
        /// <param name="w1">token-index 1.</param>
        /// <param name="w2">token-index 2.</param>
        /// <returns>log probability.</returns>
        public float GetTriGramProbability(int w0, int w1, int w2)
        {
            int fingerPrint = MultiLevelMphf.Hash(w0, w1, w2, -1);
            return GetTriGramProbability(w0, w1, w2, fingerPrint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w0">token-index 0</param>
        /// <param name="w1">token-index 1</param>
        /// <param name="w2">token-index 2</param>
        /// <param name="fingerPrint"></param>
        /// <returns>log probability.</returns>
        public float GetTriGramProbability(int w0, int w1, int w2, int fingerPrint)
        {
            int nGramIndex = mphfs[3].Get(w0, w1, w2, fingerPrint);
            if (!ngramData[3].CheckFingerPrint(fingerPrint, nGramIndex))
            { //3 gram does not exist.
                return GetBigramBackoffValue(w0, w1) + GetBigramProbability(w1, w2);
            }
            else
            {
                return probabilityLookups[3].Get(ngramData[3].GetProbabilityRank(nGramIndex));
            }
        }

        /// <summary>
        /// log probability.
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public float GetTriGramProbability(params int[] w)
        {
            int fingerPrint = MultiLevelMphf.Hash(w, -1);
            int nGramIndex = mphfs[3].Get(w, fingerPrint);
            if (!ngramData[3].CheckFingerPrint(fingerPrint, nGramIndex))
            { //3 gram does not exist.
                return GetBigramBackoffValue(w[0], w[1]) + GetBigramProbability(w[1], w[2]);
            }
            else
            {
                return probabilityLookups[3].Get(ngramData[3].GetProbabilityRank(nGramIndex));
            }
        }

        /// <summary>
        /// This method is used when calculating probability of an ngram sequence, how many times it backed
        /// off to lower order n-gram calculations.
        /// </summary>
        /// <param name="tokens">n-gram strings</param>
        /// <returns>if no back-off, returns 0 if none of the n-grams exist (Except 1 gram), it returns
        /// order-1</returns>
        public int GetBackoffCount(params string[] tokens)
        {
            return GetBackoffCount(vocabulary.ToIndexes(tokens));
        }

        /// <summary>
        /// This method is used when calculating probability of an ngram sequence, how many times it backed
        /// off to lower order n-gram calculations.
        /// </summary>
        /// <param name="wordIndexes">n-gram index array</param>
        /// <returns>if no back-off, returns 0 if none of the n-grams exist (Except 1 gram), it returns
        /// order-1</returns>
        public int GetBackoffCount(params int[] wordIndexes)
        {
            int n = wordIndexes.Length;
            if (n == 0 || n > order)
            {
                throw new ArgumentException(
                    "At least one or " + order + " tokens are required. But it is:" + wordIndexes.Length);
            }
            if (n == 1)
            {
                return 0;
            }
            if (n == 2)
            {
                return GetProbabilityValue(wordIndexes) == LogMath.LogZero ? 1 : 0;
            }

            int begin = 0;
            int backoffCount = 0;
            int gram = n;
            while (gram > 1)
            {
                // try to find P(N|begin..N-1)
                int fingerPrint = MultiLevelMphf.Hash(wordIndexes, begin, n, -1);
                int nGramIndex = mphfs[gram].Get(wordIndexes, begin, n, fingerPrint);
                if (!ngramData[gram]
                    .CheckFingerPrint(fingerPrint, nGramIndex))
                { //  back off to B(begin..N-1)
                    backoffCount++;
                }
                else
                {
                    return backoffCount;
                }
                begin++;
                gram = n - begin;
            }
            return backoffCount;
        }

        /// <summary>
        /// It generates a single line String that explains the probability calculations.
        /// </summary>
        /// <param name="wordIndexes">n-gram index array</param>
        /// <returns>explanation String.</returns>
        public string Explain(params int[] wordIndexes)
        {
            return Explain(new Explanation(), wordIndexes).sb.ToString();
        }

        private Explanation Explain(Explanation exp, params int[] wordIndexes)
        {
            double probability = GetProbabilityValue(wordIndexes);
            exp.sb.Append(GetProbabilityExpression(wordIndexes));
            if (probability == LogMath.LogZero)
            { // if probability does not exist.
                exp.sb.Append("=[");
                double backOffValue = GetBackoffValue(Head(wordIndexes));
                String backOffStr = GetBackoffExpression(Head(wordIndexes));
                exp.sb.Append(backOffStr).Append("=").Append(Format(backOffValue)).Append(" + ");
                exp.score = exp.score + backOffValue + Explain(exp, Tail(wordIndexes)).score;
                exp.sb.Append("]");
            }
            else
            {
                exp.score = probability;
            }
            exp.sb.Append("= ").Append(Format(exp.score));
            return exp;
        }

        private string Format(double d)
        {
            return string.Format("{0:F3}", d);
        }

        /// <summary>
        /// Changes the log base. Generally probabilities in language models are log10 based. But some
        /// applications uses their own log base. Such as Sphinx uses 1.0003 as their base, some other uses
        /// e
        /// </summary>
        /// <param name="newBase">new logBase</param>
        private void ChangeLogBase(double newBase)
        {
            FloatLookup.ChangeBase(unigramProbs, logBase, newBase);
            FloatLookup.ChangeBase(unigramBackoffs, logBase, newBase);
            for (int i = 2; i < probabilityLookups.Length; i++)
            {
                probabilityLookups[i].ChangeBase(logBase, newBase);
                if (i < probabilityLookups.Length - 1)
                {
                    backoffLookups[i].ChangeBase(logBase, newBase);
                }
            }
            this.logBase = (float)newBase;
        }

        /// <summary>
        /// This method applies more smoothing to unigram log probability values. Some ASR engines does
        /// this.
        /// </summary>
        /// <param name="unigramWeight">weight factor.</param>
        private void ApplyUnigramSmoothing(double unigramWeight)
        {
            double logUnigramWeigth = Math.Log(unigramWeight);
            double inverseLogUnigramWeigth = Math.Log(1 - unigramWeight);
            double logUniformUnigramProbability = -Math.Log(unigramProbs.Length);
            // apply uni-gram weight. This applies smoothing to uni-grams. As lowering high probabilities and
            // adding gain to small probabilities.
            // uw = uni-gram weight  , uniformProb = 1/#unigram
            // so in linear domain, we apply this to all probability values as: p(w1)*uw + uniformProb * (1-uw) to
            // maintain the probability total to one while smoothing the values.
            // this converts to log(p(w1)*uw + uniformProb*(1-uw)) which is calculated with log probabilities
            // a = log(p(w1)) + log(uw) and b = -log(#unigram)+log(1-uw) applying logSum(a,b)
            // approach is taken from Sphinx-4
            for (int i = 0; i < unigramProbs.Length; i++)
            {
                double p1 = unigramProbs[i] + logUnigramWeigth;
                double p2 = logUniformUnigramProbability + inverseLogUnigramWeigth;
                unigramProbs[i] = (float)LogMath.LogSum(p1, p2);
            }
        }

        /// <summary>
        /// Returns the logarithm base of the values used in this model.
        /// </summary>
        /// <returns>log base</returns>
        public double GetLogBase()
        {
            return logBase;
        }

        public enum MphfType
        {
            Small, Large
        }

        /// <summary>
        ///  Builder is used for instantiating the compressed language model. Default values: <p>Log Base =
        ///  e <p>Unknown backoff penalty = 0 <p>Default unigram weight = 1 <p>Use Stupid Backoff = false
        ///  <p>Stupid Backoff alpha value = 0.4
        /// </summary>
        public class SmoothLmBuilder
        {

            private float _logBase = DefaultLogBase;
            private float _unknownBackoffPenalty = DefaultUnknownBackoffPenalty;
            private float _unigramWeight = DefaultUnigramWeight;
            private bool _useStupidBackoff = false;
            private float _stupidBackoffAlpha = DefaultStupidBackoffAlpha;
            private BinaryReader _dis;
            private string _ngramIds;

            public SmoothLmBuilder(Stream stream)
            {
                this._dis = new BinaryReader(new BufferedStream(stream));
            }

            public SmoothLmBuilder(string file)
            {
                this._dis = new BinaryReader(new BufferedStream(File.OpenRead(file)));
            }

            public SmoothLmBuilder LogBase(double logBase)
            {
                this._logBase = (float)logBase;
                return this;
            }

            public SmoothLmBuilder UnknownBackoffPenalty(double unknownPenalty)
            {
                this._unknownBackoffPenalty = (float)unknownPenalty;
                return this;
            }

            public SmoothLmBuilder NgramKeyFilesDirectory(string dir)
            {
                this._ngramIds = dir;
                return this;
            }

            public SmoothLmBuilder UnigramWeight(double weight)
            {
                this._unigramWeight = (float)weight;
                return this;
            }

            public SmoothLmBuilder UseStupidBackoff()
            {
                this._useStupidBackoff = true;
                return this;
            }

            public SmoothLmBuilder UseStupidBackoff(bool useStupidBackoff)
            {
                this._useStupidBackoff = useStupidBackoff;
                return this;
            }

            public SmoothLmBuilder StupidBackoffAlpha(double alphaValue)
            {
                this._stupidBackoffAlpha = (float)alphaValue;
                return this;
            }

            public SmoothLm Build()
            {
                return new SmoothLm(
                    _dis,
                    _logBase,
                    _unigramWeight,
                    _unknownBackoffPenalty,
                    _useStupidBackoff,
                    _stupidBackoffAlpha,
                    _ngramIds);
            }
        }

        private class Explanation
        {
            internal StringBuilder sb = new StringBuilder();
            internal double score = 0;
        }

        /// <summary>
        /// This class contains actual n-gram key information in flat arrays.This is only useful for
        /// debugging purposes to check the false-positive ration of the compressed LM It has a limitation
        /// that for an order, key_count* order value must be lower than Integer.MAX_VALUE.Otherwise it
        /// does not load the information.
        /// </summary>
        class NgramIds
        {
            // flat arrays carrying actual ngram information.
            internal int[][] ids;

            internal NgramIds(int order, string idFileDir, IMphf[] mphfs)
            {
                ids = new int[order + 1][];
                for (int i = 2; i <= order; i++)
                {
                    // TODO: check consistency of the file names.
                    string idFile = Path.Combine(idFileDir, i + ".gram");
                    Log.Info("Loading from: " + idFile);
                    if (!File.Exists(idFile))
                    {
                        Log.Warn("Cannot find n-gram id file " + Path.GetFullPath(idFile));
                        continue;
                    }
                    using (FileStream fileStream = File.OpenRead(idFile))
                    {
                        using (BinaryReader dis = new BinaryReader(fileStream))
                        {
                            dis.ReadInt32(); // skip order.
                            int keyAmount = dis.ReadInt32().EnsureEndianness();
                            if ((long)keyAmount * i > int.MaxValue)
                            {
                                Log.Warn("Cannot load key file as flat array. Too much index values.");
                                continue;
                            }
                            ids[i] = new int[keyAmount * i];
                            int[] data = new int[i];
                            int k = 0;
                            while (k < keyAmount)
                            {
                                // load the k.th gram ids and calculate mphf for that.
                                for (int j = 0; j < i; ++j)
                                {
                                    data[j] = dis.ReadInt32().EnsureEndianness();
                                }
                                int mphfIndex = mphfs[i].Get(data);
                                // put data to flat array with mphfIndex val.
                                Array.Copy(data, 0, ids[i], mphfIndex * i, i);
                                k++;
                            }
                        }
                    }
                }
            }

            internal bool Exists(int[] indexes, int mphfIndex)
            {
                int order = indexes.Length;
                int index = mphfIndex * order;
                for (int i = 0; i < order; i++)
                {
                    if (ids[order][index + i] != indexes[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
