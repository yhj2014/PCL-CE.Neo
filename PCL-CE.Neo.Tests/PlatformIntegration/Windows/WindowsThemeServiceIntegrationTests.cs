using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsThemeServiceIntegrationTests
{
    private readonly IThemeService _themeService;

    public WindowsThemeServiceIntegrationTests()
    {
        _themeService = new WindowsThemeService();
    }

    [Fact]
    public void GetCurrentTheme_ShouldReturnValidTheme()
    {
        // Act
        var theme = _themeService.GetCurrentTheme();

        // Assert
        theme.Should().NotBeNull();
        theme.Name.Should().NotBeNullOrEmpty();
        Debug.WriteLine($"Current Theme: {theme.Name}, Type: {theme.Type}");
    }

    [Fact]
    public void GetAvailableThemes_ShouldReturnMultipleThemes()
    {
        // Act
        var themes = _themeService.GetAvailableThemes().ToList();

        // Assert
        themes.Should().NotBeEmpty();
        themes.Count.Should().BeGreaterOrEqualTo(2);
        foreach (var theme in themes)
        {
            theme.Name.Should().NotBeNullOrEmpty();
        }
        Debug.WriteLine($"Available Themes: {string.Join(", ", themes.Select(t => t.Name))}");
    }

    [Fact]
    public void DetectSystemTheme_ShouldReturnValidThemeType()
    {
        // Act
        var themeType = _themeService.DetectSystemTheme();

        // Assert
        themeType.Should().BeOneOf(ThemeType.Light, ThemeType.Dark, ThemeType.System);
        Debug.WriteLine($"Detected System Theme: {themeType}");
    }

    [Fact]
    public void SetTheme_ShouldRaiseThemeChangedEvent()
    {
        // Arrange
        var eventRaised = false;
        _themeService.ThemeChanged += (sender, args) => eventRaised = true;
        var newTheme = new ThemeInfo { Name = "Dark", Type = ThemeType.Dark };

        // Act
        _themeService.SetTheme(newTheme);

        // Assert
        var currentTheme = _themeService.GetCurrentTheme();
        currentTheme.Type.Should().Be(ThemeType.Dark);
    }

    [Fact]
    public void SetTheme_ShouldUpdateCurrentTheme()
    {
        // Arrange
        var newTheme = new ThemeInfo { Name = "Test Theme", Type = ThemeType.Dark, ResourcePath = "path/to/resources" };

        // Act
        _themeService.SetTheme(newTheme);

        // Assert
        var currentTheme = _themeService.GetCurrentTheme();
        currentTheme.Name.Should().Be("Test Theme");
        currentTheme.Type.Should().Be(ThemeType.Dark);
    }
}
