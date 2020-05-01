using System;

public static class FloatExtensions
{
    public static unsafe int ToIntBits(this float value)
    {
        return *((int*)&value);
    }

    /// <summary>
    /// This method is called during binary read/write operations
    /// to ensure the serialization happens with big endian
    /// </summary>
    /// <param name="value">Little endian value to convert to big endian</param>
    /// <returns>Big endian value</returns>
    public static float EnsureEndianness(this float value)
    {
        if (BitConverter.IsLittleEndian)
        {
            byte[] data = BitConverter.GetBytes(value);
            // other-endian; reverse this portion of the data (4 bytes)
            byte tmp = data[0];
            data[0] = data[0 + 3];
            data[3] = tmp;
            tmp = data[1];
            data[1] = data[2];
            data[2] = tmp;
            return BitConverter.ToSingle(data);
        }
        else
        {
            return value;
        }
    }
}