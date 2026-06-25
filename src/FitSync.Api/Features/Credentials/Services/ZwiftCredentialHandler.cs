namespace FitSync.Api.Features.Credentials.Services;

using FitSync.Api.Features.Credentials.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Encryption.Extensions;
using FitSync.Shared.Features.Encryption.Services;
using FitSync.Zwift.Shared.AuthData;
using Microsoft.EntityFrameworkCore;

public class ZwiftCredentialHandler(
    FitSyncDbContext context,
    IEncryptionService encryptionService,
    ILogger<ZwiftCredentialHandler> logger
) : IServiceCredentialHandler
{
    private readonly FitSyncDbContext context = context;
    private readonly IEncryptionService encryptionService = encryptionService;
    private readonly ILogger<ZwiftCredentialHandler> logger = logger;

    public string ServiceType => ServiceTypes.Zwift;
    public Database.Enums.ServiceType? HeartbeatServiceType =>
        Database.Enums.ServiceType.ZwiftFetcher;
    public bool IsFetcher => true;
    public bool IsUploader => false;
    public string AuthType => "credentials";
    public string? ConnectUrl => null;

    public object BuildAuthData(CreateCredentialRequest request) =>
        new ZwiftAuthData { Username = request.Username, Password = request.Password };

    public string GetDisplayName(Integration integration) =>
        integration.GetAuthData<ZwiftAuthData>(this.encryptionService).Username;

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
            "Created FetcherConfig for Zwift integration {Id}.",
            integration.Id
        );
    }

    public async Task OnCredentialDeletedAsync(
        Integration integration,
        CancellationToken cancellationToken = default
    )
    {
        FetcherConfig? config = await this.context.FetcherConfigs.FirstOrDefaultAsync(
            f => f.IntegrationId == integration.Id,
            cancellationToken
        );

        if (config == null)
            return;

        this.context.FetcherConfigs.Remove(config);
        await this.context.SaveChangesAsync(cancellationToken);
        this.logger.LogInformation(
            "Deleted FetcherConfig for Zwift integration {Id}.",
            integration.Id
        );
    }
}
