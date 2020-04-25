using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Tests.IO
{
    [TestClass]
    public class TestBytes
    {
        private readonly byte[] ba = { 0x7e, (byte)0xac, (byte)0x8a, (byte)0x93 };
        private readonly uint bigEndianInt = 0x7eac8a93;
        private readonly uint littleEndianInt = 0x938aac7e;


        [TestMethod]
        public void TestToByteArray()
        {
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(0x7e, 0xac, 0x8a, 0x93), ba));
        }

        [TestMethod]
        public void TestToByteArrayNegativeException()
        {
            //Not possible in c#
            //Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(-1, 0xac, 0x8a, 0x93), ba));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestToByteArrayLargeNumberException()
        {
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(256, 0xac, 0x8a, 0x93), ba));
        }


        [TestMethod]
        public void TestToInt()
        {
            Assert.AreEqual(Bytes.ToInt(ba, true), bigEndianInt);
            Assert.AreEqual(Bytes.ToInt(ba, false), littleEndianInt);
            Assert.AreEqual(Bytes.ToInt((byte)0x7e, (byte)0xac, (byte)0x8a, (byte)0x93, true),
                bigEndianInt);
            Assert.AreEqual(Bytes.ToInt((byte)0x7e, (byte)0xac, (byte)0x8a, (byte)0x93, false),
                littleEndianInt);
            Assert.AreEqual(Bytes.ToInt(new byte[] { 0x7e, (byte)0xac, (byte)0x8a }, true), (uint)0x7eac8a);
            Assert.AreEqual(Bytes.ToInt(new byte[] { 0x7e, (byte)0xac, (byte)0x8a }, false), (uint)0x8aac7e);
            Assert.AreEqual(Bytes.ToInt(new byte[] { 0x7e, (byte)0xac }, true), (uint)0x7eac);
            Assert.AreEqual(Bytes.ToInt(new byte[] { 0x7e, (byte)0xac }, false), (uint)0xac7e);
            Assert.AreEqual(Bytes.ToInt(new byte[] { 0x7e }, true), (uint)0x7e);
            Assert.AreEqual(Bytes.ToInt(new byte[] { 0x7e }, false), (uint)0x7e);
            Assert.AreEqual(Bytes.ToInt(new byte[] { 0x2f, (byte)0xff }, false), (uint)0xff2f);
        }

        [TestMethod]
        public void TestNormalize()
        {
            Assert.AreEqual(Bytes.Normalize(0xff, 8), -1);
            Assert.AreEqual(Bytes.Normalize(0x8000, 16), short.MinValue);
        }

        [TestMethod]
        public void TestToByte()
        {
            byte[] baReverse = { (byte)0x93, (byte)0x8a, (byte)0xac, 0x7e };
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(bigEndianInt, true), ba));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(bigEndianInt, false), baReverse));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(littleEndianInt, false), ba));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(littleEndianInt, true), baReverse));
        }

        [TestMethod]
        public void TestToByteShort()
        {
            byte[] baShort = { 0x43, (byte)0xac };
            byte[] baShortReverse = { (byte)0xac, 0x43 };
            ushort bigEndianShort = 0x43ac;
            ushort littleEndianShort = (ushort)0xac43;
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(bigEndianShort, true), baShort));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(bigEndianShort, false), baShortReverse));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(littleEndianShort, false), baShort));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(littleEndianShort, true), baShortReverse));
        }

        [TestMethod]
        public void TestToIntArray()
        {
            uint[] intArrBig = { 0x7eac8a93, 0x66AABBCC };
            uint[] intArrLittle = { 0x938aac7e, 0xCCBBAA66, };
            byte[] barr = {0x7e, (byte) 0xac, (byte) 0x8a, (byte) 0x93, 0x66, (byte) 0xAA, (byte) 0xBB,
        (byte) 0xCC};
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, true), intArrBig));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, false), intArrLittle));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 4, true), intArrBig));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 4, false), intArrLittle));

            barr = new byte[] { 0x7e, (byte)0xac, (byte)0x8a, (byte)0x93, 0x66, (byte)0xAA };
            intArrBig = new uint[] { 0x7eac8a, 0x9366aa };
            intArrLittle = new uint[] { 0x8aac7e, 0xaa6693 };
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 3, true), intArrBig));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 3, false), intArrLittle));

            barr = new byte[] { 0x7e, (byte)0xac, (byte)0x8a, (byte)0x93, 0x66, (byte)0xAA };
            intArrBig = new uint[] { 0x7eac, 0x8a93, 0x66aa };
            intArrLittle = new uint[] { 0xac7e, 0x938a, 0xaa66 };
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 2, true), intArrBig));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 2, false), intArrLittle));

            barr = new byte[] { 0x7e, (byte)0xac, (byte)0x8a };
            intArrBig = new uint[] { 0x7e, 0xac, 0x8a };
            intArrLittle = new uint[] { 0x7e, 0xac, 0x8a };
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 1, true), intArrBig));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToIntArray(barr, barr.Length, 1, false), intArrLittle));
        }

        [TestMethod]
        public void TestToByteArrayShort()
        {
            byte[] baBe = { 0x7e, (byte)0xac, (byte)0x8a, (byte)0x93 };
            byte[] baLe = { (byte)0xac, 0x7e, (byte)0x93, (byte)0x8a };
            ushort[] sarr = { 0x7eac, (ushort)0x8a93 };

            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr, sarr.Length, true), baBe));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr, sarr.Length, false), baLe));
        }

        [TestMethod]
        public void TestByteArray()
        {
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(0xCA, 0xFE, 0xBA, 0xBE, 0x45),
               new byte[] { (byte)0xCA, (byte)0xFE, (byte)0xBA, (byte)0xBE, 0x45 }));
        }

        [TestMethod]
        public void TestToByteArrayInt()
        {
            uint[] sarr4 = { 0xCAFEBABE, 0xDEADBEEF };
            uint[] sarr3 = { 0xCAFEBA, 0xDEADBE };
            uint[] sarr2 = { 0xCAFE, 0xDEAD };
            uint[] sarr1 = { 0xCA, 0xFE, 0xBA, 0xBE };

            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr4, sarr4.Length, 4, true),
               Bytes.ToByteArray(0xCA, 0xFE, 0xBA, 0xBE, 0xDE, 0xAD, 0xBE, 0xEF)));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr4, sarr4.Length, 4, false),
               Bytes.ToByteArray(0xBE, 0xBA, 0xFE, 0xCA, 0xEF, 0xBE, 0xAD, 0xDE)));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr3, sarr3.Length, 3, true),
               Bytes.ToByteArray(0xCA, 0xFE, 0xBA, 0xDE, 0xAD, 0xBE)));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr3, sarr3.Length, 3, false),
               Bytes.ToByteArray(0xBA, 0xFE, 0xCA, 0xBE, 0xAD, 0xDE)));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr2, sarr2.Length, 2, true),
               Bytes.ToByteArray(0xCA, 0xFE, 0xDE, 0xAD)));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr2, sarr2.Length, 2, false),
               Bytes.ToByteArray(0xFE, 0xCA, 0xAD, 0xDE)));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr1, sarr1.Length, 1, true),
               Bytes.ToByteArray(0xCA, 0xFE, 0xBA, 0xBE)));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToByteArray(sarr1, sarr1.Length, 1, false),
               Bytes.ToByteArray(0xCA, 0xFE, 0xBA, 0xBE)));
        }

        [TestMethod]
        public void TestToShort()
        {
            byte[] barr = { 0x7e, (byte)0xac, (byte)0x8a, (byte)0x93 };
            ushort[] sarrBe = { 0x7eac, (ushort)0x8a93 };
            ushort[] sarrLe = { (ushort)0xac7e, (ushort)0x938a };
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToShortArray(barr, barr.Length, true), sarrBe));
            Assert.IsTrue(Enumerable.SequenceEqual(Bytes.ToShortArray(barr, barr.Length, false), sarrLe));
        }

        [TestMethod]
        public void ToHextTest()
        {
            Assert.AreEqual(Bytes.ToHex((byte)0), "0");
            Assert.AreEqual(Bytes.ToHex((byte)1), "1");
            Assert.AreEqual(Bytes.ToHex((byte)15), "f");
            Assert.AreEqual(Bytes.ToHex((byte)127), "7f");
            Assert.AreEqual(Bytes.ToHex((byte)0xcc), "cc");
            // arrays
            Assert.AreEqual(Bytes.ToHex(new byte[] { (byte)0x01 }), "1");
            Assert.AreEqual(Bytes.ToHex(new byte[] { (byte)0xcc }), "cc");
            Assert.AreEqual(Bytes.ToHex(new byte[] { 0x00, 0x00 }), "0");
            Assert.AreEqual(Bytes.ToHex(new byte[] { 0x01, 0x1f, (byte)0xcc }), "11fcc");
            Assert.AreEqual(Bytes.ToHex(new byte[] { 0x01, 0x1f, 0x00 }), "11f00");
            Assert.AreEqual(Bytes.ToHex(new byte[] { 0x00, 0x01, 0x1f, 0x01, 0x00, 0x00 }), "11f010000");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ToHexExceptionTest()
        {
            Bytes.ToHex(null);
        }

        //    ~~~~~~~~~~~ Bytes.ToHexWithZerosWithZeros ~~~~~~~~~~~~~~
        [TestMethod]
        public void ToHexWithZerostWithZerosTest()
        {
            Assert.AreEqual(Bytes.ToHexWithZeros((byte)0), "00");
            Assert.AreEqual(Bytes.ToHexWithZeros((byte)1), "01");
            Assert.AreEqual(Bytes.ToHexWithZeros((byte)15), "0f");
            Assert.AreEqual(Bytes.ToHexWithZeros((byte)127), "7f");
            Assert.AreEqual(Bytes.ToHexWithZeros((byte)0xcc), "cc");
            // arrays
            Assert.AreEqual(Bytes.ToHexWithZeros(new byte[] { (byte)0x01 }), "01");
            Assert.AreEqual(Bytes.ToHexWithZeros(new byte[] { (byte)0xcc }), "cc");
            Assert.AreEqual(Bytes.ToHexWithZeros(new byte[] { 0x00, 0x00 }), "0000");
            Assert.AreEqual(Bytes.ToHexWithZeros(new byte[] { 0x01, 0x1f, (byte)0xcc }), "011fcc");
            Assert.AreEqual(Bytes.ToHexWithZeros(new byte[] { 0x01, 0x1f, 0x00 }), "011f00");
            Assert.AreEqual(Bytes.ToHexWithZeros(new byte[] { 0x00, 0x01, 0x1f, 0x01, 0x00, 0x00 }), "00011f010000");
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void ToHexWithZerosExceptionTest()
        {
            Bytes.ToHexWithZeros(null);
        }

        [TestMethod]
        [Ignore("Not a test")]
        public void Dump()
        {
            Bytes.HexDump(new byte[] { 0x01 }, 20);
        }
    }
}