using System;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

/// <summary>
/// Windows 平台集成测试工具类
/// </summary>
public static class WindowsIntegrationTestUtils
{
    /// <summary>
    /// 检查当前是否在 Windows 平台上运行
    /// </summary>
    public static bool IsRunningOnWindows =>
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows);

    /// <summary>
    /// 如果不在 Windows 平台上时跳过测试
    /// </summary>
    public static void SkipTestIfNotWindows(string testName = "This test")
    {
        if (!IsRunningOnWindows)
        {
            throw new SkipTestException($"{testName} requires Windows platform");
        }
    }
}

/// <summary>
/// 用于跳过测试的异常
/// </summary>
public class SkipTestException : Exception
{
    public SkipTestException(string message) : base(message)
    {
    }
}
