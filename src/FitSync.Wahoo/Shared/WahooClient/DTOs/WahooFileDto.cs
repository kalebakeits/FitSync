namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public record WahooFileDto([property: JsonPropertyName("url")] string? Url);
