using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Models.Homepage.News;

public class Result
{
    [JsonPropertyName("results")]
    public required List<NewsItem> Results { get; set; }

    [JsonPropertyName("numFound")]
    public int NumFound { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }
}