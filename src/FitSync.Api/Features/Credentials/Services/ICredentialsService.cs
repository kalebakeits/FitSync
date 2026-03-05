namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Api.Features.Credentials.DTOs;

public interface ICredentialsService
{
    Task<CredentialResponse> CreateOrUpdateCredentialAsync(
        Guid userId,
        CreateCredentialRequest request,
        CancellationToken cancellationToken = default
    );

    Task<List<CredentialResponse>> GetCredentialsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task DeleteCredentialAsync(
        Guid userId,
        string serviceType,
        CancellationToken cancellationToken = default
    );

    Task<List<AvailableServiceResponse>> GetAvailableServicesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    List<AvailableServiceResponse> GetAllServices();
}
