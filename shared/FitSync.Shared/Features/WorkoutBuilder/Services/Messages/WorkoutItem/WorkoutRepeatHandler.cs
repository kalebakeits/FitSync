namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class WorkoutRepeatHandler : IWorkoutRepeatHandler
{
    public void Expand(
        WorkoutItem.Repeat repeat,
        List<WorkoutStepMesg> accumulator,
        IWorkoutItemResolver resolver,
        Sport sport
    )
    {
        int blockStartIndex = accumulator.Count;

        foreach (WorkoutItem child in repeat.Steps)
            resolver.Resolve(child, accumulator, sport);

        WorkoutStepMesg repeatMesg = new();
        repeatMesg.SetMessageIndex((ushort)accumulator.Count);
        repeatMesg.SetDurationType(WktStepDuration.RepeatUntilStepsCmplt);
        repeatMesg.SetDurationValue((uint)blockStartIndex);
        repeatMesg.SetTargetType(WktStepTarget.Open);
        repeatMesg.SetTargetValue(repeat.RepeatCount);
        accumulator.Add(repeatMesg);
    }
}
