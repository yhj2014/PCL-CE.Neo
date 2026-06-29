using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class GetProtocolsHandler : IRequestHandler
{
    public string RequestType { get; } = "c:protocols";

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(ReadOnlyMemory<byte> requestBody,
        IServerContext context, string sessionId, CancellationToken ct)
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

        return Task.FromResult(((byte)0, new ReadOnlyMemory<byte>(responseBody)));
    }
}