using Xunit;
using FluentAssertions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL_CE.Neo.Tests;

public class DatabaseAdapterTests
{
    private readonly DatabaseAdapter _dbAdapter;

    public DatabaseAdapterTests()
    {
        _dbAdapter = new DatabaseAdapter();
    }

    [Fact]
    public void DatabaseAdapter_Should_Initialize_Successfully()
    {
        _dbAdapter.Should().NotBeNull();
    }

    [Fact]
    public void InitializeDatabase_Should_Not_Throw()
    {
        Action act = () => _dbAdapter.InitializeDatabase();
        act.Should().NotThrow();
    }

    [Fact]
    public void GetConnection_Should_Not_Throw()
    {
        Action act = () => _dbAdapter.GetConnection();
        act.Should().NotThrow();
    }
}
