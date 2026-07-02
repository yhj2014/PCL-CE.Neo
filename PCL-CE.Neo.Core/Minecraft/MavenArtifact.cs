using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Maven 依赖项，用于解析和构建 Maven 格式的库路径
/// Maven 格式：group:artifact:version 或 group:artifact:version:classifier
/// </summary>
public sealed class MavenArtifact
{
    /// <summary>
    /// 组 ID（如 net.minecraft）
    /// </summary>
    public string Group { get; }
    
    /// <summary>
    /// 构件 ID（如 minecraft-client）
    /// </summary>
    public string Artifact { get; }
    
    /// <summary>
    /// 版本号（如 1.20.4）
    /// </summary>
    public string Version { get; }
    
    /// <summary>
    /// 分类器（如 natives-windows，可选）
    /// </summary>
    public string? Classifier { get; }
    
    /// <summary>
    /// 扩展名（默认为 jar）
    /// </summary>
    public string Extension { get; }

    /// <summary>
    /// 从 Maven 坐标字符串创建构件
    /// </summary>
    /// <param name="mavenCoordinate">Maven 坐标，格式：group:artifact:version 或 group:artifact:version:classifier</param>
    public MavenArtifact(string mavenCoordinate)
    {
        if (string.IsNullOrWhiteSpace(mavenCoordinate))
            throw new ArgumentException("Maven 坐标不能为空", nameof(mavenCoordinate));

        var parts = mavenCoordinate.Split(':');
        if (parts.Length < 3)
            throw new ArgumentException($"无效的 Maven 坐标格式: {mavenCoordinate}", nameof(mavenCoordinate));

        Group = parts[0];
        Artifact = parts[1];
        Version = parts[2];
        Classifier = parts.Length > 3 ? parts[3] : null;
        Extension = "jar";
    }

    /// <summary>
    /// 从各个部分创建构件
    /// </summary>
    public MavenArtifact(string group, string artifact, string version, string? classifier = null, string extension = "jar")
    {
        Group = group ?? throw new ArgumentNullException(nameof(group));
        Artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Classifier = classifier;
        Extension = extension;
    }

    /// <summary>
    /// 构件文件名（如 minecraft-client-1.20.4.jar 或 minecraft-client-1.20.4-natives-windows.jar）
    /// </summary>
    public string FileName
    {
        get
        {
            var baseName = $"{Artifact}-{Version}";
            if (!string.IsNullOrEmpty(Classifier))
                baseName += $"-{Classifier}";
            return $"{baseName}.{Extension}";
        }
    }

    /// <summary>
    /// Maven 仓库相对路径（如 net/minecraft/minecraft-client/1.20.4/minecraft-client-1.20.4.jar）
    /// </summary>
    public string Path
    {
        get
        {
            var groupPath = Group.Replace('.', '/');
            var basePath = $"{groupPath}/{Artifact}/{Version}/{FileName}";
            return basePath;
        }
    }

    /// <summary>
    /// 本地仓库中的完整路径
    /// </summary>
    /// <param name="librariesDir">libraries 目录路径</param>
    /// <returns>完整文件路径</returns>
    public string GetLocalPath(string librariesDir)
    {
        return PathCombine(librariesDir, Path);
    }

    /// <summary>
    /// Maven 中央仓库 URL
    /// </summary>
    public string MavenCentralUrl => $"https://repo1.maven.org/maven2/{Path}";

    /// <summary>
    /// 获取指定 Maven 仓库的 URL
    /// </summary>
    /// <param name="repoUrl">仓库基础 URL</param>
    /// <returns>完整下载 URL</returns>
    public string GetRepositoryUrl(string repoUrl)
    {
        var baseUrl = repoUrl.TrimEnd('/');
        return $"{baseUrl}/{Path}";
    }

    /// <summary>
    /// 构件坐标字符串（group:artifact:version[:classifier]）
    /// </summary>
    public string Coordinate
    {
        get
        {
            var coord = $"{Group}:{Artifact}:{Version}";
            if (!string.IsNullOrEmpty(Classifier))
                coord += $":{Classifier}";
            return coord;
        }
    }

    /// <summary>
    /// 尝试从字符串解析 Maven 构件
    /// </summary>
    /// <param name="coordinate">坐标字符串</param>
    /// <param name="artifact">解析结果</param>
    /// <returns>是否解析成功</returns>
    public static bool TryParse(string coordinate, out MavenArtifact? artifact)
    {
        try
        {
            artifact = new MavenArtifact(coordinate);
            return true;
        }
        catch
        {
            artifact = null;
            return false;
        }
    }

    /// <summary>
    /// 从路径推断 Maven 构件信息（逆向解析）
    /// </summary>
    /// <param name="path">Maven 仓库相对路径</param>
    /// <returns>Maven 构件</returns>
    public static MavenArtifact? FromPath(string path)
    {
        try
        {
            // 路径格式：group/artifact/version/filename
            // 例如：net/minecraft/minecraft-client/1.20.4/minecraft-client-1.20.4.jar
            
            var regex = new Regex(@"^(.+)/([^/]+)/([^/]+)/([^/]+)\.([^./]+)$");
            var match = regex.Match(path);
            
            if (!match.Success)
                return null;

            var group = match.Groups[1].Value.Replace('/', '.');
            var artifact = match.Groups[2].Value;
            var version = match.Groups[3].Value;
            var filename = match.Groups[4].Value;
            var extension = match.Groups[5].Value;

            // 从文件名提取分类器
            var expectedBaseName = $"{artifact}-{version}";
            string? classifier = null;
            
            if (filename.StartsWith(expectedBaseName + "-"))
            {
                classifier = filename.Substring(expectedBaseName.Length + 1);
            }

            return new MavenArtifact(group, artifact, version, classifier, extension);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 跨平台路径拼接
    /// </summary>
    private static string PathCombine(string baseDir, string relativePath)
    {
        // Maven 使用 '/' 作为路径分隔符，本地路径使用系统分隔符
        var normalizedRelative = relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
        return System.IO.Path.Combine(baseDir, normalizedRelative);
    }

    public override string ToString() => Coordinate;
    
    public override bool Equals(object? obj)
    {
        return obj is MavenArtifact other && Coordinate == other.Coordinate;
    }
    
    public override int GetHashCode() => Coordinate.GetHashCode();
}