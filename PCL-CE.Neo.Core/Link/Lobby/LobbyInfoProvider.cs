using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Link.Lobby;

public interface ILobbyInfoProvider
{
    LobbyInfo? GetLobbyInfo(string lobbyCode);
    bool ValidateLobbyCode(string lobbyCode);
    string GenerateLobbyCode();
    string? GetServerAddressFromLobbyCode(string lobbyCode);
    bool IsLobbyCodeExpired(string lobbyCode);
}

public class LobbyInfoProvider : ILobbyInfoProvider
{
    private readonly Dictionary<string, LobbyInfo> _lobbyCache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public LobbyInfo? GetLobbyInfo(string lobbyCode)
    {
        try
        {
            if (!_lobbyCache.TryGetValue(lobbyCode, out var info))
                return null;

            if (info.CreatedAt + _cacheExpiry < DateTime.Now)
            {
                _lobbyCache.Remove(lobbyCode);
                return null;
            }

            return info;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to get lobby info for code: {lobbyCode}");
            return null;
        }
    }

    public bool ValidateLobbyCode(string lobbyCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(lobbyCode))
                return false;

            var code = lobbyCode.Trim();
            return code.Length >= 4 && code.Length <= 20 && 
                   code.All(c => char.IsLetterOrDigit(c) || c == '-');
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to validate lobby code: {lobbyCode}");
            return false;
        }
    }

    public string GenerateLobbyCode()
    {
        try
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var code = new char[8];
            
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = chars[RandomUtils.Random.Next(chars.Length)];
            }
            
            return new string(code);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to generate lobby code");
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }

    public string? GetServerAddressFromLobbyCode(string lobbyCode)
    {
        var info = GetLobbyInfo(lobbyCode);
        return info?.ServerAddress;
    }

    public bool IsLobbyCodeExpired(string lobbyCode)
    {
        var info = GetLobbyInfo(lobbyCode);
        return info == null;
    }

    public void AddLobbyInfo(string lobbyCode, LobbyInfo info)
    {
        _lobbyCache[lobbyCode] = info;
    }

    public void RemoveLobbyInfo(string lobbyCode)
    {
        _lobbyCache.Remove(lobbyCode);
    }

    public void CleanupExpiredLobbies()
    {
        var expiredCodes = _lobbyCache
            .Where(kv => kv.Value.CreatedAt + _cacheExpiry < DateTime.Now)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var code in expiredCodes)
        {
            _lobbyCache.Remove(code);
        }
    }
}

public class LobbyInfo
{
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 25565;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? PlayerName { get; set; }
    public int MaxPlayers { get; set; } = 20;
    public int CurrentPlayers { get; set; } = 1;
    public string? GameMode { get; set; }
}