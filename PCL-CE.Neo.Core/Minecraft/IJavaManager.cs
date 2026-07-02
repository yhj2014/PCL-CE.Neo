using System;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Minecraft.Parser;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Java 管理器接口
/// </summary>
public interface IJavaManager
{
    /// <summary>
    /// 保存配置到 JSON
    /// </summary>
    string SaveConfig();
    
    /// <summary>
    /// 从 JSON 读取配置
    /// </summary>
    void ReadConfig(string jsonConfig);
    
    /// <summary>
    /// 扫描 Java 安装
    /// </summary>
    Task ScanJavaAsync(bool force = false);
    
    /// <summary>
    /// 获取排序后的 Java 列表
    /// </summary>
    System.Collections.Generic.List<JavaEntry> GetSortedJavaList();
    
    /// <summary>
    /// 是否存在64位 Java
    /// </summary>
    bool Existing64BitJava();
    
    /// <summary>
    /// 是否存在任何 Java
    /// </summary>
    bool ExistAnyJava();
    
    /// <summary>
    /// 是否存在指定路径的 Java
    /// </summary>
    bool Exist(string javaExePath);
    
    /// <summary>
    /// 添加或获取 Java 条目
    /// </summary>
    JavaEntry? AddOrGet(string javaExePath);
    
    /// <summary>
    /// 仅获取 Java 条目
    /// </summary>
    JavaEntry? Get(string javaExePath);
    
    /// <summary>
    /// 选择适合指定版本范围的 Java
    /// </summary>
    Task<JavaEntry[]> SelectSuitableJavaAsync(Version minVersion, Version maxVersion);
    
    /// <summary>
    /// 检查所有 Java 的可用性
    /// </summary>
    void CheckAllAvailability();
}