namespace FitSync.Shared.Features.Fetcher.Services;

using System.Text.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ActivityPersistenceService(
    FitSyncDbContext dbContext,
    IActivityPublisher activityPublisher,
    ILogger<ActivityPersistenceService> logger
) : IActivityPersistenceService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IActivityPublisher activityPublisher = activityPublisher;
    private readonly ILogger<ActivityPersistenceService> logger = logger;

    public async Task SaveAndPublishAsync(
        Guid userId,
        FetchedActivity fetchedActivity,
        CancellationToken cancellationToken
    )
    {
        bool alreadyProcessed = await this.dbContext.ProcessedActivities.AnyAsync(
            p =>
                p.UserId == userId
                && p.ExternalActivityId == fetchedActivity.ExternalActivityId
                && p.Source == fetchedActivity.Source,
            cancellationToken
        );

        if (alreadyProcessed)
        {
            this.logger.LogDebug(
                "Activity {ExternalActivityId} already processed",
                fetchedActivity.ExternalActivityId
            );
            return;
        }

        Activity activity =
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExternalActivityId = fetchedActivity.ExternalActivityId,
                Source = fetchedActivity.Source,
                FitFileData = fetchedActivity.FitFileData,
                FileSizeBytes = fetchedActivity.FitFileData.Length,
                OriginalFileName = fetchedActivity.FileName,
                ActivityDate = fetchedActivity.ActivityDate,
                ActivityName = fetchedActivity.ActivityName ?? $"{fetchedActivity.Source} Activity",
                ActivityMetadata =
                    fetchedActivity.Metadata != null
                        ? JsonSerializer.Serialize(fetchedActivity.Metadata)
                        : null
            };

        this.dbContext.Activities.Add(activity);

        List<UserDestinationConfig> destinations = await this.dbContext.UserDestinationConfigs
            .Where(c => c.UserId == userId && c.SourceServiceType == fetchedActivity.Source)
            .ToListAsync(cancellationToken);

        foreach (UserDestinationConfig dest in destinations)
        {
            this.dbContext.ActivityUploadStatuses.Add(new ActivityUploadStatus
            {
                ActivityId = activity.Id,
                DestinationServiceType = dest.DestinationServiceType,
            });
        }

        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Saved activity {ExternalActivityId} to database",
            fetchedActivity.ExternalActivityId
        );

        await this.activityPublisher.PublishActivityFetchedAsync(activity, cancellationToken);

        ProcessedActivity processedActivity =
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExternalActivityId = fetchedActivity.ExternalActivityId,
                Source = fetchedActivity.Source,
                FetchedAt = DateTime.UtcNow
            };

        this.dbContext.ProcessedActivities.Add(processedActivity);
        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Activity {ExternalActivityId} marked as processed",
            fetchedActivity.ExternalActivityId
        );
    }
}
