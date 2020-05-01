using System.Text;

namespace ZemberekDotNet.Core.Native
{
    public class Arrays
    {
        public static string ToString<T>(T[] a)
        {
            if (a == null)
                return "null";
            int iMax = a.Length - 1;
            if (iMax == -1)
                return "[]";

            StringBuilder b = new StringBuilder();
            b.Append('[');
            for (int i = 0; ; i++)
            {
                b.Append(a[i]);
                if (i == iMax)
                    return b.Append(']').ToString();
                b.Append(", ");
            }
        }
    }
}
