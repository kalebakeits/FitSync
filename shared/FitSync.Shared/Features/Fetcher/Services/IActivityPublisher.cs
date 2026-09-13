namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database.Models;

public interface IActivityPublisher
{
    Task PublishActivityFetchedAsync(Activity activity, CancellationToken cancellationToken);
}
