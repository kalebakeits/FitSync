namespace FitSync.Api.Features.Wahoo.DTOs;

using System.Text.Json.Serialization;

public sealed class WahooTokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
