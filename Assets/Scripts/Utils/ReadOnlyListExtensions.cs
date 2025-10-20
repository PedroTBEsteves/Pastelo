using System.Collections.Generic;
using UnityEngine;

public static class ReadOnlyListExtensions
{
    public static T GetRandomElement<T>(this IReadOnlyList<T> collection)
    {
        var size = collection.Count;
        var index = Random.Range(0, size);
        Debug.Log(size);
        Debug.Log(index);
        return collection[index];
    }
}
