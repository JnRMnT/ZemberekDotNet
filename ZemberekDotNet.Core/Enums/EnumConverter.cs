using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.Enums
{
    public class EnumConverter<E, P>
    {
        private static Type _originalNameAttributeType;
        private static Type originalNameAttributeType
        {
            get
            {
                if (_originalNameAttributeType == null)
                {
                    _originalNameAttributeType = Type.GetType("Google.Protobuf.Reflection.OriginalNameAttribute, Google.Protobuf");
                }
                return _originalNameAttributeType;
            }
        }

        private static PropertyInfo _originalNamePropertyInfo;
        private static PropertyInfo originalNamePropertyInfo
        {
            get
            {
                if (_originalNamePropertyInfo == null)
                {
                    _originalNamePropertyInfo = originalNameAttributeType.GetProperty("Name");
                }
                return _originalNamePropertyInfo;
            }
        }

        readonly Dictionary<string, P> conversionFromEToP;
        readonly Dictionary<string, E> conversionFromPToE;

        private EnumConverter(
            Dictionary<string, P> conversionFromEToP,
            Dictionary<string, E> conversionFromPToE)
        {
            this.conversionFromEToP = conversionFromEToP;
            this.conversionFromPToE = conversionFromPToE;
        }

        public static EnumConverter<E, P> CreateConverter()
        {
            Dictionary<string, E> namesMapE = CreateEnumNameMap<E>();
            Dictionary<string, P> namesMapP = CreateEnumNameMap<P>();

            Dictionary<string, P> conversionFromEToP = new Dictionary<string, P>();
            Dictionary<string, E> conversionFromPToE = new Dictionary<string, E>();
            foreach (var entry in namesMapE)
            {
                if (namesMapP.ContainsKey(entry.Key))
                {
                    conversionFromEToP.TryAdd(entry.Key, namesMapP.GetValueOrDefault(entry.Key));
                }
            }
            foreach (var entry in namesMapP)
            {
                if (namesMapP.ContainsKey(entry.Key))
                {
                    conversionFromPToE.TryAdd(entry.Key, namesMapE.GetValueOrDefault(entry.Key));
                }
            }
            return new EnumConverter<E, P>(conversionFromEToP, conversionFromPToE);
        }

        private static Dictionary<string, T> CreateEnumNameMap<T>()
        {
            Type type = typeof(T);
            Dictionary<string, T> nameToEnum = new Dictionary<string, T>();
            if (type.IsEnum)
            {
                // put Enums in map by name
                foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    Attribute originalNameAttribute = field.GetCustomAttribute(originalNameAttributeType);
                    string originalName = (string)originalNamePropertyInfo.GetValue(originalNameAttribute);
                    nameToEnum.Add(originalName, (T)field.GetValue(null));
                }
            }
            else if (typeof(IStringEnum).IsAssignableFrom(type))
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (FieldInfo field in fields)
                {
                    nameToEnum.Add(field.Name, (T)field.GetValue(null));
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            return nameToEnum;
        }

        public P ConvertTo(E en, P defaultEnum)
        {
            P pEnum = conversionFromEToP.GetValueOrDefault(GetName(en));
            if (pEnum == null)
            {
                Log.Warn("Could not map from Enum {0} Returning default", en);
                return defaultEnum;
            }
            return pEnum;
        }

        public E ConvertBack(P en, E defaultEnum)
        {
            E eEnum = conversionFromPToE.GetValueOrDefault(GetName(en));
            if (eEnum == null)
            {
                Log.Warn("Could not map from Enum {0} Returning default", en);
                return defaultEnum;
            }
            return eEnum;
        }

        private string GetName<T>(T enumValue)
        {
            Type type = typeof(T);
            if (type.IsEnum)
            {
                FieldInfo declaredField = type.GetFields(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(e => EqualityComparer<T>.Default.Equals((T)e.GetValue(null), enumValue));
                Attribute originalNameAttribute = declaredField.GetCustomAttribute(originalNameAttributeType);
                string originalName = (string)originalNamePropertyInfo.GetValue(originalNameAttribute);
                return originalName;
            }
            else if (typeof(IStringEnum).IsAssignableFrom(type))
            {
                return ((IStringEnum)enumValue).GetStringForm();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
