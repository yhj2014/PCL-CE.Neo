using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class SimilaritySearch {
    private const double LengthPowerBase = 1.4;
    private const double LengthWeightOffset = 3.6;
    private const double PositionBonusFactor = 0.3;
    private const int MaxPositionBonusDistance = 3;
    private const double SourceLengthImpactFactor = 3.0;
    private const double SourceLengthSmoothing = 15.0;
    private const double ShortQueryBonusFactor = 2.0;

    private static double SearchSimilarity(string source, string query) {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(query)) {
            return 0.0;
        }

        var sourceSpan = source.ToLower().Replace(" ", "").AsSpan();
        var querySpan = query.ToLower().Replace(" ", "").AsSpan();

        if (sourceSpan.IsEmpty || querySpan.IsEmpty) {
            return 0.0;
        }

        var usedSourceIndices = new bool[sourceSpan.Length];
        double weightedLengthSum = 0;
        var queryIndex = 0;

        while (queryIndex < querySpan.Length) {
            var longestMatchLength = 0;
            var bestMatchSourceStartIndex = -1;

            for (var sourceIndex = 0; sourceIndex < sourceSpan.Length; sourceIndex++) {
                var currentMatchLength = 0;
                while ((queryIndex + currentMatchLength) < querySpan.Length &&
                       (sourceIndex + currentMatchLength) < sourceSpan.Length &&
                       !usedSourceIndices[sourceIndex + currentMatchLength] &&
                       sourceSpan[sourceIndex + currentMatchLength] == querySpan[queryIndex + currentMatchLength]) {
                    currentMatchLength++;
                }

                if (currentMatchLength <= longestMatchLength) {
                    continue;
                }

                longestMatchLength = currentMatchLength;
                bestMatchSourceStartIndex = sourceIndex;
            }

            if (longestMatchLength > 0) {
                for (var i = 0; i < longestMatchLength; i++) {
                    usedSourceIndices[bestMatchSourceStartIndex + i] = true;
                }

                var incrementWeight = Math.Pow(LengthPowerBase, 3 + longestMatchLength) - LengthWeightOffset;
                var positionDifference = Math.Abs(queryIndex - bestMatchSourceStartIndex);
                var positionBonus = 1.0 + PositionBonusFactor * Math.Max(0, MaxPositionBonusDistance - positionDifference);
                incrementWeight *= positionBonus;

                weightedLengthSum += incrementWeight;
            }

            queryIndex += Math.Max(1, longestMatchLength);
        }

        var normalizedScore = weightedLengthSum / querySpan.Length;
        var sourceLengthPenalty = SourceLengthImpactFactor / Math.Sqrt(sourceSpan.Length + SourceLengthSmoothing);
        var shortQueryBonus = query.Length == 1 ? ShortQueryBonusFactor : 1.0;

        return normalizedScore * sourceLengthPenalty * shortQueryBonus;
    }

    private static double SearchSimilarityWeighted(List<KeyValuePair<string, double>> source, string query) {
        if (source.Count == 0) return 0.0;

        var totalWeight = source.Sum(pair => pair.Value);
        if (totalWeight == 0) return 0.0;

        var weightedSum = source.Sum(pair => SearchSimilarity(pair.Key, query) * pair.Value);

        return weightedSum / totalWeight;
    }

    private static bool IsAbsoluteMatch(IEnumerable<KeyValuePair<string, double>> searchSources, string[] queryParts) {
        var processedSources = searchSources
            .Select(s => s.Key.Replace(" ", "").ToLower())
            .ToList();

        return queryParts.All(queryPart => processedSources.Any(source => source.Contains(queryPart)));
    }

    public static List<SearchEntry<T>> Search<T>(
        List<SearchEntry<T>> entries,
        string query,
        int maxBlurCount = 5,
        double minBlurSimilarity = 0.1) {
        if (entries.Count == 0) {
            return [];
        }

        if (string.IsNullOrWhiteSpace(query)) {
            return entries;
        }

        var queryParts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(q => q.ToLower())
            .ToArray();

        if (queryParts.Length == 0) {
            return entries;
        }

        foreach (var entry in entries) {
            entry.Similarity = SearchSimilarityWeighted(entry.SearchSource, query);
            entry.AbsoluteRight = IsAbsoluteMatch(entry.SearchSource, queryParts);
        }

        var sortedEntries = entries
            .OrderByDescending(e => e.AbsoluteRight)
            .ThenByDescending(e => e.Similarity);

        var sortedEntriesList = sortedEntries.ToList();
        
        var absoluteMatches = sortedEntriesList.Where(e => e.AbsoluteRight);

        var blurMatches = sortedEntriesList
            .Where(e => !e.AbsoluteRight && e.Similarity >= minBlurSimilarity)
            .Take(maxBlurCount);

        return absoluteMatches.Concat(blurMatches).ToList();
    }
}

public class SearchEntry<T>(T item, List<KeyValuePair<string, double>> searchSource) {
    public T Item { get; set; } = item;
    public List<KeyValuePair<string, double>> SearchSource { get; set; } = searchSource;
    public double Similarity { get; set; }
    public bool AbsoluteRight { get; set; }
}