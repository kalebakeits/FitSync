namespace FitSync.Shared.Features.ActivityIngest.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.ActivityIngest.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ScheduledWorkoutMatcher(
    FitSyncDbContext dbContext,
    ISportCategoryResolver sportCategoryResolver,
    ILogger<ScheduledWorkoutMatcher> logger
) : IScheduledWorkoutMatcher
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly ISportCategoryResolver sportCategoryResolver = sportCategoryResolver;
    private readonly ILogger<ScheduledWorkoutMatcher> logger = logger;

    public async Task<ScheduledWorkout?> MatchAsync(
        Activity activity,
        CancellationToken cancellationToken = default
    )
    {
        DateOnly activityDate = DateOnly.FromDateTime(activity.ActivityDate);
        SportCategory category = this.sportCategoryResolver.Resolve(activity.Sport);

        this.logger.LogInformation(
            "Matching scheduled workout for activity {ActivityId} on {ActivityDate} in category {Category}.",
            activity.Id,
            activityDate,
            category
        );

        List<Guid> linkedScheduledWorkoutIds = await this.dbContext.Activities.Where(
            a => a.UserId == activity.UserId && a.ScheduledWorkoutId != null
        )
            .Select(a => a.ScheduledWorkoutId!.Value)
            .ToListAsync(cancellationToken);

        List<ScheduledWorkout> candidates = await this.dbContext.ScheduledWorkouts.Include(
            s => s.Workout
        )
            .Where(
                s =>
                    s.UserId == activity.UserId
                    && s.ScheduledDate == activityDate
                    && !linkedScheduledWorkoutIds.Contains(s.Id)
            )
            .ToListAsync(cancellationToken);

        List<ScheduledWorkout> sameCategory = candidates
            .Where(s => this.sportCategoryResolver.Resolve(s.Workout.Sport) == category)
            .ToList();

        this.logger.LogInformation(
            "Found {CandidateCount} linkable scheduled workouts for activity {ActivityId}.",
            sameCategory.Count,
            activity.Id
        );

        if (sameCategory.Count == 0)
            return null;

        if (sameCategory.Count == 1)
            return sameCategory[0];

        return sameCategory
            .OrderBy(s =>
                s.PlannedDurationSeconds.HasValue && activity.DurationSeconds.HasValue
                    ? Math.Abs(s.PlannedDurationSeconds.Value - activity.DurationSeconds.Value)
                    : int.MaxValue
            )
            .ThenBy(s => s.CreatedAt)
            .First();
    }
}
