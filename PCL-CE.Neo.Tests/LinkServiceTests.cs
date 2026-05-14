using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link;
using PCL_CE.Neo.Core.Network;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class LinkServiceTests
{
    [Fact]
    public void LinkService_GeneratesLobbyCode()
    {
        var service = new LinkService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<LinkService>.Instance,
            new NetworkService());

        var code = service.GetLobbyCodeAsync().Result;
        
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.True(code.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void LinkService_GeneratesUniqueLobbyCodes()
    {
        var service = new LinkService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<LinkService>.Instance,
            new NetworkService());

        var codes = Enumerable.Range(0, 10)
            .Select(_ => service.GetLobbyCodeAsync().Result)
            .ToList();

        Assert.Equal(10, codes.Distinct().Count());
    }

    [Fact]
    public async Task LinkService_JoinLobbyReturnsResult()
    {
        var service = new LinkService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<LinkService>.Instance,
            new NetworkService());

        var result = await service.JoinLobbyAsync("ABC123");
        
        Assert.True(result);
    }
}
