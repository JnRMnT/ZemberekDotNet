using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ZemberekDotNet.Core.Native.Constants;

namespace ZemberekDotNet.Core.Native.Helpers
{
    public class GeneralHelper
    {
        /// <summary>
        /// Handles creating a tombstone in C# as primitive types are not assignable from object!
        /// </summary>
        /// <typeparam name="T">Type of the contained items</typeparam>
        /// <returns>A tombstone object to be checked</returns>
        public static object CreateTombstone<T>()
        {
            Type type = typeof(T);
            if (type.Equals(typeof(string)))
            {
                return Guid.NewGuid().ToString();
            }
            else if (type.IsAssignableFrom(typeof(Object)))
            {
                return new object();
            }
            else if (TombStones.TypeCollection.ContainsKey(type))
            {
                return TombStones.TypeCollection.GetValueOrDefault(type);
            }
            else if (!type.IsPrimitive)
            {
                return FormatterServices.GetUninitializedObject(type);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Ensures that a given type T is a nullable type
        /// </summary>
        /// <typeparam name="T">Type to ensure nullable</typeparam>
        public static void EnsureNullable<T>()
        {
            var defaultValue = default(T);
            if (defaultValue is ValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new InvalidOperationException(
                    string.Format("Cannot instantiate with non-nullable type: {0}",
                        typeof(T)));
            }
        }
    }
}
