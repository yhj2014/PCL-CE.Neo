using System;

namespace PCL_CE.Neo.Core.Utils;

/// <summary>
/// 版本范围，用于判断版本是否在指定范围内
/// </summary>
public class VersionRange
{
    /// <summary>
    /// 最小版本（包含）
    /// </summary>
    public Version? MinVersion { get; set; }
    
    /// <summary>
    /// 最大版本（包含）
    /// </summary>
    public Version? MaxVersion { get; set; }

    public VersionRange(Version? minVersion = null, Version? maxVersion = null)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
    }

    /// <summary>
    /// 判断目标版本是否在范围内
    /// </summary>
    /// <param name="target">目标版本</param>
    /// <returns>是否在范围内</returns>
    public bool IsInRange(Version target)
    {
        if (MinVersion == null && MaxVersion == null)
            return false;

        var minOk = MinVersion == null || MinVersion <= target;
        var maxOk = MaxVersion == null || MaxVersion >= target;
        
        return minOk && maxOk;
    }

    /// <summary>
    /// 缩减版本号范围，如果 target 比当前 MinVersion 大则应用
    /// </summary>
    /// <param name="target">目标版本号</param>
    /// <returns>是否使用了该版本</returns>
    public bool SetMin(Version target)
    {
        if (MinVersion == null || target > MinVersion)
        {
            MinVersion = target;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 缩减版本号范围，如果 target 比当前 MaxVersion 小则应用
    /// </summary>
    /// <param name="target">目标版本号</param>
    /// <returns>是否使用了该版本</returns>
    public bool SetMax(Version target)
    {
        if (MaxVersion == null || target < MaxVersion)
        {
            MaxVersion = target;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 合并两个版本范围
    /// </summary>
    /// <param name="other">另一个版本范围</param>
    public void Merge(VersionRange other)
    {
        if (other.MinVersion != null)
            SetMin(other.MinVersion);
        
        if (other.MaxVersion != null)
            SetMax(other.MaxVersion);
    }

    /// <summary>
    /// 创建包含单个版本的版本范围
    /// </summary>
    public static VersionRange Single(Version version)
    {
        return new VersionRange(version, version);
    }

    /// <summary>
    /// 创建从指定版本开始的版本范围
    /// </summary>
    public static VersionRange From(Version minVersion)
    {
        return new VersionRange(minVersion, null);
    }

    /// <summary>
    /// 创建到指定版本结束的版本范围
    /// </summary>
    public static VersionRange To(Version maxVersion)
    {
        return new VersionRange(null, maxVersion);
    }

    /// <summary>
    /// 创建所有版本的版本范围
    /// </summary>
    public static VersionRange All => new VersionRange(null, null);

    public override string ToString()
    {
        if (MinVersion == null && MaxVersion == null)
            return "[all]";
        
        if (MinVersion == null)
            return $"[≤ {MaxVersion}]";
        
        if (MaxVersion == null)
            return $"[≥ {MinVersion}]";
        
        if (MinVersion == MaxVersion)
            return $"[{MinVersion}]";
        
        return $"[{MinVersion} - {MaxVersion}]";
    }
}