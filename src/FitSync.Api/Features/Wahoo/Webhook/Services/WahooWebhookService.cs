namespace FitSync.Api.Features.Wahoo.Webhook.Services;

using FitSync.Api.Features.Wahoo.Webhook.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using FitSync.Wahoo.Shared.WahooClient;
using Microsoft.EntityFrameworkCore;

public class WahooWebhookService(
    FitSyncDbContext dbContext,
    IWahooClient wahooClient,
    IActivityPublisher activityPublisher,
    ILogger<WahooWebhookService> logger
) : IWahooWebhookService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWahooClient wahooClient = wahooClient;
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

        Integration? integration = await this.dbContext.Integrations.FirstOrDefaultAsync(
            i => i.ServiceType == ServiceTypes.Wahoo && i.LookupValue == wahooUserId,
            cancellationToken
        );

        if (integration == null)
        {
            this.logger.LogWarning("No Wahoo integration for Wahoo user {WahooUserId}. Skipping.", wahooUserId);
            return;
        }

        bool alreadyProcessed = await this.dbContext.ProcessedActivities.AnyAsync(
            p => p.UserId == integration.UserId && p.ExternalActivityId == externalId && p.Source == ServiceTypes.Wahoo,
            cancellationToken
        );

        if (alreadyProcessed)
        {
            this.logger.LogInformation("Workout {ExternalId} already processed. Skipping.", externalId);
            return;
        }

        byte[] fitData = await this.wahooClient.DownloadFitFileAsync(fileUrl, cancellationToken);

        Activity dbActivity = new()
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        this.dbContext.Activities.Add(dbActivity);

        List<UserDestinationConfig> destinations = await this.dbContext.UserDestinationConfigs
            .Where(c => c.UserId == integration.UserId && c.SourceServiceType == ServiceTypes.Wahoo)
            .ToListAsync(cancellationToken);

        foreach (UserDestinationConfig dest in destinations)
        {
            this.dbContext.ActivityUploadStatuses.Add(new ActivityUploadStatus
            {
                ActivityId = dbActivity.Id,
                DestinationServiceType = dest.DestinationServiceType,
            });
        }

        await this.dbContext.SaveChangesAsync(cancellationToken);
        await this.activityPublisher.PublishActivityFetchedAsync(dbActivity, cancellationToken);

        this.logger.LogInformation(
            "Processed webhook workout {ExternalId} for user {UserId}.",
            externalId,
            integration.UserId
        );
    }
}
