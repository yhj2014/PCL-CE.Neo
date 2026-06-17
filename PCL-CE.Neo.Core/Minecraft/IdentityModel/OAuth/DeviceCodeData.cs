using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;

public record DeviceCodeData
{
    public bool IsError => !string.IsNullOrEmpty(Error);

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }

    [JsonPropertyName("user_code")]
    public string? UserCode { get; init; }

    [JsonPropertyName("device_code")]
    public string? DeviceCode { get; init; }

    [JsonPropertyName("verification_uri")]
    public string? VerificationUri { get; init; }

    [JsonPropertyName("verification_uri_complete")]
    public string? VerificationUriComplete { get; init; }

    [JsonPropertyName("interval")]
    public int? Interval { get; init; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }
}