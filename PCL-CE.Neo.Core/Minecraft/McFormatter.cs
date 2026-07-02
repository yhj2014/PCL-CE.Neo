namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Minecraft 版本格式化工具，用于将版本 ID 转换为友好名称或 Wiki URL 后缀
/// </summary>
public static class McFormatter
{
    /// <summary>
    /// 获取 Minecraft Wiki URL 后缀
    /// </summary>
    /// <param name="gameVersion">游戏版本 ID</param>
    /// <returns>Wiki URL 后缀</returns>
    public static string GetWikiUrlSuffix(string gameVersion)
    {
        if (string.IsNullOrEmpty(gameVersion))
            return string.Empty;

        var formattedVersion = FormatVersion(gameVersion);

        // 快照版本（包含 'w'）直接返回格式化版本
        if (gameVersion.Contains('w'))
            return formattedVersion;

        // 正式版本添加 "Java版" 前缀
        return "Java版" + formattedVersion;
    }

    /// <summary>
    /// 格式化版本 ID 为友好名称
    /// </summary>
    /// <param name="gameVersion">游戏版本 ID</param>
    /// <returns>格式化后的版本名称</returns>
    public static string FormatVersion(string gameVersion)
    {
        if (string.IsNullOrEmpty(gameVersion))
            return string.Empty;

        var id = gameVersion.ToLowerInvariant();

        // 特殊版本映射表
        switch (id)
        {
            case "0.30-1":
            case "0.30-2":
            case "c0.30_01c":
                return "Classic_0.30";
            case "in-20100206-2103":
                return "Indev_20100206";
            case "inf-20100630-1":
                return "Infdev_20100630";
            case "inf-20100630-2":
                return "Alpha_v1.0.0";
            case "1.19_deep_dark_experimental_snapshot-1":
                return "1.19-exp1";
            case "in-20100130":
                return "Indev_0.31_20100130";
            case "b1.6-tb3":
                return "Beta_1.6_Test_Build_3";
            case "1_14_combat-212796":
                return "1.14.3_-_Combat_Test";
            case "1_14_combat-0":
                return "Combat_Test_2";
            case "1_14_combat-3":
                return "Combat_Test_3";
            case "1_15_combat-1":
                return "Combat_Test_4";
            case "1_15_combat-6":
                return "Combat_Test_5";
            case "1_16_combat-0":
                return "Combat_Test_6";
            case "1_16_combat-1":
                return "Combat_Test_7";
            case "1_16_combat-2":
                return "Combat_Test_7b";
            case "1_16_combat-3":
                return "Combat_Test_7c";
            case "1_16_combat-4":
                return "Combat_Test_8";
            case "1_16_combat-5":
                return "Combat_Test_8b";
            case "1_16_combat-6":
                return "Combat_Test_8c";
        }

        // 前缀匹配的特殊版本
        if (id.StartsWith("1.0.0-rc2")) return "RC2";
        if (id.StartsWith("2.0") || id.StartsWith("2point0")) return "2.0";
        if (id.StartsWith("b1.8-pre1")) return "Beta_1.8-pre1";
        if (id.StartsWith("b1.1-")) return "Beta_1.1";
        if (id.StartsWith("a1.1.0")) return "Alpha_v1.1.0";
        if (id.StartsWith("a1.0.14")) return "Alpha_v1.0.14";
        if (id.StartsWith("a1.0.13_01")) return "Alpha_v1.0.13_01";
        if (id.StartsWith("in-20100214")) return "Indev_20100214";

        // 实验性快照
        if (id.Contains("experimental-snapshot"))
        {
            return id.Replace("_experimental-snapshot-", "-exp");
        }

        // 各类版本前缀处理
        if (id.StartsWith("inf-")) return "Infdev_" + id[4..];
        if (id.StartsWith("in-")) return "Indev_" + id[3..];
        if (id.StartsWith("rd-")) return "pre-Classic_" + id;
        if (id.StartsWith('b')) return "Beta_" + id[1..];
        if (id.StartsWith('a')) return "Alpha_v" + id[1..];
        if (id.StartsWith('c'))
        {
            var classic = "Classic_" + id[1..];
            return classic.Replace("st", "SURVIVAL_TEST");
        }

        // 默认返回原版本 ID
        return id;
    }

    /// <summary>
    /// 解析版本类型（Release/Snapshot/OldAlpha/OldBeta）
    /// </summary>
    /// <param name="versionId">版本 ID</param>
    /// <returns>版本类型</returns>
    public static GameVersionType ParseVersionType(string versionId)
    {
        if (string.IsNullOrEmpty(versionId))
            return GameVersionType.Release;

        var id = versionId.ToLowerInvariant();

        // 旧版本
        if (id.StartsWith('a') || id.StartsWith("alpha"))
            return GameVersionType.OldAlpha;
        
        if (id.StartsWith('b') || id.StartsWith("beta"))
            return GameVersionType.OldBeta;

        // 快照版本（包含 'w' 或特定关键词）
        if (id.Contains('w') || 
            id.Contains("snapshot") ||
            id.Contains("pre") ||
            id.Contains("rc") ||
            id.Contains("exp") ||
            id.Contains("combat"))
            return GameVersionType.Snapshot;

        return GameVersionType.Release;
    }

    /// <summary>
    /// 判断是否为正式版本
    /// </summary>
    /// <param name="versionId">版本 ID</param>
    /// <returns>是否为正式版本</returns>
    public static bool IsReleaseVersion(string versionId)
    {
        return ParseVersionType(versionId) == GameVersionType.Release;
    }

    /// <summary>
    /// 判断是否为旧版本（Alpha/Beta）
    /// </summary>
    /// <param name="versionId">版本 ID</param>
    /// <returns>是否为旧版本</returns>
    public static bool IsOldVersion(string versionId)
    {
        var type = ParseVersionType(versionId);
        return type == GameVersionType.OldAlpha || type == GameVersionType.OldBeta;
    }
}