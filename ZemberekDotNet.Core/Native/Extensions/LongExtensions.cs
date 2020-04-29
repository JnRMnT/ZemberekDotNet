using System;
using System.Buffers.Binary;

public static class LongExtensions
{
    public static int BitCount(this long value)
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

    /// <summary>
    /// This method is called during binary read/write operations
    /// to ensure the serialization happens with big endian
    /// </summary>
    /// <param name="value">Little endian value to convert to big endian</param>
    /// <returns>Big endian value</returns>
    public static long EnsureEndianness(this long value)
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
    public static ulong EnsureEndianness(this ulong value)
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