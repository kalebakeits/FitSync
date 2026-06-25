namespace FitSync.Shared.Features.WorkoutBuilder.Services.WahooWorkoutBuilder;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class WahooWorkoutBuilder : IWahooWorkoutBuilder
{
    private const string Version = "1.0.0";
    private const int OpenDurationSeconds = 3600;

    public WahooPlanDto Build(WorkoutSchema schema)
    {
        return schema.Match<WahooPlanDto>(
            @default => this.BuildFromMeta(@default),
            poolSwim => this.BuildFromMeta(poolSwim)
        );
    }

    private WahooPlanDto BuildFromMeta(IWorkoutMeta meta)
    {
        WahooPlanHeader header =
            new(
                Name: meta.Name,
                Description: string.Empty,
                Version: Version,
                WorkoutTypeFamily: MapWorkoutTypeFamily(meta.Sport),
                WorkoutTypeLocation: 0,
                Ftp: 0
            );

        List<WahooPlanInterval> intervals = [];
        FlattenItems(meta.Items, intervals);

        return new WahooPlanDto(header, [.. intervals]);
    }

    private static void FlattenItems(WorkoutItem[] items, List<WahooPlanInterval> output)
    {
        foreach (WorkoutItem item in items)
        {
            item.Match(
                step => output.Add(BuildInterval(step)),
                _ => output.Add(BuildSwimInterval()),
                repeat =>
                {
                    for (int i = 0; i < repeat.RepeatCount; i++)
                        FlattenItems(repeat.Steps, output);
                }
            );
        }
    }

    private static WahooPlanInterval BuildSwimInterval() =>
        new(OpenTarget, "time", OpenDurationSeconds, "active");

    private static WahooPlanInterval BuildInterval(WorkoutItem.Step step)
    {
        int exitValue =
            step.DurationType == WktStepDuration.Time && step.DurationValue.HasValue
                ? Convert.ToInt32(step.DurationValue.Value / 1000)
                : OpenDurationSeconds;

        WahooPlanTarget[] targets = BuildTargets(step);

        return new WahooPlanInterval(
            Targets: targets,
            ExitTriggerType: "time",
            ExitTriggerValue: exitValue,
            IntensityType: MapIntensity(step.Intensity),
            Name: step.Name
        );
    }

    private static readonly WahooPlanTarget[] OpenTarget = [new WahooPlanTarget("watts", 0, 0)];

    private static WahooPlanTarget[] BuildTargets(WorkoutItem.Step step)
    {
        if (step.TargetType == WktStepTarget.Invalid || step.TargetType == WktStepTarget.Open)
            return OpenTarget;

        string? targetType = MapTargetType(step.TargetType);
        if (targetType is null)
            return OpenTarget;

        double low = step.TargetLow.HasValue ? step.TargetLow.Value : 0;
        double high = step.TargetHigh.HasValue ? step.TargetHigh.Value : low;

        return [new WahooPlanTarget(targetType, low, high)];
    }

    private static string MapIntensity(Intensity intensity) =>
        intensity switch
        {
            Intensity.Warmup => "wu",
            Intensity.Cooldown => "cd",
            Intensity.Rest => "rest",
            Intensity.Recovery => "recover",
            _ => "active",
        };

    private static string? MapTargetType(WktStepTarget target) =>
        target switch
        {
            WktStepTarget.Power
            or WktStepTarget.Power3s
            or WktStepTarget.Power10s
            or WktStepTarget.Power30s
            or WktStepTarget.PowerLap
                => "watts",
            WktStepTarget.HeartRate => "hr",
            WktStepTarget.Cadence => "rpm",
            WktStepTarget.Speed or WktStepTarget.SpeedLap => "speed",
            _ => null,
        };

    private static int MapWorkoutTypeFamily(Sport sport) =>
        sport switch
        {
            Sport.Cycling => 0,
            Sport.Running => 1,
            Sport.Swimming => 4,
            _
                => throw new ArgumentOutOfRangeException(
                    nameof(sport),
                    sport,
                    $"No Wahoo workout type family mapped for sport {sport}."
                ),
        };
}
