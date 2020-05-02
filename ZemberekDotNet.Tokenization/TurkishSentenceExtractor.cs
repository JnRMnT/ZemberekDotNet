using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.IO;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Core.Turkish;

namespace ZemberekDotNet.Tokenization
{
    /// <summary>
    /// This class is used for extracting sentences from paragraphs.For making boundary decisions it
    /// uses a combination of rules and a binary averaged perceptron model.It only breaks paragraphs
    /// from[.!?…] symbols. <p> Use the static DEFAULT singleton for the TurkishSentenceExtractor
    /// instance that uses the internal extraction model.
    /// </summary>
    public class TurkishSentenceExtractor : PerceptronSegmenter
    {
        /// <summary>
        /// A singleton instance that is generated from the default internal model.
        /// </summary>
        public static readonly TurkishSentenceExtractor Default = Singleton.Instance.Extractor;

        static readonly String BoundaryChars = ".!?…";
        private static readonly Regex LineBreakPattern = new Regex("[\n\r]+");
        private bool doNotSplitInDoubleQuotes = false;

        private TurkishSentenceExtractor(FloatValueMap<String> weights)
        {
            this.weights = weights;
        }

        private TurkishSentenceExtractor(FloatValueMap<String> weights,
            bool doNotSplitInDoubleQuotes)
        {
            this.weights = weights;
            this.doNotSplitInDoubleQuotes = doNotSplitInDoubleQuotes;
        }

        private static TurkishSentenceExtractor FromDefaultModel()
        {
            using (BinaryReader dis = IOUtil.GetDataInputStream("Resources/tokenization/sentence-boundary-model.bin"))
            {
                return new TurkishSentenceExtractor(Load(dis));
            }
        }

        public static TurkishSentenceExtractorBuilder Builder()
        {
            return new TurkishSentenceExtractorBuilder();
        }

        public class TurkishSentenceExtractorBuilder
        {

            bool _doNotSplitInDoubleQuotes = false;
            FloatValueMap<string> _model;

            public TurkishSentenceExtractorBuilder DoNotSplitInDoubleQuotes()
            {
                this._doNotSplitInDoubleQuotes = true;
                return this;
            }

            public TurkishSentenceExtractorBuilder UseModelFromResource(string resource)
            {
                return UseModelFromPath(resource);
            }

            public TurkishSentenceExtractorBuilder UseModelFromPath(string path)
            {
                using (BinaryReader dis = IOUtil.GetDataInputStream(path))
                {
                    this._model = Load(dis);
                }
                return this;
            }

            public TurkishSentenceExtractorBuilder UseDefaultModel()
            {
                string resource = "Resources/tokenization/sentence-boundary-model.bin";
                try
                {
                    UseModelFromResource(resource);
                }
                catch (IOException)
                {
                    throw new InvalidOperationException("Cannot find internal resource:" + resource);
                }
                return this;
            }

            public TurkishSentenceExtractor Build()
            {
                if (_model == null)
                {
                    UseDefaultModel();
                }
                return new TurkishSentenceExtractor(_model, _doNotSplitInDoubleQuotes);
            }
        }

        /// <summary>
        /// Extracts sentences from a list if paragraph strings. This method does not split from line
        /// breaks assuming paragraphs do not contain line breaks. <p> If content contains line breaks, use
        /// {@link #fromDocument(String)}
        /// </summary>
        /// <param name="paragraphs">a String List representing multiple paragraphs.</param>
        /// <returns>a list of String representing sentences.</returns>
        public List<string> FromParagraphs(ICollection<string> paragraphs)
        {
            List<string> result = new List<string>();
            foreach (string paragraph in paragraphs)
            {
                result.AddRange(FromParagraph(paragraph));
            }
            return result;
        }

        internal int[] BoundaryIndexes(string paragraph)
        {
            List<Span> spans = ExtractToSpans(paragraph);
            int[] indexes = new int[spans.Count * 2];
            int i = 0;
            foreach (Span span in spans)
            {
                indexes[i] = span.start;
                indexes[i + 1] = span.end;
                i += 2;
            }
            return indexes;
        }

        // TODO: doNotSplitInDoubleQuotes may not be suitable for some cases.
        // such as for paragraph: "Merhaba. Nasılsın?"
        private List<Span> ExtractToSpans(string paragraph)
        {
            List<Span> spans = new List<Span>();
            List<Span> quoteSpans = null;
            if (doNotSplitInDoubleQuotes)
            {
                quoteSpans = DoubleQuoteSpans(paragraph);
            }
            int begin = 0;
            for (int j = 0; j < paragraph.Length; j++)
            {

                // skip if char cannot be a boundary char.
                char chr = paragraph[j];
                if (BoundaryChars.IndexOf(chr) < 0)
                {
                    continue;
                }

                // skip if breaking is not allowed between double quotes.
                if (doNotSplitInDoubleQuotes && quoteSpans != null && InSpan(j, quoteSpans))
                {
                    continue;
                }

                BoundaryData boundaryData = new BoundaryData(paragraph, j);
                if (boundaryData.NonBoundaryCheck())
                {
                    continue;
                }
                List<string> features = boundaryData.ExtractFeatures();
                double score = 0;
                foreach (string feature in features)
                {
                    score += weights.Get(feature);
                }
                if (score > 0)
                {
                    Span span = new Span(begin, j + 1);
                    if (span.Length() > 0)
                    {
                        spans.Add(span);
                    }
                    begin = j + 1;
                }
            }

            if (begin < paragraph.Length)
            {
                Span span = new Span(begin, paragraph.Length);
                if (span.Length() > 0)
                {
                    spans.Add(span);
                }
            }
            return spans;
        }

        private static readonly string doubleQuotes = "\"”“»«";

        /// <summary>
        /// Finds double quote spans.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private List<Span> DoubleQuoteSpans(string input)
        {
            List<Span> spans = new List<Span>();

            int start = -1;
            bool started = false;
            for (int j = 0; j < input.Length; j++)
            {
                char c = input[j];
                if (doubleQuotes.IndexOf(c) >= 0)
                {
                    if (!started)
                    {
                        start = j;
                        started = true;
                    }
                    else
                    {
                        spans.Add(new Span(start, j));
                        started = false;
                    }
                }
            }
            return spans;
        }

        private bool InSpan(int index, List<Span> spans)
        {
            foreach (Span span in spans)
            {
                if (span.start > index)
                {
                    return false;
                }
                if (span.InSpan(index))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Extracts sentences from a paragraph string. This method does not split from line breaks
        /// assuming paragraphs do not contain line breaks. <p> If content contains line breaks, use {@link
        /// #fromDocument(String)}
        /// </summary>
        /// <param name="paragraph">a String representing a paragraph of text.</param>
        /// <returns>a list of String representing sentences.</returns>
        public List<string> FromParagraph(string paragraph)
        {
            List<Span> spans = ExtractToSpans(paragraph);
            List<string> sentences = new List<string>(spans.Count);
            foreach (Span span in spans)
            {
                String sentence = span.GetSubstring(paragraph).Trim();
                if (sentence.Length > 0)
                {
                    sentences.Add(sentence);
                }
            }
            return sentences;
        }

        /// <summary>
        /// Extracts sentences from a string that represents a document text. This method first splits the
        /// String from line breaks to paragraphs. After that it calls {@link #fromParagraphs(Collection)}
        /// for extracting sentences from multiple paragraphs.
        /// </summary>
        /// <param name="document">a String List representing a complete document's text content.</param>
        /// <returns>a list of String representing sentences.</returns>
        public List<string> FromDocument(string document)
        {
            List<string> lines = LineBreakPattern.Split(document).ToList();
            return FromParagraphs(lines);
        }

        public char[] GetBoundaryCharacters()
        {
            return BoundaryChars.ToCharArray();
        }

        internal class Singleton
        {
            internal static Singleton Instance = new Singleton();
            internal TurkishSentenceExtractor Extractor;

            Singleton()
            {
                try
                {
                    Extractor = FromDefaultModel();
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e);
                }
            }
        }

        public class TrainerBuilder
        {
            internal string trainFile;
            internal int iterationCount = 20;
            internal int skipSpaceFrequency = 20;
            internal float learningRate = 0.1f;
            internal int lowerCaseFirstLetterFrequency = 20;
            internal bool shuffleInput = false;

            public TrainerBuilder(string trainFile)
            {
                this.trainFile = trainFile;
            }

            public TrainerBuilder IterationCount(int count)
            {
                this.iterationCount = count;
                return this;
            }

            public TrainerBuilder ShuffleSentences()
            {
                this.shuffleInput = true;
                return this;
            }

            /// <summary>
            /// In every [count] sentence, trainer skips the space after sentence boundary punctuation.
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            public TrainerBuilder SkipSpaceFrequency(int count)
            {
                this.skipSpaceFrequency = count;
                return this;
            }

            public TrainerBuilder LearningRate(float learningRate)
            {
                this.learningRate = learningRate;
                return this;
            }

            /// <summary>
            /// In every [count] sentence, trainer lower cases the character after sentence boundary
            /// punctuation.
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            public TrainerBuilder LowerCaseFirstLetterFrequency(int count)
            {
                this.lowerCaseFirstLetterFrequency = count;
                return this;
            }

            public Trainer Build()
            {
                return new Trainer(this);
            }
        }

        public class Trainer
        {
            private static CultureInfo Turkish = CultureInfo.GetCultureInfo("tr");
            private TrainerBuilder builder;

            internal Trainer(TrainerBuilder builder)
            {
                this.builder = builder;
            }

            public static TrainerBuilder Builder(string trainFile)
            {
                return new TrainerBuilder(trainFile);
            }

            public TurkishSentenceExtractor Train()
            {
                FloatValueMap<string> weights = new FloatValueMap<string>();
                List<string> sentences = TextIO.LoadLines(builder.trainFile);
                FloatValueMap<string> averages = new FloatValueMap<string>();

                Random rnd = new Random(1);

                int updateCount = 0;

                for (int i = 0; i < builder.iterationCount; i++)
                {
                    Log.Info("Iteration = %d", i + 1);

                    UIntSet indexSet = new UIntSet();
                    StringBuilder sb = new StringBuilder();
                    int boundaryIndexCounter;
                    int sentenceCounter = 0;
                    if (builder.shuffleInput)
                    {
                        sentences.Shuffle();
                    }
                    foreach (string sentence in sentences)
                    {
                        string sentenceToAppend = sentence;
                        if (sentence.Trim().Length == 0)
                        {
                            continue;
                        }
                        // sometimes make first letter of the sentence lower case.
                        if (rnd.Next(builder.lowerCaseFirstLetterFrequency) == 0)
                        {
                            sentenceToAppend = sentence.Substring(0, 1).ToLower(Turkish) + sentenceToAppend.Substring(1);
                        }
                        sb.Append(sentenceToAppend);
                        boundaryIndexCounter = sb.Length - 1;
                        indexSet.Add(boundaryIndexCounter);
                        // in some sentences skip adding a space between sentences.
                        if (rnd.Next(builder.skipSpaceFrequency) != 1
                            && sentenceCounter < sentences.Count - 1)
                        {
                            sb.Append(" ");
                        }
                        sentenceCounter++;
                    }
                    String joinedSentence = sb.ToString();

                    for (int j = 0; j < joinedSentence.Length; j++)
                    {
                        // skip if char cannot be a boundary char.
                        char chr = joinedSentence[j];
                        if (BoundaryChars.IndexOf(chr) < 0)
                        {
                            continue;
                        }
                        BoundaryData boundaryData = new BoundaryData(joinedSentence, j);
                        if (boundaryData.NonBoundaryCheck())
                        {
                            continue;
                        }
                        List<string> features = boundaryData.ExtractFeatures();
                        float score = 0;
                        foreach (string feature in features)
                        {
                            score += weights.Get(feature);
                        }
                        float update = 0;
                        // if we found no-boundary but it is a boundary
                        if (score <= 0 && indexSet.Contains(j))
                        {
                            update = builder.learningRate;
                        }
                        // if we found boundary but it is not a boundary
                        else if (score > 0 && !indexSet.Contains(j))
                        {
                            update = -builder.learningRate;
                        }
                        updateCount++;
                        if (update != 0)
                        {
                            foreach (string feature in features)
                            {
                                double d = weights.IncrementByAmount(feature, update);
                                if (d == 0.0)
                                {
                                    weights.Remove(feature);
                                }
                                d = averages.IncrementByAmount(feature, updateCount * update);
                                if (d == 0.0)
                                {
                                    averages.Remove(feature);
                                }
                            }
                        }
                    }
                }
                foreach (string key in weights)
                {
                    weights.Set(key, weights.Get(key) - averages.Get(key) * 1f / updateCount);
                }

                return new TurkishSentenceExtractor(weights);
            }
        }

        internal class BoundaryData
        {
            internal char currentChar;
            internal char previousLetter;
            internal char nextLetter;
            internal string previousTwoLetters;
            internal string nextTwoLetters;
            internal string currentWord;
            internal string nextWord;
            internal string currentWordNoPunctuation;
            internal string rightChunk;
            internal string rightChunkUntilBoundary;
            internal string leftChunk;
            internal string leftChunkUntilBoundary;

            internal BoundaryData(string input, int pointer)
            {

                previousLetter = pointer > 0 ? input[pointer - 1] : '_';
                nextLetter = pointer < input.Length - 1 ? nextLetter = input[pointer + 1] : '_';
                previousTwoLetters = pointer > 2 ? input.Substring(pointer - 2, 2) : "__";
                nextTwoLetters =
                    pointer < input.Length - 3 ? input.Substring(pointer + 1, 2) : "__";

                int previousSpace = FindBackwardsSpace(input, pointer);
                if (previousSpace < 0)
                {
                    previousSpace = 0;
                }
                leftChunk = input.Substring(previousSpace, pointer - previousSpace);

                int previousBoundaryOrSpace = FindBackwardsSpaceOrChar(input, pointer, '.');
                if (previousBoundaryOrSpace < 0)
                {
                    previousBoundaryOrSpace = 0;
                }
                leftChunkUntilBoundary = previousBoundaryOrSpace == previousSpace ?
                    leftChunk : input.Substring(previousBoundaryOrSpace, pointer - previousBoundaryOrSpace);

                int nextSpace = FindForwardsSpace(input, pointer);
                rightChunk = "";
                if (pointer < input.Length - 1)
                {
                    rightChunk = input.Substring(pointer + 1, nextSpace - pointer - 1);
                }
                int nextBoundaryOrSpace = FindForwardsSpaceOrChar(input, pointer, '.');
                rightChunkUntilBoundary = nextSpace == nextBoundaryOrSpace ?
                    rightChunk : input.Substring(pointer + 1, nextBoundaryOrSpace - pointer - 1);

                currentChar = input[pointer];
                currentWord = leftChunk + currentChar + rightChunk;
                currentWordNoPunctuation = Regex.Replace(currentWord, "[.!?…]", "");

                StringBuilder sb = new StringBuilder();
                int j = nextSpace;
                while (j < input.Length)
                {
                    char c = input[j];
                    if (c == ' ')
                    {
                        break;
                    }
                    sb.Append(c);
                    j++;
                }
                nextWord = sb.ToString();
            }

            internal int FindBackwardsSpace(string input, int pos)
            {
                return FindBackwardsSpaceOrChar(input, pos, ' ');
            }

            internal int FindBackwardsSpaceOrChar(string input, int pos, char chr)
            {
                int i = pos - 1;
                while (i >= 0)
                {
                    char c = input[i];
                    if (c == ' ' || c == chr)
                    {
                        i++;
                        break;
                    }
                    i--;
                }
                return i;
            }

            internal int FindForwardsSpace(string input, int pos)
            {
                return FindForwardsSpaceOrChar(input, pos, ' ');
            }

            internal int FindForwardsSpaceOrChar(string input, int pos, char chr)
            {
                int j = pos + 1;
                while (j < input.Length)
                {
                    char c = input[j];
                    if (c == ' ' || c == chr)
                    {
                        break;
                    }
                    j++;
                }
                return j;
            }

            internal bool NonBoundaryCheck()
            {
                return (leftChunkUntilBoundary.Length == 1)
                    || nextLetter == '\''
                    || BoundaryChars.IndexOf(nextLetter) >= 0
                    || TurkishAbbreviationSet.Contains(currentWord)
                    || TurkishAbbreviationSet.Contains(leftChunkUntilBoundary)
                    || PotentialWebSite(currentWord);
            }

            internal List<string> ExtractFeatures()
            {
                List<string> features = new List<string>();
                features.Add("1:" + char.IsUpper(previousLetter).ToString().ToLowerInvariant());
                features.Add("1b:" + char.IsWhiteSpace(nextLetter).ToString().ToLowerInvariant());
                features.Add("1a:" + previousLetter);
                features.Add("1b:" + nextLetter);
                features.Add("2p:" + previousTwoLetters);
                features.Add("2n:" + nextTwoLetters);

                if (currentWord.Length > 0)
                {
                    features.Add("7c:" + char.IsUpper(currentWord[0]).ToString().ToLowerInvariant());
                    features.Add("9c:" + GetMetaChars(currentWord).ToString().ToLowerInvariant());
                }

                if (rightChunk.Length > 0)
                {
                    features.Add("7r:" + char.IsUpper(rightChunk[0]).ToString().ToLowerInvariant());
                    features.Add("9r:" + GetMetaChars(rightChunk));
                    if (!TurkishAlphabet.Instance.ContainsVowel(rightChunk))
                    {
                        features.Add("rcc:true");
                    }
                }
                if (nextWord.Length > 0)
                {
                    features.Add("7n:" + char.IsUpper(nextWord[0]).ToString().ToLowerInvariant());
                    features.Add("9n:" + GetMetaChars(nextWord));
                }

                if (leftChunk.Length > 0)
                {
                    if (!TurkishAlphabet.Instance.ContainsVowel(leftChunk))
                    {
                        features.Add("lcc:true");
                    }
                }

                if (currentWordNoPunctuation.Length > 0)
                {
                    bool allUp = true;
                    bool allDigit = true;
                    for (int j = 0; j < currentWordNoPunctuation.Length; j++)
                    {
                        char c = currentWordNoPunctuation[j];
                        if (!char.IsUpper(c))
                        {
                            allUp = false;
                        }
                        if (!char.IsDigit(c))
                        {
                            allDigit = false;
                        }
                    }

                    if (allUp)
                    {
                        features.Add("11u:true");
                    }
                    if (allDigit)
                    {
                        features.Add("11d:true");
                    }
                }
                return features;
            }
        }

    }
}
