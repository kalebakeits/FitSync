namespace FitSync.Shared.Features.Fetcher.Services;

using System.Text.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.ActivityIngest.DTOs;
using FitSync.Shared.Features.ActivityIngest.Services;
using FitSync.Shared.Features.Fetcher.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ActivityPersistenceService(
    FitSyncDbContext dbContext,
    IActivityPublisher activityPublisher,
    IFitSessionDecoder fitSessionDecoder,
    IScheduledWorkoutLinker scheduledWorkoutLinker,
    ILogger<ActivityPersistenceService> logger
) : IActivityPersistenceService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IActivityPublisher activityPublisher = activityPublisher;
    private readonly IFitSessionDecoder fitSessionDecoder = fitSessionDecoder;
    private readonly IScheduledWorkoutLinker scheduledWorkoutLinker = scheduledWorkoutLinker;
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

        FitSessionStats? stats = this.fitSessionDecoder.Decode(fetchedActivity.FitFileData);

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
                Sport = stats?.Sport,
                DurationSeconds = stats?.DurationSeconds,
                DistanceMeters = stats?.DistanceMeters,
                AvgHeartRate = stats?.AvgHeartRate,
                AvgPower = stats?.AvgPower,
                ActivityMetadata =
                    fetchedActivity.Metadata != null
                        ? JsonSerializer.Serialize(fetchedActivity.Metadata)
                        : null
            };

        this.dbContext.Activities.Add(activity);

        List<UserDestinationConfig> destinations =
            await this.dbContext.UserDestinationConfigs.Where(
                c =>
                    c.UserId == userId
                    && c.SourceServiceType == fetchedActivity.Source
                    && c.DestinationServiceType != fetchedActivity.Source
            )
                .ToListAsync(cancellationToken);

        foreach (UserDestinationConfig dest in destinations)
        {
            this.dbContext.ActivityUploadStatuses.Add(
                new ActivityUploadStatus
                {
                    ActivityId = activity.Id,
                    DestinationServiceType = dest.DestinationServiceType,
                }
            );
        }

        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Saved activity {ExternalActivityId} to database",
            fetchedActivity.ExternalActivityId
        );

        Guid? scheduledWorkoutId = await this.scheduledWorkoutLinker.LinkAsync(
            activity.Id,
            cancellationToken
        );

        this.logger.LogInformation(
            "Activity {ExternalActivityId} link result: {ScheduledWorkoutId}",
            fetchedActivity.ExternalActivityId,
            scheduledWorkoutId
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
