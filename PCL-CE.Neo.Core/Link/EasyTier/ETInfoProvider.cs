using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.Link.EasyTier;

public enum ETConnectionType
{
    Local,
    P2P,
    Relay,
    Unknown
}

public class ETPlayerInfo
{
    public required bool IsHost { get; init; }
    public required string Hostname { get; init; }
    public string? Username { get; init; }
    public string? McName { get; init; }
    public ETConnectionType Cost { get; init; } = ETConnectionType.Unknown;
    public double Ping { get; set; }
    public double Loss { get; init; }
    public string? NatType { get; init; }
    public string? ETVersion { get; init; }
}

public static class ETInfoProvider
{
    public const string ETNetworkNamePrefix = "PCLCELobby";
    public const string ETNetworkSecretPrefix = "PCLCEETLOBBY2025";
    public static readonly string ETPath = Path.Combine(Paths.SharedLocalData, "EasyTier", "1.8.0",
        "easytier-windows-" + (RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x86_64"));

    private static readonly ILogger _logger = ServiceLocator.GetService<ILogger<ETInfoProvider>>() ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<ETInfoProvider>();

    private static ETConnectionType _GetConnectionType(string cost)
    {
        if (IsContains("p2p")) return ETConnectionType.P2P;
        if (IsContains("relay")) return ETConnectionType.Relay;
        if (IsContains("local")) return ETConnectionType.Local;
        return ETConnectionType.Unknown;
        bool IsContains(string str) => cost.Contains(str, StringComparison.InvariantCultureIgnoreCase);
    }

    public static async Task<int> CheckETStatusAsync()
    {
        var retryCount = 0;
        var process = ETController.ETProcess;
        while (process == null && retryCount < 10)
        {
            await Task.Delay(1000);
            retryCount++;
        }
        if (process != null)
        {
            while (ETController.Status != ETState.Ready)
            {
                var info = GetPlayerList().Item1?[0];
                if (info == null)
                {
                    await Task.Delay(1000);
                    continue;
                }
                if (info.Ping != 1000) { ETController.Status = ETState.Ready; }
                await Task.Delay(1000);
            }
            return 0;
        }

        return 1;
    }

    public static Tuple<List<ETPlayerInfo>?, ETPlayerInfo?> GetPlayerList()
    {
        try
        {
            using var cliProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $"{ETPath}\\easytier-cli.exe",
                    WorkingDirectory = ETPath,
                    Arguments = $"--rpc-portal 127.0.0.1:{ETController.ETRpcPort} -o json peer",
                    ErrorDialog = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = true
            };

            cliProcess.Start();
            cliProcess.WaitForExit(180);

            var output = cliProcess.StandardOutput.ReadToEnd() + cliProcess.StandardError.ReadToEnd();
            if (!cliProcess.HasExited)
            {
                _logger.LogWarning("Cli 获取结果超时(180 ms)，程序状态可能异常！");
                _logger.LogWarning("获取到 EasyTier Cli 信息: \r\n{Output}", output);
            }

            var playerList = new List<ETPlayerInfo>();
            ETPlayerInfo? localInfo = null;
            if (JsonNode.Parse(output) is not JsonArray json)
                return new Tuple<List<ETPlayerInfo>?, ETPlayerInfo?>(null, null);
            foreach (var p in json)
            {
                var info = p.Deserialize<ETPeerInfo>();
                if (info == null) { continue; }
                if (info.Hostname.StartsWith("PublicServer")) { continue; }
                var hostnameSplit = info.Hostname.Split('|');
                var playerInfo = new ETPlayerInfo
                {
                    IsHost = info.Hostname.StartsWith("H|") || info.Ipv4 == "10.144.144.1",
                    Hostname = info.Hostname,
                    Username = hostnameSplit.Length >= 2 ? hostnameSplit[1] : null,
                    McName = hostnameSplit.Length == 3 ? hostnameSplit[2] : null,
                    Cost = _GetConnectionType(info.Cost),
                    Ping = Math.Round(Convert.ToDouble(info.Ping != "-" ? info.Ping : "0")),
                    Loss = Math.Round(Convert.ToDouble(info.Loss != "-" ? info.Loss.Replace("%", "") : "0")),
                    NatType = info.NatType,
                    ETVersion = info.ETVersion
                };

                if (playerInfo.IsHost)
                {
                    playerList.Insert(0, playerInfo);
                }
                else
                {
                    playerList.Add(playerInfo);
                }
                if (playerInfo.Cost == ETConnectionType.Local)
                {
                    localInfo = playerInfo;
                }
            }
            return new Tuple<List<ETPlayerInfo>?, ETPlayerInfo?>(playerList, localInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 EasyTier 网络成员列表失败");
            return new Tuple<List<ETPlayerInfo>?, ETPlayerInfo?>(null, null);
        }
    }
}