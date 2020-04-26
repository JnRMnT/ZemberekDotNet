using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
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

        private void SaveLoad(int m, int n)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "foo.bar");
            BinaryWriter dos = IOUtil.GetDataOutputStream(tempFile);
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
            dos.Close();
            Assert.AreEqual(m * n * 4 + 8, new FileInfo(tempFile).Length);
            BinaryReader dis = IOUtil.GetDataInputStream(tempFile);
            ma = Matrix.Load(dis);
            k = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Assert.AreEqual(k * 0.01f, ma.data_[i][j], 0.1);
                    k++;
                }
            }

            dis.Close();
        }
    }
}
