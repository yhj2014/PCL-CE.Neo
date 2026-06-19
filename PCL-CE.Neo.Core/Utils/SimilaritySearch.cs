namespace PCL_CE.Neo.Core.Utils;

public static class SimilaritySearch
{
    public static int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
            return s2?.Length ?? 0;
        if (string.IsNullOrEmpty(s2))
            return s1.Length;

        var dp = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            dp[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++)
            dp[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }

        return dp[s1.Length, s2.Length];
    }

    public static double Similarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
            return 1.0;
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        var distance = LevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return 1.0 - (double)distance / maxLength;
    }

    public static T? FindMostSimilar<T>(string input, IEnumerable<T> items, Func<T, string> keySelector, double threshold = 0.5)
    {
        if (string.IsNullOrEmpty(input) || items == null)
            return default;

        T? bestMatch = default;
        double bestScore = threshold;

        foreach (var item in items)
        {
            var key = keySelector(item);
            var score = Similarity(input, key);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = item;
            }
        }

        return bestMatch;
    }

    public static List<T> FindSimilar<T>(string input, IEnumerable<T> items, Func<T, string> keySelector, double threshold = 0.5)
    {
        if (string.IsNullOrEmpty(input) || items == null)
            return new List<T>();

        return items
            .Select(item => new { Item = item, Score = Similarity(input, keySelector(item)) })
            .Where(x => x.Score >= threshold)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item)
            .ToList();
    }
}