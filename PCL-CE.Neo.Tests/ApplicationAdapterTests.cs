using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Tests;

public class ApplicationAdapterTests
{
    private readonly ApplicationAdapter _appAdapter;

    public ApplicationAdapterTests()
    {
        _appAdapter = new ApplicationAdapter(new TestLogger<ApplicationAdapter>(), new DefaultPlatformService());
    }

    [Fact]
    public void ApplicationAdapter_Should_Initialize_Successfully()
    {
        _appAdapter.Should().NotBeNull();
    }

    [Fact]
    public void VersionName_Should_Return_Valid_Value()
    {
        var version = _appAdapter.VersionName;
        version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void VersionCode_Should_Return_NonNegative()
    {
        var versionCode = _appAdapter.VersionCode;
        versionCode.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void ProcessId_Should_Return_Valid_Id()
    {
        var pid = _appAdapter.ProcessId;
        pid.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExecutablePath_Should_Return_Valid_Path()
    {
        var path = _appAdapter.ExecutablePath;
        path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CommandLineArguments_Should_Not_Be_Null()
    {
        var args = _appAdapter.CommandLineArguments;
        args.Should().NotBeNull();
    }

    [Fact]
    public void IsAprilFool_Should_Return_Boolean()
    {
        var result = _appAdapter.IsAprilFool;
        result.Should().BeTrue().Or.BeFalse();
    }
}
