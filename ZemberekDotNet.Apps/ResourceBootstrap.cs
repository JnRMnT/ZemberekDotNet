using System;
using System.Collections.Generic;
using System.IO;

namespace ZemberekDotNet.Apps
{
    internal static class ResourceBootstrap
    {
        private static bool initialized;

        internal static void EnsureGlobalResourcesRoot()
        {
            if (initialized)
            {
                return;
            }

            string[] markers =
            {
                Path.Combine("ZemberekDotNet.Core", "Resources", "Text", "html-char-map-full.txt"),
                Path.Combine("ZemberekDotNet.Tokenization", "Resources", "tokenization", "sentence-boundary-model.bin"),
                Path.Combine("ZemberekDotNet.Morphology", "Resources", "tr", "lexicon.bin")
            };

            if (AllMarkersExist(Environment.CurrentDirectory, markers))
            {
                initialized = true;
                return;
            }

            List<string> candidates = new List<string>
            {
                AppContext.BaseDirectory,
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."))
            };

            foreach (string candidate in candidates)
            {
                if (AllMarkersExist(candidate, markers))
                {
                    Environment.CurrentDirectory = candidate;
                    initialized = true;
                    return;
                }
            }
        }

        private static bool AllMarkersExist(string root, IEnumerable<string> markers)
        {
            foreach (string marker in markers)
            {
                string fullPath = Path.Combine(root, marker);
                if (!File.Exists(fullPath))
                {
                    return false;
                }
            }
            return true;
        }
    }
}