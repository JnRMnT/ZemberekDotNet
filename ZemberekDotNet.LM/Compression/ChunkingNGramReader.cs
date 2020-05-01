using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ZemberekDotNet.Core.Hash;

namespace ZemberekDotNet.LM.Compression
{
    public class ChunkingNGramReader : IEnumerable<IIntHashKeyProvider>
    {
        internal string file;
        internal int chunkGramSize;
        internal int chunkByteSize;
        internal int order;

        public ChunkingNGramReader(string file, int order, int chunkGramSize)
        {
            this.file = file;
            this.order = order;
            this.chunkGramSize = chunkGramSize;
            this.chunkByteSize = chunkGramSize * order * 4;
        }

        public IEnumerator<IIntHashKeyProvider> GetEnumerator()
        {
            return new ChunkIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class ChunkIterator : IEnumerator<IIntHashKeyProvider>
        {
            FileStream raf;
            int readByteAmount;
            byte[] data;
            ChunkingNGramReader owner;

            internal ChunkIterator(ChunkingNGramReader owner)
            {
                this.owner = owner;
                data = new byte[owner.chunkByteSize];
                try
                {
                    raf = System.IO.File.OpenRead(owner.file);
                    raf.Seek(8, SeekOrigin.Current);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e);
                }
            }

            public IIntHashKeyProvider Current { get; set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                if(raf != null)
                {
                    raf.Dispose();
                }
            }

            public bool HasNext()
            {
                try
                {
                    readByteAmount = raf.Read(data);
                    if (readByteAmount > 0)
                    {
                        if (readByteAmount < owner.chunkByteSize)
                        {
                            data = data.CopyOf(readByteAmount);
                        }
                        return true;
                    }
                    else
                    {
                        raf.Close();
                        return false;
                    }

                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e);
                }
                return false;
            }

            public bool MoveNext()
            {
                if (HasNext())
                {
                    Current = Next();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public IIntHashKeyProvider Next()
            {
                return new ByteGramProvider(data, owner.order, readByteAmount / (owner.order * 4));
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
