namespace FitSync.Api.Features.WorkoutPublishing.Services;

using System.Text.Json;
using FitSync.Api.Exceptions;
using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutPublisher.DTOs;
using FitSync.Shared.Features.WorkoutPublisher.Services;
using Microsoft.EntityFrameworkCore;

public class WorkoutPublishingService(
    FitSyncDbContext dbContext,
    IScheduledWorkoutPublishService publishService,
    IScheduledWorkoutDeleteService deleteService,
    IScheduledWorkoutResponseFactory responseFactory,
    IScheduledWorkoutPublishDeferral publishDeferral,
    ILogger<WorkoutPublishingService> logger
) : IWorkoutPublishingService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IScheduledWorkoutPublishService publishService = publishService;
    private readonly IScheduledWorkoutDeleteService deleteService = deleteService;
    private readonly IScheduledWorkoutResponseFactory responseFactory = responseFactory;
    private readonly IScheduledWorkoutPublishDeferral publishDeferral = publishDeferral;
    private readonly ILogger<WorkoutPublishingService> logger = logger;

    public async Task PublishAsync(
        Guid userId,
        Guid workoutId,
        string? serviceType,
        DateOnly scheduledDate,
        Guid? scheduledWorkoutId = null,
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

        DateTime? pendingPublishAt =
            scheduledWorkoutId is null
                ? this.publishDeferral.ComputePendingPublishAt(scheduledDate)
                : null;

        await this.publishService.PublishAsync(
            userId,
            workoutId,
            serviceType,
            schema,
            scheduledDate,
            scheduledWorkoutId,
            pendingPublishAt,
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

        return await this.responseFactory.BuildManyAsync(query, cancellationToken);
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
                "ScheduledWorkout {Id} not found for user {UserId}.",
                scheduledWorkoutId,
                userId
            );
            throw new NotFoundException("Scheduled workout not found.");
        }

        scheduled.ScheduledDate = newDate;
        this.publishDeferral.Defer(scheduled);

        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Moved scheduled workout {Id} to {NewDate}.",
            scheduledWorkoutId,
            newDate
        );
        return await this.responseFactory.BuildAsync(scheduled, cancellationToken);
    }

    public async Task<DeleteScheduledWorkoutResponse> DeleteScheduledWorkoutAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        bool force = false,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "DeleteScheduledWorkout {ScheduledWorkoutId} for user {UserId}. Force: {Force}.",
            scheduledWorkoutId,
            userId,
            force
        );

        ScheduledWorkoutDeletionResult result = await this.deleteService.DeleteAsync(
            userId,
            scheduledWorkoutId,
            force,
            cancellationToken
        );

        if (result.Found && result.Deleted)
            this.logger.LogInformation(
                "Deleted scheduled workout {Id} and removed it from all {Total} service(s).",
                scheduledWorkoutId,
                result.Publications.Count
            );
        else if (result.Found)
            this.logger.LogWarning(
                "Scheduled workout {Id} was kept: {FailedCount} of {Total} service removal(s) failed [{FailedServiceTypes}]. The UI should report which services still hold it.",
                scheduledWorkoutId,
                result.Publications.Count(p => !p.Succeeded),
                result.Publications.Count,
                string.Join(", ", result.Publications.Where(p => !p.Succeeded).Select(p => p.ServiceType))
            );

        return new DeleteScheduledWorkoutResponse(
            result.Found,
            result.Deleted,
            result
                .Publications.Select(p =>
                    new ScheduledWorkoutPublicationDeleteResponse(
                        p.ServiceType,
                        p.Succeeded,
                        p.Error
                    )
                )
                .ToList()
        );
    }
}
