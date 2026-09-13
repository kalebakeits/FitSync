namespace FitSync.Api.Features.WorkoutPublishing.Services;

using System.Text.Json;
using FitSync.Api.Exceptions;
using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutPublisher.Services;
using Microsoft.EntityFrameworkCore;

public class WorkoutPublishingService(
    FitSyncDbContext dbContext,
    IWorkoutPublisherService publisherService,
    ILogger<WorkoutPublishingService> logger
) : IWorkoutPublishingService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWorkoutPublisherService publisherService = publisherService;
    private readonly ILogger<WorkoutPublishingService> logger = logger;

    public async Task PublishAsync(
        Guid userId,
        Guid workoutId,
        string? serviceType,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Publishing workout {WorkoutId} to {ServiceType} for user {UserId}.",
            workoutId,
            serviceType,
            userId
        );

        Workout? workout = await this.dbContext.Workouts.FirstOrDefaultAsync(
            w => w.Id == workoutId && w.UserId == userId,
            cancellationToken
        );

        if (workout is null)
        {
            this.logger.LogWarning(
                "Workout {WorkoutId} not found for user {UserId}.",
                workoutId,
                userId
            );
            throw new NotFoundException("Workout not found.");
        }

        WorkoutSchema? schema = JsonSerializer.Deserialize<WorkoutSchema>(workout.Schema);

        if (schema is null)
            throw new NotFoundException("Workout schema is invalid.");

        await this.publisherService.PublishAsync(
            userId,
            workoutId,
            serviceType,
            schema,
            scheduledDate,
            cancellationToken
        );

        this.logger.LogInformation(
            "Workout {WorkoutId} published to {ServiceType} for user {UserId}.",
            workoutId,
            serviceType,
            userId
        );
    }

    public async Task<List<ScheduledWorkoutResponse>> GetScheduledWorkoutsAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation("GetScheduledWorkouts for user {UserId}.", userId);

        IQueryable<ScheduledWorkout> query = this.dbContext.ScheduledWorkouts.Include(
            s => s.Workout
        )
            .Where(s => s.UserId == userId);

        if (from.HasValue)
            query = query.Where(s => s.ScheduledDate >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.ScheduledDate <= to.Value);

        return await query
            .OrderBy(s => s.ScheduledDate)
            .Select(
                s =>
                    new ScheduledWorkoutResponse(
                        s.Id,
                        s.WorkoutId,
                        s.Workout.Name,
                        s.Workout.Sport,
                        s.ServiceType,
                        s.ScheduledDate,
                        s.CreatedAt
                    )
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduledWorkoutResponse> MoveScheduledWorkoutAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "MoveScheduledWorkout {ScheduledWorkoutId} to {NewDate} for user {UserId}.",
            scheduledWorkoutId,
            newDate,
            userId
        );

        ScheduledWorkout? scheduled = await this.dbContext.ScheduledWorkouts.Include(s => s.Workout)
            .FirstOrDefaultAsync(
                s => s.Id == scheduledWorkoutId && s.UserId == userId,
                cancellationToken
            );

        if (scheduled is null)
        {
            this.logger.LogWarning(
                "ScheduledWorkout {Id} not found for user {UserId}.",
                scheduledWorkoutId,
                userId
            );
            throw new NotFoundException("Scheduled workout not found.");
        }

        await this.publisherService.RescheduleAsync(
            userId,
            scheduledWorkoutId,
            newDate,
            cancellationToken
        );

        scheduled.ScheduledDate = newDate;
        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Moved scheduled workout {Id} to {NewDate}.",
            scheduledWorkoutId,
            newDate
        );
        return new ScheduledWorkoutResponse(
            scheduled.Id,
            scheduled.WorkoutId,
            scheduled.Workout.Name,
            scheduled.Workout.Sport,
            scheduled.ServiceType,
            scheduled.ScheduledDate,
            scheduled.CreatedAt
        );
    }

    public async Task DeleteScheduledWorkoutAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "DeleteScheduledWorkout {ScheduledWorkoutId} for user {UserId}.",
            scheduledWorkoutId,
            userId
        );

        int deleted = await this.dbContext.ScheduledWorkouts.Where(
            s => s.Id == scheduledWorkoutId && s.UserId == userId
        )
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
            this.logger.LogWarning(
                "ScheduledWorkout {Id} not found for user {UserId}.",
                scheduledWorkoutId,
                userId
            );
        else
            this.logger.LogInformation("Deleted scheduled workout {Id}.", scheduledWorkoutId);
    }
}
