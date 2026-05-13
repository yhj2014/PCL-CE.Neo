using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsPlatformServiceIntegrationTests
{
    private readonly IPlatformService _platformService;

    public WindowsPlatformServiceIntegrationTests()
    {
        _platformService = new WindowsPlatformService();
    }

    [Fact]
    public void PlatformName_ShouldReturnWindows()
    {
        // Act
        var platformName = _platformService.PlatformName;

        // Assert
        platformName.Should().Be("Windows");
    }

    [Fact]
    public void OSVersion_ShouldNotBeNullOrEmpty()
    {
        // Act
        var osVersion = _platformService.OSVersion;

        // Assert
        osVersion.Should().NotBeNullOrEmpty();
        Debug.WriteLine($"OS Version: {osVersion}");
    }

    [Fact]
    public void Architecture_ShouldBeValid()
    {
        // Act
        var architecture = _platformService.Architecture;

        // Assert
        architecture.Should().NotBeNullOrEmpty();
        Debug.WriteLine($"Architecture: {architecture}");
    }

    [Fact]
    public void GetLocalApplicationDataPath_ShouldReturnValidPath()
    {
        // Act
        var path = _platformService.GetLocalApplicationDataPath();

        // Assert
        path.Should().NotBeNullOrEmpty();
        Directory.Exists(Path.GetDirectoryName(path)!).Should().BeTrue();
        Debug.WriteLine($"Local Application Data Path: {path}");
    }

    [Fact]
    public void GetTempPath_ShouldReturnValidPath()
    {
        // Act
        var tempPath = _platformService.GetTempPath();

        // Assert
        tempPath.Should().NotBeNullOrEmpty();
        Directory.Exists(tempPath).Should().BeTrue();
        Debug.WriteLine($"Temp Path: {tempPath}");
    }

    [Fact]
    public void GetGameDataPath_ShouldReturnValidPath()
    {
        // Act
        var gameDataPath = _platformService.GetGameDataPath();

        // Assert
        gameDataPath.Should().NotBeNullOrEmpty();
        gameDataPath.Should().Contain(".minecraft");
        Debug.WriteLine($"Game Data Path: {gameDataPath}");
    }

    [Fact]
    public void OpenUrl_ShouldNotThrow()
    {
        // Arrange
        var url = "https://example.com";

        // Act
        Action act = () => _platformService.OpenUrl(url);

        // Assert - 我们不检查是否打开成功（可能在CI环境中没有浏览器），只检查不抛异常
        act.Should().NotThrow();
    }

    [Fact]
    public void OpenFolder_ShouldNotThrow()
    {
        // Arrange
        var tempPath = Path.GetTempPath();

        // Act
        Action act = () => _platformService.OpenFolder(tempPath);

        // Assert - 我们不检查是否打开成功，只检查不抛异常
        act.Should().NotThrow();
    }
}
