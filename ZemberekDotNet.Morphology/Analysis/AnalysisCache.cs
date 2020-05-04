using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZemberekDotNet.Core.Logging;
using ZemberekDotNet.Core.Text;
using ZemberekDotNet.Tokenization;

namespace ZemberekDotNet.Morphology.Analysis
{
    /// <summary>
    /// A simple analysis cache. Can be shared between threads.
    /// </summary>
    public class AnalysisCache
    {
        private static readonly int StaticCacheCapacity = 3000;
        private static readonly int DefaultInitialDynamicCacheCapacity = 1000;
        private static readonly int DefaultMaxDynamicCacheCapacity = 10000;
        private static readonly int DynamicCacheCapacityLimit = 1_000_000;

        private static readonly string MostUsedWordsFile = "Resources/tr/first-10K";
        private ConcurrentDictionary<string, WordAnalysis> staticCache;
        private bool staticCacheInitialized = false;
        private long staticCacheHits;
        private long staticCacheMiss;
        private MemoryCache dynamicCache;
        private bool staticCacheDisabled;
        private bool dynamicCacheDisabled;

        public AnalysisCache(AnalysisCacheBuilder builder)
        {
            this.dynamicCacheDisabled = builder.DisableDCache;
            this.staticCacheDisabled = builder.DisableSCache;

            dynamicCache = dynamicCacheDisabled ? null : new MemoryCache(new MemoryCacheOptions
            {
                //TODO: Check
                //SizeLimit = builder.DynamicCacheMaxSize
            });
            staticCache = staticCacheDisabled ? null : new ConcurrentDictionary<string, WordAnalysis>();
        }

        public static AnalysisCacheBuilder Builder()
        {
            return new AnalysisCacheBuilder();
        }

        public class AnalysisCacheBuilder
        {
            int _staticCacheSize = StaticCacheCapacity;
            int _dynamicCacheInitialSize = DefaultInitialDynamicCacheCapacity;
            int _dynamicCacheMaxSize = DefaultMaxDynamicCacheCapacity;
            bool _disableStaticCache = false;
            bool _disableDynamicCache = false;

            public int SCacheSize { get => _staticCacheSize; set => _staticCacheSize = value; }
            public int DynamicCacheInitialSize { get => _dynamicCacheInitialSize; set => _dynamicCacheInitialSize = value; }
            public int DynamicCacheMaxSize { get => _dynamicCacheMaxSize; set => _dynamicCacheMaxSize = value; }
            public bool DisableSCache { get => _disableStaticCache; set => _disableStaticCache = value; }
            public bool DisableDCache { get => _disableDynamicCache; set => _disableDynamicCache = value; }

            public AnalysisCacheBuilder StaticCacheSize(int staticCacheSize)
            {
                Contract.Requires(staticCacheSize >= 0,
                   string.Format("Static cache size cannot be negative. But it is {0}", staticCacheSize));
                this.SCacheSize = staticCacheSize;
                return this;
            }

            public AnalysisCacheBuilder DynamicCacheSize(int initial, int max)
            {
                Contract.Requires(initial >= 0,
                    string.Format("Dynamic cache initial size cannot be negative. But it is {0}", initial));
                Contract.Requires(max >= 0,
                   string.Format("Dynamic cache initial size cannot be negative. But it is {0}", max));
                Contract.Requires(max <= DynamicCacheCapacityLimit,
                   string.Format("Dynamic cache initial size cannot be larger than {0}. But it is {1}",
                    DynamicCacheCapacityLimit, max));
                this.DynamicCacheInitialSize = initial;
                this.DynamicCacheMaxSize = max;
                return this;
            }

            public AnalysisCacheBuilder DisableStaticCache()
            {
                this.DisableSCache = true;
                return this;
            }

            public AnalysisCacheBuilder DisableDynamicCache()
            {
                this.DisableDCache = true;
                return this;
            }

            public AnalysisCache Build()
            {
                return new AnalysisCache(this);
            }
        }

        public void InvalidateDynamicCache()
        {
            if (!dynamicCacheDisabled && dynamicCache != null)
            {
                dynamicCache.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void InitializeStaticCache(Func<string, WordAnalysis> analysisProvider)
        {
            if (staticCacheDisabled || staticCacheInitialized)
            {
                return;
            }
            Task.Run(() =>
            {
                try
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    List<string> words = TextIO.LoadLines(MostUsedWordsFile);
                    Log.Debug("File read in {0} ms.", stopwatch.ElapsedMilliseconds);
                    int size = Math.Min(StaticCacheCapacity, words.Count);
                    for (int i = 0; i < size; i++)
                    {
                        string word = words[i];
                        staticCache.TryAdd(word, analysisProvider(word));
                    }
                    Log.Debug("Static cache initialized with {0} most frequent words", size);
                    Log.Debug("Initialization time: {0} ms.", stopwatch.ElapsedMilliseconds);
                }
                catch (IOException e)
                {
                    Log.Error("Could not read most frequent words list, static cache is disabled.");
                    Console.Error.WriteLine(e);
                }
            }).Wait();
            staticCacheInitialized = true;
        }

        public WordAnalysis GetAnalysis(string input, Func<string, WordAnalysis> analysisProvider)
        {

            WordAnalysis analysis = staticCacheDisabled ? null : staticCache.GetValueOrDefault(input);
            if (analysis != null)
            {
                staticCacheHits++;
                return analysis;
            }
            staticCacheMiss++;
            if (dynamicCacheDisabled)
            {
                return analysisProvider(input);
            }
            else
            {
                return analysisProvider(dynamicCache.Get<string>(input).ToStringOrEmpty());
            }
        }

        public WordAnalysis GetAnalysis(Token input, Func<Token, WordAnalysis> analysisProvider)
        {
            WordAnalysis analysis = staticCacheDisabled ? null : staticCache.GetValueOrDefault(input.GetText());
            if (analysis != null)
            {
                staticCacheHits++;
                return analysis;
            }
            staticCacheMiss++;
            if (dynamicCacheDisabled)
            {
                return analysisProvider(input);
            }
            else
            {
                WordAnalysis a = dynamicCache.Get<WordAnalysis>(input.GetText());
                if (a == null)
                {
                    a = analysisProvider(input);
                    dynamicCache.Set(input.GetText(), a);
                }
                return a;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            long total = staticCacheHits + staticCacheMiss;
            if (total > 0)
            {
                sb.Append(string.Format("Static cache(size: {0}) Hit rate: {1:F3}{2}",
                    staticCache.Count, 1.0 * (staticCacheHits) / (staticCacheHits + staticCacheMiss), Environment.NewLine));
            }
            //sb.Append(String.Format("Dynamic cache hit rate: {0:F3} ", dynamicCache.hit.stats().hitRate()));
            return sb.ToString();
        }
    }
}
