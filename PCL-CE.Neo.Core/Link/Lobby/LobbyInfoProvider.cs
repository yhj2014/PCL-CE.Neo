using System;
using System.Numerics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link.Lobby;

public static class LobbyInfoProvider
{
    public static bool IsLobbyAvailable { get; set; } = false;
    public static bool AllowCustomName { get; set; } = false;
    public static bool RequiresLogin { get; set; } = true;
    public static bool RequiresRealName { get; set; } = true;
    public static int ProtocolVersion { get; set; } = 6;

    public class LobbyInfo
    {
        public required string OriginalCode { get; init; }
        public required LobbyType Type { get; init; }
        public required string NetworkName { get; init; }
        public required string NetworkSecret { get; init; }
        public string? Ip { get; init; }
        public required int Port { get; init; }
    }

    public enum LobbyType
    {
        PCLCE,
        Terracotta
    }

    public static LobbyInfo? TargetLobby { get; set; }
    public static int JoinerLocalPort { get; set; }

    private static readonly ILogger _logger = ServiceLocator.GetService<ILogger<LobbyInfoProvider>>() ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<LobbyInfoProvider>();

    public static LobbyInfo? ParseCode(string code)
    {
        code = code.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(code) || code.Length < 9 || !StringUtils.IsAscii(code))
        {
            _logger.LogError("无效的大厅编号: {Code}", code);
            return null;
        }

        if (code.Split("-".ToCharArray()).Length != 5)
        {
            try
            {
                var info = StringUtils.FromB32ToB10(code);
                return new LobbyInfo
                {
                    OriginalCode = code,
                    NetworkName = info[..8],
                    NetworkSecret = info[8..10],
                    Port = int.Parse(info[10..]),
                    Type = LobbyType.PCLCE,
                    Ip = "10.114.51.41"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "大厅编号解析失败，可能是无效的 PCL CE 大厅编号: {Code}", code);
            }
        }
        else
        {
            var matches = StringUtils.RegexSearch(code, RegexPatterns.TerracottaId);
            if (matches.Count == 0)
            {
                _logger.LogError("大厅编号解析失败，可能是无效的陶瓦大厅编号: {Code}", code);
                return null;
            }

            foreach (var match in matches)
            {
                var codeString = match.Replace("I", "1").Replace("O", "0").Replace("-", "");
                BigInteger value = 0;
                var checking = 0;
                const string baseChars = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";
                for (var i = 0; i <= 23; i++)
                {
                    var j = baseChars.IndexOf(codeString[i]);
                    value += BigInteger.Parse(j.ToString()) * BigInteger.Pow(34, i);
                    checking = (j + checking) % 34;
                }

                if (checking != baseChars.IndexOf(codeString[24])) { return null; }
                var port = (int)(value % 65536);
                if (port < 100) { return null; }
                return new LobbyInfo
                {
                    OriginalCode = code,
                    NetworkName = codeString.Substring(0, 15).ToLower(),
                    NetworkSecret = codeString.Substring(15, 10).ToLower(),
                    Port = port,
                    Type = LobbyType.Terracotta,
                    Ip = "10.144.144.1"
                };
            }
        }
        return null;
    }
}