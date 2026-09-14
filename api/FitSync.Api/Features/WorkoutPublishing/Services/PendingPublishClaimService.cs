namespace FitSync.Api.Features.WorkoutPublishing.Services;

using FitSync.Database;
using Microsoft.EntityFrameworkCore;

public class PendingPublishClaimService(
    FitSyncDbContext dbContext,
    ILogger<PendingPublishClaimService> logger
) : IPendingPublishClaimService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ILogger<PendingPublishClaimService> logger = logger;

    public async Task<bool> TryClaimAsync(
        Guid scheduledWorkoutId,
        DateTime now,
        DateTime staleClaimCutoff,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Claiming deferred publish for scheduled workout {ScheduledWorkoutId}.",
            scheduledWorkoutId
        );

        int claimed = await this.dbContext
            .ScheduledWorkouts.Where(s =>
                s.Id == scheduledWorkoutId
                && s.PendingPublishAt != null
                && s.PendingPublishAt <= now
                && (s.PublishClaimedAt == null || s.PublishClaimedAt < staleClaimCutoff)
            )
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.PublishClaimedAt, now),
                cancellationToken
            );

        if (claimed == 0)
        {
            this.logger.LogInformation(
                "Scheduled workout {ScheduledWorkoutId} was already claimed or is no longer due.",
                scheduledWorkoutId
            );
            return false;
        }

        this.logger.LogInformation(
            "Claimed deferred publish for scheduled workout {ScheduledWorkoutId}.",
            scheduledWorkoutId
        );
        return true;
    }

    public async Task<bool> CompleteAsync(
        Guid scheduledWorkoutId,
        DateOnly publishedDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Clearing deferred publish for scheduled workout {ScheduledWorkoutId} published for {PublishedDate}.",
            scheduledWorkoutId,
            publishedDate
        );

        int cleared = await this.dbContext
            .ScheduledWorkouts.Where(s =>
                s.Id == scheduledWorkoutId && s.ScheduledDate == publishedDate
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(x => x.PendingPublishAt, (DateTime?)null)
                        .SetProperty(x => x.PublishClaimedAt, (DateTime?)null),
                cancellationToken
            );

        if (cleared == 0)
        {
            this.logger.LogWarning(
                "Scheduled workout {ScheduledWorkoutId} moved again while its publish was in flight. PendingPublishAt is kept so the new date is published.",
                scheduledWorkoutId
            );
            return false;
        }

        this.logger.LogInformation(
            "Deferred publish cleared for scheduled workout {ScheduledWorkoutId}.",
            scheduledWorkoutId
        );
        return true;
    }

    public async Task ReleaseAsync(
        Guid scheduledWorkoutId,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Releasing claim on scheduled workout {ScheduledWorkoutId}. PendingPublishAt is left untouched so the next sweep retries.",
            scheduledWorkoutId
        );

        int released = await this.dbContext
            .ScheduledWorkouts.Where(s => s.Id == scheduledWorkoutId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.PublishClaimedAt, (DateTime?)null),
                cancellationToken
            );

        if (released == 0)
        {
            this.logger.LogWarning(
                "Scheduled workout {ScheduledWorkoutId} vanished before its claim could be released.",
                scheduledWorkoutId
            );
            return;
        }

        this.logger.LogInformation(
            "Claim released for scheduled workout {ScheduledWorkoutId}.",
            scheduledWorkoutId
        );
    }
}
