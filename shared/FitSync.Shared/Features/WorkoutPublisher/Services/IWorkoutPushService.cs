namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutPushService
{
    Task<string?> PushAsync(
        Guid userId,
        string serviceType,
        Guid scheduledWorkoutId,
        WorkoutSchema schema,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    );

    Task RescheduleAsync(
        Guid userId,
        string serviceType,
        string serviceMetadata,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        Guid userId,
        string serviceType,
        string serviceMetadata,
        CancellationToken cancellationToken = default
    );
}
