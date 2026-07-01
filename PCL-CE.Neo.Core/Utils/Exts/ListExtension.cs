namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ListExtension
{
    public static bool IsNullOrEmpty<T>(this List<T>? list)
    {
        return list == null || list.Count == 0;
    }

    public static bool IsNotNullOrEmpty<T>(this List<T>? list)
    {
        return list != null && list.Count > 0;
    }

    public static T? FirstOrNull<T>(this List<T> list) where T : struct
    {
        if (list.Count == 0)
            return null;

        return list[0];
    }

    public static T? LastOrNull<T>(this List<T> list) where T : struct
    {
        if (list.Count == 0)
            return null;

        return list[list.Count - 1];
    }

    public static T? GetOrNull<T>(this List<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
            return default;

        return list[index];
    }

    public static T? TryGet<T>(this List<T> list, int index)
    {
        if (index < 0 || index >= list.Count)
            return default;

        return list[index];
    }

    public static bool TryGet<T>(this List<T> list, int index, out T? value)
    {
        if (index < 0 || index >= list.Count)
        {
            value = default;
            return false;
        }

        value = list[index];
        return true;
    }

    public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T>? items)
    {
        if (items != null)
            list.AddRange(items);
    }

    public static void AddIfNotNull<T>(this List<T> list, T? item) where T : class
    {
        if (item != null)
            list.Add(item);
    }

    public static void AddIfNotNull<T>(this List<T?> list, T? item) where T : struct
    {
        if (item.HasValue)
            list.Add(item.Value);
    }

    public static void RemoveAllNulls<T>(this List<T?> list) where T : class
    {
        list.RemoveAll(item => item == null);
    }

    public static void RemoveAllNulls<T>(this List<T?> list) where T : struct
    {
        list.RemoveAll(item => !item.HasValue);
    }

    public static List<T> WhereNotNull<T>(this List<T?> list) where T : class
    {
        return list.Where(item => item != null).Select(item => item!).ToList();
    }

    public static List<T> WhereNotNull<T>(this List<T?> list) where T : struct
    {
        return list.Where(item => item.HasValue).Select(item => item.Value).ToList();
    }

    public static T[] ToArrayIfNotNull<T>(this List<T>? list)
    {
        return list?.ToArray() ?? Array.Empty<T>();
    }

    public static List<T> Clone<T>(this List<T> list) where T : ICloneable
    {
        return list.Select(item => (T)item.Clone()).ToList();
    }

    public static List<T> DeepClone<T>(this List<T> list) where T : ICloneable
    {
        return list.Select(item => (T)item.Clone()).ToList();
    }

    public static List<T> Shuffle<T>(this List<T> list)
    {
        Random random = new Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    public static T? RandomItem<T>(this List<T> list)
    {
        if (list.Count == 0)
            return default;

        Random random = new Random();
        return list[random.Next(list.Count)];
    }

    public static List<T> DistinctBy<T, TKey>(this List<T> list, Func<T, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new HashSet<TKey>();
        List<T> result = new List<T>();

        foreach (T item in list)
        {
            TKey key = keySelector(item);
            if (seenKeys.Add(key))
                result.Add(item);
        }

        return result;
    }

    public static List<T> UnionWith<T>(this List<T> list, IEnumerable<T> other)
    {
        foreach (T item in other)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        return list;
    }

    public static List<T> IntersectWith<T>(this List<T> list, IEnumerable<T> other)
    {
        HashSet<T> otherSet = new HashSet<T>(other);
        return list.Where(item => otherSet.Contains(item)).ToList();
    }

    public static List<T> ExceptWith<T>(this List<T> list, IEnumerable<T> other)
    {
        HashSet<T> otherSet = new HashSet<T>(other);
        return list.Where(item => !otherSet.Contains(item)).ToList();
    }

    public static bool ContainsAny<T>(this List<T> list, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            if (list.Contains(item))
                return true;
        }

        return false;
    }

    public static bool ContainsAll<T>(this List<T> list, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            if (!list.Contains(item))
                return false;
        }

        return true;
    }

    public static int IndexOf<T>(this List<T> list, Func<T, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
                return i;
        }

        return -1;
    }

    public static int LastIndexOf<T>(this List<T> list, Func<T, bool> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate(list[i]))
                return i;
        }

        return -1;
    }

    public static List<T> GetRangeSafe<T>(this List<T> list, int index, int count)
    {
        if (index < 0)
            index = 0;

        if (index >= list.Count)
            return new List<T>();

        if (count < 0)
            count = 0;

        if (index + count > list.Count)
            count = list.Count - index;

        return list.GetRange(index, count);
    }

    public static void InsertRangeSafe<T>(this List<T> list, int index, IEnumerable<T> items)
    {
        if (index < 0)
            index = 0;

        if (index > list.Count)
            index = list.Count;

        list.InsertRange(index, items);
    }

    public static void RemoveRangeSafe<T>(this List<T> list, int index, int count)
    {
        if (index < 0)
            index = 0;

        if (index >= list.Count)
            return;

        if (count < 0)
            count = 0;

        if (index + count > list.Count)
            count = list.Count - index;

        list.RemoveRange(index, count);
    }

    public static List<T> SplitAndTake<T>(this List<T> list, int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");

        List<T> result = new List<T>(size);
        for (int i = 0; i < Math.Min(size, list.Count); i++)
        {
            result.Add(list[i]);
        }

        return result;
    }

    public static List<List<T>> SplitIntoChunks<T>(this List<T> list, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");

        List<List<T>> chunks = new List<List<T>>();
        for (int i = 0; i < list.Count; i += chunkSize)
        {
            chunks.Add(list.GetRange(i, Math.Min(chunkSize, list.Count - i)));
        }

        return chunks;
    }

    public static List<T> Flatten<T>(this List<List<T>> list)
    {
        List<T> result = new List<T>();
        foreach (List<T> sublist in list)
        {
            result.AddRange(sublist);
        }

        return result;
    }

    public static List<TResult> ConvertAllSafe<T, TResult>(this List<T> list, Func<T, TResult> converter)
    {
        List<TResult> result = new List<TResult>(list.Count);
        foreach (T item in list)
        {
            try
            {
                result.Add(converter(item));
            }
            catch
            {
            }
        }

        return result;
    }

    public static List<T> SortBy<T, TKey>(this List<T> list, Func<T, TKey> keySelector)
    {
        list.Sort((a, b) => Comparer<TKey>.Default.Compare(keySelector(a), keySelector(b)));
        return list;
    }

    public static List<T> SortByDescending<T, TKey>(this List<T> list, Func<T, TKey> keySelector)
    {
        list.Sort((a, b) => Comparer<TKey>.Default.Compare(keySelector(b), keySelector(a)));
        return list;
    }

    public static T? MaxBy<T, TKey>(this List<T> list, Func<T, TKey> keySelector) where TKey : IComparable<TKey>
    {
        if (list.Count == 0)
            return default;

        T maxItem = list[0];
        TKey maxKey = keySelector(maxItem);

        for (int i = 1; i < list.Count; i++)
        {
            TKey currentKey = keySelector(list[i]);
            if (currentKey.CompareTo(maxKey) > 0)
            {
                maxKey = currentKey;
                maxItem = list[i];
            }
        }

        return maxItem;
    }

    public static T? MinBy<T, TKey>(this List<T> list, Func<T, TKey> keySelector) where TKey : IComparable<TKey>
    {
        if (list.Count == 0)
            return default;

        T minItem = list[0];
        TKey minKey = keySelector(minItem);

        for (int i = 1; i < list.Count; i++)
        {
            TKey currentKey = keySelector(list[i]);
            if (currentKey.CompareTo(minKey) < 0)
            {
                minKey = currentKey;
                minItem = list[i];
            }
        }

        return minItem;
    }
}