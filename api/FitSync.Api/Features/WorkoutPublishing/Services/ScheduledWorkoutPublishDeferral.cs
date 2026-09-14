namespace FitSync.Api.Features.WorkoutPublishing.Services;

using FitSync.Api.Configurations;
using FitSync.Database.Enums;
using FitSync.Database.Models;
using Microsoft.Extensions.Options;

public class ScheduledWorkoutPublishDeferral(
    IOptions<WorkoutPublishingOptions> options,
    ILogger<ScheduledWorkoutPublishDeferral> logger
) : IScheduledWorkoutPublishDeferral
{
    private readonly IOptions<WorkoutPublishingOptions> options = options;
    private readonly ILogger<ScheduledWorkoutPublishDeferral> logger = logger;

    public DateTime ComputePendingPublishAt(DateOnly scheduledDate)
    {
        DateTime debounceAt = DateTime.UtcNow.AddMinutes(
            this.options.Value.PublishDelayMinutes
        );

        DateTime horizonAt = scheduledDate
            .ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            .AddDays(-this.options.Value.PublishHorizonDays);

        return debounceAt > horizonAt ? debounceAt : horizonAt;
    }

    public void Defer(ScheduledWorkout scheduledWorkout)
    {
        if (scheduledWorkout.Publications.Count == 0)
        {
            this.logger.LogInformation(
                "Scheduled workout {Id} has no service publications, so it requires no push.",
                scheduledWorkout.Id
            );
            return;
        }

        DateTime pendingPublishAt = this.ComputePendingPublishAt(scheduledWorkout.ScheduledDate);
        scheduledWorkout.PendingPublishAt = pendingPublishAt;

        foreach (ScheduledWorkoutPublication publication in scheduledWorkout.Publications)
        {
            publication.Status = PublicationStatus.Pending;
            publication.AttemptCount = 0;
            publication.LastError = null;
        }

        this.logger.LogInformation(
            "Deferred push for scheduled workout {Id} to {Count} service(s) [{ServiceTypes}] on {ScheduledDate}. PendingPublishAt set to {PendingPublishAt}; any later move overwrites it.",
            scheduledWorkout.Id,
            scheduledWorkout.Publications.Count,
            string.Join(", ", scheduledWorkout.Publications.Select(p => p.ServiceType)),
            scheduledWorkout.ScheduledDate,
            pendingPublishAt
        );
    }
}
