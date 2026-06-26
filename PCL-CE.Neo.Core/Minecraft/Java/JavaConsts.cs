using System;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Java constants and configuration values.
/// </summary>
public static class JavaConsts
{
    /// <summary>
    /// Folder names that should be excluded from Java scanning.
    /// </summary>
    public static readonly HashSet<string> ExcludeFolderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "jre",
        "jdk",
        "bin",
        "lib",
        "conf",
        "legal",
        "demo",
        "sample",
        "docs",
        "man",
        "include",
        ".minecraft",
        "cache",
        "temp",
        "tmp",
        "logs",
        "crash-reports",
        "screenshots",
        "saves",
        "resourcepacks",
        "shaderpacks",
        "options.txt",
        "servers.dat"
    };

    /// <summary>
    /// Keywords most likely to appear in Java installation folder names.
    /// </summary>
    public static readonly HashSet<string> MostPossibleKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "java",
        "jdk",
        "jre",
        "openjdk",
        "temurin",
        "adoptopenjdk",
        "corretto",
        "zulu",
        "liberica",
        "microsoft",
        "oracle",
        "graalvm",
        "semeru",
        "kona",
        "dragonwell"
    };

    /// <summary>
    /// All keywords for Java folder name matching.
    /// </summary>
    public static readonly HashSet<string> AllKeyworkds = new(StringComparer.OrdinalIgnoreCase)
    {
        "java",
        "jdk",
        "jre",
        "openjdk",
        "temurin",
        "adoptopenjdk",
        "corretto",
        "zulu",
        "liberica",
        "microsoft",
        "oracle",
        "graalvm",
        "semeru",
        "kona",
        "dragonwell",
        "jetbrains",
        "bellsoft",
        "azul",
        "ibm",
        "tencent",
        "alibaba",
        "eclipse"
    };

    /// <summary>
    /// Default minimum Java version for different Minecraft versions.
    /// </summary>
    public static readonly Dictionary<string, int> MinJavaVersionByMcVersion = new()
    {
        ["1.7"] = 8,
        ["1.8"] = 8,
        ["1.12"] = 8,
        ["1.13"] = 8,
        ["1.14"] = 8,
        ["1.15"] = 8,
        ["1.16"] = 8,
        ["1.17"] = 16,
        ["1.18"] = 17,
        ["1.19"] = 17,
        ["1.20"] = 17,
        ["1.21"] = 21
    };

    /// <summary>
    /// Maximum search depth for Java folder scanning.
    /// </summary>
    public const int MaxSearchDepth = 8;

    /// <summary>
    /// Minimum interval between Java scans in seconds.
    /// </summary>
    public const int MinScanIntervalSeconds = 13;
}