using PCL_CE.Neo.Core.Network;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class NetworkServiceTests
{
    [Fact]
    public void NetworkService_CanBeCreated()
    {
        var service = new NetworkService();
        Assert.NotNull(service.HttpClient);
    }

    [Fact]
    public void NetworkService_SetsUserAgent()
    {
        var service = new NetworkService();
        var userAgent = service.HttpClient.DefaultRequestHeaders.UserAgent.ToString();
        Assert.Contains("PCL-CE.Neo", userAgent);
    }

    [Fact]
    public void NetworkService_CanSetProxy()
    {
        var service = new NetworkService();
        service.SetProxy("127.0.0.1", 8080);
    }

    [Fact]
    public void NetworkService_CanClearProxy()
    {
        var service = new NetworkService();
        service.SetProxy("127.0.0.1", 8080);
        service.ClearProxy();
    }
}
