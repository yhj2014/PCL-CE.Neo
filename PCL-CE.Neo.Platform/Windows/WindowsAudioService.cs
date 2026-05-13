namespace PCL_CE.Neo.Platform.Windows;

public class WindowsAudioService : IAudioService
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
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(filePath);
            player.Play();
            // Note: SoundPlayer doesn't have playback finished event for async playback
            // For full implementation, we'd need a more sophisticated approach
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
        // SoundPlayer doesn't support pause, stop instead
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
