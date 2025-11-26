namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class BackpressureMonitor(
    FitSyncDbContext dbContext,
    ILogger<BackpressureMonitor> logger,
    IOptions<FetcherOptions> options
) : IBackpressureMonitor
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ILogger<BackpressureMonitor> logger = logger;
    private readonly IOptions<FetcherOptions> options = options;

    public async Task<bool> ShouldFetchAsync(CancellationToken cancellationToken = default)
    {
        TimeSpan uploaderDeadThreshold = TimeSpan.FromMinutes(
            this.options.Value.DeadThresholdMinutes
        );
        DateTime cutoff = DateTime.UtcNow - uploaderDeadThreshold;

        bool aliveUploaders = await this.dbContext.ServiceHeartbeats.Where(
            h => h.ServiceType == ServiceType.GarminUploader
        )
            .Where(h => h.LastHeartbeatAt > cutoff)
            .AnyAsync(cancellationToken);

        if (!aliveUploaders)
        {
            this.logger.LogWarning("No alive uploaders detected. Pausing fetch.");
            return false;
        }

        int pendingCount = await this.dbContext.Activities.CountAsync(
            a => a.Status == ActivityStatus.Pending,
            cancellationToken
        );

        if (pendingCount > this.options.Value.MaxPendingActivities)
        {
            this.logger.LogWarning(
                "Pending queue too large ({Count}). Pausing fetch.",
                pendingCount
            );
            return false;
        }

        return true;
    }
}
