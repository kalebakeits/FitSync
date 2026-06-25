namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutRepeatHandler
{
    void Expand(
        WorkoutItem.Repeat repeat,
        List<WorkoutStepMesg> accumulator,
        IWorkoutItemResolver resolver,
        Sport sport
    );
}
