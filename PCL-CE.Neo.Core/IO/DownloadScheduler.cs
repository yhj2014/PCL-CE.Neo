using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.IO;

/// <summary>
/// 下载连接信息
/// </summary>
public record DlConnectionInfo(
    long Length,
    long BeginOffset,
    long EndOffset,
    bool IsSupportSegment
);

/// <summary>
/// 下载连接接口，负责与服务器进行通信
/// </summary>
public interface IDlConnection
{
    /// <summary>
    /// 开始连接，发起与服务器的通信
    /// </summary>
    /// <param name="beginOffset">起始偏移，为 0 表示不使用分块</param>
    /// <returns>连接信息</returns>
    Task<DlConnectionInfo> StartAsync(long beginOffset);

    /// <summary>
    /// 停止连接，同时停止服务器通信并释放资源
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// 读取指定长度的数据，若无法继续读取则返回空数组
    /// </summary>
    /// <param name="length">读取长度</param>
    /// <returns>字节数组形式的数据</returns>
    Task<byte[]> ReadAsync(int length);
}

/// <summary>
/// 下载写入器接口
/// </summary>
public interface IDlWriter
{
    /// <summary>
    /// 是否支持并行写入
    /// </summary>
    bool IsSupportParallel { get; }

    /// <summary>
    /// 创建写入流
    /// </summary>
    Task<Stream> CreateStreamAsync();

    /// <summary>
    /// 停止写入并释放资源
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// 完成写入，用于执行某些并行操作的收尾工作
    /// </summary>
    Task FinishAsync();
}

/// <summary>
/// 下载任务状态
/// </summary>
public enum DlTaskState
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Canceled
}

/// <summary>
/// 下载任务信息
/// </summary>
public class DlTaskInfo
{
    public string Id { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public long TotalBytes { get; set; } = -1;
    public long DownloadedBytes { get; set; } = 0;
    public double Progress { get; set; } = 0.0;
    public DlTaskState State { get; set; } = DlTaskState.Pending;
    public string? ErrorMessage { get; set; }
    public Exception? Error { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? CompleteTime { get; set; }
    public int SegmentCount { get; set; } = 1;
    public int ActiveSegments { get; set; } = 0;
}

/// <summary>
/// 下载任务分片
/// </summary>
public class DlTaskSegment
{
    public int Index { get; init; }
    public long BeginOffset { get; init; }
    public long EndOffset { get; init; }
    public long CurrentOffset { get; set; }
    public bool IsCompleted { get; set; } = false;
    public IDlConnection? Connection { get; set; }
    public Stream? WriteStream { get; set; }
    
    public long Length => EndOffset - BeginOffset;
    public long Downloaded => CurrentOffset - BeginOffset;
    public double Progress => Length > 0 ? (double)Downloaded / Length : 0;
}

/// <summary>
/// 下载调度器配置
/// </summary>
public class DlSchedulerConfig
{
    /// <summary>
    /// 最大并发下载任务数
    /// </summary>
    public int MaxConcurrentTasks { get; set; } = 4;
    
    /// <summary>
    /// 每个任务的最大分片数
    /// </summary>
    public int MaxSegmentsPerTask { get; set; } = 8;
    
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// 分片大小阈值（字节），超过此大小才会分片
    /// </summary>
    public long SegmentThreshold { get; set; } = 1024 * 1024 * 4; // 4MB
    
    /// <summary>
    /// 缓冲区大小
    /// </summary>
    public int BufferSize { get; set; } = 8192;
    
    /// <summary>
    /// 连接超时时间
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// 读取超时时间
    /// </summary>
    public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(60);
    
    /// <summary>
    /// 重试延迟基数（秒）
    /// </summary>
    public double RetryDelayBase { get; set; } = 2.0;
}

/// <summary>
/// 下载调度器
/// </summary>
public sealed class DlScheduler : IDisposable
{
    private readonly ILogger<DlScheduler> _logger;
    private readonly HttpClient _httpClient;
    private readonly DlSchedulerConfig _config;
    private readonly ConcurrentDictionary<string, DlTaskInfo> _tasks = new();
    private readonly ConcurrentQueue<string> _pendingQueue = new();
    private readonly SemaphoreSlim _taskSemaphore;
    private readonly SemaphoreSlim _queueLock = new(1, 1);
    private readonly CancellationTokenSource _globalCts = new();
    private readonly List<Task> _workerTasks = new();
    private readonly Timer _progressTimer;
    private bool _disposed;

    public event Action<DlTaskInfo>? TaskAdded;
    public event Action<DlTaskInfo>? TaskStarted;
    public event Action<DlTaskInfo>? TaskProgressChanged;
    public event Action<DlTaskInfo>? TaskCompleted;
    public event Action<DlTaskInfo>? TaskFailed;
    public event Action<DlTaskInfo>? TaskCanceled;

    public DlScheduler(ILogger<DlScheduler> logger, DlSchedulerConfig? config = null)
    {
        _logger = logger;
        _config = config ?? new DlSchedulerConfig();
        _taskSemaphore = new SemaphoreSlim(_config.MaxConcurrentTasks);
        
        _httpClient = new HttpClient
        {
            Timeout = _config.ConnectionTimeout
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PCL-CE.Neo/2.0");
        
        // 启动工作线程
        for (var i = 0; i < _config.MaxConcurrentTasks; i++)
        {
            _workerTasks.Add(Task.Run(() => WorkerLoop(_globalCts.Token)));
        }
        
        // 启动进度更新定时器
        _progressTimer = new Timer(UpdateProgress, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
        
        _logger.LogInformation("下载调度器已启动，最大并发数: {MaxConcurrent}", _config.MaxConcurrentTasks);
    }

    /// <summary>
    /// 所有下载任务
    /// </summary>
    public IReadOnlyCollection<DlTaskInfo> Tasks => _tasks.Values;

    /// <summary>
    /// 正在下载的任务数
    /// </summary>
    public int ActiveTaskCount => _tasks.Values.Count(t => t.State == DlTaskState.Running);

    /// <summary>
    /// 添加下载任务
    /// </summary>
    public async Task<DlTaskInfo> AddTaskAsync(string url, string destinationPath)
    {
        var task = new DlTaskInfo
        {
            Id = Guid.NewGuid().ToString("N"),
            Url = url,
            DestinationPath = destinationPath,
            State = DlTaskState.Pending
        };

        _tasks[task.Id] = task;
        _pendingQueue.Enqueue(task.Id);
        
        TaskAdded?.Invoke(task);
        
        _logger.LogInformation("已添加下载任务: {Id} -> {Url}", task.Id, url);
        
        // 确保目标目录存在
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        return task;
    }

    /// <summary>
    /// 添加批量下载任务
    /// </summary>
    public async Task<IReadOnlyList<DlTaskInfo>> AddTasksAsync(IEnumerable<(string Url, string Path)> files)
    {
        var tasks = new List<DlTaskInfo>();
        foreach (var (url, path) in files)
        {
            var task = await AddTaskAsync(url, path);
            tasks.Add(task);
        }
        return tasks;
    }

    /// <summary>
    /// 取消指定任务
    /// </summary>
    public async Task CancelTaskAsync(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
        {
            _logger.LogWarning("尝试取消不存在的任务: {TaskId}", taskId);
            return;
        }

        if (task.State == DlTaskState.Completed || task.State == DlTaskState.Canceled)
        {
            _logger.LogDebug("任务已完成或已取消: {TaskId}", taskId);
            return;
        }

        task.State = DlTaskState.Canceled;
        TaskCanceled?.Invoke(task);
        
        _logger.LogInformation("已取消任务: {TaskId}", taskId);
    }

    /// <summary>
    /// 取消所有任务
    /// </summary>
    public async Task CancelAllTasksAsync()
    {
        foreach (var task in _tasks.Values.Where(t => t.State == DlTaskState.Running || t.State == DlTaskState.Pending))
        {
            task.State = DlTaskState.Canceled;
            TaskCanceled?.Invoke(task);
        }
        
        _logger.LogInformation("已取消所有任务");
    }

    /// <summary>
    /// 移除已完成的任务
    /// </summary>
    public void RemoveCompletedTasks()
    {
        var completed = _tasks.Values.Where(t => t.State == DlTaskState.Completed || t.State == DlTaskState.Canceled || t.State == DlTaskState.Failed).ToList();
        foreach (var task in completed)
        {
            _tasks.TryRemove(task.Id, out _);
        }
        
        _logger.LogDebug("已移除 {Count} 个已完成的任务", completed.Count);
    }

    /// <summary>
    /// 获取任务统计信息
    /// </summary>
    public DlStatistics GetStatistics()
    {
        return new DlStatistics
        {
            Total = _tasks.Count,
            Pending = _tasks.Values.Count(t => t.State == DlTaskState.Pending),
            Running = _tasks.Values.Count(t => t.State == DlTaskState.Running),
            Completed = _tasks.Values.Count(t => t.State == DlTaskState.Completed),
            Failed = _tasks.Values.Count(t => t.State == DlTaskState.Failed),
            Canceled = _tasks.Values.Count(t => t.State == DlTaskState.Canceled),
            TotalBytes = _tasks.Values.Sum(t => t.TotalBytes > 0 ? t.TotalBytes : 0),
            DownloadedBytes = _tasks.Values.Sum(t => t.DownloadedBytes)
        };
    }

    private async Task WorkerLoop(CancellationToken globalToken)
    {
        while (!globalToken.IsCancellationRequested)
        {
            await _taskSemaphore.WaitAsync(globalToken);
            
            string? taskId = null;
            await _queueLock.WaitAsync(globalToken);
            try
            {
                if (_pendingQueue.TryDequeue(out var id))
                {
                    taskId = id;
                }
            }
            finally
            {
                _queueLock.Release();
            }

            if (taskId == null)
            {
                _taskSemaphore.Release();
                await Task.Delay(100, globalToken);
                continue;
            }

            try
            {
                await ProcessTaskAsync(taskId, globalToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理任务时出错: {TaskId}", taskId);
            }
            finally
            {
                _taskSemaphore.Release();
            }
        }
    }

    private async Task ProcessTaskAsync(string taskId, CancellationToken globalToken)
    {
        if (!_tasks.TryGetValue(taskId, out var task))
            return;

        if (task.State == DlTaskState.Canceled)
            return;

        task.State = DlTaskState.Running;
        task.StartTime = DateTime.Now;
        TaskStarted?.Invoke(task);

        var retryCount = 0;
        while (retryCount < _config.MaxRetryCount && task.State != DlTaskState.Canceled)
        {
            try
            {
                await DownloadTaskAsync(task, globalToken);
                
                if (task.State == DlTaskState.Completed)
                {
                    task.CompleteTime = DateTime.Now;
                    TaskCompleted?.Invoke(task);
                    _logger.LogInformation("下载完成: {Id} -> {Path}", task.Id, task.DestinationPath);
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                if (task.State == DlTaskState.Canceled)
                {
                    _logger.LogInformation("下载已取消: {Id}", task.Id);
                    return;
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                task.RetryCount = retryCount;
                task.ErrorMessage = ex.Message;
                task.Error = ex;
                
                _logger.LogWarning(ex, "下载失败，重试 ({Retry}/{MaxRetry}): {Id} -> {Url}", 
                    retryCount, _config.MaxRetryCount, task.Id, task.Url);
                
                if (retryCount < _config.MaxRetryCount)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(_config.RetryDelayBase, retryCount));
                    await Task.Delay(delay, globalToken);
                }
            }
        }

        if (retryCount >= _config.MaxRetryCount)
        {
            task.State = DlTaskState.Failed;
            TaskFailed?.Invoke(task);
            _logger.LogError("下载失败，超过最大重试次数: {Id} -> {Url}", task.Id, task.Url);
        }
    }

    private async Task DownloadTaskAsync(DlTaskInfo task, CancellationToken globalToken)
    {
        // 获取文件信息
        using var headRequest = new HttpRequestMessage(HttpMethod.Head, task.Url);
        using var headResponse = await _httpClient.SendAsync(headRequest, globalToken);
        headResponse.EnsureSuccessStatusCode();

        var totalBytes = headResponse.Content.Headers.ContentLength ?? -1;
        var supportsRange = headResponse.Headers.AcceptRanges.Contains("bytes");

        task.TotalBytes = totalBytes;

        // 判断是否需要分片下载
        var shouldSegment = supportsRange && totalBytes > _config.SegmentThreshold && _config.MaxSegmentsPerTask > 1;

        if (shouldSegment)
        {
            await DownloadWithSegmentsAsync(task, totalBytes, globalToken);
        }
        else
        {
            await DownloadSingleAsync(task, globalToken);
        }

        task.State = DlTaskState.Completed;
    }

    private async Task DownloadSingleAsync(DlTaskInfo task, CancellationToken token)
    {
        using var response = await _httpClient.GetAsync(task.Url, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(token);
        await using var fileStream = new FileStream(task.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, _config.BufferSize, true);

        var buffer = new byte[_config.BufferSize];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, token)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
            totalBytesRead += bytesRead;
            task.DownloadedBytes = totalBytesRead;
        }
    }

    private async Task DownloadWithSegmentsAsync(DlTaskInfo task, long totalBytes, CancellationToken token)
    {
        var segmentCount = Math.Min(_config.MaxSegmentsPerTask, (int)(totalBytes / _config.SegmentThreshold) + 1);
        var segmentSize = totalBytes / segmentCount;

        var segments = new List<DlTaskSegment>();
        for (var i = 0; i < segmentCount; i++)
        {
            var begin = i * segmentSize;
            var end = (i == segmentCount - 1) ? totalBytes : begin + segmentSize;
            
            segments.Add(new DlTaskSegment
            {
                Index = i,
                BeginOffset = begin,
                EndOffset = end,
                CurrentOffset = begin
            });
        }

        task.SegmentCount = segmentCount;

        // 创建临时文件
        var tempPath = task.DestinationPath + ".tmp";
        await using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Write, _config.BufferSize, true);

        // 下载所有分片
        var downloadTasks = segments.Select(segment => DownloadSegmentAsync(task, segment, token));
        await Task.WhenAll(downloadTasks);

        // 移动临时文件到目标位置
        File.Move(tempPath, task.DestinationPath, true);
    }

    private async Task DownloadSegmentAsync(DlTaskInfo task, DlTaskSegment segment, CancellationToken token)
    {
        task.ActiveSegments++;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, task.Url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(segment.BeginOffset, segment.EndOffset - 1);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync(token);

            var buffer = new byte[_config.BufferSize];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, token)) > 0)
            {
                // 写入临时文件对应位置（注意：这里需要更复杂的实现，暂时简化）
                segment.CurrentOffset += bytesRead;
                segment.IsCompleted = segment.CurrentOffset >= segment.EndOffset;
                
                // 更新总进度
                task.DownloadedBytes += bytesRead;
            }
        }
        finally
        {
            task.ActiveSegments--;
        }
    }

    private void UpdateProgress(object? state)
    {
        foreach (var task in _tasks.Values.Where(t => t.State == DlTaskState.Running))
        {
            if (task.TotalBytes > 0)
            {
                task.Progress = (double)task.DownloadedBytes / task.TotalBytes;
                TaskProgressChanged?.Invoke(task);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _globalCts.Cancel();
        _progressTimer.Dispose();
        
        try
        {
            Task.WaitAll(_workerTasks.ToArray(), TimeSpan.FromSeconds(5));
        }
        catch
        {
            // 忽略等待超时
        }

        _httpClient.Dispose();
        _globalCts.Dispose();
        _taskSemaphore.Dispose();
        _queueLock.Dispose();
        
        _logger.LogInformation("下载调度器已停止");
    }
}

/// <summary>
/// 下载统计信息
/// </summary>
public class DlStatistics
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Running { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Canceled { get; set; }
    public long TotalBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public double OverallProgress => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes : 0;
}