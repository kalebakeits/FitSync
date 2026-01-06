namespace FitSync.Api.Features.Fetchers.Services;

using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class FetchersService(FitSyncDbContext context, ILogger<FetchersService> logger)
    : IFetchersService
{
    private readonly FitSyncDbContext context = context;
    private readonly ILogger<FetchersService> logger = logger;

    public async Task TriggerFetchAsync(Guid userId)
    {
        this.logger.LogInformation(
            "Triggering manual fetch for user: {UserId}",
            userId
        );

        ZwiftFetcherConfig? config = await this.context.ZwiftFetcherConfigs.FirstOrDefaultAsync(
            c => c.UserId == userId
        );

        if (config == null)
        {
            this.logger.LogWarning(
                "ZwiftFetcherConfig not found for user: {UserId} - user may not have Zwift credentials",
                userId
            );
            return;
        }

        DateTime now = DateTime.UtcNow;
        config.NextFetchTime = now;
        config.UpdatedAt = now;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Manual fetch triggered successfully for user: {UserId} - NextFetchTime set to {NextFetchTime}",
            userId,
            now
        );
    }
}

