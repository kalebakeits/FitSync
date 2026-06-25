namespace FitSync.Api.Features.OAuth.Services;

using FitSync.Api.Features.OAuth.DTOs;

public interface IOAuthAuthorizationService
{
    Task<OAuthConsentInfo> ValidateAuthorizeRequestAsync(
        string clientId,
        string redirectUri,
        string responseType,
        CancellationToken cancellationToken
    );

    Task<string> IssueCodeAsync(
        string clientId,
        Guid userId,
        string redirectUri,
        CancellationToken cancellationToken
    );

    Task<string> ExchangeCodeAsync(
        string clientId,
        string clientSecret,
        string code,
        string redirectUri,
        CancellationToken cancellationToken
    );
}
