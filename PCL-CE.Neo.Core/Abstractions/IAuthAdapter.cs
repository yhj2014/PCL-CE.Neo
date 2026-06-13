namespace PCL_CE.Neo.Core.Abstractions;

public enum AuthState
{
    LoggedOut,
    LoggingIn,
    LoggedIn,
    LoggingOut,
    Error
}

public enum AuthProvider
{
    Microsoft,
    Offline,
    Yggdrasil,
    AuthLib
}

public record AuthToken
{
    public string AccessToken { get; init; } = "";
    public string? RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string? TokenType { get; init; }
    public string? Xuid { get; init; }
}

public record UserProfile
{
    public string Id { get; init; } = "";
    public string Username { get; init; } = "";
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? SkinUrl { get; init; }
    public string? CapeUrl { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public AuthProvider Provider { get; init; } = AuthProvider.Offline;
    public Dictionary<string, string> Properties { get; init; } = new();
    public bool IsSelected { get; init; }
}

public record AuthResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public UserProfile? User { get; init; }
    public AuthToken? Token { get; init; }
    public Exception? Exception { get; init; }

    public static AuthResult Succeeded(UserProfile? user, AuthToken? token = null) =>
        new() { Success = true, User = user, Token = token };
    public static AuthResult Failed(string message, Exception? ex = null) =>
        new() { Success = false, ErrorMessage = message, Exception = ex };
}

public interface IAuthAdapter
{
    event Action<AuthState>? StateChanged;
    event Action? ProfilesChanged;

    AuthState CurrentState { get; }
    UserProfile? CurrentUser { get; }

    Task<AuthResult> LoginMicrosoftAsync();
    Task<AuthResult> LoginOfflineAsync(string username);
    Task<AuthResult> LoginThirdPartyAsync(string server, string username, string password);
    Task LogoutAsync();

    Task<UserProfile?> GetCurrentUserAsync();
    Task<AuthToken?> RefreshTokenAsync();

    bool IsLoggedIn { get; }
    string? AccessToken { get; }

    IReadOnlyList<UserProfile> GetProfiles();
    UserProfile? GetSelectedProfile();
    void SetSelectedProfile(string profileId);
    void AddProfile(UserProfile profile);
    void RemoveProfile(string profileId);
    bool HasVerifiedAccount();
    bool CanCreateOfflineProfile();
}
