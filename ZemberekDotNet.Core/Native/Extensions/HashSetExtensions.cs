using System.Collections.Generic;
public static class HashSetExtensions
{
    public static HashSet<T> Clone<T>(this HashSet<T> collection)
    {
        return new HashSet<T>(collection, collection.Comparer);
    }
}