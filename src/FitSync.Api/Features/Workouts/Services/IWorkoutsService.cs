namespace FitSync.Api.Features.Workouts.Services;

using FitSync.Api.Features.Workouts.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutsService
{
    Task<PaginatedWorkoutsResponse> GetWorkoutsAsync(
        Guid userId,
        string? search,
        List<string>? tags,
        int limit,
        int offset
    );
    Task<WorkoutResponse> GetWorkoutByIdAsync(Guid userId, Guid workoutId);
    Task<WorkoutResponse> CreateWorkoutAsync(Guid userId, WorkoutSchema schema);
    Task<WorkoutResponse> UpdateWorkoutAsync(
        Guid userId,
        Guid workoutId,
        UpdateWorkoutRequest request
    );
    Task DeleteWorkoutAsync(Guid userId, Guid workoutId);
    Task<byte[]> DownloadWorkoutAsync(Guid userId, Guid workoutId);
}
