namespace FitSync.Api.Features.WorkoutPublishing.Services;

public interface IPendingPublishSweeper
{
    Task<int> SweepAsync(CancellationToken cancellationToken = default);
}
