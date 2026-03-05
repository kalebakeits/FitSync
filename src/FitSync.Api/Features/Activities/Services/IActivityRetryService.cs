namespace FitSync.Api.Features.Activities.Services;

public interface IActivityRetryService
{
    Task RetryFailedAsync(Guid userId, Guid activityId, CancellationToken ct = default);
    Task PushToDestinationAsync(
        Guid userId,
        Guid activityId,
        string destinationServiceType,
        CancellationToken ct = default
    );
}
