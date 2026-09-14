namespace FitSync.Mock.Fetcher.DTOs;

using FitSync.Shared.Features.WorkoutBuilder.DTOs;

public record SeedWorkout(WorkoutSchema Schema, int[] DayOffsets);
