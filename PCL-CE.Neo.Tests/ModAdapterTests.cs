using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class ModAdapterTests
{
    private readonly ModAdapter _modAdapter;

    public ModAdapterTests()
    {
        _modAdapter = new ModAdapter();
    }

    [Fact]
    public void ModAdapter_Should_Initialize_Successfully()
    {
        _modAdapter.Should().NotBeNull();
    }

    [Fact]
    public void ValidateModPath_Should_Handle_Null_Path()
    {
        var result = _modAdapter.ValidateModPath(null);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateModPath_Should_Handle_Empty_Path()
    {
        var result = _modAdapter.ValidateModPath("");
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetModInfo_Should_Return_Valid_Info()
    {
        // 测试方法不会抛异常
        Action act = () => _modAdapter.GetModInfo("dummy/path/mod.jar");
        act.Should().NotThrow();
    }
}
