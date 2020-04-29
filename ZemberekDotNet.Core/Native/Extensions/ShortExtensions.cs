using System;
using System.Buffers.Binary;

public static class ShortExtensions
{
    public static short ReverseBytes(this short value)
    {
        return BinaryPrimitives.ReverseEndianness(value);
    }
    
    /// <summary>
    /// This method is called during binary read/write operations
    /// to ensure the serialization happens with big endian
    /// </summary>
    /// <param name="value">Little endian value to convert to big endian</param>
    /// <returns>Big endian value</returns>
    public static short EnsureEndianness(this short value)
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
    public static ushort EnsureEndianness(this ushort value)
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