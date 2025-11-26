namespace FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;

using System.Text.Json.Serialization;

public record AuthTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);
