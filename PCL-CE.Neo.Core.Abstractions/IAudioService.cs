namespace PCL.CE.Neo.Core.Abstractions;

public interface IAudioService
{
    void Play(string filePath);
    void Stop();
    void Pause();
    void Resume();

    void SetVolume(int volume);
    int GetVolume();

    bool IsPlaying { get; }

    event EventHandler? PlaybackFinished;
}
