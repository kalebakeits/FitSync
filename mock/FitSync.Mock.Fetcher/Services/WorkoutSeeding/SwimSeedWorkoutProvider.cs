namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

using Dynastream.Fit;
using FitSync.Mock.Fetcher.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public class SwimSeedWorkoutProvider : ISeedWorkoutProvider
{
    private const float PoolLengthMetres = 25f;

    public SeedWorkout[] GetWorkouts() =>
    [
        new(
            new WorkoutSchema.PoolSwim(
                "CSS Intervals 10x100",
                Sport.Swimming,
                SubSport.LapSwimming,
                [
                    SeedStep.Swim(300f, SwimStroke.Freestyle, null, Intensity.Warmup, "Warm Up"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Swim(100f, SwimStroke.Freestyle, null, Intensity.Interval, "CSS Effort"),
                            SeedStep.Swim(15f, SwimStroke.Freestyle, null, Intensity.Rest, "Rest"),
                        ],
                        10
                    ),
                    SeedStep.Swim(200f, SwimStroke.Freestyle, null, Intensity.Cooldown, "Cool Down"),
                ],
                true,
                PoolLengthMetres,
                DisplayMeasure.Metric
            ),
            [3]
        ),
        new(
            new WorkoutSchema.PoolSwim(
                "Swim Endurance 2000m",
                Sport.Swimming,
                SubSport.LapSwimming,
                [
                    SeedStep.Swim(400f, SwimStroke.Freestyle, null, Intensity.Warmup, "Warm Up"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Swim(300f, SwimStroke.Freestyle, null, Intensity.Active, "Aerobic"),
                            SeedStep.Swim(20f, SwimStroke.Freestyle, null, Intensity.Rest, "Rest"),
                        ],
                        5
                    ),
                    SeedStep.Swim(100f, SwimStroke.Freestyle, null, Intensity.Cooldown, "Cool Down"),
                ],
                true,
                PoolLengthMetres,
                DisplayMeasure.Metric
            ),
            [1]
        ),
        new(
            new WorkoutSchema.PoolSwim(
                "Technique and Drills 1500m",
                Sport.Swimming,
                SubSport.LapSwimming,
                [
                    SeedStep.Swim(300f, SwimStroke.Freestyle, null, Intensity.Warmup, "Warm Up"),
                    new WorkoutItem.Repeat(
                        [
                            SeedStep.Swim(
                                50f,
                                SwimStroke.Drill,
                                WorkoutEquipment.SwimKickboard,
                                Intensity.Active,
                                "Kick Drill"
                            ),
                            SeedStep.Swim(100f, SwimStroke.Freestyle, null, Intensity.Active, "Swim"),
                        ],
                        6
                    ),
                    SeedStep.Swim(300f, SwimStroke.Freestyle, null, Intensity.Cooldown, "Cool Down"),
                ],
                true,
                PoolLengthMetres,
                DisplayMeasure.Metric
            ),
            []
        ),
    ];
}
