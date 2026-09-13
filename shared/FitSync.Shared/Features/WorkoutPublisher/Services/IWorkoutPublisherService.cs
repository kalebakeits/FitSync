namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutPublisherService
{
    Task PublishAsync(
        Guid userId,
        Guid workoutId,
        string? serviceType,
        WorkoutSchema schema,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    );

    Task RescheduleAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    );
}
