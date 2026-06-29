using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link;

public interface ILinkAnnounce
{
    Task<bool> AnnounceServerAsync(string serverAddress, int port = 25565);
    Task<bool> UnannounceServerAsync(string serverAddress, int port = 25565);
    Task<List<AnnouncedServer>> GetAnnouncedServersAsync();
    Task<bool> RefreshAnnouncementAsync(string serverAddress, int port = 25565);
}

public class LinkAnnounce : ILinkAnnounce
{
    private readonly Dictionary<string, AnnouncedServer> _announcedServers = new();
    private readonly TimeSpan _announcementExpiry = TimeSpan.FromMinutes(30);

    public async Task<bool> AnnounceServerAsync(string serverAddress, int port = 25565)
    {
        try
        {
            var key = $"{serverAddress}:{port}";
            var server = new AnnouncedServer
            {
                ServerAddress = serverAddress,
                Port = port,
                AnnouncedAt = DateTime.Now,
                LastRefreshed = DateTime.Now
            };

            _announcedServers[key] = server;

            await SendAnnouncementToCentralServerAsync(server);

            LogWrapper.Info($"Announced server: {serverAddress}:{port}");
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to announce server: {serverAddress}:{port}");
            return false;
        }
    }

    public async Task<bool> UnannounceServerAsync(string serverAddress, int port = 25565)
    {
        try
        {
            var key = $"{serverAddress}:{port}";
            _announcedServers.Remove(key);

            await SendUnannouncementToCentralServerAsync(serverAddress, port);

            LogWrapper.Info($"Unannounced server: {serverAddress}:{port}");
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to unannounce server: {serverAddress}:{port}");
            return false;
        }
    }

    public async Task<List<AnnouncedServer>> GetAnnouncedServersAsync()
    {
        try
        {
            await CleanupExpiredAnnouncements();
            return new List<AnnouncedServer>(_announcedServers.Values);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to get announced servers");
            return new List<AnnouncedServer>();
        }
    }

    public async Task<bool> RefreshAnnouncementAsync(string serverAddress, int port = 25565)
    {
        try
        {
            var key = $"{serverAddress}:{port}";
            if (_announcedServers.TryGetValue(key, out var server))
            {
                server.LastRefreshed = DateTime.Now;
                await SendAnnouncementToCentralServerAsync(server);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to refresh announcement: {serverAddress}:{port}");
            return false;
        }
    }

    private async Task SendAnnouncementToCentralServerAsync(AnnouncedServer server)
    {
        try
        {
            using var client = new System.Net.Http.HttpClient();
            var request = new System.Net.Http.HttpRequestMessage(
                System.Net.Http.HttpMethod.Post,
                "https://api.pcl-ce.net/v1/servers/announce");

            var content = new System.Net.Http.Json.JsonContent(new
            {
                server.ServerAddress,
                server.Port,
                server.AnnouncedAt,
                server.LastRefreshed
            });

            request.Content = content;

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to send announcement to central server");
        }
    }

    private async Task SendUnannouncementToCentralServerAsync(string serverAddress, int port)
    {
        try
        {
            using var client = new System.Net.Http.HttpClient();
            var response = await client.DeleteAsync(
                $"https://api.pcl-ce.net/v1/servers/announce?address={serverAddress}&port={port}");
            
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to send unannouncement to central server");
        }
    }

    private Task CleanupExpiredAnnouncements()
    {
        try
        {
            var expiredKeys = _announcedServers
                .Where(kv => DateTime.Now - kv.Value.LastRefreshed > _announcementExpiry)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _announcedServers.Remove(key);
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to cleanup expired announcements");
        }

        return Task.CompletedTask;
    }
}

public class AnnouncedServer
{
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 25565;
    public DateTime AnnouncedAt { get; set; } = DateTime.Now;
    public DateTime LastRefreshed { get; set; } = DateTime.Now;
    public bool IsOnline { get; set; } = true;
}