using System;
using System.IO;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.IO
{
    public class IOUtil
    {
        public static BinaryReader GetDataInputStream(string path)
        {
            return new BinaryReader(File.OpenRead(path));
        }

        public static BinaryReader GetDataInputStream(Stream inputStream)
        {
            return new BinaryReader(inputStream);
        }

        public static BinaryWriter GetDataOutputStream(string path)
        {
            return new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite));
        }

        public static StreamWriter GeBufferedOutputStream(string path)
        {
            return new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite));
        }

        public static BinaryReader GetDataInputStream(string path, int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentException("Buffer size must be positive. But it is :" + bufferSize);
            }
            return new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize));
        }

        public static StreamReader GetDataOutputStream(string path, int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentException("Buffer size must be positive. But it is :" + bufferSize);
            }
            return new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize));
        }

        public static int ReadIntLe(BinaryReader dis)
        {
            return dis.ReadInt32().ReverseBytes();
        }

        public static short ReadShortLe(BinaryReader dis)
        {
            return dis.ReadInt16().ReverseBytes();
        }

        public static void WriteShortLe(TextWriter dos, short value)
        {
            dos.Write(value.ReverseBytes());
        }

        public static void WriteIntLe(TextWriter dos, int value)
        {
            dos.Write(value.ReverseBytes());
        }

        public static void CheckFileArgument(string path, string argumentInfo)
        {
            if (path == null)
            {
                throw new ArgumentException(argumentInfo + " path is null.");
            }

            FileInfo fileInfo = new FileInfo(path);
            if (!File.Exists(path))
            {
                throw new ArgumentException(argumentInfo + " file does not exist: " + fileInfo.FullName);
            }
            if (Directory.Exists(argumentInfo))
            {
                throw new ArgumentException(
                    argumentInfo + " is expected to be a file. But path is a directory");
            }
        }

        public static void CheckFileArgument(string path)
        {
            if (path == null)
            {
                throw new ArgumentException("Path is null.");
            }
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("File does not exist: " + fileInfo.FullName);
            }
            if (Directory.Exists(path))
            {
                throw new ArgumentException(
                    "Path is expected to be a file. But it is a directory: " + fileInfo.FullName);
            }
        }

        public static void CheckDirectoryArgument(string path, string argumentInfo)
        {
            if (path == null)
            {
                throw new ArgumentException(argumentInfo + " path is null.");
            }
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException(argumentInfo + " directory does not exist: " + fileInfo.FullName);
            }
            if (!Directory.Exists(argumentInfo) && File.Exists(argumentInfo))
            {
                throw new ArgumentException(
                    argumentInfo + " is expected to be a directory. But path is a file.");
            }
        }

        public static void CheckDirectoryArgument(string path)
        {
            if (path == null)
            {
                throw new ArgumentException("Path is null.");
            }
            FileInfo fileInfo = new FileInfo(path);
            string fullPath = Path.GetFullPath(path);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("Directory does not exist: " + fullPath);
            }
            if (!Directory.Exists(path))
            {
                throw new ArgumentException(
                    "Path is expected to be a directory. But it is a file: " + fullPath);
            }
        }

        public static void DeleteTempDir(string tempDir)
        {
            string tmpRoot = Path.GetTempPath();
            if (!Path.GetFullPath(tempDir).StartsWith(tmpRoot))
            {
                Log.Info(
                    "Only directories within temporary system dir {0} are allowed to be deleted recursively. But : {1}",
                    tmpRoot,
                    Path.GetFullPath(tempDir));
                return;
            }
            Directory.Delete(tempDir, true);
        }
    }
}
