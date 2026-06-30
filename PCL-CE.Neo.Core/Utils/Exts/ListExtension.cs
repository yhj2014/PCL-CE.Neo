using System;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ListUtils
{
    extension<T>(IEnumerable<T> source)
    {
        public T? MaxOrDefault<C>(Func<T, C> selector) where C : IComparable<C>
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext()) return default;
            var maxItem = enumerator.Current;
            var maxValue = selector(maxItem);
            while (enumerator.MoveNext())
            {
                var value = selector(enumerator.Current);
                if (value.CompareTo(maxValue) <= 0) continue;
                maxItem = enumerator.Current;
                maxValue = value;
            }
            return maxItem;
        }

        public T? MinOrDefault<C>(Func<T, C> selector) where C : IComparable<C>
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext()) return default;
            var minItem = enumerator.Current;
            var minValue = selector(minItem);
            while (enumerator.MoveNext())
            {
                var value = selector(enumerator.Current);
                if (value.CompareTo(minValue) >= 0) continue;
                minItem = enumerator.Current;
                minValue = value;
            }
            return minItem;
        }
    }
}

public static class SortUtils
{
    public static List<T> Sort<T>(this IList<T> list, Func<T, T, bool> comparison)
    {
        var result = new List<T>(list);
        result.Sort(new StableComparer<T>(comparison));
        return result;
    }

    private class StableComparer<T>(Func<T, T, bool> comparison) : IComparer<T>
    {
        private readonly Func<T, T, bool> _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));

        public int Compare(T? x, T? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            var xComesFirst = _comparison(x, y);
            var yComesFirst = _comparison(y, x);

            if (!xComesFirst && !yComesFirst) return 0;
            return xComesFirst ? -1 : 1;
        }
    }
}