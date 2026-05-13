using System.Media;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsAudioService : IAudioService, IDisposable
{
    private bool _isPlaying;
    private int _volume = 100;
    private SoundPlayer? _player;
    private readonly object _lock = new object();
    private bool _disposed;

    public bool IsPlaying
    {
        get
        {
            lock (_lock)
            {
                return _isPlaying;
            }
        }
    }

    public event EventHandler? PlaybackFinished;

    public void Play(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            System.Diagnostics.Debug.WriteLine($"Audio file not found: {filePath}");
            return;
        }

        lock (_lock)
        {
            try
            {
                Stop();

                _player = new SoundPlayer(filePath);
                _player.LoadCompleted += OnLoadCompleted;
                _player.SoundLocationChanged += OnSoundLocationChanged;
                _isPlaying = true;

                // 使用 PlaySync 同步播放，或者考虑使用更完整的音频库
                // 注意：SoundPlayer 功能有限，生产环境建议使用 NAudio 或其他音频库
                Task.Run(() =>
                {
                    try
                    {
                        _player.PlaySync();
                    }
                    finally
                    {
                        OnPlaybackFinished();
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play audio: {ex.Message}");
                _isPlaying = false;
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            try
            {
                if (_player != null)
                {
                    _player.Stop();
                    _player.LoadCompleted -= OnLoadCompleted;
                    _player.SoundLocationChanged -= OnSoundLocationChanged;
                    _player.Dispose();
                    _player = null;
                }
            }
            finally
            {
                _isPlaying = false;
            }
        }
    }

    public void Pause()
    {
        // SoundPlayer 不支持暂停，停止代替
        Stop();
    }

    public void Resume()
    {
        // 简单实现：无法真正恢复，重新开始
        lock (_lock)
        {
            if (!_isPlaying && _player != null)
            {
                // SoundPlayer 无法真正恢复，这里不做任何事
                // 生产环境建议使用更完整的音频库
            }
        }
    }

    public void SetVolume(int volume)
    {
        lock (_lock)
        {
            _volume = Math.Clamp(volume, 0, 100);
            // 注意：SoundPlayer 不直接支持音量控制
            // 生产环境建议使用 NAudio 或其他音频库来实现真正的音量控制
        }
    }

    public int GetVolume()
    {
        lock (_lock)
        {
            return _volume;
        }
    }

    private void OnLoadCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            System.Diagnostics.Debug.WriteLine($"Audio load failed: {e.Error.Message}");
        }
    }

    private void OnSoundLocationChanged(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Audio sound location changed");
    }

    private void OnPlaybackFinished()
    {
        lock (_lock)
        {
            _isPlaying = false;
        }
        PlaybackFinished?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Stop();
        }

        _disposed = true;
    }

    ~WindowsAudioService()
    {
        Dispose(false);
    }
}
