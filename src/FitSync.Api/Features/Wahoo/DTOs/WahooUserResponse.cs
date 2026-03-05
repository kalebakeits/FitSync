namespace FitSync.Api.Features.Wahoo.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooUserResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}
