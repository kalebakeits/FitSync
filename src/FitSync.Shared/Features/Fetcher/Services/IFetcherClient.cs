namespace FitSync.Shared.Features.Fetcher.Services;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;

public interface IFetcherClient
{
    Task<List<FetchedActivity>> GetActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );
}
