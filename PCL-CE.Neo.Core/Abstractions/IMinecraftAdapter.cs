using System.Diagnostics;

namespace PCL_CE.Neo.Core.Abstractions;

public interface IMinecraftAdapter
{
    IJavaScanner JavaScanner { get; }

    Task<string> GetMinecraftVersionListAsync();
    Task<JavaInstallation?> DetectJavaAsync();
    IEnumerable<JavaInstallation> GetInstalledJavaVersions();

    Task<GameLaunchResult> LaunchGameAsync(GameLaunchOptions options);
    Task KillGameAsync();
    bool IsGameRunning { get; }

    string GetGameDirectory(string instanceId);
    string GetMinecraftDirectory();
}

public record GameLaunchOptions
{
    public required string InstanceId { get; init; }
    public required string MinecraftVersion { get; init; }
    public required JavaInstallation Java { get; init; }
    public required string GameDirectory { get; init; }
    public string? JavaArguments { get; init; }
    public string? GameArguments { get; init; }
    public int? MemoryMB { get; init; }
    public string? WindowTitle { get; init; }
    public int? WindowWidth { get; init; }
    public int? WindowHeight { get; init; }
    public bool? Fullscreen { get; init; }
    public required string Username { get; init; }
    public required string Uuid { get; init; }
    public required string AccessToken { get; init; }
    public string? InstanceName { get; init; }
}

public record GameLaunchResult
{
    public bool Success { get; init; }
    public int? ProcessId { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static GameLaunchResult Succeeded(int processId) => new() { Success = true, ProcessId = processId };
    public static GameLaunchResult Failed(string message, Exception? ex = null) => new() { Success = false, ErrorMessage = message, Exception = ex };
}

public record JavaInstallation
{
    public required string Path { get; init; }
    public required string Version { get; init; }
    public required int Bits { get; init; }
    public string? DisplayName { get; init; }
    public bool Is64Bit => Bits == 64;
}
