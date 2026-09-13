namespace FitSync.Shared.Features.WorkoutBuilder.Services.Writer;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutWriter
{
    byte[] BuildWorkout(WorkoutSchema schema);
}
