using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class GetProtocolsHandler : IRequestHandler
{
    public string RequestType { get; } = "c:protocols";
    private readonly ILogger<GetProtocolsHandler> _logger;

    public GetProtocolsHandler(ILogger<GetProtocolsHandler> logger)
    {
        _logger = logger;
    }

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(
        ReadOnlyMemory<byte> requestBody, IServerContext context, string sessionId, CancellationToken ct)
    {
        string[] protocols =
        [
            "c:ping",
            "c:protocols",
            "c:server_port",
            "c:player_ping",
            "c:player_profiles_list"
        ];

        var responseContent = string.Join('\0', protocols);
        var responseBody = Encoding.ASCII.GetBytes(responseContent);

        _logger.LogDebug("[{SessionId}] 返回协议列表", sessionId);
        return Task.FromResult(((byte)0, new ReadOnlyMemory<byte>(responseBody)));
    }
}