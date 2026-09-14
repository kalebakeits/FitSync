namespace FitSync.Shared.Features.WorkoutPublisher.DTOs;

public record ScheduledWorkoutDeletionResult(
    bool Found,
    bool Deleted,
    List<PublicationDeleteOutcome> Publications
);
