using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Scanner;

/// <summary>
/// Windows 注册表 Java 扫描器
/// 通过读取 Windows 注册表查找已安装的 Java
/// </summary>
public class RegistryJavaScanner : IJavaScannerStrategy
{
    private readonly ILogger<RegistryJavaScanner>? _logger;
    
    /// <summary>
    /// 标准 Java 注册表路径
    /// </summary>
    private static readonly string[] StandardRegistryPaths =
    {
        @"SOFTWARE\JavaSoft\Java Development Kit",
        @"SOFTWARE\JavaSoft\Java Runtime Environment",
        @"SOFTWARE\WOW6432Node\JavaSoft\Java Development Kit",
        @"SOFTWARE\WOW6432Node\JavaSoft\Java Runtime Environment"
    };

    /// <summary>
    /// 第三方品牌 Java 注册表路径
    /// </summary>
    private static readonly string[] BrandRegistryPaths =
    {
        @"SOFTWARE\Eclipse Adoptium\JDK",
        @"SOFTWARE\Eclipse Adoptium\JRE",
        @"SOFTWARE\Azul Systems\Zulu",
        @"SOFTWARE\BellSoft\Liberica",
        @"SOFTWARE\Microsoft\JDK",
        @"SOFTWARE\Amazon\Corretto"
    };

    public RegistryJavaScanner() : this(null) { }

    public RegistryJavaScanner(ILogger<RegistryJavaScanner>? logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 执行注册表扫描
    /// </summary>
    /// <param name="results">结果集合</param>
    public void Scan(ICollection<string> results)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger?.LogDebug("注册表扫描仅在 Windows 平台上执行");
            return;
        }

        try
        {
            _logger?.LogInformation("开始注册表 Java 扫描");
            ScanStandardRegistry(results);
            ScanBrandRegistry(results);
            _logger?.LogInformation("注册表扫描完成，找到 {Count} 个安装", results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "注册表扫描失败");
        }
    }

    private void ScanStandardRegistry(ICollection<string> results)
    {
        foreach (var regPath in StandardRegistryPaths)
        {
            try
            {
                ScanRegistryKey(regPath, "JavaHome", results);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "无法读取注册表路径: {Path}", regPath);
            }
        }
    }

    private void ScanBrandRegistry(ICollection<string> results)
    {
        foreach (var keyPath in BrandRegistryPaths)
        {
            try
            {
                ScanRegistryKey(keyPath, "InstallationPath", results);
                // 同时尝试 JavaHome
                ScanRegistryKey(keyPath, "JavaHome", results);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "无法读取品牌注册表路径: {Path}", keyPath);
            }
        }
    }

    private void ScanRegistryKey(string registryPath, string valueName, ICollection<string> results)
    {
        // Windows-only code using reflection to avoid compile-time dependency
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        
        try
        {
            // 动态加载 Registry 类（跨平台编译兼容）
            var registryType = Type.GetType("Microsoft.Win32.Registry, Microsoft.Win32.Registry");
            if (registryType == null) return;
            
            var localMachineProp = registryType.GetProperty("LocalMachine");
            if (localMachineProp == null) return;
            
            var localMachine = localMachineProp.GetValue(null) as object;
            if (localMachine == null) return;
            
            var openSubKeyMethod = localMachine.GetType().GetMethod("OpenSubKey", new[] { typeof(string) });
            if (openSubKeyMethod == null) return;
            
            var rootKey = openSubKeyMethod.Invoke(localMachine, new object[] { registryPath }) as object;
            if (rootKey == null) return;
            
            try
            {
                var getSubKeyNamesMethod = rootKey.GetType().GetMethod("GetSubKeyNames");
                if (getSubKeyNamesMethod == null) return;
                
                var subKeyNames = getSubKeyNamesMethod.Invoke(rootKey, null) as string[];
                if (subKeyNames == null) return;
                
                foreach (var subKeyName in subKeyNames)
                {
                    try
                    {
                        var openSubKeyMethod2 = rootKey.GetType().GetMethod("OpenSubKey", new[] { typeof(string) });
                        if (openSubKeyMethod2 == null) continue;
                        
                        var subKey = openSubKeyMethod2.Invoke(rootKey, new object[] { subKeyName }) as object;
                        if (subKey == null) continue;
                        
                        try
                        {
                            var getValueMethod = subKey.GetType().GetMethod("GetValue", new[] { typeof(string) });
                            if (getValueMethod == null) continue;
                            
                            var javaHome = getValueMethod.Invoke(subKey, new object[] { valueName }) as string;
                            
                            if (!string.IsNullOrEmpty(javaHome) &&
                                !Path.GetInvalidPathChars().Any(c => javaHome.Contains(c)))
                            {
                                var javaExePath = Path.Combine(javaHome, "bin", "java.exe");
                                if (File.Exists(javaExePath))
                                {
                                    results.Add(javaExePath);
                                    _logger?.LogDebug("找到注册表 Java: {Path}", javaExePath);
                                }
                            }
                        }
                        finally
                        {
                            // Dispose subKey
                            var disposeMethod = subKey.GetType().GetMethod("Dispose");
                            if (disposeMethod != null)
                                disposeMethod.Invoke(subKey, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "无法读取子键: {Name}", subKeyName);
                    }
                }
            }
            finally
            {
                // Dispose rootKey
                var disposeMethod = rootKey.GetType().GetMethod("Dispose");
                if (disposeMethod != null)
                    disposeMethod.Invoke(rootKey, null);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "注册表访问失败: {Path}", registryPath);
        }
    }
}