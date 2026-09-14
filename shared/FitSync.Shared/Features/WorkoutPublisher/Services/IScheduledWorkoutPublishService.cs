namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IScheduledWorkoutPublishService
{
    Task PublishAsync(
        Guid userId,
        Guid workoutId,
        string? serviceType,
        WorkoutSchema schema,
        DateOnly scheduledDate,
        Guid? scheduledWorkoutId = null,
        DateTime? pendingPublishAt = null,
        CancellationToken cancellationToken = default
    );
}
