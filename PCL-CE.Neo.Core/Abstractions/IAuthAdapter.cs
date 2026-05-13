namespace PCL_CE.Neo.Core.Abstractions;

public interface IAuthAdapter
{
    event Action<AuthState>? StateChanged;

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
}

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

public record UserProfile
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public AuthProvider Provider { get; init; }
    public Dictionary<string, string> Properties { get; init; } = new();
}

public record AuthToken
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public string? TokenType { get; init; }
    public string? Xuid { get; init; }
}
