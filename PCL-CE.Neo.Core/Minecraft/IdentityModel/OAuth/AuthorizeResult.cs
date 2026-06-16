using System.Text.Json.Serialization;
using PCL_CE.Neo.Core.Utils.Exts;

namespace PCL_CE.Neo.Core.Minecraft.IdentityModel.OAuth;

public record AuthorizeResult
{
    public bool IsError => !Error.IsNullOrEmpty();
    
    [JsonPropertyName("error")] public string? Error { get; init; }
    [JsonPropertyName("error_description")] public string? ErrorDescription { get; init; }
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
    [JsonPropertyName("id_token")] public string? IdToken { get; init; }
    [JsonPropertyName("token_type")] public string? TokenType { get; init; }
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; init; }
}