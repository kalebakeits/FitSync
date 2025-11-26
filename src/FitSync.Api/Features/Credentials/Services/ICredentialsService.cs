namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Api.Features.Credentials.DTOs;

public interface ICredentialsService
{
    Task<CredentialResponse> CreateOrUpdateCredentialAsync(
        Guid userId,
        CreateCredentialRequest request
    );
    Task<List<CredentialResponse>> GetCredentialsAsync(Guid userId);
    Task DeleteCredentialAsync(Guid userId, string serviceType);
    Task<List<string>> GetAvailableServicesAsync(Guid userId);
}
