namespace FitSync.Zwift.Fetcher.Features.ZwiftClient.Services;

using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Zwift.Fetcher.Features.ZwiftClient.DTOs;

public interface IZwiftActivityProcessor
{
    Task<List<FetchedActivity>> ProcessActivitiesAsync(
        ZwiftActivityDto[] activities,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );
}
