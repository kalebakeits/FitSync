namespace FitSync.Api.Features.WorkoutPublishing.DTOs;

public record PendingPublishCandidate(Guid ScheduledWorkoutId, Guid UserId, DateOnly ScheduledDate);
