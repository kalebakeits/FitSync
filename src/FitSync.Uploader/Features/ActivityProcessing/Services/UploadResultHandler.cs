namespace FitSync.Uploader.Features.ActivityProcessing.Services;

using System.Net;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Uploader.Features.GarminUpload.DTOs;
using Microsoft.EntityFrameworkCore;

public class UploadResultHandler(
    IActivityStatusMapper activityStatusMapper,
    ILogger<UploadResultHandler> logger,
    FitSyncDbContext dbContext
) : IUploadResultHandler
{
    private readonly IActivityStatusMapper activityStatusMapper = activityStatusMapper;
    private readonly ILogger<UploadResultHandler> logger = logger;
    private readonly FitSyncDbContext fitSyncDbContext = dbContext;

    public async Task HandleUploadResultAsync(
        Activity activity,
        UploadResult result,
        int maxRetries
    )
    {
        if (result.Success)
        {
            activity.Status = ActivityStatus.Uploaded;
            activity.ProcessingCompletedAt = DateTime.UtcNow;
            this.logger.LogInformation("Successfully uploaded activity {ActivityId}", activity.Id);

            // Reset credential failure count on success
            await this.fitSyncDbContext.UserCredentials.Where(
                c => c.UserId == activity.UserId && c.ServiceType == ServiceTypes.Garmin
            )
                .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.FailureCount, 0));

            return;
        }

        activity.Status = this.activityStatusMapper.MapHttpStatusToActivityStatus(
            result.StatusCode
        );
        activity.LastError = result.ErrorMessage;
        activity.LastErrorAt = DateTime.UtcNow;
        activity.RetryCount++;

        this.logger.LogWarning(
            "Upload failed for activity {ActivityId} with status {Status}",
            activity.Id,
            activity.Status
        );

        // Increment credential failure count on auth errors
        if (
            result.StatusCode == HttpStatusCode.Unauthorized
            || result.StatusCode == HttpStatusCode.Forbidden
        )
        {
            await this.fitSyncDbContext.UserCredentials.Where(
                c => c.UserId == activity.UserId && c.ServiceType == ServiceTypes.Garmin
            )
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.FailureCount, c => c.FailureCount + 1)
                );

            this.logger.LogWarning(
                "Incremented credential failure count for user {UserId} due to auth failure",
                activity.UserId
            );
        }

        if (ShouldRetry(activity, maxRetries))
        {
            this.logger.LogInformation(
                "Will retry activity {ActivityId} (attempt {Retry}/{Max})",
                activity.Id,
                activity.RetryCount,
                maxRetries
            );
            activity.Status = ActivityStatus.Pending;
            activity.ClaimedBy = null;
            activity.ClaimedAt = null;
        }
    }

    private static bool ShouldRetry(Activity activity, int maxRetries)
    {
        return activity.Status == ActivityStatus.ServiceUnavailable
            && activity.RetryCount < maxRetries;
    }
}
