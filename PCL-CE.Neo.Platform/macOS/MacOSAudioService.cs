using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSAudioService : IAudioService
{
    private readonly ILogger<MacOSAudioService> _logger;
    private int _volume = 50;
    private bool _isPlaying;
    private bool _isPaused;
    private string? _currentFilePath;
    private Process? _playProcess;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public MacOSAudioService(ILogger<MacOSAudioService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing macOS audio service");
            _currentFilePath = string.Empty;
            _logger.LogInformation("macOS audio service initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing macOS audio service");
        }
    }

    public void Play(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("Play called with empty file path, ignored");
                return;
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Audio file does not exist: {FilePath}", filePath);
                return;
            }

            _logger.LogDebug("Playing audio file: {FilePath}", filePath);
            _currentFilePath = filePath;

            Task.Run(() =>
            {
                try
                {
                    _playProcess?.Dispose();

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = "\"" + filePath.Replace("\"", "\\\"") + "\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    };

                    _playProcess = Process.Start(startInfo);
                    if (_playProcess != null)
                    {
                        _isPlaying = true;
                        _isPaused = false;
                        _playProcess.WaitForExit();
                        _isPlaying = false;
                        _logger.LogInformation("Audio playback finished: {FilePath}", filePath);
                        PlaybackFinished?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        _isPlaying = false;
                        _logger.LogWarning("Failed to start afplay process");
                    }
                }
                catch (Exception ex)
                {
                    _isPlaying = false;
                    _logger.LogError(ex, "Error playing audio file: {FilePath}", filePath);
                }
            });
        }
        catch (Exception ex)
        {
            _isPlaying = false;
            _logger.LogError(ex, "Error during play operation");
        }
    }

    public void Stop()
    {
        try
        {
            _logger.LogDebug("Stopping audio playback");
            if (_playProcess != null && !_playProcess.HasExited)
            {
                _playProcess.Kill();
                _playProcess.WaitForExit(2000);
            }
            _playProcess?.Dispose();
            _playProcess = null;
            _isPlaying = false;
            _isPaused = false;
            _logger.LogInformation("Audio playback stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping audio playback");
        }
    }

    public void Pause()
    {
        try
        {
            _logger.LogDebug("Pausing audio playback");
            if (_isPlaying && _playProcess != null && !_playProcess.HasExited)
            {
                try
                {
                    _playProcess.Kill();
                    _playProcess.WaitForExit(2000);
                    _playProcess.Dispose();
                    _playProcess = null;
                }
                catch { }
                _isPaused = true;
                _isPlaying = false;
                _logger.LogInformation("Audio playback paused");
            }
            else
            {
                _logger.LogInformation("No active audio to pause");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing audio playback");
        }
    }

    public void Resume()
    {
        try
        {
            _logger.LogDebug("Resuming audio playback");
            if (_isPaused && !string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
            {
                Play(_currentFilePath);
                _isPaused = false;
                _logger.LogInformation("Audio playback resumed: {FilePath}", _currentFilePath);
            }
            else
            {
                _logger.LogInformation("No audio to resume");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming audio playback");
        }
    }

    public void SetVolume(int volume)
    {
        try
        {
            if (volume < 0 || volume > 100)
            {
                _logger.LogWarning("Volume value out of range [0,100]: {Volume}", volume);
                volume = Math.Clamp(volume, 0, 100);
            }
            _logger.LogDebug("Setting system volume: {Volume}", volume);
            _volume = volume;

            var scaledVolume = (int)Math.Round((double)volume * 7 / 100);
            var script = $"set volume output volume {volume}";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit(2000);
            _logger.LogInformation("System volume updated to: {Volume}", _volume);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting system volume");
        }
    }

    public int GetVolume()
    {
        try
        {
            _logger.LogDebug("Getting system volume");
            var script = "output volume of (get volume settings)";
            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);
                if (int.TryParse(output?.Trim(), out var parsedVolume))
                {
                    _volume = Math.Clamp(parsedVolume, 0, 100);
                    _logger.LogDebug("Got system volume via osascript: {Volume}", _volume);
                }
                else
                {
                    _logger.LogWarning("Failed to parse system volume, using cached value: {Volume}", _volume);
                }
            }

            _logger.LogInformation("Current volume: {Volume}", _volume);
            return _volume;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system volume, returning cached value");
            return _volume;
        }
    }
}
