using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZemberekDotNet.Core.Hash
{
    public interface IMphf
    {
        int Get(int[] key);

        int Get(int[] key, int hash);

        int Get(String key);

        int Get(int k0, int k1, int k2, int initialHash);

        int Get(int k0, int k1, int initialHash);

        int Get(String key, int hash);

        int Get(int[] ngram, int begin, int end, int hash);

        void Serialize(FileStream file);

        void Serialize(BinaryWriter os);

        double AverageBitsPerKey();

        int Size();
    }
}
