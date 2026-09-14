namespace FitSync.Shared.Features.WorkoutPublisher.Services;

public interface IWorkoutPushTargetResolver
{
    Task<WorkoutPushTarget> ResolveAsync(
        Guid userId,
        string serviceType,
        CancellationToken cancellationToken = default
    );
}
