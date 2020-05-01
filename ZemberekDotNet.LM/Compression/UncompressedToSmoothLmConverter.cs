using System;
using System.Collections;
using System.IO;
using ZemberekDotNet.Core.Hash;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Quantization;

namespace ZemberekDotNet.LM.Compression
{
    public class UncompressedToSmoothLmConverter
    {
        private static readonly int VERSION = 1;
        string lmFile;
        string tempDir;

        int order;

        public UncompressedToSmoothLmConverter(string lmFile, string tempDir)
        {
            this.lmFile = lmFile;
            this.tempDir = tempDir;
        }

        public void ConvertSmall(string binaryUncompressedLmDir, NgramDataBlock block)
        {
            Convert(binaryUncompressedLmDir, block, SmoothLm.MphfType.Small, null, -1);
        }

        public void ConvertLarge(string binaryUncompressedLmDir, NgramDataBlock block, int chunkBits)

        {
            Convert(binaryUncompressedLmDir, block, SmoothLm.MphfType.Large, null, chunkBits);
        }

        public void ConvertLarge(string binaryUncompressedLmDir, NgramDataBlock block,
            string[] oneBasedMphfstrings, int chunkBits)
        {
            Convert(binaryUncompressedLmDir, block, SmoothLm.MphfType.Large, oneBasedMphfstrings, chunkBits);
        }

        private void Convert(string binaryUncompressedLmDir,
            NgramDataBlock block,
            SmoothLm.MphfType type,
            string[] oneBasedMphfstrings,
            int chunkBits)
        {

            Log.Info("Generating compressed language model.");

            MultiFileUncompressedLm lm = new MultiFileUncompressedLm(binaryUncompressedLmDir);

            lm.GenerateRankFiles(block.probabilitySize * 8, QuantizerType.Binning);

            order = lm.order;

            BinaryWriter dos = new BinaryWriter(new FileStream(lmFile, FileMode.Create));

            // generate Minimal Perfect Hash functions for 2,3...n grams and save them as separate files.

            string[] phfstrings = new string[order + 1];
            if (oneBasedMphfstrings != null)
            {
                phfstrings = oneBasedMphfstrings;
            }
            else
            {
                for (int i = 2; i <= order; i++)
                {
                    IMphf mphf;
                    if (type == SmoothLm.MphfType.Large)
                    {
                        mphf = LargeNgramMphf.Generate(lm.GetGramFile(i), chunkBits);
                    }
                    else
                    {
                        mphf = MultiLevelMphf.Generate(lm.GetGramFile(i));
                    }
                    Log.Info("MPHF is generated for order %d with %d keys. Average bits per key: %.3f",
                        i,
                        mphf.Size(),
                        mphf.AverageBitsPerKey());
                    string mphfFile = Path.Combine(tempDir, Path.GetFileName(lmFile) + i + "gram.mphf");
                    phfstrings[i] = mphfFile;
                    using (FileStream fileStream = new FileStream(mphfFile, FileMode.Create, FileAccess.Write))
                    {
                        mphf.Serialize(fileStream);
                    }
                }
            }
            // generate header.
            Log.Info("Writing header");
            // write version and type info
            dos.Write(VERSION.EnsureEndianness());

            // write Mphf type
            if (type == SmoothLm.MphfType.Small)
            {
                dos.Write(0.EnsureEndianness());
            }
            else
            {
                dos.Write(1.EnsureEndianness());
            }

            // write log-base
            dos.Write(10d.EnsureEndianness());
            // write n value for grams (3 for trigram model)
            dos.Write(order.EnsureEndianness());
            // write counts, generate gramdata

            for (int i = 1; i <= order; i++)
            {
                dos.Write(lm.GetCount(i).EnsureEndianness());
            }

            // write rank lookup data (contains size+doubles)
            for (int i = 1; i <= order; i++)
            {
                dos.Write(File.ReadAllBytes(lm.GetProbabilityLookupFile(i)));
            }
            for (int i = 1; i <= order; i++)
            {
                if (i < order)
                {
                    dos.Write(File.ReadAllBytes(lm.GetBackoffLookupFile(i)));
                }
            }

            Log.Info("Reordering probability data and saving it together with n-gram fingerprints");
            for (int i = 1; i <= order; i++)
            {
                InMemoryBigByteArray probData = new InMemoryBigByteArray(lm.GetProbRankFile(i));
                InMemoryBigByteArray backoffData = null;

                if (i < order)
                {
                    backoffData = new InMemoryBigByteArray(lm.GetBackoffRankFile(i));
                }

                ReorderData reorderData;
                int gramCount = probData.count;
                if (i == 1)
                {
                    int[] reorderedIndexes = new int[gramCount];
                    for (int j = 0; j < gramCount; j++)
                    {
                        reorderedIndexes[j] = j;
                    }
                    reorderData = new ReorderData(reorderedIndexes, new int[0]);
                }
                else
                {
                    if (type == SmoothLm.MphfType.Large)
                    {
                        reorderData = ReorderIndexes(block, lm, i, LargeNgramMphf.Deserialize(phfstrings[i]));
                    }
                    else
                    {
                        reorderData = ReorderIndexes(block, lm, i, MultiLevelMphf.Deserialize(phfstrings[i]));
                    }
                }
                Log.Info("Validating reordered index array for order: %d", i);

                ValidateIndexArray(reorderData.reorderedKeyIndexes);

                int fingerPrintSize = block.fingerPrintSize;
                if (i == 1)
                {
                    fingerPrintSize = 0;
                }
                int backOffSize = block.backoffSize;
                if (i == order)
                {
                    backOffSize = 0;
                }

                dos.Write(gramCount.EnsureEndianness());
                dos.Write(fingerPrintSize.EnsureEndianness());
                dos.Write(block.probabilitySize.EnsureEndianness());
                dos.Write(backOffSize.EnsureEndianness());

                byte[] probBuff = new byte[block.probabilitySize];
                byte[] fpBuff = new byte[fingerPrintSize];
                byte[] backoffBuff = new byte[backOffSize];

                for (int k = 0; k < gramCount; k++)
                {
                    // save fingerprint values for 2,3,.. grams.
                    if (i > 1)
                    {
                        block.FingerprintAsBytes(reorderData.fingerprints[k], fpBuff);
                        dos.Write(fpBuff);
                    }
                    probData.Get(reorderData.reorderedKeyIndexes[k], probBuff);
                    dos.Write(probBuff);
                    // write backoff value if exists.
                    if (backoffData != null)
                    {
                        backoffData.Get(reorderData.reorderedKeyIndexes[k], backoffBuff);
                        dos.Write(backoffBuff);
                    }
                }
            }

            // append size of the Perfect hash and its content.
            if (phfstrings.Length > 0)
            {
                Log.Info("Saving MPHF values.");
            }

            for (int i = 2; i <= order; i++)
            {
                dos.Write(File.ReadAllBytes(phfstrings[i]));
            }

            // save vocabulary
            Log.Info("Saving vocabulary.");
            dos.Write(lm.GetVocabularyFile());

            dos.Close();
        }

        private void ValidateIndexArray(int[] arr)
        {
            BitArray set = new BitArray(arr.Length);
            foreach (int i in arr)
            {
                if (i >= arr.Length || i < 0)
                {
                    throw new InvalidOperationException(
                        "array contains a value=" + i + " larger than the array size=" + arr.Length);
                }
                set.Set(i, true);
            }
            for (int i = 0; i < arr.Length; i++)
            {
                if (!set.Get(i))
                {
                    throw new InvalidOperationException("Not validated.");
                }
            }
        }

        /// <summary>
        /// This class does the following: suppose we have the keys as: [k0, k1, k2, k3, k4, k5] ->
        /// [0,1,2,3,4,5] their mphf values are however: k0=2, k1=5, k2=0, k3=4, k4=1, k5=3 So what we want
        /// is to have key indexes in their minimal perfect has index order. reordered keys indexes: [k2,
        /// k4, k0, k5, k3, k1] -> [2,4,0,5,3,1]
        /// </summary>
        /// <param name="block"></param>
        /// <param name="lm">multifile language language model.</param>
        /// <param name="_order">current order of language model</param>
        /// <param name="mphf">MPH function</param>
        /// <returns>reordered key indexes and those keys fingerprint values.</returns>
        private ReorderData ReorderIndexes(NgramDataBlock block, MultiFileUncompressedLm lm, int _order,
            IMphf mphf)
        {
            ChunkingNGramReader reader = new ChunkingNGramReader(lm.GetGramFile(_order), _order, 1000000);
            int[] reorderedIndexes = new int[lm.GetCount(_order)];
            int[] fingerPrints = new int[lm.GetCount(_order)];
            int counter = 0;
            foreach (IIntHashKeyProvider provider in reader)
            {
                for (int k = 0; k < provider.KeyAmount(); k++)
                {
                    int[] key = provider.GetKey(k);
                    int hashVal = mphf.Get(key);
                    reorderedIndexes[hashVal] = counter;
                    fingerPrints[hashVal] = block.Fingerprint(key);
                    counter++;
                }
            }
            return new ReorderData(reorderedIndexes, fingerPrints);
        }

        public class NgramDataBlock
        {
            internal int fingerPrintSize;
            internal int probabilitySize;
            internal int backoffSize;
            internal int fingerprintMask;

            public NgramDataBlock(int fingerPrintBits, int probabilityBits, int backoffBits)
            {
                if (fingerPrintBits % 8 != 0)
                {
                    throw new ArgumentException("FingerPrint bit size must be an order of 8");
                }
                if (probabilityBits % 8 != 0)
                {
                    throw new ArgumentException("Probability bit size must be an order of 8");
                }
                if (backoffBits % 8 != 0)
                {
                    throw new ArgumentException("Backoff bit size must be an order of 8");
                }
                this.fingerPrintSize = fingerPrintBits / 8;
                this.probabilitySize = probabilityBits / 8;
                this.backoffSize = backoffBits / 8;
                if (fingerPrintBits == 4)
                {
                    this.fingerprintMask = unchecked((int)0xffffffff);
                }
                else
                {
                    this.fingerprintMask = (1 << fingerPrintBits) - 1;
                }
            }

            internal int Fingerprint(int[] key)
            {
                return MultiLevelMphf.Hash(key, -1) & fingerprintMask;
            }

            internal void FingerprintAsBytes(int fingerprint, byte[] res)
            {
                int k = 0;
                switch (fingerPrintSize)
                {
                    case 1:
                        res[k] = (byte)(fingerprint & 0xff);
                        break;
                    case 2:
                        res[k] = (byte)((fingerprint >> 8) & 0xff);
                        res[k + 1] = (byte)(fingerprint & 0xff);
                        break;
                    case 3:
                        res[k] = (byte)((fingerprint >> 16) & 0xff);
                        res[k + 1] = (byte)((fingerprint >> 8) & 0xff);
                        res[k + 2] = (byte)(fingerprint & 0xff);
                        break;
                    case 4:
                        res[k] = (byte)((fingerprint >> 24) & 0xff);
                        res[k + 1] = (byte)((fingerprint >> 16) & 0xff);
                        res[k + 2] = (byte)((fingerprint >> 8) & 0xff);
                        res[k + 3] = (byte)(fingerprint & 0xff);
                        break;
                }
            }

            internal void FingerprintAsBytes(int[] key, byte[] res)
            {
                FingerprintAsBytes(MultiLevelMphf.Hash(key, -1) & fingerprintMask, res);
            }
        }

        private class ReorderData
        {
            internal int[] reorderedKeyIndexes;
            internal int[] fingerprints;

            internal ReorderData(int[] reorderedKeyIndexes, int[] fingerprints)
            {
                this.reorderedKeyIndexes = reorderedKeyIndexes;
                this.fingerprints = fingerprints;
            }
        }
    }
}
