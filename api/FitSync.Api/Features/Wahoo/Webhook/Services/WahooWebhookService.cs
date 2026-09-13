namespace FitSync.Api.Features.Wahoo.Webhook.Services;

using FitSync.Api.Features.Wahoo.Webhook.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Wahoo.Shared.WahooClient.Services;
using Microsoft.EntityFrameworkCore;

public class WahooWebhookService(
    FitSyncDbContext dbContext,
    IWahooActivityProcessor activityProcessor,
    IActivityPublisher activityPublisher,
    ILogger<WahooWebhookService> logger
) : IWahooWebhookService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWahooActivityProcessor activityProcessor = activityProcessor;
    private readonly IActivityPublisher activityPublisher = activityPublisher;
    private readonly ILogger<WahooWebhookService> logger = logger;

    public async Task ProcessAsync(
        WahooWebhookPayload payload,
        CancellationToken cancellationToken = default
    )
    {
        string? fileUrl = payload.WorkoutSummary?.File?.Url;
        WahooWebhookWorkout? workout = payload.WorkoutSummary?.Workout;

        if (string.IsNullOrEmpty(fileUrl) || workout == null)
        {
            this.logger.LogWarning("Webhook payload missing FIT file URL or workout. Skipping.");
            return;
        }

        if (payload.User == null)
        {
            this.logger.LogWarning("Webhook payload missing user. Skipping.");
            return;
        }

        string externalId = workout.Id.ToString();
        string wahooUserId = payload.User.Id.ToString();

        List<Integration> integrations = await this.dbContext.Integrations.Where(
            i => i.ServiceType == ServiceTypes.Wahoo && i.LookupValue == wahooUserId
        )
            .ToListAsync(cancellationToken);

        if (integrations.Count == 0)
        {
            this.logger.LogWarning(
                "No Wahoo integration for Wahoo user {WahooUserId}. Skipping.",
                wahooUserId
            );
            return;
        }

        List<Guid> alreadyProcessedUserIds = await this.dbContext.ProcessedActivities.Where(
            p => p.ExternalActivityId == externalId && p.Source == ServiceTypes.Wahoo
        )
            .Select(p => p.UserId)
            .ToListAsync(cancellationToken);

        List<Integration> toProcess = integrations
            .Where(i => !alreadyProcessedUserIds.Contains(i.UserId))
            .ToList();

        if (toProcess.Count == 0)
        {
            this.logger.LogInformation(
                "Workout {ExternalId} already processed for all users. Skipping.",
                externalId
            );
            return;
        }

        byte[] fitData = await this.activityProcessor.DownloadFitFileAsync(
            fileUrl,
            cancellationToken
        );
        DateTime now = DateTime.UtcNow;

        foreach (Integration integration in toProcess)
        {
            Activity dbActivity =
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = integration.UserId,
                    ExternalActivityId = externalId,
                    Source = ServiceTypes.Wahoo,
                    OriginalFileName = $"wahoo_{externalId}.fit",
                    FitFileData = fitData,
                    FileSizeBytes = fitData.LongLength,
                    ActivityDate = workout.Starts,
                    ActivityName = workout.Name,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

            this.dbContext.Activities.Add(dbActivity);

            List<UserDestinationConfig> destinations =
                await this.dbContext.UserDestinationConfigs.Where(
                    c => c.UserId == integration.UserId && c.SourceServiceType == ServiceTypes.Wahoo
                )
                    .ToListAsync(cancellationToken);

            foreach (UserDestinationConfig dest in destinations)
            {
                this.dbContext.ActivityUploadStatuses.Add(
                    new ActivityUploadStatus
                    {
                        ActivityId = dbActivity.Id,
                        DestinationServiceType = dest.DestinationServiceType,
                    }
                );
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);
            await this.activityPublisher.PublishActivityFetchedAsync(dbActivity, cancellationToken);

            this.dbContext.ProcessedActivities.Add(
                new ProcessedActivity
                {
                    Id = Guid.NewGuid(),
                    UserId = integration.UserId,
                    ExternalActivityId = externalId,
                    Source = ServiceTypes.Wahoo,
                    FetchedAt = now,
                }
            );
            await this.dbContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation(
                "Processed webhook workout {ExternalId} for user {UserId}.",
                externalId,
                integration.UserId
            );
        }
    }
}
