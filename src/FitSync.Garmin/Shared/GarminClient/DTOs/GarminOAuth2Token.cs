namespace FitSync.Garmin.Shared.GarminClient.DTOs;

using System.Text.Json.Serialization;

public record GarminOAuth2Token(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
