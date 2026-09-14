namespace FitSync.Api.Features.WorkoutPublishing.DTOs;

public record DeleteScheduledWorkoutResponse(
    bool Found,
    bool Deleted,
    List<ScheduledWorkoutPublicationDeleteResponse> Publications
);
