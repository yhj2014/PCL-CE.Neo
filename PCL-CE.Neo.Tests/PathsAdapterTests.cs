using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Tests;

public class PathsAdapterTests
{
    private readonly PathsAdapter _paths;

    public PathsAdapterTests()
    {
        _paths = new PathsAdapter(new DefaultPlatformService());
    }

    [Fact]
    public void PathsAdapter_Should_Initialize_Successfully()
    {
        _paths.Should().NotBeNull();
    }

    [Fact]
    public void ApplicationDataPath_Should_Return_Valid_Path()
    {
        var path = _paths.ApplicationDataPath;
        path.Should().NotBeNullOrEmpty();
        Path.IsPathRooted(path).Should().BeTrue();
    }

    [Fact]
    public void TemporaryPath_Should_Return_Valid_Path()
    {
        var path = _paths.TemporaryPath;
        path.Should().NotBeNullOrEmpty();
        Path.IsPathRooted(path).Should().BeTrue();
    }

    [Fact]
    public void MinecraftPath_Should_Return_Valid_Path()
    {
        var path = _paths.MinecraftPath;
        path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EnsureDirectoryExists_Should_Work()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "PCL_CE_Neo_Test_Dir");
        try
        {
            _paths.EnsureDirectoryExists(tempPath);
            Directory.Exists(tempPath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath);
        }
    }
}
