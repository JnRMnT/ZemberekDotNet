using System.Collections.ObjectModel;


public static class CollectionExtensions
{
    public static bool IsEmpty<T>(this Collection<T> collection)
    {
        return collection == null || collection.Count == 0;
    }
}
