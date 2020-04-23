using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class IntVectorTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            IntVector darray = new IntVector();
            Assert.AreEqual(0, darray.Size());
            Assert.AreEqual(7, darray.Capacity());
        }

        [TestMethod]
        public void TestConstructor2()
        {
            IntVector darray = new IntVector(1);
            Assert.AreEqual(0, darray.Size());
            Assert.AreEqual(1, darray.Capacity());
        }

        [TestMethod]
        public void TestAdd()
        {
            IntVector darray = new IntVector();
            for (int i = 0; i < 10000; i++)
            {
                darray.Add(i);
            }
            Assert.AreEqual(10000, darray.Size());
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual(i, darray.Get(i));
            }
        }

        [TestMethod]
        public void TestAddAll()
        {
            int[] d1 = { 2, 4, 5, 17, -1, -2, 5, -123 };
            IntVector darray = new IntVector();
            IntVector i = new IntVector(d1);
            darray.AddAll(i);
            Assert.AreEqual(i, darray);
        }

        [TestMethod]
        public void TestAddAllVector()
        {
            int[] d1 = { 2, 4, 5, 17, -1, -2, 5, -123 };
            IntVector darray = new IntVector();
            darray.AddAll(d1);
            Assert.AreEqual(8, darray.Size());
            Assert.IsTrue(Enumerable.SequenceEqual(d1, darray.CopyOf()));
            darray.AddAll(d1);
            Assert.AreEqual(16, darray.Size());
            Assert.AreEqual(2, darray.Get(0));
            Assert.AreEqual(-123, darray.Get(15));

            Assert.IsTrue(Enumerable.SequenceEqual(d1, darray.CopyOf().CopyOfRange(8, 16)));
        }


        [TestMethod]
        public void TestTrimToSize()
        {
            IntVector darray = new IntVector();
            for (int i = 0; i < 10000; i++)
            {
                darray.Add(i);
            }
            Assert.AreEqual(10000, darray.Size());
            Assert.AreNotEqual(darray.Size(), darray.Capacity());
            darray.TrimToSize();
            Assert.AreEqual(10000, darray.Size());
            Assert.AreEqual(10000, darray.CopyOf().Length);
            Assert.AreEqual(darray.Size(), darray.Capacity());
        }
    }
}
