namespace FitSync.Api.Features.Credentials.Services;

public interface IServiceCredentialHandler
{
    string ServiceType { get; }
    Task OnCredentialCreatedAsync(Guid userId);
    Task OnCredentialDeletedAsync(Guid userId);
}
