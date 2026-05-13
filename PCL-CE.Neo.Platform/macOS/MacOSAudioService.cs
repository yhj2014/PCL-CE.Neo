using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSAudioService : IAudioService
{
    private bool _isPlaying;
    private int _volume = 100;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public void Play(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;
        _isPlaying = true;
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "afplay",
                    Arguments = filePath,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
        }
        catch
        {
        }
        _isPlaying = false;
        PlaybackFinished?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void Pause()
    {
    }

    public void Resume()
    {
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
