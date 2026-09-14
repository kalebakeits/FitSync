namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using System.Text.Json;
using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ScheduledWorkoutPushService(
    FitSyncDbContext dbContext,
    IWorkoutPushService workoutPushService,
    ILogger<ScheduledWorkoutPushService> logger
) : IScheduledWorkoutPushService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWorkoutPushService workoutPushService = workoutPushService;
    private readonly ILogger<ScheduledWorkoutPushService> logger = logger;

    public async Task<bool> PushPendingAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        int maxAttempts,
        CancellationToken cancellationToken = default
    )
    {
        ScheduledWorkout? scheduled = await this.dbContext
            .ScheduledWorkouts.Include(s => s.Workout)
            .Include(s => s.Publications)
            .FirstOrDefaultAsync(
                s => s.Id == scheduledWorkoutId && s.UserId == userId,
                cancellationToken
            );

        if (scheduled is null)
        {
            this.logger.LogWarning(
                "Scheduled workout {Id} not found for user {UserId}; nothing to push.",
                scheduledWorkoutId,
                userId
            );
            return true;
        }

        List<ScheduledWorkoutPublication> pending = scheduled
            .Publications.Where(p => p.Status == PublicationStatus.Pending)
            .ToList();

        if (pending.Count == 0)
            return true;

        WorkoutSchema? schema = JsonSerializer.Deserialize<WorkoutSchema>(scheduled.Workout.Schema);

        if (schema is null)
        {
            this.logger.LogError(
                "Workout schema for scheduled workout {Id} is unreadable; failing its {Count} pending publication(s) so they stop retrying.",
                scheduledWorkoutId,
                pending.Count
            );

            foreach (ScheduledWorkoutPublication broken in pending)
            {
                broken.Status = PublicationStatus.Failed;
                broken.LastError = "Workout schema is invalid.";
            }

            await this.dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        this.logger.LogInformation(
            "Pushing {Count} pending publication(s) [{ServiceTypes}] for scheduled workout {Id} on {ScheduledDate}.",
            pending.Count,
            string.Join(", ", pending.Select(p => p.ServiceType)),
            scheduledWorkoutId,
            scheduled.ScheduledDate
        );

        foreach (ScheduledWorkoutPublication publication in pending)
        {
            try
            {
                if (publication.ServiceMetadata is null)
                {
                    publication.ServiceMetadata = await this.workoutPushService.PushAsync(
                        userId,
                        publication.ServiceType,
                        scheduledWorkoutId,
                        schema,
                        scheduled.ScheduledDate,
                        cancellationToken
                    );
                }
                else
                {
                    await this.workoutPushService.RescheduleAsync(
                        userId,
                        publication.ServiceType,
                        publication.ServiceMetadata,
                        scheduled.ScheduledDate,
                        cancellationToken
                    );
                }

                publication.Status = PublicationStatus.Success;
                publication.AttemptCount++;
                publication.LastError = null;
                publication.PublishedAt = DateTime.UtcNow;

                this.logger.LogInformation(
                    "Publication of scheduled workout {Id} to {ServiceType} succeeded.",
                    scheduledWorkoutId,
                    publication.ServiceType
                );
            }
            catch (Exception ex)
            {
                publication.AttemptCount++;
                publication.LastError = ex.Message;

                if (publication.AttemptCount >= maxAttempts)
                {
                    publication.Status = PublicationStatus.Failed;
                    this.logger.LogError(
                        ex,
                        "Publication of scheduled workout {Id} to {ServiceType} failed {Attempts} time(s) and will not be retried.",
                        scheduledWorkoutId,
                        publication.ServiceType,
                        publication.AttemptCount
                    );
                }
                else
                {
                    this.logger.LogWarning(
                        ex,
                        "Publication of scheduled workout {Id} to {ServiceType} failed on attempt {Attempt} of {MaxAttempts}; will retry.",
                        scheduledWorkoutId,
                        publication.ServiceType,
                        publication.AttemptCount,
                        maxAttempts
                    );
                }
            }
        }

        await this.dbContext.SaveChangesAsync(cancellationToken);

        return pending.All(p => p.Status != PublicationStatus.Pending);
    }
}
