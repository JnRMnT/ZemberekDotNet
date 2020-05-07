using System;


public static class DoubleExtensions
{
    /// <summary>
    /// This method is called during binary read/write operations
    /// to ensure the serialization happens with big endian
    /// </summary>
    /// <param name="value">Little endian value to convert to big endian</param>
    /// <returns>Big endian value</returns>
    public static double EnsureEndianness(this double value)
    {
        if (BitConverter.IsLittleEndian)
        {
            byte[] data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }
        else
        {
            return value;
        }
    }

    public static long ToLongBits(this double value)
    {
        return BitConverter.DoubleToInt64Bits(value);
    }
}
