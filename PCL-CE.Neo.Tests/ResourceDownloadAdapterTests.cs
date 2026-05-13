using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class ResourceDownloadAdapterTests
{
    private readonly ResourceDownloadAdapter _adapter;

    public ResourceDownloadAdapterTests()
    {
        _adapter = new ResourceDownloadAdapter();
    }

    [Fact]
    public void ResourceDownloadAdapter_Should_Initialize_Successfully()
    {
        _adapter.Should().NotBeNull();
    }

    [Fact]
    public void DownloadResource_Should_Not_Throw()
    {
        Action act = () => _adapter.DownloadResource("https://example.com", "test/output/path");
        act.Should().NotThrow();
    }

    [Fact]
    public void CancelDownload_Should_Not_Throw()
    {
        Action act = () => _adapter.CancelDownload();
        act.Should().NotThrow();
    }
}
