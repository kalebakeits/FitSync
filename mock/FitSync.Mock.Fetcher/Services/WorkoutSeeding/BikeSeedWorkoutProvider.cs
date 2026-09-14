namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

using Dynastream.Fit;
using FitSync.Mock.Fetcher.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class BikeSeedWorkoutProvider : ISeedWorkoutProvider
{
    public SeedWorkout[] GetWorkouts() =>
    [
        new(
            new WorkoutSchema.Default(
                "2x20min Threshold",
                Sport.Cycling,
                SubSport.Road,
                [
                    SeedStep.Zone(900, WktStepTarget.Power, 2, Intensity.Warmup, "Warm Up"),
                    SeedStep.Zone(1200, WktStepTarget.Power, 4, Intensity.Interval, "Threshold Interval 1"),
                    SeedStep.Zone(300, WktStepTarget.Power, 1, Intensity.Recovery, "Recovery"),
                    SeedStep.Zone(1200, WktStepTarget.Power, 4, Intensity.Interval, "Threshold Interval 2"),
                    SeedStep.Zone(600, WktStepTarget.Power, 1, Intensity.Cooldown, "Cool Down"),
                ],
                true
            ),
            [0]
        ),
        new(
            new WorkoutSchema.Default(
                "5min 3x8 Threshold",
                Sport.Cycling,
                SubSport.Road,
                [
                    SeedStep.Zone(300, WktStepTarget.Power, 2, Intensity.Warmup, "Warm Up"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Zone(480, WktStepTarget.Power, 4, Intensity.Interval, "Threshold"),
                            SeedStep.Zone(120, WktStepTarget.HeartRate, 1, Intensity.Rest, "Recovery"),
                        ],
                        3
                    ),
                    SeedStep.Zone(300, WktStepTarget.HeartRate, 1, Intensity.Cooldown, "Cool Down"),
                ],
                false
            ),
            [4]
        ),
        new(
            new WorkoutSchema.Default(
                "Bike Threshold 5x8",
                Sport.Cycling,
                SubSport.VirtualActivity,
                [
                    SeedStep.Range(900, WktStepTarget.Power, 140, 180, Intensity.Warmup, "Warmup"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Range(480, WktStepTarget.Power, 260, 280, Intensity.Interval, "Threshold"),
                            SeedStep.Range(180, WktStepTarget.Power, 120, 160, Intensity.Recovery, "Recovery"),
                        ],
                        5
                    ),
                    SeedStep.Range(600, WktStepTarget.Power, 120, 160, Intensity.Cooldown, "Cooldown"),
                ],
                true
            ),
            [5]
        ),
        new(
            new WorkoutSchema.Default(
                "Endurance Z2 Ride",
                Sport.Cycling,
                SubSport.Road,
                [
                    SeedStep.Zone(900, WktStepTarget.Power, 2, Intensity.Warmup, "Warm Up"),
                    SeedStep.Zone(5400, WktStepTarget.Power, 2, Intensity.Active, "Endurance Z2"),
                    SeedStep.Zone(600, WktStepTarget.Power, 1, Intensity.Cooldown, "Cool Down"),
                ],
                true
            ),
            [2]
        ),
        new(
            new WorkoutSchema.Default(
                "Race Pace Bike 3x10",
                Sport.Cycling,
                SubSport.Road,
                [
                    SeedStep.Range(900, WktStepTarget.Power, 135, 190, Intensity.Warmup, "Warm Up"),
                    SeedStep.Range(60, WktStepTarget.Cadence, 100, 110, Intensity.Active, "High Cadence"),
                    SeedStep.Open(60, Intensity.Recovery, "Easy"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Range(600, WktStepTarget.Power, 215, 225, Intensity.Interval, "Race Pace"),
                            SeedStep.Open(300, Intensity.Recovery, "Easy Spin"),
                        ],
                        3
                    ),
                    SeedStep.Open(600, Intensity.Cooldown, "Cool Down"),
                ],
                false
            ),
            [6]
        ),
        new(
            new WorkoutSchema.Default(
                "Recovery Spin",
                Sport.Cycling,
                SubSport.IndoorCycling,
                [
                    SeedStep.Zone(300, WktStepTarget.Power, 1, Intensity.Warmup, "Warm Up"),
                    SeedStep.Zone(2400, WktStepTarget.Power, 1, Intensity.Recovery, "Easy Spin"),
                ],
                true
            ),
            []
        ),
        new(
            new WorkoutSchema.Default(
                "VO2 Max 5x3",
                Sport.Cycling,
                SubSport.Road,
                [
                    SeedStep.Zone(900, WktStepTarget.Power, 2, Intensity.Warmup, "Warm Up"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Zone(180, WktStepTarget.Power, 5, Intensity.Interval, "VO2 Effort"),
                            SeedStep.Zone(180, WktStepTarget.Power, 1, Intensity.Recovery, "Easy Spin"),
                        ],
                        5
                    ),
                    SeedStep.Zone(600, WktStepTarget.Power, 1, Intensity.Cooldown, "Cool Down"),
                ],
                true
            ),
            []
        ),
    ];
}
