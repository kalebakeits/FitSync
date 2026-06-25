namespace FitSync.Api.Features.OAuth.DTOs;

using Microsoft.AspNetCore.Mvc;

public record OAuthAuthorizeRequest(
    [FromQuery(Name = "client_id")] string ClientId,
    [FromQuery(Name = "redirect_uri")] string RedirectUri,
    [FromQuery(Name = "response_type")] string ResponseType,
    [FromQuery(Name = "state")] string? State
);
