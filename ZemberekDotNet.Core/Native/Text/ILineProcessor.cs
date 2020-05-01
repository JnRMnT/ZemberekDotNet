namespace ZemberekDotNet.Core.Native.Text
{
    public interface ILineProcessor<T>
    {
        bool ProcessLine(string line);

        /** Return the result of processing all the lines. */
        T GetResult();
    }
}
