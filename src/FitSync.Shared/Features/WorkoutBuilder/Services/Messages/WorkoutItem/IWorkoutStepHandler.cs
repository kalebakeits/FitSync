namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutStepHandler
{
    WorkoutStepMesg Build(WorkoutItem.Step step, int messageIndex, Sport sport);
}
