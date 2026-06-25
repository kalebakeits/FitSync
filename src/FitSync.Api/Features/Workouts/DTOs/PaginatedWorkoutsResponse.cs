namespace FitSync.Api.Features.Workouts.DTOs;

public record PaginatedWorkoutsResponse(
    List<WorkoutResponse> Items,
    int Total,
    int Limit,
    int Offset
);
