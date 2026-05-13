using Xunit;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests;

public class MinecraftAdapterTests
{
    [Fact]
    public void GameLaunchOptions_RequiredProperties()
    {
        var options = new GameLaunchOptions
        {
            InstanceId = "test-instance",
            MinecraftVersion = "1.20.1",
            Java = new JavaInstallation
            {
                Path = "/usr/bin/java",
                Version = "17.0.1",
                Bits = 64
            },
            GameDirectory = "/tmp/game",
            Username = "TestPlayer",
            Uuid = "test-uuid",
            AccessToken = "test-token"
        };

        Assert.Equal("test-instance", options.InstanceId);
        Assert.Equal("1.20.1", options.MinecraftVersion);
        Assert.Equal("TestPlayer", options.Username);
    }

    [Fact]
    public void GameLaunchResult_Success()
    {
        var result = GameLaunchResult.Succeeded(12345);

        Assert.True(result.Success);
        Assert.Equal(12345, result.ProcessId);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void GameLaunchResult_Failure()
    {
        var result = GameLaunchResult.Failed("Test error", new InvalidOperationException("Test"));

        Assert.False(result.Success);
        Assert.Equal("Test error", result.ErrorMessage);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public void JavaInstallation_Is64Bit()
    {
        var java = new JavaInstallation
        {
            Path = "/usr/lib/jvm/java-17",
            Version = "17.0.1",
            Bits = 64
        };

        Assert.True(java.Is64Bit);
    }

    [Fact]
    public void JavaInstallation_IsNot64Bit()
    {
        var java = new JavaInstallation
        {
            Path = "/usr/lib/jvm/java-8",
            Version = "1.8.0",
            Bits = 32
        };

        Assert.False(java.Is64Bit);
    }
}
