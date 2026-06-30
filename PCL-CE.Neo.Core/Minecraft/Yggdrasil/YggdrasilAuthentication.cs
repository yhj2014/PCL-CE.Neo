using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Yggdrasil;

public class YggdrasilAuthentication
{
    private readonly ILogger<YggdrasilAuthentication> _logger;
    private readonly HttpClient _httpClient;

    public YggdrasilAuthentication(ILogger<YggdrasilAuthentication> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public YggdrasilAuthentication() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<YggdrasilAuthentication>.Instance) { }

    public async Task<AuthenticationResponse?> AuthenticateAsync(string username, string password, ApiLocation apiLocation)
    {
        if (string.IsNullOrEmpty(apiLocation.AuthServerUrl))
        {
            return new AuthenticationResponse
            {
                AccessToken = Guid.NewGuid().ToString(),
                ClientToken = Guid.NewGuid().ToString(),
                SelectedProfile = new Profile
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = username
                }
            };
        }

        try
        {
            var request = new AuthenticationRequest
            {
                Agent = new Agent { Name = "Minecraft", Version = 1 },
                Username = username,
                Password = password,
                ClientToken = Guid.NewGuid().ToString()
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{apiLocation.AuthServerUrl}/authenticate", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Yggdrasil authentication failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthenticationResponse>(responseJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil authentication exception");
            return null;
        }
    }

    public async Task<RefreshResponse?> RefreshAsync(string accessToken, string clientToken, ApiLocation apiLocation)
    {
        try
        {
            var request = new RefreshRequest
            {
                AccessToken = accessToken,
                ClientToken = clientToken
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{apiLocation.AuthServerUrl}/refresh", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Yggdrasil refresh failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RefreshResponse>(responseJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil refresh exception");
            return null;
        }
    }

    public async Task<bool> ValidateAsync(string accessToken, string clientToken, ApiLocation apiLocation)
    {
        try
        {
            var request = new ValidateRequest
            {
                AccessToken = accessToken,
                ClientToken = clientToken
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{apiLocation.AuthServerUrl}/validate", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil validate exception");
            return false;
        }
    }

    public async Task<bool> InvalidateAsync(string accessToken, string clientToken, ApiLocation apiLocation)
    {
        try
        {
            var request = new InvalidateRequest
            {
                AccessToken = accessToken,
                ClientToken = clientToken
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{apiLocation.AuthServerUrl}/invalidate", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yggdrasil invalidate exception");
            return false;
        }
    }

    #region Models

    public class Agent
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("version")] public int Version { get; set; }
    }

    public class Profile
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("properties")] public List<ProfileProperty> Properties { get; set; } = [];
    }

    public class ProfileProperty
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("value")] public string Value { get; set; } = "";
        [JsonPropertyName("signature")] public string? Signature { get; set; }
    }

    public class AuthenticationRequest
    {
        [JsonPropertyName("agent")] public Agent Agent { get; set; } = new();
        [JsonPropertyName("username")] public string Username { get; set; } = "";
        [JsonPropertyName("password")] public string Password { get; set; } = "";
        [JsonPropertyName("clientToken")] public string ClientToken { get; set; } = "";
        [JsonPropertyName("requestUser")] public bool RequestUser { get; set; } = true;
    }

    public class AuthenticationResponse
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = "";
        [JsonPropertyName("clientToken")] public string ClientToken { get; set; } = "";
        [JsonPropertyName("selectedProfile")] public Profile? SelectedProfile { get; set; }
        [JsonPropertyName("availableProfiles")] public List<Profile> AvailableProfiles { get; set; } = [];
        [JsonPropertyName("user")] public User? User { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";
        [JsonPropertyName("properties")] public List<ProfileProperty> Properties { get; set; } = [];
    }

    public class RefreshRequest
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = "";
        [JsonPropertyName("clientToken")] public string ClientToken { get; set; } = "";
        [JsonPropertyName("requestUser")] public bool RequestUser { get; set; } = true;
    }

    public class RefreshResponse
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = "";
        [JsonPropertyName("clientToken")] public string ClientToken { get; set; } = "";
        [JsonPropertyName("selectedProfile")] public Profile? SelectedProfile { get; set; }
        [JsonPropertyName("user")] public User? User { get; set; }
    }

    public class ValidateRequest
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = "";
        [JsonPropertyName("clientToken")] public string ClientToken { get; set; } = "";
    }

    public class InvalidateRequest
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; } = "";
        [JsonPropertyName("clientToken")] public string ClientToken { get; set; } = "";
    }

    #endregion
}