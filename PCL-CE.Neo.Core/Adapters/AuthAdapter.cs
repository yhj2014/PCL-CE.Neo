using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class AuthAdapter : IAuthAdapter
{
    private readonly ILogger<AuthAdapter> _logger;
    private readonly INetworkAdapter _network;
    private readonly IConfigAdapter _config;
    private readonly IPathsAdapter _paths;

    private AuthState _currentState = AuthState.LoggedOut;
    private UserProfile? _currentUser;
    private AuthToken? _currentToken;

    public event Action<AuthState>? StateChanged;

    public AuthState CurrentState => _currentState;
    public UserProfile? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentState == AuthState.LoggedIn && _currentUser != null;
    public string? AccessToken => _currentToken?.AccessToken;

    public AuthAdapter(
        ILogger<AuthAdapter> logger,
        INetworkAdapter network,
        IConfigAdapter config,
        IPathsAdapter paths)
    {
        _logger = logger;
        _network = network;
        _config = config;
        _paths = paths;
    }

    public async Task<AuthResult> LoginMicrosoftAsync()
    {
        try
        {
            _logger.LogInformation("开始微软登录流程");
            SetState(AuthState.LoggingIn);

            var tokenJson = _config.GetConfig("LoginMsJson", "{}");
            if (!string.IsNullOrEmpty(tokenJson) && tokenJson != "{}")
            {
                var token = JsonSerializer.Deserialize<MicrosoftToken>(tokenJson);
                if (token != null && token.expires_at > DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    var user = await ValidateMicrosoftTokenAsync(token.access_token);
                    if (user != null)
                    {
                        _currentToken = new AuthToken
                        {
                            AccessToken = token.access_token,
                            RefreshToken = token.refresh_token,
                            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(token.expires_at).DateTime,
                            Xuid = token.xuid
                        };
                        _currentUser = user;
                        SetState(AuthState.LoggedIn);
                        _logger.LogInformation("微软登录成功: {Username}", user.Username);
                        return AuthResult.Succeeded(user, _currentToken);
                    }
                }
            }

            SetState(AuthState.LoggedOut);
            return AuthResult.Failed("需要重新进行微软登录");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "微软登录失败");
            SetState(AuthState.Error);
            return AuthResult.Failed(ex.Message, ex);
        }
    }

    public async Task<AuthResult> LoginOfflineAsync(string username)
    {
        try
        {
            _logger.LogInformation("离线登录: {Username}", username);

            var offlineName = _config.GetConfig("LoginLegacyName", username);

            _currentUser = new UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                Username = offlineName,
                DisplayName = offlineName,
                Provider = AuthProvider.Offline
            };

            _currentToken = new AuthToken
            {
                AccessToken = "offline",
                ExpiresAt = DateTime.MaxValue
            };

            SetState(AuthState.LoggedIn);
            _logger.LogInformation("离线登录成功: {Username}", username);
            return AuthResult.Succeeded(_currentUser, _currentToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "离线登录失败");
            SetState(AuthState.Error);
            return AuthResult.Failed(ex.Message, ex);
        }
    }

    public async Task<AuthResult> LoginThirdPartyAsync(string server, string username, string password)
    {
        try
        {
            _logger.LogInformation("第三方登录: {Server}", server);
            SetState(AuthState.LoggingIn);

            var response = await _network.PostAsync($"{server}/authserver/authenticate",
                JsonSerializer.Serialize(new
                {
                    username,
                    password,
                    clientToken = Guid.NewGuid().ToString()
                }));

            if (response.IsSuccess)
            {
                var result = JsonSerializer.Deserialize<YggdrasilResponse>(response.BodyAsString);
                if (result?.SelectedProfile != null)
                {
                    _currentUser = new UserProfile
                    {
                        Id = result.SelectedProfile.Id,
                        Username = result.SelectedProfile.Name,
                        DisplayName = result.SelectedProfile.Name,
                        Provider = AuthProvider.Yggdrasil
                    };

                    _currentToken = new AuthToken
                    {
                        AccessToken = result.AccessToken,
                        ExpiresAt = DateTimeOffset.FromUnixTimeMilliseconds(result.ExpiresAt).DateTime
                    };

                    SetState(AuthState.LoggedIn);
                    return AuthResult.Succeeded(_currentUser, _currentToken);
                }
            }

            SetState(AuthState.Error);
            return AuthResult.Failed($"第三方登录失败: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "第三方登录失败");
            SetState(AuthState.Error);
            return AuthResult.Failed(ex.Message, ex);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("开始登出");
            SetState(AuthState.LoggingOut);

            _config.SetConfig("LoginMsJson", "{}");

            _currentUser = null;
            _currentToken = null;

            SetState(AuthState.LoggedOut);
            _logger.LogInformation("登出完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登出失败");
        }
    }

    public Task<UserProfile?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    public async Task<AuthToken?> RefreshTokenAsync()
    {
        if (_currentToken?.RefreshToken == null) return null;

        try
        {
            _logger.LogDebug("刷新令牌");
            return _currentToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新令牌失败");
            return null;
        }
    }

    private void SetState(AuthState state)
    {
        _currentState = state;
        StateChanged?.Invoke(state);
    }

    private async Task<UserProfile?> ValidateMicrosoftTokenAsync(string accessToken)
    {
        try
        {
            _logger.LogDebug("验证微软访问令牌");

            // 调用 Xbox Live API 获取 XSTS token
            var xboxResponse = await _network.PostAsync(
                "https://user.auth.xboxlive.com/user/authenticate",
                JsonSerializer.Serialize(new
                {
                    Properties = new
                    {
                        AuthMethod = "RPS",
                        SiteName = "user.auth.xboxlive.com",
                        RpsTicket = $"d={accessToken}"
                    },
                    RelyingParty = "http://auth.xboxlive.com",
                    TokenType = "JWT"
                }));

            if (!xboxResponse.IsSuccess)
            {
                _logger.LogWarning("Xbox Live 认证失败: {Status}", xboxResponse.StatusCode);
                return null;
            }

            var xboxData = JsonSerializer.Deserialize<XboxAuthResponse>(xboxResponse.BodyAsString);
            if (xboxData == null)
            {
                return null;
            }

            // 调用 Minecraft API 获取访问令牌
            var mcResponse = await _network.PostAsync(
                "https://api.minecraftservices.com/authentication/login_with_xbox",
                JsonSerializer.Serialize(new
                {
                    identityToken = $"XBL3.0 x={xboxData.DisplayClaims?.Xui?[0]?.Xid ?? ""};{xboxData.Token}"
                }));

            if (!mcResponse.IsSuccess)
            {
                _logger.LogWarning("Minecraft 认证失败: {Status}", mcResponse.StatusCode);
                return null;
            }

            var mcData = JsonSerializer.Deserialize<MinecraftAuthResponse>(mcResponse.BodyAsString);
            if (mcData == null)
            {
                return null;
            }

            // 获取玩家资料
            var profileResponse = await _network.GetAsync(
                "https://api.minecraftservices.com/minecraft/profile",
                accessToken: mcData.AccessToken);

            if (!profileResponse.IsSuccess)
            {
                _logger.LogWarning("获取玩家资料失败: {Status}", profileResponse.StatusCode);
                return null;
            }

            var profile = JsonSerializer.Deserialize<MinecraftProfile>(profileResponse.BodyAsString);
            if (profile == null)
            {
                return null;
            }

            return new UserProfile
            {
                Id = profile.Id,
                Username = profile.Name,
                DisplayName = profile.Name,
                Provider = AuthProvider.Microsoft,
                SkinUrl = profile.Skins?.FirstOrDefault()?.Url,
                CapeUrl = profile.Capes?.FirstOrDefault()?.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证微软令牌失败");
            return null;
        }
    }

    private class XboxAuthResponse
    {
        public string Token { get; set; } = "";
        public DisplayClaims? DisplayClaims { get; set; }
    }

    private class DisplayClaims
    {
        public List<XboxUser>? Xui { get; set; }
    }

    private class XboxUser
    {
        public string? Xid { get; set; }
        public string? UserHash { get; set; }
    }

    private class MinecraftAuthResponse
    {
        public string AccessToken { get; set; } = "";
        public string TokenType { get; set; } = "";
    }

    private class MinecraftProfile
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<MinecraftSkin>? Skins { get; set; }
        public List<MinecraftCape>? Capes { get; set; }
    }

    private class MinecraftSkin
    {
        public string Id { get; set; } = "";
        public string State { get; set; } = "";
        public string Url { get; set; } = "";
        public string? Variant { get; set; }
    }

    private class MinecraftCape
    {
        public string Id { get; set; } = "";
        public string State { get; set; } = "";
        public string Url { get; set; } = "";
        public string Alias { get; set; } = "";
    }
}

    private class MicrosoftToken
    {
        public string access_token { get; set; } = "";
        public string refresh_token { get; set; } = "";
        public long expires_at { get; set; }
        public string xuid { get; set; } = "";
    }

    private class YggdrasilResponse
    {
        public string AccessToken { get; set; } = "";
        public long ExpiresAt { get; set; }
        public YggdrasilProfile? SelectedProfile { get; set; }
    }

    private class YggdrasilProfile
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
