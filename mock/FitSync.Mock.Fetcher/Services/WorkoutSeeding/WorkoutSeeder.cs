namespace FitSync.Mock.Fetcher.Services.WorkoutSeeding;

using System.Text.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Mock.Fetcher.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutSchema.Services;
using Microsoft.Extensions.Logging;

public class WorkoutSeeder(
    FitSyncDbContext fitSyncDbContext,
    IEnumerable<ISeedWorkoutProvider> seedWorkoutProviders,
    IWorkoutDurationCalculator durationCalculator,
    ILogger<WorkoutSeeder> logger
) : IWorkoutSeeder
{
    private readonly FitSyncDbContext fitSyncDbContext = fitSyncDbContext;
    private readonly IEnumerable<ISeedWorkoutProvider> seedWorkoutProviders = seedWorkoutProviders;
    private readonly IWorkoutDurationCalculator durationCalculator = durationCalculator;
    private readonly ILogger<WorkoutSeeder> logger = logger;

    public async Task<(int Workouts, int ScheduledWorkouts)> SeedAsync(Guid userId)
    {
        this.logger.LogInformation("Seeding workouts and scheduled workouts for user {UserId}", userId);

        DateTime now = DateTime.UtcNow;
        DateOnly monday = DateOnly.FromDateTime(now)
            .AddDays(-((Convert.ToInt32(now.DayOfWeek) + 6) % 7));

        int workoutCount = 0;
        int scheduledCount = 0;

        foreach (ISeedWorkoutProvider provider in this.seedWorkoutProviders)
        {
            foreach (SeedWorkout seed in provider.GetWorkouts())
            {
                (string name, int sport, string? description) =
                    seed.Schema.Match<(string, int, string?)>(
                        @default => (
                            @default.Name,
                            Convert.ToInt32(@default.Sport),
                            @default.Description
                        ),
                        poolSwim => (
                            poolSwim.Name,
                            Convert.ToInt32(poolSwim.Sport),
                            poolSwim.Description
                        )
                    );

                Workout workout =
                    new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Name = name,
                        Description = description,
                        Tags = [],
                        Sport = sport,
                        Schema = JsonSerializer.Serialize(seed.Schema),
                        CreatedAt = now,
                        UpdatedAt = now,
                    };

                this.fitSyncDbContext.Workouts.Add(workout);
                workoutCount++;

                foreach (int dayOffset in seed.DayOffsets)
                {
                    this.fitSyncDbContext.ScheduledWorkouts.Add(
                        new ScheduledWorkout
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            WorkoutId = workout.Id,
                            ScheduledDate = monday.AddDays(dayOffset),
                            PlannedDurationSeconds = this.durationCalculator.CalculateSeconds(
                                seed.Schema
                            ),
                            CreatedAt = now,
                        }
                    );
                    scheduledCount++;
                }
            }
        }

        await this.fitSyncDbContext.SaveChangesAsync();

        this.logger.LogInformation(
            "Seeded {WorkoutCount} workouts and {ScheduledCount} scheduled workouts for user {UserId}",
            workoutCount,
            scheduledCount,
            userId
        );
        return (workoutCount, scheduledCount);
    }
}
