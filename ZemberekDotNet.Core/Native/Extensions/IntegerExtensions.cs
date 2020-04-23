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
}