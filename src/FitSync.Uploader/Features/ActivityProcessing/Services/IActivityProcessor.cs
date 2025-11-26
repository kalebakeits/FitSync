namespace FitSync.Uploader.Features.ActivityProcessing.Services;

public interface IActivityProcessor
{
    Task ClaimAndProcessActivityAsync(
        Guid activityId,
        string instanceId,
        CancellationToken cancellationToken
    );

    Task ProcessActivityAsync(Guid activityId, CancellationToken cancellationToken);
}
