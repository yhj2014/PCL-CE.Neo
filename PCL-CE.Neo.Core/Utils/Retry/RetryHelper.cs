using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Retry;

public static class RetryHelper
{
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        var actualDelay = delay ?? TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                    throw;

                if (shouldRetry != null && !shouldRetry(ex))
                    throw;

                LogWrapper.Warn(ex, $"Operation failed on attempt {attempt}/{maxRetries}, retrying...");
                
                await Task.Delay(actualDelay * attempt);
            }
        }
        
        throw new InvalidOperationException("Retry logic failed");
    }

    public static async Task RetryAsync(
        Func<Task> operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        var actualDelay = delay ?? TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                    throw;

                if (shouldRetry != null && !shouldRetry(ex))
                    throw;

                LogWrapper.Warn(ex, $"Operation failed on attempt {attempt}/{maxRetries}, retrying...");
                
                await Task.Delay(actualDelay * attempt);
            }
        }
        
        throw new InvalidOperationException("Retry logic failed");
    }

    public static T Retry<T>(
        Func<T> operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        var actualDelay = delay ?? TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                    throw;

                if (shouldRetry != null && !shouldRetry(ex))
                    throw;

                LogWrapper.Warn(ex, $"Operation failed on attempt {attempt}/{maxRetries}, retrying...");
                
                Thread.Sleep(actualDelay * attempt);
            }
        }
        
        throw new InvalidOperationException("Retry logic failed");
    }

    public static void Retry(
        Action operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        var actualDelay = delay ?? TimeSpan.FromSeconds(1);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                operation();
                return;
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                    throw;

                if (shouldRetry != null && !shouldRetry(ex))
                    throw;

                LogWrapper.Warn(ex, $"Operation failed on attempt {attempt}/{maxRetries}, retrying...");
                
                Thread.Sleep(actualDelay * attempt);
            }
        }
        
        throw new InvalidOperationException("Retry logic failed");
    }

    public static async Task<T> RetryWithBackoffAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 5,
        TimeSpan baseDelay = default,
        Func<Exception, bool>? shouldRetry = null)
    {
        var actualBaseDelay = baseDelay == default ? TimeSpan.FromSeconds(1) : baseDelay;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                    throw;

                if (shouldRetry != null && !shouldRetry(ex))
                    throw;

                var delay = actualBaseDelay * (int)Math.Pow(2, attempt - 1);
                LogWrapper.Warn(ex, $"Operation failed on attempt {attempt}/{maxRetries}, retrying in {delay.TotalSeconds}s...");
                
                await Task.Delay(delay);
            }
        }
        
        throw new InvalidOperationException("Retry logic failed");
    }

    public static async Task<T> RetryWithJitterAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 5,
        TimeSpan baseDelay = default)
    {
        var actualBaseDelay = baseDelay == default ? TimeSpan.FromSeconds(1) : baseDelay;
        var random = new Random();
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                    throw;

                var delay = actualBaseDelay * (int)Math.Pow(2, attempt - 1);
                var jitter = TimeSpan.FromMilliseconds(random.Next(0, 1000));
                var totalDelay = delay + jitter;
                
                LogWrapper.Warn(ex, $"Operation failed on attempt {attempt}/{maxRetries}, retrying in {totalDelay.TotalSeconds}s...");
                
                await Task.Delay(totalDelay);
            }
        }
        
        throw new InvalidOperationException("Retry logic failed");
    }
}