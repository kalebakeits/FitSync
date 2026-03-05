namespace FitSync.Zwift.Shared.ZwiftClient.Services;

using FitSync.Shared.Features.Fetcher.DTOs;
using FitSync.Zwift.Shared.ZwiftClient.DTOs;

public interface IZwiftActivityProcessor
{
    Task<List<FetchedActivity>> ProcessActivitiesAsync(
        ZwiftActivityDto[] activities,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );
}
