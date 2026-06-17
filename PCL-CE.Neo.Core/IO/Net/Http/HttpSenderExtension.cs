using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Net.Http;

public static class HttpSenderExtension
{
    public static async Task<HttpResponseMessage> SendAsync(
        this HttpRequestMessage requestMessage,
        HttpClient? httpClient = null,
        bool addMetadata = true,
        bool enableLogging = true,
        HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead,
        int retryTimes = 3,
        CancellationToken cancellationToken = default)
    {
        using var request = requestMessage;
        httpClient ??= new HttpClient();

        if (addMetadata)
        {
            request
                .WithHeader("User-Agent", "PCL-CE.Neo/1.0.0 (pclc.cc)")
                .WithHeader("Referer", "https://pclc.cc/");
        }

        var requestId = Guid.NewGuid().ToString();

        for (int attempt = 1; attempt <= retryTimes; attempt++)
        {
            try
            {
                using var requestCopy = await request.CloneAsync().ConfigureAwait(false);
                var resp = await httpClient
                    .SendAsync(requestCopy, httpCompletionOption, cancellationToken)
                    .ConfigureAwait(false);

                if (resp.IsSuccessStatusCode || attempt >= retryTimes)
                {
                    return resp;
                }
            }
            catch (Exception)
            {
                if (attempt >= retryTimes)
                    throw;

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken).ConfigureAwait(false);
            }
        }

        throw new HttpRequestException("Request failed after retries.");
    }
}