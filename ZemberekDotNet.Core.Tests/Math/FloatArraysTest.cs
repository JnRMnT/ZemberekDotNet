using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Math;

namespace ZemberekDotNet.Core.Tests.Math
{
    [TestClass]
    public class FloatArraysTest
    {
        static float delta = 0.0001f;

        public static bool InDelta(float result, float actual)
        {
            return System.Math.Abs(result - actual) < delta;
        }

        public static bool InDelta(float[] result, float[] actual)
        {
            for (int i = 0; i < result.Length; i++)
            {
                if (System.Math.Abs(result[i] - actual[i]) > delta)
                {
                    return false;
                }
            }
            return true;
        }

        [TestMethod]
        public void TestSum()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            float[] da2 = { 0.5f, -2, 30, 1, -30 };
            Assert.IsTrue(InDelta(FloatArrays.Sum(da), -1));
            Assert.IsTrue(InDelta(FloatArrays.Sum(da, da2), new float[] { 1.5f, 0, 30, 0, -33 }));
            FloatArrays.AddToFirst(da, da2);
            Assert.IsTrue(InDelta(da, new float[] { 1.5f, 0, 30, 0, -33 }));
        }

        [TestMethod]
        public void AddToFirstScaledTest()
        {
            float[] da1 = { 1, 2, 0, -1, -30 };
            float[] da2 = { -0.5f, -1, 0, 0.5f, 15 };
            FloatArrays.AddToFirstScaled(da1, da2, 2);
            Assert.AreEqual(FloatArrays.Max(da1), 0f, 0.0001);
            Assert.AreEqual(FloatArrays.Min(da1), 0f, 0.0001);
        }

        [TestMethod]
        public void testDotProduct()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            float[] da2 = { 0.5f, -2, 30, 1, -30 };
            Assert.IsTrue(InDelta(FloatArrays.DotProduct(da, da2), 85.5f));
        }

        [TestMethod]
        public void TestSubstract()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            float[] da2 = { 0.5f, -2, 30, 1, -30 };
            Assert.IsTrue(InDelta(FloatArrays.Subtract(da, da2), new float[] { 0.5f, 4, -30, -2, 27 }));
            FloatArrays.SubtractFromFirst(da, da2);
            Assert.IsTrue(InDelta(da, new float[] { 0.5f, 4, -30, -2, 27 }));
        }

        [TestMethod]
        public void TestAppendZeros()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            float[] da2 = { 1, 2, 0, -1, -3, 0, 0, 0, 0, 0, 0 };
            Assert.IsTrue(InDelta(FloatArrays.AppendZeros(da, 6), da2));
            Assert.IsTrue(InDelta(FloatArrays.AppendZeros(da2, 0), da2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAppendZerosExc()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            FloatArrays.AppendZeros(da, -10);
        }

        [TestMethod]
        public void TestMaxMinValue()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(FloatArrays.Max(da), 2));
            Assert.IsTrue(InDelta(FloatArrays.Min(da), -3));
        }

        [TestMethod]
        public void TestMaxMinIndex()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            Assert.AreEqual(FloatArrays.MaxIndex(da), 1);
            Assert.AreEqual(FloatArrays.MinIndex(da), 4);
            float[] da2 = { 2, 2, 0, -1, -3, 2, -3 };
            Assert.AreEqual(FloatArrays.MaxIndex(da2), 0);
            Assert.AreEqual(FloatArrays.MinIndex(da2), 4);
        }

        [TestMethod]
        public void TestSquare()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(FloatArrays.Square(da), new float[] { 1, 4, 0, 1, 9 }));
        }

        [TestMethod]
        public void TestSquaredSum()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(FloatArrays.SquaredSum(da), 15));
            float[] da2 = { 0, 1, 1, -2, 1 };
            Assert.IsTrue(InDelta(FloatArrays.SquaredSumOfDifferences(da, da2), 20));
            Assert.IsTrue(InDelta(FloatArrays.AbsoluteDifference(da, da2), new float[] { 1, 1, 1, 1, 4 }));
        }

        [TestMethod]
        public void TestMean()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(FloatArrays.Mean(da), -1f / 5f));
        }

        [TestMethod]
        public void TestInRange()
        {
            float d = 3;
            float d2 = 2.5f;
            float d3 = 7;
            Assert.IsTrue(FloatArrays.InRange(d, d2, 1));
            Assert.IsFalse(FloatArrays.InRange(d2, d3, 4));
            Assert.IsTrue(FloatArrays.InRange(d, d2, 0.5f));
        }

        [TestMethod]
        public void TestReverse()
        {
            float[] da = { 1, 3, 7, 2.5f, 0 };
            float[] da2 = FloatArrays.Reverse(FloatArrays.Reverse(da));
            Assert.IsNotNull(da2);
            Assert.AreEqual(da.Length, da2.Length);
            Assert.IsTrue(InDelta(da, da2));
        }

        [TestMethod]
        public void TestConvertInt()
        {
            int[] ia = { 1, 3, -1, 0, 9, 12 };
            float[] da2 = { 1, 3, -1, 0, 9.0f, 12.0f };
            float[] da = FloatArrays.Convert(ia);
            Assert.IsNotNull(da);
            Assert.IsTrue(InDelta(da, da2));
        }

        [TestMethod]
        public void TestConvert2f()
        {
            int[][] ia = { new int[] { 1, 2 }, new int[] { 4, 3 }, new int[] { -1, 5 } };
            float[][] da = { new float[] { 1, 2 }, new float[] { 4, 3 }, new float[] { -1, 5 } };
            float[][] da2 = FloatArrays.Convert(ia);
            Assert.AreEqual(ia.Length, da2.Length);
            Assert.IsNotNull(da2);
            int k = 0;
            foreach (float[] i in da)
            {
                int j = 0;
                foreach (float ii in i)
                {
                    Assert.AreEqual(da2[k][j++], ii, 0.0001);
                }
                k++;
            }
        }

        [TestMethod]
        public void TestArrayEqualsInRange()
        {
            float[] da = { 0, 3.5f, 7, -2, 19 };
            float[] da2 = { 0, 4.5f, 4, 2, -18 };
            Assert.IsTrue(FloatArrays.ArrayEqualsInRange(da, da2, 37));
            Assert.IsFalse(FloatArrays.ArrayEqualsInRange(da, da2, 15));
            Assert.IsTrue(FloatArrays.ArrayEqualsInRange(da, da2, 45));
        }

        [TestMethod]
        public void TestArrayEquals()
        {
            float[] da = { -1, 0, 3.4f, 7 };
            float[] da2 = { -1, 0.0f, 3.4f, 7 };
            float[] da3 = { 7, 9.0f, 12.3f, -5.6f };
            Assert.IsTrue(FloatArrays.ArrayEquals(da, da2));
            Assert.IsFalse(FloatArrays.ArrayEquals(da2, da3));
        }

        [TestMethod]
        public void TestMultiply()
        {
            float[] da = { -1, 1.3f, 8.2f, 10, 90 };
            float[] da2 = { -1, 3, 2, 2, -1 };
            Assert.IsTrue(InDelta(FloatArrays.Multiply(da, da2), new float[] { 1, 3.9f, 16.4f, 20, -90 }));
            FloatArrays.MultiplyToFirst(da, da2);
            Assert.IsTrue(InDelta(da, new float[] { 1, 3.9f, 16.4f, 20, -90 }));
        }

        [TestMethod]
        public void TestScale()
        {
            float[] da = { 1, -1, 3.1f, 0.0f, 0 };
            Assert.IsTrue(InDelta(FloatArrays.Scale(da, 0), new float[] { 0, 0, 0, 0, 0 }));
            Assert.IsTrue(InDelta(FloatArrays.Scale(da, -1), new float[] { -1, 1, -3.1f, 0, 0.0f }));
            Assert.IsTrue(InDelta(FloatArrays.Scale(da, 4), new float[] { 4, -4, 12.4f, 0.0f, 0.0f }));

            FloatArrays.ScaleInPlace(da, 4);
            Assert.IsTrue(InDelta(da, new float[] { 4, -4, 12.4f, 0.0f, 0.0f }));
            FloatArrays.ScaleInPlace(da, 0);
            Assert.IsTrue(InDelta(da, new float[] { 0, 0, 0, 0, 0 }));
        }

        [TestMethod]
        public void TestAbsoluteDifference()
        {
            float[] da = { 1, 2, 4.5f, -2, 4 };
            float[] da2 = { 1, 1, 2.5f, 5, -15.2f };
            Assert.IsTrue(
                InDelta(FloatArrays.AbsoluteDifference(da, da2), new float[] { 0, 1, 2, 7, 19.2f }));
            Assert.AreEqual(FloatArrays.AbsoluteSumOfDifferences(da, da2), 29.2f, 0.0001);
        }

        [TestMethod]
        public void TestVariance()
        {
            float[] da = { 0, 2, 4 };
            Assert.AreEqual(FloatArrays.Variance(da), 4f, 0.0001);
            Assert.AreEqual(FloatArrays.StandardDeviation(da), 2f, 0.0001);
        }

        [TestMethod]
        public void TestContainsNaN()
        {
            float[] da = { -1, 1, 3, 5, 7, 9, float.NaN };
            Assert.IsTrue(FloatArrays.ContainsNaN(da));

            float[] da2 = { -1, 1, 3, 5, 7, 9 };
            Assert.IsFalse(FloatArrays.ContainsNaN(da2));
        }

        [TestMethod]
        public void TestFloorInPlace()
        {
            float[] da = { 0, -7, 2, 1.1123f, -10, -22, 56 };
            FloatArrays.FloorInPlace(da, -7);
            Assert.IsTrue(InDelta(da, new float[] { 0, -7, 2, 1.1123f, -7, -7, 56 }));

        }

        [TestMethod]
        public void TestNonZeroFloorInPlace()
        {
            float[] da = { 0, -7, 2, 1.1123f, -10, -22, 56 };
            FloatArrays.NonZeroFloorInPlace(da, 3);
            Assert.IsTrue(InDelta(da, new float[] { 0, 3, 3, 3, 3, 3, 56 }));

        }

        [TestMethod]
        public void TestNormalizeInPlace()
        {
            float[] da = { 1, 5.6f, 2.4f, -1, -3.0f, 5 };
            FloatArrays.NormalizeInPlace(da);
            Assert.IsTrue(InDelta(da, new float[] { 0.1f, 0.56f, 0.24f, -0.1f, -0.3f, 0.5f }));
        }

        [TestMethod]
        public void TestAddToAll()
        {
            float[] da = { 1, 2, 0, -1, -3 };
            float[] expected = { 2, 3, 1, 0, -2 };
            FloatArrays.AddToAll(da, 1);
            Assert.IsTrue(Enumerable.SequenceEqual(expected, da));
        }

        [TestMethod]
        public void TestValidateArray()
        {
            float[] da = null;
            try
            {
                FloatArrays.ValidateArray(da);
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }

            da = new float[0];
            try
            {
                FloatArrays.ValidateArray(da);
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestValidateArrays()
        {
            float[] da1 = { 1, 2, 3 };
            float[] da2 = null;
            try
            {
                FloatArrays.ValidateArrays(da1, da2);
            }
            catch (NullReferenceException)
            {
                Assert.IsTrue(true);
            }

            float[] da3 = { 5, 6, 2.33f, 2 };
            try
            {
                FloatArrays.ValidateArrays(da1, da3);
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestNormalize16bitLittleEndian()
        {
            byte[] ba = { 0x10, 0x71, 0x18, 0x54 };
            float[] da = FloatArrays.Normalize16bitLittleEndian(ba);
            Assert.AreEqual(da[0] * short.MaxValue, 28944f, 0.0001);
            Assert.AreEqual(da[1] * short.MaxValue, 21528f, 0.0001);

            byte[] ba2 = FloatArrays.Denormalize16BitLittleEndian(da);
            Assert.AreEqual(ba2[0], 0x10);
            Assert.AreEqual(ba2[3], 0x54);

            byte[] ba3 = FloatArrays.DenormalizeLittleEndian(da, 16);
            Assert.AreEqual(ba3[0], 0x10);
            Assert.AreEqual(ba3[3], 0x54);

            byte[] ba4 = { (byte)0xCC, (byte)0xAB };
            da = FloatArrays.Normalize16bitLittleEndian(ba4);
            Assert.AreEqual(da[0] * short.MinValue, 21556f, 0.0001);
        }

        [TestMethod]
        public void TestToUnsignedInteger()
        {
            float[] da = { 0.2f, -0.4f, 0.6f };
            int[] ia = FloatArrays.ToUnsignedInteger(da, 6);
            Assert.AreEqual((int)(0.2f * 3.0f), ia[0]);
            Assert.AreEqual((int)(-0.4f * 3.0f), ia[1]);
        }

        [TestMethod]
        public void TestFormat()
        {
            float[] da = { 0.2f, -0.45f, 0.6f };
            Assert.AreEqual("0.2 -0.4 0.6", FloatArrays.Format(1, " ", da));
            Assert.AreEqual("0.20 -0.45 0.60", FloatArrays.Format(2, " ", da));
            Assert.AreEqual("0.20, -0.45, 0.60", FloatArrays.Format(2, ", ", da));
            Assert.AreEqual("0.20-0.450.60", FloatArrays.Format(2, "", da));
            Assert.AreEqual("0.2   -0.4  0.6", FloatArrays.Format(5, 1, " ", da));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValidateArrayExc()
        {
            float[] da1 = new float[0];
            FloatArrays.ValidateArray(da1);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestValidateArraysNullExc()
        {
            float[] da1 = null;
            float[] da2 = null;
            FloatArrays.ValidateArrays(da1, da2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValidateArraysArgExc()
        {
            float[] da1 = new float[2];
            float[] da2 = new float[3];
            FloatArrays.ValidateArrays(da1, da2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNormalize16bitExc()
        {
            byte[] ba = { 255 - 10, 45, 120 };
            FloatArrays.Normalize16bitLittleEndian(ba);
        }

        [TestMethod]
        public void TestSerialization1D()
        {
            Serialization1D(new float[] { 0.2f, -0.4f, 0.6f });
            Serialization1D(new float[] { 0.256f });
            Serialization1D(new float[] { });
        }

        [TestMethod]
        public void TestSerialization2D()
        {
            Serialization2D(new float[][] { new float[] { 0.2f }, new float[] { -0.4f }, new float[] { 0.6f } });
            Serialization2D(new float[][] { new float[] { 0.2f, 1f }, new float[] { -0.4f, 2 }, new float[] { 0.6f, 3 } });
            Serialization2D(new float[][] { new float[] { 0.2f, 1f }, new float[] { -0.4f }, new float[] { 0.6f, 3, 6 } });
            Serialization2D(new float[][] { new float[] { }, new float[] { }, new float[] { 0.6f, 3, 6 } });
            Serialization2D(new float[][] { new float[] { }, new float[] { }, new float[] { } });
        }

        private void Serialization1D(float[] da)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "blah.foo");
            FileStream f = File.Create(tempFile);
            BinaryWriter dos = new BinaryWriter(f);
            FloatArrays.Serialize(dos, da);
            dos.Close();
            BinaryReader dis = new BinaryReader(File.OpenRead(tempFile));
            float[] read = FloatArrays.Deserialize(dis);
            Assert.IsTrue(FloatArrays.ArrayEquals(da, read));
            dis.Close();
            File.Delete(tempFile);
        }


        private void Serialization2D(float[][] da)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "blah.foo");
            FileStream f = File.Create(tempFile);
            BinaryWriter dos = new BinaryWriter(f);
            FloatArrays.Serialize(dos, da);
            dos.Close();
            BinaryReader dis = new BinaryReader(File.OpenRead(tempFile));
            float[][] read = FloatArrays.Deserialize2d(dis);
            for (int i = 0; i < read.Length; i++)
            {
                Assert.IsTrue(FloatArrays.ArrayEquals(da[i], read[i]));
            }
            dis.Close();
            File.Delete(tempFile);
        }
    }
}
