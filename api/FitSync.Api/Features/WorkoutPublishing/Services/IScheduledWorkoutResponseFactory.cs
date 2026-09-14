namespace FitSync.Api.Features.WorkoutPublishing.Services;

using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Database.Models;

public interface IScheduledWorkoutResponseFactory
{
    Task<List<ScheduledWorkoutResponse>> BuildManyAsync(
        IQueryable<ScheduledWorkout> scheduledWorkouts,
        CancellationToken cancellationToken = default
    );

    Task<ScheduledWorkoutResponse> BuildAsync(
        ScheduledWorkout scheduledWorkout,
        CancellationToken cancellationToken = default
    );
}
