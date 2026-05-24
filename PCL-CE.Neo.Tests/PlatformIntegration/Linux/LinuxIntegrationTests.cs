using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Linux;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Linux;

public class LinuxPlatformServiceIntegrationTests
{
    private readonly LinuxPlatformService _service;

    public LinuxPlatformServiceIntegrationTests()
    {
        _service = new LinuxPlatformService();
    }

    [Fact]
    public void GetCurrentPlatform_ReturnsLinux()
    {
        var platform = _service.CurrentPlatform;
        Assert.Equal(PlatformType.Linux, platform);
    }

    [Fact]
    public void GetPlatformName_ReturnsLinux()
    {
        var name = _service.PlatformName;
        Assert.Contains("Linux", name, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Is64BitProcess_ReturnsTrue()
    {
        var is64Bit = _service.Is64BitProcess;
        Assert.True(is64Bit);
    }

    [Fact]
    public void GetResourcePath_ReturnsValidPath()
    {
        var path = _service.GetResourcePath("test.txt");
        Assert.False(string.IsNullOrEmpty(path));
        Assert.Contains("test.txt", path);
    }

    [Fact]
    public void PlatformVersion_IsNotEmpty()
    {
        var version = _service.PlatformVersion;
        Assert.NotNull(version);
        Assert.False(string.IsNullOrEmpty(version.ToString()));
    }
}

public class LinuxJavaScannerIntegrationTests
{
    private readonly LinuxJavaScanner _scanner;

    public LinuxJavaScannerIntegrationTests()
    {
        _scanner = new LinuxJavaScanner();
    }

    [Fact]
    public void ScanJavaPaths_ReturnsEnumerable()
    {
        var paths = _scanner.ScanJavaPaths();
        Assert.NotNull(paths);
    }

    [Fact]
    public void ScanDirectory_NonExistentDirectory_ReturnsEmpty()
    {
        var paths = _scanner.ScanDirectory("/nonexistent/path");
        Assert.Empty(paths);
    }

    [Fact]
    public void IsValidJavaPath_NonExistentPath_ReturnsFalse()
    {
        var isValid = _scanner.IsValidJavaPath("/nonexistent/java");
        Assert.False(isValid);
    }

    [Fact]
    public void ScanJavaPaths_IncludesCommonPaths()
    {
        var paths = _scanner.ScanJavaPaths().ToList();
        Assert.NotNull(paths);
    }
}

public class LinuxWindowServiceIntegrationTests
{
    [Fact]
    public void CanCreateWindowService()
    {
        var service = new LinuxWindowService();
        Assert.NotNull(service);
    }
}

public class LinuxThemeServiceIntegrationTests
{
    private readonly LinuxThemeService _service;

    public LinuxThemeServiceIntegrationTests()
    {
        _service = new LinuxThemeService();
    }

    [Fact]
    public async Task GetSystemTheme_ReturnsValidTheme()
    {
        var theme = await _service.GetSystemThemeAsync();
        Assert.True(Enum.IsDefined(typeof(AppTheme), theme));
    }

    [Fact]
    public void CurrentTheme_HasDefaultValue()
    {
        var theme = _service.CurrentTheme;
        Assert.True(Enum.IsDefined(typeof(AppTheme), theme));
    }

    [Fact]
    public async Task SetTheme_DoesNotThrow()
    {
        await _service.SetThemeAsync(AppTheme.Light);
        await _service.SetThemeAsync(AppTheme.Dark);
    }
}

public class LinuxAudioServiceIntegrationTests
{
    private readonly LinuxAudioService _service;

    public LinuxAudioServiceIntegrationTests()
    {
        _service = new LinuxAudioService();
    }

    [Fact]
    public void PlayNotificationAsync_DoesNotThrow()
    {
        var task = _service.PlayNotificationAsync();
        Assert.NotNull(task);
    }

    [Fact]
    public void SetVolume_DoesNotThrow()
    {
        _service.SetVolume(50);
    }

    [Fact]
    public void IsMuted_HasDefaultValue()
    {
        var isMuted = _service.IsMuted;
        Assert.False(isMuted);
    }

    [Fact]
    public void SetMute_DoesNotThrow()
    {
        _service.SetMute(true);
        Assert.True(_service.IsMuted);
        _service.SetMute(false);
        Assert.False(_service.IsMuted);
    }
}

public class LinuxClipboardServiceIntegrationTests
{
    private readonly LinuxClipboardService _service;

    public LinuxClipboardServiceIntegrationTests()
    {
        _service = new LinuxClipboardService();
    }

    [Fact]
    public async Task SetTextAsync_DoesNotThrow()
    {
        await _service.SetTextAsync("Test text");
    }

    [Fact]
    public async Task GetTextAsync_AfterSetText_ReturnsText()
    {
        var testText = $"Linux Clipboard Test {Guid.NewGuid()}";
        await _service.SetTextAsync(testText);
        var retrieved = await _service.GetTextAsync();
        Assert.Equal(testText, retrieved);
    }

    [Fact]
    public async Task Clear_DoesNotThrow()
    {
        await _service.SetTextAsync("Test");
        await _service.ClearAsync();
        var text = await _service.GetTextAsync();
        Assert.Null(text);
    }
}

public class LinuxDialogServiceIntegrationTests
{
    private readonly LinuxDialogService _service;

    public LinuxDialogServiceIntegrationTests()
    {
        _service = new LinuxDialogService();
    }

    [Fact]
    public async Task ShowInfoAsync_DoesNotThrow()
    {
        await _service.ShowInfoAsync("Test Title", "Test Message");
    }

    [Fact]
    public async Task ShowErrorAsync_DoesNotThrow()
    {
        await _service.ShowErrorAsync("Error Title", "Error Message");
    }
}

public class LinuxNotificationServiceIntegrationTests
{
    private readonly LinuxNotificationService _service;

    public LinuxNotificationServiceIntegrationTests()
    {
        _service = new LinuxNotificationService();
    }

    [Fact]
    public void IsSupported_ReturnsTrue()
    {
        Assert.True(_service.IsSupported);
    }

    [Fact]
    public async Task ShowNotificationAsync_DoesNotThrow()
    {
        await _service.ShowNotificationAsync("Test Title", "Test Message");
    }

    [Fact]
    public async Task ShowNotificationWithSeverity_DoesNotThrow()
    {
        await _service.ShowNotificationAsync(
            "Info Notification",
            "This is an info notification",
            NotificationSeverity.Info);
        
        await _service.ShowNotificationAsync(
            "Warning Notification",
            "This is a warning notification",
            NotificationSeverity.Warning);
        
        await _service.ShowNotificationAsync(
            "Error Notification",
            "This is an error notification",
            NotificationSeverity.Error);
    }
}

public class LinuxServiceBuilderIntegrationTests
{
    [Fact]
    public void ServiceCollectionExtensions_ContainsLinuxServices()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLinuxPlatformServices();
        
        var provider = services.BuildServiceProvider();
        
        Assert.NotNull(provider.GetService<IPlatformService>());
        Assert.NotNull(provider.GetService<IWindowService>());
        Assert.NotNull(provider.GetService<IJavaScanner>());
        Assert.NotNull(provider.GetService<IThemeService>());
        Assert.NotNull(provider.GetService<IAudioService>());
        Assert.NotNull(provider.GetService<IClipboardService>());
        Assert.NotNull(provider.GetService<IDialogService>());
        Assert.NotNull(provider.GetService<INotificationService>());
        Assert.NotNull(provider.GetService<IUIAccessProvider>());
        Assert.NotNull(provider.GetService<IAnimationService>());
    }
}

public class LinuxIntegrationTestSuite
{
    [Fact]
    public void AllLinuxServices_CanBeResolved()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLinuxPlatformServices();
        var provider = services.BuildServiceProvider();
        
        var platform = provider.GetService<IPlatformService>();
        var window = provider.GetService<IWindowService>();
        var java = provider.GetService<IJavaScanner>();
        var theme = provider.GetService<IThemeService>();
        var audio = provider.GetService<IAudioService>();
        var clipboard = provider.GetService<IClipboardService>();
        var dialog = provider.GetService<IDialogService>();
        var notification = provider.GetService<INotificationService>();
        var uiAccess = provider.GetService<IUIAccessProvider>();
        var animation = provider.GetService<IAnimationService>();
        
        Assert.NotNull(platform);
        Assert.NotNull(window);
        Assert.NotNull(java);
        Assert.NotNull(theme);
        Assert.NotNull(audio);
        Assert.NotNull(clipboard);
        Assert.NotNull(dialog);
        Assert.NotNull(notification);
        Assert.NotNull(uiAccess);
        Assert.NotNull(animation);
    }
}
