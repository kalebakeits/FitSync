namespace FitSync.Shared.Features.WorkoutPublisher.Services;

public interface IScheduledWorkoutPushService
{
    Task<bool> PushPendingAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        int maxAttempts,
        CancellationToken cancellationToken = default
    );
}
