using Xunit;
using FluentAssertions;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

/// <summary>
/// Windows 平台集成测试套件 - 包含所有 Windows 平台集成测试的集合类
/// </summary>
[Collection("Windows Platform Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
[Trait("TestSuite", "WindowsPlatform")]
public class WindowsPlatformIntegrationTestSuite
{
    /// <summary>
    /// 完整的 Windows 平台集成测试集合
    /// </summary>
    [Fact]
    [Trait("TestType", "SmokeTest")]
    public void WindowsPlatform_SmokeTest_AllServicesShouldBeCreatable()
    {
        // 这个测试是一个快速冒烟测试，确保所有 Windows 平台服务都可以被实例化而不抛异常
        var platformService = new PCL_CE.Neo.Platform.Windows.WindowsPlatformService();
        var windowService = new PCL_CE.Neo.Platform.Windows.WindowsWindowService();
        var javaScanner = new PCL_CE.Neo.Platform.Windows.WindowsJavaScanner();
        var themeService = new PCL_CE.Neo.Platform.Windows.WindowsThemeService();
        var audioService = new PCL_CE.Neo.Platform.Windows.WindowsAudioService();
        var clipboardService = new PCL_CE.Neo.Platform.Windows.WindowsClipboardService();
        var dialogService = new PCL_CE.Neo.Platform.Windows.WindowsDialogService();
        var notificationService = new PCL_CE.Neo.Platform.Windows.WindowsNotificationService();
        var uiAccessProvider = new PCL_CE.Neo.Platform.Windows.WindowsUIAccessProvider();

        // 验证所有服务都可以实例化
        platformService.Should().NotBeNull();
        windowService.Should().NotBeNull();
        javaScanner.Should().NotBeNull();
        themeService.Should().NotBeNull();
        audioService.Should().NotBeNull();
        clipboardService.Should().NotBeNull();
        dialogService.Should().NotBeNull();
        notificationService.Should().NotBeNull();
        uiAccessProvider.Should().NotBeNull();

        Debug.WriteLine("✅ All Windows platform services instantiated successfully!");
    }

    [Fact]
    [Trait("TestType", "PlatformDetection")]
    public void WindowsPlatform_Detection_ShouldWork()
    {
        var currentPlatform = PCL_CE.Neo.Core.PlatformDetector.CurrentPlatform;
        Debug.WriteLine($"Detected platform: {currentPlatform}");
        Debug.WriteLine($"IsWindows: {PCL_CE.Neo.Core.PlatformDetector.IsWindows}");
        Debug.WriteLine($"IsLinux: {PCL_CE.Neo.Core.PlatformDetector.IsLinux}");
        Debug.WriteLine($"IsMacOS: {PCL_CE.Neo.Core.PlatformDetector.IsMacOS}");

        // 无论在什么平台上运行，至少应该能检测到当前平台
        currentPlatform.Should().NotBeNullOrEmpty();
    }
}
