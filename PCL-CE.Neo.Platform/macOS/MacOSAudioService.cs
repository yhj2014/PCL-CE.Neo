namespace PCL_CE.Neo.Platform.macOS;

public class MacOSAudioService : Core.Abstractions.IAudioService
{
#if MACCATALYST
    private bool _isPlaying;
    private int _volume = 100;

    public bool IsPlaying => _isPlaying;

    public event EventHandler? PlaybackFinished;

    public void Play(string filePath)
    {
        _isPlaying = true;
    }

    public void Stop()
    {
        _isPlaying = false;
        PlaybackFinished?.Invoke(this, EventArgs.Empty);
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
#else
    public bool IsPlaying => throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");

    public event EventHandler? PlaybackFinished;

    public void Play(string filePath)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void Stop()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void Pause()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void Resume()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void SetVolume(int volume)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public int GetVolume()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }
#endif
}
