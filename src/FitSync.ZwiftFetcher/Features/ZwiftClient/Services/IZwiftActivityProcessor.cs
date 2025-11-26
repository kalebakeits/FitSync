namespace FitSync.ZwiftFetcher.Features.ZwiftClient.Services;

using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.ZwiftFetcher.Features.ZwiftClient.DTOs;

public interface IZwiftActivityProcessor
{
    Task<List<FetchedActivity>> ProcessActivitiesAsync(
        ZwiftActivityDto[] activities,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );
}
