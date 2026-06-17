using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Models.Homepage.News;

public class ApiResponse
{
    [JsonPropertyName("result")]
    public required Result Result { get; set; }
}