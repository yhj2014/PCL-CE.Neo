using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.IO;
using PCL_CE.Neo.Core.Network;

namespace PCL_CE.Neo.Core.Minecraft;

public interface IGameLauncher
{
    Task<Process> LaunchGameAsync(GameInstance instance, LaunchOptions options);
    event Action<string>? GameOutput;
    event Action<int>? GameExited;
}

public record LaunchOptions(
    string GameDirectory,
    string JavaPath,
    int MaxMemory,
    int MinMemory,
    string? JvmArguments = null,
    string? GameArguments = null,
    string? AssetsDirectory = null,
    string? NativeDirectory = null,
    bool EnableInnocence = false
);

public class GameLauncher : IGameLauncher
{
    private readonly ILogger<GameLauncher> _logger;
    private readonly INetworkService _networkService;

    public event Action<string>? GameOutput;
    public event Action<int>? GameExited;

    public GameLauncher(ILogger<GameLauncher> logger, INetworkService networkService)
    {
        _logger = logger;
        _networkService = networkService;
    }

    public async Task<Process> LaunchGameAsync(GameInstance instance, LaunchOptions options)
    {
        _logger.LogInformation("Launching game {InstanceName} with Java {JavaPath}", 
            instance.Name, options.JavaPath);

        var processInfo = CreateProcess(instance, options);
        
        try
        {
            var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start game process");
            }

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    GameOutput?.Invoke(e.Data);
                }
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    GameOutput?.Invoke($"[ERROR] {e.Data}");
                }
            };
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.Exited += (s, e) =>
            {
                GameExited?.Invoke(process.ExitCode);
            };

            _logger.LogInformation("Game process started with PID {ProcessId}", process.Id);
            return process;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch game");
            throw;
        }
    }

    private ProcessStartInfo CreateProcess(GameInstance instance, LaunchOptions options)
    {
        var javaPath = options.JavaPath;
        var gameDir = options.GameDirectory;
        var assetsDir = options.AssetsDirectory ?? Path.Combine(gameDir, "assets");
        var nativesDir = options.NativeDirectory ?? Path.Combine(gameDir, "natives");
        var librariesDir = Path.Combine(gameDir, "libraries");
        var versionsDir = Path.Combine(gameDir, "versions", instance.GameCoreId);

        var classPath = BuildClassPath(librariesDir, versionsDir);
        
        var arguments = new List<string>();
        
        arguments.Add($"-Xms{options.MinMemory}M");
        arguments.Add($"-Xmx{options.MaxMemory}M");
        
        if (!string.IsNullOrEmpty(options.JvmArguments))
        {
            arguments.Add(options.JvmArguments);
        }

        arguments.Add($"-Djava.library.path=\"{nativesDir}\"");
        arguments.Add($"-Dminecraft.client.jar=\"{versionsDir}\\{instance.GameCoreId}.jar\"");
        arguments.Add($"-Dminecraft.assets.root=\"{assetsDir}\"");
        arguments.Add($"-Dminecraft.assets.index={GetAssetsIndex(instance.GameCoreId)}");

        arguments.Add("net.minecraft.client.main.Main");
        arguments.Add("--username ${username}");
        arguments.Add("--version ${version}");
        arguments.Add("--gameDir \"" + gameDir + "\"");
        arguments.Add("--assetsDir \"" + assetsDir + "\"");
        arguments.Add("--assetIndex ${assetIndex}");
        arguments.Add("--uuid ${uuid}");
        arguments.Add("--accessToken ${accessToken}");
        arguments.Add("--clientId ${clientId}");
        arguments.Add("--xuid ${xuid}");
        arguments.Add("--userType ${userType}");
        arguments.Add("--versionType ${versionType}");

        if (options.EnableInnocence)
        {
            arguments.Add("-- Innocence");
        }

        var argumentString = string.Join(" ", arguments);

        return new ProcessStartInfo
        {
            FileName = javaPath,
            Arguments = argumentString,
            WorkingDirectory = gameDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    private string BuildClassPath(string librariesDir, string versionsDir)
    {
        var classPath = new List<string>();
        
        var versionJar = Path.Combine(versionsDir, Path.GetFileName(versionsDir) + ".jar");
        if (File.Exists(versionJar))
        {
            classPath.Add(versionJar);
        }

        return string.Join(";", classPath);
    }

    private string GetAssetsIndex(string gameCoreId)
    {
        return gameCoreId switch
        {
            "1.7.10" => "1.7.10",
            "1.8.9" => "1.8",
            "1.12.2" => "1.12",
            "1.16.5" => "1.16",
            "1.17.1" => "1.17",
            "1.18.2" => "1.18",
            "1.19.2" => "1.19",
            "1.19.4" => "1.19.4",
            "1.20.1" => "1.20.1",
            "1.20.4" => "1.20.4",
            "1.21" => "1.21",
            "1.21.1" => "1.21.1",
            "1.21.3" => "1.21.3",
            _ => "1.20.1"
        };
    }
}
