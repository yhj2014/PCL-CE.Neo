using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxAudioService : IAudioService
{
    private bool _isPlaying;
    private int _volume = 100;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public void Play(string filePath)
    {
        try
        {
            _isPlaying = true;
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "paplay",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            process.WaitForExit();
            _isPlaying = false;
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            _isPlaying = false;
        }
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Resume()
    {
        _isPlaying = true;
    }

    public void SetVolume(int volume)
    {
        _volume = Math.Clamp(volume, 0, 100);
    }

    public int GetVolume()
    {
        return _volume;
    }
}
