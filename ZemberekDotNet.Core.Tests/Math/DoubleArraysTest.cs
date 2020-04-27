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
    public class DoubleArraysTest
    {
        static double delta = 0.0001;

        public static bool InDelta(double result, double actual)
        {
            return System.Math.Abs(result - actual) < delta;
        }

        public static bool InDelta(double[] result, double[] actual)
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
            double[] da = { 1, 2, 0, -1, -3 };
            double[] da2 = { 0.5, -2, 30, 1, -30 };
            Assert.IsTrue(InDelta(DoubleArrays.Sum(da), -1));
            Assert.IsTrue(InDelta(DoubleArrays.Sum(da, da2), new double[] { 1.5, 0, 30, 0, -33 }));
            DoubleArrays.AddToFirst(da, da2);
            Assert.IsTrue(InDelta(da, new double[] { 1.5, 0, 30, 0, -33 }));
        }

        [TestMethod]
        public void AddToFirstScaledTest()
        {
            double[] da1 = { 1, 2, 0, -1, -30 };
            double[] da2 = { -0.5, -1, 0, 0.5, 15 };
            DoubleArrays.AddToFirstScaled(da1, da2, 2);
            Assert.AreEqual(DoubleArrays.Max(da1), 0d, 0.0001);
            Assert.AreEqual(DoubleArrays.Min(da1), 0d, 0.0001);
        }

        [TestMethod]
        public void TestDotProduct()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            double[] da2 = { 0.5, -2, 30, 1, -30 };
            Assert.IsTrue(InDelta(DoubleArrays.DotProduct(da, da2), 85.5));
        }

        [TestMethod]
        public void TestSubstract()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            double[] da2 = { 0.5, -2, 30, 1, -30 };
            Assert.IsTrue(InDelta(DoubleArrays.Subtract(da, da2), new double[] { 0.5, 4, -30, -2, 27 }));
            DoubleArrays.SubtractFromFirst(da, da2);
            Assert.IsTrue(InDelta(da, new double[] { 0.5, 4, -30, -2, 27 }));
        }

        [TestMethod]
        public void TestAppendZeros()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            double[] da2 = { 1, 2, 0, -1, -3, 0, 0, 0, 0, 0, 0 };
            Assert.IsTrue(InDelta(DoubleArrays.AppendZeros(da, 6), da2));
            Assert.IsTrue(InDelta(DoubleArrays.AppendZeros(da2, 0), da2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAppendZerosExc()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            DoubleArrays.AppendZeros(da, -10);
        }

        [TestMethod]
        public void TestMaxMinValue()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(DoubleArrays.Max(da), 2));
            Assert.IsTrue(InDelta(DoubleArrays.Min(da), -3));
        }

        [TestMethod]
        public void TestMaxMinIndex()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            Assert.AreEqual(DoubleArrays.MaxIndex(da), 1);
            Assert.AreEqual(DoubleArrays.MinIndex(da), 4);
            double[] da2 = { 2, 2, 0, -1, -3, 2, -3 };
            Assert.AreEqual(DoubleArrays.MaxIndex(da2), 0);
            Assert.AreEqual(DoubleArrays.MinIndex(da2), 4);
        }

        [TestMethod]
        public void TestSquare()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(DoubleArrays.Square(da), new double[] { 1, 4, 0, 1, 9 }));
        }

        [TestMethod]
        public void TestSquaredSum()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(DoubleArrays.SquaredSum(da), 15));
            double[] da2 = { 0, 1, 1, -2, 1 };
            Assert.IsTrue(InDelta(DoubleArrays.SquaredSumOfDifferences(da, da2), 20));
            Assert
                .IsTrue(InDelta(DoubleArrays.AbsoluteDifference(da, da2), new double[] { 1, 1, 1, 1, 4 }));
        }

        [TestMethod]
        public void TestAddToAll()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            double[] expected = { 2, 3, 1, 0, -2 };
            DoubleArrays.AddToAll(da, 1);
            Assert.IsTrue(Enumerable.SequenceEqual(expected, da));
        }

        [TestMethod]
        public void TestMean()
        {
            double[] da = { 1, 2, 0, -1, -3 };
            Assert.IsTrue(InDelta(DoubleArrays.Mean(da), -1d / 5d));
        }

        [TestMethod]
        public void TestInRange()
        {
            double d = 3;
            double d2 = 2.5;
            double d3 = 7;
            Assert.IsTrue(DoubleArrays.InRange(d, d2, 1));
            Assert.IsFalse(DoubleArrays.InRange(d2, d3, 4));
            Assert.IsTrue(DoubleArrays.InRange(d, d2, 0.5));
        }

        [TestMethod]
        public void TestReverse()
        {
            double[] da = { 1, 3, 7, 2.5, 0 };
            double[] da2 = DoubleArrays.Reverse(DoubleArrays.Reverse(da));
            Assert.IsNotNull(da2);
            Assert.AreEqual(da.Length, da2.Length);
            Assert.IsTrue(InDelta(da, da2));
        }

        [TestMethod]
        public void TestConvertInt()
        {
            int[] ia = { 1, 3, -1, 0, 9, 12 };
            double[] da2 = { 1, 3, -1, 0, 9.0, 12.0 };
            double[] da = DoubleArrays.Convert(ia);
            Assert.IsNotNull(da);
            Assert.IsTrue(InDelta(da, da2));
        }

        [TestMethod]
        public void TestConvert2d()
        {
            int[][] ia = new int[][] { new int[] { 1, 2 }, new int[] { 4, 3 }, new int[] { -1, 5 } };
            double[][] da = new double[][] { new double[] { 1, 2 }, new double[] { 4, 3 }, new double[] { -1, 5 } };
            double[][] da2 = DoubleArrays.Convert(ia);
            Assert.AreEqual(ia.Length, da2.Length);
            Assert.IsNotNull(da2);
            int k = 0;
            foreach (double[] i in da)
            {
                int j = 0;
                foreach (double ii in i)
                {
                    Assert.AreEqual(da2[k][j++], ii, 0.0001);
                }
                k++;
            }
        }

        [TestMethod]
        public void TestConvertFloat()
        {
            float[] fa = { 1, 3, -1, 0, 9, 12 };
            double[] da2 = { 1, 3, -1, 0, 9.0, 12.0 };
            double[] da = DoubleArrays.Convert(fa);
            Assert.IsNotNull(da);
            Assert.IsTrue(InDelta(da, da2));
        }

        [TestMethod]
        public void TestArrayEqualsInRange()
        {
            double[] da = { 0, 3.5, 7, -2, 19 };
            double[] da2 = { 0, 4.5, 4, 2, -18 };
            Assert.IsTrue(DoubleArrays.ArrayEqualsInRange(da, da2, 37));
            Assert.IsFalse(DoubleArrays.ArrayEqualsInRange(da, da2, 15));
            Assert.IsTrue(DoubleArrays.ArrayEqualsInRange(da, da2, 45));
        }

        [TestMethod]
        public void TestArrayEquals()
        {
            double[] da = { -1, 0, 3.4, 7 };
            double[] da2 = { -1, 0.0, 3.4, 7 };
            double[] da3 = { 7, 9.0, 12.3, -5.6 };
            Assert.IsTrue(DoubleArrays.ArrayEquals(da, da2));
            Assert.IsFalse(DoubleArrays.ArrayEquals(da2, da3));
        }

        [TestMethod]
        public void TestMultiply()
        {
            double[] da = { -1, 1.3, 8.2, 10, 90 };
            double[] da2 = { -1, 3, 2, 2, -1 };
            Assert.IsTrue(InDelta(DoubleArrays.Multiply(da, da2), new double[] { 1, 3.9, 16.4, 20, -90 }));
            DoubleArrays.MultiplyToFirst(da, da2);
            Assert.IsTrue(InDelta(da, new double[] { 1, 3.9, 16.4, 20, -90 }));
        }

        [TestMethod]
        public void TestScale()
        {
            double[] da = { 1, -1, 3.1, 0.0, 0 };
            Assert.IsTrue(InDelta(DoubleArrays.Scale(da, 0), new double[] { 0, 0, 0, 0, 0 }));
            Assert.IsTrue(InDelta(DoubleArrays.Scale(da, -1), new double[] { -1, 1, -3.1, 0, 0.0 }));
            Assert.IsTrue(InDelta(DoubleArrays.Scale(da, 4), new double[] { 4, -4, 12.4, 0.0, 0.0 }));

            DoubleArrays.ScaleInPlace(da, 4);
            Assert.IsTrue(InDelta(da, new double[] { 4, -4, 12.4, 0.0, 0.0 }));
            DoubleArrays.ScaleInPlace(da, 0);
            Assert.IsTrue(InDelta(da, new double[] { 0, 0, 0, 0, 0 }));
        }

        [TestMethod]
        public void TestAbsoluteDifference()
        {
            double[] da = { 1, 2, 4.5, -2, 4 };
            double[] da2 = { 1, 1, 2.5, 5, -15.2 };
            Assert.IsTrue(
                InDelta(DoubleArrays.AbsoluteDifference(da, da2), new double[] { 0, 1, 2, 7, 19.2 }));
            Assert.AreEqual(DoubleArrays.AbsoluteSumOfDifferences(da, da2), 29.2, 0.0001);
        }

        [TestMethod]
        public void TestVariance()
        {
            double[] da = { 0, 2, 4 };
            Assert.AreEqual(DoubleArrays.Variance(da), 4d, 0.0001);
            Assert.AreEqual(DoubleArrays.StandardDeviation(da), 2d, 0.0001);
        }

        [TestMethod]
        public void TestContainsNaN()
        {
            double[] da = { -1, 1, 3, 5, 7, 9, Double.NaN };
            Assert.IsTrue(DoubleArrays.ContainsNaN(da));

            double[] da2 = { -1, 1, 3, 5, 7, 9 };
            Assert.IsFalse(DoubleArrays.ContainsNaN(da2));
        }

        [TestMethod]
        public void TestFloorInPlace()
        {
            double[] da = { 0, -7, 2, 1.1123, -10, -22, 56 };
            DoubleArrays.FloorInPlace(da, -7);
            Assert.IsTrue(InDelta(da, new double[] { 0, -7, 2, 1.1123, -7, -7, 56 }));

        }

        [TestMethod]
        public void TestNonZeroFloorInPlace()
        {
            double[] da = { 0, -7, 2, 1.1123, -10, -22, 56 };
            DoubleArrays.NonZeroFloorInPlace(da, 3);
            Assert.IsTrue(InDelta(da, new double[] { 0, 3, 3, 3, 3, 3, 56 }));

        }

        [TestMethod]
        public void TestNormalizeInPlace()
        {
            double[] da = { 1, 5.6, 2.4, -1, -3.0, 5 };
            DoubleArrays.NormalizeInPlace(da);
            Assert.IsTrue(InDelta(da, new double[] { 0.1, 0.56, 0.24, -0.1, -0.3, 0.5 }));
        }

        [TestMethod]
        public void TestValidateArray()
        {
            double[] da = null;
            try
            {
                DoubleArrays.ValidateArray(da);
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }

            da = new double[0];
            try
            {
                DoubleArrays.ValidateArray(da);
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestValidateArrays()
        {
            double[] da1 = { 1, 2, 3 };
            double[] da2 = null;
            try
            {
                DoubleArrays.ValidateArrays(da1, da2);
            }
            catch (NullReferenceException)
            {
                Assert.IsTrue(true);
            }

            double[] da3 = { 5, 6, 2.33, 2 };
            try
            {
                DoubleArrays.ValidateArrays(da1, da3);
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
            double[] da = DoubleArrays.Normalize16bitLittleEndian(ba);
            Assert.AreEqual(da[0] * short.MaxValue, 28944d, 0.0001);
            Assert.AreEqual(da[1] * short.MaxValue, 21528d, 0.0001);

            byte[] ba2 = DoubleArrays.Denormalize16BitLittleEndian(da);
            Assert.AreEqual(ba2[0], 0x10);
            Assert.AreEqual(ba2[3], 0x54);

            byte[] ba3 = DoubleArrays.DenormalizeLittleEndian(da, 16);
            Assert.AreEqual(ba3[0], 0x10);
            Assert.AreEqual(ba3[3], 0x54);

            byte[] ba4 = { (byte)0xCC, (byte)0xAB };
            da = DoubleArrays.Normalize16bitLittleEndian(ba4);
            Assert.AreEqual(da[0] * short.MinValue, 21556d, 0.0001);
        }

        [TestMethod]
        public void TestToUnsignedInteger()
        {
            double[] da = { 0.2, -0.4, 0.6 };
            int[] ia = DoubleArrays.ToUnsignedInteger(da, 6);
            Assert.AreEqual((int)(0.2 * 3.0), ia[0]);
            Assert.AreEqual((int)(-0.4 * 3.0), ia[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValidateArrayExc()
        {
            double[] da1 = new double[0];
            DoubleArrays.ValidateArray(da1);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void testValidateArraysNullExc()
        {
            double[] da1 = null;
            double[] da2 = null;
            DoubleArrays.ValidateArrays(da1, da2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestValidateArraysArgExc()
        {
            double[] da1 = new double[2];
            double[] da2 = new double[3];
            DoubleArrays.ValidateArrays(da1, da2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNormalize16bitExc()
        {
            byte[] ba = { (-10 + 255), 45, 120 };
            DoubleArrays.Normalize16bitLittleEndian(ba);
        }

        [TestMethod]
        public void TestSerialization1D()
        {
            Serialization1D(new double[] { 0.2, -0.4, 0.6 });
            Serialization1D(new double[] { 0.256 });
            Serialization1D(new double[] { });
        }

        [TestMethod]
        public void TestSerialization2D()
        {
            Serialization2D(new double[][] { new double[] { 0.2 }, new double[] { -0.4 }, new double[] { 0.6 } });
            Serialization2D(new double[][] { new double[] { 0.2, 1 }, new double[] { -0.4, 2 }, new double[] { 0.6, 3 } });
            Serialization2D(new double[][] { new double[] { 0.2, 1 }, new double[] { -0.4 }, new double[] { 0.6, 3, 6 } });
            Serialization2D(new double[][] { new double[] { }, new double[] { }, new double[] { 0.6, 3, 6 } });
            Serialization2D(new double[][] { new double[] { }, new double[] { }, new double[] { } });
        }

        private void Serialization1D(double[] da)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "blah.foo");
            FileStream f = File.Create(tempFile);
            BinaryWriter dos = new BinaryWriter(f);
            DoubleArrays.Serialize(dos, da);
            dos.Close();
            BinaryReader dis = new BinaryReader(File.OpenRead(tempFile));
            double[] read = DoubleArrays.Deserialize(dis);
            Assert.IsTrue(DoubleArrays.ArrayEquals(da, read));
            dis.Close();
            File.Delete(tempFile);
        }

        private void Serialization2D(double[][] da)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "blah.foo");
            FileStream f = File.Create(tempFile);
            BinaryWriter dos = new BinaryWriter(f);
            DoubleArrays.Serialize(dos, da);
            dos.Close();
            BinaryReader dis = new BinaryReader(File.OpenRead(tempFile));

            double[][] read = DoubleArrays.Deserialize2d(dis);
            for (int i = 0; i < read.Length; i++)
            {
                Assert.IsTrue(DoubleArrays.ArrayEquals(da[i], read[i]));
            }
            dis.Close();
            File.Delete(tempFile);
        }
    }
}
