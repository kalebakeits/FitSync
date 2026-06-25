namespace FitSync.Shared.Features.WorkoutBuilder.Services.Messages.WorkoutItem;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class WorkoutStepHandler : IWorkoutStepHandler
{
    private uint ApplyTargetOffset(WktStepTarget targetType, uint value) =>
        targetType switch
        {
            WktStepTarget.HeartRate => value + WorkoutHr.BpmOffset,
            WktStepTarget.Power
            or WktStepTarget.Power3s
            or WktStepTarget.Power10s
            or WktStepTarget.Power30s
            or WktStepTarget.PowerLap
                => value + WorkoutPower.WattsOffset,
            WktStepTarget.Speed or WktStepTarget.SpeedLap => value * 1000,
            _ => value,
        };

    public WorkoutStepMesg Build(WorkoutItem.Step step, int messageIndex, Sport sport)
    {
        WorkoutStepMesg mesg = new();
        mesg.SetMessageIndex((ushort)messageIndex);

        if (step.Name is not null)
            mesg.SetWktStepName(step.Name);

        mesg.SetIntensity(step.Intensity);
        mesg.SetDurationType(step.DurationType);

        if (step.DurationValue.HasValue)
            mesg.SetDurationValue(step.DurationValue);

        if (step.TargetType == WktStepTarget.Invalid)
            return mesg;

        mesg.SetTargetType(step.TargetType);

        uint low = step.TargetLow ?? 0;
        uint high = step.TargetHigh ?? low;

        if (low > 0 || high > 0)
        {
            mesg.SetTargetValue(0);
            mesg.SetCustomTargetValueLow(this.ApplyTargetOffset(step.TargetType, low));
            mesg.SetCustomTargetValueHigh(this.ApplyTargetOffset(step.TargetType, high));
        }
        else
        {
            mesg.SetTargetValue(step.TargetLow ?? 0);
            mesg.SetCustomTargetValueLow(0);
            mesg.SetCustomTargetValueHigh(0);
        }

        return mesg;
    }
}
