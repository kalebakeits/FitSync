namespace FitSync.Api.Features.OAuth.DTOs;

public record OAuthConsentInfo(string ClientName, string RedirectUri, string? State);
