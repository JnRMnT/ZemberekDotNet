using System;
using System.Collections.Generic;

public static class CollectionExtensions
{
    public static bool IsEmpty<T>(this ICollection<T> collection)
    {
        return collection == null || collection.Count == 0;
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> collectionToAdd)
    {
        foreach (T item in collectionToAdd)
        {
            collection.Add(item);
        }
    }

    public static void Shuffle<T>(this IList<T> collection)
    {
        collection.Shuffle(new Random());
    }

    public static void Shuffle<T>(this IList<T> collection, Random rnd)
    {
        int n = collection.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = collection[k];
            collection[k] = collection[n];
            collection[n] = value;
        }
    }

    public static void Remove<T>(this ICollection<T> collection, IEnumerable<T> itemsToRemove)
    {
        if(collection != null && itemsToRemove != null)
        {
            foreach(T itemToRemove in itemsToRemove)
            {
                collection.Remove(itemToRemove);
            }
        }
    }
}
