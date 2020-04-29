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
}