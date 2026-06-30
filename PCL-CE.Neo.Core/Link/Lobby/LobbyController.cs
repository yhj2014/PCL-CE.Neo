using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.Link.Lobby;

public sealed class LobbyController
{
    public bool IsHost = false;

    private readonly ILogger _logger;

    public LobbyController(ILogger<LobbyController> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsHostInstanceAvailableAsync(int port)
    {
        try
        {
            _logger.LogDebug("检查本地 MC 实例是否可用: {Port}", port);
            using var tcpClient = new System.Net.Sockets.TcpClient();
            var result = await tcpClient.ConnectAsync("127.0.0.1", port).WaitAsync(TimeSpan.FromSeconds(2));
            tcpClient.Close();
            return true;
        }
        catch (Exception)
        {
            _logger.LogWarning("本地 MC 局域网实例 ({Port}) 疑似已关闭", port);
            return false;
        }
    }

    public async Task<int> CloseAsync()
    {
        try
        {
            _logger.LogInformation("关闭大厅连接");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭大厅连接时发生错误");
            return 1;
        }
    }
}