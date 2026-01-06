namespace FitSync.Api.Features.Activities.Services;

using FitSync.Api.Features.Activities.DTOs;
using FitSync.Database.Enums;

public interface IActivitiesService
{
    Task<PaginatedActivitiesResponse> GetActivitiesAsync(
        Guid userId,
        ActivityStatus? status,
        int limit,
        int offset
    );
    Task<ActivityResponse> GetActivityByIdAsync(Guid userId, Guid activityId);
    Task DeleteActivityAsync(Guid userId, Guid activityId);
}
