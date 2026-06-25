namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class WorkoutSwimStepHandler : IWorkoutSwimStepHandler
{
    public WorkoutStepMesg Build(WorkoutItem.SwimStep step, int messageIndex)
    {
        WorkoutStepMesg mesg = new();
        mesg.SetMessageIndex((ushort)messageIndex);

        if (step.Name is not null)
            mesg.SetWktStepName(step.Name);

        mesg.SetIntensity(step.Intensity);

        if (step.Intensity == Intensity.Rest)
        {
            mesg.SetDurationType(WktStepDuration.Open);
            mesg.SetTargetType(WktStepTarget.Open);
            return mesg;
        }

        mesg.SetDurationType(WktStepDuration.Distance);
        mesg.SetDurationDistance(step.Distance);
        mesg.SetTargetType(WktStepTarget.SwimStroke);
        mesg.SetTargetStrokeType((byte)step.SwimStroke);

        if (step.Equipment.HasValue)
            mesg.SetEquipment(step.Equipment);

        return mesg;
    }
}
