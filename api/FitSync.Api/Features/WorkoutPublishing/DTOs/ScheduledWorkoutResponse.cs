namespace FitSync.Api.Features.WorkoutPublishing.DTOs;

public record ScheduledWorkoutResponse(
    Guid Id,
    Guid WorkoutId,
    string WorkoutName,
    int Sport,
    DateOnly ScheduledDate,
    DateTime CreatedAt,
    int? PlannedDurationSeconds,
    Guid? LinkedActivityId,
    List<ScheduledWorkoutPublicationResponse> Publications
);
