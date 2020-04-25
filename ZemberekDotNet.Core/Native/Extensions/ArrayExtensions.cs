using System;
using ZemberekDotNet.Core.Native.Helpers;

public static class ArrayExtensions
{
    public static T[] CopyOfRange<T>(this T[] original, int from, int to)
    {
        int newLength = to - from;
        if (newLength < 0)
            throw new ArgumentException(from + " > " + to);
        T[] copy = new T[newLength];
        Array.Copy(original, from, copy, 0, Math.Min(original.Length - from, newLength));
        return copy;
    }


    public static T[] CopyOf<T>(this T[] original, int newLength)
    {
        T[] copy = new T[newLength];
        Array.Copy(original, 0, copy, 0, Math.Min(original.Length, newLength));
        return copy;
    }

    public static byte[] ToByteArray<T>(this T[] array)
    {
        byte[] result = new byte[array.Length * SizeHelper.SizeOf(typeof(T))];
        Buffer.BlockCopy(array, 0, result, 0, result.Length);
        return result;
    }
}