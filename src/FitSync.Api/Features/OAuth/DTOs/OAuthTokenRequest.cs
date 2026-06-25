namespace FitSync.Api.Features.OAuth.DTOs;

using Microsoft.AspNetCore.Mvc;

public record OAuthTokenRequest(
    [FromForm(Name = "grant_type")] string GrantType,
    [FromForm(Name = "code")] string Code,
    [FromForm(Name = "redirect_uri")] string RedirectUri,
    [FromForm(Name = "client_id")] string ClientId,
    [FromForm(Name = "client_secret")] string ClientSecret
);
