using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsUIAccessProviderIntegrationTests
{
    private readonly IUIAccessProvider _uiAccessProvider;

    public WindowsUIAccessProviderIntegrationTests()
    {
        _uiAccessProvider = new WindowsUIAccessProvider();
    }

    [Fact]
    public void GetScreenDpi_ShouldReturnValidDpi()
    {
        // Act
        var dpi = _uiAccessProvider.GetScreenDpi();

        // Assert
        dpi.Should().BeGreaterThan(0);
        dpi.Should().BeLessOrEqualTo(300); // 合理的最大DPI值
        Debug.WriteLine($"Screen DPI: {dpi}");
    }

    [Fact]
    public void GetScreenSize_ShouldReturnValidSize()
    {
        // Act
        var screenSize = _uiAccessProvider.GetScreenSize();

        // Assert
        screenSize.Width.Should().BeGreaterThan(0);
        screenSize.Height.Should().BeGreaterThan(0);
        Debug.WriteLine($"Screen Size: {screenSize.Width}x{screenSize.Height}");
    }

    [Fact]
    public void Invoke_ShouldNotThrow()
    {
        // Arrange
        var executed = false;

        // Act
        Action act = () => _uiAccessProvider.Invoke(() => executed = true);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotThrow()
    {
        // Arrange
        var executed = false;

        // Act & Assert
        Func<Task> act = () => _uiAccessProvider.InvokeAsync(() => executed = true);
        await act.Should().NotThrowAsync();
    }
}
