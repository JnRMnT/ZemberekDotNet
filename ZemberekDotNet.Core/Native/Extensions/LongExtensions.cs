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
}