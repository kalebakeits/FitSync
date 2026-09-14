namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Database;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutSchema.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ScheduledWorkoutPublishService(
    FitSyncDbContext dbContext,
    IWorkoutPushService workoutPushService,
    IAutoPublishServiceResolver autoPublishServiceResolver,
    IWorkoutDurationCalculator durationCalculator,
    ILogger<ScheduledWorkoutPublishService> logger
) : IScheduledWorkoutPublishService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWorkoutPushService workoutPushService = workoutPushService;
    private readonly IAutoPublishServiceResolver autoPublishServiceResolver =
        autoPublishServiceResolver;
    private readonly IWorkoutDurationCalculator durationCalculator = durationCalculator;
    private readonly ILogger<ScheduledWorkoutPublishService> logger = logger;

    public async Task PublishAsync(
        Guid userId,
        Guid workoutId,
        string? serviceType,
        WorkoutSchema schema,
        DateOnly scheduledDate,
        Guid? scheduledWorkoutId = null,
        DateTime? pendingPublishAt = null,
        CancellationToken cancellationToken = default
    )
    {
        List<string> targetServiceTypes =
            serviceType is null
                ? await this.autoPublishServiceResolver.ResolveServiceTypesAsync(
                    userId,
                    workoutId,
                    cancellationToken
                )
                : [serviceType];

        bool deferPush = pendingPublishAt is not null;

        this.logger.LogInformation(
            "Scheduling workout {WorkoutId} for user {UserId} to {Count} service(s) [{ServiceTypes}]. Deferred: {Deferred} (amend of {ScheduledWorkoutId}).",
            workoutId,
            userId,
            targetServiceTypes.Count,
            string.Join(", ", targetServiceTypes),
            deferPush,
            scheduledWorkoutId?.ToString() ?? "none"
        );

        Guid resolvedScheduledWorkoutId = scheduledWorkoutId ?? Guid.NewGuid();
        List<ScheduledWorkoutPublication> publications = [];
        List<string> failedServiceTypes = [];

        foreach (string targetServiceType in targetServiceTypes)
        {
            if (deferPush)
            {
                publications.Add(
                    new ScheduledWorkoutPublication
                    {
                        Id = Guid.NewGuid(),
                        ScheduledWorkoutId = resolvedScheduledWorkoutId,
                        ServiceType = targetServiceType,
                        PublishedAt = DateTime.UtcNow,
                        Status = PublicationStatus.Pending,
                        AttemptCount = 0,
                    }
                );
                continue;
            }

            try
            {
                string? serviceMetadata = await this.workoutPushService.PushAsync(
                    userId,
                    targetServiceType,
                    resolvedScheduledWorkoutId,
                    schema,
                    scheduledDate,
                    cancellationToken
                );

                publications.Add(
                    new ScheduledWorkoutPublication
                    {
                        Id = Guid.NewGuid(),
                        ScheduledWorkoutId = resolvedScheduledWorkoutId,
                        ServiceType = targetServiceType,
                        ServiceMetadata = serviceMetadata,
                        PublishedAt = DateTime.UtcNow,
                        Status = PublicationStatus.Success,
                        AttemptCount = 1,
                    }
                );
            }
            catch (Exception ex)
            {
                failedServiceTypes.Add(targetServiceType);
                publications.Add(
                    new ScheduledWorkoutPublication
                    {
                        Id = Guid.NewGuid(),
                        ScheduledWorkoutId = resolvedScheduledWorkoutId,
                        ServiceType = targetServiceType,
                        PublishedAt = DateTime.UtcNow,
                        Status = PublicationStatus.Failed,
                        AttemptCount = 1,
                        LastError = ex.Message,
                    }
                );
                this.logger.LogError(
                    ex,
                    "Pushing workout {WorkoutId} to {ServiceType} for user {UserId} failed. Continuing with the remaining services.",
                    workoutId,
                    targetServiceType,
                    userId
                );
            }
        }

        if (scheduledWorkoutId is null)
        {
            ScheduledWorkout scheduledWorkout = new()
            {
                Id = resolvedScheduledWorkoutId,
                UserId = userId,
                WorkoutId = workoutId,
                ScheduledDate = scheduledDate,
                PlannedDurationSeconds = this.durationCalculator.CalculateSeconds(schema),
                PendingPublishAt = pendingPublishAt,
                CreatedAt = DateTime.UtcNow,
            };

            scheduledWorkout.Publications.AddRange(publications);
            this.dbContext.ScheduledWorkouts.Add(scheduledWorkout);
        }
        else
        {
            ScheduledWorkout? scheduledWorkout =
                await this.dbContext
                    .ScheduledWorkouts.Include(s => s.Publications)
                    .FirstOrDefaultAsync(
                        s => s.Id == scheduledWorkoutId && s.UserId == userId,
                        cancellationToken
                    );

            if (scheduledWorkout is null)
            {
                this.logger.LogWarning(
                    "ScheduledWorkout {Id} not found for user {UserId}.",
                    scheduledWorkoutId,
                    userId
                );
                throw new InvalidOperationException(
                    $"Scheduled workout {scheduledWorkoutId} not found."
                );
            }

            scheduledWorkout.ScheduledDate = scheduledDate;
            scheduledWorkout.PendingPublishAt = pendingPublishAt;
            scheduledWorkout.PublishClaimedAt = null;

            foreach (ScheduledWorkoutPublication publication in publications)
            {
                ScheduledWorkoutPublication? existing = scheduledWorkout.Publications.FirstOrDefault(
                    p => p.ServiceType == publication.ServiceType
                );

                if (existing is null)
                {
                    scheduledWorkout.Publications.Add(publication);
                    continue;
                }

                existing.ServiceMetadata = publication.ServiceMetadata;
                existing.PublishedAt = publication.PublishedAt;
                existing.Status = publication.Status;
                existing.AttemptCount = publication.AttemptCount;
                existing.LastError = publication.LastError;
            }
        }

        await this.dbContext.SaveChangesAsync(cancellationToken);

        if (failedServiceTypes.Count > 0)
        {
            this.logger.LogError(
                "Workout {WorkoutId} for user {UserId} failed for [{FailedServiceTypes}].",
                workoutId,
                userId,
                string.Join(", ", failedServiceTypes)
            );
        }

        this.logger.LogInformation(
            "Scheduled workout {ScheduledWorkoutId} saved for workout {WorkoutId} and user {UserId}.",
            resolvedScheduledWorkoutId,
            workoutId,
            userId
        );
    }
}
