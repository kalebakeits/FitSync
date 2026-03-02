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
        List<FetcherConfig> configs = await this.context.FetcherConfigs
            .Include(f => f.Integration)
            .Where(f => f.Integration.UserId == userId)
            .ToListAsync();

        if (configs.Count == 0)
        {
            this.logger.LogWarning("No FetcherConfigs found for user {UserId}.", userId);
            return;
        }

        DateTime now = DateTime.UtcNow;
        foreach (FetcherConfig config in configs)
        {
            config.NextFetchTime = now;
            config.UpdatedAt = now;
        }

        await this.context.SaveChangesAsync();
        this.logger.LogInformation(
            "Manual fetch triggered for user {UserId} across {Count} fetcher(s).",
            userId,
            configs.Count
        );
    }
}
