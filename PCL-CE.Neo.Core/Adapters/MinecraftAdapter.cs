using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class MinecraftAdapter : IMinecraftAdapter
{
    private readonly ILogger<MinecraftAdapter> _logger;
    private readonly IPathsAdapter _pathsAdapter;
    private readonly IJavaScanner _javaScanner;
    private readonly IDownloadAdapter _downloadAdapter;
    private readonly INetworkAdapter _networkAdapter;
    private Process? _currentGameProcess;

    public MinecraftAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<MinecraftAdapter>.Instance,
        new PathsAdapter(),
        new PCL_CE.Neo.Core.Abstractions.Mock.JavaScannerMock(),
        new DownloadAdapter(),
        new NetworkAdapter())
    {
    }

    public MinecraftAdapter(
        ILogger<MinecraftAdapter> logger,
        IPathsAdapter pathsAdapter,
        IJavaScanner javaScanner,
        IDownloadAdapter downloadAdapter,
        INetworkAdapter networkAdapter)
    {
        _logger = logger;
        _pathsAdapter = pathsAdapter;
        _javaScanner = javaScanner;
        _downloadAdapter = downloadAdapter;
        _networkAdapter = networkAdapter;
    }

    public IJavaScanner JavaScanner => _javaScanner;

    public async Task<string> GetMinecraftVersionListAsync()
    {
        try
        {
            var url = "https://launchermeta.mojang.com/mc/game/version_manifest_v2.json";
            return await _networkAdapter.GetAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 Minecraft 版本列表失败");
            return "{}";
        }
    }

    public async Task<JavaInstallation?> DetectJavaAsync()
    {
        var javaPath = await Task.Run(() =>
        {
            foreach (var path in _javaScanner.ScanJavaPaths())
            {
                try
                {
                    var version = GetJavaVersion(path);
                    if (!string.IsNullOrEmpty(version))
                    {
                        return new JavaInstallation
                        {
                            Path = path,
                            Version = version,
                            Bits = Is64BitJava(path) ? 64 : 32,
                            DisplayName = $"Java {version} ({path})"
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "检测 Java 失败: {Path}", path);
                }
            }
            return null;
        });

        return javaPath;
    }

    public IEnumerable<JavaInstallation> GetInstalledJavaVersions()
    {
        var versions = new List<JavaInstallation>();
        foreach (var path in _javaScanner.ScanJavaPaths())
        {
            try
            {
                var version = GetJavaVersion(path);
                if (!string.IsNullOrEmpty(version))
                {
                    versions.Add(new JavaInstallation
                    {
                        Path = path,
                        Version = version,
                        Bits = Is64BitJava(path) ? 64 : 32,
                        DisplayName = $"Java {version} ({path})"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "获取 Java 版本失败: {Path}", path);
            }
        }
        return versions;
    }

    public async Task<GameLaunchResult> LaunchGameAsync(GameLaunchOptions options)
    {
        try
        {
            _logger.LogInformation("正在启动游戏: {Instance} ({Version})",
                options.InstanceName ?? options.InstanceId, options.MinecraftVersion);

            var gameDir = options.GameDirectory;
            Directory.CreateDirectory(gameDir);

            var nativesDir = Path.Combine(gameDir, "natives");
            Directory.CreateDirectory(nativesDir);

            var libraries = await DownloadLibrariesAsync(gameDir, options.MinecraftVersion);
            if (libraries == null)
            {
                return GameLaunchResult.Failed("下载依赖库失败");
            }

            var classPath = BuildClassPath(libraries, gameDir, options.MinecraftVersion);

            var arguments = BuildJvmArguments(options, nativesDir, classPath);

            var processInfo = new ProcessStartInfo
            {
                FileName = options.Java.Path,
                Arguments = arguments,
                WorkingDirectory = gameDir,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processInfo);
            if (process == null)
            {
                return GameLaunchResult.Failed("无法启动游戏进程");
            }

            _currentGameProcess = process;

            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                _logger.LogInformation("游戏进程已退出，退出代码: {ExitCode}", process.ExitCode);
                _currentGameProcess = null;
            };

            _logger.LogInformation("游戏已启动，进程 ID: {ProcessId}", process.Id);
            return GameLaunchResult.Succeeded(process.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动游戏失败");
            return GameLaunchResult.Failed(ex.Message, ex);
        }
    }

    public async Task KillGameAsync()
    {
        if (_currentGameProcess != null && !_currentGameProcess.HasExited)
        {
            _logger.LogInformation("正在关闭游戏进程");
            await Task.Run(() =>
            {
                try
                {
                    _currentGameProcess.Kill(entireProcessTree: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "关闭游戏进程失败");
                }
            });
            _currentGameProcess = null;
        }
    }

    public bool IsGameRunning => _currentGameProcess != null && !_currentGameProcess.HasExited;

    public string GetGameDirectory(string instanceId)
    {
        return Path.Combine(_pathsAdapter.Data, "instances", instanceId);
    }

    public string GetMinecraftDirectory()
    {
        return Path.Combine(_pathsAdapter.SharedData, ".minecraft");
    }

    private string GetJavaVersion(string javaPath)
    {
        try
        {
            var javaExe = GetJavaExecutable(javaPath);

            var psi = new ProcessStartInfo
            {
                FileName = javaExe,
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return "";

            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(5000);

            var match = System.Text.RegularExpressions.Regex.Match(error, @"version\s+""(.+?)""");
            return match.Success ? match.Groups[1].Value : "";
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "获取 Java 版本信息失败: {Path}", javaPath);
            return "";
        }
    }

    private string GetJavaExecutable(string javaPath)
    {
        var exeExtensions = new[] { ".exe", "" };
        
        if (Path.GetFileName(javaPath).StartsWith("java"))
        {
            if (File.Exists(javaPath))
                return javaPath;
            
            foreach (var ext in exeExtensions)
            {
                var testPath = javaPath + ext;
                if (File.Exists(testPath))
                    return testPath;
            }
        }
        
        var javaBinDir = Path.Combine(javaPath, "bin");
        foreach (var name in new[] { "java", "javaw" })
        {
            foreach (var ext in exeExtensions)
            {
                var testPath = Path.Combine(javaBinDir, name + ext);
                if (File.Exists(testPath))
                    return testPath;
            }
        }
        
        return javaPath;
    }
    
    private bool Is64BitJava(string javaPath)
    {
        return javaPath.Contains("64") || _pathsAdapter.SharedData.Contains("64");
    }

    private async Task<List<string>?> DownloadLibrariesAsync(string gameDir, string version)
    {
        var libraries = new List<string>();
        var librariesDir = Path.Combine(gameDir, "libraries");
        if (!Directory.Exists(librariesDir))
        {
            Directory.CreateDirectory(librariesDir);
        }

        // 读取版本 JSON 获取库列表
        var versionJsonPath = Path.Combine(gameDir, "versions", version, $"{version}.json");
        if (!File.Exists(versionJsonPath))
        {
            // 如果没有 version.json，返回空列表
            return libraries;
        }

        try
        {
            var versionJson = await File.ReadAllTextAsync(versionJsonPath);
            var versionData = System.Text.Json.JsonDocument.Parse(versionJson);
            
            if (versionData.RootElement.TryGetProperty("libraries", out var librariesElement))
            {
                foreach (var lib in librariesElement.EnumerateArray())
                {
                    if (!lib.TryGetProperty("downloads", out var downloads)) continue;
                    if (!downloads.TryGetProperty("artifact", out var artifact)) continue;
                    
                    if (artifact.TryGetProperty("path", out var path) && path.GetString() is { } libPath)
                    {
                        var localPath = Path.Combine(librariesDir, libPath);
                        
                        // 检查文件是否存在，文件大小是否匹配
                        var isValid = true;
                        if (File.Exists(localPath) && artifact.TryGetProperty("size", out var size) && size.GetInt32() is { } expectedSize)
                        {
                            var fileInfo = new FileInfo(localPath);
                            if (fileInfo.Length != expectedSize)
                            {
                                isValid = false;
                            }
                        }
                        else if (!File.Exists(localPath))
                        {
                            isValid = false;
                        }

                        if (!isValid)
                        {
                            // 如果文件不存在或大小不匹配，尝试下载
                            if (artifact.TryGetProperty("url", out var url) && url.GetString() is { } downloadUrl)
                            {
                                try
                                    {
                                        var libDir = Path.GetDirectoryName(localPath)!;
                                        if (!Directory.Exists(libDir))
                                        {
                                            Directory.CreateDirectory(libDir);
                                        }
                                        
                                        var request = new DownloadRequest
                                        {
                                            Url = downloadUrl,
                                            DestinationPath = localPath
                                        };
                                        await _downloadAdapter.DownloadFileAsync(request, CancellationToken.None);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, $"Failed to download library {libPath}");
                                        continue;
                                    }
                            }
                        }

                        if (File.Exists(localPath))
                        {
                            libraries.Add(localPath);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error parsing version json {versionJsonPath}");
        }

        return libraries;
    }

    private string BuildClassPath(List<string> libraries, string gameDir, string version)
    {
        var pathSeparator = Path.PathSeparator;
        
        var classPathParts = new List<string>();
        
        // 添加版本 JAR
        var versionJar = Path.Combine(gameDir, "versions", version, $"{version}.jar");
        if (File.Exists(versionJar))
        {
            classPathParts.Add(versionJar);
        }
        
        // 添加所有库
        classPathParts.AddRange(libraries);
        
        return string.Join(pathSeparator, classPathParts);
    }

    private string BuildJvmArguments(GameLaunchOptions options, string nativesDir, string classPath)
    {
        var args = new List<string>();

        args.Add($"-Xmx{options.MemoryMB ?? 2048}M");
        args.Add($"-Xms{(options.MemoryMB ?? 2048) / 2}M");

        if (!string.IsNullOrEmpty(options.JavaArguments))
        {
            args.Add(options.JavaArguments);
        }

        args.Add($"-Djava.library.path=\"{nativesDir}\"");
        args.Add("-cp");
        args.Add(classPath);

        args.Add("net.minecraft.client.Main");
        args.Add("--username");
        args.Add(options.Username);
        args.Add("--version");
        args.Add(options.MinecraftVersion);
        args.Add("--gameDir");
        args.Add(options.GameDirectory);
        args.Add("--assetsDir");
        args.Add(Path.Combine(_pathsAdapter.SharedData, "assets"));
        args.Add("--assetIndex");
        args.Add(options.MinecraftVersion);
        args.Add("--uuid");
        args.Add(options.Uuid);
        args.Add("--accessToken");
        args.Add(options.AccessToken);

        if (!string.IsNullOrEmpty(options.WindowTitle))
        {
            args.Add("--title");
            args.Add(options.WindowTitle);
        }

        if (options.WindowWidth.HasValue)
        {
            args.Add("--width");
            args.Add(options.WindowWidth.Value.ToString());
        }

        if (options.WindowHeight.HasValue)
        {
            args.Add("--height");
            args.Add(options.WindowHeight.Value.ToString());
        }

        if (options.Fullscreen == true)
        {
            args.Add("--fullscreen");
        }

        if (!string.IsNullOrEmpty(options.GameArguments))
        {
            args.Add(options.GameArguments);
        }

        return string.Join(" ", args);
    }
}
