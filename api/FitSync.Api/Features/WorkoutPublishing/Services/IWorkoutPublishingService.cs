namespace FitSync.Api.Features.WorkoutPublishing.Services;

using FitSync.Api.Features.WorkoutPublishing.DTOs;

public interface IWorkoutPublishingService
{
    Task PublishAsync(
        Guid userId,
        Guid workoutId,
        string? serviceType,
        DateOnly scheduledDate,
        Guid? scheduledWorkoutId = null,
        CancellationToken cancellationToken = default
    );

    Task<List<ScheduledWorkoutResponse>> GetScheduledWorkoutsAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default
    );

    Task<ScheduledWorkoutResponse> MoveScheduledWorkoutAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    );

    Task<DeleteScheduledWorkoutResponse> DeleteScheduledWorkoutAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        bool force = false,
        CancellationToken cancellationToken = default
    );
}
