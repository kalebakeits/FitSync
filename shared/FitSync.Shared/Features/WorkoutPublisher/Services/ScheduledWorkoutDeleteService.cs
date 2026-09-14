namespace FitSync.Shared.Features.WorkoutPublisher.Services;

using FitSync.Database;
using FitSync.Database.Models;
using FitSync.Shared.Features.WorkoutPublisher.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

public class ScheduledWorkoutDeleteService(
    FitSyncDbContext dbContext,
    IWorkoutPushService workoutPushService,
    ILogger<ScheduledWorkoutDeleteService> logger
) : IScheduledWorkoutDeleteService
{
    private readonly FitSyncDbContext dbContext = dbContext;
    private readonly IWorkoutPushService workoutPushService = workoutPushService;
    private readonly ILogger<ScheduledWorkoutDeleteService> logger = logger;

    public async Task<ScheduledWorkoutDeletionResult> DeleteAsync(
        Guid userId,
        Guid scheduledWorkoutId,
        bool force = false,
        CancellationToken cancellationToken = default
    )
    {
        this.logger.LogInformation(
            "Deleting scheduled workout {ScheduledWorkoutId} for user {UserId}. Force: {Force}.",
            scheduledWorkoutId,
            userId,
            force
        );

        bool exists = await this.dbContext.ScheduledWorkouts.AnyAsync(
            s => s.Id == scheduledWorkoutId && s.UserId == userId,
            cancellationToken
        );

        if (!exists)
        {
            this.logger.LogWarning(
                "ScheduledWorkout {Id} not found for user {UserId}.",
                scheduledWorkoutId,
                userId
            );
            return new ScheduledWorkoutDeletionResult(false, false, []);
        }

        List<ScheduledWorkoutPublication> publications = await this.dbContext
            .ScheduledWorkoutPublications.AsNoTracking()
            .Where(p => p.ScheduledWorkoutId == scheduledWorkoutId)
            .OrderBy(p => p.ServiceType)
            .ToListAsync(cancellationToken);

        List<PublicationDeleteOutcome> outcomes = [];
        List<Guid> succeededPublicationIds = [];

        foreach (ScheduledWorkoutPublication publication in publications)
        {
            try
            {
                if (publication.ServiceMetadata is null)
                    throw new InvalidOperationException(
                        $"No service metadata recorded for {publication.ServiceType}."
                    );

                await this.workoutPushService.DeleteAsync(
                    userId,
                    publication.ServiceType,
                    publication.ServiceMetadata,
                    cancellationToken
                );

                outcomes.Add(new PublicationDeleteOutcome(publication.ServiceType, true, null));
                succeededPublicationIds.Add(publication.Id);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Removing scheduled workout {ScheduledWorkoutId} from {ServiceType} for user {UserId} failed. Keeping its publication row so the removal can be retried.",
                    scheduledWorkoutId,
                    publication.ServiceType,
                    userId
                );
                outcomes.Add(
                    new PublicationDeleteOutcome(publication.ServiceType, false, ex.Message)
                );
            }
        }

        bool deleteParent = force || outcomes.All(o => o.Succeeded);
        List<Guid> removedPublicationIds = force
            ? publications.Select(p => p.Id).ToList()
            : succeededPublicationIds;

        await using IDbContextTransaction transaction =
            await this.dbContext.Database.BeginTransactionAsync(cancellationToken);

        await this.dbContext
            .ScheduledWorkoutPublications.Where(p => removedPublicationIds.Contains(p.Id))
            .ExecuteDeleteAsync(cancellationToken);

        if (deleteParent)
        {
            await this.dbContext
                .ScheduledWorkouts.Where(s => s.Id == scheduledWorkoutId && s.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        if (deleteParent && !outcomes.All(o => o.Succeeded))
            this.logger.LogWarning(
                "Force-deleted scheduled workout {ScheduledWorkoutId} for user {UserId} while {FailedCount} of {Total} service deletion(s) failed [{FailedServiceTypes}]. The workout still exists on those services; the local record of it is gone.",
                scheduledWorkoutId,
                userId,
                outcomes.Count(o => !o.Succeeded),
                outcomes.Count,
                string.Join(", ", outcomes.Where(o => !o.Succeeded).Select(o => o.ServiceType))
            );
        else if (deleteParent)
            this.logger.LogInformation(
                "Deleted scheduled workout {ScheduledWorkoutId} for user {UserId} and all {Count} publication(s).",
                scheduledWorkoutId,
                userId,
                outcomes.Count
            );
        else
            this.logger.LogError(
                "Scheduled workout {ScheduledWorkoutId} for user {UserId} was kept because {FailedCount} of {Total} service deletion(s) failed [{FailedServiceTypes}]. Those publication rows were kept so the deletion can be retried.",
                scheduledWorkoutId,
                userId,
                outcomes.Count(o => !o.Succeeded),
                outcomes.Count,
                string.Join(", ", outcomes.Where(o => !o.Succeeded).Select(o => o.ServiceType))
            );

        return new ScheduledWorkoutDeletionResult(true, deleteParent, outcomes);
    }
}
