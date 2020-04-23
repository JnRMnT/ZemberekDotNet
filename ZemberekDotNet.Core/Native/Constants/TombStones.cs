using System;
using System.Collections.Generic;
using System.Text;

namespace ZemberekDotNet.Core.Native.Constants
{
    /// <summary>
    /// Contains a collection of tombstones based on primitive types
    /// </summary>
    public class TombStones
    {
        public static readonly Dictionary<Type, object> TypeCollection = new Dictionary<Type, object>
        {
            { typeof(int?), int.MinValue },
            { typeof(uint?), uint.MinValue },
            { typeof(short?), short.MinValue },
            { typeof(ushort?), ushort.MinValue },
            { typeof(long?), long.MinValue },
            { typeof(ulong?), ulong.MinValue }
        };
    }
}
