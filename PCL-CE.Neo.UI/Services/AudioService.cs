namespace PCL_CE.Neo.UI.Services;

public class AudioService : Core.Abstractions.IAudioService
{
    private bool _isPlaying = false;
    private int _volume = 100;
    private CancellationTokenSource? _playbackCts;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public void Play(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        Stop();

        _isPlaying = true;
        _playbackCts = new CancellationTokenSource();

#if WINDOWS
        PlayWindowsAudio(filePath, _playbackCts.Token);
#elif MACCATALYST
        PlayMacOSAudio(filePath, _playbackCts.Token);
#elif LINUX
        PlayLinuxAudio(filePath, _playbackCts.Token);
#endif
    }

#if WINDOWS
    private void PlayWindowsAudio(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var player = new System.Media.SoundPlayer(filePath);
                player.PlaySync();
            }
            else
            {
                PlayUsingNAudio(filePath, cancellationToken);
            }
        }
        catch
        {
            PlayUsingNAudio(filePath, cancellationToken);
        }
        finally
        {
            _isPlaying = false;
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    private void PlayUsingNAudio(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(filePath))
                return;

            using var reader = new System.IO.FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0 && !cancellationToken.IsCancellationRequested)
            {
            }

            System.Threading.Thread.Sleep(100);
        }
        catch
        {
        }
    }
#endif

#if MACCATALYST
    private void PlayMacOSAudio(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var escapedPath = filePath.Replace("\"", "\\\"");
            var script = $"-e 'do shell script \"afplay \\\"{escapedPath}\\\"\"'";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = script,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            process.WaitForExit();
        }
        catch
        {
        }
        finally
        {
            _isPlaying = false;
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
    }
#endif

#if LINUX
    private void PlayLinuxAudio(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var player = FindLinuxAudioPlayer();
            if (string.IsNullOrEmpty(player))
                player = "aplay";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = player,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            process.WaitForExit();
        }
        catch
        {
        }
        finally
        {
            _isPlaying = false;
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    private string? FindLinuxAudioPlayer()
    {
        var players = new[] { "paplay", "aplay", "ffplay", "mpg123" };
        foreach (var player in players)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = player,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                var output = process.StandardOutput.ReadLine();
                process.WaitForExit(1000);

                if (!string.IsNullOrEmpty(output) && File.Exists(output))
                    return output;
            }
            catch
            {
            }
        }
        return null;
    }
#endif

    public void Stop()
    {
        _isPlaying = false;
        _playbackCts?.Cancel();
        _playbackCts?.Dispose();
        _playbackCts = null;
    }

    public void Pause()
    {
#if WINDOWS
        _isPlaying = false;
#elif MACCATALYST
        _isPlaying = false;
#elif LINUX
        _isPlaying = false;
#endif
    }

    public void Resume()
    {
#if WINDOWS
        _isPlaying = true;
#elif MACCATALYST
        _isPlaying = true;
#elif LINUX
        _isPlaying = true;
#endif
    }

    public void SetVolume(int volume)
    {
        _volume = Math.Clamp(volume, 0, 100);

#if WINDOWS
        SetWindowsVolume(_volume);
#elif MACCATALYST
        SetMacOSVolume(_volume);
#elif LINUX
        SetLinuxVolume(_volume);
#endif
    }

#if WINDOWS
    private void SetWindowsVolume(int volume)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"(New-Object -ComObject WScript.Shell).SendKeys([char]175 * ({volume} / 5))\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            process.WaitForExit(1000);
        }
        catch
        {
        }
    }
#endif

#if MACCATALYST
    private void SetMacOSVolume(int volume)
    {
        try
        {
            var script = $"set volume output volume {volume}";
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e '{script}'",
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit(1000);
        }
        catch
        {
        }
    }
#endif

#if LINUX
    private void SetLinuxVolume(int volume)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "amixer",
                    Arguments = $"set Master {volume}%",
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit(1000);
        }
        catch
        {
        }
    }
#endif

    public int GetVolume()
    {
#if WINDOWS
        return GetWindowsVolume();
#elif MACCATALYST
        return GetMacOSVolume();
#elif LINUX
        return GetLinuxVolume();
#else
        return _volume;
#endif
    }

#if WINDOWS
    private int GetWindowsVolume()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Audio { [DllImport(\\\"user32.dll\\\")] public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo); }'; [Audio]::keybd_event(175, 0, 0, UIntPtr.Zero); [Audio]::keybd_event(175, 0, 2, UIntPtr.Zero)\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            process.WaitForExit(1000);
        }
        catch
        {
        }
        return _volume;
    }
#endif

#if MACCATALYST
    private int GetMacOSVolume()
    {
        try
        {
            var script = "output volume of (get volume settings)";
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e '{script}'",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadLine();
            process.WaitForExit(1000);

            if (!string.IsNullOrEmpty(output) && int.TryParse(output, out var volume))
                return volume;
        }
        catch
        {
        }
        return _volume;
    }
#endif

#if LINUX
    private int GetLinuxVolume()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "amixer",
                    Arguments = "get Master",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(1000);

            var match = System.Text.RegularExpressions.Regex.Match(output, @"(\d+)%");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var volume))
                return volume;
        }
        catch
        {
        }
        return _volume;
    }
#endif
}
