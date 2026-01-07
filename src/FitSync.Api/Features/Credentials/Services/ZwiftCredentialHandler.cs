using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FitSync.Api.Features.Credentials.Services;

public class ZwiftCredentialHandler(
    FitSyncDbContext context,
    ILogger<ZwiftCredentialHandler> logger
) : IServiceCredentialHandler
{
    private readonly FitSyncDbContext context = context;
    private readonly ILogger<ZwiftCredentialHandler> logger = logger;

    public string ServiceType => ServiceTypes.Zwift;

    public async Task OnCredentialCreatedAsync(Guid userId)
    {
        this.logger.LogInformation("Creating ZwiftFetcherConfig for user: {UserId}", userId);

        ZwiftFetcherConfig? existing = await this.context.ZwiftFetcherConfigs.FirstOrDefaultAsync(
            c => c.UserId == userId
        );

        if (existing != null)
        {
            this.logger.LogInformation(
                "ZwiftFetcherConfig already exists for user: {UserId}",
                userId
            );
            return;
        }

        ZwiftFetcherConfig config =
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FetchIntervalMinutes = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        this.context.ZwiftFetcherConfigs.Add(config);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "ZwiftFetcherConfig created successfully for user: {UserId}",
            userId
        );
    }

    public async Task OnCredentialDeletedAsync(Guid userId)
    {
        this.logger.LogInformation("Deleting ZwiftFetcherConfig for user: {UserId}", userId);

        ZwiftFetcherConfig? config = await this.context.ZwiftFetcherConfigs.FirstOrDefaultAsync(
            c => c.UserId == userId
        );

        if (config == null)
        {
            this.logger.LogWarning(
                "ZwiftFetcherConfig not found for deletion - user: {UserId}",
                userId
            );
            return;
        }

        this.context.ZwiftFetcherConfigs.Remove(config);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "ZwiftFetcherConfig deleted successfully for user: {UserId}",
            userId
        );
    }
}
