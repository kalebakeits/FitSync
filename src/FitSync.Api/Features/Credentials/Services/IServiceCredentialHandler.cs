namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Database.Models;

public interface IServiceCredentialHandler
{
    string ServiceType { get; }
    bool IsFetcher { get; }
    bool IsUploader { get; }
    string? GetDisplayName(Integration integration);
    object BuildAuthData(CreateCredentialRequest request);
    Task OnCredentialCreatedAsync(Integration integration, CancellationToken cancellationToken = default);
    Task OnCredentialDeletedAsync(Integration integration, CancellationToken cancellationToken = default);
}
