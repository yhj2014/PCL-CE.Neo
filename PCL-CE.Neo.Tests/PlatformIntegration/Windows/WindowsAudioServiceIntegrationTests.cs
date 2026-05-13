using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Platform.Windows;
using System.Diagnostics;

namespace PCL_CE.Neo.Tests.PlatformIntegration.Windows;

[Trait("Category", "Integration")]
[Trait("Platform", "Windows")]
public class WindowsAudioServiceIntegrationTests
{
    private readonly IAudioService _audioService;

    public WindowsAudioServiceIntegrationTests()
    {
        _audioService = new WindowsAudioService();
    }

    [Fact]
    public void IsPlaying_ShouldInitiallyBeFalse()
    {
        // Act
        var isPlaying = _audioService.IsPlaying;

        // Assert
        isPlaying.Should().BeFalse();
    }

    [Fact]
    public void SetVolume_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _audioService.SetVolume(50);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetVolume_ShouldReturnDefaultValue()
    {
        // Act
        var volume = _audioService.GetVolume();

        // Assert
        volume.Should().BeGreaterOrEqualTo(0);
        volume.Should().BeLessOrEqualTo(100);
    }

    [Fact]
    public void Stop_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _audioService.Stop();
        act.Should().NotThrow();
    }

    [Fact]
    public void Pause_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _audioService.Pause();
        act.Should().NotThrow();
    }

    [Fact]
    public void Resume_ShouldNotThrow()
    {
        // Act & Assert
        Action act = () => _audioService.Resume();
        act.Should().NotThrow();
    }

    [Fact]
    public void Play_WithNonexistentFile_ShouldNotThrow()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent_audio_file.wav");

        // Act & Assert
        Action act = () => _audioService.Play(nonExistentFile);
        act.Should().NotThrow();
    }
}
