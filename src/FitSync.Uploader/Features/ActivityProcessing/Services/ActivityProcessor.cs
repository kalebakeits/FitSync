using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Uploader.Configuration;
using FitSync.Uploader.Features.FitModification.Services;
using FitSync.Uploader.Features.GarminUpload;
using FitSync.Uploader.Features.GarminUpload.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FitSync.Uploader.Features.ActivityProcessing.Services;

public class ActivityProcessor(
    FitSyncDbContext dbContext,
    IFitModifier fitModifier,
    IGarminUploader garminUploader,
    IUploadResultHandler resultHandler,
    ILogger<ActivityProcessor> logger,
    IOptions<GarminUploaderOptions> options
) : IActivityProcessor
{
    private readonly FitSyncDbContext fitSyncDbContext = dbContext;
    private readonly IFitModifier fitModifier = fitModifier;
    private readonly IGarminUploader garminUploader = garminUploader;
    private readonly IUploadResultHandler resultHandler = resultHandler;
    private readonly ILogger<ActivityProcessor> logger = logger;
    private readonly IOptions<GarminUploaderOptions> options = options;

    public async Task ClaimAndProcessActivityAsync(
        Guid activityId,
        string instanceId,
        CancellationToken cancellationToken
    )
    {
        int affected = await this.fitSyncDbContext.Activities.Where(
            a => a.Id == activityId && a.ClaimedBy == null
        )
            .ExecuteUpdateAsync(
                a =>
                    a.SetProperty(x => x.Status, ActivityStatus.Claimed)
                        .SetProperty(x => x.ClaimedBy, instanceId)
                        .SetProperty(x => x.ClaimedAt, DateTime.UtcNow),
                cancellationToken
            );

        if (affected == 0)
        {
            this.logger.LogInformation(
                "Activity {ActivityId} was already claimed by another instance",
                activityId
            );
            return;
        }

        this.logger.LogInformation("Successfully claimed activity {ActivityId}", activityId);

        await this.ProcessActivityAsync(activityId, cancellationToken);
    }

    public async Task ProcessActivityAsync(Guid activityId, CancellationToken cancellationToken)
    {
        Activity? activity = await this.fitSyncDbContext.Activities.Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == activityId, cancellationToken);

        if (activity == null)
        {
            this.logger.LogWarning("Activity {ActivityId} not found", activityId);
            return;
        }

        try
        {
            activity.Status = ActivityStatus.Processing;
            activity.ProcessingStartedAt = DateTime.UtcNow;
            await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation(
                "Processing activity {ActivityId} for user {UserId}",
                activityId,
                activity.UserId
            );

            byte[] modifiedFit = this.fitModifier.ModifyDeviceInfo(
                activity.FitFileData ?? Array.Empty<byte>()
            );

            UploadResult uploadResult = await this.garminUploader.UploadActivityAsync(
                modifiedFit,
                activity.User,
                cancellationToken
            );

            await this.resultHandler.HandleUploadResultAsync(
                activity,
                uploadResult,
                this.options.Value.MaxRetries
            );
            await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to process activity {ActivityId}", activityId);

            activity.Status = ActivityStatus.Failed;
            activity.LastError = ex.Message;
            activity.LastErrorAt = DateTime.UtcNow;
            activity.RetryCount++;

            // Retry logic
            if (activity.RetryCount < this.options.Value.MaxRetries)
            {
                this.logger.LogInformation(
                    "Will retry activity {ActivityId} (attempt {Retry}/{Max})",
                    activityId,
                    activity.RetryCount,
                    this.options.Value.MaxRetries
                );
                activity.Status = ActivityStatus.Pending;
                activity.ClaimedBy = null;
                activity.ClaimedAt = null;
            }

            await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
