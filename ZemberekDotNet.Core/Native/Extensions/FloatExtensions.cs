public static class FloatExtensions
{
    public static unsafe int ToIntBits(this float value)
    {
        return *((int*)&value);
    }
}