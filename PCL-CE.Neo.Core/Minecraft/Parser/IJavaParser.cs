namespace PCL_CE.Neo.Core.Minecraft.Parser;

/// <summary>
/// Java 安装解析器接口
/// </summary>
public interface IJavaParser
{
    /// <summary>
    /// 解析 Java 可执行文件，获取详细的安装信息
    /// </summary>
    /// <param name="javaExePath">java.exe 或 javaw.exe 的完整路径</param>
    /// <returns>解析成功返回 JavaInstallation，失败返回 null</returns>
    JavaInstallation? Parse(string javaExePath);
}