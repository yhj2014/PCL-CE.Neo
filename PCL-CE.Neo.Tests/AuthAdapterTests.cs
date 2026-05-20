using Xunit;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Tests;

public class AuthAdapterTests
{
    [Fact]
    public void AuthResult_Success()
    {
        var user = new UserProfile
        {
            Id = "user-123",
            Username = "TestUser",
            Provider = AuthProvider.Offline
        };

        var token = new AuthToken
        {
            AccessToken = "test-token",
            ExpiresAt = DateTime.MaxValue
        };

        var result = AuthResult.Succeeded(user, token);

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Token);
        Assert.Equal("TestUser", result.User.Username);
    }

    [Fact]
    public void AuthResult_Failure()
    {
        var result = AuthResult.Failed("Invalid credentials");

        Assert.False(result.Success);
        Assert.Equal("Invalid credentials", result.ErrorMessage);
    }

    [Fact]
    public void AuthResult_FailureWithException()
    {
        var exception = new InvalidOperationException("Token expired");
        var result = AuthResult.Failed("Token expired", exception);

        Assert.False(result.Success);
        Assert.Equal("Token expired", result.ErrorMessage);
        Assert.NotNull(result.Exception);
        Assert.IsType<InvalidOperationException>(result.Exception);
    }

    [Fact]
    public void UserProfile_Properties()
    {
        var user = new UserProfile
        {
            Id = "ms-123",
            Username = "MicrosoftUser",
            DisplayName = "Microsoft User",
            Provider = AuthProvider.Microsoft,
            AvatarUrl = "https://example.com/avatar.png",
            SkinUrl = "https://example.com/skin.png",
            CapeUrl = "https://example.com/cape.png"
        };

        Assert.Equal("ms-123", user.Id);
        Assert.Equal(AuthProvider.Microsoft, user.Provider);
        Assert.Equal("Microsoft User", user.DisplayName);
        Assert.Equal("https://example.com/skin.png", user.SkinUrl);
        Assert.Equal("https://example.com/cape.png", user.CapeUrl);
    }

    [Fact]
    public void UserProfile_YggdrasilProvider()
    {
        var user = new UserProfile
        {
            Id = "ygg-123",
            Username = "YggdrasilUser",
            Provider = AuthProvider.Yggdrasil
        };

        Assert.Equal(AuthProvider.Yggdrasil, user.Provider);
        Assert.Equal("YggdrasilUser", user.Username);
    }

    [Fact]
    public void UserProfile_OfflineProvider()
    {
        var user = new UserProfile
        {
            Id = Guid.NewGuid().ToString(),
            Username = "OfflinePlayer",
            Provider = AuthProvider.Offline
        };

        Assert.Equal(AuthProvider.Offline, user.Provider);
        Assert.NotEmpty(user.Id);
    }

    [Fact]
    public void AuthToken_Expiration()
    {
        var token = new AuthToken
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Xuid = "123456789"
        };

        Assert.True(token.ExpiresAt > DateTime.UtcNow);
        Assert.Equal("123456789", token.Xuid);
        Assert.NotEmpty(token.AccessToken);
        Assert.NotEmpty(token.RefreshToken);
    }

    [Fact]
    public void AuthToken_IsExpired()
    {
        var token = new AuthToken
        {
            AccessToken = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        };

        Assert.True(token.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public void AuthToken_NoExpiration()
    {
        var token = new AuthToken
        {
            AccessToken = "offline-token",
            ExpiresAt = DateTime.MaxValue
        };

        Assert.Equal(DateTime.MaxValue, token.ExpiresAt);
    }

    [Fact]
    public void AuthState_Transitions()
    {
        var states = new List<AuthState>
        {
            AuthState.LoggedOut,
            AuthState.LoggingIn,
            AuthState.LoggedIn,
            AuthState.LoggingOut,
            AuthState.Error
        };

        foreach (var state in states)
        {
            Assert.True(Enum.IsDefined(typeof(AuthState), state));
        }
    }

    [Fact]
    public void AuthProvider_AllTypes()
    {
        var providers = new List<AuthProvider>
        {
            AuthProvider.Microsoft,
            AuthProvider.Yggdrasil,
            AuthProvider.Offline
        };

        Assert.Equal(3, providers.Count);
        Assert.Contains(AuthProvider.Microsoft, Enum.GetValues<AuthProvider>());
        Assert.Contains(AuthProvider.Yggdrasil, Enum.GetValues<AuthProvider>());
        Assert.Contains(AuthProvider.Offline, Enum.GetValues<AuthProvider>());
    }

    [Fact]
    public void AuthResult_Equality()
    {
        var user = new UserProfile { Id = "1", Username = "Test" };
        var token = new AuthToken { AccessToken = "t1", ExpiresAt = DateTime.MaxValue };
        var result1 = AuthResult.Succeeded(user, token);
        var result2 = AuthResult.Succeeded(user, token);

        Assert.Equal(result1.Success, result2.Success);
        Assert.Equal(result1.ErrorMessage, result2.ErrorMessage);
    }

    [Fact]
    public void UserProfile_DefaultValues()
    {
        var user = new UserProfile();

        Assert.Equal(AuthProvider.Offline, user.Provider);
        Assert.Null(user.AvatarUrl);
        Assert.Null(user.SkinUrl);
        Assert.Null(user.CapeUrl);
    }
}
