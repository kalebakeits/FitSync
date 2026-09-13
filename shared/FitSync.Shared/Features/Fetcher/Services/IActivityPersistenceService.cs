namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Shared.Features.Fetcher.DTOs;

public interface IActivityPersistenceService
{
    Task SaveAndPublishAsync(
        Guid userId,
        FetchedActivity fetchedActivity,
        CancellationToken cancellationToken
    );
}
