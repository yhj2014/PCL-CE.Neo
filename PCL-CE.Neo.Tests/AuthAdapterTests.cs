using Xunit;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests;

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
    public void UserProfile_Properties()
    {
        var user = new UserProfile
        {
            Id = "ms-123",
            Username = "MicrosoftUser",
            DisplayName = "Microsoft User",
            Provider = AuthProvider.Microsoft,
            AvatarUrl = "https://example.com/avatar.png"
        };

        Assert.Equal("ms-123", user.Id);
        Assert.Equal(AuthProvider.Microsoft, user.Provider);
        Assert.NotNull(user.Properties);
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
    }
}
