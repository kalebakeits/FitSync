namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutItemResolver
{
    void Resolve(WorkoutItem item, List<WorkoutStepMesg> accumulator, Sport sport);
}
