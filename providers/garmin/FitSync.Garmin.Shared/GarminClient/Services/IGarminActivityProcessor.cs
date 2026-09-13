namespace FitSync.Garmin.Shared.GarminClient.Services;

using FitSync.Database.Models;
using FitSync.Garmin.Shared.GarminClient.DTOs;
using FitSync.Shared.Features.Fetcher.DTOs;

public interface IGarminActivityProcessor
{
    Task<List<FetchedActivity>> ProcessActivitiesAsync(
        Integration integration,
        GarminActivityDto[] activities,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );
}
