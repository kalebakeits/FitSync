namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Garmin.Shared.AuthData;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using Microsoft.EntityFrameworkCore;

public class GarminCredentialHandler(
    FitSyncDbContext context,
    IEncryptionService encryptionService,
    ILogger<GarminCredentialHandler> logger
) : IServiceCredentialHandler
{
    private readonly FitSyncDbContext context = context;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<GarminCredentialHandler> logger = logger;

    public string ServiceType => ServiceTypes.Garmin;
    public Database.Enums.ServiceType? HeartbeatServiceType =>
        Database.Enums.ServiceType.GarminFetcher;
    public bool IsFetcher => true;
    public bool IsUploader => true;
    public bool SupportsWorkoutPublishing => true;
    public string AuthType => "credentials";
    public string? ConnectUrl => null;

    public object BuildAuthData(CreateCredentialRequest request) =>
        new GarminAuthData { Username = request.Username, Password = request.Password };

    public string GetDisplayName(Integration integration) =>
        integration.GetAuthData<GarminAuthData>(this.encryptionService).Username;

    public async Task OnCredentialCreatedAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        bool exists = await this.context.FetcherConfigs.AnyAsync(
            f => f.IntegrationId == integration.Id,
            cancellationToken
        );

        if (exists)
            return;

        this.context.FetcherConfigs.Add(
            new FetcherConfig
            {
                Id = Guid.NewGuid(),
                IntegrationId = integration.Id,
                FetchIntervalMinutes = 10,
            }
        );

        await this.context.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation(
            "Created FetcherConfig for Garmin integration {Id}.",
            integration.Id
        );
    }

    public async Task OnCredentialDeletedAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        int deleted = await this.context.FetcherConfigs.Where(
            f => f.IntegrationId == integration.Id
        )
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
        {
            this.logger.LogWarning(
                "No FetcherConfig found for Garmin integration {Id}.",
                integration.Id
            );
            return;
        }

        this.logger.LogInformation(
            "Deleted FetcherConfig for Garmin integration {Id}.",
            integration.Id
        );
    }
}
