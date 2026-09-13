namespace FitSync.Api.Features.OAuth.DTOs;

using System.Text.Json.Serialization;

public record OAuthTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType
);
