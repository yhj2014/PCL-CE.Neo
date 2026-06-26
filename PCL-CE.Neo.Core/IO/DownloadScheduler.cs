using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.IO;

/// <summary>
/// Download task segment for multi-part downloading.
/// </summary>
public sealed class DownloadSegment
{
    public long StartOffset { get; init; }
    public long EndOffset { get; init; }
    public long BytesWritten { get; set; }
    public bool IsCompleted { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Download task for scheduled downloading.
/// </summary>
public sealed class DownloadTaskItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Url { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public long TotalBytes { get; set; }
    public long BytesDownloaded { get; set; }
    public DownloadTaskState State { get; set; } = DownloadTaskState.Pending;
    public int Priority { get; set; }
    public string? ExpectedHash { get; init; }
    public string? SourceName { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Exception? Error { get; set; }
    public DownloadSegment[]? Segments { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }
}

/// <summary>
/// Download task state enumeration.
/// </summary>
public enum DownloadTaskState
{
    Pending,
    Preparing,
    Downloading,
    Paused,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Download scheduler for managing multiple download tasks.
/// Supports parallel downloading, speed limiting, and retry logic.
/// </summary>
public sealed class DownloadScheduler
{
    private readonly ILogger<DownloadScheduler> _logger;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, DownloadTaskItem> _tasks = new();
    private readonly ConcurrentQueue<DownloadTaskItem> _pendingQueue = new();
    private readonly SemaphoreSlim _concurrentLimit;
    private readonly SemaphoreSlim _queueLock = new(1, 1);
    private readonly int _maxConcurrent;
    private readonly int _maxRetries;
    private int _speedLimitBytesPerSecond;
    private bool _isRunning;
    private Task? _schedulerTask;
    private CancellationTokenSource? _schedulerCts;

    public event Action<DownloadTaskItem>? TaskAdded;
    public event Action<DownloadTaskItem>? TaskStarted;
    public event Action<DownloadTaskItem>? TaskProgressChanged;
    public event Action<DownloadTaskItem>? TaskCompleted;
    public event Action<DownloadTaskItem>? TaskFailed;

    public IReadOnlyDictionary<string, DownloadTaskItem> Tasks => _tasks;
    public int ActiveCount => _tasks.Values.Count(t => t.State == DownloadTaskState.Downloading);
    public int PendingCount => _pendingQueue.Count;
    public long TotalBytesDownloaded => _tasks.Values.Sum(t => t.BytesDownloaded);

    public DownloadScheduler() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<DownloadScheduler>.Instance,
        maxConcurrent: 4,
        maxRetries: 3)
    {
    }

    public DownloadScheduler(
        ILogger<DownloadScheduler> logger,
        int maxConcurrent = 4,
        int maxRetries = 3)
    {
        _logger = logger;
        _maxConcurrent = Math.Max(1, Math.Min(maxConcurrent, 16));
        _maxRetries = Math.Max(0, Math.Min(maxRetries, 10));
        _concurrentLimit = new SemaphoreSlim(_maxConcurrent, _maxConcurrent);
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxConnectionsPerServer = _maxConcurrent
        });
        _httpClient.Timeout = TimeSpan.FromMinutes(30);

        _logger.LogInformation("DownloadScheduler initialized with max concurrent: {MaxConcurrent}, max retries: {MaxRetries}",
            _maxConcurrent, _maxRetries);
    }

    /// <summary>
    /// Add a new download task to the scheduler.
    /// </summary>
    public async Task<DownloadTaskItem> AddTaskAsync(
        string url,
        string destinationPath,
        int priority = 0,
        string? expectedHash = null,
        string? sourceName = null)
    {
        var task = new DownloadTaskItem
        {
            Url = url,
            DestinationPath = destinationPath,
            Priority = priority,
            ExpectedHash = expectedHash,
            SourceName = sourceName,
            CancellationTokenSource = new CancellationTokenSource()
        };

        _tasks[task.Id] = task;
        _pendingQueue.Enqueue(task);

        TaskAdded?.Invoke(task);
        _logger.LogInformation("Added download task: {Url} -> {Path}", url, destinationPath);

        if (!_isRunning)
        {
            StartScheduler();
        }

        return task;
    }

    /// <summary>
    /// Start the scheduler background processing.
    /// </summary>
    public void StartScheduler()
    {
        if (_isRunning) return;

        _isRunning = true;
        _schedulerCts = new CancellationTokenSource();
        _schedulerTask = Task.Run(() => ProcessQueueAsync(_schedulerCts.Token));

        _logger.LogInformation("Download scheduler started");
    }

    /// <summary>
    /// Stop the scheduler and cancel all pending tasks.
    /// </summary>
    public async Task StopSchedulerAsync()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _schedulerCts?.Cancel();

        if (_schedulerTask != null)
        {
            await _schedulerTask;
        }

        _logger.LogInformation("Download scheduler stopped");
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _queueLock.WaitAsync(cancellationToken);

                DownloadTaskItem? task = null;
                while (_pendingQueue.TryDequeue(out var dequeuedTask))
                {
                    if (dequeuedTask.State == DownloadTaskState.Pending)
                    {
                        task = dequeuedTask;
                        break;
                    }
                }

                _queueLock.Release();

                if (task == null)
                {
                    await Task.Delay(100, cancellationToken);
                    continue;
                }

                await _concurrentLimit.WaitAsync(cancellationToken);

                _ = Task.Run(() => ExecuteTaskAsync(task, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing download queue");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private async Task ExecuteTaskAsync(DownloadTaskItem task, CancellationToken globalCts)
    {
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            globalCts,
            task.CancellationTokenSource?.Token ?? CancellationToken.None);

        task.State = DownloadTaskState.Downloading;
        task.StartedAt = DateTime.Now;
        TaskStarted?.Invoke(task);

        var retries = 0;
        var success = false;

        while (!success && retries <= _maxRetries && !linkedCts.IsCancellationRequested)
        {
            try
            {
                if (retries > 0)
                {
                    _logger.LogInformation("Retrying download task {Id} (attempt {Retry})", task.Id, retries);
                    await Task.Delay(1000 * retries, linkedCts.Token);
                }

                success = await DownloadFileAsync(task, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                task.State = DownloadTaskState.Cancelled;
                _logger.LogInformation("Download task cancelled: {Id}", task.Id);
                break;
            }
            catch (Exception ex)
            {
                task.Error = ex;
                retries++;
                _logger.LogWarning(ex, "Download task {Id} failed, attempt {Retry}", task.Id, retries);
            }
        }

        _concurrentLimit.Release();

        if (success)
        {
            task.State = DownloadTaskState.Completed;
            task.CompletedAt = DateTime.Now;
            TaskCompleted?.Invoke(task);
            _logger.LogInformation("Download task completed: {Id}", task.Id);
        }
        else if (task.State != DownloadTaskState.Cancelled)
        {
            task.State = DownloadTaskState.Failed;
            TaskFailed?.Invoke(task);
            _logger.LogError("Download task failed after {Retries} retries: {Id}", retries, task.Id);
        }
    }

    private async Task<bool> DownloadFileAsync(DownloadTaskItem task, CancellationToken cancellationToken)
    {
        try
        {
            var directory = Path.GetDirectoryName(task.DestinationPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var response = await _httpClient.GetAsync(
                task.Url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            task.TotalBytes = response.Content.Headers.ContentLength ?? -1;

            var canReportProgress = task.TotalBytes > 0;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(
                task.DestinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            var buffer = new byte[81920];
            long totalRead = 0;
            var lastReportedBytes = 0L;
            var lastReportedTime = DateTime.Now;

            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;
                task.BytesDownloaded = totalRead;

                if (canReportProgress)
                {
                    var now = DateTime.Now;
                    var elapsed = (now - lastReportedTime).TotalSeconds;

                    if (elapsed >= 0.5)
                    {
                        var currentSpeed = (totalRead - lastReportedBytes) / elapsed;

                        if (_speedLimitBytesPerSecond > 0 && currentSpeed > _speedLimitBytesPerSecond)
                        {
                            var delayMs = (int)((bytesRead / (double)_speedLimitBytesPerSecond) * 1000 - elapsed * 1000);
                            if (delayMs > 0)
                            {
                                await Task.Delay(Math.Min(delayMs, 100), cancellationToken);
                            }
                        }

                        TaskProgressChanged?.Invoke(task);

                        lastReportedBytes = totalRead;
                        lastReportedTime = now;
                    }
                }
            }

            if (!string.IsNullOrEmpty(task.ExpectedHash))
            {
                var actualHash = await ComputeFileHashAsync(task.DestinationPath, cancellationToken);
                if (!actualHash.Equals(task.ExpectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Hash verification failed for {Path}: expected {Expected}, got {Actual}",
                        task.DestinationPath, task.ExpectedHash, actualHash);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download failed for {Url}", task.Url);

            if (File.Exists(task.DestinationPath))
            {
                try
                {
                    File.Delete(task.DestinationPath);
                }
                catch { }
            }

            return false;
        }
    }

    private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Set download speed limit.
    /// </summary>
    public void SetSpeedLimit(int bytesPerSecond)
    {
        _speedLimitBytesPerSecond = Math.Max(0, bytesPerSecond);
        _logger.LogInformation("Speed limit set to {Speed} bytes/s", bytesPerSecond);
    }

    /// <summary>
    /// Pause a specific download task.
    /// </summary>
    public void PauseTask(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var task) && task.State == DownloadTaskState.Downloading)
        {
            task.State = DownloadTaskState.Paused;
            task.CancellationTokenSource?.Cancel();
            _logger.LogInformation("Paused download task: {Id}", taskId);
        }
    }

    /// <summary>
    /// Resume a paused download task.
    /// </summary>
    public void ResumeTask(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var task) && task.State == DownloadTaskState.Paused)
        {
            task.CancellationTokenSource = new CancellationTokenSource();
            task.State = DownloadTaskState.Pending;
            _pendingQueue.Enqueue(task);

            _logger.LogInformation("Resumed download task: {Id}", taskId);

            if (!_isRunning)
            {
                StartScheduler();
            }
        }
    }

    /// <summary>
    /// Cancel a download task.
    /// </summary>
    public void CancelTask(string taskId)
    {
        if (_tasks.TryGetValue(taskId, out var task))
        {
            task.CancellationTokenSource?.Cancel();
            task.State = DownloadTaskState.Cancelled;

            if (File.Exists(task.DestinationPath))
            {
                try
                {
                    File.Delete(task.DestinationPath);
                }
                catch { }
            }

            _logger.LogInformation("Cancelled download task: {Id}", taskId);
        }
    }

    /// <summary>
    /// Get download statistics.
    /// </summary>
    public DownloadStatistics GetStatistics()
    {
        var completed = _tasks.Values.Count(t => t.State == DownloadTaskState.Completed);
        var failed = _tasks.Values.Count(t => t.State == DownloadTaskState.Failed);
        var downloading = _tasks.Values.Count(t => t.State == DownloadTaskState.Downloading);
        var pending = _pendingQueue.Count;
        var totalBytes = _tasks.Values.Sum(t => t.BytesDownloaded);

        return new DownloadStatistics
        {
            TotalTasks = _tasks.Count,
            CompletedTasks = completed,
            FailedTasks = failed,
            DownloadingTasks = downloading,
            PendingTasks = pending,
            TotalBytesDownloaded = totalBytes
        };
    }

    /// <summary>
    /// Clear completed and failed tasks from history.
    /// </summary>
    public void ClearFinishedTasks()
    {
        var toRemove = _tasks.Values
            .Where(t => t.State == DownloadTaskState.Completed ||
                        t.State == DownloadTaskState.Failed ||
                        t.State == DownloadTaskState.Cancelled)
            .Select(t => t.Id)
            .ToArray();

        foreach (var id in toRemove)
        {
            _tasks.TryRemove(id, out _);
        }

        _logger.LogInformation("Cleared {Count} finished tasks", toRemove.Length);
    }
}

/// <summary>
/// Download statistics snapshot.
/// </summary>
public sealed class DownloadStatistics
{
    public int TotalTasks { get; init; }
    public int CompletedTasks { get; init; }
    public int FailedTasks { get; init; }
    public int DownloadingTasks { get; init; }
    public int PendingTasks { get; init; }
    public long TotalBytesDownloaded { get; init; }
}