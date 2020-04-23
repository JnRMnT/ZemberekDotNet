using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ZemberekDotNet.Core.Collections;

namespace ZemberekDotNet.Core.Tests.Collections
{
    [TestClass]
    public class LookupSetTest
    {
        [TestMethod]
        public void AddTest()
        {
            Foo f1 = new Foo("abc", 1);
            Foo f2 = new Foo("abc", 2);

            LookupSet<Foo> fooSet = new LookupSet<Foo>();
            Assert.IsTrue(fooSet.Add(f1));
            Assert.IsFalse(fooSet.Add(f2));
        }

        [TestMethod]
        public void LookupTest()
        {
            Foo f1 = new Foo("abc", 1);
            Foo f2 = new Foo("abc", 2);

            LookupSet<Foo> fooSet = new LookupSet<Foo>();
            Assert.IsNull(fooSet.Lookup(f1));
            Assert.IsNull(fooSet.Lookup(f2));
            fooSet.Add(f1);
            Assert.AreEqual(1, fooSet.Lookup(f1).b);
            Assert.AreEqual(1, fooSet.Lookup(f2).b);
            fooSet.Add(f2);
            Assert.AreEqual(1, fooSet.Lookup(f1).b);
            Assert.AreEqual(1, fooSet.Lookup(f2).b);
        }

        [TestMethod]
        public void GetOrAddTest()
        {
            Foo f1 = new Foo("abc", 1);
            Foo f2 = new Foo("abc", 2);

            LookupSet<Foo> fooSet = new LookupSet<Foo>();
            Assert.AreEqual(1, fooSet.GetOrAdd(f1).b);
            Assert.AreEqual(1, fooSet.GetOrAdd(f2).b);
        }

        [TestMethod]
        public void RemoveTest()
        {
            Foo f1 = new Foo("abc", 1);
            Foo f2 = new Foo("abc", 2);

            LookupSet<Foo> fooSet = new LookupSet<Foo>();
            Assert.AreEqual(1, fooSet.GetOrAdd(f1).b);
            Assert.AreEqual(1, fooSet.GetOrAdd(f2).b);
            Assert.AreEqual(1, fooSet.Remove(f2).b);
            Assert.AreEqual(2, fooSet.GetOrAdd(f2).b);
        }

        internal class Foo
        {
            internal string a;
            internal int b;

            internal Foo(String a, int b)
            {
                this.a = a;
                this.b = b;
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || !GetType().Equals(o.GetType()))
                {
                    return false;
                }

                Foo foo = (Foo)o;
                return a.Equals(foo.a);
            }

            public override int GetHashCode()
            {
                return a.GetHashCode();
            }
        }
    }
}
