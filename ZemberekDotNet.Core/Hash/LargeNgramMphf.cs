using System;
using System.IO;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.Hash
{
    /// <summary>
    /// This is a MPHF implementation suitable for very large key sets.
    /// </summary>
    public class LargeNgramMphf : IMphf
    {
        private static readonly int DefaultChunkSetInBits = 22;
        readonly int maxBitMask;
        readonly int bucketMask;
        readonly int pageShift;

        MultiLevelMphf[] mphfs;
        int[] offsets;

        public LargeNgramMphf(int maxBitMask, int bucketMask, int pageShift, MultiLevelMphf[] mphfs,
            int[] offsets)
        {
            this.maxBitMask = maxBitMask;
            this.bucketMask = bucketMask;
            this.pageShift = pageShift;
            this.mphfs = mphfs;
            this.offsets = offsets;
        }

        /**
         * Same as generate(File file, int chunkBits) but uses DEFAULT_CHUNK_SIZE_IN_BITS for chunk size.
         *
         * @param file binary key file
         * @return generated LargeNgramMphf
         */
        public static LargeNgramMphf Generate(string file)
        {
            return Generate(file, DefaultChunkSetInBits);
        }

        /**
         * Generates MPHF from a binary integer key file. File needs to be in this structure: int32 order
         * (how many integers each key) int32 amount of keys . Max is 2^31-1 int32... key1 int32... key2
         * <p/>
         * Keys in the file must be unique. During generation of PHF system does not check for uniqueness.
         * System does the following:
         * <p/>
         * it splits the total amount of keys to large chunks (2^23 ~ 8 million keys by default.) during
         * split operation, all keys are divided to a chunk file according to it's bucket index. this is
         * calculated with PHF algorithms generic hash function g() Key counts in each chunk may be
         * different. But they are generally close values. These count values are important Because they
         * are used during global PHF value. Values are stored in an array (offsets)
         * <p/>
         * After files chunks are generated, a PHF is calculated for each chunk. And they are stored in an
         * array
         *
         * @param file binary key file
         * @return LargeNgramMphf fro the keys in the file
         * @ If an error occurs during file access.
         */
        public static LargeNgramMphf Generate(string file, int chunkBits)
        {
            string temporaryDirectory = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            Directory.CreateDirectory(temporaryDirectory);
            Splitter splitter = new Splitter(file, temporaryDirectory, chunkBits);
            Log.Info("Gram count: " + splitter.gramCount);
            Log.Info("Segment count: " + splitter.pageCount);
            Log.Info("Average segment size: " + (1 << splitter.pageBit));
            Log.Info("Segmenting File...");
            splitter.Split();
            int bucketBits = splitter.pageBit - 2;
            if (bucketBits <= 0)
            {
                bucketBits = 1;
            }
            MultiLevelMphf[] mphfs = new MultiLevelMphf[splitter.pageCount];
            int[] offsets = new int[splitter.pageCount];
            int total = 0;
            for (int i = 0; i < splitter.pageCount; i++)
            {
                ByteGramProvider keySegment = splitter.GetKeySegment(i);
                Log.Debug("Segment key count: " + keySegment.KeyAmount());
                Log.Debug("Segment bucket ratio: " + ((double)keySegment.KeyAmount() / (1 << bucketBits)));
                total += keySegment.KeyAmount();
                MultiLevelMphf mphf = MultiLevelMphf.Generate(keySegment);
                Log.Info("MPHF is generated for segment %d with %d keys. Average bits per key: %.3f",
                          i,
                          mphf.Size(),
                          mphf.AverageBitsPerKey());
                mphfs[i] = mphf;
                if (i > 0)
                {
                    offsets[i] = offsets[i - 1] + mphfs[i - 1].Size();
                }
            }
            Log.Debug("Total processed keys:" + total);
            int maxMask = (1 << splitter.maxBit) - 1;
            int bucketMask = (1 << bucketBits) - 1;
            return new LargeNgramMphf(maxMask, bucketMask, splitter.pageShift, mphfs, offsets);
        }

        /**
         * A custom Deserializer.
         *
         * @param file file that contains serialized data.
         * @param skip amoutn to skip
         * @return a new FastMinimalPerfectHash object.
         * @ if an error occurs during file access.
         */
        public static LargeNgramMphf Deserialize(string file, long skip)
        {
            BinaryReader dis = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1000000));
            long actualSkip = dis.BaseStream.Seek(skip, SeekOrigin.Current);
            if (actualSkip != skip)
            {
                throw new IOException("Cannot skip necessary amount of bytes from stream:" + skip);
            }
            return Deserialize(dis);
        }

        /**
         * A custom Deserializer.
         *
         * @param file file that contains serialized data.
         * @return a new FastMinimalPerfectHash object.
         * @ if an error occurs during file access.
         */
        public static LargeNgramMphf Deserialize(string file)
        {
            return Deserialize(file, 0);
        }

        /**
         * A custom Deserializer. Look serialization method document for the format.
         *
         * @param dis DataInputStream that contains serialized data.
         * @return a new ChdPerfectHash object.
         * @ if an error occurs during stream access.
         */
        public static LargeNgramMphf Deserialize(BinaryReader dis)
        {
            int maxBitMask = dis.ReadInt32().EnsureEndianness();
            int bucketMask = dis.ReadInt32().EnsureEndianness();
            int pageShift = dis.ReadInt32().EnsureEndianness();
            int phfCount = dis.ReadInt32().EnsureEndianness();

            int[]
            offsets = new int[phfCount];
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = dis.ReadInt32().EnsureEndianness();
            }
            MultiLevelMphf[] hashes = new MultiLevelMphf[phfCount];
            for (int i = 0; i < offsets.Length; i++)
            {
                hashes[i] = MultiLevelMphf.Deserialize(dis);
            }
            return new LargeNgramMphf(maxBitMask, bucketMask, pageShift, hashes, offsets);
        }

        public int Get(int[] ngram)
        {
            int hash = MultiLevelMphf.Hash(ngram, -1);
            int pageIndex = (hash & maxBitMask) >> pageShift;
            return mphfs[pageIndex].Get(ngram, hash) + offsets[pageIndex];
        }

        public int Get(int[] ngram, int hash)
        {
            int pageIndex = (hash & maxBitMask) >> pageShift;
            return mphfs[pageIndex].Get(ngram, hash) + offsets[pageIndex];
        }

        public int Get(int g1, int g2, int g3, int hash)
        {
            int pageIndex = (hash & maxBitMask) >> pageShift;
            return mphfs[pageIndex].Get(g1, g2, g3, hash) + offsets[pageIndex];
        }

        public int Get(int g1, int g2, int hash)
        {
            int pageIndex = (hash & maxBitMask) >> pageShift;
            return mphfs[pageIndex].Get(g1, g2, hash) + offsets[pageIndex];
        }

        public int Get(string ngram)
        {
            int hash = MultiLevelMphf.Hash(ngram, -1);
            int pageIndex = (hash & maxBitMask) >> pageShift;
            return mphfs[pageIndex].Get(ngram, hash) + offsets[pageIndex];
        }

        public int Get(string ngram, int hash)
        {
            int pageIndex = (hash & maxBitMask) >> pageShift;
            return mphfs[pageIndex].Get(ngram, hash) + offsets[pageIndex];
        }

        public int Get(int[] ngram, int begin, int end, int hash)
        {
            int pageIndex = (hash & maxBitMask) >> pageShift;
            return mphfs[pageIndex].Get(ngram, begin, end, hash) + offsets[pageIndex];
        }

        /**
         * A custom serializer.
         *
         * @param file file to serialize data.
         * @ if an error occurs during file access.
         */
        public void Serialize(string file)
        {
            Serialize(new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write, 1000000));
        }


        public void Serialize(FileStream fs)
        {
            Serialize(new BinaryWriter(fs));
        }

        public void Serialize(BinaryWriter dos)
        {
            dos.Write(this.maxBitMask.EnsureEndianness());
            dos.Write(this.bucketMask.EnsureEndianness());
            dos.Write(this.pageShift.EnsureEndianness());
            dos.Write(this.mphfs.Length.EnsureEndianness());
            foreach (int offset in offsets)
            {
                dos.Write(offset.EnsureEndianness());
            }
            foreach (MultiLevelMphf mphf in mphfs)
            {
                mphf.Serialize(dos);
            }
        }

        public double AverageBitsPerKey()
        {
            double total = 0;
            foreach (MultiLevelMphf mphf in mphfs)
            {
                total = total + mphf.AverageBitsPerKey();
            }
            return mphfs.Length > 0 ? total / mphfs.Length : 0;
        }


        public int Size()
        {
            int size = 0;
            foreach (MultiLevelMphf mphf in mphfs)
            {
                size += mphf.Size();
            }
            return size;
        }

        private class Splitter
        {
            internal readonly int pageShift;
            internal readonly int pageMask;
            internal string gramFile;
            internal string[] files;
            internal int order;
            internal int gramCount;
            internal string tmpDir;
            internal int maxBit;
            internal int pageCount;
            internal int pageBit;

            public Splitter(string gramFile, string tmpdir, int pageBit)
            {
                this.gramFile = gramFile;
                using (FileStream fileStream = File.OpenRead(gramFile))
                {
                    using (BinaryReader raf = new BinaryReader(fileStream))
                    {
                        this.order = raf.ReadInt32().EnsureEndianness();
                        this.gramCount = raf.ReadInt32().EnsureEndianness();
                        raf.Close();
                        this.tmpDir = tmpdir;
                        this.pageBit = pageBit;

                        // maxbit ıs the x value where 2^(x-1)<gramCount<2^x
                        maxBit = GetBitCountHigher(gramCount);
                        if (maxBit == 0)
                        {
                            maxBit = 1;
                        }

                        // if the x is smaller than the default page bit, we use the x value for the page bit.
                        if (maxBit < this.pageBit)
                        {
                            this.pageBit = maxBit;
                            pageShift = maxBit;
                        }
                        else
                        {
                            pageShift = this.pageBit;
                        }

                        int pageLength = 1 << this.pageBit;
                        pageMask = pageLength - 1;
                        pageCount = 1 << (maxBit - this.pageBit);
                        files = new string[pageCount];
                    }
                }
            }

            internal int GetBitCountHigher(int num)
            {
                int i = 1;
                int c = 0;
                while (i < num)
                {
                    i *= 2;
                    c++;
                }
                return c;
            }

            public int KeyAmount()
            {
                return gramCount;
            }

            public void Split()
            {
                if (pageCount == 1)
                {
                    files[0] = gramFile;
                    return;
                }

                FileKeyWriter[] fileKeyWriters = new FileKeyWriter[pageCount];
                int[] counts = new int[pageCount];

                for (int i = 0; i < pageCount; i++)
                {
                    files[i] = Path.Combine(tmpDir, order + "-gramidfile" + i + ".batch");
                    fileKeyWriters[i] = new FileKeyWriter(files[i], order);
                }

                byte[] buffer = new byte[(1 << pageBit) * 4 * order];
                FileStream raf = File.OpenRead(gramFile);
                raf.Seek(8, SeekOrigin.Current);
                int actual;
                while ((actual = raf.Read(buffer)) > 0)
                {
                    if (actual % (order * 4) != 0)
                    {
                        throw new InvalidOperationException("Cannot read order*4 aligned bytes from:" + gramFile);
                    }

                    int[] gramIds = new int[order];
                    int p = 0;
                    int maxBitMask = (1 << maxBit) - 1;
                    for (int i = 0; i < actual; i += 4)
                    {
                        gramIds[p++] =
                            (buffer[i] & 0xff) << 24 | (buffer[i + 1] & 0xff) << 16 | (buffer[i + 2] & 0xff) << 8
                                | (buffer[i + 3] & 0xff);
                        if (p == order)
                        {
                            int hash = MultiLevelMphf.Hash(gramIds, -1) & maxBitMask;
                            int segmentId = hash >> pageShift;
                            fileKeyWriters[segmentId].Write(gramIds);
                            counts[segmentId]++;
                            p = 0;
                        }
                    }
                }
                raf.Close();
                int j = 0;
                foreach (FileKeyWriter writer in fileKeyWriters)
                {
                    writer.Dispose();
                    writer.ChangeCount(counts[j++]);
                }
            }

            public ByteGramProvider GetKeySegment(int segment)
            {
                return new ByteGramProvider(File.OpenRead(files[segment]));
            }
        }

        private class FileKeyWriter : IDisposable
        {
            BinaryWriter dos;
            string file;
            int order;

            internal FileKeyWriter(string file, int order)
            {
                this.file = file;
                dos = new BinaryWriter(new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1000000));
                dos.Write(order.EnsureEndianness());
                dos.Write(0.EnsureEndianness());
                this.order = order;
            }

            internal void Write(int[] key)
            {
                foreach (int i in key)
                {
                    dos.Write(i);
                }
            }

            internal void ChangeCount(int count)
            {
                using (FileStream fileStream = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Write))
                {
                    using (BinaryWriter rafw = new BinaryWriter(fileStream))
                    {
                        rafw.Write(order.EnsureEndianness());
                        rafw.Write(count.EnsureEndianness());
                    }
                }
            }

            public void Dispose()
            {
                dos.Close();
            }
        }
    }
}
