namespace FitSync.Api.Features.WorkoutPublishing.DTOs;

public record PublishWorkoutRequest(string? ServiceType, DateOnly ScheduledDate);
