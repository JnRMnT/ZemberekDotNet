using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.Hash
{
    /// <summary>
    /// This is a Minimum Perfect Hash Function implementation(MPHF). A MPHF generates distinct integers
    /// for a unique set of n keys in the range of[0, ..., n - 1]
    /// <p/>
    /// MultiLevelMphf can be considered as an improvement over MOS Algorithm II of Fox, Heath, Chen, and
    /// Daoud's Practical minimal perfect hash `functions for large databases [CACM 1992] and MPHF
    /// structure defined in Belazzougui, Botelho and Dietzfelbinger's Hash, Displace and Compress (2009)
    /// papers.
    /// <p/>
    /// It is different in these aspects:
    /// <p/>
    /// MOS algorithm implementation requires two integer arrays.First one carries hash seed values for
    /// buckets with more than one keys. Second one contains MPHF values for 1 key buckets.
    /// <p/>
    /// MultiLevelMphf does not follow this. It uses byte arrays instead of int arrays for bucket hash
    /// seed values. If a bucket's keys fail to find a hash index value that maps to zero bit indexes
    /// after the index value 255, it stops trying and marks it as failed bucket. Keys in the failed
    /// buckets are applied to the same mechanism until no failed bucket is left recursively. System
    /// carries an extra int index array for next levels failed buckets.
    /// <p/>
    /// MOS implementation may fail to generate a MPHF. But MultiLevelMphf guarantees generation of
    /// MPHF.
    /// <p/>
    /// For same byte per key values, MultiLevelMPHF generates hash values orders of magnitude faster
    /// than MOS. </p> It is also different than Belazzougui et al.'s approach as it does not apply
    /// integer array compression. </p> MultiLevelMPHF typically uses around 3.1-3.2 bits memory per
    /// key.
    /// </summary>
    public class MultiLevelMphf : IMphf
    {
        public static readonly int HASH_MULTIPLIER = 16777619;
        public static readonly uint INITIAL_HASH_SEED = 0x811C9DC5;
        public static readonly int BIT_MASK_21 = (1 << 21) - 1;
        readonly HashIndexes[] hashLevelData;

        private MultiLevelMphf(HashIndexes[] hashLevelData)
        {
            this.hashLevelData = hashLevelData;
        }

        public static MultiLevelMphf Generate(IIntHashKeyProvider keyProvider)
        {
            BucketCalculator bc = new BucketCalculator(keyProvider);
            return new MultiLevelMphf(bc.Calculate());
        }

        public static MultiLevelMphf Generate(FileStream binaryKeyFile)
        {
            return Generate(new ByteGramProvider(binaryKeyFile));
        }

        public static int Hash(byte[] data, int seed)
        {
            int d = seed > 0 ? seed : (int)INITIAL_HASH_SEED;
            foreach (int a in data)
            {
                d = (d ^ a) * HASH_MULTIPLIER;
            }
            return d & 0x7fffffff;
        }

        public static int Hash(int[] data, int seed)
        {
            int d = seed > 0 ? seed : (int)INITIAL_HASH_SEED;
            foreach (int a in data)
            {
                d = ((d ^ a) * HASH_MULTIPLIER);
            }
            return d & 0x7fffffff;
        }

        public static int Hash(int d0, int d1, int d2, int seed)
        {
            int d = seed > 0 ? seed : (int)INITIAL_HASH_SEED;
            d = (d ^ d0) * HASH_MULTIPLIER;
            d = (d ^ d1) * HASH_MULTIPLIER;
            d = (d ^ d2) * HASH_MULTIPLIER;
            return d & 0x7fffffff;
        }

        public static int Hash(int d0, int d1, int seed)
        {
            uint d = seed > 0 ? (uint)seed : INITIAL_HASH_SEED;
            d = (uint)((d ^ d0) * HASH_MULTIPLIER);
            d = (uint)((d ^ d1) * HASH_MULTIPLIER);
            return (int)d & 0x7fffffff;
        }

        public static int Hash(String data, int seed)
        {
            int d = seed > 0 ? seed : (int)INITIAL_HASH_SEED;
            for (int i = 0; i < data.Length; i++)
            {
                d = (d ^ data[i]) * HASH_MULTIPLIER;
            }
            return d & 0x7fffffff;
        }

        /**
         * This hash assumes that a trigram or bigram value is embedded into a 64 bit long value.
         * Structure for order=3: [1bit empty][gram-3][gram-2][gram-1] Structure for order=2: [22bit
         * empty][gram-2][gram-1]
         *
         * @param gramData gram data
         * @param order order of grams. max-3
         * @param seed hash seed
         * @return hash value.
         */
        public static int Hash(long gramData, int order, int seed)
        {
            int d = seed > 0 ? seed : (int)INITIAL_HASH_SEED;
            for (int i = 0; i < order; i++)
            {
                int h = (int)(gramData & BIT_MASK_21);
                gramData = gramData >> 21;
                d = ((d ^ h) * HASH_MULTIPLIER);
            }
            return d & 0x7fffffff;
        }

        public static int Hash(int[] data, int begin, int end, int seed)
        {
            int d = seed > 0 ? seed : (int)INITIAL_HASH_SEED;
            for (int i = begin; i < end; i++)
            {
                d = (d ^ data[i]) * HASH_MULTIPLIER;
            }
            return d & 0x7fffffff;
        }

        /**
         * A custom deserializer.
         *
         * @param file file that contains serialized data.
         * @param skip amount to skip
         * @return a new FastMinimalPerfectHash object.
         * @throws IOException if an error occurs during file access.
         */
        public static MultiLevelMphf Deserialize(string file, long skip)
        {
            BinaryReader dis = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 100000));
            long actualSkip = dis.BaseStream.Seek(skip, SeekOrigin.Current);
            if (actualSkip != skip)
            {
                throw new IOException("Cannot skip necessary amount of bytes from stream:" + skip);
            }
            return Deserialize(dis);
        }

        /**
         * A custom deserializer.
         *
         * @param file file that contains serialized data.
         * @return a new FastMinimalPerfectHash object.
         * @throws IOException if an error occurs during file access.
         */
        public static MultiLevelMphf Deserialize(string file)
        {
            return Deserialize(file, 0);
        }

        /**
         * A custom deserializer. Look serialization method document for the format.
         *
         * @param dis DataInputStream that contains serialized data.
         * @return a new ChdPerfectHash object.
         * @throws IOException if an error occurs during stream access.
         */
        public static MultiLevelMphf Deserialize(BinaryReader dis)
        {
            int levelCount = dis.ReadInt32();
            HashIndexes[] indexes = new HashIndexes[levelCount];
            for (int i = 0; i < levelCount; i++)
            {
                int keycount = dis.ReadInt32();
                int bucketAmount = dis.ReadInt32();
                byte[] hashSeedValues = new byte[bucketAmount];
                dis.Read(hashSeedValues);
                int failedIndexesCount = dis.ReadInt32();
                int[] failedIndexes = new int[failedIndexesCount];
                for (int j = 0; j < failedIndexesCount; j++)
                {
                    failedIndexes[j] = dis.ReadInt32();
                }
                indexes[i] = new HashIndexes(keycount, bucketAmount, hashSeedValues, failedIndexes);
            }
            return new MultiLevelMphf(indexes);
        }

        public int Size()
        {
            return hashLevelData[0].keyAmount;
        }

        public int GetLevelCount()
        {
            return hashLevelData.Length;
        }

        /**
         * @param key int array representation of the key.
         * @return minimal perfect hash value for the given input. returning number is between
         * [0-keycount] keycount excluded.
         */
        public int Get(int[] key)
        {
            return Get(key, Hash(key, -1));
        }

        /**
         * @param key int array representation of the key.
         * @param initialHash sometimes initial hash value for MPHF calculation is already calculated. So
         * this value is used instead of re-calculation.
         * @return minimal perfect hash value for the given input. returning number is between
         * [0-keycount] keycount excluded.
         */
        public int Get(int[] key, int initialHash)
        {
            for (int i = 0; i < hashLevelData.Length; i++)
            {
                int seed = hashLevelData[i].GetSeed(initialHash);
                if (seed != 0)
                {
                    if (i == 0)
                    {
                        return Hash(key, seed) % hashLevelData[0].keyAmount;
                    }
                    else
                    {
                        return hashLevelData[i - 1].failedIndexes[Hash(key, seed) % hashLevelData[i].keyAmount];
                    }
                }
            }
            throw new InvalidOperationException("Cannot be here.");
        }

        public int Get(int k0, int k1, int k2, int initialHash)
        {
            for (int i = 0; i < hashLevelData.Length; i++)
            {
                int seed = hashLevelData[i].GetSeed(initialHash);
                if (seed != 0)
                {
                    if (i == 0)
                    {
                        return Hash(k0, k1, k2, seed) % hashLevelData[0].keyAmount;
                    }
                    else
                    {
                        return hashLevelData[i - 1].failedIndexes[Hash(k0, k1, k2, seed)
                            % hashLevelData[i].keyAmount];
                    }
                }
            }
            throw new InvalidOperationException("Cannot be here.");
        }

        public int Get(int k0, int k1, int initialHash)
        {
            for (int i = 0; i < hashLevelData.Length; i++)
            {
                int seed = hashLevelData[i].GetSeed(initialHash);
                if (seed != 0)
                {
                    if (i == 0)
                    {
                        return Hash(k0, k1, seed) % hashLevelData[0].keyAmount;
                    }
                    else
                    {
                        return hashLevelData[i - 1].failedIndexes[Hash(k0, k1, seed)
                            % hashLevelData[i].keyAmount];
                    }
                }
            }
            throw new InvalidOperationException("Cannot be here.");
        }

        /**
         * @param key byte array representation of the key.
         * @return minimal perfect hash value for the given input. returning number is between
         * [0-keycount] keycount excluded.
         */
        public int Get(byte[] key)
        {
            return Get(key, Hash(key, -1));
        }

        /**
         * @param key byte array representation of the key.
         * @param initialHash sometimes initial hash value for MPHF calculation is already calculated. So
         * this value is used instead of re-calculation.
         * @return minimal perfect hash value for the given input. returning number is between
         * [0-keycount] keycount excluded.
         */
        public int Get(byte[] key, int initialHash)
        {
            for (int i = 0; i < hashLevelData.Length; i++)
            {
                int seed = hashLevelData[i].GetSeed(initialHash);
                if (seed != 0)
                {
                    if (i == 0)
                    {
                        return Hash(key, seed) % hashLevelData[0].keyAmount;
                    }
                    else
                    {
                        return hashLevelData[i - 1].failedIndexes[Hash(key, seed) % hashLevelData[i].keyAmount];
                    }
                }
            }
            throw new InvalidOperationException("Cannot be here.");
        }

        /**
         * @param key the key.
         * @return minimal perfect hash value for the given input. returning number is between
         * [0-keycount) keycount excluded.
         */
        public int Get(String key)
        {
            return Get(key, Hash(key, -1));
        }

        /**
         * @param key the key.
         * @param initialHash sometimes initial hash value for MPHF calculation is already calculated. So
         * this value is used instead of re-calculation. This provides a small performance enhancement.
         * @return minimal perfect hash value for the given input. returning number is between
         * [0-keycount] keycount excluded.
         */
        public int Get(String key, int initialHash)
        {
            for (int i = 0; i < hashLevelData.Length; i++)
            {
                int seed = hashLevelData[i].GetSeed(initialHash);
                if (seed != 0)
                {
                    if (i == 0)
                    {
                        return Hash(key, seed) % hashLevelData[0].keyAmount;
                    }
                    else
                    {
                        return hashLevelData[i - 1].failedIndexes[Hash(key, seed) % hashLevelData[i].keyAmount];
                    }
                }
            }
            throw new InvalidOperationException("Cannot be here.");
        }

        /**
         * @param key int array representation of the key.
         * @param initialHash sometimes initial hash value for MPHF calculation is already calculated. So
         * this value is used instead of re-calculation. This provides a small performance enhancement.
         * @return minimal perfect hash value for the given input. returning number is between
         * [0-keycount] keycount excluded.
         */
        public int Get(int[] key, int begin, int end, int initialHash)
        {
            for (int i = 0; i < hashLevelData.Length; i++)
            {
                int seed = hashLevelData[i].GetSeed(initialHash);
                if (seed != 0)
                {
                    if (i == 0)
                    {
                        return Hash(key, begin, end, seed) % hashLevelData[0].keyAmount;
                    }
                    else
                    {
                        return hashLevelData[i - 1].failedIndexes[Hash(key, begin, end, seed)
                            % hashLevelData[i].keyAmount];
                    }
                }
            }
            throw new InvalidOperationException("Cannot be here.");
        }

        /**
         * @return total bytes used for this structure. This is an average number and it adds 12 bytes per
         * array as overhead
         */
        public long TotalBytesUsed()
        {
            long result = 12; // array overhead
            foreach (HashIndexes data in hashLevelData)
            {
                result += 12; // array overhead for failed buckets
                result += data.bucketHashSeedValues.Length;
                result += data.failedIndexes.Length * 4;
            }
            return result;
        }

        public double AverageBitsPerKey()
        {
            return ((double)TotalBytesUsed() * 8) / hashLevelData[0].keyAmount;
        }
        
        public void Serialize(string path)
        {
            BinaryWriter os = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, 1000000));
            Serialize(os);
        }


        public void Serialize(FileStream file)
        {
            BinaryWriter os = new BinaryWriter(file);
            Serialize(os);
        }

        /**
         * A custom serializer.
         * <p/>
         * <p/>int level count <p/> ---- level 0 <p/>int key count <p/>int bucket amount <p/>byte[] seed
         * values. Length = bucket count <p/>int failed indexes length <p/>int[] failed indexes <p/> ----
         * level 1 <p/>int key count <p/>int bucket amount <p/>byte[] seed values. Length = bucket count
         * <p/>int failed indexes length <p/>int[] failed indexes <p/> .... <p/> ---- level n <p/>int key
         * count <p/>int bucket amount <p/>byte[] seed values. Length = bucket count <p/>int 0
         *
         * @param os stream to serialize data.
         * @throws IOException if an error occurs during file access.
         */
        public void Serialize(BinaryWriter dos)
        {
            dos.Write(hashLevelData.Length);
            foreach (HashIndexes index in hashLevelData)
            {
                dos.Write(index.keyAmount);
                dos.Write(index.bucketAmount);
                dos.Write(index.bucketHashSeedValues);
                dos.Write(index.failedIndexes.Length);
                foreach (int i in index.failedIndexes)
                {
                    dos.Write(i);
                }
            }
        }
        
        private class HashIndexes
        {
            internal readonly int keyAmount;
            internal readonly int bucketAmount;
            internal readonly byte[] bucketHashSeedValues;
            internal readonly int[] failedIndexes;

            internal HashIndexes(int keyAmount, int bucketAmount, byte[] bucketHashSeedValues, int[] failedIndexes)
            {
                this.keyAmount = keyAmount;
                this.bucketAmount = bucketAmount;
                this.bucketHashSeedValues = bucketHashSeedValues;
                this.failedIndexes = failedIndexes;
            }

            internal int GetSeed(int fingerPrint)
            {
                return (bucketHashSeedValues[fingerPrint % bucketAmount]) & 0xff;
            }
        }

        private class BucketCalculator
        {
            private static readonly int HashSeedLimit = 255;
            internal IIntHashKeyProvider keyProvider;
            int keyAmount;
            double averageKeysPerBucket = 3.0;


            internal BucketCalculator(IIntHashKeyProvider keyProvider)
            {
                this.keyProvider = keyProvider;
            }

            public HashIndexes[] Calculate()
            {
                keyAmount = keyProvider.KeyAmount();

                int bucketAmount = (int)(keyAmount / averageKeysPerBucket) + 1;

                Bucket[] buckets = generateInitialBuckets(bucketAmount);

                // sort buckets larger to smaller.
                Array.Sort(buckets);

                List<HashIndexes> result = new List<HashIndexes>();

                CalculateIndexes(buckets, keyAmount, result);

                return result.ToArray();
            }

            private Bucket[] generateInitialBuckets(int bucketAmount)
            {

                // Generating buckets
                Bucket[] buckets = new Bucket[bucketAmount];
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = new Bucket(i);
                }

                for (int i = 0; i < keyAmount; i++)
                {
                    int[] key = keyProvider.GetKey(i);
                    int bucketIndex = Hash(key, -1) % bucketAmount;
                    buckets[bucketIndex].Add(i);
                }
                return buckets;
            }

            private void CalculateIndexes(Bucket[] buckets, int keyAmount, List<HashIndexes> indexes)
            {

                // generate a long bit vector with size of hash target size.
                LongBitVector bitVector = new LongBitVector(keyAmount, 100);
                bitVector.Add(keyAmount, false);

                byte[] hashSeedArray = new byte[buckets.Length];
                Array.Fill(hashSeedArray, (byte)0x01);

                // we need to collect failed buckets (A failed bucket such that we cannot find empty slots for all bucket keys
                // after 255 trials. )
                List<Bucket> failedBuckets = new List<Bucket>(buckets.Length / 20);

                // for each bucket, find a hash function that will map each key in it to an empty slot in bitVector.
                int bucketIndex = 0;

                foreach (Bucket bucket in buckets)
                {
                    if (bucket.keyIndexes.Length == 0) // because buckets are sorted, we can finish here.
                    {
                        break;
                    }
                    int l = 1;
                    bool loop = true;
                    while (loop)
                    {
                        int j = 0;
                        int[] slots = new int[bucket.keyIndexes.Length];
                        foreach (int keyIndex in bucket.keyIndexes)
                        {
                            int[] key = keyProvider.GetKey(keyIndex);
                            slots[j] = Hash(key, l) % keyAmount;
                            if (bitVector.Get(slots[j]))
                            {
                                break;
                            }
                            else
                            {
                                bitVector.Set(slots[j]);
                                j++;
                            }
                        }
                        // if we fail to place all items in the bucket to the bitvector"s empty slots
                        if (j < bucket.keyIndexes.Length)
                        {
                            // we reset the occupied slots from bitvector.
                            for (int k = 0; k < j; k++)
                            {
                                bitVector.Clear(slots[k]);
                            }
                            // We reached the HASH_SEED_LIMIT.
                            // We place a 0 for its hash index value to know later that bucket is left to secondary lookup.
                            if (l == HashSeedLimit)
                            {
                                failedBuckets.Add(bucket);
                                hashSeedArray[buckets[bucketIndex].id] = 0;
                                loop = false;
                            }

                        }
                        else
                        { // sweet. We have found empty slots in bit vector for all keys of the bucket.
                            hashSeedArray[buckets[bucketIndex].id] = (byte)(l & 0xff);
                            loop = false;
                        }
                        l++;
                    }
                    bucketIndex++;
                }

                if (failedBuckets.Count == 0)
                {
                    // we are done.
                    indexes.Add(new HashIndexes(keyAmount, buckets.Length, hashSeedArray, new int[0]));
                    return;
                }

                // we assign lower average per key per bucket after each iteration to avoid generation failure.
                if (averageKeysPerBucket > 1)
                {
                    averageKeysPerBucket--;
                }

                // start calculation for failed buckets.
                int failedKeyCount = 0;
                foreach (Bucket failedBucket in failedBuckets)
                {
                    failedKeyCount += failedBucket.keyIndexes.Length;
                }

                int failedBucketAmount = (int)(failedKeyCount / averageKeysPerBucket);
                Log.Debug("Failed key Count:%d ", failedKeyCount);


                // this is a worst case scenario. No empty slot find for any buckets and we are already using buckets where bucket Amount>=keyAmount
                // In this case we double the bucket size with the hope that it will have better bucket-key distribution.
                if (failedKeyCount == keyAmount && averageKeysPerBucket <= 1d)
                {
                    averageKeysPerBucket = averageKeysPerBucket / 2;
                    failedBucketAmount *= 2;
                }

                if (failedBucketAmount == 0)
                {
                    failedBucketAmount++;
                }

                // this time we generate item keyAmount of Buckets
                Bucket[] nextLevelBuckets = new Bucket[failedBucketAmount];
                for (int i = 0; i < failedBucketAmount; i++)
                {
                    nextLevelBuckets[i] = new Bucket(i);
                }

                // generate secondary buckets with item indexes.
                foreach (Bucket largeHashIndexBucket in failedBuckets)
                {
                    foreach (int itemIndex in largeHashIndexBucket.keyIndexes)
                    {
                        int[] key = keyProvider.GetKey(itemIndex);
                        int secondaryBucketIndex = Hash(key, -1) % failedBucketAmount;
                        nextLevelBuckets[secondaryBucketIndex].Add(itemIndex);
                    }
                }
                // sort buckets larger to smaller.
                Array.Sort(nextLevelBuckets);

                int[] failedHashValues;
                int currentLevel = indexes.Count;
                if (currentLevel == 0)
                {
                    // if we are in the first level  generate a bit vector with the size of zero indexes of the primary bit vector.
                    failedHashValues = bitVector.ZeroIntIndexes();
                }
                else
                {
                    failedHashValues = new int[failedKeyCount];
                    int k = 0;
                    for (int i = 0; i < bitVector.Size(); i++)
                    {
                        if (!bitVector.Get(i))
                        {
                            failedHashValues[k++] = indexes[currentLevel - 1].failedIndexes[i];
                        }
                    }
                }
                indexes.Add(new HashIndexes(keyAmount, buckets.Length, hashSeedArray, failedHashValues));

                // recurse for failed buckets.
                CalculateIndexes(nextLevelBuckets, failedKeyCount, indexes);
            }
        }
    }
}
