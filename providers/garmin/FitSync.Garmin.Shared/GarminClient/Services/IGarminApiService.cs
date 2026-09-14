namespace FitSync.Garmin.Shared.GarminClient.Services;

using FitSync.Database.Models;
using FitSync.Garmin.Shared.GarminClient.DTOs;

public interface IGarminApiService
{
    Task<GarminActivityDto[]> FetchActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );
}
