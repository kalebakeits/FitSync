namespace FitSync.Wahoo.Shared.WahooClient.DTOs;

using System.Text.Json.Serialization;

public record WahooTokenRefreshResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
