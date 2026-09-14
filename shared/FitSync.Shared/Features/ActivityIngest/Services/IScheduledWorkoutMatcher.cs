namespace FitSync.Shared.Features.ActivityIngest.Services;

using FitSync.Database.Models;

public interface IScheduledWorkoutMatcher
{
    Task<ScheduledWorkout?> MatchAsync(
        Activity activity,
        CancellationToken cancellationToken = default
    );
}
