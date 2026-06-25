namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class WorkoutItemResolver(
    IWorkoutStepHandler stepHandler,
    IWorkoutSwimStepHandler swimStepHandler,
    IWorkoutRepeatHandler repeatHandler
) : IWorkoutItemResolver
{
    private readonly IWorkoutStepHandler stepHandler = stepHandler;
    private readonly IWorkoutSwimStepHandler swimStepHandler = swimStepHandler;
    private readonly IWorkoutRepeatHandler repeatHandler = repeatHandler;

    public void Resolve(WorkoutItem item, List<WorkoutStepMesg> accumulator, Sport sport)
    {
        item.Match(
            step => accumulator.Add(this.stepHandler.Build(step, accumulator.Count, sport)),
            swimStep => accumulator.Add(this.swimStepHandler.Build(swimStep, accumulator.Count)),
            repeat => this.repeatHandler.Expand(repeat, accumulator, this, sport)
        );
    }
}
