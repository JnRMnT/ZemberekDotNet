using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Native;

namespace ZemberekDotNet.Morphology.Morphotactics
{
    /// <summary>
    /// A class for representing a set of enums efficiently.
    /// 
    /// </p>
    /// Note: Uses ordinals as bit indexes of an int, so only works for maximum of
    /// 32 different enum values. If serialized as is, the ordinals of enums must
    /// not change.
    /// </summary>
    public class AttributeSet<E> where E : IClassEnum
    {

        private int bits;

        public AttributeSet() : this(0)
        {

        }

        private AttributeSet(int initialValue)
        {
            this.bits = initialValue;
        }

        public static AttributeSet<E> Of(params E[] enums)
        {
            AttributeSet<E> res = new AttributeSet<E>();
            foreach (E en in enums)
            {
                res.Add(en);
            }
            return res;
        }

        public static AttributeSet<E> EmptySet()
        {
            return new AttributeSet<E>();
        }

        public void CopyFrom(AttributeSet<E> other)
        {
            this.bits = other.bits;
        }

        public void Add(E en)
        {
            if (en.GetIndex() > 31)
            {
                throw new ArgumentException("Set can contain enums with max ordinal of 31.");
            }
            bits |= Mask(en);
        }

        public void Add(params E[] enums)
        {
            foreach (E en in enums)
            {
                Add(en);
            }
        }

        public void AddAll(IEnumerable<E> enums)
        {
            foreach (E en in enums)
            {
                Add(en);
            }
        }

        public void Remove(E en)
        {
            bits &= ~Mask(en);
        }

        public bool Contains(E en)
        {
            return (bits & Mask(en)) != 0;
        }

        private int Mask(E e)
        {
            return 1 << e.GetIndex();
        }

        public AttributeSet<E> Copy()
        {
            return new AttributeSet<E>(this.bits);
        }

        public override int GetHashCode()
        {
            return bits;
        }

        public override bool Equals(Object other)
        {
            if (this == other)
            {
                return true;
            }
            if (!(other is AttributeSet<E>))
            {
                return false;
            }
            return bits == ((AttributeSet<E>)other).bits;
        }

        public int GetBits()
        {
            return bits;
        }
    }
}
