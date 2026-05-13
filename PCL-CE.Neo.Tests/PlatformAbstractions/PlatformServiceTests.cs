using Xunit;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core;

namespace PCL.CE.Neo.Tests.PlatformAbstractions;

public class PlatformDetectorTests
{
    [Fact]
    public void PlatformDetector_CurrentPlatform_ReturnsValidPlatform()
    {
        var platform = PlatformDetector.CurrentPlatform;
        Assert.NotNull(platform);
        Assert.Contains(platform, new[] { "Windows", "macOS", "Linux" });
    }

    [Fact]
    public void PlatformDetector_CurrentPlatform_IsConsistent()
    {
        var platform1 = PlatformDetector.CurrentPlatform;
        var platform2 = PlatformDetector.CurrentPlatform;
        Assert.Equal(platform1, platform2);
    }

    [Fact]
    public void PlatformDetector_IsLinux_ReturnsTrueOnLinux()
    {
        Assert.True(PlatformDetector.IsLinux);
    }

    [Fact]
    public void PlatformDetector_GetLineEnding_ReturnsCorrectEnding()
    {
        var lineEnding = PlatformDetector.GetLineEnding();
        Assert.NotNull(lineEnding);
        Assert.True(lineEnding == "\r\n" || lineEnding == "\n");
    }

    [Fact]
    public void PlatformDetector_GetPathSeparator_ReturnsCorrectSeparator()
    {
        var separator = PlatformDetector.GetPathSeparator();
        Assert.NotNull(separator);
        Assert.True(separator == ":" || separator == ";");
    }

    [Fact]
    public void PlatformDetector_NormalizePath_ConvertsForwardSlashes()
    {
        var normalized = PlatformDetector.NormalizePath("path/to/file");
        Assert.DoesNotContain("\\", normalized);
    }

    [Fact]
    public void PlatformDetector_NormalizePath_ConvertsBackslashesOnLinux()
    {
        var normalized = PlatformDetector.NormalizePath("path\\to\\file");
        Assert.DoesNotContain("\\", normalized);
        Assert.Contains("/", normalized);
    }
}
