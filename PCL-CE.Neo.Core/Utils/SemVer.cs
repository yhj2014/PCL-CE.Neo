using System;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

/// <summary>
/// 语义化版本号（SemVer 2.0.0），用于精确的版本比较
/// 格式：major.minor.patch[-prerelease][+build]
/// </summary>
[Serializable]
public class SemVer : IComparable<SemVer>, IEquatable<SemVer>
{
    /// <summary>
    /// 主版本号
    /// </summary>
    public int Major { get; }
    
    /// <summary>
    /// 次版本号
    /// </summary>
    public int Minor { get; }
    
    /// <summary>
    /// 补丁版本号
    /// </summary>
    public int Patch { get; }
    
    /// <summary>
    /// 预发布标识（如 alpha, beta, rc.1）
    /// </summary>
    public string Prerelease { get; }
    
    /// <summary>
    /// 构建元数据（如 build.123）
    /// </summary>
    public string BuildMetadata { get; }

    /// <summary>
    /// 是否为预发布版本
    /// </summary>
    public bool IsPrerelease => !string.IsNullOrEmpty(Prerelease);

    /// <summary>
    /// SemVer 正则表达式匹配模式
    /// </summary>
    private static readonly Regex SemVerPattern = new Regex(
        @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
        RegexOptions.Compiled);

    public SemVer(int major, int minor, int patch, string? prerelease = null, string? buildMetadata = null)
    {
        if (major < 0) throw new ArgumentException("主版本号不能为负数", nameof(major));
        if (minor < 0) throw new ArgumentException("次版本号不能为负数", nameof(minor));
        if (patch < 0) throw new ArgumentException("补丁版本号不能为负数", nameof(patch));

        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease ?? string.Empty;
        BuildMetadata = buildMetadata ?? string.Empty;
    }

    /// <summary>
    /// 解析版本字符串为 SemVer
    /// </summary>
    /// <param name="version">版本字符串</param>
    /// <returns>SemVer 对象</returns>
    public static SemVer Parse(string version)
    {
        if (!TryParse(version, out var result))
            throw new ArgumentException($"无效的语义化版本格式: {version}", nameof(version));
        
        return result!;
    }

    /// <summary>
    /// 尝试解析版本字符串为 SemVer
    /// </summary>
    /// <param name="version">版本字符串</param>
    /// <param name="result">解析结果</param>
    /// <returns>是否解析成功</returns>
    public static bool TryParse(string version, out SemVer? result)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            result = null;
            return false;
        }

        var match = SemVerPattern.Match(version.Trim());
        if (!match.Success)
        {
            result = null;
            return false;
        }

        result = CreateFromMatch(match);
        return true;
    }

    private static SemVer CreateFromMatch(Match match)
    {
        var major = int.Parse(match.Groups["major"].Value);
        var minor = int.Parse(match.Groups["minor"].Value);
        var patch = int.Parse(match.Groups["patch"].Value);
        var prerelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;
        var build = match.Groups["buildmetadata"].Success ? match.Groups["buildmetadata"].Value : null;

        return new SemVer(major, minor, patch, prerelease, build);
    }

    /// <summary>
    /// 比较两个 SemVer
    /// </summary>
    public int CompareTo(SemVer? other)
    {
        if (other is null) return 1;

        // 比较 Major
        var compare = Major.CompareTo(other.Major);
        if (compare != 0) return compare;

        // 比较 Minor
        compare = Minor.CompareTo(other.Minor);
        if (compare != 0) return compare;

        // 比较 Patch
        compare = Patch.CompareTo(other.Patch);
        if (compare != 0) return compare;

        // 比较 Prerelease
        return ComparePrerelease(Prerelease, other.Prerelease);
    }

    /// <summary>
    /// 比较预发布标识（按照 SemVer 规范）
    /// </summary>
    private static int ComparePrerelease(string a, string b)
    {
        if (string.Equals(a, b, StringComparison.Ordinal))
            return 0;

        // 正式版（无预发布标识）优先级高于预发布版
        if (string.IsNullOrEmpty(a)) return 1;
        if (string.IsNullOrEmpty(b)) return -1;

        var identifiersA = a.Split('.');
        var identifiersB = b.Split('.');

        var minLength = Math.Min(identifiersA.Length, identifiersB.Length);
        
        for (var i = 0; i < minLength; i++)
        {
            var idA = identifiersA[i];
            var idB = identifiersB[i];

            var aIsNumeric = int.TryParse(idA, out var numA);
            var bIsNumeric = int.TryParse(idB, out var numB);

            int result;
            if (aIsNumeric && bIsNumeric)
            {
                result = numA.CompareTo(numB);
            }
            else if (aIsNumeric || bIsNumeric)
            {
                // 数值标识符比非数值标识符优先级低
                result = aIsNumeric ? -1 : 1;
            }
            else
            {
                result = string.Compare(idA, idB, StringComparison.Ordinal);
            }

            if (result != 0)
                return result;
        }

        // 较长的预发布标识符优先级较低
        return identifiersA.Length.CompareTo(identifiersB.Length);
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";

        if (!string.IsNullOrEmpty(Prerelease))
            version += $"-{Prerelease}";

        if (!string.IsNullOrEmpty(BuildMetadata))
            version += $"+{BuildMetadata}";

        return version;
    }

    /// <summary>
    /// 转换为简短字符串（不含构建元数据）
    /// </summary>
    public string ToShortString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        
        if (!string.IsNullOrEmpty(Prerelease))
            version += $"-{Prerelease}";
        
        return version;
    }

    public override bool Equals(object? obj) => Equals(obj as SemVer);

    public bool Equals(SemVer? other)
    {
        return other != null &&
               Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch &&
               string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal) &&
               string.Equals(BuildMetadata, other.BuildMetadata, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + Major.GetHashCode();
            hash = hash * 23 + Minor.GetHashCode();
            hash = hash * 23 + Patch.GetHashCode();
            hash = hash * 23 + Prerelease.GetHashCode();
            hash = hash * 23 + BuildMetadata.GetHashCode();
            return hash;
        }
    }

    // 运算符重载
    public static bool operator ==(SemVer? left, SemVer? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(SemVer? left, SemVer? right) => !(left == right);
    
    public static bool operator <(SemVer? left, SemVer? right) =>
        left is null ? right != null : left.CompareTo(right) < 0;
    
    public static bool operator >(SemVer? left, SemVer? right) =>
        left != null && left.CompareTo(right) > 0;
    
    public static bool operator <=(SemVer? left, SemVer? right) =>
        left is null || left.CompareTo(right) <= 0;
    
    public static bool operator >=(SemVer? left, SemVer? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;
}