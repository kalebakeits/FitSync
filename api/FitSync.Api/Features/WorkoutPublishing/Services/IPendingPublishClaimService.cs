namespace FitSync.Api.Features.WorkoutPublishing.Services;

public interface IPendingPublishClaimService
{
    Task<bool> TryClaimAsync(
        Guid scheduledWorkoutId,
        DateTime now,
        DateTime staleClaimCutoff,
        CancellationToken cancellationToken = default
    );

    Task<bool> CompleteAsync(
        Guid scheduledWorkoutId,
        DateOnly publishedDate,
        CancellationToken cancellationToken = default
    );

    Task ReleaseAsync(Guid scheduledWorkoutId, CancellationToken cancellationToken = default);
}
