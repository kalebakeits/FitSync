namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

using FitSync.Mock.Fetcher.DTOs;

public interface ISeedWorkoutProvider
{
    SeedWorkout[] GetWorkouts();
}
