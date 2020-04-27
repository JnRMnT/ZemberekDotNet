using System.Collections.Generic;
public static class CollectionExtensions
{
    public static bool IsEmpty<T>(this ICollection<T> collection)
    {
        return collection == null || collection.Count == 0;
    }

    public static void AddRange<T>(this ICollection<T> collection, ICollection<T> collectionToAdd)
    {
        foreach(T item in collectionToAdd)
        {
            collection.Add(item);
        }
    }
}
