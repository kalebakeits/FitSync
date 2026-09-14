namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

using Dynastream.Fit;
using FitSync.Mock.Fetcher.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class RunSeedWorkoutProvider : ISeedWorkoutProvider
{
    public SeedWorkout[] GetWorkouts() =>
    [
        new(
            new WorkoutSchema.Default(
                "40min Threshold Run",
                Sport.Running,
                SubSport.Street,
                [
                    SeedStep.Zone(600, WktStepTarget.HeartRate, 2, Intensity.Warmup, "Warm Up Easy Run"),
                    SeedStep.Zone(300, WktStepTarget.HeartRate, 3, Intensity.Active, "Steady Build"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Zone(300, WktStepTarget.HeartRate, 4, Intensity.Interval, "Threshold Effort"),
                            SeedStep.Zone(120, WktStepTarget.HeartRate, 2, Intensity.Recovery, "Easy Recovery Jog"),
                        ],
                        3
                    ),
                    SeedStep.Zone(300, WktStepTarget.HeartRate, 4, Intensity.Interval, "Sustained Threshold Push"),
                    SeedStep.Zone(600, WktStepTarget.HeartRate, 1, Intensity.Cooldown, "Cool Down Jog"),
                ],
                true
            ),
            [1]
        ),
        new(
            new WorkoutSchema.Default(
                "Easy Recovery Run",
                Sport.Running,
                SubSport.Street,
                [
                    SeedStep.Zone(300, WktStepTarget.HeartRate, 1, Intensity.Warmup, "Warm Up Jog"),
                    SeedStep.Zone(1800, WktStepTarget.HeartRate, 2, Intensity.Active, "Easy Aerobic"),
                    SeedStep.Zone(300, WktStepTarget.HeartRate, 1, Intensity.Cooldown, "Cool Down"),
                ],
                true
            ),
            [3]
        ),
        new(
            new WorkoutSchema.Default(
                "Hill Repeats 6x2",
                Sport.Running,
                SubSport.Trail,
                [
                    SeedStep.Zone(900, WktStepTarget.HeartRate, 2, Intensity.Warmup, "Warm Up"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Zone(120, WktStepTarget.HeartRate, 5, Intensity.Interval, "Hill Effort"),
                            SeedStep.Zone(120, WktStepTarget.HeartRate, 1, Intensity.Recovery, "Jog Down"),
                        ],
                        6
                    ),
                    SeedStep.Zone(600, WktStepTarget.HeartRate, 1, Intensity.Cooldown, "Cool Down"),
                ],
                true
            ),
            []
        ),
        new(
            new WorkoutSchema.Default(
                "Long Run Z2",
                Sport.Running,
                SubSport.Trail,
                [
                    SeedStep.Zone(600, WktStepTarget.HeartRate, 1, Intensity.Warmup, "Warm Up"),
                    SeedStep.Zone(4200, WktStepTarget.HeartRate, 2, Intensity.Active, "Long Aerobic"),
                    SeedStep.Zone(600, WktStepTarget.HeartRate, 1, Intensity.Cooldown, "Cool Down"),
                ],
                true
            ),
            [5]
        ),
    ];
}
