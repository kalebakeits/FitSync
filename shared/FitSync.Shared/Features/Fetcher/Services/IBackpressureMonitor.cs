namespace FitSync.Shared.Features.Fetcher.Services;

public interface IBackpressureMonitor
{
    Task<bool> ShouldFetchAsync(CancellationToken cancellationToken = default);
}
