﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using ZemberekDotNet.Core.IO;

namespace ZemberekDotNet.Core.Enums
{
    /// <summary>
    /// This is a convenience class for enums that also are represented with strings.This classes can be
    /// useful for loading enum values from textual data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StringEnumMap<T> where T : IStringEnum
    {
        private readonly ReadOnlyDictionary<String, T> map;
        private readonly Type clazz;
        private bool ignoreCase;

        private StringEnumMap(Type clazz) : this(clazz, true)
        {

        }

        private StringEnumMap(Type clazz, bool ignoreCase)
        {
            this.clazz = clazz;
            Dictionary<String, T> mapBuilder = new Dictionary<string, T>();
            foreach (T senum in (IEnumerable<T>)clazz.GetProperty("Values", BindingFlags.Public | BindingFlags.Static).GetValue(null))
            {
                mapBuilder.Add(senum.GetStringForm(), senum);
                if (ignoreCase)
                {
                    string lowerCase = senum.GetStringForm().ToLowerInvariant();
                    if (!lowerCase.Equals(senum.GetStringForm()))
                    {
                        mapBuilder.Add(lowerCase, senum);
                    }
                }
            }
            this.map = new ReadOnlyDictionary<string, T>(mapBuilder);
            this.ignoreCase = ignoreCase;
        }

        public static StringEnumMap<T> Get()
        {
            return new StringEnumMap<T>(typeof(T));
        }

        public T GetEnum(string s)
        {
            if (Strings.IsNullOrEmpty(s))
            {
                throw new ArgumentException("Input String must have content.");
            }
            T res = map.GetValueOrDefault(s);
            if (res == null)
            {
                throw new ArgumentException(
                    "Cannot find a representation of :" + s + " for enum class:" + clazz.Name);
            }
            return res;
        }

        public ISet<string> GetKeysSet()
        {
            return map.Keys.ToHashSet();
        }

        public ICollection<T> GetEnums(ICollection<string> strings)
        {
            if (strings == null || strings.IsEmpty())
            {
                return new Collection<T>();
            }
            else
            {
                return new Collection<T>(strings.Select(e => GetEnum(e)).ToList());
            }
        }

        public bool EnumExists(String s)
        {
            return ignoreCase ? map.ContainsKey(s) || map.ContainsKey(s.ToLowerInvariant())
                : map.ContainsKey(s);
        }
    }
}
