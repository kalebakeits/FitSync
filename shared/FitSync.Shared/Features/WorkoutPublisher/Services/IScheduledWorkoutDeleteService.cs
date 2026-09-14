namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Shared.Features.WorkoutPublisher.DTOs;

public interface IScheduledWorkoutDeleteService
{
    Task<ScheduledWorkoutDeletionResult> DeleteAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        bool force = false,
        CancellationToken cancellationToken = default
    );
}
