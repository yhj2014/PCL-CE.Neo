using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsClipboardServiceIntegrationTests
{
    private readonly IClipboardService _clipboardService;

    public WindowsClipboardServiceIntegrationTests()
    {
        _clipboardService = new WindowsClipboardService();
    }

    [Fact]
    public void SetText_ShouldNotThrow()
    {
        // Arrange
        var testText = "Hello, PCL-CE.Neo!";

        // Act & Assert
        Action act = () => _clipboardService.SetText(testText);
        act.Should().NotThrow();
    }

    [Fact]
    public void Clear_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _clipboardService.Clear();
        act.Should().NotThrow();
    }

    [Fact]
    public void GetText_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _clipboardService.GetText();
        act.Should().NotThrow();
    }

    [Fact]
    public void GetImage_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _clipboardService.GetImage();
        act.Should().NotThrow();
    }

    [Fact]
    public void SetImage_ShouldNotThrow()
    {
        // Arrange
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // 简单的PNG头

        // Act & Assert
        Action act = () => _clipboardService.SetImage(imageData);
        act.Should().NotThrow();
    }
}
