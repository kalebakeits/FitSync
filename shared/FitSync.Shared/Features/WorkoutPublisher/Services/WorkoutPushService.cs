namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutBuilder.DTOs;
using FitSync.Shared.Features.WorkoutBuilder.Services.SchemaResolver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class WorkoutPushService(
    FitSyncDbContext dbContext,
    IWorkoutPushTargetResolver targetResolver,
    IWorkoutSchemaResolver schemaResolver,
    ILogger<WorkoutPushService> logger
) : IWorkoutPushService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWorkoutPushTargetResolver targetResolver = targetResolver;
    private readonly IWorkoutSchemaResolver schemaResolver = schemaResolver;
    private readonly ILogger<WorkoutPushService> logger = logger;

    public async Task<string?> PushAsync(
        Guid userId,
        string serviceType,
        Guid scheduledWorkoutId,
        WorkoutSchema schema,
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Pushing scheduled workout {ScheduledWorkoutId} to {ServiceType} for user {UserId}.",
            scheduledWorkoutId,
            serviceType,
            userId
        );

        WorkoutPushTarget target = await this.targetResolver.ResolveAsync(
            userId,
            serviceType,
            cancellationToken
        );

        TrainingProfile? trainingProfile = await this.dbContext.TrainingProfiles.FirstOrDefaultAsync(
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

        string? serviceMetadata = await target.Client.PublishAsync(
            target.Integration,
            resolvedSchema,
            scheduledWorkoutId.ToString(),
            scheduledDate,
            cancellationToken
        );

        this.logger.LogInformation(
            "Pushed scheduled workout {ScheduledWorkoutId} to {ServiceType} for user {UserId}.",
            scheduledWorkoutId,
            serviceType,
            userId
        );
        return serviceMetadata;
    }

    public async Task RescheduleAsync(
        Guid userId,
        string serviceType,
        string serviceMetadata,
        DateOnly newDate,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Rescheduling {ServiceType} workout for user {UserId} to {NewDate}.",
            serviceType,
            userId,
            newDate
        );

        WorkoutPushTarget target = await this.targetResolver.ResolveAsync(
            userId,
            serviceType,
            cancellationToken
        );

        await target.Client.RescheduleAsync(
            target.Integration,
            serviceMetadata,
            newDate,
            cancellationToken
        );

        this.logger.LogInformation(
            "Rescheduled {ServiceType} workout for user {UserId} to {NewDate}.",
            serviceType,
            userId,
            newDate
        );
    }

    public async Task DeleteAsync(
        Guid userId,
        string serviceType,
        string serviceMetadata,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Deleting {ServiceType} workout for user {UserId}.",
            serviceType,
            userId
        );

        WorkoutPushTarget target = await this.targetResolver.ResolveAsync(
            userId,
            serviceType,
            cancellationToken
        );

        await target.Client.DeleteAsync(target.Integration, serviceMetadata, cancellationToken);

        this.logger.LogInformation(
            "Deleted {ServiceType} workout for user {UserId}.",
            serviceType,
            userId
        );
    }
}
