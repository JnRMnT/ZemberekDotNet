using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZemberekDotNet.Core.Collections;
using ZemberekDotNet.Core.Hash;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Compression
{
    /// <summary>
    /// This is a compact integer value lookup.Keys are considered as Strings.There may be false
    /// positives(that it can return values of other keys for an input). But the probability of occurring
    ///this is very low.
    /// </summary>
    public class LossyIntLookup
    {
        private IMphf mphf; // Minimal perfect hash function that provides string to integer index lookup.
        private int[] data; // contains fingerprints and actual data.

        private static readonly uint MAGIC = 0xcafebeef;

        private LossyIntLookup(IMphf mphf, int[] data)
        {
            this.mphf = mphf;
            this.data = data;
        }

        public int Get(String s)
        {
            int index = mphf.Get(s) * 2;
            int fingerprint = GetFingerprint(s);
            if (fingerprint == data[index])
            {
                return data[index + 1];
            }
            else
            {
                return 0;
            }
        }

        public int Size()
        {
            return data.Length / 2;
        }

        public float GetAsFloat(string s)
        {
            return Get(s).ToFloatFromBits();
        }

        private static int GetFingerprint(string s)
        {
            return s.GetHashCode() & 0x7ffffff;
        }

        /// <summary>
        /// Generates a LossyIntLookup from a String->Float lookup
        /// </summary>
        /// <param name="lookup"></param>
        /// <returns></returns>
        public static LossyIntLookup Generate(FloatValueMap<string> lookup)
        {
            List<String> keyList = lookup.GetKeyList();
            StringHashKeyProvider provider = new StringHashKeyProvider(keyList);
            MultiLevelMphf mphf = MultiLevelMphf.Generate(provider);
            int[] data = new int[keyList.Count * 2];
            foreach (string s in keyList)
            {
                int index = mphf.Get(s);
                data[index * 2] = GetFingerprint(s); // fingerprint
                data[index * 2 + 1] = lookup.Get(s).ToIntBits(); // data in int form
            }
            return new LossyIntLookup(mphf, data);
        }

        /// <summary>
        ///  Serialized this data structure to a binary file.
        /// </summary>
        /// <param name="path"></param>
        public void Serialize(string path)
        {
            BinaryWriter dos = IOUtil.GetDataOutputStream(path);
            dos.Write(MAGIC);
            dos.Write(data.Length);
            foreach (int d in data)
            {
                dos.Write(d);
            }
            mphf.Serialize(dos);
        }

        /// <summary>
        /// Checks if input {@link DataInputStream} [dis] contains a serialized LossyIntLookup.
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public static bool CheckStream(BinaryReader dis)
        {
            //PushbackInputStream pis = new PushbackInputStream(dis, 4);
            byte[] fourBytes = new byte[4];
            int c = dis.Read(fourBytes);
            if (c < 4)
            {
                return false;
            }
            int magic = (int)Bytes.ToInt(fourBytes, true);
            dis.BaseStream.Seek(-4, SeekOrigin.Current);
            return magic == MAGIC;
        }

        /// <summary>
        /// Deseializes a LossyIntLookup structure from a {@link DataInputStream} [dis]
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public static LossyIntLookup Deserialize(BinaryReader dis)
        {
            long magic = dis.ReadInt32();
            if (magic != MAGIC)
            {
                throw new InvalidOperationException("File does not carry expected value in the beginning.");
            }
            int length = dis.ReadInt32();
            int[]
            data = new int[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = dis.ReadInt32();
            }
            IMphf mphf = MultiLevelMphf.Deserialize(dis);
            return new LossyIntLookup(mphf, data);
        }
    }
}
