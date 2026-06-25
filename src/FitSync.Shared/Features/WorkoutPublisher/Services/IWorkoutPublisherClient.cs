namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutPublisherClient
{
    string ServiceType { get; }

    Task<string> PublishAsync(
        Integration integration,
        WorkoutSchema schema,
        string externalId,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    );

    Task RescheduleAsync(
        Integration integration,
        string serviceMetadata,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    );
}
