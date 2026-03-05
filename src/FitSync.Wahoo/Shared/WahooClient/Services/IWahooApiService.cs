namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Database.Models;
using FitSync.Wahoo.Shared.WahooClient.DTOs;

public interface IWahooApiService
{
    Task<List<WahooWorkoutDto>> FetchWorkoutsAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );
}
