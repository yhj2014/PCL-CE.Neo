using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class SimilaritySearch
{
    public static List<T> Search<T>(List<T> items, string query, Func<T, string> textExtractor, int topN = 10, double threshold = 0.3)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || items == null || items.Count == 0)
                return new List<T>();

            var results = items.Select(item =>
            {
                var text = textExtractor(item);
                var similarity = CalculateSimilarity(query.ToLower(), text.ToLower());
                return new { Item = item, Similarity = similarity };
            })
            .Where(r => r.Similarity >= threshold)
            .OrderByDescending(r => r.Similarity)
            .Take(topN)
            .Select(r => r.Item)
            .ToList();

            return results;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Similarity search failed");
            return new List<T>();
        }
    }

    public static List<T> FuzzySearch<T>(List<T> items, string query, Func<T, string> textExtractor, int maxEdits = 2)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || items == null || items.Count == 0)
                return new List<T>();

            var results = items.Select(item =>
            {
                var text = textExtractor(item);
                var distance = LevenshteinDistance(query.ToLower(), text.ToLower());
                return new { Item = item, Distance = distance };
            })
            .Where(r => r.Distance <= maxEdits)
            .OrderBy(r => r.Distance)
            .Select(r => r.Item)
            .ToList();

            return results;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Fuzzy search failed");
            return new List<T>();
        }
    }

    public static double CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
            return 1.0;

        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        var distance = LevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        
        return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
    }

    public static int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
            return s2.Length;

        if (string.IsNullOrEmpty(s2))
            return s1.Length;

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }

    public static double JaccardSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
            return 1.0;

        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        var set1 = s1.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var set2 = s2.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        if (set1.Count == 0 && set2.Count == 0)
            return 1.0;

        var intersection = set1.Intersect(set2).Count();
        var union = set1.Union(set2).Count();

        return union == 0 ? 0.0 : (double)intersection / union;
    }
}