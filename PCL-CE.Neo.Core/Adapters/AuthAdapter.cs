using System.Text.Json;
using System.Text.Json.Serialization;
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
    private List<UserProfile> _profiles = new();
    private string? _selectedProfileId;
    private bool _isRestrictedFeatureAllowed;
    private bool _hasNetwork = true;

    public event Action<AuthState>? StateChanged;
    public event Action? ProfilesChanged;

    public AuthState CurrentState => _currentState;
    public UserProfile? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentState == AuthState.LoggedIn && _currentUser != null;
    public string? AccessToken => _currentToken?.AccessToken;

    public AuthAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthAdapter>.Instance,
        new NetworkAdapter(),
        new ConfigAdapter(),
        new PathsAdapter())
    {
    }

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
        LoadProfiles();
        CheckRestrictedFeature();
    }

    private void CheckRestrictedFeature()
    {
        try
        {
            var timeZone = TimeZoneInfo.Local.Id;
            var culture = System.Globalization.CultureInfo.CurrentCulture.Name;
            _isRestrictedFeatureAllowed = timeZone == "China Standard Time" &&
                                           (culture == "zh-CN" || culture == "zh-Hans");
            _logger.LogDebug("区域限制功能状态: {IsRestricted}", _isRestrictedFeatureAllowed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "检查区域限制功能失败");
            _isRestrictedFeatureAllowed = false;
        }
    }

    private void LoadProfiles()
    {
        try
        {
            var profilesJson = _config.GetConfig("Profiles", "[]");
            if (!string.IsNullOrEmpty(profilesJson) && profilesJson != "[]")
            {
                _profiles = JsonSerializer.Deserialize<List<UserProfile>>(profilesJson) ?? new();
                _logger.LogInformation("已加载 {Count} 个档案", _profiles.Count);
            }

            _selectedProfileId = _config.GetConfig<string>("SelectedProfileId", null);
            if (string.IsNullOrEmpty(_selectedProfileId) && _profiles.Count > 0)
            {
                _selectedProfileId = _profiles[0].Id;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载档案失败");
            _profiles = new List<UserProfile>();
        }
    }

    private void SaveProfiles()
    {
        try
        {
            var profilesJson = JsonSerializer.Serialize(_profiles);
            _config.SetConfig("Profiles", profilesJson);

            if (_selectedProfileId != null)
            {
                _config.SetConfig("SelectedProfileId", _selectedProfileId);
            }

            ProfilesChanged?.Invoke();
            _logger.LogDebug("档案已保存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存档案失败");
        }
    }

    public IReadOnlyList<UserProfile> GetProfiles() => _profiles.AsReadOnly();

    public UserProfile? GetSelectedProfile()
    {
        return _profiles.FirstOrDefault(p => p.Id == _selectedProfileId);
    }

    public void SetSelectedProfile(string profileId)
    {
        if (_profiles.Any(p => p.Id == profileId))
        {
            _selectedProfileId = profileId;
            _config.SetConfig("SelectedProfileId", profileId);
            ProfilesChanged?.Invoke();
            _logger.LogInformation("已选择档案: {ProfileId}", profileId);
        }
        else
        {
            _logger.LogWarning("档案不存在: {ProfileId}", profileId);
        }
    }

    public void AddProfile(UserProfile profile)
    {
        if (_profiles.Any(p => p.Id == profile.Id))
        {
            _logger.LogWarning("档案已存在: {ProfileId}", profile.Id);
            return;
        }

        _profiles.Add(profile);
        SaveProfiles();
        _logger.LogInformation("已添加档案: {Username} ({Provider})", profile.Username, profile.Provider);
    }

    public void RemoveProfile(string profileId)
    {
        var profile = _profiles.FirstOrDefault(p => p.Id == profileId);
        if (profile == null)
        {
            _logger.LogWarning("档案不存在: {ProfileId}", profileId);
            return;
        }

        _profiles.Remove(profile);

        if (_selectedProfileId == profileId)
        {
            _selectedProfileId = _profiles.FirstOrDefault()?.Id;
        }

        SaveProfiles();
        _logger.LogInformation("已移除档案: {Username}", profile.Username);
    }

    public bool HasVerifiedAccount()
    {
        return _profiles.Any(p => p.Provider == AuthProvider.Microsoft || p.Provider == AuthProvider.Yggdrasil || p.Provider == AuthProvider.AuthLib);
    }

    public bool CanCreateOfflineProfile()
    {
        if (HasVerifiedAccount())
        {
            return true;
        }

        if (_isRestrictedFeatureAllowed && _profiles.Count > 0)
        {
            return true;
        }

        if (!_hasNetwork)
        {
            return true;
        }

        return false;
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

                        AddOrUpdateProfile(user);
                        SetSelectedProfile(user.Id);

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

            if (!CanCreateOfflineProfile())
            {
                _logger.LogWarning("离线登录功能受限，需要先验证一个账号");
                return AuthResult.Failed("离线登录功能受限，需要先验证一个账号");
            }

            var offlineId = GenerateOfflineUuid(username);
            _currentUser = new UserProfile
            {
                Id = offlineId,
                Username = username,
                DisplayName = username,
                Provider = AuthProvider.Offline
            };

            _currentToken = new AuthToken
            {
                AccessToken = "offline",
                ExpiresAt = DateTime.MaxValue
            };

            AddOrUpdateProfile(_currentUser);
            SetSelectedProfile(offlineId);

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
                        Provider = AuthProvider.Yggdrasil,
                        Properties = new Dictionary<string, string>
                        {
                            { "Server", server }
                        }
                    };

                    _currentToken = new AuthToken
                    {
                        AccessToken = result.AccessToken,
                        ExpiresAt = DateTimeOffset.FromUnixTimeMilliseconds(result.ExpiresAt).DateTime
                    };

                    AddOrUpdateProfile(_currentUser);
                    SetSelectedProfile(_currentUser.Id);

                    SetState(AuthState.LoggedIn);
                    _logger.LogInformation("第三方登录成功: {Username}", _currentUser.Username);
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
        if (_currentToken?.RefreshToken == null)
        {
            _logger.LogWarning("刷新令牌失败：无可用刷新令牌");
            return null;
        }

        if (_currentUser == null)
        {
            _logger.LogWarning("刷新令牌失败：无当前用户");
            return null;
        }

        try
        {
            if (_currentUser.Provider == AuthProvider.Microsoft)
            {
                return await RefreshMicrosoftTokenAsync();
            }
            else if (_currentUser.Provider == AuthProvider.Yggdrasil || _currentUser.Provider == AuthProvider.AuthLib)
            {
                return await RefreshYggdrasilTokenAsync();
            }
            else
            {
                _logger.LogWarning("刷新令牌失败：离线账号不支持刷新");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新令牌时发生异常");
            ClearToken();
            return null;
        }
    }

    private async Task<AuthToken?> RefreshMicrosoftTokenAsync()
    {
        _logger.LogInformation("刷新微软令牌");

        var bodyData = new Dictionary<string, string>
        {
            { "client_id", "000000004C12AE6F" },
            { "grant_type", "refresh_token" },
            { "refresh_token", _currentToken!.RefreshToken! }
        };

        var response = await _network.PostAsync(
            "https://login.live.com/oauth20_token.srf",
            string.Join("&", bodyData.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}")),
            new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } });

        if (!response.IsSuccess)
        {
            _logger.LogWarning("微软令牌刷新失败: {StatusCode}", response.StatusCode);
            ClearToken();
            return null;
        }

        var tokenResponse = JsonSerializer.Deserialize<MicrosoftRefreshResponse>(response.BodyAsString);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
        {
            _logger.LogWarning("微软令牌刷新响应无效");
            ClearToken();
            return null;
        }

        var newExpiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.expires_in + DateTimeOffset.Now.ToUnixTimeSeconds()).DateTime;
        _currentToken = new AuthToken
        {
            AccessToken = tokenResponse.access_token,
            RefreshToken = tokenResponse.refresh_token ?? _currentToken.RefreshToken,
            ExpiresAt = newExpiresAt,
            Xuid = _currentToken.Xuid
        };

        SaveMicrosoftToken(tokenResponse);
        _logger.LogInformation("微软令牌刷新成功，新过期时间: {ExpiresAt}", newExpiresAt);
        return _currentToken;
    }

    private async Task<AuthToken?> RefreshYggdrasilTokenAsync()
    {
        var server = _currentUser?.Properties.GetValueOrDefault("Server");
        if (string.IsNullOrEmpty(server))
        {
            _logger.LogWarning("Yggdrasil令牌刷新失败：未找到服务器地址");
            return null;
        }

        _logger.LogInformation("刷新Yggdrasil令牌: {Server}", server);

        var bodyData = new Dictionary<string, string>
        {
            { "clientToken", Guid.NewGuid().ToString() },
            { "accessToken", _currentToken!.AccessToken }
        };

        var response = await _network.PostAsync(
            $"{server}/authserver/refresh",
            JsonSerializer.Serialize(bodyData));

        if (!response.IsSuccess)
        {
            if (response.StatusCode == 403 || response.StatusCode == 401)
            {
                _logger.LogWarning("Yggdrasil令牌刷新失败，令牌已失效: {StatusCode}", response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Yggdrasil令牌刷新失败: {StatusCode}", response.StatusCode);
            }
            ClearToken();
            return null;
        }

        var refreshResponse = JsonSerializer.Deserialize<YggdrasilRefreshResponse>(response.BodyAsString);
        if (refreshResponse == null || string.IsNullOrEmpty(refreshResponse.AccessToken))
        {
            _logger.LogWarning("Yggdrasil令牌刷新响应无效");
            ClearToken();
            return null;
        }

        var newExpiresAt = refreshResponse.ExpiresAt > 0
            ? DateTimeOffset.FromUnixTimeMilliseconds(refreshResponse.ExpiresAt).DateTime
            : DateTime.UtcNow.AddDays(30);

        _currentToken = new AuthToken
        {
            AccessToken = refreshResponse.AccessToken,
            RefreshToken = refreshResponse.AccessToken,
            ExpiresAt = newExpiresAt,
            Xuid = _currentToken.Xuid
        };

        _logger.LogInformation("Yggdrasil令牌刷新成功，新过期时间: {ExpiresAt}", newExpiresAt);
        return _currentToken;
    }

    private void ClearToken()
    {
        _currentToken = null;
        _config.SetConfig("LoginMsJson", "{}");
    }

    private void SaveMicrosoftToken(MicrosoftRefreshResponse tokenResponse)
    {
        var token = new MicrosoftToken
        {
            access_token = tokenResponse.access_token,
            refresh_token = tokenResponse.refresh_token ?? _currentToken?.RefreshToken ?? "",
            expires_at = tokenResponse.expires_in + DateTimeOffset.Now.ToUnixTimeSeconds(),
            xuid = _currentToken?.Xuid ?? ""
        };
        _config.SetConfig("LoginMsJson", JsonSerializer.Serialize(token));
    }

    private class MicrosoftRefreshResponse
    {
        public string access_token { get; set; } = "";
        public string refresh_token { get; set; } = "";
        public int expires_in { get; set; }
        public string token_type { get; set; } = "";
    }

    private class YggdrasilRefreshResponse
    {
        public string AccessToken { get; set; } = "";
        public long ExpiresAt { get; set; }
    }

    private void AddOrUpdateProfile(UserProfile profile)
    {
        var existingIndex = _profiles.FindIndex(p => p.Id == profile.Id);
        if (existingIndex >= 0)
        {
            _profiles[existingIndex] = profile;
            _logger.LogDebug("更新档案: {Username}", profile.Username);
        }
        else
        {
            _profiles.Add(profile);
            _logger.LogDebug("新增档案: {Username}", profile.Username);
        }
        SaveProfiles();
    }

    private string GenerateOfflineUuid(string username)
    {
        using var sha = System.Security.Cryptography.SHA1.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("OfflinePlayer:" + username));
        var uuidBytes = new byte[16];
        Array.Copy(hash, uuidBytes, 16);
        uuidBytes[6] = (byte)((uuidBytes[6] & 0x0f) | 0x30);
        uuidBytes[8] = (byte)((uuidBytes[8] & 0x3f) | 0x80);

        var uuid = new System.Text.StringBuilder();
        for (int i = 0; i < 16; i++)
        {
            if (i == 4 || i == 6 || i == 8 || i == 10)
                uuid.Append('-');
            uuid.AppendFormat("{0:x2}", uuidBytes[i]);
        }
        return uuid.ToString();
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

            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {mcData.AccessToken}" }
            };

            var profileResponse = await _network.GetAsync(
                "https://api.minecraftservices.com/minecraft/profile",
                headers);

            if (profileResponse.StartsWith("Error") || string.IsNullOrEmpty(profileResponse))
            {
                _logger.LogWarning("获取玩家资料失败");
                return null;
            }

            var profile = JsonSerializer.Deserialize<MinecraftProfile>(profileResponse);
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
                AvatarUrl = profile.Skins?.FirstOrDefault()?.Url,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                Properties = new Dictionary<string, string>()
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
