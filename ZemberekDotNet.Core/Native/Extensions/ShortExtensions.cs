using System;
using System.Linq;

public static class ShortExtensions
{
    public static short ReverseBytes(this short value)
    {
        return BitConverter.ToInt16(BitConverter.GetBytes(value).Reverse().ToArray());
    }
}