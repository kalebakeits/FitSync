namespace FitSync.Api.Features.Activities.Services;

using FitSync.Api.Exceptions;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.Services;
using Microsoft.EntityFrameworkCore;

public class ActivityRetryService(
    FitSyncDbContext context,
    IActivityPublisher activityPublisher,
    ILogger<ActivityRetryService> logger
) : IActivityRetryService
{
    private readonly FitSyncDbContext context = context;
    private readonly IActivityPublisher activityPublisher = activityPublisher;
    private readonly ILogger<ActivityRetryService> logger = logger;

    private static readonly HashSet<ActivityStatus> retryableStatuses =
    [
        ActivityStatus.Failed,
        ActivityStatus.Conflict,
        ActivityStatus.ServiceUnavailable,
    ];

    public async Task RetryFailedAsync(Guid userId, Guid activityId, CancellationToken ct = default)
    {
        Activity? activity = await this.context.Activities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.UserId == userId && !a.IsDeleted, ct);

        if (activity == null)
            throw new NotFoundException("Activity not found.");

        int updated = await this.context.ActivityUploadStatuses
            .Where(u => u.ActivityId == activityId && retryableStatuses.Contains(u.Status))
            .ExecuteUpdateAsync(
                u => u
                    .SetProperty(x => x.Status, ActivityStatus.Pending)
                    .SetProperty(x => x.ClaimedBy, (string?)null)
                    .SetProperty(x => x.ClaimedAt, (DateTime?)null),
                ct
            );

        if (updated == 0)
        {
            this.logger.LogInformation(
                "No retryable upload statuses for activity {ActivityId}",
                activityId
            );
            return;
        }

        await this.activityPublisher.PublishActivityFetchedAsync(activity, ct);

        this.logger.LogInformation(
            "Queued retry for {Count} destinations on activity {ActivityId}",
            updated,
            activityId
        );
    }

    public async Task PushToDestinationAsync(
        Guid userId,
        Guid activityId,
        string destinationServiceType,
        CancellationToken ct = default
    )
    {
        Activity? activity = await this.context.Activities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.UserId == userId && !a.IsDeleted, ct);

        if (activity == null)
            throw new NotFoundException("Activity not found.");

        bool configured = await this.context.UserDestinationConfigs.AnyAsync(
            c =>
                c.UserId == userId
                && c.SourceServiceType == activity.Source
                && c.DestinationServiceType == destinationServiceType,
            ct
        );

        if (!configured)
            throw new NotFoundException("Destination not configured for this user.");

        bool alreadyExists = await this.context.ActivityUploadStatuses.AnyAsync(
            u => u.ActivityId == activityId && u.DestinationServiceType == destinationServiceType,
            ct
        );

        if (alreadyExists)
        {
            this.logger.LogInformation(
                "Upload status for destination {Dest} already exists on activity {ActivityId}",
                destinationServiceType,
                activityId
            );
            return;
        }

        this.context.ActivityUploadStatuses.Add(new ActivityUploadStatus
        {
            ActivityId = activityId,
            DestinationServiceType = destinationServiceType,
        });

        await this.context.SaveChangesAsync(ct);
        await this.activityPublisher.PublishActivityFetchedAsync(activity, ct);

        this.logger.LogInformation(
            "Pushed activity {ActivityId} to destination {Dest}",
            activityId,
            destinationServiceType
        );
    }
}
