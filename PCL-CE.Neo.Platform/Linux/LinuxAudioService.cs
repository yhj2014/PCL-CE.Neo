using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxAudioService : IAudioService
{
    private readonly ILogger<LinuxAudioService> _logger;
    private int _volume = 50;
    private bool _isPlaying;
    private CancellationTokenSource? _playbackCts;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public LinuxAudioService(ILogger<LinuxAudioService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("LinuxAudioService initializing");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during LinuxAudioService initialization");
        }
    }

    public void Play(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                _logger.LogWarning("Play called with invalid file path: {Path}", filePath);
                return;
            }

            Stop();
            _isPlaying = true;
            _playbackCts = new CancellationTokenSource();

            var token = _playbackCts.Token;
            _logger.LogInformation("Starting audio playback: {Path}", filePath);

            Task.Run(async () =>
            {
                try
                {
                    var player = FindPlayer();
                    _logger.LogDebug("Using player: {Player}", player);

                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = player,
                        Arguments = $"\"{filePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    });

                    if (process != null)
                    {
                        await Task.Delay(50, token);
                        var waitTask = process.WaitForExitAsync(token);
                        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(10), token);
                        await Task.WhenAny(waitTask, timeoutTask);

                        if (!process.HasExited)
                        {
                            try { process.Kill(); } catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Audio playback failed for: {Path}", filePath);
                }
                finally
                {
                    _isPlaying = false;
                    PlaybackFinished?.Invoke(this, EventArgs.Empty);
                    _logger.LogDebug("Audio playback finished");
                }
            }, token);
        }
        catch (Exception ex)
        {
            _isPlaying = false;
            _logger.LogError(ex, "Failed to play audio");
        }
    }

    private string FindPlayer()
    {
        var candidates = new[] { "paplay", "aplay", "ffplay", "mpg123" };
        foreach (var candidate in candidates)
        {
            try
            {
                using var check = Process.Start(new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = candidate,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });

                if (check != null)
                {
                    var output = check.StandardOutput.ReadToEnd().Trim();
                    check.WaitForExit(1000);
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        return candidate;
                    }
                }
            }
            catch { }
        }

        return "aplay";
    }

    public void Stop()
    {
        try
        {
            _playbackCts?.Cancel();
            _playbackCts?.Dispose();
            _playbackCts = null;
            _isPlaying = false;
            _logger.LogDebug("Audio playback stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop audio");
        }
    }

    public void Pause()
    {
        try
        {
            _isPlaying = false;
            _logger.LogDebug("Audio playback paused");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause audio");
        }
    }

    public void Resume()
    {
        try
        {
            _isPlaying = true;
            _logger.LogDebug("Audio playback resumed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume audio");
        }
    }

    public void SetVolume(int volume)
    {
        try
        {
            var clampedVolume = Math.Clamp(volume, 0, 100);
            _volume = clampedVolume;
            _logger.LogDebug("Setting volume to: {Volume}%", clampedVolume);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "amixer",
                Arguments = $"set Master {clampedVolume}%",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process != null)
            {
                process.WaitForExit(3000);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set system volume via amixer");
        }
    }

    public int GetVolume()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "amixer",
                Arguments = "get Master",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(3000);

                var match = System.Text.RegularExpressions.Regex.Match(output, @"(\d+)%");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var vol))
                {
                    _volume = vol;
                    return vol;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read system volume via amixer");
        }

        return _volume;
    }
}
