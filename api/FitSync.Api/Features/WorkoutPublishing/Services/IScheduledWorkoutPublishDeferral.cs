namespace FitSync.Api.Features.WorkoutPublishing.Services;

using FitSync.Database.Models;

public interface IScheduledWorkoutPublishDeferral
{
    DateTime ComputePendingPublishAt(DateOnly scheduledDate);

    void Defer(ScheduledWorkout scheduledWorkout);
}
