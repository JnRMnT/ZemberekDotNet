using System;
using System.Collections.Generic;
using ZemberekDotNet.Core.Logging;

namespace ZemberekDotNet.Core.Enums
{
    public class EnumConverter<E, P> where E : Enum where P : Enum
    {
        Dictionary<string, P> conversionFromEToP;
        Dictionary<string, E> conversionFromPToE;

        private EnumConverter(
            Dictionary<string, P> conversionFromEToP,
            Dictionary<string, E> conversionFromPToE)
        {
            this.conversionFromEToP = conversionFromEToP;
            this.conversionFromPToE = conversionFromPToE;
        }

        public static EnumConverter<E, P> CreateConverter(Type enumType, Type otherEnumType)
        {
            Dictionary<string, E> namesMapE = CreateEnumNameMap<E>(enumType);
            Dictionary<string, P> namesMapP = CreateEnumNameMap<P>(otherEnumType);

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

        private static Dictionary<string, T> CreateEnumNameMap<T>(Type enumType)
        {
            Dictionary<string, T> nameToEnum = new Dictionary<string, T>();
            // put Enums in map by name
            foreach (string enumElement in Enum.GetNames(typeof(T)))
            {
                nameToEnum.Add(enumElement, (T)Enum.Parse(enumType, enumElement));
            }
            return nameToEnum;
        }

        public P ConvertTo(E en, P defaultEnum)
        {
            P pEnum = conversionFromEToP.GetValueOrDefault(en.ToString());
            if (pEnum == null)
            {
                Log.Warn("Could not map from Enum %s Returning default", en);
                return defaultEnum;
            }
            return pEnum;
        }

        public E ConvertBack(P en, E defaultEnum)
        {
            E eEnum = conversionFromPToE.GetValueOrDefault(en.ToString());
            if (eEnum == null)
            {
                Log.Warn("Could not map from Enum %s Returning default", en);
                return defaultEnum;
            }
            return eEnum;
        }
    }
}
