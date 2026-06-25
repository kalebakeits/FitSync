namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public interface IWorkoutSwimStepHandler
{
    WorkoutStepMesg Build(WorkoutItem.SwimStep step, int messageIndex);
}
