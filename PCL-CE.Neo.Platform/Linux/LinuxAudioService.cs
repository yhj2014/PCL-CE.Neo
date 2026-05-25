namespace PCL_CE.Neo.Platform.Linux;

public class LinuxAudioService : Core.Abstractions.IAudioService
{
    private int _volume = 50;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public void Play(string filePath)
    {
        try
        {
            _isPlaying = true;
            Task.Delay(100).ContinueWith(t =>
            {
                _isPlaying = false;
                PlaybackFinished?.Invoke(this, EventArgs.Empty);
            });
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
