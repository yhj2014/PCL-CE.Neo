using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class SimilaritySearch
{
    public static int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
            return string.IsNullOrEmpty(s2) ? 0 : s2.Length;

        if (string.IsNullOrEmpty(s2))
            return s1.Length;

        var costs = new int[s2.Length + 1];

        for (int i = 0; i <= s2.Length; i++)
            costs[i] = i;

        for (int i = 1; i <= s1.Length; i++)
        {
            costs[0] = i;
            int last = i - 1;

            for (int j = 1; j <= s2.Length; j++)
            {
                int temp = costs[j];
                costs[j] = Math.Min(Math.Min(costs[j] + 1, costs[j - 1] + 1), 
                    last + (s1[i - 1] == s2[j - 1] ? 0 : 1));
                last = temp;
            }
        }

        return costs[s2.Length];
    }

    public static double LevenshteinSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
            return 1.0;

        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        int distance = LevenshteinDistance(s1, s2);
        int maxLength = Math.Max(s1.Length, s2.Length);

        return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
    }

    public static int HammingDistance(string s1, string s2)
    {
        if (s1 == null)
            throw new ArgumentNullException(nameof(s1));
        if (s2 == null)
            throw new ArgumentNullException(nameof(s2));

        if (s1.Length != s2.Length)
            throw new ArgumentException("Strings must be of equal length");

        int distance = 0;
        for (int i = 0; i < s1.Length; i++)
        {
            if (s1[i] != s2[i])
                distance++;
        }

        return distance;
    }

    public static double HammingSimilarity(string s1, string s2)
    {
        if (s1 == null)
            throw new ArgumentNullException(nameof(s1));
        if (s2 == null)
            throw new ArgumentNullException(nameof(s2));

        if (s1.Length != s2.Length)
            return 0.0;

        if (s1.Length == 0)
            return 1.0;

        int distance = HammingDistance(s1, s2);
        return 1.0 - (double)distance / s1.Length;
    }

    public static double JaccardSimilarity(string s1, string s2, int nGramSize = 2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        var nGrams1 = GetNGramSet(s1, nGramSize);
        var nGrams2 = GetNGramSet(s2, nGramSize);

        if (nGrams1.Count == 0 && nGrams2.Count == 0)
            return 1.0;

        if (nGrams1.Count == 0 || nGrams2.Count == 0)
            return 0.0;

        int intersection = nGrams1.Intersect(nGrams2).Count();
        int union = nGrams1.Union(nGrams2).Count();

        return (double)intersection / union;
    }

    private static HashSet<string> GetNGramSet(string s, int nGramSize)
    {
        var nGrams = new HashSet<string>();

        for (int i = 0; i <= s.Length - nGramSize; i++)
        {
            nGrams.Add(s.Substring(i, nGramSize));
        }

        return nGrams;
    }

    public static double CosineSimilarity(string s1, string s2, int nGramSize = 2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        var nGrams1 = GetNGramFrequency(s1, nGramSize);
        var nGrams2 = GetNGramFrequency(s2, nGramSize);

        if (nGrams1.Count == 0 || nGrams2.Count == 0)
            return 0.0;

        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        foreach (var key in nGrams1.Keys.Union(nGrams2.Keys))
        {
            double freq1 = nGrams1.TryGetValue(key, out var f1) ? f1 : 0;
            double freq2 = nGrams2.TryGetValue(key, out var f2) ? f2 : 0;

            dotProduct += freq1 * freq2;
            magnitude1 += freq1 * freq1;
            magnitude2 += freq2 * freq2;
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0.0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    private static Dictionary<string, int> GetNGramFrequency(string s, int nGramSize)
    {
        var frequencies = new Dictionary<string, int>();

        for (int i = 0; i <= s.Length - nGramSize; i++)
        {
            var nGram = s.Substring(i, nGramSize);
            frequencies[nGram] = frequencies.TryGetValue(nGram, out var count) ? count + 1 : 1;
        }

        return frequencies;
    }

    public static IEnumerable<T> FindSimilar<T>(T target, IEnumerable<T> candidates, 
        Func<T, string> stringSelector, double threshold = 0.5, int topN = 5)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (candidates == null)
            throw new ArgumentNullException(nameof(candidates));
        if (stringSelector == null)
            throw new ArgumentNullException(nameof(stringSelector));

        var targetString = stringSelector(target);

        return candidates
            .Select(c => new { Item = c, Similarity = LevenshteinSimilarity(targetString, stringSelector(c)) })
            .Where(x => x.Similarity >= threshold)
            .OrderByDescending(x => x.Similarity)
            .Take(topN)
            .Select(x => x.Item);
    }

    public static IEnumerable<string> FindSimilar(string target, IEnumerable<string> candidates, 
        double threshold = 0.5, int topN = 5)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (candidates == null)
            throw new ArgumentNullException(nameof(candidates));

        return candidates
            .Select(c => new { Item = c, Similarity = LevenshteinSimilarity(target, c) })
            .Where(x => x.Similarity >= threshold)
            .OrderByDescending(x => x.Similarity)
            .Take(topN)
            .Select(x => x.Item);
    }

    public static T FindMostSimilar<T>(T target, IEnumerable<T> candidates, Func<T, string> stringSelector)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (candidates == null)
            throw new ArgumentNullException(nameof(candidates));
        if (stringSelector == null)
            throw new ArgumentNullException(nameof(stringSelector));

        var targetString = stringSelector(target);

        var result = candidates
            .Select(c => new { Item = c, Similarity = LevenshteinSimilarity(targetString, stringSelector(c)) })
            .OrderByDescending(x => x.Similarity)
            .FirstOrDefault();

        return result != null ? result.Item : default!;
    }

    public static string? FindMostSimilar(string target, IEnumerable<string> candidates)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (candidates == null)
            throw new ArgumentNullException(nameof(candidates));

        return candidates
            .Select(c => new { Item = c, Similarity = LevenshteinSimilarity(target, c) })
            .OrderByDescending(x => x.Similarity)
            .FirstOrDefault()?.Item;
    }
}