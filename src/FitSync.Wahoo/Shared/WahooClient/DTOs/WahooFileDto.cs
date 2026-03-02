namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooFileDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
