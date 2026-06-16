using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class RandomUtils
{
    private static readonly Random SharedRandom = Random.Shared;

    public static T PickRandom<T>(ICollection<T> collection)
    {
        if (collection.Count == 0)
            throw new ArgumentException("Collection cannot be empty", nameof(collection));
        var index = SharedRandom.Next(collection.Count);
        if (collection is IList<T> list)
            return list[index];
        return collection.Skip(index).First();
    }

    public static int NextInt(int min, int max)
    {
        return min > max ? throw new ArgumentOutOfRangeException(nameof(min), "Minimum cannot be greater than maximum") : SharedRandom.Next(min, max + 1);
    }

    public static List<T> Shuffle<T>(IList<T> list)
    {
        var result = new List<T>(list);
        var n = result.Count;
        for (var i = n - 1; i > 0; i--)
        {
            var j = SharedRandom.Next(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }

    public static void ShuffleInPlace<T>(IList<T> list)
    {
        var n = list.Count;
        for (var i = n - 1; i > 0; i--)
        {
            var j = SharedRandom.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}