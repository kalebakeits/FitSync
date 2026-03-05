namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Features.RateLimiting;
using FitSync.Garmin.Uploader.Configuration;
using FitSync.Garmin.Uploader.Features.FitModification.Services;
using FitSync.Garmin.Uploader.Features.GarminUpload;
using FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class ActivityProcessor(
    FitSyncDbContext dbContext,
    IFitModifierFactory fitModifierFactory,
    IGarminUploader garminUploader,
    IUploadResultHandler resultHandler,
    ILogger<ActivityProcessor> logger,
    IRateLimiter rateLimiter,
    IOptions<GarminUploaderOptions> options
) : IActivityProcessor
{
    private readonly FitSyncDbContext fitSyncDbContext = dbContext;
    private readonly IFitModifierFactory fitModifierFactory = fitModifierFactory;
    private readonly IGarminUploader garminUploader = garminUploader;
    private readonly IUploadResultHandler resultHandler = resultHandler;
    private readonly ILogger<ActivityProcessor> logger = logger;
    private readonly IRateLimiter rateLimiter = rateLimiter;
    private readonly IOptions<GarminUploaderOptions> options = options;

    public async Task ClaimAndProcessActivityAsync(
        Guid activityId,
        string instanceId,
        CancellationToken cancellationToken
    )
    {
        ServiceType type = ServiceType.GarminUploader;
        if (await this.rateLimiter.RateLimitedReachedAsync(type, this.options.Value.RateLimits, cancellationToken))
            return;

        int affected = await this.fitSyncDbContext.ActivityUploadStatuses.Where(
            u =>
                u.ActivityId == activityId
                && u.DestinationServiceType == ServiceTypes.Garmin
                && u.ClaimedBy == null
                && u.Status == ActivityStatus.Pending
        )
            .ExecuteUpdateAsync(
                u =>
                    u.SetProperty(x => x.Status, ActivityStatus.Claimed)
                        .SetProperty(x => x.ClaimedBy, instanceId)
                        .SetProperty(x => x.ClaimedAt, DateTime.UtcNow),
                cancellationToken
            );

        if (affected == 0)
        {
            this.logger.LogInformation(
                "Upload status for activity {ActivityId} already claimed. We'll get em next time.",
                activityId
            );
            return;
        }

        this.logger.LogInformation(
            "Successfully claimed upload status for activity {ActivityId}. Take your L boys.",
            activityId
        );

        await this.ProcessActivityAsync(activityId, cancellationToken);
    }

    public async Task ProcessActivityAsync(Guid activityId, CancellationToken cancellationToken)
    {
        Activity? activity = await this.fitSyncDbContext.Activities.Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == activityId, cancellationToken);

        if (activity == null)
        {
            this.logger.LogWarning(
                "Activity {ActivityId} not found. This really should not have happened",
                activityId
            );
            return;
        }

        if (activity.IsDeleted)
        {
            this.logger.LogInformation(
                "Activity {ActivityId} is soft-deleted, skipping processing",
                activityId
            );
            return;
        }

        ActivityUploadStatus? uploadStatus = await this.fitSyncDbContext.ActivityUploadStatuses
            .FirstOrDefaultAsync(
                u => u.ActivityId == activityId && u.DestinationServiceType == ServiceTypes.Garmin,
                cancellationToken
            );

        if (uploadStatus == null)
        {
            this.logger.LogWarning(
                "No Garmin upload status for activity {ActivityId}",
                activityId
            );
            return;
        }

        try
        {
            uploadStatus.Status = ActivityStatus.Processing;
            uploadStatus.ProcessingStartedAt = DateTime.UtcNow;
            await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation(
                "Processing activity {ActivityId} for user {UserId}. Outta my way!!!",
                activityId,
                activity.UserId
            );

            byte[] modifiedFit = this.fitModifierFactory
                .GetModifier(activity.Source)
                .ModifyDeviceInfo(activity.FitFileData ?? Array.Empty<byte>());

            UploadResult uploadResult = await this.garminUploader.UploadActivityAsync(
                modifiedFit,
                activity.User,
                cancellationToken
            );

            await this.resultHandler.HandleUploadResultAsync(
                activity,
                uploadStatus,
                uploadResult,
                this.options.Value.MaxRetries
            );
            await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Failed to process activity {ActivityId}. Sorry chat",
                activityId
            );

            uploadStatus.Status = ActivityStatus.Failed;
            uploadStatus.LastError = ex.Message;
            uploadStatus.LastErrorAt = DateTime.UtcNow;
            uploadStatus.RetryCount++;

            if (uploadStatus.RetryCount < this.options.Value.MaxRetries)
            {
                this.logger.LogInformation(
                    "Will retry activity {ActivityId} (attempt {Retry}/{Max}). I'm such a hard worker",
                    activityId,
                    uploadStatus.RetryCount,
                    this.options.Value.MaxRetries
                );
                uploadStatus.Status = ActivityStatus.Pending;
                uploadStatus.ClaimedBy = null;
                uploadStatus.ClaimedAt = null;
            }

            await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
