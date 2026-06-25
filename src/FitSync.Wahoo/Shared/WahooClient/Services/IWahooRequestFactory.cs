namespace FitSync.Wahoo.Shared.WahooClient.Services;

using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Wahoo.Shared.WahooClient.DTOs;

public interface IWahooRequestFactory
{
    HttpRequestMessage BuildFetchWorkoutsRequest(Integration integration, string url);
    HttpRequestMessage BuildPublishPlanRequest(
        Integration integration,
        WahooPlanDto plan,
        string externalId
    );
    HttpRequestMessage BuildUpdatePlanRequest(
        Integration integration,
        long planId,
        WahooPlanDto plan
    );
    HttpRequestMessage BuildScheduleWorkoutRequest(
        Integration integration,
        long planId,
        string name,
        DateOnly scheduledDate,
        int durationMinutes
    );

    HttpRequestMessage BuildRescheduleWorkoutRequest(
        Integration integration,
        long workoutId,
        DateOnly newDate
    );
}
