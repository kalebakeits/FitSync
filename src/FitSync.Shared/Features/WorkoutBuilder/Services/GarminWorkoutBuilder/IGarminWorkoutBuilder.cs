namespace FitSync.Shared.Features.WorkoutBuilder.Services.GarminWorkoutBuilder;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IGarminWorkoutBuilder
{
    GarminWorkoutDto Build(WorkoutSchema schema);
}
