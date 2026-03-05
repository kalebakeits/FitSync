namespace FitSync.Garmin.Uploader.Features.OrphanedWork.Services;

using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Garmin.Uploader.Configuration;
using FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class OrphanedActivityReclaimer(
    FitSyncDbContext dbContext,
    IOptions<GarminUploaderOptions> options,
    ILogger<OrphanedActivityReclaimer> logger,
    IActivityProcessor activityProcessor
) : IOrphanedActivityReclaimer
{
    private readonly FitSyncDbContext fitSyncDbContext = dbContext;
    private readonly IOptions<GarminUploaderOptions> options = options;
    private readonly ILogger<OrphanedActivityReclaimer> logger = logger;
    private readonly IActivityProcessor activityProcessor = activityProcessor;
    private const int orphanProcessingBatchSize = 5;

    public async Task ReclaimOrphanedActivitiesAsync(CancellationToken cancellationToken)
    {
        TimeSpan orphanThreshold = TimeSpan.FromMinutes(this.options.Value.OrphanThresholdMinutes);
        DateTime cutoff = DateTime.UtcNow - orphanThreshold;
        HashSet<ActivityStatus> incompleteStatuses =
        [
            ActivityStatus.Pending,
            ActivityStatus.Processing,
            ActivityStatus.ServiceUnavailable,
            ActivityStatus.Claimed,
        ];

        List<Guid> orphanedActivityIds = await this.fitSyncDbContext.ActivityUploadStatuses
            .Where(u => u.DestinationServiceType == ServiceTypes.Garmin)
            .Where(u => incompleteStatuses.Contains(u.Status))
            .Where(u => u.ClaimedBy != null && u.ClaimedAt < cutoff)
            .Take(orphanProcessingBatchSize)
            .Select(u => u.ActivityId)
            .ToListAsync(cancellationToken);

        if (orphanedActivityIds.Count == 0)
        {
            this.logger.LogDebug("There are no orphans to adopt. Back you go!");
            return;
        }

        this.logger.LogWarning(
            "Adopting {Count} orphans. Ids: {@ActivityIds}",
            orphanedActivityIds.Count,
            orphanedActivityIds
        );

        await this.fitSyncDbContext.ActivityUploadStatuses
            .Where(u =>
                orphanedActivityIds.Contains(u.ActivityId)
                && u.DestinationServiceType == ServiceTypes.Garmin
            )
            .ExecuteUpdateAsync(u =>
                u.SetProperty(x => x.Status, ActivityStatus.Pending)
                    .SetProperty(x => x.ClaimedAt, (DateTime?)null)
                    .SetProperty(x => x.ClaimedBy, (string?)null)
            );

        await Parallel.ForEachAsync(
            orphanedActivityIds,
            async (id, ct) =>
                await this.activityProcessor.ClaimAndProcessActivityAsync(id, this.options.Value.InstanceId, ct)
        );
    }
}
