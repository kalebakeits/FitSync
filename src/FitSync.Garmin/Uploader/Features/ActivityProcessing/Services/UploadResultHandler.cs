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
            this.logger.LogInformation("Successfully uploaded activity {ActivityId}. Let's goooooooo", activity.Id);

            await this.fitSyncDbContext.Integrations
                .Where(i => i.UserId == activity.UserId && i.ServiceType == ServiceTypes.Garmin)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.FailureCount, 0));

            return;
        }

        activity.Status = this.activityStatusMapper.MapHttpStatusToActivityStatus(result.StatusCode);
        activity.LastError = result.ErrorMessage;
        activity.LastErrorAt = DateTime.UtcNow;
        activity.RetryCount++;

        this.logger.LogWarning("Upload failed for activity {ActivityId} with status {Status}. Womp womp", activity.Id, activity.Status);

        if (result.StatusCode == HttpStatusCode.Unauthorized || result.StatusCode == HttpStatusCode.Forbidden)
        {
            await this.fitSyncDbContext.Integrations
                .Where(i => i.UserId == activity.UserId && i.ServiceType == ServiceTypes.Garmin)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.FailureCount, i => i.FailureCount + 1));

            this.logger.LogWarning("Incremented failure count for user {UserId}. Skill issue", activity.UserId);
        }

        if (ShouldRetry(activity, maxRetries))
        {
            this.logger.LogInformation("Will retry activity {ActivityId} (attempt {Retry}/{Max}). I'm such a hard worker", activity.Id, activity.RetryCount, maxRetries);
            activity.Status = ActivityStatus.Pending;
            activity.ClaimedBy = null;
            activity.ClaimedAt = null;
        }
    }

    private static bool ShouldRetry(Activity activity, int maxRetries)
    {
        HashSet<ActivityStatus> statuses = [ActivityStatus.Failed, ActivityStatus.Conflict];
        return !statuses.Contains(activity.Status) && activity.RetryCount < maxRetries;
    }
}
