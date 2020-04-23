namespace ZemberekDotNet.Core.IO
{
    public interface IFilter<T>
    {
        bool CanPass(T t);
    }
}
