namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class AudioServiceMock : IAudioService
{
    public event EventHandler? PlaybackFinished;
    
    public bool IsPlaying { get; private set; }
    public int CurrentVolume { get; set; } = 100;
    public string? LastPlayedFile { get; private set; }
    
    public void Play(string filePath)
    {
        LastPlayedFile = filePath;
        IsPlaying = true;
    }

    public void Stop()
    {
        IsPlaying = false;
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void Resume()
    {
        IsPlaying = true;
    }

    public void SetVolume(int volume)
    {
        CurrentVolume = Math.Clamp(volume, 0, 100);
    }

    public int GetVolume()
    {
        return CurrentVolume;
    }

    public void SimulatePlaybackFinished()
    {
        IsPlaying = false;
        PlaybackFinished?.Invoke(this, EventArgs.Empty);
    }
}
