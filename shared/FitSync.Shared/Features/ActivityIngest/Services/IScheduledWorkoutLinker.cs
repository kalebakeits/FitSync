namespace FitSync.Shared.Features.ActivityIngest.Services;

public interface IScheduledWorkoutLinker
{
    Task<Guid?> LinkAsync(Guid activityId, CancellationToken cancellationToken = default);
}
