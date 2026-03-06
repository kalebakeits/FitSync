namespace FitSync.Garmin.Uploader.Features.ActivityProcessing.Services;

using System.Net;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Garmin.Uploader.Features.GarminUpload.DTOs;
using Microsoft.EntityFrameworkCore;

public class UploadResultHandler(
    IActivityStatusMapper activityStatusMapper,
    ILogger<UploadResultHandler> logger,
    FitSyncDbContext dbContext
) : IUploadResultHandler
{
    private static readonly HashSet<ActivityStatus> TerminalStatuses =
    [
        ActivityStatus.Failed,
        ActivityStatus.Conflict
    ];

    private readonly IActivityStatusMapper activityStatusMapper = activityStatusMapper;
    private readonly ILogger<UploadResultHandler> logger = logger;
    private readonly FitSyncDbContext fitSyncDbContext = dbContext;

    public async Task HandleUploadResultAsync(
        Activity activity,
        ActivityUploadStatus uploadStatus,
        UploadResult result,
        int maxRetries
    )
    {
        if (result.Success)
        {
            uploadStatus.Status = ActivityStatus.Uploaded;
            uploadStatus.ProcessingCompletedAt = DateTime.UtcNow;
            this.logger.LogInformation(
                "Successfully uploaded activity {ActivityId} to {Destination}. Let's goooooooo",
                activity.Id,
                uploadStatus.DestinationServiceType
            );

            await this.fitSyncDbContext.Integrations.Where(
                i => i.UserId == activity.UserId && i.ServiceType == ServiceTypes.Garmin
            )
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.FailureCount, 0));

            return;
        }

        if (result.ShouldRetry)
        {
            this.logger.LogInformation(
                "Activity {ActivityId} deferred for retry: {Reason}. I'm such a hard worker",
                activity.Id,
                result.ErrorMessage
            );
            uploadStatus.Status = ActivityStatus.Pending;
            uploadStatus.ClaimedBy = null;
            uploadStatus.ClaimedAt = null;
            return;
        }

        uploadStatus.Status = this.activityStatusMapper.MapHttpStatusToActivityStatus(
            result.StatusCode
        );
        uploadStatus.LastError = result.ErrorMessage;
        uploadStatus.LastErrorAt = DateTime.UtcNow;
        uploadStatus.RetryCount++;

        this.logger.LogWarning(
            "Upload failed for activity {ActivityId} to {Destination} with status {Status}. Womp womp",
            activity.Id,
            uploadStatus.DestinationServiceType,
            uploadStatus.Status
        );

        if (
            result.StatusCode == HttpStatusCode.Unauthorized
            || result.StatusCode == HttpStatusCode.Forbidden
        )
        {
            await this.fitSyncDbContext.Integrations.Where(
                i => i.UserId == activity.UserId && i.ServiceType == ServiceTypes.Garmin
            )
                .ExecuteUpdateAsync(
                    s => s.SetProperty(i => i.FailureCount, i => i.FailureCount + 1)
                );

            this.logger.LogWarning(
                "Incremented failure count for user {UserId}. Skill issue",
                activity.UserId
            );
        }

        if (!TerminalStatuses.Contains(uploadStatus.Status) && uploadStatus.RetryCount < maxRetries)
        {
            this.logger.LogInformation(
                "Will retry activity {ActivityId} (attempt {Retry}/{Max}). I'm such a hard worker",
                activity.Id,
                uploadStatus.RetryCount,
                maxRetries
            );
            uploadStatus.Status = ActivityStatus.Pending;
            uploadStatus.ClaimedBy = null;
            uploadStatus.ClaimedAt = null;
        }
    }
}
