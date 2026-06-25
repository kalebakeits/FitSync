namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Wahoo.Shared.WahooClient.DTOs;

public interface IWahooApiService
{
    Task<List<WahooWorkoutDto>> FetchWorkoutsAsync(
        Integration integration,
        int lookbackDays,
        CancellationToken cancellationToken = default
    );

    Task<long> PublishPlanAsync(
        Integration integration,
        WahooPlanDto plan,
        string externalId,
        CancellationToken cancellationToken = default
    );

    Task UpdatePlanAsync(
        Integration integration,
        long planId,
        WahooPlanDto plan,
        CancellationToken cancellationToken = default
    );

    Task<long> ScheduleWorkoutAsync(
        Integration integration,
        long planId,
        string name,
        DateOnly scheduledDate,
        int durationMinutes,
        CancellationToken cancellationToken = default
    );

    Task RescheduleWorkoutAsync(
        Integration integration,
        long workoutId,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    );
}
