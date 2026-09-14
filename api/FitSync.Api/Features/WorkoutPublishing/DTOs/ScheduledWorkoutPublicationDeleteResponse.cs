namespace FitSync.Api.Features.WorkoutPublishing.DTOs;

public record ScheduledWorkoutPublicationDeleteResponse(
    string ServiceType,
    bool Succeeded,
    string? Error
);
