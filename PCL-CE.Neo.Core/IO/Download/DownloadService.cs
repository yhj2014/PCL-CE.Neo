using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.IO.Download;

public class DownloadService : IDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly IHashProviderFactory _hashProviderFactory;
    private readonly ILogger<DownloadService> _logger;

    public event EventHandler<DownloadProgressEventArgs>? ProgressChanged;
    public event EventHandler<DownloadStateChangedEventArgs>? StateChanged;

    public DownloadService(HttpClient httpClient, IHashProviderFactory hashProviderFactory, ILogger<DownloadService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _hashProviderFactory = hashProviderFactory ?? throw new ArgumentNullException(nameof(hashProviderFactory));
        _logger = logger;
    }

    public Task<DownloadTask> CreateDownloadTaskAsync(string url, string destinationPath, 
        string? checksum = null, string? checksumAlgorithm = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentNullException(nameof(destinationPath));

        var task = new DownloadTask(url, destinationPath)
        {
            Checksum = checksum,
            ChecksumAlgorithm = checksumAlgorithm
        };

        _logger.LogInformation("Created download task: {Id} -> {Url}", task.Id, url);
        return Task.FromResult(task);
    }

    public async Task StartDownloadAsync(DownloadTask task, CancellationToken cancellationToken = default)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        var oldState = task.State;
        task.Start();
        OnStateChanged(task, oldState, task.State);

        try
        {
            _logger.LogInformation("Starting download: {Id} -> {Url}", task.Id, task.Url);

            using var response = await _httpClient.GetAsync(task.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            task.TotalSize = response.Content.Headers.ContentLength ?? -1;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(task.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[81920];
            long totalBytesRead = 0;
            int bytesRead;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;
                task.DownloadedSize = totalBytesRead;

                var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                if (elapsedSeconds > 0)
                {
                    task.Speed = totalBytesRead / elapsedSeconds / 1024 / 1024;
                }

                OnProgressChanged(task, totalBytesRead, task.TotalSize, task.Speed);
            }

            stopwatch.Stop();
            _logger.LogInformation("Download completed: {Id} -> {Path}", task.Id, task.DestinationPath);

            if (!string.IsNullOrEmpty(task.Checksum))
            {
                if (!await VerifyChecksumAsync(task))
                {
                    throw new InvalidDataException("Checksum verification failed");
                }
            }

            var completedOldState = task.State;
            task.Complete();
            OnStateChanged(task, completedOldState, task.State);
        }
        catch (OperationCanceledException)
        {
            task.Cancel();
            OnStateChanged(task, oldState, task.State);
            _logger.LogInformation("Download canceled: {Id}", task.Id);
        }
        catch (HttpRequestException ex)
        {
            task.Fail(DownloadErrorType.NetworkError, ex.Message);
            OnStateChanged(task, oldState, task.State);
            _logger.LogError(ex, "Network error during download: {Id}", task.Id);
            throw;
        }
        catch (Exception ex)
        {
            task.Fail(DownloadErrorType.FileError, ex.Message);
            OnStateChanged(task, oldState, task.State);
            _logger.LogError(ex, "Error during download: {Id}", task.Id);
            throw;
        }
    }

    public Task PauseDownloadAsync(DownloadTask task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        var oldState = task.State;
        task.Pause();
        OnStateChanged(task, oldState, task.State);
        _logger.LogInformation("Download paused: {Id}", task.Id);
        return Task.CompletedTask;
    }

    public Task ResumeDownloadAsync(DownloadTask task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        var oldState = task.State;
        task.Resume();
        OnStateChanged(task, oldState, task.State);
        _logger.LogInformation("Download resumed: {Id}", task.Id);
        return Task.CompletedTask;
    }

    public Task CancelDownloadAsync(DownloadTask task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        var oldState = task.State;
        task.Cancel();
        OnStateChanged(task, oldState, task.State);
        _logger.LogInformation("Download canceled: {Id}", task.Id);
        return Task.CompletedTask;
    }

    public async Task<bool> VerifyChecksumAsync(DownloadTask task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        if (string.IsNullOrEmpty(task.Checksum) || string.IsNullOrEmpty(task.ChecksumAlgorithm))
            return true;

        try
        {
            var fileBytes = await File.ReadAllBytesAsync(task.DestinationPath);
            var provider = _hashProviderFactory.GetProvider(task.ChecksumAlgorithm);
            var fileHash = await provider.ComputeHashStringAsync(fileBytes);
            var result = fileHash.Equals(task.Checksum, StringComparison.OrdinalIgnoreCase);

            _logger.LogInformation("Checksum verification {Result} for task: {Id}", result ? "passed" : "failed", task.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Checksum verification failed for task: {Id}", task.Id);
            return false;
        }
    }

    protected virtual void OnProgressChanged(DownloadTask task, long bytesDownloaded, long totalBytes, double speed)
    {
        ProgressChanged?.Invoke(this, new DownloadProgressEventArgs(task, bytesDownloaded, totalBytes, speed));
    }

    protected virtual void OnStateChanged(DownloadTask task, DownloadState oldState, DownloadState newState)
    {
        StateChanged?.Invoke(this, new DownloadStateChangedEventArgs(task, oldState, newState));
    }
}