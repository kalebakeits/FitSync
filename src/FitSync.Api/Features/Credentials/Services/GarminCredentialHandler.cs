namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;

public class GarminCredentialHandler(
    IEncryptionService encryptionService,
    ILogger<GarminCredentialHandler> logger
) : IServiceCredentialHandler
{
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<GarminCredentialHandler> logger = logger;

    public string ServiceType => ServiceTypes.Garmin;
    public string AuthType => "credentials";
    public string? ConnectUrl => null;

    public object BuildAuthData(CreateCredentialRequest request) =>
        new GarminAuthData { Username = request.Username, Password = request.Password };

    public string GetDisplayName(Integration integration) =>
        integration.GetAuthData<GarminAuthData>(this.encryptionService).Username;

    public Task OnCredentialCreatedAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation("Garmin integration {Id} created.", integration.Id);
        return Task.CompletedTask;
    }

    public Task OnCredentialDeletedAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation("Garmin integration {Id} deleted.", integration.Id);
        return Task.CompletedTask;
    }
}
