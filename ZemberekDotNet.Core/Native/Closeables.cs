using System.IO;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.Native
{
    public class Closeables
    {
        public static void Close(BinaryReader closeable, bool swallowIOException)
        {
            if (closeable == null)
            {
                return;
            }
            try
            {
                closeable.Close();
            }
            catch (IOException e)
            {
                if (swallowIOException)
                {
                    Log.Warn("IOException thrown while closing Closeable.", e);
                }
                else
                {
                    throw e;
                }
            }
        }
        public static void Close(BinaryWriter closeable, bool swallowIOException)
        {
            if (closeable == null)
            {
                return;
            }
            try
            {
                closeable.Close();
            }
            catch (IOException e)
            {
                if (swallowIOException)
                {
                    Log.Warn("IOException thrown while closing Closeable.", e);
                }
                else
                {
                    throw e;
                }
            }
        }
    }
}
