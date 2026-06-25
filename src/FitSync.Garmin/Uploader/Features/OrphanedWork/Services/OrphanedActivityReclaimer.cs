namespace FitSync.Garmin.Uploader.Features.OrphanedWork.Services;

using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Garmin.Uploader.Configuration;
using FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class OrphanedActivityReclaimer(
    IDbContextFactory<FitSyncDbContext> dbContextFactory,
    IOptions<GarminUploaderOptions> options,
    ILogger<OrphanedActivityReclaimer> logger,
    IActivityProcessor activityProcessor
) : IOrphanedActivityReclaimer
{
    private readonly IDbContextFactory<FitSyncDbContext> dbContextFactory = dbContextFactory;
    private readonly IOptions<GarminUploaderOptions> options = options;
    private readonly ILogger<OrphanedActivityReclaimer> logger = logger;
    private readonly IActivityProcessor activityProcessor = activityProcessor;

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

        await using FitSyncDbContext dbContext = await this.dbContextFactory.CreateDbContextAsync(
            cancellationToken
        );

        IQueryable<ActivityUploadStatus> orphanedActivities =
            dbContext.ActivityUploadStatuses.Where(
                u =>
                    u.DestinationServiceType == ServiceTypes.Garmin
                    && incompleteStatuses.Contains(u.Status)
                    && (
                        (u.ClaimedBy != null && u.ClaimedAt < cutoff)
                        || (u.ClaimedBy == null && u.UpdatedAt < cutoff)
                    )
            );

        int orphanCount = await orphanedActivities.CountAsync(cancellationToken);

        if (orphanCount == 0)
        {
            this.logger.LogDebug("There are no orphans to adopt. Back you go!");
            return;
        }

        this.logger.LogWarning(
            "{AffectedRows} orphans have been found. Will attempt to adopt them.",
            orphanCount
        );

        await foreach (
            Guid activityId in orphanedActivities
                .Select(a => a.ActivityId)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken)
        )
        {
            this.logger.LogDebug("Trying to adopt orphan - Activity: {ActivityId}.", activityId);
            await this.activityProcessor.ReclaimAndProcessActivityAsync(
                activityId,
                this.options.Value.InstanceId,
                cutoff,
                cancellationToken
            );
        }
    }
}
