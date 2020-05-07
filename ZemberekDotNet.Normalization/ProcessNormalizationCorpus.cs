using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Morphology;

namespace ZemberekDotNet.Normalization
{
    public class ProcessNormalizationCorpus
    {
        public static readonly int BlockSize = 1_000_000;
        TurkishSentenceNormalizer normalizer;

        public ProcessNormalizationCorpus(TurkishSentenceNormalizer normalizer)
        {
            this.normalizer = normalizer;
        }

        public static void DummyMain(string[] args)
        {
            TurkishMorphology morphology = NormalizationVocabularyGenerator.GetTurkishMorphology();

            string normalizationDataRoot = "/home/aaa/data/normalization/test-large";
            string lmPath = "/home/aaa/data/normalization/lm.slm";

            TurkishSentenceNormalizer normalizationPreprocessor = new TurkishSentenceNormalizer(
                morphology, normalizationDataRoot, lmPath);

            ProcessNormalizationCorpus processor = new ProcessNormalizationCorpus(normalizationPreprocessor);

            string corporaRoot = "/home/aaa/data/corpora";
            string outRoot = "/home/aaa/data/normalization/corpus/clean";
            string rootList = Path.Combine(corporaRoot, "clean-list");

            Directory.CreateDirectory(outRoot);

            BlockTextLoader corpusProvider = BlockTextLoader
                .FromDirectoryRoot(corporaRoot, rootList, BlockSize);

            // create vocabularies
            int threadCount = Environment.ProcessorCount / 2;
            if (threadCount > 10)
            {
                threadCount = 10;
            }

            processor.Process(corpusProvider, threadCount, outRoot);
            Log.Info("Done.");

        }

        internal void Process(
            BlockTextLoader corpusProvider,
            int threadCount,
            string outRoot)
        {

            int c = 0;
            Parallel.ForEach(corpusProvider, new ParallelOptions
            {
                MaxDegreeOfParallelism = threadCount
            }, (TextChunk chunk) =>
            {
                List<string> sentences = TextCleaner.CleanAndExtractSentences(chunk.GetData());
                sentences = sentences
                            .Select(s => normalizer.PreProcess(s))
                            .ToList();
                string p = Path.Combine(outRoot, Interlocked.Increment(ref c).ToString());
                try
                {
                    File.WriteAllLines(p, sentences, Encoding.UTF8);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e);
                }
                Log.Info(c * BlockSize + " Lines processed.");
            });
        }
    }
}
