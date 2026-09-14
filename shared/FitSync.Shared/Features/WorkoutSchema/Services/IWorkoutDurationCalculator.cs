namespace FitSync.Shared.Features.WorkoutSchema.Services;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutDurationCalculator
{
    int? CalculateSeconds(WorkoutSchema schema);
    int CalculateSeconds(WorkoutItem[] items);
}
