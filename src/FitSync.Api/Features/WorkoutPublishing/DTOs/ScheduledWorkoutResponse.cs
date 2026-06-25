namespace FitSync.Api.Features.WorkoutPublishing.DTOs;

public record ScheduledWorkoutResponse(
    Guid Id,
    Guid WorkoutId,
    string WorkoutName,
    int Sport,
    string? ServiceType,
    DateOnly ScheduledDate,
    DateTime CreatedAt
);
