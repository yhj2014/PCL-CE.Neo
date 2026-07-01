using System.IO;

namespace PCL_CE.Neo.Core.Utils.FileSystem;

public class LockFile : IDisposable
{
    private readonly string _lockPath;
    private FileStream? _lockStream;
    private bool _disposed;

    public LockFile(string lockPath)
    {
        _lockPath = lockPath;
    }

    public bool Acquire(int timeoutMs = 5000)
    {
        ThrowIfDisposed();

        try
        {
            FileUtils.EnsureParentDirectoryExists(_lockPath);

            using var cts = new CancellationTokenSource(timeoutMs);
            DateTime startTime = DateTime.Now;

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    _lockStream = new FileStream(_lockPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    return true;
                }
                catch (IOException)
                {
                    if (DateTime.Now - startTime >= TimeSpan.FromMilliseconds(timeoutMs))
                        return false;
                    Thread.Sleep(50);
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool IsHeld => _lockStream != null;

    public void Release()
    {
        if (_lockStream != null)
        {
            _lockStream.Dispose();
            _lockStream = null;

            try
            {
                File.Delete(_lockPath);
            }
            catch
            {
            }
        }
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
            Release();
        }

        _disposed = true;
    }

    ~LockFile()
    {
        Dispose(false);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LockFile));
    }
}