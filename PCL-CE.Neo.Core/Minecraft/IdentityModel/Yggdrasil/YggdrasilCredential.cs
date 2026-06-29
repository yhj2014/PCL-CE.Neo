using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.Yggdrasil;

public record YggdrasilCredential
{
    [JsonPropertyName("username")] public required string User { get; init; }
    [JsonPropertyName("password")] public required string Password { get; init; }
    [JsonPropertyName("agent")] public Agent Agent = new();
    [JsonPropertyName("requestUser")] public bool RequestUser { get; set; }
}

public record YggdrasilAuthenticateResult
{
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; init; }
    [JsonPropertyName("accessToken")] public string? AccessToken { get; init; }
    [JsonPropertyName("clientToken")] public string? ClientToken { get; init; }
    [JsonPropertyName("selectedProfile")] public Profile? SelectedProfile { get; init; }
    [JsonPropertyName("availableProfiles")] public Profile[]? AvailableProfiles { get; init; }
    [JsonPropertyName("user")] public Profile? User;
}

public record YggdrasilRefresh
{
    [JsonPropertyName("accessToken")] public required string AccessToken { get; set; }
    [JsonPropertyName("selectedProfile")] public Profile? SelectedProfile { get; set; }
}