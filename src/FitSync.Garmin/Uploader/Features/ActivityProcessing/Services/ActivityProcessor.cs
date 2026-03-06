namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
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
    IOptions<GarminUploaderOptions> options
) : IActivityProcessor
{
    private readonly FitSyncDbContext fitSyncDbContext = dbContext;
    private readonly IFitModifierFactory fitModifierFactory = fitModifierFactory;
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
        bool claimed = await this.TryClaimAsync(
            activityId,
            instanceId,
            u => u.ClaimedBy == null && u.Status == ActivityStatus.Pending,
            cancellationToken
        );

        if (claimed)
            await this.ProcessActivityAsync(activityId, cancellationToken);
    }

    public async Task ReclaimAndProcessActivityAsync(
        Guid activityId,
        string instanceId,
        DateTime orphanCutoff,
        CancellationToken cancellationToken
    )
    {
        bool claimed = await this.TryClaimAsync(
            activityId,
            instanceId,
            u => u.ClaimedBy != null && u.ClaimedAt < orphanCutoff,
            cancellationToken
        );

        if (claimed)
            await this.ProcessActivityAsync(activityId, cancellationToken);
    }

    private async Task ProcessActivityAsync(Guid activityId, CancellationToken cancellationToken)
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

        ActivityUploadStatus? uploadStatus =
            await this.fitSyncDbContext.ActivityUploadStatuses.FirstOrDefaultAsync(
                u => u.ActivityId == activityId && u.DestinationServiceType == ServiceTypes.Garmin,
                cancellationToken
            );

        if (uploadStatus == null)
        {
            this.logger.LogWarning("No Garmin upload status for activity {ActivityId}", activityId);
            return;
        }
        UploadResult? uploadResult = null;
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

            byte[] modifiedFit = this.fitModifierFactory.GetModifier(activity.Source)
                .ModifyDeviceInfo(activity.FitFileData ?? Array.Empty<byte>());

            uploadResult = await this.garminUploader.UploadActivityAsync(
                modifiedFit,
                activity.User,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Failed to process activity {ActivityId}. Sorry chat",
                activityId
            );
        }
        finally
        {
            uploadResult ??= UploadResult.Failed("Activity failed for an unknown reason.");
            await this.resultHandler.HandleUploadResultAsync(
                activity,
                uploadStatus,
                uploadResult,
                this.options.Value.MaxRetries
            );
            await this.fitSyncDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<bool> TryClaimAsync(
        Guid activityId,
        string instanceId,
        System.Linq.Expressions.Expression<Func<ActivityUploadStatus, bool>> claimPredicate,
        CancellationToken cancellationToken
    )
    {
        int affected = await this.fitSyncDbContext.ActivityUploadStatuses.Where(
            u => u.ActivityId == activityId && u.DestinationServiceType == ServiceTypes.Garmin
        )
            .Where(claimPredicate)
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
            return false;
        }

        this.logger.LogInformation(
            "Successfully claimed upload status for activity {ActivityId}. Take your L boys.",
            activityId
        );
        return true;
    }
}
