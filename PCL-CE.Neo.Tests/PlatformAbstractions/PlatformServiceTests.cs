using Xunit;
using PCL.CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests.PlatformAbstractions;

public class PlatformServiceTests
{
    [Fact]
    public void WindowsPlatformService_PlatformName_ReturnsWindows()
    {
        var service = new Platform.Windows.WindowsPlatformService();
        Assert.Equal("Windows", service.PlatformName);
    }

    [Fact]
    public void WindowsPlatformService_GetLocalApplicationDataPath_ReturnsValidPath()
    {
        var service = new Platform.Windows.WindowsPlatformService();
        var path = service.GetLocalApplicationDataPath();
        Assert.NotNull(path);
        Assert.Contains("PCL", path);
    }

    [Fact]
    public void WindowsPlatformService_GetGameDataPath_ReturnsMinecraftPath()
    {
        var service = new Platform.Windows.WindowsPlatformService();
        var path = service.GetGameDataPath();
        Assert.NotNull(path);
        Assert.Contains(".minecraft", path);
    }

    [Fact]
    public void WindowsPlatformService_GetTempPath_ReturnsValidPath()
    {
        var service = new Platform.Windows.WindowsPlatformService();
        var path = service.GetTempPath();
        Assert.NotNull(path);
        Assert.True(Directory.Exists(Path.GetDirectoryName(path) ?? string.Empty));
    }
}

public class JavaScannerTests
{
    [Fact]
    public void WindowsJavaScanner_IsValidJavaPath_WithJavaExe_ReturnsTrue()
    {
        var scanner = new Platform.Windows.WindowsJavaScanner();
        var result = scanner.IsValidJavaPath("java.exe");
        Assert.True(result);
    }

    [Fact]
    public void WindowsJavaScanner_IsValidJavaPath_WithNull_ReturnsFalse()
    {
        var scanner = new Platform.Windows.WindowsJavaScanner();
        var result = scanner.IsValidJavaPath(null!);
        Assert.False(result);
    }

    [Fact]
    public void WindowsJavaScanner_IsValidJavaPath_WithEmpty_ReturnsFalse()
    {
        var scanner = new Platform.Windows.WindowsJavaScanner();
        var result = scanner.IsValidJavaPath(string.Empty);
        Assert.False(result);
    }

    [Fact]
    public void WindowsJavaScanner_ScanDirectory_WithInvalidPath_ReturnsEmpty()
    {
        var scanner = new Platform.Windows.WindowsJavaScanner();
        var results = scanner.ScanDirectory("/nonexistent/path");
        Assert.Empty(results);
    }
}

public class ThemeServiceTests
{
    [Fact]
    public void WindowsThemeService_GetAvailableThemes_ReturnsTwoThemes()
    {
        var service = new Platform.Windows.WindowsThemeService();
        var themes = service.GetAvailableThemes();
        Assert.Equal(2, themes.Count());
    }

    [Fact]
    public void WindowsThemeService_GetCurrentTheme_ReturnsLightTheme()
    {
        var service = new Platform.Windows.WindowsThemeService();
        var theme = service.GetCurrentTheme();
        Assert.Equal(ThemeType.Light, theme.Type);
    }
}
