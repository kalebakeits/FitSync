namespace FitSync.Api.Features.Tokens.Services;

using FitSync.Api.Features.Tokens.DTOs;

public interface IApiTokenService
{
    Task<List<ApiTokenResponse>> GetTokensAsync(Guid userId);
    Task<CreateApiTokenResponse> CreateTokenAsync(Guid userId, string name);
    Task RevokeTokenAsync(Guid userId, Guid tokenId);
    Task<Guid?> ValidateTokenAsync(string rawToken);
}
