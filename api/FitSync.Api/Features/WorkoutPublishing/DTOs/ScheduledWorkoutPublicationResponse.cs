namespace FitSync.Api.Features.WorkoutPublishing.DTOs;

using FitSync.Database.Enums;

public record ScheduledWorkoutPublicationResponse(
    string ServiceType,
    DateTime PublishedAt,
    PublicationStatus Status,
    string? LastError
);
