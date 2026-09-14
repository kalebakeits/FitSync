namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

using Dynastream.Fit;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public static class SeedStep
{
    private const uint MillisecondsPerSecond = 1_000;

    public static WorkoutItem Zone(
        int seconds,
        WktStepTarget target,
        int zone,
        Intensity intensity,
        string name
    ) =>
        new WorkoutItem.Step(
            WktStepDuration.Time,
            Convert.ToUInt32(seconds) * MillisecondsPerSecond,
            target,
            null,
            null,
            zone,
            intensity,
            name
        );

    public static WorkoutItem Range(
        int seconds,
        WktStepTarget target,
        uint low,
        uint high,
        Intensity intensity,
        string name
    ) =>
        new WorkoutItem.Step(
            WktStepDuration.Time,
            Convert.ToUInt32(seconds) * MillisecondsPerSecond,
            target,
            low,
            high,
            null,
            intensity,
            name
        );

    public static WorkoutItem Open(int seconds, Intensity intensity, string name) =>
        new WorkoutItem.Step(
            WktStepDuration.Time,
            Convert.ToUInt32(seconds) * MillisecondsPerSecond,
            WktStepTarget.Open,
            null,
            null,
            null,
            intensity,
            name
        );

    public static WorkoutItem Swim(
        float distance,
        SwimStroke stroke,
        WorkoutEquipment? equipment,
        Intensity intensity,
        string name
    ) => new WorkoutItem.SwimStep(distance, stroke, equipment, intensity, name);
}
