namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

public interface IWorkoutSeeder
{
    Task<(int Workouts, int ScheduledWorkouts)> SeedAsync(Guid userId);
}
