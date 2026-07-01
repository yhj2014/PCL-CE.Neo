using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using PCL_CE.Neo.Core.Utils.FileSystem;

namespace PCL_CE.Neo.Core.Utils.Download;

public class DownloadManager
{
    private readonly ConcurrentDictionary<string, DownloadTask> _tasks = new ConcurrentDictionary<string, DownloadTask>();
    private readonly HttpClient _httpClient;
    private readonly int _maxConcurrentDownloads;
    private readonly SemaphoreSlim _semaphore;

    public event EventHandler<DownloadProgressEventArgs>? ProgressChanged;
    public event EventHandler<DownloadProgressEventArgs>? TaskCompleted;
    public event EventHandler<DownloadProgressEventArgs>? TaskFailed;

    public DownloadManager(int maxConcurrentDownloads = 5)
    {
        _maxConcurrentDownloads = maxConcurrentDownloads;
        _semaphore = new SemaphoreSlim(maxConcurrentDownloads);

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10,
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(30)
        };
    }

    public DownloadTask CreateTask(string url, string savePath, int maxRetryCount = 3)
    {
        var task = new DownloadTask
        {
            Url = url,
            SavePath = savePath,
            MaxRetryCount = maxRetryCount,
            CancellationTokenSource = new CancellationTokenSource()
        };
        _tasks[task.TaskId] = task;
        return task;
    }

    public async Task<DownloadTask> StartDownloadAsync(string url, string savePath, int maxRetryCount = 3, CancellationToken cancellationToken = default)
    {
        var task = CreateTask(url, savePath, maxRetryCount);
        await StartDownloadAsync(task, cancellationToken);
        return task;
    }

    public async Task StartDownloadAsync(DownloadTask task, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await ExecuteDownloadAsync(task, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ExecuteDownloadAsync(DownloadTask task, CancellationToken cancellationToken)
    {
        task.Status = DownloadStatus.Downloading;
        task.StartTime = DateTime.Now;

        while (task.RetryCount <= task.MaxRetryCount && task.Status != DownloadStatus.Cancelled)
        {
            try
            {
                await DownloadFileAsync(task, cancellationToken);
                task.Status = DownloadStatus.Completed;
                task.EndTime = DateTime.Now;
                OnTaskCompleted(task);
                return;
            }
            catch (OperationCanceledException)
            {
                task.Status = DownloadStatus.Cancelled;
                task.EndTime = DateTime.Now;
                OnTaskFailed(task, "下载已取消");
                return;
            }
            catch (Exception ex)
            {
                task.RetryCount++;
                task.ErrorMessage = ex.Message;

                if (task.RetryCount <= task.MaxRetryCount)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, task.RetryCount)), cancellationToken);
                }
                else
                {
                    task.Status = DownloadStatus.Failed;
                    task.EndTime = DateTime.Now;
                    OnTaskFailed(task, ex.Message);
                    return;
                }
            }
        }
    }

    private async Task DownloadFileAsync(DownloadTask task, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, task.Url);
        request.Headers.Add("User-Agent", "PCL-CE-NEO/1.0");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        task.TotalSize = response.Content.Headers.ContentLength ?? 0;

        FileUtils.EnsureParentDirectoryExists(task.SavePath);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(task.SavePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        int bytesRead;
        long totalRead = 0;
        DateTime lastProgressUpdate = DateTime.Now;
        long bytesReadSinceLastUpdate = 0;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalRead += bytesRead;
            bytesReadSinceLastUpdate += bytesRead;
            task.DownloadedSize = totalRead;

            DateTime now = DateTime.Now;
            if (now - lastProgressUpdate >= TimeSpan.FromSeconds(0.5))
            {
                task.SpeedBytesPerSecond = (long)(bytesReadSinceLastUpdate / (now - lastProgressUpdate).TotalSeconds);
                OnProgressChanged(task);
                lastProgressUpdate = now;
                bytesReadSinceLastUpdate = 0;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        task.SpeedBytesPerSecond = 0;
        OnProgressChanged(task);
    }

    public void CancelTask(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out DownloadTask? task))
        {
            task.Cancel();
        }
    }

    public void CancelAll()
    {
        foreach (var task in _tasks.Values)
        {
            task.Cancel();
        }
    }

    public DownloadTask? GetTask(string taskId)
    {
        _tasks.TryGetValue(taskId, out DownloadTask? task);
        return task;
    }

    public IEnumerable<DownloadTask> GetAllTasks()
    {
        return _tasks.Values.ToArray();
    }

    public void RemoveTask(string taskId)
    {
        if (_tasks.TryRemove(taskId, out DownloadTask? task))
        {
            task.CancellationTokenSource?.Dispose();
        }
    }

    public void CleanupCompletedTasks()
    {
        foreach (var task in _tasks.Values.Where(t => t.Status is DownloadStatus.Completed or DownloadStatus.Failed or DownloadStatus.Cancelled).ToArray())
        {
            RemoveTask(task.TaskId);
        }
    }

    private void OnProgressChanged(DownloadTask task)
    {
        var args = new DownloadProgressEventArgs(task.TaskId, task.Status, task.DownloadedSize, task.TotalSize, task.SpeedBytesPerSecond);
        ProgressChanged?.Invoke(this, args);
    }

    private void OnTaskCompleted(DownloadTask task)
    {
        var args = new DownloadProgressEventArgs(task.TaskId, task.Status, task.DownloadedSize, task.TotalSize, 0);
        TaskCompleted?.Invoke(this, args);
    }

    private void OnTaskFailed(DownloadTask task, string errorMessage)
    {
        var args = new DownloadProgressEventArgs(task.TaskId, task.Status, task.DownloadedSize, task.TotalSize, 0, errorMessage);
        TaskFailed?.Invoke(this, args);
    }

    public void Dispose()
    {
        CancelAll();
        _httpClient.Dispose();
        _semaphore.Dispose();
    }
}