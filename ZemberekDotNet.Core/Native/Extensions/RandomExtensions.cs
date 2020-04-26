using System;

public static class RandomExtensions
{
    public static float NextFloat(this Random prng)
    {
        var sign = prng.Next(2);
        var exponent = prng.Next((1 << 8) - 1); // do not generate 0xFF (infinities and NaN)
        var mantissa = prng.Next(1 << 23);

        var bits = (sign << 31) + (exponent << 23) + mantissa;
        return bits.ToFloatFromBits();
    }
}
