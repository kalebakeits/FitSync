namespace FitSync.Shared.Features.ActivityIngest.Services;

using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ScheduledWorkoutLinker(
    FitSyncDbContext dbContext,
    IScheduledWorkoutMatcher matcher,
    ILogger<ScheduledWorkoutLinker> logger
) : IScheduledWorkoutLinker
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IScheduledWorkoutMatcher matcher = matcher;
    private readonly ILogger<ScheduledWorkoutLinker> logger = logger;

    public async Task<Guid?> LinkAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation("Linking scheduled workout for activity {ActivityId}.", activityId);

        Activity? activity = await this.dbContext.Activities.FirstOrDefaultAsync(
            a => a.Id == activityId,
            cancellationToken
        );

        if (activity is null)
        {
            this.logger.LogWarning("Activity {ActivityId} not found for linking.", activityId);
            return null;
        }

        if (activity.ScheduledWorkoutId is not null)
        {
            this.logger.LogInformation(
                "Activity {ActivityId} is already linked to scheduled workout {ScheduledWorkoutId}.",
                activityId,
                activity.ScheduledWorkoutId
            );
            return activity.ScheduledWorkoutId;
        }

        ScheduledWorkout? match = await this.matcher.MatchAsync(activity, cancellationToken);

        if (match is null)
        {
            this.logger.LogInformation(
                "No scheduled workout to link to activity {ActivityId}.",
                activityId
            );
            return null;
        }

        Guid? scheduledWorkoutId = match.Id;

        int updated = await this.dbContext.Activities.Where(
            a => a.Id == activityId && a.ScheduledWorkoutId == null
        )
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(a => a.ScheduledWorkoutId, scheduledWorkoutId)
                        .SetProperty(a => a.UpdatedAt, DateTime.UtcNow),
                cancellationToken
            );

        if (updated == 0)
        {
            this.logger.LogWarning(
                "Activity {ActivityId} was linked concurrently, leaving scheduled workout {ScheduledWorkoutId} untouched.",
                activityId,
                scheduledWorkoutId
            );
            return null;
        }

        this.logger.LogInformation(
            "Linked activity {ActivityId} to scheduled workout {ScheduledWorkoutId}.",
            activityId,
            scheduledWorkoutId
        );

        return scheduledWorkoutId;
    }
}
