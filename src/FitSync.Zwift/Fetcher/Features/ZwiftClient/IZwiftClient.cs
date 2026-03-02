namespace FitSync.Zwift.Fetcher.Features.ZwiftClient;

using FitSync.Database.Models;
using FitSync.Shared.Features.Fetcher.DTOs;

public interface IZwiftClient
{
    Task<List<FetchedActivity>> GetActivitiesAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken
    );
}
