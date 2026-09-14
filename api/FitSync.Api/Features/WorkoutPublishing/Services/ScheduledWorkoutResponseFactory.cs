namespace FitSync.Api.Features.WorkoutPublishing.Services;

using FitSync.Api.Features.WorkoutPublishing.DTOs;
using FitSync.Database;
using FitSync.Database.Models;
using Microsoft.EntityFrameworkCore;

public class ScheduledWorkoutResponseFactory(FitSyncDbContext dbContext)
    : IScheduledWorkoutResponseFactory
{
    private readonly FitSyncDbContext dbContext = dbContext;

    public async Task<List<ScheduledWorkoutResponse>> BuildManyAsync(
        IQueryable<ScheduledWorkout> scheduledWorkouts,
        CancellationToken cancellationToken = default
    ) =>
        await scheduledWorkouts
            .OrderBy(s => s.ScheduledDate)
            .Select(
                s =>
                    new ScheduledWorkoutResponse(
                        s.Id,
                        s.WorkoutId,
                        s.Workout.Name,
                        s.Workout.Sport,
                        s.ScheduledDate,
                        s.CreatedAt,
                        s.PlannedDurationSeconds,
                        this.dbContext.Activities.Where(
                                a => a.ScheduledWorkoutId == s.Id && !a.IsDeleted
                            )
                            .Select(a => (Guid?)a.Id)
                            .FirstOrDefault(),
                        s.Publications.OrderBy(p => p.ServiceType)
                            .Select(
                                p =>
                                    new ScheduledWorkoutPublicationResponse(
                                        p.ServiceType,
                                        p.PublishedAt,
                                        p.Status,
                                        p.LastError
                                    )
                            )
                            .ToList()
                    )
            )
            .ToListAsync(cancellationToken);

    public async Task<ScheduledWorkoutResponse> BuildAsync(
        ScheduledWorkout scheduledWorkout,
        CancellationToken cancellationToken = default
    )
    {
        Activity? linkedActivity = await this.dbContext.Activities.FirstOrDefaultAsync(
            a => a.ScheduledWorkoutId == scheduledWorkout.Id && !a.IsDeleted,
            cancellationToken
        );

        List<ScheduledWorkoutPublicationResponse> publications = await this.dbContext
            .ScheduledWorkoutPublications.AsNoTracking()
            .Where(p => p.ScheduledWorkoutId == scheduledWorkout.Id)
            .OrderBy(p => p.ServiceType)
            .Select(
                p =>
                    new ScheduledWorkoutPublicationResponse(
                        p.ServiceType,
                        p.PublishedAt,
                        p.Status,
                        p.LastError
                    )
            )
            .ToListAsync(cancellationToken);

        return new ScheduledWorkoutResponse(
            scheduledWorkout.Id,
            scheduledWorkout.WorkoutId,
            scheduledWorkout.Workout.Name,
            scheduledWorkout.Workout.Sport,
            scheduledWorkout.ScheduledDate,
            scheduledWorkout.CreatedAt,
            scheduledWorkout.PlannedDurationSeconds,
            linkedActivity?.Id,
            publications
        );
    }
}
