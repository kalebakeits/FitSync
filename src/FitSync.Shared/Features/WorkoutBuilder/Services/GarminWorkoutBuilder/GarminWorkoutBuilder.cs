namespace FitSync.Shared.Features.WorkoutBuilder.Services.GarminWorkoutBuilder;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class GarminWorkoutBuilder : IGarminWorkoutBuilder
{
    private static readonly GarminEndCondition LapButton = new("lap.button", 1);
    private static readonly GarminEndCondition TimeCondition = new("time", 2);
    private static readonly GarminTargetType NoTarget = new("no.target", 1);
    private static readonly GarminTargetType PowerZoneTarget = new("power.zone", 2);
    private static readonly GarminTargetType HeartRateZoneTarget = new("heart.rate.zone", 4);
    private static readonly GarminTargetType CadenceTarget = new("cadence.zone", 3);
    private static readonly GarminTargetType SpeedTarget = new("speed.zone", 5);

    public GarminWorkoutDto Build(WorkoutSchema schema)
    {
        return schema.Match<GarminWorkoutDto>(
            @default => this.BuildFromMeta(@default),
            poolSwim => this.BuildFromMeta(poolSwim)
        );
    }

    private GarminWorkoutDto BuildFromMeta(IWorkoutMeta meta)
    {
        GarminSportType sportType = MapSportType(meta.Sport);
        int[] counter = [1];
        GarminWorkoutStep[] steps = BuildSteps(meta.Items, counter);
        GarminWorkoutSegment segment = new(1, sportType, steps);
        return new GarminWorkoutDto(meta.Name, sportType, [segment]);
    }

    private static GarminWorkoutStep[] BuildSteps(WorkoutItem[] items, int[] counter)
    {
        List<GarminWorkoutStep> steps = [];
        foreach (WorkoutItem item in items)
        {
            int order = counter[0]++;
            if (item is WorkoutItem.Step step)
                steps.Add(BuildExecutableStep(step, order));
            else if (item is WorkoutItem.SwimStep)
                steps.Add(BuildOpenExecutableStep(order));
            else if (item is WorkoutItem.Repeat repeat)
                steps.Add(BuildRepeatStep(repeat, order, counter));
        }
        return [.. steps];
    }

    private static GarminWorkoutStep BuildExecutableStep(WorkoutItem.Step step, int order)
    {
        GarminStepType stepType = MapStepType(step.Intensity);
        (GarminEndCondition endCondition, double? endValue) = MapEndCondition(step);
        (GarminTargetType targetType, double? low, double? high) = MapTarget(step);

        return new GarminWorkoutStep(
            Type: "ExecutableStepDTO",
            StepOrder: order,
            StepType: stepType,
            ChildStepId: null,
            EndCondition: endCondition,
            EndConditionValue: endValue,
            TargetType: targetType,
            TargetValueOne: low,
            TargetValueTwo: high,
            NumberOfIterations: null,
            WorkoutSteps: null,
            SmartRepeat: null
        );
    }

    private static GarminWorkoutStep BuildOpenExecutableStep(int order) =>
        new(
            Type: "ExecutableStepDTO",
            StepOrder: order,
            StepType: new GarminStepType("interval", 3),
            ChildStepId: null,
            EndCondition: LapButton,
            EndConditionValue: null,
            TargetType: NoTarget,
            TargetValueOne: null,
            TargetValueTwo: null,
            NumberOfIterations: null,
            WorkoutSteps: null,
            SmartRepeat: null
        );

    private static GarminWorkoutStep BuildRepeatStep(
        WorkoutItem.Repeat repeat,
        int order,
        int[] counter
    )
    {
        GarminWorkoutStep[] nested = BuildSteps(repeat.Steps, counter);
        return new GarminWorkoutStep(
            Type: "RepeatGroupDTO",
            StepOrder: order,
            StepType: new GarminStepType("repeat", 6),
            ChildStepId: order,
            EndCondition: null,
            EndConditionValue: null,
            TargetType: null,
            TargetValueOne: null,
            TargetValueTwo: null,
            NumberOfIterations: Convert.ToInt32(repeat.RepeatCount),
            WorkoutSteps: nested,
            SmartRepeat: false
        );
    }

    private static (GarminEndCondition, double?) MapEndCondition(WorkoutItem.Step step)
    {
        if (step.DurationType == WktStepDuration.Time && step.DurationValue.HasValue)
            return (TimeCondition, step.DurationValue.Value / 1000.0);
        if (step.DurationType == WktStepDuration.Distance && step.DurationValue.HasValue)
            return (new GarminEndCondition("distance", 3), step.DurationValue.Value / 100.0);
        return (LapButton, null);
    }

    private static (GarminTargetType, double?, double?) MapTarget(WorkoutItem.Step step)
    {
        if (step.TargetType == WktStepTarget.Invalid || step.TargetType == WktStepTarget.Open)
            return (NoTarget, null, null);

        GarminTargetType targetType = step.TargetType switch
        {
            WktStepTarget.Power
            or WktStepTarget.Power3s
            or WktStepTarget.Power10s
            or WktStepTarget.Power30s
            or WktStepTarget.PowerLap
                => PowerZoneTarget,
            WktStepTarget.HeartRate => HeartRateZoneTarget,
            WktStepTarget.Cadence => CadenceTarget,
            WktStepTarget.Speed or WktStepTarget.SpeedLap => SpeedTarget,
            _ => NoTarget,
        };

        if (targetType == NoTarget)
            return (NoTarget, null, null);

        double low = step.TargetLow ?? 0;
        double high = step.TargetHigh ?? low;
        return (targetType, low, high);
    }

    private static GarminStepType MapStepType(Intensity intensity) =>
        intensity switch
        {
            Intensity.Warmup => new GarminStepType("warmup", 1),
            Intensity.Cooldown => new GarminStepType("cooldown", 2),
            Intensity.Rest => new GarminStepType("rest", 4),
            Intensity.Recovery => new GarminStepType("rest", 4),
            _ => new GarminStepType("interval", 3),
        };

    private static GarminSportType MapSportType(Sport sport) =>
        sport switch
        {
            Sport.Running => new GarminSportType("running", 1),
            Sport.Swimming => new GarminSportType("swimming", 4),
            _ => new GarminSportType("cycling", 2),
        };
}
