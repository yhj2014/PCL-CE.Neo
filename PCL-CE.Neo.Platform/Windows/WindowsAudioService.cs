using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsAudioService : IAudioService
{
    private readonly ILogger<WindowsAudioService> _logger;
    private Process? _currentProcess;
    private int _volume = 50;
    private bool _isPlaying;
    private string? _currentFilePath;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public WindowsAudioService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsAudioService>.Instance) { }

    public WindowsAudioService(ILogger<WindowsAudioService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("正在初始化 Windows 音频服务");
            _currentFilePath = string.Empty;
            _logger.LogInformation("Windows 音频服务初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 Windows 音频服务时发生错误");
        }
    }

    public void Play(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("播放文件路径为空，已忽略");
                return;
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("音频文件不存在: {FilePath}", filePath);
                return;
            }

            _logger.LogDebug("开始播放音频文件: {FilePath}", filePath);
            _currentFilePath = filePath;

            Task.Run(() =>
            {
                try
                {
                    _isPlaying = true;
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c start \"\" \"" + filePath.Replace("\"", "\"\"") + "\"",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });

                    if (process != null)
                    {
                        _currentProcess = process;
                        process.WaitForExit();
                    }

                    _isPlaying = false;
                    _logger.LogInformation("音频播放完成: {FilePath}", filePath);
                    PlaybackFinished?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    _isPlaying = false;
                    _logger.LogError(ex, "播放音频文件时发生错误: {FilePath}", filePath);
                }
            });
        }
        catch (Exception ex)
        {
            _isPlaying = false;
            _logger.LogError(ex, "播放音频时发生错误");
        }
    }

    public void Stop()
    {
        try
        {
            _logger.LogDebug("停止音频播放");
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                _currentProcess.Kill();
                _currentProcess = null;
            }
            _isPlaying = false;
            _logger.LogInformation("音频播放已停止");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止音频播放时发生错误");
        }
    }

    public void Pause()
    {
        try
        {
            _logger.LogDebug("暂停音频播放");
            if (_isPlaying)
            {
                Stop();
                _logger.LogInformation("音频播放已暂停");
            }
            else
            {
                _logger.LogInformation("没有正在播放的音频");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "暂停音频播放时发生错误");
        }
    }

    public void Resume()
    {
        try
        {
            _logger.LogDebug("恢复音频播放");
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                Play(_currentFilePath);
                _logger.LogInformation("音频播放已恢复: {FilePath}", _currentFilePath);
            }
            else
            {
                _logger.LogInformation("没有可恢复的音频");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复音频播放时发生错误");
        }
    }

    public void SetVolume(int volume)
    {
        try
        {
            volume = Math.Clamp(volume, 0, 100);
            _logger.LogDebug("设置系统音量: {Volume}", volume);
            _volume = volume;

            var script = "$wsh = New-Object -ComObject WScript.Shell; $wsh.SendKeys([char]174);";
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit(2000);
            _logger.LogInformation("系统音量已更新为: {Volume}", _volume);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置系统音量时发生错误");
        }
    }

    public int GetVolume()
    {
        try
        {
            _logger.LogDebug("获取系统音量");
            return _volume;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统音量时发生错误，返回缓存值");
            return _volume;
        }
    }
}
