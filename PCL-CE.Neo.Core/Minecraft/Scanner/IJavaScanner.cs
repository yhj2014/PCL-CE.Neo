using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Minecraft.Scanner;

/// <summary>
/// Java 安装扫描器接口
/// </summary>
public interface IJavaScannerStrategy
{
    /// <summary>
    /// 执行 Java 安装扫描
    /// </summary>
    /// <param name="results">扫描结果集合，用于存储找到的 java.exe 路径</param>
    void Scan(ICollection<string> results);
}