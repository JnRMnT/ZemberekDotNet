using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace ZemberekDotNet.Core.Native.Helpers
{
    public class SizeHelper
    {
        private static readonly Dictionary<Type, int> sizes = new Dictionary<Type, int>();
        public static int SizeOf(Type type)
        {
            int size;
            if (sizes.TryGetValue(type, out size))
            {
                return size;
            }

            size = SizeOfType(type);
            sizes.Add(type, size);
            return size;
        }

        private static int SizeOfType(Type type)
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, type);
            il.Emit(OpCodes.Ret);
            return (int)dm.Invoke(null, null);
        }
    }
}
