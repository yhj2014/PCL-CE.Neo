using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Minecraft.Yggdrasil;

public sealed class YggdrasilLegacyClient
{
    private readonly YggdrasilLegacyAuthenticateOptions _options;
    private readonly ILogger<YggdrasilLegacyClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public YggdrasilLegacyClient(YggdrasilLegacyAuthenticateOptions options, ILogger<YggdrasilLegacyClient>? logger = null)
    {
        _options = options;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<YggdrasilLegacyClient>.Instance;
    }

    public async Task<YggdrasilAuthenticateResult?> AuthenticateAsync(CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(_options.Username);
        ArgumentException.ThrowIfNullOrEmpty(_options.Password);

        _logger.LogInformation("Authenticating with Yggdrasil server: {Server}", _options.YggdrasilApiLocation);

        var credential = new YggdrasilCredential
        {
            User = _options.Username,
            Password = _options.Password,
        };

        var address = $"{_options.YggdrasilApiLocation}/authserver/authenticate";

        try
        {
            using var client = _options.GetClient.Invoke();
            var jsonContent = JsonSerializer.Serialize(credential, JsonOptions);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            if (_options.Headers != null)
            {
                foreach (var header in _options.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using var response = await client.PostAsync(address, content, token).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Yggdrasil authentication failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var result = JsonSerializer.Deserialize<YggdrasilAuthenticateResult>(responseContent, JsonOptions);

            if (result != null && !string.IsNullOrEmpty(result.Error))
            {
                _logger.LogError("Yggdrasil error: {Error} - {Message}", result.Error, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil authentication request failed");
            throw;
        }
    }

    public async Task<YggdrasilAuthenticateResult?> RefreshAsync(CancellationToken token, Profile? selectedProfile)
    {
        ArgumentException.ThrowIfNullOrEmpty(_options.AccessToken);

        _logger.LogInformation("Refreshing Yggdrasil token");

        var refreshData = new YggdrasilRefresh
        {
            AccessToken = _options.AccessToken
        };

        if (selectedProfile is not null)
            refreshData.SelectedProfile = selectedProfile;

        var address = $"{_options.YggdrasilApiLocation}/authserver/refresh";

        try
        {
            using var client = _options.GetClient.Invoke();
            var jsonContent = JsonSerializer.Serialize(refreshData, JsonOptions);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            if (_options.Headers != null)
            {
                foreach (var header in _options.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using var response = await client.PostAsync(address, content, token).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Yggdrasil refresh failed: {StatusCode}", response.StatusCode);
                return null;
            }

            return JsonSerializer.Deserialize<YggdrasilAuthenticateResult>(responseContent, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil refresh request failed");
            throw;
        }
    }

    public async Task<bool> ValidateAsync(CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(_options.AccessToken);

        _logger.LogInformation("Validating Yggdrasil token");

        var validateData = new YggdrasilRefresh
        {
            AccessToken = _options.AccessToken
        };

        var address = $"{_options.YggdrasilApiLocation}/authserver/validate";

        try
        {
            using var client = _options.GetClient.Invoke();
            var jsonContent = JsonSerializer.Serialize(validateData, JsonOptions);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            if (_options.Headers != null)
            {
                foreach (var header in _options.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using var response = await client.PostAsync(address, content, token).ConfigureAwait(false);
            return response.StatusCode == HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil validation request failed");
            return false;
        }
    }

    public async Task InvalidateAsync(CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(_options.AccessToken);

        _logger.LogInformation("Invalidating Yggdrasil token");

        var validateData = new YggdrasilRefresh
        {
            AccessToken = _options.AccessToken
        };

        var address = $"{_options.YggdrasilApiLocation}/authserver/invalidate";

        try
        {
            using var client = _options.GetClient.Invoke();
            var jsonContent = JsonSerializer.Serialize(validateData, JsonOptions);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            if (_options.Headers != null)
            {
                foreach (var header in _options.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using var response = await client.PostAsync(address, content, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil invalidate request failed");
            throw;
        }
    }

    public async Task<(bool IsSuccess, string ErrorDescription)> SignOutAsync(CancellationToken token)
    {
        _logger.LogInformation("Signing out from Yggdrasil server");

        var signoutData = new JsonObject
        {
            ["username"] = _options.Username,
            ["password"] = _options.Password
        };

        var address = $"{_options.YggdrasilApiLocation}/authserver/signout";

        try
        {
            using var client = _options.GetClient.Invoke();
            var jsonContent = signoutData.ToJsonString();
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            if (_options.Headers != null)
            {
                foreach (var header in _options.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            using var response = await client.PostAsync(address, content, token).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);

            var data = JsonNode.Parse(responseContent);
            var errorMessage = data?["errorMessage"]?.ToString() ?? string.Empty;

            return (response.StatusCode == HttpStatusCode.NoContent, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil signout request failed");
            return (false, ex.Message);
        }
    }
}