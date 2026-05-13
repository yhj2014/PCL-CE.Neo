using PCL_CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests;

public class DownloadAdapterTests
{
    [Fact]
    public void DownloadRequest_DefaultValues()
    {
        var request = new DownloadRequest
        {
            Url = "https://example.com/file.jar",
            DestinationPath = "/tmp/file.jar"
        };

        Assert.True(request.ResumeSupported);
        Assert.Null(request.ExpectedHash);
        Assert.Null(request.Headers);
    }

    [Fact]
    public void DownloadResult_Success()
    {
        var result = DownloadResult.Succeeded("/tmp/file.jar", 1024, TimeSpan.FromSeconds(5));

        Assert.True(result.Success);
        Assert.Equal("/tmp/file.jar", result.FilePath);
        Assert.Equal(1024, result.BytesDownloaded);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
    }

    [Fact]
    public void DownloadResult_Failure()
    {
        var result = DownloadResult.Failed("/tmp/file.jar", "Network error");

        Assert.False(result.Success);
        Assert.Equal("Network error", result.ErrorMessage);
    }

    [Fact]
    public void DownloadProgress_CalculatesPercentage()
    {
        var progress = new DownloadProgress
        {
            Url = "https://example.com/file.jar",
            FilePath = "/tmp/file.jar",
            TotalBytes = 1000,
            DownloadedBytes = 500
        };

        Assert.Equal(50, progress.ProgressPercent);
    }

    [Fact]
    public void DownloadProgress_ZeroTotal_ReturnsZeroPercent()
    {
        var progress = new DownloadProgress
        {
            Url = "https://example.com/file.jar",
            FilePath = "/tmp/file.jar",
            TotalBytes = 0,
            DownloadedBytes = 0
        };

        Assert.Equal(0, progress.ProgressPercent);
    }
}
