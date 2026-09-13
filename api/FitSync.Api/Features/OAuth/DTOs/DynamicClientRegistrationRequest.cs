namespace FitSync.Api.Features.OAuth.DTOs;

using System.Text.Json.Serialization;

public record DynamicClientRegistrationRequest(
    [property: JsonPropertyName("client_name")] string ClientName,
    [property: JsonPropertyName("redirect_uris")] string[] RedirectUris,
    [property: JsonPropertyName("grant_types")] string[]? GrantTypes,
    [property: JsonPropertyName("response_types")] string[]? ResponseTypes,
    [property: JsonPropertyName("token_endpoint_auth_method")] string? TokenEndpointAuthMethod
);
