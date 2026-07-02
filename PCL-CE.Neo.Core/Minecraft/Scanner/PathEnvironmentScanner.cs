using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Scanner;

/// <summary>
/// PATH 环境变量 Java 扫描器，扫描系统 PATH 中的 Java 安装
/// </summary>
public class PathEnvironmentScanner : IJavaScannerStrategy
{
    private readonly ILogger<PathEnvironmentScanner>? _logger;

    public PathEnvironmentScanner() : this(null) { }

    public PathEnvironmentScanner(ILogger<PathEnvironmentScanner>? logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 执行 PATH 环境变量扫描
    /// </summary>
    /// <param name="results">结果集合</param>
    public void Scan(ICollection<string> results)
    {
        try
        {
            _logger?.LogInformation("开始 PATH 环境变量 Java 扫描");

            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
            {
                _logger?.LogDebug("PATH 环境变量为空");
                return;
            }

            var pathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            var pathDirs = pathEnv.Split(pathSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var dir in pathDirs)
            {
                try
                {
                    var trimmedDir = dir.Trim();
                    if (!Directory.Exists(trimmedDir))
                        continue;

                    // 直接检查目录中是否有 java 可执行文件
                    var javaExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";
                    var javaExe = Path.Combine(trimmedDir, javaExeName);

                    if (File.Exists(javaExe))
                    {
                        // 找到 java.exe，获取其父目录（通常是 bin 目录）
                        var binDir = Path.GetDirectoryName(javaExe);
                        if (binDir != null)
                        {
                            // 获取 Java 安装根目录（bin 的父目录）
                            var javaHome = Directory.GetParent(binDir)?.FullName;
                            if (javaHome != null && !results.Contains(javaExe))
                            {
                                results.Add(javaExe);
                                _logger?.LogDebug("在 PATH 中找到 Java: {Path}", javaExe);
                            }
                        }
                    }
                    else
                    {
                        // 有些 PATH 指向 Java home 而不是 bin，检查 bin 子目录
                        var binSubDir = Path.Combine(trimmedDir, "bin");
                        if (Directory.Exists(binSubDir))
                        {
                            var javaInBin = Path.Combine(binSubDir, javaExeName);
                            if (File.Exists(javaInBin) && !results.Contains(javaInBin))
                            {
                                results.Add(javaInBin);
                                _logger?.LogDebug("在 PATH 子目录中找到 Java: {Path}", javaInBin);
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _logger?.LogDebug("无权限访问 PATH 目录: {Dir}", dir);
                }
                catch (IOException ioEx)
                {
                    _logger?.LogDebug("IO错误，跳过 PATH 目录: {Dir}, Error: {Error}", dir, ioEx.Message);
                }
            }

            // 同时检查 JAVA_HOME 环境变量
            ScanJavaHome(results);

            _logger?.LogInformation("PATH 环境变量扫描完成，找到 {Count} 个安装", results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "PATH 环境变量扫描失败");
        }
    }

    /// <summary>
    /// 扫描 JAVA_HOME 环境变量
    /// </summary>
    private void ScanJavaHome(ICollection<string> results)
    {
        try
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (string.IsNullOrEmpty(javaHome) || !Directory.Exists(javaHome))
            {
                return;
            }

            _logger?.LogDebug("检查 JAVA_HOME: {Home}", javaHome);

            var javaExeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";
            var binDir = Path.Combine(javaHome, "bin");
            var javaExe = Path.Combine(binDir, javaExeName);

            if (File.Exists(javaExe) && !results.Contains(javaExe))
            {
                results.Add(javaExe);
                _logger?.LogDebug("在 JAVA_HOME 中找到 Java: {Path}", javaExe);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "检查 JAVA_HOME 失败");
        }
    }
}