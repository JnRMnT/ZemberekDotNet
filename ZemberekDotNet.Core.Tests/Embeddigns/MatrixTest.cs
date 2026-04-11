using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using ZemberekDotNet.Core.Embeddings;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Tests.Embeddigns
{
    [TestClass]
    public class MatrixTest
    {
        [TestMethod]
        public void SaveLoadTest()
        {
            SaveLoad(1, 1);
            SaveLoad(10, 10);
            SaveLoad(10, 1);
            SaveLoad(100000, 10);
            SaveLoad(100001, 1);
            SaveLoad(100010, 1);
        }

        [TestMethod]
        public void SaveLoadLargeTest()
        {
            SaveLoad(11_000_000, 20);
        }

        // Extracted to its own method so the JIT stack frame (and therefore the GC root for `ma`)
        // is torn down on return. In Debug builds the JIT extends local lifetimes to the end of
        // the declaring method, so leaving this code inline in SaveLoad would keep `ma` alive
        // during Matrix.Load even after GC.Collect(), causing ~2x peak memory instead of ~1x.
        // NoInlining prevents the JIT from merging the locals back into the caller's frame.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteMatrix(int m, int n, string path)
        {
            using (BinaryWriter dos = IOUtil.GetDataOutputStream(path))
            {
                Matrix ma = new Matrix(m, n);
                int k = 0;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        ma.data_[i][j] = k * 0.01f;
                        k++;
                    }
                }
                ma.Save(dos);
            }
        } // ma's stack frame is gone here — definitely collectible

        private void SaveLoad(int m, int n)
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                WriteMatrix(m, n, tempFile);

                Assert.AreEqual(m * n * 4 + 8, new FileInfo(tempFile).Length);

                // WriteMatrix's stack frame is fully torn down at this point, so ma has no GC
                // roots. GC.Collect() reliably reclaims the ~1 GB before loading begins.
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Matrix loaded;
                using (BinaryReader dis = IOUtil.GetDataInputStream(tempFile))
                {
                    loaded = Matrix.Load(dis);
                }

                int idx = 0;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        Assert.AreEqual(idx * 0.01f, loaded.data_[i][j], 0.1);
                        idx++;
                    }
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
