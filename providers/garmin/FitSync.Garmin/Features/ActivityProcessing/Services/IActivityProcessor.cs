namespace FitSync.Garmin.Features.ActivityProcessing.Services;

public interface IActivityProcessor
{
    Task ClaimAndProcessActivityAsync(
        Guid activityId,
        string instanceId,
        CancellationToken cancellationToken
    );

    Task ReclaimAndProcessActivityAsync(
        Guid activityId,
        string instanceId,
        DateTime orphanCutoff,
        CancellationToken cancellationToken
    );
}
