namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using System.Text.Json;
using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.SchemaResolver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class WorkoutPublisherService(
    FitSyncDbContext dbContext,
    IEnumerable<IWorkoutPublisherClient> clients,
    IWorkoutSchemaResolver schemaResolver,
    ILogger<WorkoutPublisherService> logger
) : IWorkoutPublisherService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly Dictionary<string, IWorkoutPublisherClient> clientMap = clients.ToDictionary(
        c => c.ServiceType,
        c => c
    );
    private readonly IWorkoutSchemaResolver schemaResolver = schemaResolver;
    private readonly ILogger<WorkoutPublisherService> logger = logger;

    public async Task PublishAsync(
        Guid userId,
        Guid workoutId,
        string? serviceType,
        WorkoutSchema schema,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Publishing workout {WorkoutId} to {ServiceType} for user {UserId}.",
            workoutId,
            serviceType ?? "calendar-only",
            userId
        );

        Guid scheduledWorkoutId = Guid.NewGuid();
        string? serviceMetadata = null;

        if (serviceType is not null)
        {
            if (!this.clientMap.TryGetValue(serviceType, out IWorkoutPublisherClient? client))
                throw new NotSupportedException(
                    $"Workout publishing is not supported for service type '{serviceType}'."
                );

            Integration? integration = await this.dbContext.Integrations.FirstOrDefaultAsync(
                i => i.UserId == userId && i.ServiceType == serviceType,
                cancellationToken
            );

            if (integration is null)
            {
                this.logger.LogWarning(
                    "No {ServiceType} integration found for user {UserId}.",
                    serviceType,
                    userId
                );
                throw new InvalidOperationException($"No {serviceType} integration found.");
            }

            TrainingProfile? trainingProfile =
                await this.dbContext.TrainingProfiles.FirstOrDefaultAsync(
                    p => p.UserId == userId,
                    cancellationToken
                );

            WorkoutSchema resolvedSchema = trainingProfile is null
                ? schema
                : this.schemaResolver.Resolve(
                    schema,
                    new ZoneProfile(
                        trainingProfile.FtpWatts,
                        trainingProfile.CyclingMaxHr,
                        trainingProfile.RunningMaxHr,
                        trainingProfile.SwimThresholdHr,
                        trainingProfile.RunningThresholdPaceSeconds,
                        trainingProfile.SwimCssSeconds
                    )
                );

            serviceMetadata = await client.PublishAsync(
                integration,
                resolvedSchema,
                scheduledWorkoutId.ToString(),
                scheduledDate,
                cancellationToken
            );
        }

        this.dbContext.ScheduledWorkouts.Add(
            new ScheduledWorkout
            {
                Id = scheduledWorkoutId,
                UserId = userId,
                WorkoutId = workoutId,
                ServiceType = serviceType,
                ScheduledDate = scheduledDate,
                ServiceMetadata = serviceMetadata,
                CreatedAt = DateTime.UtcNow,
            }
        );

        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Successfully scheduled workout {WorkoutId} for user {UserId}.",
            workoutId,
            userId
        );
    }

    public async Task RescheduleAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Rescheduling scheduled workout {ScheduledWorkoutId} to {NewDate} for user {UserId}.",
            scheduledWorkoutId,
            newDate,
            userId
        );

        ScheduledWorkout? scheduled = await this.dbContext.ScheduledWorkouts.FirstOrDefaultAsync(
            s => s.Id == scheduledWorkoutId && s.UserId == userId,
            cancellationToken
        );

        if (scheduled is null)
            throw new InvalidOperationException(
                $"Scheduled workout {scheduledWorkoutId} not found."
            );

        if (scheduled.ServiceType is not null)
        {
            if (!this.clientMap.TryGetValue(scheduled.ServiceType, out IWorkoutPublisherClient? client))
                throw new NotSupportedException(
                    $"Rescheduling not supported for '{scheduled.ServiceType}'."
                );

            if (scheduled.ServiceMetadata is null)
                throw new InvalidOperationException(
                    $"No service metadata for scheduled workout {scheduledWorkoutId}."
                );

            Integration? integration = await this.dbContext.Integrations.FirstOrDefaultAsync(
                i => i.UserId == userId && i.ServiceType == scheduled.ServiceType,
                cancellationToken
            );

            if (integration is null)
                throw new InvalidOperationException(
                    $"No {scheduled.ServiceType} integration found for user {userId}."
                );

            await client.RescheduleAsync(
                integration,
                scheduled.ServiceMetadata,
                newDate,
                cancellationToken
            );
        }

        this.logger.LogInformation(
            "Successfully rescheduled {ScheduledWorkoutId} to {NewDate} for user {UserId}.",
            scheduledWorkoutId,
            newDate,
            userId
        );
    }
}
