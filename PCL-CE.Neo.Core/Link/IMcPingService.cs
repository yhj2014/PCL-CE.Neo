using System.Net;

namespace PCL_CE.Neo.Core.Link;

public enum PingProtocol
{
    Modern,
    Legacy
}

public record PingResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public PingProtocol Protocol { get; init; }
    
    public string? VersionName { get; init; }
    public int ProtocolVersion { get; init; }
    
    public string? Description { get; init; }
    public string? DescriptionRaw { get; init; }
    
    public int PlayerCount { get; init; }
    public int MaxPlayers { get; init; }
    
    public string? Favicon { get; init; }
    
    public long Latency { get; init; }
    
    public string? ModInfo { get; init; }
    public bool IsModded { get; init; }

    public static PingResult Failed(string error) => new() { Success = false, Error = error };
    public static PingResult Succeeded(PingProtocol protocol, long latency) => 
        new() { Success = true, Protocol = protocol, Latency = latency };
}

public interface IMcPingService
{
    Task<PingResult?> PingAsync(CancellationToken cancellationToken = default);
    void Dispose();
}

public interface IMcPingServiceFactory
{
    IMcPingService CreateService(string host, int port, int timeout = 5000);
    IMcPingService CreateLegacyService(string host, int port, int timeout = 5000);
}
