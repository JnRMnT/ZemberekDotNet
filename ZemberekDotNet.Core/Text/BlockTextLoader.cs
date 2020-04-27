using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Native.Collections;
using static ZemberekDotNet.Core.Text.BlockTextLoader._SingleLoader;

namespace ZemberekDotNet.Core.Text
{
    public class BlockTextLoader : IEnumerable<TextChunk>
    {
        List<string> corpusPaths;
        int blockSize;

        public List<string> getCorpusPaths()
        {
            // defensive copy.
            return new List<string>(corpusPaths);
        }

        public int GetBlockSize()
        {
            return blockSize;
        }

        public int PathCount()
        {
            return corpusPaths.Count;
        }

        BlockTextLoader(List<string> corpusPaths, int blockSize)
        {
            corpusPaths = corpusPaths.OrderBy(e => Path.GetFullPath(e)).ToList();
            this.corpusPaths = corpusPaths;
            this.blockSize = blockSize;
        }

        public static BlockTextLoader FromPaths(List<string> corpora)
        {
            return new BlockTextLoader(corpora, _SingleLoader.DefaultBlockSize);
        }

        public static BlockTextLoader FromDirectory(string directoryPath)
        {
            List<string> paths = Directory.GetFiles(directoryPath).ToList();
            return new BlockTextLoader(paths, _SingleLoader.DefaultBlockSize);
        }

        public static BlockTextLoader FromPaths(List<string> corpora, int blockSize)
        {
            return new BlockTextLoader(corpora, blockSize);
        }

        public static BlockTextLoader FromPath(string corpus, int blockSize)
        {
            return new BlockTextLoader(new List<string> { corpus }, blockSize);
        }

        public static BlockTextLoader FromDirectoryRoot(
            string corporaRoot,
            string folderListFile,
            int blockSize)
        {
            List<string> rootNames = TextIO.LoadLines(folderListFile, "#");
            List<string> roots = new List<string>();
            rootNames.ForEach(e =>
            {
                roots.Add(Path.GetFullPath(Path.Combine(corporaRoot, e)));
            });

            List<string> corpora = new List<string>();
            foreach (string corpusRoot in roots)
            {
                corpora.AddRange(Directory.GetFiles(corpusRoot).ToList());
            }

            corpora = corpora.OrderBy(e => Path.GetFullPath(e)).ToList();
            Log.Info("There are %d corpus files.", corpora.Count);
            return new BlockTextLoader(corpora, blockSize);
        }

        public static IEnumerator<TextChunk> SinglePathIterator(string path, int blockSize)
        {
            return new _SingleLoader(path, blockSize).GetEnumerator();
        }

        public static IEnumerator<TextChunk> IteratorFromCharIndex(
            string path,
            int blockSize,
            long charIndex)
        {
            return new _SingleLoader(path, blockSize).IteratorFromCharIndex(charIndex);
        }

        public IEnumerator<TextChunk> GetEnumerator()
        {
            return new CorpusLinesIterator(new Deque<string>(corpusPaths), this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class CorpusLinesIterator : IEnumerator<TextChunk>
        {
            Deque<string> paths;
            string currentPath;
            _SingleLoader loader;
            TextIterator iterator;
            TextChunk Current { get; set; }

            TextChunk IEnumerator<TextChunk>.Current => Current;
            object IEnumerator.Current => Current;

            int index;
            int sourceIndex;
            BlockTextLoader owner;

            internal CorpusLinesIterator(Deque<string> paths, BlockTextLoader owner)
            {
                this.paths = paths;
                this.owner = owner;
            }

            public bool HasNext()
            {
                if (loader == null)
                {
                    NextPath();
                }
                if (iterator.HasNext())
                {
                    iterator.MoveNext();
                    Current = iterator.Current;
                    index++;
                    return true;
                }

                if (paths.IsEmpty)
                {
                    return false;
                }

                NextPath();
                if (iterator.HasNext())
                {
                    index = 0;
                    iterator.MoveNext();
                    Current = iterator.Current;
                    index++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private void NextPath()
            {
                string p = paths.PopFirst();
                loader = new _SingleLoader(p, sourceIndex, owner.blockSize);
                sourceIndex++;
                currentPath = p;
                iterator = (TextIterator)loader.GetEnumerator();
            }

            public TextChunk MoveNext()
            {
                return Current;
            }

            bool IEnumerator.MoveNext()
            {
                if (HasNext())
                {
                    MoveNext();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                this.Current = null;
                this.index = 0;
            }

            public void Dispose()
            {
                iterator.Dispose();
            }
        }

        internal class _SingleLoader : IEnumerable<TextChunk>
        {

            // by default load 10,000 lines.
            internal static readonly int DefaultBlockSize = 10_000;

            readonly string path;
            readonly int blockSize;
            readonly Encoding charset;
            readonly int sourceIndex;

            internal _SingleLoader(string path, Encoding charset, int sourceIndex, int blockSize)
            {
                this.path = path;
                this.charset = charset;
                this.blockSize = blockSize;
                this.sourceIndex = sourceIndex;
            }

            internal _SingleLoader(string path, int blockSize) : this(path, Encoding.UTF8, 0, blockSize)
            {

            }

            internal _SingleLoader(string path, int sourceIndex, int blockSize) : this(path, Encoding.UTF8, sourceIndex, blockSize)
            {

            }

            /// <summary>
            /// Returns an Iterator that loads [blocksize] lines in each iteration.
            /// </summary>
            /// <returns></returns>
            public IEnumerator<TextChunk> GetEnumerator()
            {
                try
                {
                    StreamReader reader = new StreamReader(File.OpenRead(path), charset);
                    return new TextIterator(reader, this);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e);
                    throw new SystemException("An IO Exception occured during getting enumerator", e);
                }
            }

            /**
             * Returns an Iterator that loads [blocksize] lines in each iteration. It starts loading from
             * [charIndex] value of the content.
             */
            internal IEnumerator<TextChunk> IteratorFromCharIndex(long charIndex)
            {
                try
                {
                    StreamReader reader = new StreamReader(File.OpenRead(path), charset);
                    long k = reader.BaseStream.Seek(charIndex, SeekOrigin.Current);
                    if (k != charIndex)
                    {
                        throw new InvalidOperationException("Cannot skip " + charIndex + " skip returned " + k);
                    }
                    if (charIndex != 0)
                    { // skip first line
                        reader.ReadLine();
                    }
                    return new TextIterator(reader, this);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e);
                    throw new SystemException("An IO Exception occured during creating a TextIterator from Char Index", e);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal class TextIterator : IEnumerator<TextChunk>, IDisposable
            {
                int blockIndex = 0;
                List<String> currentBlock;
                bool finished = false;
                private StreamReader br;
                private _SingleLoader owner;

                public TextChunk Current { get; set; }

                object IEnumerator.Current => Current;

                internal TextIterator(StreamReader br, _SingleLoader owner)
                {
                    this.br = br;
                    this.owner = owner;
                }

                public bool HasNext()
                {
                    if (finished)
                    {
                        return false;
                    }
                    int lineCounter = 0;
                    currentBlock = new List<string>(owner.blockSize);
                    String line;
                    try
                    {
                        while (lineCounter < owner.blockSize)
                        {
                            line = br.ReadLine();
                            if (line != null)
                            {
                                currentBlock.Add(line);
                            }
                            else
                            {
                                br.Close();
                                finished = true;
                                break;
                            }
                            lineCounter++;
                        }
                        return currentBlock.Count > 0;
                    }
                    catch (IOException e)
                    {
                        Console.Error.WriteLine(e);
                        throw new SystemException("An IO Exception occured during enumerating with TextIterator", e);
                    }
                }

                public void Close()
                {
                    if (br != null)
                    {
                        br.Close();
                    }
                }
                public bool MoveNext()
                {
                    TextChunk chunk = new TextChunk(
                        owner.path,
                        owner.sourceIndex,
                        blockIndex,
                        currentBlock);
                    blockIndex++;
                    Current = chunk;
                    return true;
                }

                public void Reset()
                {
                    blockIndex = 0;
                    finished = false;
                    currentBlock = null;
                    Current = null;
                }

                public void Dispose()
                {
                    br.Dispose();
                }
            }
        }
    }
}

