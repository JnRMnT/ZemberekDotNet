using System;
using System.Buffers.Binary;
using System.Linq;

public static class IntegerExtensions
{
    public static int BitCount(this int value)
    {
        //Sparse Bit Count
        int count = 0;
        while (value != 0)
        {
            count++;
            value &= (value - 1);
        }
        return count;
    }

    public static unsafe float ToFloatFromBits(this int value)
    {
        return *(float*)(&value);
    }

    public static int ReverseBytes(this int value)
    {
        return BinaryPrimitives.ReverseEndianness(value);
    }

    /// <summary>
    /// This method is called during binary read/write operations
    /// to ensure the serialization happens with big endian
    /// </summary>
    /// <param name="value">Little endian value to convert to big endian</param>
    /// <returns>Big endian value</returns>
    public static int EnsureEndianness(this int value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }
        else
        {
            return value;
        }
    }

    /// <summary>
    /// This method is called during binary read/write operations
    /// to ensure the serialization happens with big endian
    /// </summary>
    /// <param name="value">Little endian value to convert to big endian</param>
    /// <returns>Big endian value</returns>
    public static uint EnsureEndianness(this uint value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }
        else
        {
            return value;
        }
    }

    public static int NumberOfLeadingZeros(this int i)
    {
        // HD, Figure 5-6
        if (i == 0)
            return 32;
        int n = 1;
        if (i >> 16 == 0) { n += 16; i <<= 16; }
        if (i >> 24 == 0) { n += 8; i <<= 8; }
        if (i >> 28 == 0) { n += 4; i <<= 4; }
        if (i >> 30 == 0) { n += 2; i <<= 2; }
        n -= i >> 31;
        return n;
    }
}